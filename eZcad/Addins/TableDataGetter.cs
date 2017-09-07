using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using eZcad.Utility;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZstd.MarshalReflection;
using eZstd.Miscellaneous;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using Utils = eZstd.Miscellaneous.Utils;

namespace eZcad.Addins
{
    /// <summary> 从AutoCAD中的文字或者表格中提取出表格数据 </summary>
    public class TableDataGetter
    {
        /// <summary> 如果用户未指定，则为 null </summary>
        private static bool? _addRow;

        #region   --- 从文字中获取数据

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ExternalCmdResult GetVectorFromText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            // 确定是要按行添加还是按列添加
            if (_addRow == null)
            {
                _addRow = AsRow(docMdf);
            }
            //
            bool continueSelect = false;
            bool exportData = false;
            List<List<DBText>> textss = new List<List<DBText>>();
            List<DBText> texts = GetVectorTextsFromUI(docMdf, ref continueSelect, ref exportData);
            while (continueSelect)
            {
                textss.Add(texts);
                texts = GetVectorTextsFromUI(docMdf, ref continueSelect, ref exportData);
            }
            // 数据导出
            if (exportData)
            {
                var arr = ConvertVectorsToArray(textss, _addRow.Value);

                // 将数据保存到表格中
                SaveDataToExcel(arr);
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary> 提示用户通过界面选择文字 </summary>
        /// <param name="docMdf"></param>
        /// <returns>如果用户选择完所有的行或者列，决定要进行输出表格了，则返回null</returns>
        private static List<DBText> GetVectorTextsFromUI(DocumentModifier docMdf, ref bool continueSelect,
            ref bool exportData)
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "TEXT"),
            };

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            //获取当前文档编辑器
            Editor ed = docMdf.acActiveDocument.Editor;

            // Create our options object
            PromptSelectionOptions pso = new PromptSelectionOptions();
            // Add our keywords

