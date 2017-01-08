using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eZcad.AddinManager
{
    /// <summary> 将 AddinManager中的程序信与外部命令信息提取为可以进行序列化的信息 </summary>
    [Serializable()]
    internal class AssemblyInfos
    {
        //  public List<string> AssemblyPaths;
        public string[] AssemblyPaths;
    }
}
