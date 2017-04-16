using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZstd.MarshalReflection;
using eZstd.Miscellaneous;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;

namespace AutoCADDev
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
        public string[] GetVectorFromText()
        {
            using (DocumentModifier docMdf = new DocumentModifier())
            {
                try
                {
                    // 确定是要按行添加还是按列添加
                    if (_addRow == null)
                    {
                        _addRow = AsRow(docMdf);
                    }
                    //
                    List<List<DBText>> textss = new List<List<DBText>>();
                    List<DBText> texts = GetTextsFromUI(docMdf);
                    while (texts != null)
                    {
                        textss.Add(texts);
                        texts = GetTextsFromUI(docMdf);
                    }

                    var arr = ConvertVectorsToArray(textss, _addRow.Value);

                    // 将数据保存到表格中
                    SaveDataToExcel(arr);

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

        public static string[,] GetTableFromText(double[] yLocations, double xLocations)
        {
            return null;
        }

        /// <summary> 提示用户通过界面选择文字 </summary>
        /// <param name="docMdf"></param>
        /// <returns>如果用户选择完所有的行或者列，决定要进行输出表格了，则返回null</returns>
        private static List<DBText> GetTextsFromUI(DocumentModifier docMdf)
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
            bool pickFinished = false;
            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    if (e.Input == "Export")
                    {
                        pickFinished = true;
                        ed.WriteMessage("\n开始进行数据的导出");
                        // 考虑强行退出 命令行编辑模式
                        // .....
                    }
                };

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择要进行输出的单行文本 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。

            // 请求在图形区域选择对象
            PromptSelectionResult psr = ed.GetSelection(pso, acSelFtr);

            List<DBText> texts = null;

            if (pickFinished)
            {
                return null;
            }
            // MessageBox.Show($"1. {psr.Status.ToString()}\r\n2. {psr.Value}");

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
                return false;
            }
        }

        #region   --- 

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
                    var app = new Microsoft.Office.Interop.Excel.Application();
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

        #endregion

        #region   --- 从lj表格中获取数据

        public static object[,] GetTableFromTable(double[] yLocations, double xLocations)
        {
            return null;
        }

        #endregion
    }
}