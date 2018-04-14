using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Debug;
using eZcad.Utility;

namespace eZcad.OnCode
{
    /// <summary> 批量修改标注样式 </summary>
    [EcDescription(CommandDescription)]
    public class DimStyles : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ModifyDimStyle";

        private const string CommandText = @"修改标注";
        private const string CommandDescription = @"批量修改标注样式";

        /// <summary> 批量修改标注样式 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void ModifyDimStyle()
        {
            DocumentModifier.ExecuteCommand(ModifyDimStyle);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new DimStyles();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.ModifyDimStyle,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 批量修改标注样式 </summary>
        public ExternalCmdResult ModifyDimStyle(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var dimStyles = docMdf.acTransaction.GetObject
                (docMdf.acDataBase.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            foreach (var dimStyleId in dimStyles)
            {
                var dimStyle = docMdf.acTransaction.GetObject(dimStyleId, OpenMode.ForWrite) as DimStyleTableRecord;
                
                // 开始修改标注样式
                if (dimStyle.Name.StartsWith("D"))
                {
                    // 修改箭头大小
                    dimStyle.Dimdec = 3;
                }
                else
                {
                    dimStyle.Dimdec = 0;
                }
            }

            return ExternalCmdResult.Commit;
        }
    }
}