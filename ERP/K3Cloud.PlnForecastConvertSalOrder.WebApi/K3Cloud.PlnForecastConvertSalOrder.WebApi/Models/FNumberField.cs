using Newtonsoft.Json;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// K3基础资料字段，如 { "fnumber": "101" }
    /// 部分字段如fBillTypeID使用大写fNUMBER
    /// </summary>
    public class FNumberField
    {
        [JsonProperty("fnumber")]
        public string FNumber { get; set; }

        [JsonProperty("fNUMBER")]
        public string FNUMBER { get; set; }
    }
}
