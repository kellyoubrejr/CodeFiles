using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Kingdee.K3.MFG.App.AppServiceContext;

namespace Zitn_wlCGZQ_PluginDev4
{
    [Description("服务插件,计算物料最快/最慢采购周期并写入日志表")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Class1 : IScheduleService
    {
        public void Run(Kingdee.BOS.Context ctx, Schedule schedule)
        {
            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin("688399bec6449e", "admin", "Flzx3qc!", 2052);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();

            if (resultType != 1)
            {
                return;
            }

            DbUtils.Execute(ctx, "DELETE FROM T_ZITN_MATERIAL_CYCLE_LOG");

            string query = @"/*dialect*/SELECT A.FNUMBER, B.FNAME, B.FSPECIFICATION
FROM T_BD_MATERIAL A
JOIN T_BD_MATERIAL_L B ON A.FMATERIALID = B.FMATERIALID AND B.FLOCALEID = 2052
WHERE A.FUSEORGID = '101006' AND A.FDOCUMENTSTATUS = 'C'";
            DynamicObjectCollection materialList = DbUtils.ExecuteDynamicObject(ctx, query);

            if (materialList == null || materialList.Count == 0) return;

            foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject material in materialList)
            {
                string materialNumber = material["FNUMBER"]?.ToString() ?? "";
                string materialName = material["FNAME"]?.ToString() ?? "";
                string materialSpec = material["FSPECIFICATION"]?.ToString() ?? "";

                // 查询该物料三年内所有 PO→收料 记录（不关联入库，避免笛卡尔积）
                var allRecords = GetPoReceiveRecords(ctx, materialNumber);

                if (allRecords == null || allRecords.Count == 0) continue;

                // 过滤掉负间隔，保留 >=0（当天采购当天收料间隔为0，记录为1天）
                var validRecords = allRecords
                    .Where(r => r.IntervalDays >= 0)
                    .ToList();

                if (validRecords.Count == 0) continue;

                var fastest = validRecords.OrderBy(r => r.IntervalDays).First();
                var slowest = validRecords.OrderByDescending(r => r.IntervalDays).First();

                // 间隔为0的（当天采购当天收料）统一记录为1天
                if (fastest.IntervalDays == 0) fastest.IntervalDays = 1;
                if (slowest.IntervalDays == 0) slowest.IntervalDays = 1;

                // 分别查最快和最慢那条收料对应的入库单
                fastest.StockBillNo = GetStockBillNo(ctx, fastest.ReceiveBillNo, materialNumber);
                slowest.StockBillNo = GetStockBillNo(ctx, slowest.ReceiveBillNo, materialNumber);

                InsertCycleLog(ctx, materialNumber, materialName, materialSpec, fastest, slowest);
            }
        }

        /// <summary>
        /// 查询物料三年内所有 PO→收料 记录（不含入库单，避免笛卡尔积膨胀）
        /// </summary>
        private List<CycleRecord> GetPoReceiveRecords(Kingdee.BOS.Context ctx, string materialNumber)
        {
            string sql = string.Format(@"/*dialect*/SELECT
    PO.FBILLNO AS POBILLNO,
    PO.FMODIFYDATE AS POMODIFYDATE,
    PO.FAPPROVEDATE AS POAPPROVEDATE,
    REV.FBILLNO AS RECEIVEBILLNO,
    REV.FDATE AS RECEIVEDATE,
    DATEDIFF(DAY, PO.FMODIFYDATE, REV.FDATE) AS INTERVALDAYS,
    V.FNAME AS POBUYERNAME,
	    SU.FNAME AS GYSNAME
FROM T_PUR_POORDER PO
LEFT JOIN V_BD_BUYER_L V ON PO.FPURCHASERID = V.FID
LEFT JOIN T_BD_SUPPLIER_L SU ON PO.FSUPPLIERID = SU.FSUPPLIERID AND SU.FLOCALEID = 2052
JOIN T_PUR_POORDERENTRY POE ON PO.FID = POE.FID AND PO.FDOCUMENTSTATUS = 'C'
JOIN T_BD_MATERIAL M ON POE.FMATERIALID = M.FMATERIALID AND M.FDOCUMENTSTATUS = 'C' AND M.FUSEORGID = 101006
LEFT JOIN T_PUR_ReceiveENTRY RE ON RE.FSRCBILLNO = PO.FBILLNO AND RE.FMATERIALID = POE.FMATERIALID
LEFT JOIN T_PUR_Receive REV ON RE.FID = REV.FID AND REV.FDOCUMENTSTATUS = 'C'
WHERE M.FNUMBER = '{0}'
  AND PO.FCREATEDATE >= DATEADD(YEAR, -3, CAST(GETDATE() AS DATE))
  AND REV.FDATE IS NOT NULL",
                materialNumber.Replace("'", "''"));

            DynamicObjectCollection list = DbUtils.ExecuteDynamicObject(ctx, sql);
            if (list == null || list.Count == 0) return null;

            var result = new List<CycleRecord>();
            foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject row in list)
            {
                result.Add(new CycleRecord
                {
                    PoBillNo = row["POBILLNO"]?.ToString() ?? "",
                    PoModifyDate = row["POMODIFYDATE"] != null ? Convert.ToDateTime(row["POMODIFYDATE"]) : DateTime.MinValue,
                    PoApproveDate = row["POAPPROVEDATE"] != null ? Convert.ToDateTime(row["POAPPROVEDATE"]) : DateTime.MinValue,
                    ReceiveBillNo = row["RECEIVEBILLNO"]?.ToString() ?? "",
                    ReceiveDate = row["RECEIVEDATE"] != null ? Convert.ToDateTime(row["RECEIVEDATE"]) : DateTime.MinValue,
                    IntervalDays = row["INTERVALDAYS"] != null ? Convert.ToInt32(row["INTERVALDAYS"]) : 0,
                    PoBuyerName = row["POBUYERNAME"]?.ToString() ?? "",
                    GysName = row["GYSNAME"]?.ToString() ?? ""
                });
            }
            return result;
        }

