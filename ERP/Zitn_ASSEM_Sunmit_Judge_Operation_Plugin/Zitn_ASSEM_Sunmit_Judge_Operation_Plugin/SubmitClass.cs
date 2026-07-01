using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zitn_ASSEM_Sunmit_Judge_Operation_Plugin
{
    [Description("【服务插件】：组装拆卸单审核，校验是否存在在途组装拆卸，若存在，不允许提交组装拆卸单，并提示：该产品已在组装拆卸过程中，请确认！")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SubmitClass : AbstractOperationServicePlugIn
    {
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);

            var currentIds = e.SelectedRows
                .Select(row => row.DataEntity["Id"]?.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (currentIds.Count == 0) return;

            ValidateOnWayAssemblies(currentIds);
        }

        /// <summary>
        /// 校验是否存在在途组装拆卸单
        /// </summary>
        /// <param name="currentIds">当前提交的单据ID集合</param>
        private void ValidateOnWayAssemblies(List<string> currentIds)
        {
            var idList = string.Join(",", currentIds.Select(id => $"'{id}'"));

            var materialSql = $@"
                SELECT DISTINCT M.FNUMBER 
                FROM T_STK_ASSEMBLY A
                INNER JOIN T_STK_ASSEMBLYPRODUCT B ON A.FID = B.FID
                INNER JOIN T_STK_ASSEMBLYSUBITEM C ON B.FENTRYID = C.FENTRYID
                INNER JOIN T_BD_MATERIAL M ON C.FMATERIALID = M.FMATERIALID
                WHERE A.FID IN ({idList})";

            var materialResult = DBUtils.ExecuteDynamicObject(this.Context, materialSql);

            if (materialResult == null || materialResult.Count == 0) return;

            var materialNumbers = new List<string>();
            foreach (var item in materialResult)
            {
                var number = item["FNUMBER"]?.ToString();
                if (!string.IsNullOrEmpty(number))
                {
                    materialNumbers.Add(number);
                }
            }

            if (materialNumbers.Count == 0) return;

            var materialFilter = string.Join(",", materialNumbers.Select(n => $"'{n}'"));

            var onWaySql = $@"
                SELECT DISTINCT A.FBILLNO, M.FNUMBER, M1.FNAME
                FROM T_STK_ASSEMBLY A
                INNER JOIN T_STK_ASSEMBLYPRODUCT B ON A.FID = B.FID
                INNER JOIN T_STK_ASSEMBLYSUBITEM C ON B.FENTRYID = C.FENTRYID
                INNER JOIN T_BD_MATERIAL M ON C.FMATERIALID = M.FMATERIALID
                INNER JOIN T_BD_MATERIAL_L M1 ON M.FMATERIALID = M1.FMATERIALID
                WHERE A.FDOCUMENTSTATUS = 'B'
                    AND A.FID NOT IN ({idList})
                    AND M.FNUMBER IN ({materialFilter})";

            var onWayResult = DBUtils.ExecuteDynamicObject(this.Context, onWaySql);

            if (onWayResult != null && onWayResult.Count > 0)
            {
                var conflictMaterials = new HashSet<string>();
                var conflictBillNos = new HashSet<string>();

                foreach (var item in onWayResult)
                {
                    var billNo = item["FBILLNO"]?.ToString();
                    var materialNumber = item["FNUMBER"]?.ToString();
                    var materialName = item["FNAME"]?.ToString();

                    if (!string.IsNullOrEmpty(billNo))
                        conflictBillNos.Add(billNo);
                    if (!string.IsNullOrEmpty(materialNumber))
                        conflictMaterials.Add($"{materialNumber}({materialName})");
                }

                var message = new StringBuilder();
                message.AppendLine("提交失败：该产品已在组装拆卸过程中，请确认！");
                message.AppendLine($"在途单据号：{string.Join("、", conflictBillNos)}");

                throw new KDBusinessException("提交失败", message.ToString());
            }
        }
    }
}