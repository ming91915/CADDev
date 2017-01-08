using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace eZcad.AddinManager
{
    /// <summary>
    /// 在.NET中，我们可以将对象序列化从而保存对象的状态到内存或者磁盘文件中，或者分布式应用程序中用于系统通信，，这样就有可能做出一个“对象数据库”了。
    /// 一般来说，二进制序列化的效率要高，所获得的字节数最小。
    /// </summary>
    /// <remarks></remarks>
    internal static class BinarySerializer
    {
        /// <summary>
        /// 将任意一个声明为Serializable的类或者其List等集合中的数据，以二进制的格式保存到对应的流文件中。
        /// </summary>
        /// <param name="fs">推荐使用FileStream对象。此方法中不会对Stream对象进行Close。</param>
        /// <param name="Data">要进行保存的可序列化对象</param>
        /// <remarks></remarks>
        public static void EnCode(Stream fs, object Data)
        {
            BinaryFormatter bf = new BinaryFormatter(); // 最关键的对象，用来进行类到二进制的序列化与反序列化操作
            bf.Serialize(fs, Data);
        }

        /// <summary>
        /// 从二进制流文件中，将其中的二进制数据反序列化为对应的类或集合对象。
        /// </summary>
        /// <param name="fs">推荐使用FileStream对象。此方法中不会对Stream对象进行Close。</param>
        /// <returns>此二进制流文件所对应的可序列化对象</returns>
        /// <remarks></remarks>
        public static object DeCode(Stream fs)
        {
            BinaryFormatter bf = new BinaryFormatter();
            object dt = bf.Deserialize(fs);
            return dt;
        }
    }
}
