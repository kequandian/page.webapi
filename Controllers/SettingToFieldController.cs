﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PageConfig.WebApi.Controllers.ApiHandle;
using PageConfig.WebApi.utils;
using PageConfig.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using PageConfig.WebApi.Method;

namespace PageConfig.WebApi.Controllers
{
    [ApiController]
    [Route("pageconfig")]
    public class SettingToFieldController : ControllerBase
    {
        private static ApiTools tool = new ApiTools();
        //private static IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        //private static TokenService getToken = new TokenService(httpContextAccessor);
        private static SettingToFieldHandle settingToFieldHandle = new SettingToFieldHandle();

        private string endpoint = "";
        private static string testEndpoint = PublicFunction.getAddress();
        private int pageId = 0;

        private readonly ILogger<SettingToFieldController> _logger;

        public SettingToFieldController(ILogger<SettingToFieldController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取原始数据并创建页面
        /// </summary>
        /// <param name="requestData"></param>
        [HttpPost("toField")]
        //[Authorize]
        public HttpResponseMessage SettingJsonConvertToField(dynamic requestData)
        {
            const string resultinfo_key = "data";
            dynamic reqData = JsonConvert.DeserializeObject(Convert.ToString(requestData));
            string token = reqData["token"].ToString();
            JObject pageData = reqData[resultinfo_key];
            JObject pageJsonResponse = getSettingJson(pageData["path"].ToString(), token);

            var status = pageJsonResponse["code"] != null ? Convert.ToInt32(pageJsonResponse["code"]) : 0;
            if (status != 200)
            {
                return tool.MsgFormat(ResponseCode.操作失败, pageJsonResponse["message"].ToString(), "获取 pageSettingJson 失败");

                //return tool.MsgError(pageJsonResponse["message"].ToString());
            }

            dynamic obj = JsonConvert.DeserializeObject(Convert.ToString(pageJsonResponse[resultinfo_key]));


            #region 获取原始数据并创建页面
            //创建页面
            JObject createPageResponse = createPage(obj, pageData["menuId"].ToString(), token);
            var createPageStatus = createPageResponse["code"] != null ? Convert.ToInt32(createPageResponse["code"]) : 0;

            if (createPageStatus != 200)
            {

                return tool.MsgFormat(ResponseCode.操作失败, "创建页面失败", createPageResponse["message"].ToString());
            }

            #endregion

            //pageId = 105;
            pageId = int.Parse(createPageResponse[resultinfo_key]["id"].ToString());
            int errCount = 0;

            #region 操作栏
            //操作栏
            JObject lowOperationsItem = new JObject();
            JArray tableOperationList = obj["tableOperation"];

            string bb = tableOperationList.ToString();
            for (int i = 0; i < tableOperationList.Count; i++)
            {
                JObject itemObj = (JObject)tableOperationList[i];
                string aa = itemObj.ToString();
                int addOptionStatus = addlowoperationss("", pageId, itemObj, token);

                if (addOptionStatus != 0)
                {
                    errCount++;
                    return tool.MsgFormat(ResponseCode.操作失败, "新增操作按钮失败", "Error");
                }
            }
            if (errCount > 0)
            {
                int deletePageStatus = deletePage("", pageId, token);
                if (deletePageStatus != 0)
                {
                    Console.WriteLine("操作栏异常 -- 删除页面失败");
                }
                return tool.MsgFormat(ResponseCode.操作失败, "转换失败", "Error");
            }
            #endregion

            #region actions
            JObject lowActionsItem = new JObject();
            JArray tableActionsList = obj["tableActions"];
            foreach (JObject listItem in tableActionsList)
            {
                JObject itemObj = (JObject)listItem;
                int addActionStatus = addLowActions("", pageId, listItem, token);

                if (addActionStatus != 0)
                {
                    errCount++;
                    return tool.MsgFormat(ResponseCode.操作失败, "新增action按钮失败", "Error");
                }
            }
            if (errCount > 0)
            {
                int deletePageStatus = deletePage("", pageId, token);
                if (deletePageStatus != 0)
                {
                    Console.WriteLine("actions栏异常 -- 删除页面失败");
                }
                return tool.MsgFormat(ResponseCode.操作失败, "转换失败", "Error");
            }
            #endregion

            #region 搜索
            JObject lowFiltersItem = new JObject();
            JArray searchFieldsList = obj["searchFields"];
            foreach (JObject listItem in searchFieldsList)
            {
                JObject itemObj = (JObject)listItem;
                int addFilterStatus = addLowFilterss("", pageId, listItem, token);

                if (addFilterStatus != 0)
                {
                    errCount++;
                    return tool.MsgFormat(ResponseCode.操作失败, "新增搜索栏失败", "Error");
                }
            }
            if (errCount > 0)
            {
                int deletePageStatus = deletePage("", pageId, token);
                if (deletePageStatus != 0)
                {
                    Console.WriteLine("搜索栏异常 -- 删除页面失败");
                }
                return tool.MsgFormat(ResponseCode.操作失败, "转换失败", "Error");
            }
            #endregion


            #region 列表
            JObject lowFieldsItem = new JObject();
            JArray tableFieldsList = obj["tableFields"];
            foreach (JObject listItem in tableFieldsList)
            {

                JObject itemObj = (JObject)listItem;
                int addFieldStatus = addPageField("", pageId, listItem, token, obj["createFields"], obj["updateFields"], obj["viewConfig"]);

                if (addFieldStatus != 0)
                {
                    errCount++;
                    return tool.MsgFormat(ResponseCode.操作失败, "新增列表字段失败", "Error");
                }
            }

            if (errCount > 0)
            {
                int deletePageStatus = deletePage("", pageId, token);
                if (deletePageStatus != 0)
                {
                    Console.WriteLine("列表异常 -- 删除页面失败");
                }
                return tool.MsgFormat(ResponseCode.操作失败, "转换失败", "Error");
            }
            #endregion


            //获取详情
            JObject pageDetailObj = getPageDetail("", pageId, token);
            if (pageDetailObj != null)
            {
                return tool.MsgFormatToJson(ResponseCode.成功, pageDetailObj, "Success");
            }
            else
            {
                int deletePageStatus = deletePage("", pageId, token);
                if (deletePageStatus != 0)
                {
                    return tool.MsgFormat(ResponseCode.操作失败, "页面删除失败", "Error");
                }
                return tool.MsgFormat(ResponseCode.操作失败, "转换失败", "Error");
            }
        }

        #region 获取 setting json
        /// <summary>
        /// 获取seting json
        /// </summary>
        /// 
        static JObject getSettingJson(string pagePath, string token)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            //处理提交数据
            JObject bodyObj = new JObject();
            bodyObj.Add("path", pagePath);
            HttpContent content = new StringContent(bodyObj.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var http = new HttpClient(handler))
            {
                //string url = string.Format("{0}/vallation/node/api/get-setting", testEndpoint);
                string url = string.Format("{0}/sideapi/pageconfig/get-setting", testEndpoint);
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var task = http.PostAsync(url, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject joRet = (JObject)JsonConvert.DeserializeObject(task2.Result);

                return joRet;

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                //Console.WriteLine(await rep.Content.ReadAsStringAsync());

            }
        }
        #endregion

        #region 创建页面
        /// <summary>
        /// 创建页面
        /// </summary>
        /// 
        static JObject createPage(JObject obj, string menuId, string token)
        {
            JObject bodyContent = new JObject();
            bodyContent.Add("formAddTitle", obj["pageName"]["new"] != null ? obj["pageName"]["new"] : "");
            bodyContent.Add("searchType", obj["searchType"] != null ? obj["searchType"] : "");
            bodyContent.Add("pageTitle", obj["pageName"]["table"] != null ? obj["pageName"]["table"] : "");
            bodyContent.Add("formViewTitle", obj["pageName"]["view"] != null ? obj["pageName"]["view"] : "");
            bodyContent.Add("formDefaultContentLayout", obj["layout"]["form"] != null ? obj["layout"]["form"] : "");
            bodyContent.Add("formDefaultWidth", 0);
            bodyContent.Add("pageName", obj["pageName"]["name"] != null ? obj["pageName"]["name"] : "");
            bodyContent.Add("contentLayout", obj["layout"]["table"] != null ? obj["layout"]["table"] : "");
            bodyContent.Add("searchButtonType", obj["searchButtonType"] != null ? obj["searchButtonType"] : "");
            bodyContent.Add("apiEndpoint", obj["listAPI"] != null ? obj["listAPI"] : "");
            bodyContent.Add("columnAlign", "left");
            bodyContent.Add("formEditTitle", obj["pageName"]["edit"] != null ? obj["pageName"]["edit"] : "");
            bodyContent.Add("contentItemContainerStyle", "");
            bodyContent.Add("lowOperationss", "");
            bodyContent.Add("lowActionss", "");
            bodyContent.Add("lowFilterss", "");
            bodyContent.Add("lowFieldss", "");
            bodyContent.Add("menuId", menuId);

            string apiUrl = "api/lc/lowMainPage/lowMainPages/";
            string testUrl = "http://gitlab2.cdnline.cn:8000/" + apiUrl;  //by zxq 目前测试地址http://gitlab2.cdnline.cn:8000/

            //先请求api/lc/lowMainPage/lowMainPages
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            JObject lowMainPagesRespose;
            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                HttpContent content = new StringContent(bodyContent.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                lowMainPagesRespose = (JObject)JsonConvert.DeserializeObject(task2.Result);
            }
            if (lowMainPagesRespose == null)
                throw new Exception(string.Format("{0} response empty", apiUrl));
            if (lowMainPagesRespose["code"].ToString() != "200")
                throw new Exception(string.Format("{0} response error :{0}", apiUrl, lowMainPagesRespose["message"]));

            //在返回值中取到pageID
            string id = lowMainPagesRespose["data"]["id"].ToString();
            if (id == string.Empty)
                throw new Exception(string.Format("{0} response id empty", apiUrl));

            //再用返回值请求api/crud/menu/menus/${id}
            bodyContent = new JObject();
            bodyContent.Add("pageId", id);
            apiUrl = "api/crud/menu/menus/" + menuId;
            testUrl = "http://gitlab2.cdnline.cn:8000/" + apiUrl;  //by zxq 目前测试地址http://gitlab2.cdnline.cn:8000/
            JObject menusRespose;
            handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            using (var http = new HttpClient(handler))
            {
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                HttpContent content = new StringContent(bodyContent.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PutAsync(testUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                menusRespose = (JObject)JsonConvert.DeserializeObject(task2.Result);
            }
            if (menusRespose == null)
                throw new Exception(string.Format("{0} response empty", apiUrl));
            if (menusRespose["data"] == null)
                throw new Exception(string.Format("{0} response data empty", apiUrl));

            //需要返回创建成功的动态页面id

            JObject data = new JObject();
            data.Add("id", id);
            return ResultTools.successResult(data);
        }
        #endregion

        #region 添加操作栏按钮
        /// <summary>
        /// 添加操作栏按钮
        /// </summary>
        /// 
        static int addlowoperationss(string url, int pageId, JObject itemData, string token)
        {
            string testUrl = string.Format("{0}/api/lc/lowOperations/lowOperationses", "http://gitlab2.cdnline.cn:8000");  //by zxq 目前测试地址http://gitlab2.cdnline.cn:8000/
            //string testUrl = string.Format("{0}/api/crud/lowOperations/lowOperationses", testEndpoint); //zxq

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("path", itemData["options"] != null && itemData["options"]["path"] != null ? itemData["options"]["path"] : "");
                postJO.Add("outside", itemData["options"] != null && itemData["options"]["outside"] != null ? settingToFieldHandle.matchOutside(Boolean.Parse(itemData["options"]["outside"].ToString())) : 0);
                postJO.Add("requestRefreshApi", itemData["options"] != null && itemData["options"]["API"] != null ? itemData["options"]["API"] : "");
                postJO.Add("requestBody", itemData["options"] != null && itemData["options"]["data"] != null ? itemData["options"]["data"] : "");
                postJO.Add("requestMethod", itemData["options"] != null && itemData["options"]["method"] != null ? itemData["options"]["method"] : "");
                postJO.Add("pageId", pageId);
                postJO.Add("requestOptions", "");
                postJO.Add("title", itemData["title"]);
                //postJO.Add("type", "modal.update");//by zxq  成功
                postJO.Add("type", itemData["type"] != null ? itemData["type"] : "path");//zxq
                postJO.Add("expectField", itemData["expect"] != null && itemData["expect"]["field"] != null ? itemData["expect"]["field"] : "");
                postJO.Add("expectValue", itemData["expect"] != null && itemData["expect"]["value"] != null ? itemData["expect"]["value"] : "");
                postJO.Add("modalTitle", "");
                postJO.Add("requestApi", "");

                string aa = postJO.ToString();

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = respJO["code"] != null ? Convert.ToInt32(respJO["code"]) : 0;

                if (status == 200)
                {
                    return 0;
                }
                return 1;
            }
        }
        #endregion

        #region 添加 action 按钮
        /// <summary>
        /// 添加lowActionss
        /// </summary>
        /// 
        static int addLowActions(string url, int pageId, JObject itemData, string token)
        {

            string testUrl = string.Format("{0}/api/crud/lowActions/lowActionses", testEndpoint);

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("path", itemData["options"]["path"].ToString());
                postJO.Add("requestRefreshApi", "");
                postJO.Add("requestBody", "");
                postJO.Add("requestMethod", "");
                postJO.Add("pageId", pageId);
                postJO.Add("requestOptions", "");
                postJO.Add("title", itemData["title"].ToString());
                postJO.Add("type", itemData["type"] != null ? itemData["type"].ToString() : "path");
                postJO.Add("modalTitle", "");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = respJO["code"] != null ? Convert.ToInt32(respJO["code"]) : 0;

                if (status == 200)
                {
                    return 0;
                }
                return 1;

            }
        }
        #endregion

        #region 添加搜索
        /// <summary>
        /// 添加 搜索栏
        /// </summary>
        /// 
        static int addLowFilterss(string url, int pageId, JObject filterData, string token)
        {

            string testUrl = string.Format("{0}/api/crud/lowFilters/lowFilterses", testEndpoint, pageId);

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();
                postJO.Add("contentLayout", "Grid");
                postJO.Add("fieldName", filterData["field"] != null ? filterData["field"].ToString() : "search");
                postJO.Add("defaultSearchHint", filterData["props"]["placeholder"] != null ? filterData["props"]["placeholder"].ToString() : "请输入");
                postJO.Add("searchFields", "");
                postJO.Add("pageId", pageId);
                postJO.Add("fieldTitle", filterData["label"] != null ? filterData["label"].ToString() : "搜索");
                postJO.Add("fieldType", filterData["type"] != null ? filterData["type"].ToString() : "search");

                HttpContent content = new StringContent(postJO.ToString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var task = http.PostAsync(requestUrl, content);
                var rep = task.Result;
                var task2 = rep.Content.ReadAsStringAsync();

                JObject respJO = (JObject)JsonConvert.DeserializeObject(task2.Result);
                var status = respJO["code"] != null ? Convert.ToInt32(respJO["code"]) : 0;

                if (status == 200)
                {
                    return 0;
                }
                return 1;

            }
        }
        #endregion

        #region 添加列表字段
        /// <summary>
        /// 添加页面字段
        /// </summary>
        /// 
        static int addPageField(string url, int pageId, JObject fieldData, string token, JArray createFields, JArray updateFields, JArray viewConfig)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            string testUrl = string.Format("{0}/api/crud/lowFields/lowFieldses", testEndpoint);

            using (var http = new HttpClient(handler))
            {
                string requestUrl = testUrl;

                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                JObject postJO = new JObject();

                postJO.Add("fieldLabel", fieldData["label"]);
                postJO.Add("fieldBinding", fieldData["field"]);
                postJO.Add("fieldScopes", "page,table");
                postJO.Add("listColumnType", fieldData["type"] != null ? fieldData["field"] : "plain");
                postJO.Add("listColumnName", fieldData["field"]);
                postJO.Add("listFontWeight", "");
                postJO.Add("listFontSize", "");
                postJO.Add("listFontColor", "");
                postJO.Add("listColumnLayout", "");
                postJO.Add("listColumnAlign", fieldData["align"] != null ? fieldData["align"] : "left");
                postJO.Add("listColumnWidth", fieldData["width"] != null ? fieldData["width"] : 0);
                postJO.Add("listColumnOptions", "");
                postJO.Add("fieldValueFilter", "");
                postJO.Add("formInputType", "input");
                postJO.Add("formViewType", "plain");
                postJO.Add("fieldViewOneManyOptions", "");
                postJO.Add("formFieldTitle", "");
                postJO.Add("formFieldHint", "");
                postJO.Add("formFieldTips", "");
                postJO.Add("formInputRequired", 0);
                postJO.Add("formFieldQuestion", "");
                postJO.Add("fieldValueOptions", "");
                postJO.Add("formInputOptions", "");
                postJO.Add("formViewOptions", "");
                postJO.Add("type", "path");
                postJO.Add("pageId", pageId.ToString());

                if (createFields != null && createFields.Count > 0)
                {
                    postJO = settingToFieldHandle.handleCreatePage(postJO, createFields);
                }

                if (updateFields != null && updateFields.Count > 0)
                {
                    postJO = settingToFieldHandle.handleEditPage(postJO, updateFields);
                }

                if (viewConfig != null && viewConfig.Count > 0)
                {
                    postJO = settingToFieldHandle.handleDetailPage(postJO, viewConfig);
                }

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
        #endregion

        #region 获取页面详情
        /// <summary>
        /// 根据pageId获取页面详情
        /// </summary>
        /// 
        static JObject getPageDetail(string url, int pageId, string token)
        {

            string testUrl = string.Format("{0}/api/crud/lowMainPage/lowMainPages/{1}", testEndpoint, pageId);

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

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


        #endregion

        #region 删除页面
        /// <summary>
        /// 根据pageId获取页面详情
        /// </summary>
        /// 
        static int deletePage(string url, int pageId, string token)
        {

            string testUrl = string.Format("{0}/api/crud/lowMainPage/lowMainPages/{1}", testEndpoint, pageId);

            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                //string requestUrl = string.Format("{0}/{1}", url, pageId);
                string requestUrl = testUrl;
                if (token != null && !token.Equals(""))
                {
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var task = http.DeleteAsync(requestUrl);
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
        #endregion
    }
}
