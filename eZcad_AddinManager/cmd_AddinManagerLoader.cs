// (C) Copyright 2016 by XN 
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using eZcad.AddinManager;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using eZcad_AddinManager;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(cmd_AddinManagerLoader))]

namespace eZcad_AddinManager
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    partial class cmd_AddinManagerLoader
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        private bool _addinManagerFirstLoaded = true;
        // Modal Command with localized name
        [CommandMethod("AddinManager", "LoadAddinManager", CommandFlags.Modal)]
        public void LoadAddinManager() // This method can have any name
        {
            form_AddinManager frm = form_AddinManager.GetUniqueForm();
            if (_addinManagerFirstLoaded)
            {
                // 将上次插件卸载时保存的程序集数据加载进来

                var nodesInfo = AssemblyInfoDllManager.GetInfosFromSettings();
                frm.RefreshTreeView(nodesInfo);

                //
                _addinManagerFirstLoaded = false;
            }
            else
            {
            }
           Application.ShowModelessDialog(null,frm);
           // Application.ShowModalDialog(frm);
        }

        // Modal Command with localized name
        [CommandMethod("AddinManager", "LastExternalCommand", CommandFlags.Modal)]
        public void LastExternalCommand() // This method can have any name
        {
            ExCommandExecutor.InvokeCurrentExternalCommand();
        }
    }
}
