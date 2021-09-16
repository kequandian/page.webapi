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
    [Route("/api")]
    public class PageConfigController : ControllerBase
    {
        private ApiTools tool = new ApiTools();
        private PageConfigHandle handle = new PageConfigHandle();

        private readonly ILogger<PageConfigController> _logger;

        public PageConfigController(ILogger<PageConfigController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 转换页面配置json格式
        /// </summary>
        /// <param name="pageJson"></param>
        [Route("/toconfig")]
        [HttpPost]
        public HttpResponseMessage ToConfig(dynamic pageJson)
        {
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
                tableActionsJO = handle.handleActionsConf((JArray)jsonData["lowActionss"]);
                //操作栏
                tableOperationJO = handle.handleOperationConf((JArray)jsonData["lowOperationss"]);
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

                return tool.MsgFormatToJson(ResponseCode.成功, bodyContent.ToString(), "Success");

            }
            catch (Exception ex)
            {
                return tool.MsgFormat(ResponseCode.操作失败, "获取信息失败", ex.ToString());
            }

        }



    }
}
