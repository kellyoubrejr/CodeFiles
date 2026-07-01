using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using static Kingdee.K3.MFG.App.AppServiceContext;

namespace Zitn_wlCGZQ_PluginDev4
{
    [Description("服务插件,计算物料最快/最慢采购周期并更新物料表")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Class2 : IScheduleService
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

            // 批量聚合查询所有物料的采购周期（最小/最大间隔天数）
            string query = @"/*dialect*/SELECT
    M.FMATERIALID,
    MIN(DATEDIFF(DAY, PO.FMODIFYDATE, REV.FDATE)) AS MIN_INTERVAL,
    MAX(DATEDIFF(DAY, PO.FMODIFYDATE, REV.FDATE)) AS MAX_INTERVAL
FROM T_PUR_POORDER PO
JOIN T_PUR_POORDERENTRY POE ON PO.FID = POE.FID AND PO.FDOCUMENTSTATUS = 'C'
JOIN T_BD_MATERIAL M ON POE.FMATERIALID = M.FMATERIALID AND M.FDOCUMENTSTATUS = 'C' AND M.FUSEORGID = 101006
LEFT JOIN T_PUR_ReceiveENTRY RE ON RE.FSRCBILLNO = PO.FBILLNO AND RE.FMATERIALID = POE.FMATERIALID
LEFT JOIN T_PUR_Receive REV ON RE.FID = REV.FID AND REV.FDOCUMENTSTATUS = 'C'
WHERE PO.FCREATEDATE >= DATEADD(YEAR, -3, CAST(GETDATE() AS DATE))
  AND REV.FDATE IS NOT NULL
  AND DATEDIFF(DAY, PO.FMODIFYDATE, REV.FDATE) >= 0
GROUP BY M.FMATERIALID";

            DynamicObjectCollection results = DbUtils.ExecuteDynamicObject(ctx, query);

            if (results == null || results.Count == 0) return;

            foreach (DynamicObject row in results)
            {
                long materialId = Convert.ToInt64(row["FMATERIALID"]);
                int minInterval = Convert.ToInt32(row["MIN_INTERVAL"]);
                int maxInterval = Convert.ToInt32(row["MAX_INTERVAL"]);

                // 间隔为0的（当天采购当天收料）统一记录为1天
                if (minInterval == 0) minInterval = 1;
                if (maxInterval == 0) maxInterval = 1;

                string updateSql = string.Format(
                    @"UPDATE T_BD_MATERIAL SET F_ZMER_QTY_IMU = {0}, F_ZMER_QTY_1XJ = {1} WHERE FMATERIALID = {2}",
                    maxInterval, minInterval, materialId);

                DbUtils.Execute(ctx, updateSql);
            }
        }
    }
}
