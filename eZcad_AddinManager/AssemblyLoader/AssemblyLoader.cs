using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace eZcad.AddinManager
{
    /// <summary> 将程序集及其引用项加载到进程中 </summary>
    /// <remarks>此类是完全从 Revit AddinManager中移植过来并稍微修改的</remarks>
    public class AssemLoader
    {
        #region ---   Fields & Properties

        private List<string> m_refedFolders;
        private Dictionary<string, DateTime> m_copiedFiles;
        private bool m_parsingOnly;
        private static string m_dotnetDir = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

        public static string m_resolvedAssemPath = string.Empty;

        private string m_revitAPIAssemblyFullName;

        private string m_originalFolder;
        public string OriginalFolder
        {
            get
            {
                return this.m_originalFolder;
            }
            set
            {
                this.m_originalFolder = value;
            }
        }

        private string m_tempFolder;
        public string TempFolder
        {
            get
            {
                return this.m_tempFolder;
            }
            set
            {
                this.m_tempFolder = value;
            }
        }

        #endregion

        /// <summary> 构造函数 </summary>
        public AssemLoader()
        {
            this.m_tempFolder = string.Empty;
            this.m_refedFolders = new List<string>();
            this.m_copiedFiles = new Dictionary<string, DateTime>();
        }

        public void CopyGeneratedFilesBack()
        {
            string[] files = Directory.GetFiles(this.m_tempFolder, "*.*", SearchOption.AllDirectories);
            string[] array = files;
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                if (this.m_copiedFiles.ContainsKey(text))
                {
                    DateTime t = this.m_copiedFiles[text];
                    FileInfo fileInfo = new FileInfo(text);
                    if (fileInfo.LastWriteTime > t)
                    {
                        string str = text.Remove(0, this.m_tempFolder.Length);
                        string destinationFilename = this.m_originalFolder + str;
                        FileUtils.CopyFile(text, destinationFilename);
                    }
                }
                else
                {
                    string str2 = text.Remove(0, this.m_tempFolder.Length);
                    string destinationFilename2 = this.m_originalFolder + str2;
                    FileUtils.CopyFile(text, destinationFilename2);
                }
            }
        }

        #region ---  !!! LoadAddinsToTempFolder 将程序集拷贝到新的临时文件夹中

        public Assembly LoadAddinsToTempFolder(string originalFilePath, bool parsingOnly)
        {
            if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
            {
                return null;
            }
            this.m_parsingOnly = parsingOnly;
            this.m_originalFolder = Path.GetDirectoryName(originalFilePath);
            StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
            if (parsingOnly)
            {
                stringBuilder.Append("-Parsing-");
            }
            else
            {
                stringBuilder.Append("-Executing-");
            }
            this.m_tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
            Assembly assembly = this.CopyAndLoadAddin(originalFilePath, parsingOnly);
            if (null == assembly || !this.IsRevitAPIReferenced(assembly))
            {
                return null;
            }
            return assembly;
        }

        private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
        {
            string text = string.Empty;
            if (!FileUtils.FileExistsInFolder(srcFilePath, this.m_tempFolder))
            {
                string directoryName = Path.GetDirectoryName(srcFilePath);
                if (!this.m_refedFolders.Contains(directoryName))
                {
                    this.m_refedFolders.Add(directoryName);
                }
                List<FileInfo> list = new List<FileInfo>();
                text = FileUtils.CopyFileToFolder(srcFilePath, this.m_tempFolder, onlyCopyRelated, list);
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }
                foreach (FileInfo current in list)
                {
                    this.m_copiedFiles.Add(current.FullName, current.LastWriteTime);
                }
            }
            return this.LoadAddin(text);
        }

        /// <summary> 将程序集加载到程序中 </summary>
        /// <param name="filePath">程序集的文件路径</param>
        /// <returns></returns>
        private  Assembly LoadAddin(string filePath)
        {
            Assembly result = null;
            try
            {
                Monitor.Enter(this);

                // 方法一：Revit中的实现方法
                result = Assembly.LoadFile(filePath); // LoadFile方法不会加载此程序集引用的其他程序集，也就是不会加载程序的依赖项。

                // 方法二：zengfy 的实现方法，实验中证明不能替换上面的方法一
                // byte[] buff = File.ReadAllBytes(filePath);  //先将插件拷贝到内存缓冲。一般情况下，当加载的文件大小大于2^32 byte (即4.2 GB），就会出现OutOfMemoryException，在实际测试中的极限值为630MB。
                // result = Assembly.Load(buff); //不能直接通过LoadFrom或者LoadFile，而必须先将插件拷贝到内存，然后再从内存中Load

                // MessageBox.Show(result.FullName);
            }
            finally
            {
                Monitor.Exit(this);
            }
            return result;
        }

        #endregion

        #region ---   HookAssemblyResolve ：以异常处理的方式加载不能正常引用的程序集

        /// <summary>
        /// AssemblyResolve事件在.Net对程序集的解析失败时触发，返回一个Assembly对象。
        /// 因此，我们只要在这个事件的处理程序里手动加载对应目录的dll，并把对应dll的Assembly对象返回， .Net就能正确加载对应的dll了。
        /// </summary>
        /// <remarks></remarks>
        public void HookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
        }

        public void UnhookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
        }

        /// <summary> 在 Execute() 方法中将不能引用到的程序集进行手动加载 </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly result;
            lock (this)
            {
                new AssemblyName(args.Name);
                // 1. 在临时文件夹中搜索程序集所对应的文件：对应于引用时 Copy Local 设置为 true 的程序集
                string text = this.SearchAssemblyFileInTempFolder(args.Name);
                if (File.Exists(text))
                {
                    result = this.LoadAddin(text);
                }
                else  // 2. 在临时文件夹中没搜索到程序集所对应的文件，此时去原始文件夹中搜索
                {
                    text = this.SearchAssemblyFileInOriginalFolders(args.Name);
                    if (string.IsNullOrEmpty(text))
                    {
                        string[] array = args.Name.Split(new char[]
                        {
                            ','
                        });
                        string text2 = array[0];
                        if (array.Length > 1)
                        {
                            string text3 = array[2];
                            if (text2.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                            {
                                text2 = text2.Substring(0, text2.Length - ".resources".Length);
                            }
                            text = this.SearchAssemblyFileInTempFolder(text2);
                            if (File.Exists(text))
                            {
                                result = this.LoadAddin(text);
                                return result;
                            }
                            text = this.SearchAssemblyFileInOriginalFolders(text2);
                        }
                    }

                    if (string.IsNullOrEmpty(text))  // 3. 在临时文件夹与原始文件夹都没搜索到程序集所对应的文件，则跳出对话框，让用户自己选择
                    {
                        using (AssemblySelectorForm assemblySelectorForm = new AssemblySelectorForm(args.Name))
                        {
                            if (assemblySelectorForm.ShowDialog() != DialogResult.OK)
                            {
                                result = null;
                                return result;
                            }
                            text = assemblySelectorForm.m_resultPath;
                        }
                    }
                    result = this.CopyAndLoadAddin(text, true);
                }
            }
            return result;
        }


        /// <summary> 在临时文件夹中根据程序集名搜索对应的文件：对应于引用时 Copy Local 设置为 true 的程序集 </summary>
        /// <param name="assemName"></param>
        /// <returns></returns>
        private string SearchAssemblyFileInTempFolder(string assemName)
        {
            string[] array = new string[]
            {
                ".dll",
                ".exe"
            };
            string text = string.Empty;
            string str = assemName.Substring(0, assemName.IndexOf(','));
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string str2 = array2[i];
                text = this.m_tempFolder + "\\" + str + str2;
                if (File.Exists(text))
                {
                    return text;
                }
            }
            return string.Empty;
        }

        private string SearchAssemblyFileInOriginalFolders(string assemName)
        {
            string[] array = new string[]
            {
                ".dll",
                ".exe"
            };
            string text = string.Empty;
            string text2 = assemName.Substring(0, assemName.IndexOf(','));
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string str = array2[i];
                text = AssemLoader.m_dotnetDir + "\\" + text2 + str;
                if (File.Exists(text))
                {
                    string result = text;
                    return result;
                }
            }
            string[] array3 = array;
            for (int j = 0; j < array3.Length; j++)
            {
                string str2 = array3[j];
                foreach (string current in this.m_refedFolders)
                {
                    text = current + "\\" + text2 + str2;
                    if (File.Exists(text))
                    {
                        string result = text;
                        return result;
                    }
                }
            }
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                string path = directoryInfo.Parent.FullName + "\\Regression\\_RegressionTools\\";
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    for (int k = 0; k < files.Length; k++)
                    {
                        string text3 = files[k];
                        if (Path.GetFileNameWithoutExtension(text3).Equals(text2, StringComparison.OrdinalIgnoreCase))
                        {
                            string result = text3;
                            return result;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            int num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
            if (num != -1)
            {
                assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
                return this.SearchAssemblyFileInOriginalFolders(assemName);
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 检查 RevitAPI.dll 是否被引用
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private bool IsRevitAPIReferenced(Assembly assembly)
        {
            return true;
            // 以上为 zengfy 添加

            if (string.IsNullOrEmpty(this.m_revitAPIAssemblyFullName))
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Assembly assembly2 = assemblies[i];
                    if (string.Compare(assembly2.GetName().Name, "RevitAPI", true) == 0)
                    {
                        this.m_revitAPIAssemblyFullName = assembly2.GetName().Name;
                        break;
                    }
                }
            }
            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            for (int j = 0; j < referencedAssemblies.Length; j++)
            {
                AssemblyName assemblyName = referencedAssemblies[j];
                if (this.m_revitAPIAssemblyFullName == assemblyName.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
