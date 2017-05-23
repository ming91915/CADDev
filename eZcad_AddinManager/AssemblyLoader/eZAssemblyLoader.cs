using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eZcad.AddinManager;

namespace eZcad.AssemblyLoader
{
    /// <summary>
    /// 为了将要加载的程序集中的外部命令显示到<see cref="form_AddinManager"/>窗口中，
    /// 在加载程序集时（只是加载程序集，而不是调用程序集中的外部命令时），可能会出现“未能加载文件或程序集“Accessibility,Version=2.0.0.0，Culture=neutral,PublicKeyToken=b03f5f7f11d50a3a”或它的某一个依赖项。系统找不到指定文件。”这种报错。
    /// 此时要利用  AppDomain.CurrentDomain.AssemblyResolve  事件进行解决。
    /// </summary>
    /// <remarks>此类由Zengfy参照<see cref="AssemLoader"/>类自行编写，即为了解决在 AutoCAD 2016 中，AddinManager Load 程序集时，总是出现
    /// “未能加载文件或程序集“Accessibility,Version=2.0.0.0，Culture=neutral,PublicKeyToken=b03f5f7f11d50a3a”或它的某一个依赖项。系统找不到指定文件。”
    /// 这种报错的问题。在AutoCAD 2014 的开发中并不需要。</remarks>
    public class eZAssemblyLoader
    {
        #region ---   Fields
        /// <summary> 要进行加载的主程序集所在的文件夹 </summary>
        private readonly string _srcDir;
        /// <summary> 要进行加载的主程序集的名称，比如 eZcad.dll </summary>
        private readonly string _srcDll;

        /// <summary> 是否为当前程序域中添加了新的程序集 </summary>
        private bool _newAssemblyAddedToAppDomain;
        /// <summary> 当前程序域中已经加载的程序集 </summary>
        private Assembly[] _appDomainAssemblies;

        #endregion

        public eZAssemblyLoader(string srcDllPath)
        {
            _srcDir = Path.GetDirectoryName(srcDllPath);
            _srcDll = Path.GetFileName(srcDllPath);
        }

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
        /// <returns>返回的程序集即作为调用的程序集，即使返回的程序集并不是想要的那一个。</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_appDomainAssemblies == null || _newAssemblyAddedToAppDomain)
            {
                _appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            _newAssemblyAddedToAppDomain = false;
            Assembly result = null;
            lock (this)
            {
                // 1. 在临时文件夹中搜索程序集所对应的文件：对应于引用时 Copy Local 设置为 true 的程序集
                var wantedDll = args.Name;
                string dllPath = SearchAssemblyFileInDllFolders(wantedDll);
                if (File.Exists(dllPath))
                {
                    result = this.LoadAddin(dllPath);
                }
                else // 2. 
                {
                    // wantedDll 可能有两种格式：
                    // 1. eZcad.resources, Version = 1.0.0.0, Culture = zh - CN, PublicKeyToken = null
                    // 2. eZstd, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null
                    string[] array = wantedDll.Split(',');
                    string dllNm = array[0];
                    if (array.Length > 1)
                    {
                        string cult = array[2];
                        var dllName = dllNm.Substring(0, dllNm.Length - ".resources".Length);
                        if (dllNm.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) &&
                            !cult.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase)) // neutral 表示已经加载了的程序集
                        {
                            // 从当前程序域中已经加载了的程序集中去进行匹配
                            foreach (var ass in _appDomainAssemblies)
                            {
                                var array2 = ass.FullName.Split(',');
                                // 1. 首先进行基本的程序集名称的匹配
                                if (array2[0] == dllName)
                                {
                                    // 2. 比较版本号 与 强命名
                                    if (array2[1] == array[1] && array2[3] == array[3])
                                    {
                                        return ass;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 从当前程序域中已经加载了的程序集中去进行匹配
                            var res = _appDomainAssemblies.FirstOrDefault(r => r.FullName == wantedDll);
                            if (res != null)
                            {
                                return res;
                            }
                        }
                        dllPath = this.SearchAssemblyFileInDllFolders(dllName);
                        if (File.Exists(dllPath))
                        {
                            result = this.LoadAddin(dllPath);
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary> 源程序集的绝对路径 </summary>
        /// <param name="srcDllPath"></param>
        /// <param name="assemName"></param>
        /// <returns></returns>
        private string SearchAssemblyFileInDllFolders(string assemName)
        {
            string res;
            res = Path.Combine(_srcDir, assemName);
            if (File.Exists(res))
            {
                return res;
            }
            string[] array = new string[]
            {
                ".dll",
                ".exe"
            };
            foreach (var ext in array)
            {
                res = Path.Combine(_srcDir, assemName + ext);
                if (File.Exists(res))
                {
                    return res;
                }
            }
            return res;
        }
        #endregion


        /// <summary> 将程序集加载到程序中 </summary>
        /// <param name="filePath">程序集的文件路径</param>
        /// <returns></returns>
        private Assembly LoadAddin(string filePath)
        {
            Assembly result = null;
            try
            {
                Monitor.Enter(this);

                // 方法一：Revit中的实现方法
                result = Assembly.LoadFile(filePath); // LoadFile方法不会加载此程序集引用的其他程序集，也就是不会加载程序的依赖项。
                if (result != null)
                {
                    _newAssemblyAddedToAppDomain = true;
                }
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

    }
}
