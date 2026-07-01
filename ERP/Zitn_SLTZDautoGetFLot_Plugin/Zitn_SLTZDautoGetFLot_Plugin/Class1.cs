using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.K3.Core.SCM.STK;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace Zitn_SLTZDautoGetFLot_Plugin
{
    [Description("【服务插件】收料通知单保存提交，自动获取批号")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Class1 : AbstractOperationServicePlugIn
    {
        public override void BeforeDoSaveExecute(BeforeDoSaveExecuteEventArgs e)
        {
            base.BeforeDoSaveExecute(e);

            // 获取对应批号字段
            LotField lotField = this.BusinessInfo.GetField("Plot") as LotField;

            // 如果批号字段不存在或者没有配置物料和库存组织字段则不处理
            if (lotField == null || string.IsNullOrWhiteSpace(lotField.OrgFieldKey)
                || string.IsNullOrWhiteSpace(lotField.ControlFieldKey)
                || lotField.InputModel != LotField.Enum_InputModel.TextAndSelect)
            {
                return;
            }

            ExtendedDataEntitySet extEntitySet = new ExtendedDataEntitySet();
            extEntitySet.Parse(e.DataEntities, this.BusinessInfo);
            ExtendedDataEntity[] entities = extEntitySet.FindByEntityKey(lotField.EntityKey);

            // 调用服务获取主档
            ILogService lotService = SCM.Contracts.ServiceFactory.GetLotService(this.Context);
            CodeAppResult codeRet = lotService.GenerateLotMasterByCodeRule(this.Context, this.BusinessInfo, lotField, entities);
            List<KeyValuePair<int, DynamicObject>> lotLists = codeRet.CodeResults;

            if (lotLists == null || lotLists.Count < 1)
            {
                return;
            }

            // 回填单据
            int index = 0;
            foreach (ExtendedDataEntity entity in entities)
            {
                KeyValuePair<int, DynamicObject> lotPair = lotLists.SingleOrDefault(p => p.Key == index);
                if (lotPair.Value != null && lotPair.Value.Count > 0)
                {
                    DynamicObject lot = lotPair.Value[0];
                    if (lot != null)
                    {
                        if (Convert.ToInt64(lot["Id"]) > 0)
                        {
                            entity.DataEntity["Lot"] = lot;
                        }
                        else
                        {
                            entity.DataEntity["Lot_Text"] = lot["Number"];
                        }
                    }
                    else
                    {
                        entity.DataEntity["Lot"] = null;
                        entity.DataEntity["Lot_Text"] = null;
                    }
                }
                index++;
            }
        }
    }
}
