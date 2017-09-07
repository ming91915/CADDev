using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace eZcad.SubgradeQuantity.ApplicationSetup
{
    public partial class ApplicationSetup : Form
    {
        private readonly CheckBox[] myCheckBox = new CheckBox[10];

        private readonly string[] LocationString = new string[10];

        /// <summary> 构造函数 </summary>
        public ApplicationSetup()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myCheckBox[0] = checkBox0;
            myCheckBox[1] = checkBox1;
            myCheckBox[2] = checkBox2;
            myCheckBox[3] = checkBox3;
            myCheckBox[4] = checkBox4;
            myCheckBox[5] = checkBox5;
            myCheckBox[6] = checkBox6;
            myCheckBox[7] = checkBox7;
            myCheckBox[8] = checkBox8;
            myCheckBox[9] = checkBox9;
            LocationString[0] = "SOFTWARE\\Autodesk\\AutoCAD\\R16.2\\ACAD-4001:804";
            LocationString[1] = "SOFTWARE\\Autodesk\\AutoCAD\\R16.2\\ACAD-4001:409";
            LocationString[2] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:804";
            LocationString[3] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.0\\ACAD-5001:409";
            LocationString[4] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.1\\ACAD-6001:804";
            LocationString[5] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.1\\ACAD-6001:409";
            LocationString[6] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.2\\ACAD-7001:804";
            LocationString[7] = "SOFTWARE\\Autodesk\\AutoCAD\\R17.2\\ACAD-7001:409";
            LocationString[8] = "SOFTWARE\\Autodesk\\AutoCAD\\R18.0\\ACAD-8001:804";
            LocationString[9] = "SOFTWARE\\Autodesk\\AutoCAD\\R18.0\\ACAD-8001:409";
            for (int i = 0; i < 10; i++)
            {
                myCheckBox[i].Enabled = IsRegeditItemExist(LocationString[i], "AcadLocation");
            }
        }

        private bool IsRegeditItemExist(string keypath, string keyname)
        {
            bool result;
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keypath);
                string text = registryKey.GetValue(keyname).ToString();
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool flag = false;
            for (int i = 0; i < 10; i++)
            {
                if (myCheckBox[i].Enabled && myCheckBox[i].Checked)
                {
                    string location = Thread.GetDomain().BaseDirectory + "ChangeFonts.dll";
                    RegApp(LocationString[i], location);
                    flag = true;
                }
            }
            if (flag)
            {
                MessageBox.Show("安装成功");
            }
            else
            {
                MessageBox.Show("安装失败");
            }
        }

        #region ---   注册表的读写


        public void RegApp(string keypath, string Location)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keypath + "\\Applications", true);
            RegistryKey registryKey2 = registryKey.CreateSubKey("KAKANIMOTools");
            registryKey2.SetValue("DESCRIPTION", "初始化.NET程序", RegistryValueKind.String);
            registryKey2.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
            registryKey2.SetValue("LOADER", Location, RegistryValueKind.String);
            registryKey2.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            registryKey.Close();
        }

        public static void UnRegApp(string keypath)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keypath + "\\Applications", true);
            registryKey.DeleteSubKeyTree("KAKANIMOTools");
        }

        #endregion
    }
}
