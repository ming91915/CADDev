using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(TextScaler))]

namespace eZcad.Addins
{
    /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
    public class TextScaler
    {

        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ScaleText", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void EcScaleText()
        {
            DocumentModifier.ExecuteCommand(ScaleText);
        }

        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        public static void ScaleText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var texts = GetTexts(docMdf);
            //
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            double sc = 2;
            var psr = ed.GetDouble("\n缩放比例： ");
            if (psr.Status == PromptStatus.OK)
            {
                sc = psr.Value;
            }
            //

            foreach (var id in texts)
            {
                var ent = docMdf.acTransaction.GetObject(id, OpenMode.ForRead);
                if (ent is DBText)
                {
                    ent.UpgradeOpen();
                    var t = ent as DBText;

                    t.Height = t.Height * sc;
                    ent.DowngradeOpen();
                }
                else if (ent is MText)
                {
                    ent.UpgradeOpen();
                    var t = ent as MText;
                    t.TextHeight *= sc;
                    ent.DowngradeOpen();
                }
            }
            docMdf.acActiveDocument.Editor.Regen();
        }

        [CommandMethod("GetTexts", CommandFlags.UsePickSet)]
        public static ObjectId[] GetTexts(DocumentModifier docMdf)
        {
            // 在界面中选择单行文字或者多行文字
            TypedValue[] acTypValAr2 = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Start, "TEXT"),
                new TypedValue((int) DxfCode.Start, "MTEXT"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr2);

            //获取当前文档编辑器
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // Create our options object
            PromptSelectionOptions pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\nPlease add objects to selection or " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\nPlease remove objects from selection or " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    ed.WriteMessage("\nKeyword entered: {0}", e.Input);
                };

            // Finally run the selection and show any results
            PromptSelectionResult psr = ed.GetSelection(pso, acSelFtr);

            if (psr.Status == PromptStatus.OK)
            {
                ed.WriteMessage($"\n{psr.Value.Count} object{(psr.Value.Count == 1 ? "" : "s")} selected.");
                return psr.Value.GetObjectIds();
            }
            return new ObjectId[0];
        }
    }
}