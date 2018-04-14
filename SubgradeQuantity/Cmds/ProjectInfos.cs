using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(eZcad.SubgradeQuantity.Cmds.ProjectInfos))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 路基工程量相关选项的设置 </summary>
    [EcDescription(CommandDescription)]
    public class ProjectInfos : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "SetProjectInfos";
        private const string CommandText = @"项目信息";
        private const string CommandDescription = @"道路路基工程量的基础项目信息设置";

        /// <summary> 边坡防护的选项设置 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, SQConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "ProjectInfos_32.png")]
        public void SetProjectInfos()
        {
            DocumentModifier.ExecuteCommand(SetProjectInfos);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ProjectInfos();
            return SQAddinManagerDebuger.DebugInAddinManager(s.SetProjectInfos,
                impliedSelection, ref errorMessage, ref elementSet);
        }


        #endregion  /// <summary> 边坡防护的选项设置 </summary>
        public ExternalCmdResult SetProjectInfos(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            // 刷新所有的全局选项到内存中
            // var allXdataTypes = DbXdata.GetAllXdataTypes();
            var handledXdataTypes = DbXdata.DatabaseXdataType.General | DbXdata.DatabaseXdataType.LayerNames;
            // 在执行此方法前，已经通过“DbXdata.LoadAllOptionsFromDbToMemory”方法，将文档中的通用选项加载到了内存中，所以不需要再特别地调用 RefreshOptionsFromDb()方法了。
            //DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);

            var f = new Form_ProjectInfos(docMdf);
            var res = f.ShowDialog(null);
            if (res == DialogResult.OK)
            {
                DbXdata.FlushXData(docMdf, handledXdataTypes);
            }
            else if (res == DialogResult.Cancel)
            {
                // 将内存中对全局选项的修改进行还原，还原的方法就是重新从数据库中加载修改前的数据。
                DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            }
            return ExternalCmdResult.Cancel;
        }
    }
}