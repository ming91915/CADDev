using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutoCADDev.AddinManager
{
    /// <summary> 比较两个 Assembly 是否表示同一个对象 </summary>
    /// <remarks>在比较的过程中，会先对GetHashCode的返回值进行比较，如果返回的HashCode是相同的，则再进行 Equals 比较；
    /// 如果返回的HashCode值不同，则直接认为Equals为false。</remarks>  
    internal class AddinManagerAssembly
    {
        public readonly string Path;
        public readonly Assembly Assembly;

        public AddinManagerAssembly(string path, Assembly assembly)
        {
            Path = path;
            Assembly = assembly;
        }
    }

    /// <summary> 比较两个 Assembly 是否表示同一个对象 </summary>
    /// <remarks>在比较的过程中，会先对GetHashCode的返回值进行比较，如果返回的HashCode是相同的，则再进行 Equals 比较；
    /// 如果返回的HashCode值不同，则直接认为Equals为false。</remarks>
    internal class AssemblyComparer : IEqualityComparer<AddinManagerAssembly>
    {
        public bool Equals(AddinManagerAssembly x, AddinManagerAssembly y)
        {
            if (x.Path != y.Path)  // 保证是同一个dll文件
            {
                return false;
            }
            //
            return true;
        }

        public int GetHashCode(AddinManagerAssembly obj)
        {
            int hcode = obj.Path.GetHashCode();

            return hcode;
        }
    }

}
