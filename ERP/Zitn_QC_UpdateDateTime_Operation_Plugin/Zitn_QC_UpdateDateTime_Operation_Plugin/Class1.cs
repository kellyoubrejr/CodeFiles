using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Zitn_QC_UpdateDateTime_Operation_Plugin
{
    [Description("【服务operation】:检验单提交，更新之间开始日期为创建日期"), HotUpdate]
    public class Class1 : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            foreach(DynamicObject obj in e.DataEntitys)
            {
                string billno = Convert.ToString(obj["BillNo"]);
                DateTime createdate = Convert.ToDateTime(obj["CreateDate"]);

                if (string.IsNullOrEmpty(billno) || createdate == DateTime.MinValue)
                {
                    return;
                }

                string sql = string.Format($@"/*dialect*/UPDATE B SET B.FINSPECTSTARTDATE = '{createdate}'
                FROM T_QM_INSPECTBILL A JOIN T_QM_INSPECTBILLENTRY B ON A.FID = B.FID 
                WHERE FBILLNO = '{billno}'");
                Kingdee.BOS.App.Data.DBUtils.Execute(this.Context, sql);
            }
        }
    }
}
