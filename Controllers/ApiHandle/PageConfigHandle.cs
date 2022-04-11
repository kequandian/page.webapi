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
                    JObject listItemJO = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    if (listItemJO["fieldScopes"].ToString().Contains("table"))
                    {
                        tableFieldItem.Add("field", listItemJO["fieldBinding"]);
                        tableFieldItem.Add("label", listItemJO["fieldLabel"]);
                        if (!listItemJO["listColumnType"].ToString().Equals("plain"))
                        {
                            tableFieldItem.Add("valueType", listItemJO["listColumnType"]);
                            JObject optionsJO = (JObject)JsonConvert.DeserializeObject(listItemJO["listColumnOptions"].ToString());
                            if(optionsJO != null)
                            {
                                if (optionsJO.ContainsKey("options"))
                                {
                                    tableFieldItem.Add("options", optionsJO["options"]);

                                    
                                    //JObject optionJO = (JObject)tableFieldItem["options"];
                                    //if (listItemJO.ContainsKey("listFontSize") && int.Parse(listItemJO["listFontSize"].ToString()) > 0)
                                    //{
                                    //    optionJO.Add("font-size", listItemJO["listFontSize"]);
                                    //}

                                    //if (listItemJO.ContainsKey("listFontWeight") && !listItemJO["listFontWeight"].ToString().Equals(""))
                                    //{
                                    //    optionJO.Add("font-weight", listItemJO["listFontWeight"]);
                                    //}

                                    //if (listItemJO.ContainsKey("listFontColor") && !listItemJO["listFontWeight"].ToString().Equals(""))
                                    //{
                                    //    optionJO.Add("font-color", listItemJO["listFontColor"]);
                                    //}

                                    //tableFieldItem["options"] = optionJO;
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
                        else
                        {

                            JObject otherOption = new JObject();
                            JObject styles = new JObject();
                            if (listItemJO.ContainsKey("listFontSize") && int.Parse(listItemJO["listFontSize"].ToString()) > 0)
                            {
                                styles.Add("font-size", listItemJO["listFontSize"]);
                            }

                            if (listItemJO.ContainsKey("listFontWeight") && !listItemJO["listFontWeight"].ToString().Equals(""))
                            {
                                styles.Add("font-weight", listItemJO["listFontWeight"]);
                            }

                            if (listItemJO.ContainsKey("listFontColor") && !listItemJO["listFontColor"].ToString().Equals(""))
                            {
                                styles.Add("color", listItemJO["listFontColor"]);
                            }

                            if (styles.ContainsKey("font-size") || styles.ContainsKey("font-weight") || styles.ContainsKey("font-color"))
                            {
                                otherOption.Add("style", styles);
                                tableFieldItem.Add("options", otherOption);
                            }
                        }

                        //内容摆放位置属性
                        if (!listItemJO["listColumnAlign"].ToString().Equals(""))
                        {
                            tableFieldItem.Add("align", listItemJO["listColumnAlign"]);
                        }

                        //列宽 
                        if (listItemJO.ContainsKey("listColumnWidth") && int.Parse(listItemJO["listColumnWidth"].ToString()) > 0)
                        {
                            tableFieldItem.Add("width", listItemJO["listColumnWidth"]);
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
        public JArray handleActionsConf(JArray listFields, JArray fields)
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
                    JObject listItemJO = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    string actionType = listItemJO["type"].ToString();
                    actionsItem.Add("title", listItemJO["title"]);
                    actionsItem.Add("type", actionType);
                    JObject propsJO = new JObject();

                    #region options非必须属性
                    if (listItemJO["requestOptions"] != null && listItemJO["requestOptions"].ToString().StartsWith("{") && listItemJO["requestOptions"].ToString().EndsWith("}"))
                    {
                        JObject requestOptionsJO = (JObject)JsonConvert.DeserializeObject(listItemJO["requestOptions"].ToString());
                        foreach (var rqItem in requestOptionsJO)
                        {
                            propsJO.Add(rqItem.Key.ToString(), rqItem.Value);
                        }
                    }
                    #endregion
                    if (actionType.Equals("request"))
                    {
                        propsJO.Add("API", listItemJO["requestRefreshApi"]);
                        propsJO.Add("method", listItemJO["requestMethod"]);
                        if(listItemJO["requestBody"] != null)
                        {
                            JObject dataJO = (JObject)JsonConvert.DeserializeObject(listItemJO["requestBody"].ToString());
                            propsJO.Add("data", dataJO);
                        }
                    }
                    else if (actionType.Equals("import"))
                    {

                    }
                    else if (actionType.Equals("export"))
                    {

                    }
                    else if (actionType.Equals("modal")) //模态框
                    {
                        propsJO.Add("modalTitle", listItemJO["modalTitle"]);
                        propsJO.Add("modalWidth", listItemJO["modalWidth"] != null ? int.Parse(listItemJO["modalWidth"].ToString()) : "600");
                        propsJO.Add("layout", listItemJO["modalLayout"]);

                        JArray itemsJA = new JArray();
                        JObject itemJO = new JObject();

                        JObject originItemJO = new JObject();
                        JArray itemList = (JArray)listItemJO["items"];
                        if (itemList.Count > 0)
                        {
                            originItemJO = (JObject)itemList[0];
                            itemJO.Add("layout", originItemJO["modalItemsLayout"]);
                            itemJO.Add("component", originItemJO["modalItemsComp"]);


                            JObject itemConfigJO = new JObject();
                            itemConfigJO.Add("layout", originItemJO["modalContentLayout"]);

                            JObject itemConfigApiJO = new JObject();
                            if ( originItemJO["modalContentUpdateApi"] != null &&!originItemJO["modalContentUpdateApi"].ToString().Equals(""))
                            {
                                itemConfigApiJO.Add("getAPI", originItemJO["modalContentUpdateApi"]);
                                itemConfigApiJO.Add("updateAPI", originItemJO["modalContentUpdateApi"]);
                            }
                            else if( originItemJO["modalContentCreateApi"] != null &&!originItemJO["modalContentCreateApi"].ToString().Equals(""))
                            {
                                itemConfigApiJO.Add("createAPI", originItemJO["modalContentCreateApi"]);
                            }
                            itemConfigJO.Add("API", itemConfigApiJO);

                            itemConfigJO.Add("fields", handleCreateConf(fields, "add"));
                            itemJO.Add("config", itemConfigJO);
                            itemsJA.Add(itemJO);
                        }

                        propsJO.Add("items", itemsJA);

                    }
                    else
                    {
                        propsJO.Add("path", listItemJO["path"]);
                    }
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
                    JObject listItemJO = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    string type = listItemJO["type"].ToString();
                    operationsItem.Add("title", listItemJO["title"]);
                    operationsItem.Add("type", type);

                    JObject propsJO = new JObject();
                    if (listItemJO["outside"] != null && outsideStatus(int.Parse(listItemJO["outside"].ToString())))
                    {
                        propsJO.Add("outside", true);
                    }

                    //模态框
                    if (type.Equals("modal"))
                    {
                        propsJO.Add("modalTitle", listItemJO["modalTitle"]);
                        propsJO.Add("modalWidth", listItemJO["modalWidth"] != null ? int.Parse(listItemJO["modalWidth"].ToString()) : "600");
                        propsJO.Add("layout", listItemJO["modalLayout"]);

                        JArray itemsJA = new JArray();
                        JObject itemJO = new JObject();

                        JObject originItemJO = new JObject();
                        JArray itemList = (JArray)listItemJO["items"];


                        if (itemList.Count > 0)
                        {
                            originItemJO = (JObject)itemList[0];
                            itemJO.Add("layout", originItemJO["modalItemsLayout"]);
                            itemJO.Add("component", originItemJO["modalItemsComp"]);


                            JObject itemConfigJO = new JObject();
                            itemConfigJO.Add("layout", originItemJO["modalContentLayout"]);

                            JObject itemConfigApiJO = new JObject();
                            if (originItemJO["modalContentUpdateApi"] != null && !originItemJO["modalContentUpdateApi"].ToString().Equals(""))
                            {
                                itemConfigApiJO.Add("getAPI", originItemJO["modalContentUpdateApi"]);
                                itemConfigApiJO.Add("updateAPI", originItemJO["modalContentUpdateApi"]);
                            }
                            else if (originItemJO["modalContentCreateApi"] != null && !originItemJO["modalContentCreateApi"].ToString().Equals(""))
                            {
                                itemConfigApiJO.Add("createAPI", originItemJO["modalContentCreateApi"]);
                            }
                            itemConfigJO.Add("API", itemConfigApiJO);

                            itemConfigJO.Add("fields", handleCreateConf(fields, "add"));
                            itemJO.Add("config", itemConfigJO);
                            itemsJA.Add(itemJO);
                        }
                        propsJO.Add("items", itemsJA);

                    }
                    //路由跳转
                    else if (type.Equals("path"))
                    {
                        propsJO.Add("path", listItemJO["path"]);
                    }
                    //网络访问
                    else if (type.Equals("request"))
                    {
                        propsJO.Add("API", listItemJO["requestRefreshApi"]);
                        propsJO.Add("method", listItemJO["requestMethod"]);
                        JObject dataJO = (JObject)JsonConvert.DeserializeObject(listItemJO["requestBody"].ToString());
                        propsJO.Add("data", dataJO);
                    }
                    //删除
                    else if (type.Equals("delete"))
                    {
                        if (!listItemJO["requestRefreshApi"].ToString().Equals(""))
                        {
                            propsJO.Add("API", listItemJO["requestRefreshApi"]);
                            propsJO.Add("method", listItemJO["requestMethod"]);
                        }
                    }

                    operationsItem.Add("options", propsJO);

                    //显示/隐藏属性
                    if (listItemJO["expectField"] != null && listItemJO["expectValue"] != null && !listItemJO["expectField"].ToString().Equals("") && !listItemJO["expectValue"].ToString().Equals(""))
                    {
                        JObject expectJO = new JObject();
                        expectJO.Add("field", listItemJO["expectField"]);
                        expectJO.Add("value", listItemJO["expectValue"]);
                        operationsItem.Add("expect", expectJO);
                    }

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
                    JObject listItemJO = (JObject)JsonConvert.DeserializeObject(listItem.ToString());
                    if (listItemJO["fieldScopes"]!=null &&listItemJO["fieldScopes"].ToString().Contains(filterType))
                    {
                        createFieldItem.Add("field", listItemJO["fieldBinding"]);
                        createFieldItem.Add("label", listItemJO["fieldLabel"]);
                        JArray rulesJA = new JArray();
                        if (listItemJO["formInputRequired"]!= null && int.Parse(listItemJO["formInputRequired"].ToString()) == 1)
                        {
                            JObject rulesItemJO = new JObject();
                            rulesItemJO.Add("type", "required");
                            if(listItemJO.ContainsKey("formFieldTips") && !listItemJO["formFieldTips"].ToString().Equals(""))
                            {
                                rulesItemJO.Add("message", listItemJO["formFieldTips"]);
                            }
                            rulesJA.Add(rulesItemJO);
                            createFieldItem.Add("rules", rulesJA);
                        }
                        JObject propsJO = new JObject();
                        if (listItemJO["formFieldHint"] != null && !listItemJO["formFieldHint"].ToString().Equals(""))
                        {
                            propsJO.Add("placeholder", listItemJO["formFieldHint"].ToString());
                            createFieldItem.Add("props", propsJO);
                        }
                        createFieldItem.Add("type", listItemJO["formInputType"]);

                        if (listItemJO["formFieldQuestion"] != null && !(listItemJO["formFieldQuestion"].ToString().Equals("")))
                        {
                            createFieldItem.Add("toptips", listItemJO["formFieldQuestion"]);
                        }

                        if (listItemJO["formInputOptions"] != null && listItemJO["formInputOptions"].ToString().StartsWith("{") && listItemJO["formInputOptions"].ToString().EndsWith("}"))
                        {
                            JObject formInputOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJO["formInputOptions"].ToString());
                            foreach (var fioItem in formInputOptionsJObect)
                            {
                                createFieldItem.Add(fioItem.Key.ToString(), fioItem.Value);
                            }
                        }

                        if (listItemJO["fieldValueOptions"]!=null && listItemJO["fieldValueOptions"].ToString().StartsWith("{") && listItemJO["fieldValueOptions"].ToString().EndsWith("}"))
                        {
                            JObject fieldValueOptionsJObect = (JObject)JsonConvert.DeserializeObject(listItemJO["fieldValueOptions"].ToString());
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
                    if (listItemJObect["fieldScopes"] != null && listItemJObect["fieldScopes"].ToString().Contains("view"))
                    {
                        viFieldItem.Add("field", listItemJObect["fieldBinding"]);
                        viFieldItem.Add("label", listItemJObect["fieldLabel"]);

                        if ( listItemJObect["formViewOptions"] != null &&!listItemJObect["formViewOptions"].ToString().Equals(""))
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