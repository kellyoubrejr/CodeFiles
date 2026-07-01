using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zitn_exe_App.Data;
using Zitn_exe_App.Models;
using static Zitn_exe_App.Forms.IndexForm;

namespace Zitn_exe_App.Services
{
    public class RuleService
    {
        /// <summary>
        /// 保存当前分页数据
        /// 逻辑：
        /// 1、只删除当前供应商+当前物料的数据
        /// 2、插入当前页的新数据
        /// 3、不影响其它分页数据
        /// </summary>
        public void SaveToDatabase(
            List<RuleDto> newData,
            string currentSupplier,
            string currentMaterialId,
            List<RuleDto> originalData)
        {
            if (newData == null || newData.Count == 0)
            {
                return;
            }

            // ==================== 1. 删除当前分页旧数据 ====================

            string deleteBodySql = $@"
DELETE FROM ZMER_t_Cust_Entry100101
WHERE FSUPPLIER = N'{currentSupplier.Replace("'", "''")}'
  AND FMATERIALID = {Convert.ToInt64(currentMaterialId)}
";

            DbHelper.ExecuteNonQuery(deleteBodySql);

            string deleteHeaderSql = @"
DELETE FROM ZMER_t_Cust100025
WHERE NOT EXISTS (
    SELECT 1
    FROM ZMER_t_Cust_Entry100101
    WHERE ZMER_t_Cust100025.FID = ZMER_t_Cust_Entry100101.FID
)";
            DbHelper.ExecuteNonQuery(deleteHeaderSql);

            // ==================== 2. 写删除日志 ====================

            var oldPageData = originalData
                .Where(x =>
                    x.Supplier == currentSupplier
                    && x.MaterialCode == currentMaterialId)
                .ToList();

            foreach (var item in oldPageData)
            {
                WriteLog(
                    (
                        item.Supplier ?? "",
                        Convert.ToInt64(item.MaterialId),
                        item.Down,
                        item.Up,
                        item.Price
                    ),
                    ("", 0, 0, 0, 0),
                    "DELETE");
            }

            // ==================== 3. 插入新数据 ====================

            long headerId = GenerateFid();
            string billNo = GenerateBillNo();

            string insertHeaderSql = $@"
INSERT INTO ZMER_t_Cust100025 
(
    FID,
    FBILLNO,
    FDOCUMENTSTATUS,
    F_ZMER_CREATORID_QTR,
    F_ZMER_CREATEDATE_83G,
    F_ZMER_USERID_RE5,
    F_ZMER_DATE_APV,
    FUNAUDIT
)
VALUES
(
    {headerId},
    '{billNo}',
    N'C',
    0,
    GETDATE(),
    0,
    NULL,
    N''
)
";

            DbHelper.ExecuteNonQuery(insertHeaderSql);

            int fseq = 1;

            foreach (var item in newData)
            {
                long entryId = GenerateEntryId();

                string insertBodySql = $@"
INSERT INTO ZMER_t_Cust_Entry100101
(
    FID,
    FEntryID,
    FPURNO,
    FSUPPLIER,
    FQTY,
    FTAXPRICE,
    FMATERIALID,
    F_ZMER_TEXT_QTR,
    F_ZMER_TEXT_83G,
    FSeq,
    FMODTIME,
    FDANWEI
)
VALUES
(
    {headerId},
    {entryId},
    N'{item.Source?.Replace("'", "''") ?? ""}',
    N'{item.Supplier?.Replace("'", "''") ?? ""}',
    N'{item.Qty}',
    N'{item.Price}',
    {Convert.ToInt64(item.MaterialId)},
    N'{item.Down}',
    N'{item.Up}',
    {fseq},
    N'{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
    N'{item.Unit?.Replace("'", "''") ?? ""}'
)
";

                DbHelper.ExecuteNonQuery(insertBodySql);

                WriteLog(
                    ("", 0, 0, 0, 0),
                    (
                        item.Supplier ?? "",
                        Convert.ToInt64(item.MaterialId),
                        item.Down,
                        item.Up,
                        item.Price
                    ),
                    "INSERT");

                fseq++;
            }

            // ==================== 4. 更新_originalData ====================

            originalData.RemoveAll(x =>
                x.Supplier == currentSupplier
                && x.MaterialCode == currentMaterialId);

            originalData.AddRange(newData.Select(item => new RuleDto
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
            }));
        }

        private void WriteLog(
            (string supplier, long materialId, int start, int end, decimal price) oldItem,
            (string supplier, long materialId, int start, int end, decimal price) newItem,
            string action)
        {
            string insertLogSql = "";

            if (action == "DELETE")
            {
                insertLogSql = $@"
INSERT INTO ZMER_T_PRICE_LOG
(FSUPPLIER, FMATERIALID,
 F_OLD_START, F_OLD_END, F_OLD_PRICE,
 F_ACTION, F_CREATE_TIME, F_CREATOR)
VALUES
(N'{oldItem.supplier.Replace("'", "''")}', {oldItem.materialId},
 {oldItem.start}, {oldItem.end}, {oldItem.price},
 N'DELETE', GETDATE(), N'刘军')
";
            }
            else if (action == "INSERT")
            {
                insertLogSql = $@"
INSERT INTO ZMER_T_PRICE_LOG
(FSUPPLIER, FMATERIALID,
 F_NEW_START, F_NEW_END, F_NEW_PRICE,
 F_ACTION, F_CREATE_TIME, F_CREATOR)
VALUES
(N'{newItem.supplier.Replace("'", "''")}', {newItem.materialId},
 {newItem.start}, {newItem.end}, {newItem.price},
 N'INSERT', GETDATE(), N'刘军')
";
            }

            DbHelper.ExecuteNonQuery(insertLogSql);
        }

