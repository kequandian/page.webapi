using System;
using System.Runtime.InteropServices;

namespace PageConfig.WebApi.Method
{
    #region 公共方法类
    public static class PublicFunction
    {
        //地址系统变量名称
        static readonly string AddressPathName = "SIDEAPI_ENDPOINT";

        //地址
        static string address = string.Empty;

        #region 获取地址系统变量
        public static string getAddress()
        {
            if (string.IsNullOrEmpty(address))
                try
                {
                    address = Environment.GetEnvironmentVariable(AddressPathName, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.Process);
                }
                catch (Exception ex)
                {

                }
            return address;
        }
        #endregion

    }
    #endregion
}
