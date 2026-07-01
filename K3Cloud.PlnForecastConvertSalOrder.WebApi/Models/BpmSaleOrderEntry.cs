using Newtonsoft.Json;
using System.Collections.Generic;

namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// 销售订单分录
    /// </summary>
    public class BpmSaleOrderEntry
    {
        [JsonProperty("FMaterialId")]
        public FNumberField FMaterialId { get; set; }

        [JsonProperty("FUnitID")]
        public FNumberField FUnitId { get; set; }

        [JsonProperty("FPriceUnitId")]
        public FNumberField FPriceUnitId { get; set; }

        [JsonProperty("FSettleOrgIds")]
        public FNumberField FSettleOrgIds { get; set; }

        [JsonProperty("FIsFree")]
        public bool FIsFree { get; set; }

        [JsonProperty("F_PAEZ_SFPJ")]
        public bool F_PAEZ_SFPJ { get; set; }

        [JsonProperty("FStockOrgId")]
        public FNumberField FStockOrgId { get; set; }

        [JsonProperty("FSOURCETYPE")]
        public string FSourceType { get; set; }

        [JsonProperty("FOutLmtUnitID")]
        public FNumberField FOutLmtUnitId { get; set; }

        [JsonProperty("FDeliveryDate")]
        public string FDeliveryDate { get; set; }

        [JsonProperty("F_PAEZ_YHDATE")]
        public string F_PAEZ_YHDATE { get; set; }

        [JsonProperty("FQty")]
        public decimal FQty { get; set; }

        [JsonProperty("FPrice")]
        public decimal FPrice { get; set; }

        [JsonProperty("FEntryTaxAmount")]
        public decimal FEntryTaxAmount { get; set; }

        [JsonProperty("FTaxPrice")]
        public decimal FTaxPrice { get; set; }

        [JsonProperty("F_PAEZ_HTBH")]
        public string F_PAEZ_HTBH { get; set; }

        [JsonProperty("FCRMHTNO")]
        public string FCRMHTNO { get; set; }

        [JsonProperty("FCRMHTID")]
        public string FCRMHTID { get; set; }

        [JsonProperty("FRowId")]
        public string FRowId { get; set; }

        [JsonProperty("F_PAEZ_BOMJC")]
        public string F_PAEZ_BOMJC { get; set; }

        [JsonProperty("FEntryTaxRate")]
        public decimal FEntryTaxRate { get; set; }

        [JsonProperty("F_PAEZ_CCPES")]
        public bool F_PAEZ_CCPES { get; set; }

        [JsonProperty("F_PAEZ_SCCJ")]
        public FNumberField F_PAEZ_SCCJ { get; set; }

        [JsonProperty("F_SYB")]
        public string F_SYB { get; set; }

        [JsonProperty("FCRMSYDJH")]
        public string FCRMSYDJH { get; set; }

        [JsonProperty("FCRMSYDJID")]
        public string FCRMSYDJID { get; set; }

        [JsonProperty("FCRMSYDJMXID")]
        public string FCRMSYDJMXID { get; set; }

        [JsonProperty("FCRMDJH")]
        public string FCRMDJH { get; set; }

        [JsonProperty("FCRMDJID")]
        public string FCRMDJID { get; set; }

        [JsonProperty("FCRMDJMXID")]
        public string FCRMDJMXID { get; set; }

        [JsonProperty("FSaleOrderEntry_Link")]
        public List<BpmSaleOrderEntryLink> FSaleOrderEntry_Link { get; set; }

        [JsonProperty("FORDERYXZZ")]
        public List<BpmOrderYxzz> FORDERYXZZ { get; set; }
    }
}
