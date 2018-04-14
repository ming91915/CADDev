using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins.Text;
using eZcad.Debug;
using eZcad.Utility;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(TextRegexTool))]

namespace eZcad.Addins.Text
{
    /// <summary> <seealso cref="CommandDescription"/> </summary>
    [EcDescription(CommandDescription)]
    public class TextRegexTool : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"TextRegex";

        private const string CommandText = @"正则文字";
        private const string CommandDescription = @"在AutoCAD中进行正则表达式的查询与替换";

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void TextRegex()
        {
            DocumentModifier.ExecuteCommand(TextRegex);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new TextRegexTool();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.TextRegex,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        public ExternalCmdResult TextRegex(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var texts = SelectTexts(docMdf);
            if (texts == null || texts.Length == 0) return ExternalCmdResult.Cancel;

            //
            string pattern = null;
            bool showTips = false;
            bool ignoreCase = false;
            bool succ = GetRegexPattern(docMdf.acEditor, out showTips, out ignoreCase, out pattern);
            while (!succ)
            {
                succ = true;
                if (showTips)
                {
                    string tips = GetRegexTip();
                    docMdf.WriteNow(tips);
                    Cancel(texts);
                    return ExternalCmdResult.Cancel;
                }
                if (ignoreCase)
                {
                    succ = GetRegexPattern(docMdf.acEditor, out showTips, out ignoreCase, out pattern);
                }
            }

            //
            var rextRegex = new TextRegex(texts);
            var matches = rextRegex.Match(pattern, ignoreCase: ignoreCase);
            if (matches.Length == 0)
            {
                _docMdf.WriteNow(@"未找到任何匹配的文字!");
                Cancel(texts);
                return ExternalCmdResult.Cancel;
            }
            else
            {
                Cancel(matches);
                _docMdf.WriteNow($"匹配的文字元素个数：{matches.Length}");
                eZcad.Utility.Utils.FocusOnMainUIWindow();
            }
            //
            return ExternalCmdResult.Commit;
        }

        private void Cancel(ObjectId[] selection)
        {
            //_docMdf.acEditor.SetImpliedSelection(new ObjectId[0]);
            _docMdf.acEditor.SetImpliedSelection(selection);
        }

        private string GetRegexTip()
        {
            var tips = eZcad.Properties.Resources.RegexTips;
            return tips;
        }

        #region ---   界面操作

        /// <summary> 选择单行或者多行文字 </summary>
        public static ObjectId[] SelectTexts(DocumentModifier docMdf)
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Start, "TEXT"),
                new TypedValue((int) DxfCode.Start, "MTEXT"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            var op = new PromptSelectionOptions();
            // Set our prompts to include our keywords
            op.MessageForAdding = "\n选择单行或者多行文字以进行正则表达式查询或替换"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = op.MessageForAdding;


            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetSelection(new SelectionFilter(filterTypes));

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value;
                return acSSet.GetObjectIds();
            }
            return null;
        }

        /// <summary> 在命令行中获取一个字符 </summary>
        /// <param name="value">成功获得的数值</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetRegexPattern(Editor ed, out bool showtips, out bool ignoreCase, out string value)
        {
            value = "";
            showtips = false;
            ignoreCase = false;
            var op = new PromptKeywordOptions("正则表达式")
            {
                AllowArbitraryInput = true,
            };
            op.SetMessageAndKeywords(messageAndKeywords: "\n查询正则表达式[提示(T) / 不区分大小写(C)]:",
                    globalKeywords: "提示 不区分大小写"); // 默认值写在前面
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.StringResult;
                if (value == "提示")
                {
                    showtips = true;
                }
                else if (value == "不区分大小写")
                {
                    ignoreCase = true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}