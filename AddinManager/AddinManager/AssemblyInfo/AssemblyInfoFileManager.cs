using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoCADDev.AddinManager;
using AutoCADDev.AddinManager;
using AutoCADDev.ExternalCommand;

namespace AutoCADDev.AddinManager
{
    internal class AssemblyInfoFileManager
    {
        private static readonly string _addinManagerDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
        public static string AddinManagerDirectory
        {
            get { return _addinManagerDirectory; }
        }

        private const string SerializedFileName = "LoadedAssemblies.am";

        #region ---   从文件反序列化

        /// <summary> 将外部 二进制文件 中的字符进行反序列化 </summary>
        /// <remarks>对于CAD.NET的开发，不要在 IExtensionApplication.Initialize() 方法中执行此操作，否则即使在Initialize时可以正常序列化，
        /// 但是在调用ExternalCommand时还是会出bug，通常的报错为：没有为该对象定义无参数的构造函数。 </remarks>
        public static Dictionary<AddinManagerAssembly, List<MethodInfo>> GetInfosFromFile()
        {
            Dictionary<AddinManagerAssembly, List<MethodInfo>> nodesInfo;
            nodesInfo = new Dictionary<AddinManagerAssembly, List<MethodInfo>>(new AssemblyComparer());

            string infoPath = Path.Combine(AddinManagerDirectory, SerializedFileName);
            if (File.Exists(infoPath))
            {
                FileStream fs = new FileStream(infoPath, FileMode.Open, FileAccess.Read);
                AssemblyInfos infos = BinarySerializer.DeCode(fs) as AssemblyInfos;

                // 提取数据
                nodesInfo = DeserializeAssemblies(infos);
                //
                fs.Close();
                fs.Dispose();
            }

            return nodesInfo;
        }


        private static Dictionary<AddinManagerAssembly, List<MethodInfo>> DeserializeAssemblies(
            AssemblyInfos amInfos)
        {
            Dictionary<AddinManagerAssembly, List<MethodInfo>> nodesInfo;
            nodesInfo = new Dictionary<AddinManagerAssembly, List<MethodInfo>>(new AssemblyComparer());
            //
            if (amInfos != null)
            {
                foreach (string assemblyPath in amInfos.AssemblyPaths)
                {
                    if (File.Exists(assemblyPath))
                    {
                        // 将每一个程序集中的外部命令提取出来
                        List<MethodInfo> m = ExternalCommandHandler.LoadExternalCommandsFromAssembly(assemblyPath);
                        if (m.Any())
                        {
                            Assembly ass = m[0].DeclaringType.Assembly;
                            AddinManagerAssembly amAssembly = new AddinManagerAssembly(assemblyPath, ass);
                            nodesInfo.Add(amAssembly, m);
                        }
                    }
                }
            }
            return nodesInfo;
        }

        #endregion

        #region ---   序列化到文件

        public static void SaveAssemblyInfosToFile(
            Dictionary<AddinManagerAssembly, List<MethodInfo>> nodesInfo)
        {
            List<string> assemblyPaths = nodesInfo.Select(r => r.Key.Path).ToList();
            AssemblyInfos amInfos = new AssemblyInfos() { AssemblyPaths = assemblyPaths.ToArray() };

            // 序列化

            string infoPath = Path.Combine(AddinManagerDirectory, SerializedFileName);

            // 保存到物理存储中
            FileStream fs = new FileStream(infoPath, FileMode.Create, FileAccess.Write);
            BinarySerializer.EnCode(fs, amInfos);

            //
            fs.Close();
            fs.Dispose();
        }
        
        #endregion

    }
}
