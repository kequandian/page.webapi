## 转换成curd setting API 使用

```
POST /createpage

    - endpoint: 必填
    - originApi: 必填
    - createPageApi: 必填
    - addFieldApi: 必填
    - lowActions:  可填
    - lowOperations:  可填
    - token: 可选

//提交数据格式

{
    "endpoint": "https://api.xiaojiuban.smallsaas.cn",
    "originApi": "/api/crud/product/productCategoryies/17",
    "createPageApi": "/api/crud/lowMainPage/lowMainPages",
    "addFieldApi": "/api/crud/lowFields/lowFieldses",
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
