## 转换成curd setting API 使用

```
POST /createpage

    - endpoint: 必填
    - originApi: 必填 -- 如返回详情格式api
    - createPageApi: 必填 -- 创建页面
      - api: 必填
      - data: 必填
    - addFieldApi: 必填 -- 添加页面字段，这里根据 originApi 返回的信息进行处理
    - lowFilterss: 可填 -- 搜索栏
    - lowActions:  可填 -- 添加按钮
    - lowOperations:  可填  --列表操作栏按钮
    - token: 可选 --如api需要权限访问即填上token

//提交数据格式

{
    "endpoint": "https://api.xiaojiuban.smallsaas.cn",
    "originApi": "/api/crud/product/productCategoryies/17",
    "createPageApi": { 
        "api": "/api/crud/lowMainPage/lowMainPages",  
        "data": {
            "apiEndpoint": "/api/crud/fieldModel/fieldModels",
            "columnAlign": "",
            "contentItemContainerStyle": "",
            "contentItems": "",
            "contentLayout": "Grid",
            "formAddFields": "",
            "formAddTitle": "新增字段模板",
            "formDefaultContentLayout": "TitleContent",
            "formDefaultWidth": 0,
            "formEditFields": "",
            "formEditTitle": "编辑字段模板",
            "formViewFields": "",
            "formViewTitle": "查看字段模板",
            "listFields": "",
            "listOperationFields": "",
            "pageTitle": "字段模板"
        }
    },
    "addFieldApi": "/api/crud/lowFields/lowFieldses", 
    "lowFilterss":{
        "api": "/api/crud/lowFilters/lowFilterses",
        "filters": [
            {
                "contentLayout": "Grid", 
                "fieldName": "search", 
                "defaultSearchHint": "请输入", 
                "searchFields": "", 
                "fieldTitle": "分类名称", 
                "fieldType": "search"
            }
        ]
    },
    "lowActions": { 
        "api": "/api/crud/lowActions/lowActionses",
        "actions": [
            { "title": "添加", "path": "/fieldTemplate/fieldTemplate-add"}
        ]
    },
    "lowOperations": {
        "api": "/api/crud/lowOperations/lowOperationses",
        "actions": [
            { "title": "详情", "path": "/fieldTemplate/fieldTemplate-view", "type": "path", "outside": 1},
            { "title": "编辑", "path": "/fieldTemplate/fieldTemplate-edit", "type": "path"},
            { "title": "删除", "path": "", "type": "delete"}
        ]
    },
    "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJvcmdJZCI6IjEwMDAwMDAwMDAwMDAwMDAwMSIsInVzZXJJZCI6Ijg3NjcwODA4MjQzNzE5NzgyNyIsInVzZXJUeXBlIjowLCJiVXNlclR5cGUiOiJTWVNURU0iLCJ0ZW5hbnRPcmdJZCI6MTAwMDAwMDAwMDAwMDAwMDAxLCJhY2NvdW50IjoiYWRtaW4iLCJleHRyYVVzZXJUeXBlIjowLCJpYXQiOjE2MzIzODI1NTMsImp0aSI6Ijg3NjcwODA4MjQzNzE5NzgyNyIsInN1YiI6ImFkbWluIiwiZXhwIjoxNjMyNjQxNzUzfQ.0i3FJd3loudqAqz3m8H0S_xCCzT8dDH8AjdGpgjvWKIzpMKK1jacroljNCiaxdsb7kD3bsVOJfPHGHbX_m4aMA"
}

```
