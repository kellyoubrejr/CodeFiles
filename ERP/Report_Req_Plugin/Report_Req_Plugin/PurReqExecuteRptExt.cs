using Kingdee.K3.SCM.App.Purchase.Report;
using System.Text;
using Kingdee.BOS.Util;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using System;
using Kingdee.BOS.Orm.DataEntity;

namespace Report_Req_Plugin
{
    public class PurReqExecuteRptExt : PurReqExecuteRpt
    {
        private string[] _customTempTableNames;

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            IDBService dbService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
            _customTempTableNames = dbService.CreateTemporaryTableName(this.Context, 1);

            string tempTable = _customTempTableNames[0];

            base.BuilderReportSqlAndTempTable(filter, tempTable);

            DynamicObject customFilter = filter.FilterParameter.CustomFilter;

            string note = "";

            if (customFilter != null)
            {
                note = Convert.ToString(customFilter["FNOTE"]);
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SELECT T1.*, TCH.FNOTE");
            sb.AppendFormat(" INTO {0} ", tableName);
            sb.AppendFormat(" FROM {0} T1 ", tempTable);

            sb.AppendLine(" LEFT JOIN T_PUR_REQENTRY TCT ON T1.FREQID = TCT.FENTRYID");
            sb.AppendLine(" LEFT JOIN T_PUR_REQUISITION TCH ON TCT.FID = TCH.FID");

            sb.AppendLine(" WHERE 1=1 ");

            if (!string.IsNullOrWhiteSpace(note))
            {
                note = note.Replace("'", "''");

                sb.AppendFormat(" AND TCH.FNOTE LIKE '%{0}%'", note);
            }

            DBUtils.Execute(this.Context, sb.ToString());
        }



        public override void CloseReport()
        {
            if (!_customTempTableNames.IsNullOrEmptyOrWhiteSpace())
            {
                IDBService dbService = Kingdee.BOS.App.ServiceHelper.GetService<IDBService>();
                dbService.DeleteTemporaryTableName(this.Context, _customTempTableNames);
            }
            base.CloseReport();
        }

    }
}