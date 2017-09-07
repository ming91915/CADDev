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
            DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);

            var f = new Form_CriterionEditor(docMdf);
            var res = f.ShowDialog(null);
            if (res == DialogResult.OK)
            {
                DbXdata.FlushXData(docMdf, handledXdataTypes);
            }
            else if (res == DialogResult.Cancel)
            {
                DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            }
            return  ExternalCmdResult.Commit;
        }
    }
}