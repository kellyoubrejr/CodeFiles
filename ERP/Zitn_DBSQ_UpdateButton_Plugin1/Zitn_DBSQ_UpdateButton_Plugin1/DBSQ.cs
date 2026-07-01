using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using static Kingdee.K3.MFG.App.AppServiceContext;

namespace Zitn_DBSQ_UpdateButton_Plugin1
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("【表单插件】点击按钮获取wlnum+stkid即时库存的可用量更新对应字段")]
    public class DBSQ : AbstractBillPlugIn
    {
        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);

            if (e.BarItemKey.Equals("ZMER_tbButton", StringComparison.OrdinalIgnoreCase))
            {
                var entity = this.View.BillBusinessInfo.GetEntity("FEntity");
                var entityObjs = this.View.Model.GetEntityDataObject(entity);



                #region 在途生产订单

                // 收集所有物料ID
                List<long> materialIds = new List<long>();

                for (int i = 0; i < entityObjs.Count; i++)
                {
                    DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", i) as DynamicObject;
                    if (materialObj == null) continue;

                    long materialId = Convert.ToInt64(materialObj["Id"]);

                    if (!materialIds.Contains(materialId))
                    {
                        materialIds.Add(materialId);
                    }
                }

                if (materialIds.Count == 0)
                {
                    return;
                }

                string ids = string.Join(",", materialIds);


                // 查询所有涉及这些物料的领料单
                string sql = $@"/*dialect*/
                                        SELECT
                                            E1.FPARENTMATERIALID,
                                            E.FMATERIALID,
                                            MO.FBILLNO
                                        FROM
                                            T_PRD_PICKMTRLDATA E
                                        INNER JOIN T_PRD_PICKMTRL H ON E.FID = H.FID
                                        JOIN T_PRD_PICKMTRLDATA_A E1 ON E.FENTRYID = E1.FENTRYID
                                        JOIN T_PRD_MO MO ON E.FMOID = MO.FID
                                        JOIN T_PRD_MOENTRY MO1 ON MO.FID = MO1.FID
                                        JOIN T_PRD_MOENTRY_A MO2 ON MO1.FENTRYID = MO2.FENTRYID
                                        WHERE
                                            E.FMATERIALID IN ({ids})
                                            AND MO2.FSTATUS = 4
                                        ";

                /*AND H.FDATE >= DATEADD(MONTH, -6, GETDATE())
                                            AND H.FDATE <= GETDATE()*/
                DynamicObjectCollection queryResult1 = DbUtils.ExecuteDynamicObject(this.Context, sql);


                // 物料 → 数据映射
                Dictionary<long, List<Tuple<string, string>>> materialMoMap
                = new Dictionary<long, List<Tuple<string, string>>>();

                foreach (DynamicObject row in queryResult1)
                {
                    long materialId = Convert.ToInt64(row["FMATERIALID"]);
                    string parentMaterial = Convert.ToString(row["FPARENTMATERIALID"]);
                    string moBillNo = Convert.ToString(row["FBILLNO"]);

                    if (!materialMoMap.ContainsKey(materialId))
                    {
                        materialMoMap[materialId] = new List<Tuple<string, string>>();
                    }

                    materialMoMap[materialId].Add(
                        new Tuple<string, string>(parentMaterial, moBillNo)
                    );
                }


                // 再循环单据体写入结果
                for (int k = 0; k < entityObjs.Count; k++)
                {
                    string currentMo = Convert.ToString(this.View.Model.GetValue("FMOBillNo", k));

                    DynamicObject materialObj = this.View.Model.GetValue("FMaterialId", k) as DynamicObject;
                    if (materialObj == null) continue;

                    long materialId = Convert.ToInt64(materialObj["Id"]);

                    string currentProduct = Convert.ToString(this.View.Model.GetValue("FCPNUM", k));

                    if (!materialMoMap.ContainsKey(materialId))
                        continue;

                    HashSet<string> resultSet = new HashSet<string>();

                    foreach (var item in materialMoMap[materialId])
                    {
                        string parentMaterial = item.Item1;

                        var wlnumQuery = string.Format("/*dialect*/select distinct fnumber from t_bd_material where fmaterialid = '{0}'", parentMaterial);
                        DynamicObjectCollection wlnumResult = DbUtils.ExecuteDynamicObject(this.Context, wlnumQuery);

                        string wlnum = Convert.ToString(wlnumResult[0]["FNUMBER"]);

                        string moBillNo = item.Item2;

                        // 产品不同 且 生产订单不同
                        if (wlnum != currentProduct && moBillNo != currentMo)
                        {
                            resultSet.Add(moBillNo);
                        }
                    }

                    string result = string.Join(",", resultSet);

                    this.View.Model.SetValue("FZTMO", result, k);
                }

                #endregion



                for (int i = 0; i < entityObjs.Count; i++)
                {

                    DynamicObject wlidObj = this.View.Model.GetValue("FMaterialId", i) as DynamicObject;
                    if (wlidObj == null) continue;

                    string wlid = Convert.ToString(wlidObj["ID"]);

                    var wl = string.Format("/*dialect*/select FNUMBER from T_BD_MATERIAL where fmaterialid = '{0}'", wlid);
                    DynamicObjectCollection wlsql = DbUtils.ExecuteDynamicObject(this.Context, wl);

                    string wlnum = Convert.ToString(wlsql[0]["FNUMBER"]);

                    DynamicObject stkObj = this.View.Model.GetValue("FStockId", i) as DynamicObject;
                    if (stkObj == null) continue;
                    string stkid = Convert.ToString(stkObj["ID"]);

                    var sqqty = this.View.Model.GetValue("FQty", i);

                    #region 即时库存可用量总

                    var query = string.Format(@"/*dialect*/SELECT 
                                                            SUM(子查询.可用量) AS 总可用量
                                                        FROM (
                                                            SELECT 
                                                                m.FNUMBER AS 商品编码,
                                                                ml.FNAME AS 物料名称,
                                                                ml.FSPECIFICATION AS 规格型号,
                                                                a.FPRODUCEDATE AS 生产日期,
                                                                a.FEXPIRYDATE AS 有效期至,
                                                                kcztL.FNAME AS 库存状态,
                                                                baseUnit.FNAME AS 单位,
                                                                a.FBASEQTY - 0 AS 库存量,
                                                                CASE 
                                                                    WHEN TSUB.FBASELOCKQTY IS NULL THEN a.FBASEQTY 
                                                                    ELSE a.FBASEQTY - TSUB.FBASELOCKQTY 
                                                                END AS 可用量,
                                                                stockL.FName AS 仓库
                                                            FROM T_STK_INVENTORY a 
                                                            LEFT JOIN T_BD_LOTMASTER lotStock 
                                                                ON lotStock.FLOTID = a.FLOT 
                                                                AND lotStock.FMATERIALID = a.FMATERIALID 
                                                                AND a.FSTOCKORGID = lotStock.FUSEORGID 
                                                            LEFT JOIN (
                                                                SELECT 
                                                                    TLKE.FSUPPLYINTERID AS FINVENTRYID, 
                                                                    SUM(TLKE.FBASEQTY) AS FBASELOCKQTY,
                                                                    SUM(TLKE.FSECQTY) AS FSECLOCKQTY 
                                                                FROM T_PLN_RESERVELINKENTRY TLKE 
                                                                INNER JOIN T_PLN_RESERVELINK TLKH ON TLKE.FID = TLKH.FID
                                                                WHERE TLKE.FSUPPLYFORMID = 'STK_Inventory'  
                                                                    AND TLKE.FLINKTYPE = '4' 
                                                                GROUP BY TLKE.FSUPPLYINTERID
                                                            ) TSUB ON a.FID = TSUB.FINVENTRYID
                                                            INNER JOIN T_BD_MATERIAL m ON m.FMATERIALID = a.FMATERIALID
                                                            INNER JOIN T_BD_MATERIAL_L ml ON ml.FMATERIALID = m.FMATERIALID AND ml.FLOCALEID = 2052
                                                            INNER JOIN t_BD_StockStatus kczt ON kczt.FSTOCKSTATUSID = a.FSTOCKSTATUSID
                                                            INNER JOIN T_BD_STOCKSTATUS_L kcztL ON kcztL.FSTOCKSTATUSID = kczt.FSTOCKSTATUSID AND kcztL.FLOCALEID = 2052
                                                            INNER JOIN T_BD_UNIT_L baseUnit ON baseUnit.FUNITID = a.FBASEUNITID AND baseUnit.FLOCALEID = 2052
                                                            INNER JOIN T_BD_Stock_L stockL ON stockL.FSTOCKID = a.FSTOCKID AND stockL.FLOCALEID = 2052
                                                            WHERE  a.FBASEQTY>0 AND A.FSTOCKID = '{0}' AND M.FNUMBER = '{1}'
                                                        ) AS 子查询", stkid, wlnum);
                    DynamicObjectCollection queryResult = DbUtils.ExecuteDynamicObject(this.Context, query);
                    if (queryResult != null && queryResult.Count > 0)
                    {
                        for (int j = 0; j < queryResult.Count; j++)
                        {
                            int dqkcqty = Convert.ToInt32(queryResult[j]["总可用量"]);
                            this.View.Model.SetValue("FDQKCQTY", dqkcqty, i);

                            int cyqty = 0;
                            if (sqqty != null)
                            {
                                cyqty = Convert.ToInt32(sqqty) - dqkcqty;
                                this.View.Model.SetValue("FCYQTY", cyqty, i);

                                if (cyqty > 0)
                                {
                                    string msg = string.Format("第{0}行出现负库存，请检查！", i + 1);
                                    throw new KDException("StockCheckError", msg);
                                }
                            }


                        }
                    }
                    #endregion

                }
                this.View.UpdateView("FEntity");
            }
        }
    }

}
