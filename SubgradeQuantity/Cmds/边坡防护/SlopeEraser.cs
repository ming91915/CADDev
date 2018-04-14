using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(SlopeEraser))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 将边坡对象清理为一般的边坡线，并删除其中所有与边坡相关的数据 </summary>
    [EcDescription(CommandDescription)]
    public class SlopeEraser : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "EraseSlope";
        private const string CommandText = @"清理边坡";
        private const string CommandDescription = @"将边坡对象清理为一般的边坡线，并删除其中所有与边坡相关的数据";


        /// <summary> 将边坡对象清理为一般的边坡线，并删除其中所有与边坡相关的数据 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, SQConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription),
         RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "EraseSlope_32.png")]
        public void EraseSlope()
        {
            DocumentModifier.ExecuteCommand(EraseSlope);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeEraser();
            return SQAddinManagerDebuger.DebugInAddinManager(s.EraseSlope,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 将边坡对象清理为一般的边坡线，并删除其中所有与边坡相关的数据 </summary>
        public ExternalCmdResult EraseSlope(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            SQUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 所有的断面
            // var allSections = ProtectionUtils.GetAllSections(docMdf,sort:true);
            var slopes = SQUtils.SelecteExistingSlopeLines(docMdf, left: null, sort: false);
            var es = EditStateIdentifier.GetCurrentEditState(_docMdf);
            es.CurrentBTR.UpgradeOpen();
            foreach (var slp in slopes)
            {
                slp.ClearXData();
            }
            es.CurrentBTR.DowngradeOpen();
            return ExternalCmdResult.Commit;
        }
    }
}