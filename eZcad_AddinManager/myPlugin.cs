// (C) Copyright 2016 by XN 
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using eZcad.AddinManager;
using eZcad.AddinManager;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad_AddinManager;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

// This line is not mandatory, but improves loading performances

[assembly: ExtensionApplication(typeof(MyPlugin))]

namespace eZcad_AddinManager
{

    /// <summary>
    /// AddinManager 插件程序集 加载与卸载时的操作
    /// </summary>
    /// 
    /// <remarks>
    /// This class is instantiated by AutoCAD once and kept alive for the 
    /// duration of the session. If you don't do any one time initialization 
    /// then you should remove this class.
    /// </remarks>
    internal class MyPlugin : IExtensionApplication
    {
        #region ---   加载与卸载

        /// <summary>
        /// 加载 AddinManager 插件时自动执行
        /// </summary>
        void IExtensionApplication.Initialize()
        {
            try
            {
                var ime = new AutoSwitchIME();
            }
            catch (Exception ex)
            {
                Debug.Print("AddinManager 插件加载时出错： \n\r" + ex.Message + "\n\r" + ex.StackTrace);
            }
        }

        void IExtensionApplication.Terminate()
        {
            try
            {
                form_AddinManager frm = form_AddinManager.GetUniqueForm();
                var nodesInfo = frm.NodesInfo;
                //
                AssemblyInfoDllManager.SaveAssemblyInfosToSettings(nodesInfo);
            }
            catch (Exception ex)
            {
                Debug.Print("AddinManager 插件关闭时出错： \n\r" + ex.Message + "\n\r" + ex.StackTrace);
            }
        }
        #endregion

    }
}