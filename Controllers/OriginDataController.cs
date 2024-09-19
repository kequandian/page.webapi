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
    [Route("pageconfig")]
    public class OriginDataController : ControllerBase
    {

        private static ApiTools tool = new ApiTools();

        private string endpoint = "";
        private string originUrl = "";
        private string createPageUrl = "";
        private string addFieldUrl = "";
        private int pageId = 0;


        private readonly ILogger<OriginDataController> _logger;

        public OriginDataController(ILogger<OriginDataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取原始数据并创建页面
        /// </summary>
        /// <param name="requestData"></param>
        [HttpPost("createpage")]
        public HttpResponseMessage CreatePage(dynamic requestData)
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

                if (jsonData["createPageApi"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 createPageApi 参数", "Error");
                }

                if (jsonData["addFieldApi"] == null)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "缺少 addFieldApi 参数", "Error");
                }

                endpoint = jsonData["endpoint"].ToString();
                originUrl = string.Format("{0}{1}", endpoint, jsonData["originApi"].ToString()); 
                createPageUrl = string.Format("{0}{1}", endpoint, jsonData["createPageApi"]["api"].ToString()); 
                addFieldUrl = string.Format("{0}{1}", endpoint, jsonData["addFieldApi"].ToString());

                string token = jsonData["token"] != null ? jsonData["token"].ToString(): "";

                JObject resJO = getOriginData(originUrl, token);

                var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                if (status == 200)
                {
                    const string resultinfo_key = "data";
                    if (resJO.ContainsKey(resultinfo_key))
                    {
                        JObject info = (JObject)resJO[resultinfo_key];
                        //创建页面
                        JObject responseCreateJO = createPage(createPageUrl, (JObject)(jsonData["createPageApi"]["data"]), token);
                        var createPageStatus = responseCreateJO["code"] != null ? Convert.ToInt32(responseCreateJO["code"]) : 0;
                        if (createPageStatus == 200)
                        {
                            if (responseCreateJO.ContainsKey(resultinfo_key))
                            {
                                pageId = int.Parse(responseCreateJO[resultinfo_key]["id"].ToString());
                            }

                            //addLowFilterss
                            if (jsonData.ContainsKey("lowFilterss"))
                            {
                                JObject lowFilterssJO = (JObject)jsonData["lowFilterss"];

                                JArray lowFiltersJA = (JArray)lowFilterssJO["filters"];

                                string requestApi = string.Format("{0}{1}", endpoint, lowFilterssJO["api"].ToString());

                                foreach (var filtersItem in lowFiltersJA)
                                {
                                    JObject respLowFiltersJO = addLowFilterss(requestApi, pageId, (JObject)filtersItem, token);
                                    var lowFiltersStatus = respLowFiltersJO["code"] != null ? Convert.ToInt32(respLowFiltersJO["code"]) : 0;
                                    if (lowFiltersStatus != 200)
                                    {
                                        return tool.MsgFormatToJson(ResponseCode.操作失败, string.Format("新增搜索栏异常,  {0}", respLowFiltersJO.ToString()), "Error");
                                    }
                                }

                            }

                            //添加 lowActions 按钮
                            if (jsonData.ContainsKey("lowActions"))
                            {
                                JObject lowActionsJO = (JObject)jsonData["lowActions"];

                                JArray actionListJA = (JArray)lowActionsJO["actions"];

                                string requestApi = string.Format("{0}{1}", endpoint, lowActionsJO["api"].ToString());

                                foreach (var actionItem in actionListJA)
                                {
                                    JObject respLowActionsJO = addLowActions(requestApi, pageId, (JObject)actionItem, token);
                                    var lowActionsStatus = respLowActionsJO["code"] != null ? Convert.ToInt32(respLowActionsJO["code"]) : 0;
                                    if (lowActionsStatus != 200)
                                    {
                                        return tool.MsgFormatToJson(ResponseCode.操作失败, string.Format("新增lowActions异常,  {0}", respLowActionsJO.ToString()), "Error");
                                    }
                                }

                            }

                            //添加 lowOperationses 操作栏 按钮
                            if (jsonData.ContainsKey("lowOperations"))
                            {
                                JObject lowOperationsesJO = (JObject)jsonData["lowOperations"];

                                JArray OperationseListJA = (JArray)lowOperationsesJO["actions"];

                                string requestApi = string.Format("{0}{1}", endpoint, lowOperationsesJO["api"].ToString());

                                foreach (var operationseItem in OperationseListJA)
                                {
                                    JObject respLowOperationseJO = addlowoperationss(requestApi, pageId, (JObject)operationseItem, token);
                                    var lowOperationseStatus = respLowOperationseJO["code"] != null ? Convert.ToInt32(respLowOperationseJO["code"]) : 0;
                                    if (lowOperationseStatus != 200)
                                    {
                                        return tool.MsgFormatToJson(ResponseCode.操作失败, string.Format("新增lowOperationses异常,  {0}", respLowOperationseJO.ToString()), "Error");
                                    }
                                }

                            }


                        }
                        else
                        {
                            return tool.MsgFormatToJson(ResponseCode.操作失败, string.Format("创建页面 API 异常 = {0}", responseCreateJO.ToString()), "Error");
                        }



                        if (pageId > 0)
                        {
                            int errCount = 0; //添加字段异常计算
                            string fieldName = "";
                            foreach (var item in info)
                            {
                                fieldName = item.Key; 
                                var fieldValue = item.Value;
                                int pId = pageId;
                                if (!fieldName.Equals("id"))
                                {
                                    int addStatus = addPageField(addFieldUrl, fieldName, pId, token);
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
                            return tool.MsgFormat(ResponseCode.操作失败, "创建页面失败, 获取pageId异常", "Error");
                        }

                        JObject pageDetailJO = getPageDetail(createPageUrl, pageId, token);
                        //return tool.MsgFormatToJson(ResponseCode.成功, "创建成功", "Success");
                        string convertString =  toConfig(pageDetailJO);

                        return tool.MsgFormatToJson(ResponseCode.成功, convertString, "Success");

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
                return tool.MsgFormat(ResponseCode.操作失败, "创建失败", ex.ToString());
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
        static JObject createPage(string url, JObject pageData, string token)
        {

            string testUrl = "http://192.168.3.239:3333/api/crud/lowMainPage/lowMainPages";

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("apiEndpoint", pageData["apiEndpoint"]);
                postJO.Add("columnAlign", pageData["columnAlign"] != null ? pageData["columnAlign"].ToString() : "");
                postJO.Add("contentItemContainerStyle", pageData["contentItemContainerStyle"] != null ? pageData["contentItemContainerStyle"] : "");
                postJO.Add("contentItems", pageData["contentItems"] != null ? pageData["contentItems"] : "");
                postJO.Add("contentLayout", pageData["contentLayout"] != null ? pageData["contentLayout"] : "Grid");
                postJO.Add("formAddFields", pageData["formAddFields"] != null ? pageData["formAddFields"] : "");
                postJO.Add("formAddTitle", pageData["formAddTitle"] != null ? pageData["formAddTitle"].ToString() : "");
                postJO.Add("formDefaultContentLayout", pageData["formDefaultContentLayout"] != null ? pageData["formDefaultContentLayout"] : "TitleContent");
                postJO.Add("formDefaultWidth", pageData["formDefaultWidth"] != null ? int.Parse(pageData["formDefaultWidth"].ToString()):0);
                postJO.Add("formEditFields", pageData["formEditFields"] != null ? pageData["formEditFields"] : "");
                postJO.Add("formEditTitle", pageData["formEditTitle"] != null ? pageData["formEditTitle"] : "");
                postJO.Add("formViewFields", pageData["formViewFields"] != null ? pageData["formViewFields"] : "");
                postJO.Add("formViewTitle", pageData["formViewTitle"] != null ? pageData["formViewTitle"] : "");
                postJO.Add("listFields", pageData["listFields"] != null ? pageData["listFields"] : "");
                postJO.Add("listOperationFields", pageData["listOperationFields"] != null ? pageData["listOperationFields"] : "");
                postJO.Add("pageTitle", pageData["pageTitle"] != null ? pageData["pageTitle"] : "");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return respJO;

            }
        }


        /// <summary>
        /// 添加 搜索栏
        /// </summary>
        /// 
        static JObject addLowFilterss(string url, int pageId, JObject filtersData, string token)
        {

            string testUrl = "http://192.168.3.239:3333/api/crud/lowFilters/lowFilterses";

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("contentLayout", filtersData["contentLayout"] != null ? filtersData["contentLayout"].ToString() : "Grid");
                postJO.Add("fieldName", filtersData["fieldName"] != null ? filtersData["fieldName"].ToString() : "search");
                postJO.Add("defaultSearchHint", filtersData["defaultSearchHint"] != null ? filtersData["defaultSearchHint"].ToString() : "请输入");
                postJO.Add("searchFields", filtersData["searchFields"] != null ? filtersData["searchFields"].ToString() : "");
                postJO.Add("pageId", pageId);
                postJO.Add("fieldTitle", filtersData["fieldTitle"] != null ? filtersData["fieldTitle"].ToString() : "搜索");
                postJO.Add("fieldType", filtersData["fieldType"] != null ? filtersData["fieldType"].ToString() : "search");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return respJO;

            }
        }

        /// <summary>
        /// 添加lowActionss
        /// </summary>
        /// 
        static JObject addLowActions(string url, int pageId, JObject actionData, string token)
        {

            string testUrl = "http://192.168.3.239:3333/api/crud/lowActions/lowActionses";

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("path", actionData["path"].ToString());
                postJO.Add("requestRefreshApi", "");
                postJO.Add("requestBody", "");
                postJO.Add("requestMethod", "");
                postJO.Add("pageId", pageId);
                postJO.Add("requestOptions", "");
                postJO.Add("title", actionData["title"].ToString());
                postJO.Add("type", actionData["type"] != null ? actionData["type"].ToString():"path");
                postJO.Add("requestApi", "");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return respJO;

            }
        }

        /// <summary>
        /// 添加lowOperationss 操作栏按钮
        /// </summary>
        /// 
        static JObject addlowoperationss(string url, int pageId, JObject actionData, string token)
        {

            string testUrl = "http://192.168.3.239:3333/api/crud/lowOperations/lowOperationses";

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("path", actionData["path"].ToString());
                postJO.Add("outside", actionData["outside"] != null ? int.Parse(actionData["outside"].ToString()) : 0);
                postJO.Add("requestRefreshApi", "");
                postJO.Add("requestBody", "");
                postJO.Add("requestMethod", "");
                postJO.Add("pageId", pageId);
                postJO.Add("requestOptions", "");
                postJO.Add("title", actionData["title"].ToString());
                postJO.Add("type", actionData["type"] != null ? actionData["type"].ToString() : "path");
                postJO.Add("requestApi", "");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return respJO;

            }


        }

        /// <summary>
        /// 添加页面字段
        /// </summary>
        /// 
        static int addPageField(string url, string fieldName, int pageId, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            string testUrl = "http://192.168.3.239:3333/api/crud/lowFields/lowFieldses";

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

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


        /// <summary>
        /// 根据pageId获取页面详情
        /// </summary>
        /// 
        static JObject getPageDetail(string url, int pageId, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            string testUrl = string.Format("http://192.168.3.239:3333/api/crud/lowMainPage/lowMainPages/{0}", pageId);

            using (var http = new HttpClient(handler))
            {
                //string requestUrl = string.Format("{0}/{1}", url, pageId);
                string requestUrl = testUrl;
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var task = http.GetAsync(requestUrl);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject resJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = resJO["code"] != null ? Convert.ToInt32(resJO["code"]) : 0;
                if (status == 200)
                {
                    const string resultinfo_key = "data";
                    if (resJO.ContainsKey(resultinfo_key))
                    {
                        return (JObject)resJO[resultinfo_key];
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 转换成crud-setting json
        /// </summary>
        /// <param name="pageJson"></param>
        private string toConfig(dynamic pageJson)
        {
            PageConfigHandle handle = new PageConfigHandle();
            try
            {
                JObject bodyContent = new JObject();
                bodyContent.Add("columns", 1);
                JObject pageNameJO = new JObject(); //页面标题
                JArray createFieldsJO = new JArray(); //新增页面配置
                JArray updateFieldsJO = new JArray(); //编辑页面配置
                JObject layoutJO = new JObject();

                JArray tableFieldsJO = new JArray();//列表显示字段
                JArray tableActionsJO = new JArray(); //列表页--列表上方按钮
                JArray tableOperationJO = new JArray();//列表页--列表项操作按钮
                JArray searchFieldsJO = new JArray();//搜索栏
                JArray viewConfigJO = new JArray();//详情

                dynamic obj = JsonConvert.DeserializeObject(Convert.ToString(pageJson));
                //Console.WriteLine("obj = {0}", obj);
                string jsonString = JsonConvert.SerializeObject(obj);
                JObject jsonData = (JObject)JsonConvert.DeserializeObject(jsonString);

                //标题
                pageNameJO.Add("table", jsonData["pageTitle"].ToString());
                pageNameJO.Add("new", jsonData["formAddTitle"].ToString());
                pageNameJO.Add("edit", jsonData["formEditTitle"].ToString());

                string apiEndpoint = jsonData["apiEndpoint"].ToString();
                //访问api
                bodyContent.Add("listAPI", apiEndpoint);
                bodyContent.Add("createAPI", apiEndpoint);
                bodyContent.Add("getAPI", string.Format("{0}/[id]", apiEndpoint));
                bodyContent.Add("updateAPI", string.Format("{0}/[id]", apiEndpoint));
                bodyContent.Add("deleteAPI", string.Format("{0}/(id)", apiEndpoint));

                //layout
                layoutJO.Add("table", jsonData["contentLayout"].ToString());
                layoutJO.Add("form", jsonData["formDefaultContentLayout"].ToString());

                //列表
                tableFieldsJO = handle.handleFieldsConf((JArray)jsonData["lowFieldss"]);
                //搜索
                searchFieldsJO = handle.handleSearchConf((JArray)jsonData["lowFilterss"]);
                //actions
                tableActionsJO = handle.handleActionsConf((JArray)jsonData["lowActionss"], (JArray)jsonData["lowFieldss"]);
                //操作栏
                tableOperationJO = handle.handleOperationConf((JArray)jsonData["lowOperationss"], (JArray)jsonData["lowFieldss"]);
                //新增
                createFieldsJO = handle.handleCreateConf((JArray)jsonData["lowFieldss"], "add");
                //编辑
                updateFieldsJO = handle.handleCreateConf((JArray)jsonData["lowFieldss"], "edit");
                //详情
                viewConfigJO = handle.handleViewConf((JArray)jsonData["lowFieldss"]);

                bodyContent.Add("pageName", pageNameJO);
                bodyContent.Add("createFields", createFieldsJO);
                bodyContent.Add("updateFields", updateFieldsJO);
                bodyContent.Add("layout", layoutJO);
                bodyContent.Add("tableActions", tableActionsJO);
                bodyContent.Add("tableOperation", tableOperationJO);
                bodyContent.Add("searchFields", searchFieldsJO);
                bodyContent.Add("tableFields", tableFieldsJO);
                bodyContent.Add("viewConfig", viewConfigJO);

                return bodyContent.ToString();

            }
            catch (Exception ex)
            {
                return "";
            }

        }
    }
}