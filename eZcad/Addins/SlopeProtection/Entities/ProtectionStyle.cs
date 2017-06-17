using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace eZcad.Addins.SlopeProtection
{

    public enum ProtectionStyle : short
    {
        /// <summary> 挂网喷锚（6m锚杆） </summary>
        挂网喷锚6,

        /// <summary> 锚杆格梁 </summary>
        锚杆网格梁,

        /// <summary> 浆砌片石 </summary>
        浆砌片石,

        /// <summary> 边坡防护1 </summary>
        边坡防护1,

        /// <summary> 边坡防护2 </summary>
        边坡防护2,

        /// <summary> 边坡防护3 </summary>
        边坡防护3,
    }

    public static partial class ProtectionUtils
    {
        /// <summary> 可用的防护形式 </summary>
        public static readonly Dictionary<ProtectionStyle, string> AvailableProtections = new Dictionary
            <ProtectionStyle, string>
        {
            {ProtectionStyle.挂网喷锚6, "挂网喷锚（6m锚杆）"},
            {ProtectionStyle.锚杆网格梁, "锚杆网格梁"},
            {ProtectionStyle.浆砌片石, "浆砌片石"},
            {ProtectionStyle.边坡防护1, "边坡防护1"},
            {ProtectionStyle.边坡防护2, "边坡防护2"},
            {ProtectionStyle.边坡防护3, "边坡防护3"},
        };
    }
}
