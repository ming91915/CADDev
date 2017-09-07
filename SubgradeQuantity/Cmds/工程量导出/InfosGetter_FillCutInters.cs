using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.ParameterForm;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(InfosGetter_FillCutInters))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
    [EcDescription(CommandDescription)]
    public class InfosGetter_FillCutInters : ICADExCommand
    {

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "ExportFillCutIntersections";
        private const string CommandText = @"填挖交界";
        private const string CommandDescription = @"将所有的填挖交界信息提取出来并制成相应表格";

        /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, ProtectionConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "ExportFillCutIntersections_32.png")]
        public void ExportFillCutIntersections()
        {
            DocumentModifier.ExecuteCommand(ExportFillCutIntersections);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var sp = new InfosGetter_FillCutInters();
            return AddinManagerDebuger.DebugInAddinManager(sp.ExportFillCutIntersections,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;
        /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
        public ExternalCmdResult ExportFillCutIntersections(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            _docMdf = docMdf;
            var fm = PF_ExportFillCutInters.GetUniqueInstance(docMdf, impliedSelection);
            var res = fm.ShowDialog();
            if (res == DialogResult.OK)
            {
                return ExternalCmdResult.Commit;
            }
            else
            {
                return ExternalCmdResult.Cancel;
            }
        }
    }
}