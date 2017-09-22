using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using eZstd.MarshalReflection;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace eZcad.SubgradeQuantity
{
    public partial class CadAddinSetup : Form
    {
        /// <summary> 构造函数 </summary>
        public CadAddinSetup()
        {
            InitializeComponent();
            //
            ListInstalledCadVersions();
        }

        private void ListInstalledCadVersions()
        {
            foreach (var vp in CadVersionKey)
            {
                var vers = vp.Key;
                var keyPath = vp.Value;
                //
                var cadExist = IsCadVersionExist(keyPath);
                if (cadExist)
                {
                    var checkbox = new CheckBox() { Text = vers, Tag = keyPath };
                    flowLayoutPanel1.Controls.Add(checkbox);
                }
            }
        }

        #region ---   界面操作


        private void checkBox_ChooseAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var c in flowLayoutPanel1.Controls)
            {
                var chk = c as CheckBox;
                chk.Checked = checkBox_ChooseAll.Checked;
            }
        }

        private void btn_AddinLoad_Click(object sender, System.EventArgs e)
        {
            var loadedApps = new List<string>();
            var dllPath = GetAddinDllPath();
            var addinName = GetAddinName();
            foreach (var c in flowLayoutPanel1.Controls)
            {
                var check = c as CheckBox;
                if (check.Checked)
                {
                    var vers = check.Text;
                    if (CadVersionKey.ContainsKey(vers))
                    {
                        var keyPath = CadVersionKey[vers];
                        try
                        {

                            RegApp(keyPath, dllPath, addinName);
                            loadedApps.Add(vers);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"安装插件到 {vers} 中出错，原因：" + "\r\n" + ex.Message + "\r\n"
                                + "\r\n" + "请尝试以管理员身份运行安装程序",
                                "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            // 列出成功安装的版本
            if (loadedApps.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append(loadedApps[0]);
                for (int i = 1; i < loadedApps.Count; i++)
                {
                    sb.Append($", {loadedApps[i]}");
                }
                MessageBox.Show($"加载成功的 AutoCAD 版本：\r\n{sb.ToString()}",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // 关闭窗口
            Close();
        }

        private void btn_AddinUnLoad_Click(object sender, System.EventArgs e)
        {
            var unloadedApps = new List<string>();
            var addinName = GetAddinName();
            foreach (var c in flowLayoutPanel1.Controls)
            {
                var check = c as CheckBox;
                if (check.Checked)
                {
                    var vers = check.Text;
                    if (CadVersionKey.ContainsKey(vers))
                    {
                        var cadKeyPath = CadVersionKey[vers];
                        try
                        {
                            if (IsAddinExist(cadKeyPath, addinName))
                            {
                                UnRegApp(cadKeyPath, addinName);
                                unloadedApps.Add(vers);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"从 {vers} 中卸载插件时出错，原因：" + "\r\n" + ex.Message + "\r\n" + "\r\n" + "请尝试以管理员身份运行安装程序",
                                "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            // 列出成功安装的版本
            if (unloadedApps.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append(unloadedApps[0]);
                for (int i = 1; i < unloadedApps.Count; i++)
                {
                    sb.Append($", {unloadedApps[i]}");
                }
                MessageBox.Show($"成功卸载的 AutoCAD 版本：{sb.ToString()}",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // 关闭窗口
            Close();
        }

        #endregion

        #region ---   插件的具体信息

        public const string AddinDescription = @"路基工程量统计程序";

        private string GetAddinDllPath()
        {
            var assPath = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName;
            var assDir = new FileInfo(assPath).Directory;
            var p = Path.Combine(assDir.FullName, "SubgradeQuantity.dll");
            // MessageBox.Show("安装的插件位置：" + p);
            return p;
        }

        private string GetAddinName()
        {
            // 考虑不同插件版本的编号
            return @"SubgradeQuantity_10";
        }

        #endregion

        #region ---   修改注册表

        /// <summary> 不同 AutoCAD 版本所对应的注册表位置 </summary>
        public static Dictionary<string, string> CadVersionKey = new Dictionary<string, string>()
        {
            {"2006中文版", @"SOFTWARE\Autodesk\AutoCAD\R16.2\ACAD-4001:804"},
            {"2006英文版", @"SOFTWARE\Autodesk\AutoCAD\R16.2\ACAD-4001:409"},
            {"2007中文版", @"SOFTWARE\Autodesk\AutoCAD\R17.0\ACAD-5001:804"},
            {"2007英文版", @"SOFTWARE\Autodesk\AutoCAD\R17.0\ACAD-5001:409"},
            {"2008中文版", @"SOFTWARE\Autodesk\AutoCAD\R17.1\ACAD-6001:804"},
            {"2008英文版", @"SOFTWARE\Autodesk\AutoCAD\R17.1\ACAD-6001:409"},
            {"2009中文版", @"SOFTWARE\Autodesk\AutoCAD\R17.2\ACAD-7001:804"},
            {"2009英文版", @"SOFTWARE\Autodesk\AutoCAD\R17.2\ACAD-7001:409"},
            {"2010中文版", @"SOFTWARE\Autodesk\AutoCAD\R18.0\ACAD-8001:804"},
            {"2010英文版", @"SOFTWARE\Autodesk\AutoCAD\R18.0\ACAD-8001:409"},
            {"2012中文版", @"SOFTWARE\Autodesk\AutoCAD\R18.2\ACAD-A001:804"},
            {"2012英文版", @"                                             "},
            {"2014中文版", @"SOFTWARE\Autodesk\AutoCAD\R19.1\ACAD-D001:804"},
            {"2014英文版", @"                                             "},
            {"2015中文版", @"SOFTWARE\Autodesk\AutoCAD\R20.0\ACAD-E001:804"},
            {"2015英文版", @"                                             "},
            {"2016中文版", @"SOFTWARE\Autodesk\AutoCAD\R20.1\ACAD-F001:804"},
            {"2016英文版", @"                                             "},
        };


        private static RegistryKey GetLocalMachineKey(RegistryView? view)
        {
            if (view == null)
            {
                return Registry.LocalMachine;
            }
            else
            {
                return RegistryHandler.GetRegistryKeyWithRegView(RegistryHive.LocalMachine, view);
            }
        }


        /// <summary>
        /// 检查某版本的 AutoCAD 是否安装
        /// </summary>
        /// <param name="cadKeyPath">某版本 AutoCAD 所对应的注册表位置 </param>
        /// <returns></returns>
        private static bool IsCadVersionExist(string cadKeyPath)
        {

            bool result;
            try
            {
                var localMachine = GetLocalMachineKey(RegistryView.Registry64);

                var registryKey = localMachine.OpenSubKey(cadKeyPath);
                // AcadLocation 是每个 AutoCAD 软件都会有的一个键
                string text = registryKey.GetValue(@"AcadLocation").ToString();
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 检查某版本的 AutoCAD 中的指定插件是否安装
        /// </summary>
        /// <param name="cadKeyPath">某版本 AutoCAD 所对应的注册表位置 </param>
        /// <returns></returns>
        private static bool IsAddinExist(string cadKeyPath, string addinName)
        {

            bool result = false;
            try
            {
                var localMachine = GetLocalMachineKey(RegistryView.Registry64);

                var registryKey = localMachine.OpenSubKey($"{cadKeyPath}\\Applications\\{addinName}");
                if (registryKey != null)
                {
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 通过修改注册表的方式自动加载程序集
        /// </summary>
        /// <param name="cadKeyPath">程序集要注册在注册表的哪个位置</param>
        /// <param name="dllPath">要自动加载的程序集的绝对地址</param>
        /// <param name="addinName">插件在注册表中的名称</param>
        public static void RegApp(string cadKeyPath, string dllPath, string addinName)
        {
            var localMachine = GetLocalMachineKey(RegistryView.Registry64);
            RegistryKey registryKey = localMachine.OpenSubKey(cadKeyPath + "\\Applications", true);
            RegistryKey registryKey2 = registryKey.CreateSubKey(addinName);
            registryKey2.SetValue("DESCRIPTION", AddinDescription, RegistryValueKind.String);
            registryKey2.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
            registryKey2.SetValue("LOADER", dllPath, RegistryValueKind.String);
            registryKey2.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            registryKey.Close();
        }

        public static void UnRegApp(string cadKeyPath, string addinName)
        {
            var localMachine = GetLocalMachineKey(RegistryView.Registry64);
            RegistryKey registryKey = localMachine.OpenSubKey(cadKeyPath + "\\Applications", true);
            registryKey.DeleteSubKeyTree(addinName);
        }

        #endregion

    }
}