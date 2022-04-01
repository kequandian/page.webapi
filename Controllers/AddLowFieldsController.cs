using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PageConfig.WebApi.Controllers.ApiHandle;
using PageConfig.WebApi.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace PageConfig.WebApi.Controllers
{
    [ApiController]
    public class AddLowFieldsController : ControllerBase
    {

        private static ApiTools tool = new ApiTools();

        private string endpoint = "";
        private int pageId = 0;
        private string token = "";
        //private string originUrl = "";


        private readonly ILogger<OriginDataController> _logger;

        public AddLowFieldsController(ILogger<OriginDataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取原始数据并创建页面
        /// </summary>
        /// <param name="requestData"></param>
        [Route("/addLowFields")]
        [HttpPost]
        public HttpResponseMessage AddLowFields(dynamic requestData)
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(Convert.ToString(requestData));
                string jsonString = JsonConvert.SerializeObject(obj);
                JObject jsonData = (JObject)JsonConvert.DeserializeObject(jsonString);

                if (jsonData["endpoint"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 endpoint 参数", "Error");
                }

                if (jsonData["originApi"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 originApi 参数", "Error");
                }

                if (jsonData["pageId"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 pageId 参数", "Error");
                }

                //if (jsonData["token"] == null)
                //{
                //    return tool.MsgFormat(ResponseCode.操作失败, "缺少 token 参数", "Error");
                //}

                endpoint = jsonData["endpoint"].ToString();
                //originUrl = string.Format("{0}{1}", endpoint, jsonData["originApi"].ToString());
                pageId = int.Parse(jsonData["pageId"].ToString());
                token = jsonData["token"] != null ? jsonData["token"].ToString() : "";

                JObject getOriginApiData = getOriginApi(endpoint, pageId, token);
                var getOriginApiStatus = getOriginApiData["code"] != null ? Convert.ToInt32(getOriginApiData["code"]) : 0;
                if (getOriginApiStatus == 200)
                {
                    if (!getOriginApiData.ContainsKey("data"))
                    {
                        return tool.MsgFormat(ResponseCode.操作失败, "获取field 列表 api 失败", "Error");
                    }
                    string originUrl = string.Format("{0}{1}", endpoint, getOriginApiData["data"]["apiEndpoint"].ToString());
                    JObject resJO = getOriginData(originUrl, token);

                    var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                    if (status == 200)
                    {
                        const string resultinfo_key = "data";
                        if (resJO.ContainsKey(resultinfo_key))
                        {
                            JObject dataJO = (JObject)resJO[resultinfo_key];

                            if (dataJO.ContainsKey("records"))
                            {
                                JArray recordsJA = (JArray)dataJO["records"];
                                if (recordsJA != null && recordsJA.Count > 0)
                                {
                                    JObject originFieldsJO = (JObject)recordsJA[0];
                                    int errCount = 0; //添加字段异常计算
                                    string fieldName = "";
                                    foreach (var item in originFieldsJO)
                                    {
                                        fieldName = item.Key;
                                        var fieldValue = item.Value;
                                        int pId = pageId;
                                        if (!fieldName.Equals("id"))
                                        {
                                            int addStatus = addPageField(endpoint, fieldName, pId, token);
                                            if (addStatus == 1)
                                            {
                                                errCount++;
                                            }
                                            if (errCount > 0)
                                            {
                                                return tool.MsgFormat(ResponseCode.操作失败, string.Format("添加 {0} 字段失败", fieldName), "Error");
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    return tool.MsgFormat(ResponseCode.操作失败, "records 缺少字段数据", "Error");
                                }
                            }

                            return tool.MsgFormat(ResponseCode.成功, "新增成功", "Success");

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
                else
                {
                    return tool.MsgFormatToJson(ResponseCode.操作失败, getOriginApiData, "Error");
                }
                

            }
            catch (Exception ex)
            {
                return tool.MsgFormat(ResponseCode.操作失败, "新增字段失败", ex.ToString());
            }
        }

        /// <summary>
        /// 获取原始api字符串
        /// </summary>
        /// 
        static JObject getOriginApi(string endpoint, int pageId, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string url = string.Format("{0}/api/crud/lowMainPage/lowMainPages/{1}", endpoint, pageId);
                if (token != null && !token.Equals(""))
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
        /// 添加页面字段
        /// </summary>
        /// 
        static int addPageField(string endpoint, string fieldName, int pageId, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = string.Format("{0}/api/crud/lowFields/lowFieldses", endpoint);

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("listColumnMultiKeys", ""); 
                postJO.Add("fieldBinding", fieldName);
                postJO.Add("listColumnWidth", 0);
                postJO.Add("listColumnKey", "");
                postJO.Add("fieldValueOptions", "");
                postJO.Add("formInputType", "input");
                postJO.Add("formViewType", "plain");
                postJO.Add("listFontSize", 0);
                postJO.Add("fieldScopes", "page,table,edit,add,view");
                postJO.Add("formFieldQuestion", "");
                postJO.Add("fieldItemName", fieldName);
                postJO.Add("fieldValueFilter", "");
                postJO.Add("formFieldHint", "");
                postJO.Add("listFontColor", "");
                postJO.Add("fieldLabel", fieldName);
                postJO.Add("formFieldTips", "");
                postJO.Add("listColumnLayout", "");
                postJO.Add("formFieldTitle", fieldName);
                postJO.Add("formViewOptions", "");
                postJO.Add("listColumnOptions", "");
                postJO.Add("pageId", pageId);
                postJO.Add("listFontWeight", "");
                postJO.Add("listColumnFormat", "");
                postJO.Add("listColumnAlign", "left");
                postJO.Add("listColumnName", fieldName);
                postJO.Add("formInputOptions", "");
                postJO.Add("formInputRequired", 0);
                postJO.Add("listColumnType", "plain");
                postJO.Add("listColumnReference", "");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
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