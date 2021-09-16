using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace PageConfig.WebApi.utils
{
    public class ApiTools
    {
        private string msgModel = "{{\"code\":{0},\"message\":\"{1}\",\"result\":{2}}}";
        private string msgModelToJson = "{{\"code\":{0},\"data\":{1},\"message\":{2}}}";
        public ApiTools()
        {
        }
        public HttpResponseMessage MsgFormat(ResponseCode code, string explanation, string result)
        {
            string r = @"^(\-|\+)?\d+(\.\d+)?$";
            string json = string.Empty;
            if (Regex.IsMatch(result, r) || result.ToLower() == "true" || result.ToLower() == "false" || result == "[]" || result.Contains('{'))
            {
                json = string.Format(msgModel, (int)code, explanation, result);
            }
            else
            {
                if (result.Contains('"'))
                {
                    json = string.Format(msgModel, (int)code, explanation, result);
                }
                else
                {
                    json = string.Format(msgModel, (int)code, explanation, "\"" + result + "\"");
                }
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        }

        public HttpResponseMessage MsgFormatToJson(ResponseCode code, dynamic explanation, string result)
        {
            string r = @"^(\-|\+)?\d+(\.\d+)?$";
            string json = string.Empty;
            if (Regex.IsMatch(result, r) || result.ToLower() == "true" || result.ToLower() == "false" || result == "[]" || result.Contains('{'))
            {
                json = string.Format(msgModelToJson, (int)code, explanation, result);
            }
            else
            {
                if (result.Contains('"'))
                {
                    json = string.Format(msgModelToJson, (int)code, explanation, result);
                }
                else
                {
                    json = string.Format(msgModelToJson, (int)code, explanation, "\"" + result + "\"");
                }
            }
            return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        }
    }
}