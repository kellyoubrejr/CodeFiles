using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Zitn_ErpToBpmWorkStartFlow_SZ_Plugin
{
    [Description("【服务接口】，生产入库单批量审核-按生产订单维度检测是否" +
        "合规调用bpmapi发起试制流程"), HotUpdate]
    public class WorkFlowStartPrdMo : AbstractOperationServicePlugIn
    {
        private static readonly string LogPath = @"D:\金蝶自定义日志文件\试制总结.txt";

        private static void WriteLog(string msg)
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}",
                    Encoding.UTF8);
            }
            catch { /* 写日志失败不影响主流程 */ }
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            /*
             * ============================================================
             * 登录
             * ============================================================
             */
            #region 登录
            K3CloudApiClient client = new K3CloudApiClient("http://10.0.32.18/k3cloud/");
            var loginResult = client.ValidateLogin(
                "688399bec6449e",
                "admin",
                "Flzx3qc!",
                2052
            );

            /*K3CloudApiClient client = new K3CloudApiClient("http://localhost/k3cloud/");
            var loginResult = client.ValidateLogin(
                "6940c27ae377d5",
                "13210075502",
                "123456",
                2052
            );*/

            if (JObject.Parse(loginResult)["LoginResultType"].Value<int>() != 1)
                return;            
            #endregion

            /*
             * ============================================================
             * 从批量审核的选中行中收集所有入库单FID
             * e.SelectedRows = 用户在审批界面勾选的所有单据行
             * ============================================================
             */
            var fids = string.Join(",", e.SelectedRows
                .Select(row => row.DataEntity["Id"]?.ToString()));
            if (string.IsNullOrEmpty(fids)) return;
            WriteLog($"========== 批量审核开始，FIDs: {fids} ==========");

            /*
             * ============================================================
             *  按(生产订单号 + 物料编码)分组去重
             *
             * 分组维度：(FMOBILLNO, FNUMBER) = (生产订单号, 物料编码)
             *
             * 去重逻辑：
             *   同一个key在本次审核的多张入库单中出现时，GROUP BY 合并成一行
             *   例如：A订单A物料在入库单1(数量4)和入库单2(数量8)各出现一次
             *        → 合并为一行，TOTALREALQTY=12，FBILLNO取创建时间最新的
             *
             * 字段说明：
             *   FMOBILLNO       = 生产订单号
             *   FNUMBER         = 物料编码
             *   FQTY            = 生产订单数量（来自生产订单分录MOE）
             *   TOTALREALQTY    = 本次审核入库单中该key的入库数量合计 SUM(B.FREALQTY)
             *   FBILLNO         = 该key在本次审核中最新入库单的单号（用于后续大SQL）
             *
             * 关联路径：
             *   T_PRD_INSTOCK        (A) 生产入库单-主表
             *   T_PRD_INSTOCKENTRY   (B) 生产入库单-明细表
             *   T_BD_MATERIAL        (M) 物料表
             *   T_PRD_MOENTRY        (MOE) 生产订单-分录表 (B.FMOENTRYID = MOE.FENTRYID)
             * ============================================================
             */
            var summarySql = string.Format($@"/*dialect*/SELECT A.FBILLNO,
                B.FMOBILLNO,
                M.FNUMBER,
                MOE.FQTY,
                MOA.FSTOCKINQUASELAUXQTY,
                MAX(A.FCREATEDATE) AS MAXCREATEDATE
            FROM T_PRD_INSTOCK A
            JOIN T_PRD_INSTOCKENTRY B ON A.FID = B.FID
            JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
            JOIN T_PRD_MOENTRY MOE ON B.FMOENTRYID = MOE.FENTRYID
            JOIN T_PRD_MOENTRY_A MOA ON MOE.FENTRYID = MOA.FENTRYID
            WHERE A.FID IN ({fids}) AND M.FNUMBER LIKE '90%'
            GROUP BY B.FMOBILLNO, M.FNUMBER, MOE.FQTY ,A.FBILLNO,MOA.FSTOCKINQUASELAUXQTY");

            DynamicObjectCollection summaryData = DBUtils.ExecuteDynamicObject(this.Context, summarySql);
            if (summaryData == null || summaryData.Count == 0) return;

            /*
             * ============================================================
             * 遍历汇总结果（每行=一个唯一的(生产订单号+物料编码)）
             *
             * summarySql已通过GROUP BY去重，key要么唯一要么已合并
             * 循环内做3件事：
             *   (a) 80%阈值校验：TOTALREALQTY / FQTY > 80% 才发起
             *   (b) 用FBILLNO执行大SQL，按物料编码匹配明细行
             *   (c) 查生产订单信息(scddSql) → 获取token → 调BPM接口
             * ============================================================
             */
            foreach (DynamicObject item in summaryData)
            {
                string scddBillNo = "";
                string materialCode = "";

                try
                {
                    // key：(生产订单号, 物料编码) —— 已通过GROUP BY去重
                    scddBillNo = item["FMOBILLNO"].ToString();
                    materialCode = item["FNUMBER"].ToString();

                    /*
                     * (a) 80%阈值校验
                 * 累计入库数量 / 生产订单数量 > 80% 才发起BPM，否则跳过
                 */
                decimal fqty = Convert.ToDecimal(item["FQTY"]);
                decimal totalRealQty = Convert.ToDecimal(item["FSTOCKINQUASELAUXQTY"]);
                if (fqty <= 0 || (totalRealQty / fqty) <= 0.8m)
                {
                    string ratioStr = fqty > 0 ? (totalRealQty / fqty).ToString("P2") : "0%";
                    WriteLog($"跳过(未达80%): 生产订单号={scddBillNo}, 物料编码={materialCode}, " +
                        $"累计入库={totalRealQty}, 订单数量={fqty}, 比例={ratioStr}");
                    continue;
                }

                // summarySql已通过子查询取到该key最新入库单的单号
                string billno = item["FBILLNO"].ToString();

                /*
                 * 根据入库单单号查询入库单完整信息
                 *
                 * 查询字段对应BPM接口入参：
                 *   RKDCJR      → creater         入库登记人
                 *   RKDZZ       → org             入库组织
                 *   CPSYB       → shiyebu         产品事业部
                 *   MATERIALCODE→ productCode      物料编码
                 *   MATERIALNAME→ productName      物料名称
                 *   MATERIALSPEC→ model            物料规格
                 *   SCGCPCH     → developmentPhase 研发阶段(取-后部分)
                 *   QTY         → makeQty          生产订单数量
                 *   LJRKSL      → inStoreQty       累计入库数量
                 *   HGRKSL      → hgRkQty          合格入库数量
                 *   KGRQ        → productDate      开工日期
                 *   RKEQ        → rkDate           入库日期
                 *   SCCJ        → workShop         生产车间
                 */
                var scrkSql = string.Format($@"/*dialect*/SELECT
                    T.FNAME AS RKDCJR,
                    A.FCREATEDATE AS RKDCJSJ,
                    CASE
                      WHEN A.FSTOCKORGID = '1' THEN '青岛智腾科技有限公司'
                      WHEN A.FSTOCKORGID = '101006' THEN '青岛智腾微电子有限公司'
                      WHEN A.FSTOCKORGID = '101007' THEN '青岛智腾电源有限公司'
                      WHEN A.FSTOCKORGID = '101050' THEN 'test'
                      WHEN A.FSTOCKORGID = '1404303' THEN '青岛智腾烽行能源有限公司'
                      WHEN A.FSTOCKORGID = '1516310' THEN '青岛晶英电子科技有限公司'
                      WHEN A.FSTOCKORGID = '3149866' THEN '青岛智腾微电子有限公司北京分公司'
                      WHEN A.FSTOCKORGID = '3241152' THEN '青岛加速度智能科技有限公司'
                      WHEN A.FSTOCKORGID = '4032930' THEN '青岛智腾微电子有限公司西安分公司'
                      WHEN A.FSTOCKORGID = '4665868' THEN '青岛智导电子有限公司'
                      WHEN A.FSTOCKORGID = '4665869' THEN '青岛深科睿探技术有限公司'
                      WHEN A.FSTOCKORGID = '4852744' THEN '青岛智导电子有限公司北京分公司'
                     END AS RKDZZ,
                     FCPSYB AS CPSYB,
                     M.FNUMBER AS MATERIALCODE,
                     M1.FNAME AS MATERIALNAME,
                     M1.FSPECIFICATION AS MATERIALSPEC,
                     A.FSCGCPCH AS SCGCPCH,
                     MO.FQTY AS QTY,
                     MOA.FSTOCKINQUASELAUXQTY AS LJRKSL,
                     MOA.FSTOCKINQUAAUXQTY AS HGRKSL,
                     MOA.FSTARTDATE AS KGRQ,
                     A.FDATE AS RKEQ,
                     DEP.FNAME AS SCCJ,
                     CASE
                        WHEN B.FCPSYB =1 THEN '一部：民品事业部'
                        WHEN B.FCPSYB =2 THEN '二部：军品电子事业部'
                        WHEN B.FCPSYB =3 THEN '三部：传感技术事业部'
                        WHEN B.FCPSYB =4 THEN '事业四部'
                        WHEN B.FCPSYB =11 THEN '事业五部'
                      END AS SYB
                    FROM T_PRD_INSTOCK A
                    JOIN T_PRD_INSTOCKENTRY B ON A.FID = B.FID
                    JOIN T_PRD_INSTOCKENTRY_A BA ON B.FENTRYID = BA.FENTRYID
                    JOIN T_SEC_USER T ON A.FCREATORID = T.FUSERID
                    JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                    JOIN T_BD_MATERIAL_L M1 ON M.FMATERIALID = M1.FMATERIALID
                    JOIN T_PRD_MOENTRY MO ON B.FMOENTRYID = MO.FENTRYID
                    JOIN T_PRD_MOENTRY_A MOA ON MO.FENTRYID = MOA.FENTRYID
                    JOIN T_BD_DEPARTMENT_L DEP ON B.FWORKSHOPID = DEP.FDEPTID
                    WHERE A.FBILLNO = '{billno}'");

                DynamicObjectCollection scrkData = DBUtils.ExecuteDynamicObject(this.Context, scrkSql);
                if (scrkData == null || scrkData.Count == 0) continue;

                /*
                 * 一张入库单可能有多行明细（不同物料），需要精确匹配到当前物料编码那一行
                 * 因为前面是按(生产订单号+物料编码)分组的，这里找对应的物料行
                 */
                DynamicObject matchedItem = null;
                foreach (DynamicObject row in scrkData)
                {
                    if (row["MATERIALCODE"].ToString() == materialCode)
                    {
                        matchedItem = row;
                        break;
                    }
                }
                if (matchedItem == null) continue;

                // ---- 大SQL匹配到物料行后，提取BPM需要的字段 ----
                string rkdzz = matchedItem["RKDZZ"].ToString();           // 入库组织
                string cpsybName = matchedItem["SYB"].ToString();       // 产品事业部(名称)
                string matCode = matchedItem["MATERIALCODE"].ToString();  // 物料编码
                string materialname = matchedItem["MATERIALNAME"].ToString();     // 物料名称
                string materialspec = matchedItem["MATERIALSPEC"].ToString();     // 物料规格
                // 研发阶段取自 SCGCPCH(生产产品批号) 的第二个位置内容切片 如BD001 -->D
                string yzjd = matchedItem["SCGCPCH"].ToString().Substring(1, 1);             // 研发阶段
                string qty = matchedItem["QTY"].ToString();               // 生产订单数量
                string ljrkls = matchedItem["LJRKSL"].ToString();         // 累计入库数量
                string hgrksl = matchedItem["HGRKSL"].ToString();         // 合格入库数量
                string kgrq = matchedItem["KGRQ"].ToString();             // 开工日期
                string rkeq = matchedItem["RKEQ"].ToString();             // 入库日期
                string sccj = matchedItem["SCCJ"].ToString();             // 生产车间

                // 生产车间 → 责任人ID映射（creater字段用）
                var workshopUserMap = new Dictionary<string, string>
                {
                    { "电装车间", "ff8080818fd2b96c018fd2bbd63e0668" },
                    { "微电子车间", "ff8080818fd2b96c018fd2bb32eb023e" },
                    { "传感器车间", "ff8080818fd2b96c018fd2bbcd6a0627" },
                    { "加表车间", "ff8080818fd2b96c018fd2bbcd6a0627" },
                    { "加速度计车间", "ff8080818fd2b96c018fd2bbcd6a0627" },
                    { "机加工车间", "ff8080818fd2b96c018fd2bb419002bc" },
                    { "电测车间", "ff8080818fd2b96c018fd2bb32eb023e" },
                };
                string rkdcjr = workshopUserMap.TryGetValue(sccj, out var userId)
                    ? userId : "";

                // 事业部名称 → 组织ID映射（shiyebu字段用）
                var deptOrgMap = new Dictionary<string, string>
                {
                    { "事业五部", "ff8080818fd22d5b018fd271c1c40261" },
                    { "一部：民品事业部", "ff8080818fd22d5b018fd271c56c028d" },
                    { "二部：军品电子事业部", "ff8080818fd22d5b018fd271c6dd029d" },
                    { "三部：传感技术事业部", "ff8080818fd22d5b018fd271c90202b9" },
                    { "事业四部", "ff808081948850b401948d5a28531958" },
                };
                string cpsyb = deptOrgMap.TryGetValue(cpsybName, out var orgId)
                    ? orgId : "";

                var deptmentMap = new Dictionary<string, string>
                {
                    { "电装车间", "ff80808194de9bc30194dfc15d712392" },
                    { "微电子车间", "ff80808194de9bc30194dfc15fb32398" },
                    { "传感器车间", "ff8080818fd22d5b018fd271c9ff02c6" },
                    { "加表车间", "ff8080818fd22d5b018fd271c9ff02c6" },
                    { "加速度计车间", "ff8080818fd22d5b018fd271c9ff02c6" },
                    { "机加工车间", "ff80808194de9bc30194dfc16097239b" },
                    { "电测车间", "ff80808194de9bc30194dfc15fb32398" },
                };
                string deptmentId = deptmentMap.TryGetValue(sccj, out var deptId)
                    ? deptId : "";

                /*
                 * (c) 根据物料编码查询生产订单信息(scddSql)
                 *
                 * 查询该物料关联的、状态为5/6/7(已确认/已下达/已开工)的生产订单
                 * 按前缀去重，最多取前3个不同前缀的生产订单号
                 * 拼接格式："订单号1:合格数量\n订单号2:合格数量"
                 * 存入BPM的 LongText1780472941618 字段
                 */
                var scddSql = string.Format($@"/*dialect*/SELECT
                    FBILLNO,
                    BA.FSTOCKINQUAAUXQTY AS HGSL,
                    (B.FQTY - BQ.FNOSTOCKINQTY) AS LJRKSL,
                    A.FCREATEDATE
                FROM T_PRD_MO A
                JOIN T_PRD_MOENTRY B ON A.FID = B.FID
                JOIN T_PRD_MOENTRY_A BA ON B.FENTRYID = BA.FENTRYID
                JOIN T_PRD_MOENTRY_Q BQ ON B.FENTRYID = BQ.FENTRYID
                JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                WHERE M.FNUMBER = '{matCode}' AND (BA.FSTATUS = 5 OR BA.FSTATUS = 6 OR BA.FSTATUS = 7)
                ORDER BY A.FCREATEDATE DESC");

                DynamicObjectCollection scddData = DBUtils.ExecuteDynamicObject(this.Context, scddSql);
                var scddInfo = new StringBuilder();
                if (scddData != null && scddData.Count > 0)
                {
                    // 按前缀去重，保留前3个不同前缀，同一前缀的订单全部拼接
                    var prefixSet = new HashSet<string>();
                    for (int j = 0; j < scddData.Count; j++)
                    {
                        var scddItem = scddData[j];
                        string moBillNo = scddItem["FBILLNO"].ToString();
                        // 前缀 = 订单号-前部分（如 "GZ2502-001" → "GZ2502"）
                        string prefix = moBillNo.Contains('-') ? moBillNo.Split('-')[0] : moBillNo;

                        // 已经有3个不同前缀且当前前缀不在集合中，停止
                        if (prefixSet.Count >= 3 && !prefixSet.Contains(prefix))
                            break;

                        prefixSet.Add(prefix);
                        // HGSL=合格数量, LJRKSL=累计入库数量
                        // 写入格式: 生产订单号:百分比%(HGSL/LJRKSL)
                        decimal hgslVal = Convert.ToDecimal(scddItem["HGSL"]);
                        decimal ljrkslVal = Convert.ToDecimal(scddItem["LJRKSL"]);
                        decimal ratio = ljrkslVal > 0 ? hgslVal / ljrkslVal : 0;

                        if (scddInfo.Length > 0)
                            scddInfo.Append('\n');
                        scddInfo.Append(moBillNo).Append(':')
                                                .Append((ratio * 100).ToString("F2")).Append("%(")
                                                .Append(hgslVal == 0 ? "0" : hgslVal.ToString().TrimEnd('0').TrimEnd('.'))
                                                .Append('/')
                                                .Append(ljrkslVal == 0 ? "0" : ljrkslVal.ToString().TrimEnd('0').TrimEnd('.'))
                                                .Append(')');
                    }
                }

                // ========== 详细日志：记录查询到的所有字段信息 ==========
                WriteLog($"【满足80%条件，开始处理】");
                WriteLog($"  生产订单号: {scddBillNo}");
                WriteLog($"  物料编码: {matCode}");
                WriteLog($"  物料名称: {materialname}");
                WriteLog($"  物料规格: {materialspec}");
                WriteLog($"  入库单号: {billno}");
                WriteLog($"  入库组织: {rkdzz}");
                WriteLog($"  产品事业部: {cpsybName}");
                WriteLog($"  研发阶段: {yzjd}");
                WriteLog($"  生产订单数量: {qty}");
                WriteLog($"  累计入库数量: {ljrkls}");
                WriteLog($"  合格入库数量: {hgrksl}");
                WriteLog($"  开工日期: {kgrq}");
                WriteLog($"  入库日期: {rkeq}");
                WriteLog($"  生产车间: {sccj}");
                WriteLog($"  生产车间匹配责任人ID: {(string.IsNullOrEmpty(rkdcjr) ? "(未匹配)" : rkdcjr)}");
                WriteLog($"  事业部匹配组织ID: {(string.IsNullOrEmpty(cpsyb) ? "(未匹配)" : cpsyb)}");
                WriteLog($"  生产车间匹配部门ID: {(string.IsNullOrEmpty(deptmentId) ? "(未匹配)" : deptmentId)}");
                WriteLog($"  生产订单拼接信息: {(scddInfo.Length > 0 ? scddInfo.ToString() : "(无)")}");

                /*
                 * ============================================================
                 * 第4步：获取BPM认证token (OAuth2 client_credentials模式)
                 * ============================================================
                 */
                var tokenParams = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", "xclient" },
                    { "client_secret", "0a417ecce58c31b32364ce19ca8fcd15" }
                };
                string accessToken = "";
                using (var httpClient = new HttpClient())
                {
                    WriteLog($"【获取BPM Token】请求地址: http://10.0.32.10:8769/api/oauth/token, grant_type=client_credentials");
                    var tokenResponse = httpClient.PostAsync(
                        "http://10.0.32.10:8769/api/oauth/token",
                        new FormUrlEncodedContent(tokenParams)
                    ).Result;
                    var tokenJson = tokenResponse.Content.ReadAsStringAsync().Result;
                    WriteLog($"【获取BPM Token】响应: {tokenJson}");
                    var tokenObj = JObject.Parse(tokenJson);
                    accessToken = tokenObj["access_token"].ToString();
                    WriteLog($"【获取BPM Token】成功, access_token前缀: {accessToken.Substring(0, Math.Min(20, accessToken.Length))}...");
                }

                /*
                 * ============================================================
                 * 第5步：调用BPM发起流程接口
                 *
                 * 接口：POST /api/openapi/v3/workflow/start
                 * 流程：trial_production_report_flow (试制报告流程)
                 *
                 * 关键参数：
                 *   data             = 表单数据(含上面查到的所有字段)
                 *   departmentId     = 发起部门ID
                 *   userId           = 发起人ID
                 *   workflowCode     = 流程编码
                 *   finishStart      = true(直接发起，不经过草稿)
                 * ============================================================
                 */
                var workflowBody = new JObject
                {
                    ["data"] = new JObject
                    {
                        ["creater"] = rkdcjr,
                        ["org"] = rkdzz,
                        ["shiyebu"] = cpsyb,
                        ["productCode"] = matCode,
                        ["productName"] = materialname,
                        ["model"] = materialspec,
                        ["developmentPhase"] = yzjd,
                        ["makeQty"] = qty,
                        ["inStoreQty"] = ljrkls,
                        ["hgRkQty"] = hgrksl,
                        ["productDate"] = kgrq,
                        ["rkDate"] = rkeq,
                        ["workShop"] = sccj,
                        ["scrkOrderNo"] = billno,
                        ["LongText1780472941618"] = scddInfo.ToString()
                    },
                    ["departmentId"] = deptmentId,
                    //测试写死
                    //["departmentId"]= "ff8080818fd22d5b018fd271c9ff02c6",
                    ["finishStart"] = true,
                    ["nextParticipants"] = new JArray
                    {
                        new JObject
                        {
                            ["activityCode"] = "",
                            ["participants"] = new JArray()
                        }
                    },
                    ["trustor"] = "",
                    ["userId"] = rkdcjr,
                    ["workflowCode"] = "trial_production_report_flow"
                };

                WriteLog($"【调用BPM发起流程】");
                WriteLog($"  请求地址: http://10.0.32.10:8769/api/openapi/v3/workflow/start");
                WriteLog($"  流程编码: trial_production_report_flow");
                WriteLog($"  请求JSON: {workflowBody.ToString()}");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    var content = new StringContent(workflowBody.ToString(), Encoding.UTF8, "application/json");
                    var workflowResponse = httpClient.PostAsync(
                        "http://10.0.32.10:8769/api/openapi/v3/workflow/start",
                        content
                    ).Result;
                    var workflowResult = workflowResponse.Content.ReadAsStringAsync().Result;

                    WriteLog($"【BPM响应】HTTP状态: {(int)workflowResponse.StatusCode}, 响应JSON: {workflowResult}");

                    // 解析BPM接口返回，判断是否调用成功
                    var resultObj = JObject.Parse(workflowResult);
                    int errcode = resultObj["errcode"]?.Value<int>() ?? -1;
                    string errmsg = resultObj["errmsg"]?.ToString() ?? "";

                    if (errcode == 0 && errmsg == "流程启动成功")
                    {
                        // 调用成功 → 更新生产订单分录的FSFFQSZ字段为1
                        var updateMoSql = string.Format($@"/*dialect*/UPDATE B
                        SET B.FSFFQSZ = 1
                        FROM T_PRD_MO A
                        JOIN T_PRD_MOENTRY B ON A.FID = B.FID
                        JOIN T_BD_MATERIAL M ON B.FMATERIALID = M.FMATERIALID
                        WHERE M.FNUMBER = '{matCode}' AND A.FBILLNO = '{scddBillNo}'");

                        DBUtils.Execute(this.Context, updateMoSql);
                        WriteLog($"【处理成功】生产订单号={scddBillNo}, 物料编码={matCode}, " +
                            $"入库单号={billno}, 已更新FSFFQSZ=1, 更新SQL: {updateMoSql}");
                    }
                    else
                    {
                        WriteLog($"【处理失败】生产订单号={scddBillNo}, 物料编码={matCode}, " +
                            $"入库单号={billno}, errcode={errcode}, errmsg={errmsg}, " +
                            $"完整BPM响应: {workflowResult}");
                        // 调用失败 → 抛出异常，携带BPM返回的错误信息
                        throw new KDBusinessException($"失败","BPM发起流程失败: errcode={errcode}, errmsg={errmsg}, " +
                            $"生产订单号={scddBillNo}, 物料编码={matCode},错误信息{errmsg}");
                    }
                }
                }
                catch (Exception ex)
                {
                    WriteLog($"【异常捕获】生产订单号={scddBillNo}, 物料编码={materialCode}, " +
                        $"异常类型={ex.GetType().Name}, 异常消息={ex.Message}");
                    if (ex.InnerException != null)
                        WriteLog($"  内部异常: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    WriteLog($"  堆栈跟踪: {ex.StackTrace}");
                    throw;
                }
            }
        }
    }
}
