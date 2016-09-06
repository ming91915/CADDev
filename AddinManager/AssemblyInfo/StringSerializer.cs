using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace AutoCADDev.AssemblyInfo
{
        /// <summary>
        /// 将任意一个有 Serializable标记的类以二进制转换器将类中所有数据与字符串间的相互序列化。
        /// 即可以将类中的数据（包括数组）序列化为字符，还可以将序列化的字符反序列化为一个类。
        /// </summary>
        public static class StringSerializer
        {
            /// <summary>
            /// Encode arbitrary .NET serialisable object
            /// into binary data encodes as base64 string.
            /// </summary>
            public static string Encode64(object obj)
            {
                // serialize into binary stream
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                f.Serialize(stream, obj);
                stream.Position = 0;

                // 将二进制数据编码为base64的字符串
                int n = (int)stream.Length;
                byte[] buf = new byte[n - 1 + 1];
                stream.Read(buf, 0, n);
                // 如果想将二进制字节数组转直接换成字符串，可以使用具有8位编码的字符集转换，但不能使用其它字符集，比如Unicode、GB2312.
                return Convert.ToBase64String(buf);
            }

            /// <summary>
            /// Decode arbitrary .NET serialisable object
            /// from binary data encoded as base64 string.
            /// </summary>
            public static object Decode64(string s64)
            {
                // decode string back to binary data:
                MemoryStream s = new MemoryStream(Convert.FromBase64String(s64));
                s.Position = 0;

                // deserialize:
                BinaryFormatter f = new BinaryFormatter();
                //f.AssemblyFormat = FormatterAssemblyStyle.Simple;
                // add this line below to avoid the "unable to find assembly" issue:
                f.Binder = new StringSerializer.ZengfyLinkBinder();
                return f.Deserialize(s);
            }

            /// <summary>
            /// 为了解决SerializationException，方法之一是确保此assembly放置在与acad.exe 或 revit.exe相同的文件夹中，
            /// 另一个方法就是实现一个像这样的类。
            /// </summary>
            /// <remarks>
            ///  Resolve System.Runtime.Serialization.SerializationException, Message =
            /// "Unable to find assembly 'StoreData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'."
            /// One solution is to ensure that assembly resides in same directory as acad.exe or revit.exe,
            /// the other is to implement a class such as this, cf.
            /// http://www.codeproject.com/soap/Serialization_Samples.asp
            /// </remarks>
            private sealed class ZengfyLinkBinder : SerializationBinder
            {
                public override Type BindToType(string assemblyName, string typeName)
                {
                    return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
                }
            }
        }
    }
