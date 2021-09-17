## 转换成curd setting API 使用

    - endpoint: 必填
    - originApi: 必填
    - createPageApi: 必填
    - addFieldApi: 必填
    - token: 可选
```
POST /createpage

//提交数据格式

{
    "endpoint": "https://api.xiaojiuban.smallsaas.cn",
    "originApi": "/api/crud/product/productCategoryies/20",
    "createPageApi": "/api/crud/lowMainPage/lowMainPages",
    "addFieldApi": "/api/crud/lowFields/lowFieldses",
    "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJvcmdJZCI6IjEwMDAwMDAwMDAwMDAwMDAwMSIsInVzZXJJZCI6Ijg3NjcwODA4MjQzNzE5NzgyNyIsInVzZXJUeXBlIjowLCJiVXNlclR5cGUiOiJTWVNURU0iLCJ0ZW5hbnRPcmdJZCI6MTAwMDAwMDAwMDAwMDAwMDAxLCJhY2NvdW50IjoiYWRtaW4iLCJleHRyYVVzZXJUeXBlIjowLCJpYXQiOjE2MzE3NjEyOTUsImp0aSI6Ijg3NjcwODA4MjQzNzE5NzgyNyIsInN1YiI6ImFkbWluIiwiZXhwIjoxNjMyMDIwNDk1fQ.hF8nSjFCiC7KPUNM5L6sekixB3XzGVKWa3wDdQ_irAilkC-APBV2qn5z-qJWUzty_vtM8c_3_tqEWmmC-S0XNA"
}

```
