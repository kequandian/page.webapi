using System;

namespace PageConfig.WebApi.Controllers.ApiHandle
{
    public class SettingToFieldHandle
    {
        public SettingToFieldHandle()
        {
        }

        public int matchOutside(bool value)
        {
            return value ? 1 : 0;
        }

    }
}