            pso.Keywords.Add("Export");
            bool export = false;

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            // 在事件绑定的方法执行的过程中用户仍然可以在AutoCAD界面中执行选择操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    if (e.Input == "Export")
                    {
                        export = true;
                        ed.WriteMessage("\n开始进行数据的导出");

                        // 考虑强行退出 命令行编辑模式

                        // !!! ((char)27) 对应按下 Escape键，这一句会在AutoCAD进入 ed.GetSelection 之前立即执行，
                        // 即是可以模拟当用户输入关键字时立即按下 Escape，此时 API 会直接结束 ed.GetSelection 并往下执行，
                        // 其返回的 PromptSelectionResult.Status 属性值为 Cancel。
                        docMdf.acActiveDocument.SendStringToExecute(((char)27).ToString(), true,
                            wrapUpInactiveDoc: false, echoCommand: false);

                        // 这一句用来画一个圆，它不会在 ed.GetSelection 后立即执行，而是在整个 API 代码执行结束后才执行。
                        // docMdf.acActiveDocument.SendStringToExecute("._circle 2,2,0 4 ", true, wrapUpInactiveDoc: false, echoCommand: true);
                    }
                };

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择要进行输出的单行文本 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。

            // 请求在图形区域选择对象。 GetSelection() 会阻塞线程，直到用户在AutoCAD界面中选择对象完成、取消或者出错。
            PromptSelectionResult psr = ed.GetSelection(pso, acSelFtr);

            exportData = export;

            // 输出文本所对应的数据
            if (psr.Status == PromptStatus.OK)
            {
                List<DBText> texts = null;
                texts = new List<DBText>();
                DBObject v;
                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    // v = id.GetObject(OpenMode.ForRead);
                    v = docMdf.acTransaction.GetObject(id, OpenMode.ForRead);
                    if (v is DBText)
                    {
                        texts.Add(v as DBText);
                    }
                }
                continueSelect = true;
                return texts;
            }
            else
            {
                continueSelect = false;
                return null;
            }
        }


        /// <summary> 提示用户通过界面选择文字 </summary>
        /// <param name="docMdf"></param>
        /// <returns>如果用户选择完所有的行或者列，决定要进行输出表格了，则返回null</returns>
        private static List<DBText> GetTableTextsFromUI(DocumentModifier docMdf)
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "TEXT"),
            };

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            //获取当前文档编辑器
            Editor ed = docMdf.acActiveDocument.Editor;

            // Create our options object
            PromptSelectionOptions pso = new PromptSelectionOptions();
            // Add our keywords

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择表格范围内的所有文本 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。

            // 请求在图形区域选择对象。 GetSelection() 会阻塞线程，直到用户在AutoCAD界面中选择对象完成、取消或者出错。
            PromptSelectionResult psr = ed.GetSelection(pso, acSelFtr);

            List<DBText> texts = null;
            // 输出文本所对应的数据
            if (psr.Status == PromptStatus.OK)
            {
                texts = new List<DBText>();
                DBObject v;
                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    // v = id.GetObject(OpenMode.ForRead);
                    v = docMdf.acTransaction.GetObject(id, OpenMode.ForRead);
                    if (v is DBText)
                    {
                        texts.Add(v as DBText);
                    }
                }
            }
            return texts;
        }

        /// <summary> 提示用户指定是要每次选择一行还是一列 </summary>
        private static bool AsRow(DocumentModifier docMdf)
        {
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\n将选择的数据作为表格的行或列";
            pKeyOpts.Keywords.Add("Row");
            pKeyOpts.Keywords.Add("Column");
            pKeyOpts.AllowNone = true;
            pKeyOpts.AppendKeywordsToMessage = true;

            PromptResult pKeyRes = docMdf.acActiveDocument.Editor.GetKeywords(pKeyOpts);
            if (pKeyRes.StringResult == "Column")
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 将多排或者多列文本中的数据提取到一个二维表格中
        /// </summary>
        /// <param name="textss">一个二维表格所对应的点集合</param>
        /// <param name="asRow">true 表示大集合中的每一个小向量代表一行，false 表示大集合中的每一个小向量代表一列</param>
        /// <returns></returns>
        private string[,] ConvertVectorsToArray(List<List<DBText>> textss, bool asRow)
        {
            string[,] arr = null;
            int rowCount = 0, colCount = 0;

            if (asRow) // 大集合中的每一个小向量代表一行
            {
                // 确定表格的尺寸
                rowCount = textss.Count;
                foreach (var texts in textss)
                {
                    if (texts.Count > colCount)
                    {
                        colCount = texts.Count;
                    }
                }
                // 
                arr = new string[rowCount, colCount];
                for (int r = 0; r < rowCount; r++)
                {
                    var row = textss[r];
                    row.Sort(new DBTextComparerX()); // 将一列文本的坐标按X值从小到大排序
                    for (int c = 0; c < row.Count; c++)
                    {
                        arr[r, c] = textss[r][c].TextString;
                    }
                }
            }
            else // 大集合中的每一个小向量代表一列
            {
                // 确定表格的尺寸
                colCount = textss.Count;
                foreach (var texts in textss)
                {
                    if (texts.Count > rowCount)
                    {
                        rowCount = texts.Count;
                    }
                }
                // 
                arr = new string[rowCount, colCount];
                for (int c = 0; c < colCount; c++)
                {
                    var col = textss[c];
                    col.Sort(new DBTextComparerY()); // 将一列文本的坐标按Y值从大到小排序
                    for (int r = 0; r < col.Count; r++)
                    {
                        arr[r, c] = textss[c][r].TextString;
                    }
                }
            }
            return arr;
        }

        #endregion

        #region   --- 从文档中的表格排布的文字中提取出表格信息

        /// <summary> 从文档中的表格排布的文字中提取出表格信息 </summary>
        /// <returns> 对整个二维表格的集合进行排序时，采用文本的坐标Y值大的在前面。这种排序方法的最终结果是：一个单元格中有多个文本时，Y值小的在后面，X是无序的。</returns>
        public ExternalCmdResult GetTableFromText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {

            var ed = docMdf.acActiveDocument.Editor;
            List<Point3d> pRow = new List<Point3d>();
            List<Point3d> pCol = new List<Point3d>();

            // 选择行
            bool continueSelect = false;
            Point3d p = GetPointsFromUI(docMdf, true, out continueSelect);
            while (continueSelect)
            {
                pRow.Add(p);
                ed.WriteMessage($"已选择 {pRow.Count} 个点。当前点的Y坐标值：{p.Y}");
                // 继续选择
                p = GetPointsFromUI(docMdf, true, out continueSelect);
            }

            // 选择列
            p = GetPointsFromUI(docMdf, false, out continueSelect);
            while (continueSelect)
            {
                pCol.Add(p);
                ed.WriteMessage($"已选择 {pCol.Count} 个点。当前点的X坐标值：{p.X}");
                // 继续选择
                p = GetPointsFromUI(docMdf, false, out continueSelect);
            }
            // 获取整个表格中的文本
            if (pRow.Count >= 2 && pCol.Count >= 2)
            {
                // 选择表格中所有的文本数据
                var texts = GetTableTextsFromUI(docMdf);
                if (texts.Count > 0)
                {
                    // 构造表格
                    var arr = ConvertTextsTo2DArray(texts, colRange: pCol, rowRange: pRow);
                    if (arr != null)
                    {
                        // 将表格保存到Excel中
                        SaveDataToExcel(arr);
                    }
                }
            }
            else
            {
                MessageBox.Show("行与列都必须选择至少两个点，以框出一个表格矩形。", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return ExternalCmdResult.Cancel;
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary> 在界面中选择一行点或者一列点，用来进行表格的单元格划分 </summary>
        private Point3d GetPointsFromUI(DocumentModifier docMdf, bool forRow, out bool continueSelect)
        {
            continueSelect = false;
            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "TEXT"),
            };

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            //获取当前文档编辑器
            Editor ed = docMdf.acActiveDocument.Editor;
            string msg = forRow ? "选择作为 行 分隔的点（至少2个） " : "选择作为 列 分隔的点（至少2个） ";

            var pao = new PromptPointOptions(message: $"\n{msg}");
            pao.AllowNone = true; // 允许用户直接按下回车或者右键，以退出 GetPoint() 方法，此时返回的 PromptPointResult.Status 为 None。

            // 请求在图形区域选择对象。 GetSelection() 会阻塞线程，直到用户在AutoCAD界面中选择对象完成、取消或者出错。
            PromptPointResult psr = ed.GetPoint(pao);

            // 输出文本所对应的数据
            if (psr.Status == PromptStatus.OK)
            {
                continueSelect = true;
                return psr.Value;
            }
            else
            {
                continueSelect = false;
                return default(Point3d);
            }
        }

        /// <summary>
        /// 将大量的文本根据指定的表格定位信息排列成一个二维表格中
        /// </summary>
        /// <param name="texts">一个二维表格所对应的点集合</param>
        /// <param name="colRange"> 用来框住表格的X坐标，比如表格中有两列，则xRange集合中就有三个点 </param>
        /// <param name="rowRange"> 用来框住表格的Y坐标，比如表格中有一行，则yRange集合中就有两个点，分别对应其上下边界 </param>
        /// <returns>如果不满足表格的构造要求，则返回 null </returns>
        private string[,] ConvertTextsTo2DArray(List<DBText> texts, List<Point3d> colRange, List<Point3d> rowRange)
        {
            // 对表格尺寸与布置进行排序
            double c, r;
            var Rs = new SortedSet<double>(); // y的值从小到大排列
            var Cs = new SortedSet<double>(); // x的值从小到大排列
            foreach (var rp in rowRange)
            {
                r = rp.Y;
                if (!Rs.Contains(r))
                {
                    Rs.Add(r);
                }
            }
            foreach (var cp in colRange)
            {
                c = cp.X;
                if (!Cs.Contains(c))
                {
                    Cs.Add(c);
                }
            }

            if (Rs.Count > 1 && Cs.Count > 1)
            {
                // 构造表格
                string[,] arr = new string[Rs.Count - 1, Cs.Count - 1]; // ys 对应行的边界，xs对应列的边界

                // 对选择的所有文本的集合进行一个基本的排序，
                // 以优化当多个文本出现在同一个单元格中的情况下时，如何对这一个单元格中的多个文本进行组织的问题。
                // 当每一个单元格中都只有一个文本时，这里的排序不会影响到最终在表格中的位置
                // 对整个二维表格的集合进行排序时，采用文本的坐标Y值大的在前面。这种排序方法的最终结果是：一个单元格中有多个文本时，Y值小的在后面，X是无序的。
                texts.Sort(new PointComparerY());

                // 向表格中填入数据
                int rIndex, cIndex;
                foreach (DBText txt in texts)
                {
                    bool inTable = LocateCell(Cs, Rs, txt.Position, out rIndex, out cIndex);
                    if (inTable)
                    {
                        if (arr[rIndex, cIndex] == null)
                        {
                            arr[rIndex, cIndex] = txt.TextString;
                        }
                        else
                        {
                            arr[rIndex, cIndex] += "\r\n" + txt.TextString; // 多个文本放在同一个单元格中时，将多个文本进行换行附加式组合。
                        }
                    }
                }
                return arr;
            }
            return null;
        }

        /// <summary> 将文本定位到二维表格的单元格中 </summary>
        /// <param name="colRange">集合中的值从小到大排列</param>
        /// <param name="rowRange">集合中的值从小到大排列</param>
        /// <param name="point"></param>
        /// <param name="rowIndex">定位到的行号，第一行的下标为0</param>
        /// <param name="colIndex">定位到的列号，第一列的下标为0</param>
        /// <returns>如果文本不在表格内部，则返回 false  </returns>
        private static bool LocateCell(SortedSet<double> colRange, SortedSet<double> rowRange, Point3d point,
            out int rowIndex, out int colIndex)
        {
            rowIndex = 0;
            colIndex = 0;
            bool inTable = true;
            Func<SortedSet<double>, double, int?> GetVectorIndex = (rg, value) =>
            {
                if (value < rg.Min || value > rg.Max) return null;
                //
                int ind = 0;
                foreach (var v in rg) // 集合中的值从小到大排列
                {
                    if (v > value)
                    {
                        return ind - 1;
                    }
                    ind += 1;
                }
                return null; // 表格文本不在表格范围内
            };

            // 确定行的位置：注意，表格中的第一行的 Y 坐标值是最大的
            var row = GetVectorIndex(rowRange, point.Y);

            if (row != null)
            {
                rowIndex = rowRange.Count - row.Value - 2; // 将单元格所在的行号与从小到大排列的集合中的下标进行反转。
            }
            else
            {
                inTable = false;
            }

            // 确定列的位置
            var col = GetVectorIndex(colRange, point.X);
            if (col != null)
            {
                colIndex = col.Value;
            }
            else
            {
                inTable = false;
            }

            return inTable;
        }

        #endregion

        #region   --- 从表格中获取数据

        public object[,] GetTableFromTable()
        {
            using (DocumentModifier docMdf = new DocumentModifier(true))
            {
                try
                {
                    // 保存新对象到数据库中   Save the new object to the database
                    docMdf.acTransaction.Commit();
                }
                catch (Exception ex)
                {
                    docMdf.acTransaction.Abort(); // Abort the transaction and rollback to the previous state
                    DebugUtils.ShowDebugCatch(ex, "导出表格数据时出错");
                }
            }
            return null;
        }

        #endregion

        #region   --- 私有方法

        /// <summary> 对选择的表格中的所有文本的集合进行一个基本的排序，
        /// 以优化当多个文本出现在同一个单元格中的情况下时，如何对这一个单元格中的多个文本进行组织的问题。
        /// 当每一个单元格中都只有一个文本时，这里的排序不会影响到最终在表格中的位置 </summary>
        /// <remarks> 由于AutoCAD中多数出现一个单元格中放置多行的单行文字的情况（如果多列的情况，则绘图员会倾向于将它们写在同一个单行文本中。），
        /// 所以对整个二维表格的集合进行排序时，采用文本的坐标Y值大的在前面。 </remarks>
        private class PointComparerY : IComparer<DBText>
        {
            public int Compare(DBText obj1, DBText obj2)
            {
                // 对整个二维表格的集合进行排序时，采用文本的坐标Y值大的在前面。
                return obj2.Position.Y.CompareTo(obj1.Position.Y);
                // 集合中小的在前面，所以返回 -1 则 obj1 在前面，返回 1 则 obj2 在前面
            }
        }

        /// <summary> X坐标较小的位于集合前面 </summary>
        private class DBTextComparerX : IComparer<DBText>
        {
            public int Compare(DBText x, DBText y)
            {
                return x.Position.X.CompareTo(y.Position.X);
            }
        }

        /// <summary> Y坐标较大的位于集合前面 </summary>
        private class DBTextComparerY : IComparer<DBText>
        {
            public int Compare(DBText x, DBText y)
            {
                return y.Position.Y.CompareTo(x.Position.Y);
            }
        }

        private static void SaveDataToExcel(string[,] data)
        {
            var filePath = Utils.ChooseSaveFile(title: "将数据保存到Excel中",
                filter: "Excel文件(*.xls)| *.xls");
            if (filePath != null)
            {
                bool fileExists = File.Exists(filePath);
                Workbook wkbk = null;
                if (fileExists)
                {
                    wkbk = Interaction.GetObjectFromFile<Workbook>(filePath);
                }
                else
                {
                    var app = new Application();
                    wkbk = app.Workbooks.Add();
                }
                if (wkbk != null)
                {
                    Worksheet sht = wkbk.Worksheets[1];

                    RangeValueConverter.FillRange(sht, startRow: 1, startCol: 1,
                        arr: data, colPrior: true);

                    wkbk.Application.Windows[wkbk.Name].Visible = true;
                    if (fileExists)
                    {
                        wkbk.Save();
                    }
                    else
                    {
                        wkbk.SaveAs(Filename: filePath, FileFormat: XlFileFormat.xlAddIn8);
                    }
                    wkbk.Application.Visible = true;
                }
            }
        }

        #endregion
    }
}