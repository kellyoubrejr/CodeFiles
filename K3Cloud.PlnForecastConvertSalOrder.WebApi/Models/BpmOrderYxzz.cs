using Newtonsoft.Json;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// FORDERYXZZ - 预测单转销售订单操作记录
    /// </summary>
    public class BpmOrderYxzz
    {
        [JsonProperty("FmatNam")]
        public string FmatNam { get; set; }

        [JsonProperty("FcrmSrcEntryId")]
        public string FcrmSrcEntryId { get; set; }

        [JsonProperty("FmatNum")]
        public string FmatNum { get; set; }

        [JsonProperty("FoptUserNam")]
        public string FoptUserNam { get; set; }

        [JsonProperty("FtransferQty")]
        public decimal FtransferQty { get; set; }

        [JsonProperty("FcrmOrderEntryId")]
        public string FcrmOrderEntryId { get; set; }

        [JsonProperty("FerpSrcNum")]
        public string FerpSrcNum { get; set; }

        [JsonProperty("FoptUserId")]
        public string FoptUserId { get; set; }

        [JsonProperty("FcrmSrcNum")]
        public string FcrmSrcNum { get; set; }

        [JsonProperty("FoptTime")]
        public string FoptTime { get; set; }

        [JsonProperty("FcrmOrderNum")]
        public string FcrmOrderNum { get; set; }

        [JsonProperty("FmatModel")]
        public string FmatModel { get; set; }

        [JsonProperty("FerpSrcId")]
        public string FerpSrcId { get; set; }

        [JsonProperty("FcrmSrcId")]
        public string FcrmSrcId { get; set; }

        [JsonProperty("FerpSrcEntryId")]
        public string FerpSrcEntryId { get; set; }
    }
}
