using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zitn_exe_App.Models
{
    public class RuleDto
    {
        [JsonProperty("MaterialId")]
        public string MaterialId { get; set; }
        [JsonProperty("MaterialCode")]
        public string MaterialCode { get; set; }
        [JsonProperty("MaterialName")]
        public string MaterialName { get; set; }
        [JsonProperty("MaterialSpec")]
        public string MaterialSpec { get; set; }
        [JsonProperty("Supplier")]
        public string Supplier { get; set; }
        [JsonProperty("Down")]
        public int Down { get; set; }
        [JsonProperty("Up")]
        public int Up { get; set; }
        [JsonProperty("Price")]
        public decimal Price { get; set; }
        [JsonProperty("Unit")]
        public string Unit { get; set; }
        [JsonProperty("Source")]
        public string Source { get; set; }

        [JsonProperty("BillNo")]
        public string BillNo { get; set; }

        [JsonProperty("Qty")]
        public int Qty { get; set; }

        [JsonProperty("FentryId")]
        public string FentryId { get; set; }

        [JsonProperty("GuidSN")]
        public string GuidSN { get; set; }=Guid.NewGuid().ToString();

        /// <summary>
        /// 网格行号（仅用于校验报错定位，不序列化）
        /// </summary>
        [JsonIgnore]
        public int RowIndex { get; set; }
    }
}
