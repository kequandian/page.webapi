using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PageConfig.WebApi.Controllers.ApiHandle
{
    public class PageConfigHandle
    {
        public PageConfigHandle()
        {

        }

        #region 列表
        public JArray handleFieldsConf(JArray listFields)
        {
            JArray tableFields = new JArray();
            JObject tableFieldItem = null;
            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    tableFieldItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    if (listItemJObect["fieldScopes"].ToString().Contains("table"))
                    {
                        tableFieldItem.Add("field", listItemJObect["fieldBinding"]);
                        tableFieldItem.Add("label", listItemJObect["fieldLabel"]);
                        if (!listItemJObect["listColumnType"].ToString().Equals("plain"))
                        {
                            tableFieldItem.Add("valueType", listItemJObect["listColumnType"]);
                            JObject optionsJO = (JObject)JsonConvert.DeserializeObject(listItemJObect["listColumnOptions"].ToString());
                            if(optionsJO != null)
                            {
                                if (optionsJO.ContainsKey("options"))
                                {
                                    tableFieldItem.Add("options", optionsJO["options"]);
                                }
                                if (optionsJO.ContainsKey("theme"))
                                {
                                    tableFieldItem.Add("theme", optionsJO["theme"]);
                                }
                                if (optionsJO.ContainsKey("type"))
                                {
                                    tableFieldItem.Add("type", optionsJO["type"]);
                                }
                            }
                        }
                        tableFields.Add(tableFieldItem);
                    }
                }
                return tableFields;
            }
            return null;
        }
        #endregion

        #region 搜索栏
        public JArray handleSearchConf(JArray listFields)
        {

            if (listFields == null)
            {
                return null;
            }
            JArray searchFields = new JArray();
            JObject searchFieldItem = null;
            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    searchFieldItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    searchFieldItem.Add("field", listItemJObect["fieldName"]);
                    searchFieldItem.Add("label", listItemJObect["fieldTitle"]);
                    searchFieldItem.Add("type", listItemJObect["fieldType"]);
                    JObject propsJO = new JObject();
                    propsJO.Add("placeholder", listItemJObect["defaultSearchHint"].ToString());
                    searchFieldItem.Add("props", propsJO);
                    searchFields.Add(searchFieldItem);
                }
                return searchFields;
            }
            return null;
        }
        #endregion

        #region actions
        public JArray handleActionsConf(JArray listFields)
        {

            if (listFields == null)
            {
                return null;
            }
            JArray actions = new JArray();
            JObject actionsItem = null;
            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    actionsItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    actionsItem.Add("title", listItemJObect["title"]);
                    actionsItem.Add("type", listItemJObect["type"]);
                    JObject propsJO = new JObject();

                    #region options非必须属性
                    if (listItemJObect["requestOptions"].ToString().StartsWith("{") && listItemJObect["requestOptions"].ToString().EndsWith("}"))
                    {
                        JObject requestOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJObect["requestOptions"].ToString());
                        foreach (var rqItem in requestOptionsJObect)
                        {
                            propsJO.Add(rqItem.Key.ToString(), rqItem.Value);
                        }
                    }
                    #endregion

                    propsJO.Add("path", listItemJObect["path"]);
                    actionsItem.Add("options", propsJO);
                    actions.Add(actionsItem);
                }
                return actions;
            }
            return null;
        }
        #endregion

        #region 操作栏
        /// <summary>
        /// 操作栏
        /// </summary>
        /// <param name="listFields"></param>
        /// <param name="fields">config => fields数据</param>
        /// <returns></returns>
        public JArray handleOperationConf(JArray listFields, JArray fields)
        {

            if (listFields == null)
            {
                return null;
            }
            JArray operations = new JArray();
            JObject operationsItem = null;

            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    operationsItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    string type = listItemJObect["type"].ToString();
                    operationsItem.Add("title", listItemJObect["title"]);
                    operationsItem.Add("type", type);

                    JObject propsJO = new JObject();
                    if (outsideStatus(int.Parse(listItemJObect["outside"].ToString())))
                    {
                        propsJO.Add("outside", true);
                    }

                    //模态框
                    if (type.Equals("modal"))
                    {
                        propsJO.Add("modalTitle", listItemJObect["modalTitle"]);
                        propsJO.Add("modalWidth", listItemJObect["modalWidth"] != null ? int.Parse(listItemJObect["modalWidth"].ToString()) : "600");
                        propsJO.Add("layout", listItemJObect["modalLayout"]);

                        JArray itemsJA = new JArray();
                        JObject itemJO = new JObject();

                        JObject originItemJO = (JObject)(listItemJObect["items"][0]);


                        itemJO.Add("layout", originItemJO["modalItemsLayout"]);
                        itemJO.Add("component", originItemJO["modalItemsComp"]);


                        JObject itemConfigJO = new JObject();
                        itemConfigJO.Add("layout", originItemJO["modalContentLayout"]);

                        JObject itemConfigApiJO = new JObject();
                        itemConfigApiJO.Add("getAPI", originItemJO["modalContentUpdateApi"]);
                        itemConfigApiJO.Add("updateAPI", originItemJO["modalContentUpdateApi"]);

                        itemConfigJO.Add("API", itemConfigApiJO);

                        itemConfigJO.Add("fields", handleFieldsConf(fields));

                        itemJO.Add("config", itemConfigJO);


                        itemsJA.Add(itemJO);
                        propsJO.Add("items", itemsJA);

                    }
                    //路由跳转
                    else if (type.Equals("path"))
                    {
                        propsJO.Add("path", listItemJObect["path"]);
                    }
                    //网络访问
                    else if (type.Equals("request"))
                    {
                        propsJO.Add("API", listItemJObect["requestRefreshApi"]);
                        propsJO.Add("method", listItemJObect["requestMethod"]);
                    }
                    //删除
                    else if (type.Equals("delete"))
                    {
                        if (!listItemJObect["requestRefreshApi"].ToString().Equals(""))
                        {
                            propsJO.Add("API", listItemJObect["requestRefreshApi"]);
                            propsJO.Add("method", listItemJObect["requestMethod"]);
                        }
                    }

                    operationsItem.Add("options", propsJO);
                    operations.Add(operationsItem);
                }
                return operations;
            }


            return null;
        }

        private bool outsideStatus(int value)
        {
            switch (value)
            {
                case 1:
                    return true;
                case 0:
                    return false;
                default:
                    return false;
            }
        }
        #endregion

        #region 新建
        public JArray handleCreateConf(JArray listFields, string filterType)
        {

            if (listFields == null)
            {
                return null;
            }
            JArray createFields = new JArray();
            JObject createFieldItem = null;
            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    createFieldItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    if (listItemJObect["fieldScopes"].ToString().Contains(filterType))
                    {
                        createFieldItem.Add("field", listItemJObect["fieldBinding"]);
                        createFieldItem.Add("label", listItemJObect["fieldLabel"]);
                        JArray rulesJA = new JArray();
                        if (int.Parse(listItemJObect["formInputRequired"].ToString()) == 1)
                        {
                            JObject rulesItemJO = new JObject();
                            rulesItemJO.Add("type", "required");
                            rulesJA.Add(rulesItemJO);
                            createFieldItem.Add("rules", rulesJA);
                        }
                        JObject propsJO = new JObject();
                        if (!listItemJObect["formFieldHint"].ToString().Equals(""))
                        {
                            propsJO.Add("placeholder", listItemJObect["formFieldHint"].ToString());
                            createFieldItem.Add("props", propsJO);
                        }
                        createFieldItem.Add("type", listItemJObect["formInputType"]);

                        if (listItemJObect["formInputOptions"].ToString().StartsWith("{") && listItemJObect["formInputOptions"].ToString().EndsWith("}"))
                        {
                            JObject formInputOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJObect["formInputOptions"].ToString());
                            foreach (var fioItem in formInputOptionsJObect)
                            {
                                createFieldItem.Add(fioItem.Key.ToString(), fioItem.Value);
                            }
                        }

                        if (listItemJObect["fieldValueOptions"].ToString().StartsWith("{") && listItemJObect["fieldValueOptions"].ToString().EndsWith("}"))
                        {
                            JObject fieldValueOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJObect["fieldValueOptions"].ToString());
                            createFieldItem.Add("options", fieldValueOptionsJObect["options"]);
                        }

                        createFields.Add(createFieldItem);
                    }
                }
                return createFields;
            }
            return null;
        }
        #endregion

        #region 编辑
        //private JArray handleUpdateConf(JArray listFields)
        //{
        //    JArray updateFields = new JArray();
        //    JObject updateFieldItem = null;
        //    if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
        //    {
        //        foreach (var listItem in listFields)
        //        {
        //            updateFieldItem = new JObject();
        //            JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
        //            if (listItemJObect["fieldScopes"].ToString().Contains("edit"))
        //            {
        //                updateFieldItem.Add("field", listItemJObect["fieldBinding"]);
        //                updateFieldItem.Add("label", listItemJObect["fieldLabel"]);
        //                JArray rulesJA = new JArray();
        //                if (int.Parse(listItemJObect["formInputRequired"].ToString()) == 1)
        //                {
        //                    JObject rulesItemJO = new JObject();
        //                    rulesItemJO.Add("type", "required");
        //                    rulesJA.Add(rulesItemJO);
        //                    updateFieldItem.Add("rules", rulesJA);
        //                }
        //                JObject propsJO = new JObject();
        //                if (!listItemJObect["formFieldHint"].ToString().Equals(""))
        //                {
        //                    propsJO.Add("placeholder", listItemJObect["formFieldHint"].ToString());
        //                    updateFieldItem.Add("props", propsJO);
        //                }
        //                updateFieldItem.Add("type", listItemJObect["formInputType"]);

        //                if (listItemJObect["formInputOptions"].ToString().StartsWith("{") && listItemJObect["formInputOptions"].ToString().EndsWith("}"))
        //                {
        //                    JObject formInputOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJObect["formInputOptions"].ToString());
        //                    foreach (var fioItem in formInputOptionsJObect)
        //                    {
        //                        updateFieldItem.Add(fioItem.Key.ToString(), fioItem.Value);
        //                    }
        //                }

        //                if (listItemJObect["fieldValueOptions"].ToString().StartsWith("{") && listItemJObect["fieldValueOptions"].ToString().EndsWith("}"))
        //                {
        //                    JObject fieldValueOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJObect["fieldValueOptions"].ToString());
        //                    updateFieldItem.Add("options", fieldValueOptionsJObect["options"]);
        //                }

        //                updateFields.Add(updateFieldItem);
        //            }
        //        }
        //        return updateFields;
        //    }
        //    return null;
        //}
        #endregion

        #region 详情
        public JArray handleViewConf(JArray listFields)
        {
            
            if(listFields == null)
            {
                return null;
            }
            JArray viewFields = new JArray();
            JObject viewFieldItem = new JObject();
            viewFieldItem.Add("title", "详情");
            viewFieldItem.Add("type", "plain");
            JArray itemfieldsProps = new JArray();

            JObject viFieldItem = null;
            if (listFields.ToString().StartsWith("[") && listFields.ToString().EndsWith("]")) //判断是否是Json 数组
            {
                foreach (var listItem in listFields)
                {
                    viFieldItem = new JObject();
                    JObject listItemJObect = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    if (listItemJObect["fieldScopes"].ToString().Contains("view"))
                    {
                        viFieldItem.Add("field", listItemJObect["fieldBinding"]);
                        viFieldItem.Add("label", listItemJObect["fieldLabel"]);

                        if (!listItemJObect["formViewOptions"].ToString().Equals(""))
                        {
                            JObject optionsJO = (JObject)JsonConvert.DeserializeObject(listItemJObect["formViewOptions"].ToString());
                            viFieldItem.Add("options", optionsJO["options"]);

                        }
                    }

                    itemfieldsProps.Add(viFieldItem);

                }

                viewFieldItem.Add("fields", itemfieldsProps);
                viewFields.Add(viewFieldItem);
                return viewFields;
            }
            return null;
        }
        #endregion

    }
}