using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PageConfig.WebApi.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace PageConfig.WebApi.Controllers
{
    [ApiController]
    public class OriginDataController : ControllerBase
    {

        private static ApiTools tool = new ApiTools();


        private readonly ILogger<OriginDataController> _logger;

        public OriginDataController(ILogger<OriginDataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取原始数据并创建页面
        /// </summary>
        /// <param name="requestData"></param>
        [Route("/getdata")]
        [HttpPost]
        public HttpResponseMessage GetData(dynamic requestData)
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(Convert.ToString(requestData));
                string jsonString = JsonConvert.SerializeObject(obj);
                JObject jsonData = (JObject)JsonConvert.DeserializeObject(jsonString);
                
                if(jsonData["path"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 path 参数", "Error");
                }
                string path = jsonData["path"].ToString();
                string token = jsonData["token"] != null ? jsonData["token"].ToString(): "";
                JObject resJO = getOriginData(path,token);
                var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                if (status == 200)
                {
                    const string resultinfo_key = "data";
                    if (resJO.ContainsKey(resultinfo_key))
                    {
                        JObject info = (JObject)resJO[resultinfo_key];
                        int pageId = createPage();
                        if(pageId > 0)
                        {
                            Console.WriteLine("添加字段");
                            //foreach (var item in info)
                            //{
                                //addPageField(item[""].ToString(), pageId);
                            //}
                        }
                        else
                        {
                            return tool.MsgFormat(ResponseCode.操作失败, "创建页面失败, 获取pageId异常", "Error");
                        }
                        return tool.MsgFormatToJson(ResponseCode.成功, info, "Success");

                    }
                    else
                    {
                        return tool.MsgFormatToJson(ResponseCode.操作失败, resJO, "Error");
                    }
                }
                else
                {
                    return tool.MsgFormatToJson(ResponseCode.操作失败, resJO, "Error");
                }

            }
            catch (Exception ex)
            {
                return tool.MsgFormat(ResponseCode.操作失败, "获取信息失败", ex.ToString());
            }
        }

        /// <summary>
        /// 获取原始数据
        /// </summary>
        /// 
        static JObject getOriginData(string url, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                if(token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var task = http.GetAsync(url);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject joRet = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return joRet;

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                //Console.WriteLine(await rep.Content.ReadAsStringAsync());

            }
        }

        /// <summary>
        /// 创建页面获取pageId
        /// </summary>
        /// 
        static int createPage()
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string url = "http://192.168.3.189:7000/api/crud/lowMainPage/lowMainPages";
                string jsonString = "{\"apiEndpoint\":\"string\",\"columnAlign\":\"string\",\"contentItemContainerStyle\":\"string\",\"contentItems\":\"string\",\"contentLayout\":\"string\",\"formAddFields\":\"string\",\"formAddTitle\":\"string\",\"formDefaultContentLayout\":\"string\",\"formDefaultWidth\":0,\"formEditFields\":\"string\",\"formEditTitle\":\"string\",\"formViewFields\":\"string\",\"formViewTitle\":\"string\",\"listFields\":\"string\",\"listOperationFields\":\"string\",\"pageTitle\":\"string\"}";
                HttpContent content = new StringContent(jsonString);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(url, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject resJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                if (status == 200)
                {
                    const string resultinfo_key = "data";
                    if (resJO.ContainsKey(resultinfo_key))
                    {
                        JObject info = (JObject)resJO[resultinfo_key];
                        return int.Parse(info["page"].ToString());
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 添加页面字段
        /// </summary>
        /// 
        static int addPageField(string fieldName, int pageId)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string url = "http://192.168.3.189:7000/api/crud/lowFields/lowFieldses";

                string postContent = "{\"listColumnMultiKeys\": \"null\",\"listColumnMultiType\": \"null\",\"fieldBinding\": " + fieldName  + ", \"listColumnWidth\": 0,\"listColumnKey\": \"null\",\"fieldValueOptions\": \"null\",\"formInputType\": \"input\",\"formViewType\": \"plain\",\"listFontSize\": 0,\"fieldScopes\": \"page,table,edit,add,view\",\"formFieldQuestion\": \"null\",\"fieldItemName\": " + fieldName  + ",\"fieldValueFilter\": \"null\",\"formFieldHint\": \"null\",\"listFontColor\": \"null\",\"fieldLabel\": " + fieldName  + ",\"formFieldTips\": \"null\",\"listColumnLayout\": \"null\",\"formFieldTitle\": " + fieldName  + ",\"formViewOptions\": \"null\",\"listColumnOptions\": \"null\",\"pageId\": " + pageId + ", //需要api生成pageID\"listFontWeight\": \"null\",\"listColumnFormat\": \"null\",\"listColumnAlign\": \"left\", //left, center, right     \"listColumnName\": " + fieldName  + ",\"formInputOptions\": \"null\",\"formInputRequired\": 0,\"listColumnType\": \"plain\",\"listColumnReference\": \"null\" }";
                HttpContent content = new StringContent(postContent);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(url, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject resJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                if (status == 200)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

    }
}