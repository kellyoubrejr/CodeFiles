using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kingdee.BOS.PlugInTest
{
    /// <summary>
    /// 【列表插件】二开连续套打所选分录
    /// </summary>
    [Description("【列表插件】二开连续套打所选分录"), HotUpdate]
    public class BatchSelectedEntityPrintListPlugIn : AbstractListPlugIn
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        private const string TemplateId = "db4c10c2-6b69-4802-8e4a-be4c8e4840c4";

        /// <summary>
        /// 重写菜单点击事件
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);

            if (e.BarItemKey.EqualsIgnoreCase("tbBatchSelectedEntityPrint"))
            {
                // 获取选中行
                ListSelectedRowCollection selectedRows = this.ListView.SelectedRowsInfo;
                if (selectedRows == null || selectedRows.Count == 0)
                    return;

                // 获取列表过滤选择的实体标识
                string headEntityKey = string.Empty;
                string entryEntityKey = string.Empty;
                string subEntryEntityKey = string.Empty;

                List<FilterEntity> selectEntitnes = this.ListView.Model.FilterParameter.SelectedEntities;
                var headEntity = selectEntitnes.FirstOrDefault(x => x.EntityType == Core.Enums.BOSEnums.Enum_EntityType.Header);
                if (headEntity != null)
                {
                    headEntityKey = headEntity.Key;
                }
                var entryEntity = selectEntitnes.FirstOrDefault(x => x.EntityType == Core.Enums.BOSEnums.Enum_EntityType.Entity);
                if (entryEntity != null)
                {
                    entryEntityKey = entryEntity.Key;
                }
                var subEntryEntity = selectEntitnes.FirstOrDefault(x => x.EntityType == Core.Enums.BOSEnums.Enum_EntityType.SubEntity);
                if (subEntryEntity != null)
                {
                    subEntryEntityKey = subEntryEntity.Key;
                }

                // 模板获取可以通过GetTemplatesByFormId方法获取
                // 字典格式,key-套打模板ID,value-套打模板对应语言环境下的名称
                // var templateDic = PrintServiceHelper.GetTemplatesByFormId(this.Context, this.View.BillBusinessInfo.GetForm().Id);

                // 构建打印任务
                List<PrintJob> printJobs = new List<PrintJob>();
                PrintJob pJob = new PrintJob();
                pJob.Id = Guid.NewGuid().ToString();
                pJob.FormId = this.View.BillBusinessInfo.GetForm().Id;

                List<PrintJobItem> printJobsItemList = new List<PrintJobItem>();
                foreach (ListSelectedRow selectedRow in selectedRows)
                {
                    var model = printJobsItemList.FirstOrDefault(x => x.BillId.Equals(selectedRow.PrimaryKeyValue));
                    if (model == null)
                    {
                        model = new PrintJobItem(selectedRow.PrimaryKeyValue, TemplateId);
                        // 老版本选中分录集合
                        model.SelectedEtyIds = new List<string>();
                        // 新版本树形选中分录集合
                        model.RootNode = new PrintSelectNode();
                        // 单据头
                        model.RootNode.EntityKey = headEntityKey;
                        model.RootNode.PrimaryKeyValue = selectedRow.PrimaryKeyValue;
                        // 单据体
                        model.RootNode.SelectNode = new List<PrintSelectNode>();

                        if (string.IsNullOrWhiteSpace(selectedRow.EntryPrimaryKeyValue) == false)
                        {
                            // 老版本
                            model.SelectedEtyIds.Add(selectedRow.EntryPrimaryKeyValue);
                            // 新版本
                            PrintSelectNode node = new PrintSelectNode();
                            node.EntityKey = entryEntityKey;
                            node.PrimaryKeyValue = selectedRow.EntryPrimaryKeyValue;
                            model.RootNode.SelectNode.Add(node);
                        }

                        printJobsItemList.Add(model);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(selectedRow.EntryPrimaryKeyValue) == false)
                        {
                            // 老版本
                            if (model.SelectedEtyIds.Any(x => x.EqualsIgnoreCase(selectedRow.EntryPrimaryKeyValue)) == false)
                            {
                                model.SelectedEtyIds.Add(selectedRow.EntryPrimaryKeyValue);
                            }
                            // 新版本
                            if (model.RootNode.SelectNode.Any(x => x.PrimaryKeyValue.EqualsIgnoreCase(selectedRow.EntryPrimaryKeyValue)) == false)
                            {
                                PrintSelectNode node = new PrintSelectNode();
                                node.EntityKey = entryEntityKey;
                                node.PrimaryKeyValue = selectedRow.EntryPrimaryKeyValue;
                                model.RootNode.SelectNode.Add(node);
                            }
                        }
                    }
                }

                pJob.PrintJobItems = printJobsItemList;
                printJobs.Add(pJob);

                this.DoAction(printJobs);
            }
        }

        /// <summary>
        /// 处理操作
        /// </summary>
        /// <param name="printJobs"></param>
        private void DoAction(List<PrintJob> printJobs)
        {
            if (printJobs == null || printJobs.Count == 0)
                return;

            // 放到当前视图缓存中
            string printKey = Guid.NewGuid().ToString();
            this.View.Session[printKey] = printJobs;

            // 构建发送前端的命令
            JSONObject josnObj = new JSONObject();
            josnObj.Put("pageID", this.View.PageId);
            josnObj.Put("printJobId", printKey);
            // 打印类型有preview print PrintMergePreview PrintMerge
            josnObj.Put("action", "preview");

            // 打印JSAction有printPreview print
            this.View.AddAction(JSAction.printPreview, josnObj);
        }
    }
}