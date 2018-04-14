using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.ParameterForm;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof(SlopeCutter))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
    [EcDescription(CommandDescription)]
    public class SlopeCutter : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CutSlopes";

        private const string CommandText = @"标高剪切";
        private const string CommandDescription = @"按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式";

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "SepByElev_32.png")]
        public void CutSlopes()
        {
            DocumentModifier.ExecuteCommand(CutSlopes);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeCutter();
            return SQAddinManagerDebuger.DebugInAddinManager(s.CutSlopes,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        private ExternalCmdResult CutSlopes(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var fm = PF_SeperateByElev.GetUniqueInstance(docMdf, impliedSelection);
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