using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

[assembly: CommandClass(typeof (CombineDbTexts))]

namespace eZcad.Addins
{
    /// <summary> 将单行文字转换为多行文字 </summary>
    public class CombineDbTexts
    {
        /// <summary> 将单行文字转换为多行文字 </summary>
        [CommandMethod("eZcad", "CombineDbTexts", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void EcCombineDbTexts()
        {
            DocumentModifier.ExecuteCommand(Execute);
        }

        /// <summary> 将单行文字转换为多行文字 </summary>
        public void Execute(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var textIds = SelectDbtexts(docMdf);
            if (textIds != null && textIds.Length > 0)
            {
                var texts = textIds.Select(r => r.GetObject(OpenMode.ForRead) as DBText);
                var arr2D = new EntityArray2D<DBText>(texts);
                var textsArr2D = arr2D.Arrange2D();
                //
                for (int r = 0; r < textsArr2D.GetLength(0); r++)
                {
                    // 将一行中的所有文字转换到一个单行文字中
                    var sb = new StringBuilder();
                    DBText baseText = null;
                    for (int c = 0; c < textsArr2D.GetLength(1); c++)
                    {
                        var cellTexts = textsArr2D[r, c];
                        if (cellTexts.Count > 0)
                        {
                            if (baseText == null)
                            {
                                baseText = cellTexts.First();
                            }
                            foreach (var t in cellTexts)
                            {
                                sb.Append(t.TextString);
                                docMdf.WriteLineIntoDebuger(r, c, t.TextString);
                            }
                        }
                        if (baseText != null)
                        {
                            baseText.UpgradeOpen();
                            baseText.TextString = sb.ToString();
                        }
                    }
                }
            }
        }

        private ObjectId[] SelectDbtexts(DocumentModifier docMdf)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择要进行组合的单行文字 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // Finally run the selection and show any results
            var psr = docMdf.acEditor.GetSelection(pso);

            if (psr.Status == PromptStatus.OK)
            {
                return psr.Value.GetObjectIds();
            }
            return null;
        }
    }
}