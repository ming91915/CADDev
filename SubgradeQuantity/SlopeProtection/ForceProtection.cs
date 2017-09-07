using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    /// <summary> 用来对一个边坡进行防护方式强制设置的相关参数 </summary>
    public class ForceProtection
    {
        public string ProtMethod { get; }
        
        /// <summary> 如果此属性的值为 null，则表示对此边坡中的所有子边坡都设置指定的防护方式 </summary>
        public int[] SlopeLevels { get; }

        public ForceProtection(string protMethod, int[] slopeLevels)
        {
            ProtMethod = protMethod;
            SlopeLevels = slopeLevels;
        }
    }
}