        /// <summary>
        /// 生成新的单据头FID（时间戳+序号，确保唯一）
        /// </summary>
        private long GenerateFid()
        {
            return Convert.ToInt64(DbHelper.ExecuteScalar(
                "SELECT ISNULL(MAX(FID), 0) + 1 FROM ZMER_t_Cust100025"));
        }

        /// <summary>
        /// 生成新的单据体FEntryID
        /// </summary>
        private long GenerateEntryId()
        {
            return Convert.ToInt64(DbHelper.ExecuteScalar(
                "SELECT ISNULL(MAX(FEntryID), 0) + 1 FROM ZMER_t_Cust_Entry100101"));
        }

        /// <summary>
        /// 生成单据编号
        /// 格式：MSDD + 序号（如 MSDD95 → MSDD96 → MSDD97 ...）
        /// </summary>
        private string GenerateBillNo()
        {
            // 查询当前最大的单据编号
            string sql = "SELECT ISNULL(MAX(FBILLNO), '') FROM ZMER_t_Cust100025";
            object result = DbHelper.ExecuteScalar(sql);
            string maxBillNo = result?.ToString() ?? "";

            // 如果没有数据，从 MSDD1 开始
            if (string.IsNullOrEmpty(maxBillNo))
            {
                return "MSDD1";
            }

            // 分离前缀和数字
            // 从字符串末尾开始找数字
            string prefix = "";
            string numberStr = "";

            for (int i = maxBillNo.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(maxBillNo[i]))
                {
                    numberStr = maxBillNo[i] + numberStr;
                }
                else
                {
                    prefix = maxBillNo.Substring(0, i + 1);
                    break;
                }
            }

            // 解析数字并 +1
            if (int.TryParse(numberStr, out int currentNumber))
            {
                int newNumber = currentNumber + 1;
                // 保持原来的数字位数
                string newNumberStr = newNumber.ToString().PadLeft(numberStr.Length, '0');
                return prefix + newNumberStr;
            }

            // 如果解析失败（比如全是字母），默认加序号1
            return maxBillNo + "1";
        }

        /// <summary>
        /// 检查当前页面数据是否已经存在数据库
        /// 条件：供应商 + 物料编码 + 上限 + 含税价格
        /// </summary>
        public bool CheckDataAlreadyExists(List<RuleDto> data)
        {
            if (data == null || data.Count == 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                string sql = $@"
SELECT COUNT(1)
FROM ZMER_t_Cust_Entry100101 A
INNER JOIN T_BD_MATERIAL B ON A.FMATERIALID = B.FMATERIALID
WHERE A.FSUPPLIER = N'{item.Supplier.Replace("'", "''")}'
AND B.FNUMBER = N'{item.MaterialCode.Replace("'", "''")}'
AND A.F_ZMER_TEXT_83G = N'{item.Up}'
AND A.FTAXPRICE = N'{item.Price}'
";

                object result = DbHelper.ExecuteScalar(sql);
                int count = Convert.ToInt32(result);

                if (count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查历史数据是否存在（弱提醒）
        /// 用生成的完整区间（Down + Up）去数据库查询是否存在历史数据
        /// </summary>
        /// <param name="data">已经生成好下限的数据</param>
        public List<string> GetMissingHistoryData(List<RuleDto> data)
        {
            var missingDetails = new List<string>();

            if (data == null || data.Count == 0)
            {
                return missingDetails;
            }

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                try
                {
                    string checkSql = $@"
SELECT COUNT(1)
FROM ZMER_t_Cust_Entry100101
WHERE FSUPPLIER = N'{item.Supplier.Replace("'", "''")}'
AND FMATERIALID = {Convert.ToInt64(item.MaterialId)}
AND F_ZMER_TEXT_QTR = N'{item.Down}'
AND F_ZMER_TEXT_83G = N'{item.Up}'
";

                    object result = DbHelper.ExecuteScalar(checkSql);
                    int count = Convert.ToInt32(result);

                    if (count == 0)
                    {
                        string materialInfo = string.IsNullOrWhiteSpace(item.MaterialName)
                            ? item.MaterialId
                            : $"{item.MaterialId}({item.MaterialName})";

                        missingDetails.Add(
                            $"第{i + 1}行：供应商【{item.Supplier}】 物料【{materialInfo}】 数量【{item.Up}】"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"查询历史数据失败：{ex.Message}");
                }
            }

            return missingDetails;
        }

        
    }
}
