using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace PageConfig.WebApi.Controllers.ApiHandle
{
    public class SettingToFieldHandle
    {
        public SettingToFieldHandle()
        {
        }

        /// <summary>
        /// 列表是否显示按钮
        /// </summary>
        public int matchOutside(bool value)
        {
            return value ? 1 : 0;
        }

        /// <summary>
        /// 表单是否必填
        /// </summary>
        public int matchRequired(JObject value)
        {
            return 0;
        }

        /// <summary>
        /// 处理新增
        /// </summary>
        public JObject handleCreatePage(JObject postJO, JArray createFields)
        {
            foreach (JObject objItem in createFields)
            {
                if (objItem["field"].Equals(postJO["fieldBinding"]))
                {
                    string newFieldScopes = postJO["fieldScopes"].ToString();
                    newFieldScopes = string.Format("{0},add", newFieldScopes);
                    postJO["fieldScopes"] = newFieldScopes;
                    postJO["formInputType"] = objItem["type"];
                    if (objItem["rules"] != null)
                    {
                        JArray rulesJA = (JArray)objItem["rules"];
                        if(rulesJA.Count > 0)
                        {
                            postJO["formInputRequired"] = 1;
                        }
                    }
                    if (objItem["options"] != null)
                    {
                        postJO["formInputOptions"] = objItem["options"];
                    }
                }
            }

            return postJO;
        }

        /// <summary>
        /// 处理编辑
        /// </summary>
        public JObject handleEditPage(JObject postJO, JArray updateFields)
        {
            foreach (JObject objItem in updateFields)
            {
                if (objItem["field"].Equals(postJO["fieldBinding"]))
                {
                    string newFieldScopes = postJO["fieldScopes"].ToString();
                    newFieldScopes = string.Format("{0},edit", newFieldScopes);
                    postJO["fieldScopes"] = newFieldScopes;
                    postJO["formInputType"] = objItem["type"];
                    if (objItem["rules"] != null)
                    {
                        JArray rulesJA = (JArray)objItem["rules"];
                        if (rulesJA.Count > 0)
                        {
                            postJO["formInputRequired"] = 1;
                        }
                    }
                    if (objItem["options"] != null)
                    {
                        postJO["formInputOptions"] = objItem["options"];
                    }
                }
            }
            return postJO;
        }

        /// <summary>
        /// 处理详情
        /// </summary>
        public JObject handleDetailPage(JObject postJO, JArray viewConfig)
        {
            foreach (JObject objItem in viewConfig)
            {
                if (objItem["field"].Equals(postJO["fieldBinding"]))
                {
                    string newFieldScopes = postJO["fieldScopes"].ToString();
                    newFieldScopes = string.Format("{0},view", newFieldScopes);
                    postJO["fieldScopes"] = newFieldScopes;
                    postJO["formViewType"] = objItem["type"];
                    if (objItem["options"] != null)
                    {
                        postJO["fieldValueOptions"] = objItem["options"];
                    }
                }
            }
            return postJO;
        }

    }
}
