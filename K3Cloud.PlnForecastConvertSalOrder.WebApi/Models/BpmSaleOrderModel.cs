using Newtonsoft.Json;
using System.Collections.Generic;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// 销售订单Model
    /// </summary>
    public class BpmSaleOrderModel
    {
        [JsonProperty("fBillTypeID")]
        public FNumberField FBillTypeID { get; set; }

        [JsonProperty("fContractId")]
        public long FContractId { get; set; }

        [JsonProperty("fCustId")]
        public FNumberField FCustId { get; set; }

        [JsonProperty("fDate")]
        public string FDate { get; set; }

        [JsonProperty("fID")]
        public long FID { get; set; }

        [JsonProperty("fKHZT")]
        public FNumberField FKHZT { get; set; }

        [JsonProperty("fNetOrderBillId")]
        public long FNetOrderBillId { get; set; }

        [JsonProperty("fOppID")]
        public long FOppID { get; set; }

        [JsonProperty("fSFZDBD")]
        public string FSFZDBD { get; set; }

        [JsonProperty("fSaleOrderEntry")]
        public List<BpmSaleOrderEntry> FSaleOrderEntry { get; set; }

        [JsonProperty("fSaleOrderFinance")]
        public BpmSaleOrderFinance FSaleOrderFinance { get; set; }

        [JsonProperty("fSaleOrgId")]
        public FNumberField FSaleOrgId { get; set; }

        [JsonProperty("fSalerId")]
        public FNumberField FSalerId { get; set; }

        [JsonProperty("fXPKID_H")]
        public long FXPKID_H { get; set; }

        [JsonProperty("fYWLX")]
        public string FYWLX { get; set; }

        [JsonProperty("f_PAEZ_YSDJH")]
        public string F_PAEZ_YSDJH { get; set; }
    }
}
