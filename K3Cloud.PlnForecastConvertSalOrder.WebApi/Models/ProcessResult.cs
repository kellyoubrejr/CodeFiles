namespace K3Cloud.PlnForecastConvertSalOrder.WebApi.Models
{
    /// <summary>
    /// 处理结果
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 状态码：200成功，400参数错误，401未登录，500异常
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 销售订单编码 FBillNo
        /// </summary>
        public string FBillNo { get; set; }

        /// <summary>
        /// 销售订单内码 FID
        /// </summary>
        public int FId { get; set; }

        /// <summary>
        /// 销售订单分录内码 FEntryID
        /// </summary>
        public int FEntryId { get; set; }
    }
}
