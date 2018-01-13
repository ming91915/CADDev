using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins.Table;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(CopyColumns))]

namespace eZcad.Addins.Table
{
    /// <summary> 将剪切板中的一列数据复制到AutoCAD中的一列文本中 </summary>
    [EcDescription(CommandDescription)]
    public class CopyColumns : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CopyColumnsFromClipboard";

        private const string CommandText = @"复制列";
        private const string CommandDescription = @"将剪切板中的一列数据复制到AutoCAD中的一列文本中";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void SumupArea()
        {
            DocumentModifier.ExecuteCommand(CopyColumnsFromClipboard);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new CopyColumns();
            return AddinManagerDebuger.DebugInAddinManager(s.CopyColumnsFromClipboard,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        public ExternalCmdResult CopyColumnsFromClipboard(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            // var pl = AddinManagerDebuger.PickObject<Curve>(docMdf.acEditor);

            // AutoCAD中的一列数据，并从高到低排序
            var texts = SelectTexts();
            if (texts == null || texts.Count == 0)
            {
                return ExternalCmdResult.Cancel;
            }

            // 剪切板中的一列数据
            var col = true;
            var lines = GetTextsFromClipboard(out col);
            if (lines == null || lines.Length == 0)
            {
                return ExternalCmdResult.Cancel;
            }
            //
            if (col)
            {
                texts.Sort(RowsComparer);
            }
            else
            {
                texts.Sort(ColsComparer);
            }
            //
            var rowCount = Math.Min(lines.Length, texts.Count);
            var changedTextIds = new ObjectId[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                var t = texts[i];
                t.UpgradeOpen();
                t.TextString = lines[i];
                t.DowngradeOpen();
                //
                changedTextIds[i] = (t.ObjectId);
            }
            docMdf.acEditor.SetImpliedSelection(changedTextIds);
            docMdf.WriteLineIntoDebuger($"剪切板中数据行数：{lines.Length}");
            docMdf.WriteLineIntoDebuger($"AutoCAD中选择的数据行数：{texts.Count}");
            return ExternalCmdResult.Commit;
        }

        /// <summary>
        /// 从剪切板中提取一列或者一行数据
        /// </summary>
        /// <param name="col">提取的数据表示一行或者一列</param>
        /// <returns></returns>
        private string[] GetTextsFromClipboard(out bool col)
        {
            col = true;
            var s = Clipboard.GetText();
            if (string.IsNullOrEmpty(s)) { return null; }
            string[] lines = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //
            int rowsCount = lines.Length; //要写入多少行数据
            if (rowsCount == 0)
            {
                return null;
            }
            else
            {
                var cols = lines[0].Split('\t'); //要写入的每一行数据中有多少列
                if (cols.Length > 1)
                {
                    col = false; // 表示输出一行数据
                    return cols;
                }
                else
                {
                    col = true; // 表示输出一列数据
                    return lines;
                }
            }
        }

        private List<DBText> SelectTexts()
        {
            var op = new PromptSelectionOptions();
            op.MessageForAdding = "\n选择一列文本"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = op.MessageForAdding;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "TEXT"),
            };

            var res = _docMdf.acEditor.GetSelection(op, new SelectionFilter(filterType));
            var texts = new List<DBText>();
            if (res.Status == PromptStatus.OK)
            {
                foreach (var id in res.Value.GetObjectIds())
                {
                    var c = id.GetObject(OpenMode.ForRead) as DBText;
                    if (c != null)
                    {
                        texts.Add(c);
                    }
                }
            }

            return texts;
        }

        /// <summary> Y 值大的排在前面 </summary>
        private int RowsComparer(DBText ent1, DBText ent2)
        {
            // Y 值大的排在前面
            return ent2.Position.Y.CompareTo(ent1.Position.Y);
        }
        /// <summary> X 值小的排在前面 </summary>
        private int ColsComparer(DBText ent1, DBText ent2)
        {
            // X 值小的排在前面
            return ent1.Position.X.CompareTo(ent2.Position.X);
        }
    }
}