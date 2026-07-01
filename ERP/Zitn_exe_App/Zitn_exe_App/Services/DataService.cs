using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Zitn_exe_App.Models;

namespace Zitn_exe_App.Services
{
    public class DataService
    {
        /// <summary>
        /// 从金蝶参数解析数据
        /// </summary>
        //public List<RuleDto> ParseFromParam(string param)
        //{
        //    if (string.IsNullOrEmpty(param))
        //        return new List<RuleDto>();

        //    try
        //    {
        //        // JSON → List
        //        var list = JsonConvert.DeserializeObject<List<RuleDto>>(param);

        //        return list ?? new List<RuleDto>();
        //    }
        //    catch
        //    {
        //        throw new Exception("参数解析失败");
        //    }
        //}


        public List<RuleDto> ParseFromParam(string param)
        {
            if (string.IsNullOrEmpty(param))
                return new List<RuleDto>();

            try
            {
                // Base64 → JSON
                string json = Encoding.UTF8.GetString(
                    Convert.FromBase64String(param)
                );

                // JSON → List
                var list = JsonConvert.DeserializeObject<List<RuleDto>>(json);

                return list ?? new List<RuleDto>();
            }
            catch
            {
                throw new Exception("参数解析失败");
            }
        }
    }
}
