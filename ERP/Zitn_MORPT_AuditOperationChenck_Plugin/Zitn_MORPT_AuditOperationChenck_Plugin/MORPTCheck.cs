using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Linq;

namespace Zitn_MORPT_AuditOperationChenck_Plugin
{
    [Description("【服务插件】：生产汇报单提交审核，二个校验拦截")]
    [Kingdee.BOS.Util.HotUpdate]
    public class MORPTCheck : AbstractOperationServicePlugIn
    {
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            var ids = string.Join(",", e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString()));
            //TODO:当前汇报单【基本单位完成数量】与该生产订单其他汇报单的就【基本单位完成数量】之和，等于【订单数量】（最后一次汇报）
            var querySql = string.Format($@"/*dialect*/SELECT DISTINCT
                                                          FSRCBILLNO ,F_PAEZ_GSHB,A.FID,FHRWORKTIME,FBASEFINISHQTY,F_PAEZ_SCDDSL,a.fbillno
                                                        FROM
                                                          T_PRD_MORPT A
                                                          JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                          JOIN T_PRD_MORPTENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                        WHERE
                                                          A.FID IN ({ids})");
            DynamicObjectCollection dt = DBUtils.ExecuteDynamicObject(this.Context, querySql);
            if (dt != null && dt.Count >0)
            {
                for (int j = 0; j < dt.Count; j++)
                {
                    var moNo = dt[j]["FSRCBILLNO"]?.ToString();
                    long gsHb = Convert.ToInt64(dt[j]["F_PAEZ_GSHB"]);
                    long fid = Convert.ToInt64(dt[j]["FID"]);
                    decimal hrworktime = Convert.ToDecimal(dt[j]["FHRWORKTIME"]);
                    long basefinishqty = Convert.ToInt64(dt[j]["FBASEFINISHQTY"]);
                    long scddsl = Convert.ToInt64(dt[j]["F_PAEZ_SCDDSL"]);
                    string billno = dt[j]["fbillno"]?.ToString();

                    var judgeSql = string.Format($@"/*dialect*/SELECT SUM(FBASEFINISHQTY) as zjbsl FROM 
                                                      T_PRD_MORPT A
                                                      JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                      WHERE FSRCBILLNO = '{moNo}' 
                                                      and a.fbillno <> '{billno}'");
                    DynamicObjectCollection judgeDt = DBUtils.ExecuteDynamicObject(this.Context, judgeSql);
                    if (judgeDt != null && judgeDt.Count > 0)
                    {
                        if (judgeDt[0]["zjbsl"] != null && judgeDt[0]["zjbsl"] != DBNull.Value)
                        {
                            long finishqty = Convert.ToInt64(judgeDt[0]["zjbsl"]);
                            if (basefinishqty + finishqty == scddsl && gsHb==0)
                            {
                                //Getgshb(moNo);

                                #region 检查是否存在未审核的领料单或者退料单(最后一次汇报)(不考虑工时)
                                var blSql = string.Format($@"/*dialect*/SELECT 
                                                            COUNT(*) AS qq 
                                                        FROM
                                                            T_PRD_FEEDMTRL A
                                                            JOIN T_PRD_FEEDMTRLDATA B ON A.FID = B.FID
                                                            JOIN T_PRD_FEEDMTRLDATA_Q B1 ON B.FENTRYID = B1.FENTRYID 
                                                        WHERE
                                                            b.FMOBILLNO = '{moNo}' 
                                                            AND a.FDOCUMENTSTATUS = 'B'");
                                DynamicObjectCollection blDt = DBUtils.ExecuteDynamicObject(this.Context, blSql);
                                if (blDt != null && blDt.Count > 0)
                                {
                                    long count = blDt[0]["qq"] != null && blDt[0]["qq"] != DBNull.Value ? Convert.ToInt64(blDt[0]["qq"]) : 0;
                                    if (count != 0)
                                    {
                                        throw new KDBusinessException("", "存在未审核的领料单，请检查！");
                                    }
                                }

                                var tlSql = string.Format($@"/*dialect*/SELECT 
                                                            COUNT(*) AS qq 
                                                        FROM
                                                            T_PRD_RETURNMTRL A
                                                            JOIN T_PRD_RETURNMTRLENTRY B ON A.FID = B.FID
                                                            JOIN T_PRD_RETURNMTRLENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                        WHERE
                                                            b.FMOBILLNO = '{moNo}' 
                                                            AND a.FDOCUMENTSTATUS = 'B'");
                                DynamicObjectCollection tlDt = DBUtils.ExecuteDynamicObject(this.Context, tlSql);
                                if (tlDt != null && tlDt.Count > 0)
                                {
                                    long count = tlDt[0]["qq"] != null && tlDt[0]["qq"] != DBNull.Value ? Convert.ToInt64(tlDt[0]["qq"]) : 0;
                                    if (count != 0)
                                    {
                                        throw new KDBusinessException("", "存在未审核的退料单，请检查！");
                                    }
                                }
                                #endregion
                            }
                            else if (basefinishqty + finishqty == scddsl && gsHb == 1)
                            {
                                Chexk(fid);
                            }
                        }
                    }

                    if (gsHb == 1)
                    {
                        Chexk(fid);
                        
                    }
                }
            }            
        }

        private void Getgshb(string moNo)
        {
            var moSql = string.Format($@"/*dialect*/SELECT 
                                                      F_PAEZ_GSHB,FHRWORKTIME
                                                    FROM
                                                      T_PRD_MORPT A
                                                      JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                      JOIN T_PRD_MORPTENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                    WHERE
                                                      FSRCBILLNO = '{moNo}'");
            DynamicObjectCollection moDt = DBUtils.ExecuteDynamicObject(this.Context, moSql);
            if (moDt == null || moDt.Count == 0)
                return;

            bool flag = !moDt.Any(item => Convert.ToDecimal(item["FHRWORKTIME"])!=0.00m);

            /*if (flag)
                throw new Exception("该订单未做工时汇报，请检查。");*/


        }

        private void Chexk(long fid)
        {
            var querySql = string.Format($@"/*dialect*/SELECT B.FMATERIALID ,FHRWORKTIME,F_PAEZ_SCDDSL
                                                        FROM
                                                          T_PRD_MORPT A
                                                          JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                          JOIN T_PRD_MORPTENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                        WHERE
                                                          A.FID IN ({fid})");
            DynamicObjectCollection dt = DBUtils.ExecuteDynamicObject(this.Context, querySql);
            if (dt != null && dt.Count > 0)
            {
                for (int j = 0; j < dt.Count; j++)
                {
                    var materialId = dt[j]["FMATERIALID"]?.ToString();
                    decimal hrworktime = Convert.ToDecimal(dt[j]["FHRWORKTIME"]);
                    long scddsl = Convert.ToInt64(dt[j]["F_PAEZ_SCDDSL"]);
                    decimal res = hrworktime / scddsl;

                    var sql = string.Format($@"/*dialect*/SELECT 
                                                              sum(FHRWORKTIME) as zgs,sum(F_PAEZ_SCDDSL) as zsl
                                                            FROM
                                                              T_PRD_MORPT A
                                                              JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                            WHERE
                                                              b.FMATERIALID = '{materialId}' 
                                                              AND A.FDOCUMENTSTATUS = 'C'");
                    DynamicObjectCollection checkDt = DBUtils.ExecuteDynamicObject(this.Context, sql);
                    if (checkDt != null && checkDt.Count > 0)
                    {
                        decimal zgs = Convert.ToDecimal(checkDt[0]["zgs"]);
                        long zsl = Convert.ToInt64(checkDt[0]["zsl"]);
                        decimal avggs = 0.00m;
                        decimal height = 0.00m;
                        decimal low = 0.00m;

                        if (zgs > 0 && zsl != 0)
                        {
                             avggs = zgs / zsl;

                             height = avggs + (avggs * 0.1m);
                             low = avggs - (avggs * 0.1m);
                        }
                        var result = new OperateResult();
                        if (res > height)
                        {
                            var upSql = string.Format($@"/*dialect*/  UPDATE B SET 
                                                              B.F_ZMER_TEXT_TZK = '大于平均工时{avggs}',
                                                              B.F_ZMER_TEXT_APV = '{avggs}'
                                                              FROM  T_PRD_MORPT A
                                                              JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                              WHERE A.FID IN ({fid})  and B.FMATERIALID = '{materialId}'  "
                                                              );
                            DBUtils.ExecuteDynamicObject(this.Context, upSql);
                            result.Message = string.Format($"当前汇报工时高于历史平均工时10%，请检查；", $"不能提交审核！");
                            this.OperationResult.IsSuccess = true;
                            this.OperationResult.IsShowMessage = true;
                            this.OperationResult.OperateResult.Add(result);
                        }
                        if(res < low)
                        {
                            var upSql1 = string.Format($@"/*dialect*/  UPDATE B SET 
                                                              B.F_ZMER_TEXT_TZK = '小于于平均工时{avggs}',
                                                              B.F_ZMER_TEXT_APV = '{avggs}'
                                                              FROM  T_PRD_MORPT A
                                                              JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                               WHERE A.FID IN ({fid}) and B.FMATERIALID = '{materialId}' ");
                            DBUtils.ExecuteDynamicObject(this.Context, upSql1);
                            result.Message = string.Format($"当前汇报工时低于历史平均工时10%，请检查；", $"不能提交审核！");
                            this.OperationResult.IsSuccess = true;
                            this.OperationResult.IsShowMessage = true;
                            this.OperationResult.OperateResult.Add(result);
                        }
                    }
                }
            }
        }
    }


    [Description("【服务插件】：工时汇报提示"),HotUpdate]    
    public class Warning : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            var ids = string.Join(",", e.SelectedRows
                            .Select(row => row.DataEntity["Id"]?.ToString()));
            //TODO:当前汇报单【基本单位完成数量】与该生产订单其他汇报单的就【基本单位完成数量】之和，等于【订单数量】（最后一次汇报）
            var querySql = string.Format($@"/*dialect*/SELECT DISTINCT
                                                          FSRCBILLNO ,F_PAEZ_GSHB,A.FID,FHRWORKTIME,FBASEFINISHQTY,F_PAEZ_SCDDSL,a.fbillno
                                                        FROM
                                                          T_PRD_MORPT A
                                                          JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                          JOIN T_PRD_MORPTENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                        WHERE
                                                          A.FID IN ({ids})");
            DynamicObjectCollection dt = DBUtils.ExecuteDynamicObject(this.Context, querySql);
            if (dt != null && dt.Count > 0)
            {
                for (int j = 0; j < dt.Count; j++)
                {
                    var moNo = dt[j]["FSRCBILLNO"]?.ToString();
                    string billno = dt[j]["fbillno"]?.ToString();
                    long basefinishqty = Convert.ToInt64(dt[j]["FBASEFINISHQTY"]);
                    long scddsl = Convert.ToInt64(dt[j]["F_PAEZ_SCDDSL"]);
                    long gsHb = Convert.ToInt64(dt[j]["F_PAEZ_GSHB"]);

                    var judgeSql = string.Format($@"/*dialect*/SELECT SUM(FBASEFINISHQTY) as zjbsl FROM 
                                                      T_PRD_MORPT A
                                                      JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                      WHERE FSRCBILLNO = '{moNo}' 
                                                      and a.fbillno <> '{billno}'");
                    DynamicObjectCollection judgeDt = DBUtils.ExecuteDynamicObject(this.Context, judgeSql);
                    if (judgeDt != null && judgeDt.Count > 0)
                    {
                        if (judgeDt[0]["zjbsl"] != null && judgeDt[0]["zjbsl"] != DBNull.Value)
                        {
                            long finishqty = Convert.ToInt64(judgeDt[0]["zjbsl"]);
                            if (basefinishqty + finishqty == scddsl && gsHb == 0)
                            {
                                Getgshb(moNo);
                            }
                        }
                    }
                }
            }


        }

        private void Getgshb(string moNo)
        {
            var moSql = string.Format($@"/*dialect*/SELECT 
                                                      F_PAEZ_GSHB,FHRWORKTIME
                                                    FROM
                                                      T_PRD_MORPT A
                                                      JOIN T_PRD_MORPTENTRY B ON A.FID = B.FID
                                                      JOIN T_PRD_MORPTENTRY_A B1 ON B.FENTRYID = B1.FENTRYID 
                                                    WHERE
                                                      FSRCBILLNO = '{moNo}'");
            DynamicObjectCollection moDt = DBUtils.ExecuteDynamicObject(this.Context, moSql);
            if (moDt == null || moDt.Count == 0)
                return;

            bool flag = !moDt.Any(item => Convert.ToDecimal(item["FHRWORKTIME"]) != 0.00m);

            if (flag)
                throw new Exception("该订单未做工时汇报，请检查。");

        }
    }
}
