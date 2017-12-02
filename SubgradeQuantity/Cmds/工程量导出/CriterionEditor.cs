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

[assembly: CommandClass(typeof(eZcad.SubgradeQuantity.Cmds.CriterionEditor))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 路基工程量相关选项的设置 </summary>
    [EcDescription(CommandDescription)]
    public class CriterionEditor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "EditCriterion";
        private const string CommandText = @"计量准则";
        private const string CommandDescription = @"路基工程量计算中，判断与计量的相关选项设置";

        /// <summary> 边坡防护的选项设置 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, ProtectionConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "SetOptions_32.png")]
        public void EditCriterion()
        {
            DocumentModifier.ExecuteCommand(EditCriterion);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new CriterionEditor();
            return AddinManagerDebuger.DebugInAddinManager(s.EditCriterion,
                impliedSelection, ref errorMessage, ref elementSet);
        }


        #endregion  /// <summary> 边坡防护的选项设置 </summary>
        public ExternalCmdResult EditCriterion(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            // var allXdataTypes = DbXdata.GetAllXdataTypes();
            var handledXdataTypes = DbXdata.DatabaseXdataType.General | DbXdata.DatabaseXdataType.LayerNames;
            // 在执行此方法前，已经通过“DbXdata.LoadAllOptionsFromDbToMemory”方法，将文档中的通用选项加载到了内存中，所以不需要再特别地调用 RefreshOptionsFromDb()方法了。
            //DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);

            var f = new Form_CriterionEditor(docMdf);
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
            return  ExternalCmdResult.Commit;
        }
    }
}