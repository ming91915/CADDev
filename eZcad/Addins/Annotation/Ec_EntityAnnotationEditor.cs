using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(Ec_EntityAnnotationEditor))]

namespace eZcad.Addins
{
    /// <summary> <seealso cref="CommandDescription"/> </summary>
    [EcDescription(CommandDescription)]
    public class Ec_EntityAnnotationEditor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"EditEntityAnnotation";

        private const string CommandText = @"图元注释";
        private const string CommandDescription = @"对图元中的自定义注释进行读写";

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.Interruptible | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void TextRegex()
        {
            DocumentModifier.ExecuteCommand(EditEntityAnnotation);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new Ec_EntityAnnotationEditor();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.EditEntityAnnotation,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        public ExternalCmdResult EditEntityAnnotation(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            //
            var ent = AddinManagerDebuger.PickObject<Entity>(docMdf.acEditor, "查看任意一个元素的注释");
            if (ent == null) return ExternalCmdResult.Cancel;
            //
            AnnotationEntity.SetAppName(docMdf);
            var annoEnt = new AnnotationEntity(ent);
            var annots = annoEnt.ExtractAnnotsFromXdata();
            //
            annoEnt.WriteAnnotationsOnEditor(docMdf);
            // 如果只有一行注释信息，则直接在Editor中编辑即可
            if (annots.Count <= 1)
            {
                bool clearAnnot = false;
                bool editInForm = false;
                bool setNewValue = false;
                string newAnno = null;
                var succ = GetAnnotationsKey(docMdf.acEditor, out clearAnnot, out editInForm, out setNewValue,
                    out newAnno);
                if (!succ) return ExternalCmdResult.Cancel;
                //
                if (clearAnnot)
                {
                    annoEnt.ClearAnnotations();
                }
                else if (editInForm)
                {
                }
                else if (setNewValue)
                {
                    newAnno = annots.Count == 0 ? "" : annots[0];
                    succ = GetAnnotations(docMdf.acEditor, ref newAnno);
                    if (!succ) return ExternalCmdResult.Cancel;
                    annoEnt.SetAnnotsToXdata(newAnno);
                }
                else
                {
                    // 输入的为一般性的字符
                    // 如果为空，则保持原属性不变
                    if (!string.IsNullOrEmpty(newAnno))
                    {
                        annoEnt.SetAnnotsToXdata(newAnno);
                    }
                }
                // 新的属性值
                annoEnt.WriteAnnotationsOnEditor(docMdf);
            }
            else
            {
                // 要创建出一个窗口以进行编辑
            }
            //
            return ExternalCmdResult.Commit;
        }

        #region ---   界面操作

        /// <summary> 选择单行或者多行文字 </summary>
        public static ObjectId[] SelectElements(DocumentModifier docMdf)
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
        private static bool GetAnnotationsKey(Editor ed, out bool clearAnnots, out bool editInForm, out bool setNewValue,
            out string newAnnot)
        {
            clearAnnots = false;
            editInForm = false;
            setNewValue = false;
            newAnnot = "";
            var op = new PromptKeywordOptions("元素注释编辑器")
            {
                AllowArbitraryInput = true,
                AllowNone = false,
            };
            op.SetMessageAndKeywords(messageAndKeywords: "\n[清除注释(C) / 窗口编辑(F) / 设置新值(S)]:",
                globalKeywords: "K清除注释 K窗口编辑 K设置新值"); // 默认值写在前面
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                var key = res.StringResult;
                ed.WriteMessage("输入的值：" + key + ".");
                if (key == "K清除注释")
                {
                    clearAnnots = true;
                }
                else if (key == "K窗口编辑")
                {
                    editInForm = true;
                }
                else if (key == "K设置新值")
                {
                    setNewValue = true;
                }
                else
                {
                    // 输入的为一般字符，可以直接作为属性值
                    newAnnot = key;
                }
                return true;
            }
            return false;
        }


        /// <summary> 在命令行中获取一个字符 </summary>
        /// <param name="annot">成功获得的数值</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetAnnotations(Editor ed, ref string annot)
        {
            var op = new PromptStringOptions("\n元素的描述信息")
            {
                AllowSpaces = true,
                UseDefaultValue = true,
                // DefaultValue = annot,
            };
            var res = ed.GetString(op);
            if (res.Status == PromptStatus.OK)
            {
                annot = res.StringResult;
                return true;
            }
            return false;
        }

        #endregion
    }
}