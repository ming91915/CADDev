using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
namespace AutoCADDev.AddinManager
{
    class AssemblyInfoSettings : ApplicationSettingsBase
    {
        /// <summary> 将 AddinManager 中的程序集信息序列化后的字符 </summary>
        [UserScopedSetting(), DefaultSettingValue(null)]
        public string AssemblyInfoSerial
        {
            get
            {
                return this["AssemblyInfoSerial"] as string;
            }
            set
            {
                this["AssemblyInfoSerial"] = value;

            }
        }
    }


}
