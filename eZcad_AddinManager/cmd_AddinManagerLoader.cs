// (C) Copyright 2016 by XN 
//

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad_AddinManager;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。
[assembly: CommandClass(typeof(cmd_AddinManagerLoader))]

namespace eZcad_AddinManager
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class cmd_AddinManagerLoader
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
        [CommandMethod("AddinManager", "LoadAddinManager", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
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
            // 先清空以前已经选择的对象集合
            SetImpliedSelection();
            Application.ShowModelessDialog(null, frm);
            //Application.ShowModalDialog(frm);
        }

        // Modal Command with localized name
        [CommandMethod("AddinManager", "LastExternalCommand", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void LastExternalCommand() // This method can have any name
        {
            SetImpliedSelection();
            ExCommandExecutor.InvokeCurrentExternalCommand();
        }

        /// <summary> 在执行方法之前先获取已经选择到的选择对象集合 </summary>
        private void SetImpliedSelection()
        {
            // 获得当前文档   Get the current document
            Editor acDocEd =
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            // 获得 PickFirst 选择集    Get the PickFirst selection set
            PromptSelectionResult acSSPrompt = acDocEd.SelectImplied();

            // 如果提示状态是 OK，那么对象在命令启动前已经被选择了   If the prompt status is OK, objects were selected before
            // the command was started
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                ExCommandExecutor.ImpliedSelection = acSSPrompt.Value;
            }
            else
            {
                ExCommandExecutor.ImpliedSelection = null;
            }
        }

    }
}