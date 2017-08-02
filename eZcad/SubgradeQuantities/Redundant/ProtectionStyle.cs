using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace eZcad.SubgradeQuantities.Redundant
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
}
