using System;
using System.Collections.Generic;
using System.Linq;
using Zitn_exe_App.Models;

namespace Zitn_exe_App.Utils
{
    public static class ValidateHelper
    {
        // ==================== 是否有未保存数据 ====================
        public static bool HasUnsavedData(
            List<RuleDto> list,
            List<RuleDto> original,
            string supplier,
            string materialCode)
        {
            return list.Any(x =>
                x.Supplier == supplier &&
                x.MaterialCode == materialCode);
        }

        /// <summary>
        /// 获取修改的数据提示
        /// </summary>
        public static List<string> GetModifiedDataDetails(
            List<RuleDto> current,
            List<RuleDto> original)
        {
            List<string> modifiedList = new List<string>();

            foreach (var item in current)
            {
                var old = original.FirstOrDefault(x =>
                    x.MaterialId == item.MaterialCode &&
                    x.Supplier == item.Supplier);

                if (old == null)
                    continue;

                if (item.Up != old.Up ||
                    item.Price != old.Price ||
                    item.MaterialName != old.MaterialName ||
                    item.MaterialSpec != old.MaterialSpec ||
                    item.Unit != old.Unit ||
                    item.Source != old.Source)
                {
                    modifiedList.Add(
                        $"供应商:{item.Supplier} | " +
                        $"物料:{item.MaterialCode} | " +
                        $"上限:{old.Up}→{item.Up} | " +
                        $"价格:{old.Price}→{item.Price}"
                    );
                }
            }

            return modifiedList;
        }

        /// <summary>
        /// 上限唯一校验
        /// </summary>
        public static bool ValidateUpUnique(List<RuleDto> data, Action<string> showMsg)
        {
            var duplicateUps = data
                .GroupBy(x => x.Up)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateUps.Count > 0)
            {
                var duplicateInfo = string.Join(", ", duplicateUps);

                showMsg?.Invoke(
                    $"上限值不能重复！\n\n重复的上限值：{duplicateInfo}\n\n请修改后再保存。");

                return false;
            }

            return true;
        }

        /// <summary>
        /// 必填校验
        /// </summary>
        public static bool ValidateRequiredFields(
    List<RuleDto> data,
    Action<string> showMsg,
    Func<string, bool> confirm)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                if (string.IsNullOrWhiteSpace(item.MaterialCode))
                {
                    showMsg?.Invoke($"第 {i + 1} 行：物料编码不能为空！");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(item.Supplier))
                {
                    showMsg?.Invoke($"第 {i + 1} 行：供应商不能为空！");
                    return false;
                }

                if (item.Up <= 0)
                {
                    showMsg?.Invoke($"第 {i + 1} 行：上限必须大于0！");
                    return false;
                }

                if (item.Price <= 0)
                {
                    showMsg?.Invoke($"第 {i + 1} 行：价格必须大于0！");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(item.Unit))
                {
                    if (confirm != null)
                    {
                        bool ok = confirm($"第 {i + 1} 行单位为空，是否继续？");
                        if (!ok) return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 价格递减校验（上限递增 → 价格必须递减）
        /// </summary>
        public static bool ValidatePriceDescendingAfterSort(List<RuleDto> sortedByUp, Action<string> showMsg)
        {
            for (int i = 0; i < sortedByUp.Count - 1; i++)
            {
                int up1 = sortedByUp[i].Up;
                int up2 = sortedByUp[i + 1].Up;
                decimal price1 = sortedByUp[i].Price;
                decimal price2 = sortedByUp[i + 1].Price;

                bool isError = false;

                if (up2 > up1)
                {
                    
                    isError = price1 <= price2;
                }
                else if (up2 < up1)
                {
                    isError = price1 >= price2;
                }
                else
                {
                    isError = true;
                }

                if (isError)
                {
                    string upTrend = up2 > up1 ? "数量增加" :
                                     up2 < up1 ? "数量减少" : "数量不变";
                    string priceTrend = price2 > price1 ? "价格上升" :
                                        price2 < price1 ? "价格下降" : "价格不变";
                    string expectation = up2 > up1
                        ? "上限递增时价格必须递减"
                        : up2 < up1
                            ? "上限递减时价格必须递增"
                            : "上限不变时价格不能相同";

                    int row1 = sortedByUp[i].RowIndex > 0 ? sortedByUp[i].RowIndex : i + 1;
                    int row2 = sortedByUp[i + 1].RowIndex > 0 ? sortedByUp[i + 1].RowIndex : i + 2;

                    showMsg?.Invoke(
                        $"第{row1}行 vs 第{row2}行：\n" +
                        $"上限 {up1} → {up2}（{upTrend}），价格 {price1} → {price2}（{priceTrend}）\n\n" +
                        $"{expectation}，请调整后重新保存。");

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 生成下限 + 区间校验
        /// </summary>
        public static List<RuleDto> GenerateDownAndCheckConflict(List<RuleDto> sortedByUp, Action<string> showMsg)
        {
            var result = sortedByUp.Select(item => new RuleDto
            {
                MaterialId = item.MaterialId,
                MaterialCode = item.MaterialCode,
                MaterialName = item.MaterialName,
                MaterialSpec = item.MaterialSpec,
                Supplier = item.Supplier,
                Down = item.Down,
                Up = item.Up,
                Qty = item.Qty,
                Price = item.Price,
                Unit = item.Unit,
                Source = item.Source,
                BillNo = item.BillNo
            }).ToList();

            int currentDown = 0;

            for (int i = 0; i < result.Count; i++)
            {
                result[i].Down = currentDown;

                if (result[i].Down >= result[i].Up)
                {
                    showMsg?.Invoke(
                        $"区间冲突：{result[i].Down}~{result[i].Up}");

                    return null;
                }

                currentDown = result[i].Up;
            }

            return result;
        }
    }
}