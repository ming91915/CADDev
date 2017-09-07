using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.ParameterForm;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof(SlopeProtTextModifier))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 对AutoCAD图形中与边坡防护相关的文字进行修改 </summary>
    [EcDescription(CommandDescription)]
    public class SlopeProtTextModifier : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ModifySlopeProtText";

        private const string CommandText = @"修改文字";
        private const string CommandDescription = @"对AutoCAD图形中与边坡防护相关的文字进行修改";

        /// <summary> 对AutoCAD图形中与边坡防护相关的文字进行修改 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor | CommandFlags.Redraw)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "DeleteProtection_32.png")]
        public void ModifySlopeProtText()
        {
            DocumentModifier.ExecuteCommand(ModifySlopeProtText);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeProtTextModifier();
            return AddinManagerDebuger.DebugInAddinManager(s.ModifySlopeProtText,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        private ExternalCmdResult ModifySlopeProtText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {

            _docMdf = docMdf;
            var fm = PF_ModifyProtectionText.GetUniqueInstance(docMdf, impliedSelection);
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

        #region --- 通过命令行的方式进行操作

        private ExternalCmdResult ModifySlopeProtTextOnCommandLine(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var selectedTexts = new List<DBText>();
            bool succ;
            bool? onlySlope = null;
            onlySlope = GetKeywordsFromTwoOptions(_docMdf);

            succ = SelectProtectionTexts(docMdf.acEditor, onlySlope, out selectedTexts);
            while (succ)
            {
                foreach (var st in selectedTexts)
                {
                    st.UpgradeOpen();
                    st.Erase();
                    st.Draw();
                    st.DowngradeOpen();
                }
                succ = SelectProtectionTexts(docMdf.acEditor, onlySlope, out selectedTexts);
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary> 从两个选项中选择一个 </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示按顶点缩放（默认值），false 表示按长度缩放</returns>
        private static bool? GetKeywordsFromTwoOptions(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n删除选择的边坡与平台[仅平台(P) / 仅边坡(S)]:",
                globalKeywords: "平台 边坡"); // 默认值写在前面
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                if (res.StringResult == "平台")
                {
                    return false;
                }
                // 非默认值
                if (res.StringResult == "边坡")
                {
                    return true;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="onlySlope">true 表示只删除边坡，false 表示只删除平台防护；null 表示边坡与平台都删除</param>
        /// <param name="selectedTexts"></param>
        /// <returns></returns>
        private bool SelectProtectionTexts(Editor ed, bool? onlySlope, out List<DBText> selectedTexts)
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr;
            if (!onlySlope.HasValue)
            {
                acTypValAr = new TypedValue[]
                {
                    new TypedValue((int) DxfCode.Start, "TEXT"),
                    new TypedValue((int) DxfCode.Operator, "<OR"),
                    new TypedValue((int) DxfCode.LayerName, ProtectionConstants.LayerName_ProtectionMethod_Slope),
                    new TypedValue((int) DxfCode.LayerName, ProtectionConstants.LayerName_ProtectionMethod_Platform),
                    new TypedValue((int) DxfCode.Operator, "OR>")
                };
            }
            else if (onlySlope.Value)
            {
                acTypValAr = new TypedValue[]
                {
                    new TypedValue((int) DxfCode.Start, "TEXT"),
                    new TypedValue((int) DxfCode.LayerName, ProtectionConstants.LayerName_ProtectionMethod_Slope),
                };
            }
            else
            {
                acTypValAr = new TypedValue[]
                {
                    new TypedValue((int) DxfCode.Start, "TEXT"),
                    new TypedValue((int) DxfCode.LayerName, ProtectionConstants.LayerName_ProtectionMethod_Platform),
                };
            }


            // 将过滤条件赋值给SelectionFilter对象
            var op = new PromptSelectionOptions();
            op.MessageForAdding = "\n选择要删除的防护文字";
            op.MessageForRemoval = op.MessageForAdding;

            // 请求在图形区域选择对象
            var acSSPrompt = ed.GetSelection(new SelectionFilter(acTypValAr));

            selectedTexts = new List<DBText>();
            // 如果提示状态OK，表示对象已选
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                selectedTexts =
                    acSSPrompt.Value.GetObjectIds().Select(r => r.GetObject(OpenMode.ForRead)).OfType<DBText>().ToList();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}