        /// <summary>
        /// 根据收料单单号查对应的入库单单号（多条用逗号拼接）
        /// </summary>
        private string GetStockBillNo(Kingdee.BOS.Context ctx, string receiveBillNo, string materialNumber)
        {
            if (string.IsNullOrEmpty(receiveBillNo)) return "";

            string sql = string.Format(@"/*dialect*/SELECT DISTINCT STK.FBILLNO
FROM t_STK_InStock STK
JOIN T_STK_INSTOCKENTRY SE ON STK.FID = SE.FID AND STK.FDOCUMENTSTATUS = 'C'
JOIN T_BD_MATERIAL M ON SE.FMATERIALID = M.FMATERIALID
WHERE SE.FSRCBILLNO = '{0}'
  AND M.FNUMBER = '{1}'",
                receiveBillNo.Replace("'", "''"),
                materialNumber.Replace("'", "''"));

            DynamicObjectCollection list = DbUtils.ExecuteDynamicObject(ctx, sql);
            if (list == null || list.Count == 0) return "";

            return string.Join(",", list.Select(r => r["FBILLNO"]?.ToString() ?? ""));
        }

        private void InsertCycleLog(Kingdee.BOS.Context ctx, string materialNumber, string materialName, string materialSpec,
            CycleRecord fastest, CycleRecord slowest)
        {
            string notes;
            if (fastest.IntervalDays == slowest.IntervalDays)
                notes = string.Format("采购周期{0}天（仅有一笔有效记录）", fastest.IntervalDays);
            else
                notes = string.Format("最快采购周期{0}天，最慢采购周期{1}天", fastest.IntervalDays, slowest.IntervalDays);

            string sql = string.Format(@"
INSERT INTO T_ZITN_MATERIAL_CYCLE_LOG
(FMATERIALNUMBER, FMATERIALNAME, FMATERIALSPEC,
 FFAST_POBILLNO, FFAST_POMODIFYDATE, FFAST_POAPPROVEDATE, FFAST_POBUYER, FFAST_GYSNAME,
 FFAST_SLBILLNO, FFAST_SLDATE,
 FFAST_STOCKBILLNO, FFAST_STOCKCREATEDATE, FFAST_CYCLE_DAYS,
 FSLOW_POBILLNO, FSLOW_POMODIFYDATE, FSLOW_POAPPROVEDATE, FSLOW_POBUYER, FSLOW_GYSNAME,
 FSLOW_SLBILLNO, FSLOW_SLDATE,
 FSLOW_STOCKBILLNO, FSLOW_STOCKCREATEDATE, FSLOW_CYCLE_DAYS,
 FNOTES)
VALUES
('{0}', '{1}', '{2}',
 '{3}', '{4}', '{5}', '{6}', '{7}',
 '{8}', '{9}',
 '{10}', '{11}', {12},
 '{13}', '{14}', '{15}', '{16}', '{17}',
 '{18}', '{19}',
 '{20}', '{21}', {22},
 '{23}')",
                Safe(materialNumber, 100), Safe(materialName, 300), Safe(materialSpec, 500),
                Safe(fastest.PoBillNo, 100), Safe(fastest.PoModifyDate), Safe(fastest.PoApproveDate), Safe(fastest.PoBuyerName, 100), Safe(fastest.GysName, 100),
                Safe(fastest.ReceiveBillNo, 100), Safe(fastest.ReceiveDate),
                Safe(fastest.StockBillNo, 500), Safe(""), fastest.IntervalDays,
                Safe(slowest.PoBillNo, 100), Safe(slowest.PoModifyDate), Safe(slowest.PoApproveDate), Safe(slowest.PoBuyerName, 100), Safe(slowest.GysName, 100),
                Safe(slowest.ReceiveBillNo, 100), Safe(slowest.ReceiveDate),
                Safe(slowest.StockBillNo, 500), Safe(""), slowest.IntervalDays,
                Safe(notes, 200));

            DbUtils.Execute(ctx, sql);
        }

        private string Safe(string value, int maxLen = 0)
        {
            var result = (value ?? "").Replace("'", "''");
            if (maxLen > 0 && result.Length > maxLen) result = result.Substring(0, maxLen);
            return result;
        }

        private string Safe(DateTime dt)
        {
            if (dt == DateTime.MinValue) return "";
            return dt.ToString("yyyy-MM-dd");
        }

        private class CycleRecord
        {
            public string PoBillNo { get; set; }
            public DateTime PoModifyDate { get; set; }
            public DateTime PoApproveDate { get; set; }
            public string PoBuyerName { get; set; }
            public string GysName { get; set; }
            public string ReceiveBillNo { get; set; }
            public DateTime ReceiveDate { get; set; }
            public string StockBillNo { get; set; }
            public string StockCreateDate { get; set; }
            public int IntervalDays { get; set; }
        }
    }
}
