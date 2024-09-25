using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PageConfig.WebApi.utils
{
    public static class ResultTools
    {
        #region 根据原pageconfig.sideapi规则定义返回值
        private static string successCode = "200";
        private static string successMessage = "success";

        private static string errorCode = "4000";
        private static string errorMessage = "error";
        #endregion

        /// <summary>
        /// 返回成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static JObject successResult(JObject data)
        {
            JObject result = new JObject();
            result.Add("data",data);
            result.Add("code", successCode);
            result.Add("message", successMessage);
            return result;
        }

        /// <summary>
        /// 返回失败
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JObject errorResult(string message)
        {
            JObject result = new JObject();
            result.Add("code", errorCode);
            result.Add("message", !string.IsNullOrEmpty(message)? message : errorMessage);
            return result;
        }
    }
}
