using System;
using System.Runtime.InteropServices;

namespace PageConfig.WebApi.Method
{
    #region 公共方法类
    public static class PublicFunction
    {
        static readonly string AddressPathName = "SIDEAPI_ENDPOINT";

        #region 获取地址系统变量
        public static string getAddress()
        {
            string address = string.Empty;
            try
            {
                Environment.GetEnvironmentVariable(AddressPathName, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.Process);
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
