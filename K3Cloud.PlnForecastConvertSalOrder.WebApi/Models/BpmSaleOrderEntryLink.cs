using Newtonsoft.Json;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// 销售订单分录关联信息（预测单下推关系）
    /// </summary>
    public class BpmSaleOrderEntryLink
    {
        [JsonProperty("FSaleOrderEntry_Link_FSBillId")]
        public string FSaleOrderEntry_Link_FSBillId { get; set; }

        [JsonProperty("FSaleOrderEntry_Link_FRuleId")]
        public string FSaleOrderEntry_Link_FRuleId { get; set; }

        [JsonProperty("FSaleOrderEntry_Link_FSTableId")]
        public string FSaleOrderEntry_Link_FSTableId { get; set; }

        [JsonProperty("FSaleOrderEntry_Link_FSTableName")]
        public string FSaleOrderEntry_Link_FSTableName { get; set; }

        [JsonProperty("FSaleOrderEntry_Link_FSId")]
        public string FSaleOrderEntry_Link_FSId { get; set; }
    }
}
