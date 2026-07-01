using Newtonsoft.Json;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// 销售订单-财务信息
    /// </summary>
    public class BpmSaleOrderFinance
    {
        [JsonProperty("fAllDisCount")]
        public decimal FAllDisCount { get; set; }

        [JsonProperty("fEntryId")]
        public int FEntryId { get; set; }

        [JsonProperty("fExchangeRate")]
        public decimal FExchangeRate { get; set; }

        [JsonProperty("fMargin")]
        public decimal FMargin { get; set; }

        [JsonProperty("fMarginLevel")]
        public decimal FMarginLevel { get; set; }

        [JsonProperty("fSettleCurrId")]
        public FNumberField FSettleCurrId { get; set; }

        [JsonProperty("fXPKID_F")]
        public int FXPKID_F { get; set; }
    }
}
