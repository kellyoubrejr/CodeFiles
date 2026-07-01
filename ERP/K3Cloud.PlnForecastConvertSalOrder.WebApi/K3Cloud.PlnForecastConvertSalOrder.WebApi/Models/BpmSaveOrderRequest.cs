using Newtonsoft.Json;
using System.Collections.Generic;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// BPM保存销售订单请求（接收JSON入参）
    /// </summary>
    public class BpmSaveOrderRequest
    {
        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isEntryBatchFill")]
        public string IsEntryBatchFill { get; set; }

        [JsonProperty("model")]
        public BpmSaleOrderModel Model { get; set; }

        [JsonProperty("needReturnFields")]
        public List<string> NeedReturnFields { get; set; }

        [JsonProperty("needUpDateFields")]
        public List<string> NeedUpDateFields { get; set; }

        [JsonProperty("sequenceNo")]
        public string SequenceNo { get; set; }

        [JsonProperty("subSystemId")]
        public string SubSystemId { get; set; }
    }
}
