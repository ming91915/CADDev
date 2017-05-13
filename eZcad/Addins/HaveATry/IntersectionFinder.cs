using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad;
using eZstd.Drawing;
using eZstd.Miscellaneous;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;
using Line = Autodesk.AutoCAD.DatabaseServices.Line;

namespace eZcad.Addins.HaveATry
{
    internal class IntersectionFinder
    {
        public void Execute(DocumentModifier docMdf)
        {
            List<Item> items = null;
            try
            {
                items = GetItemsFromExcel();
            }
            catch (Exception ex)
            {
                DebugUtils.ShowDebugCatch(ex, "提取Excel中的信息出错");
            }
            if (items != null && items.Count > 0)
            {
                var categories = items.Select(r => r.Category).Distinct().ToArray();
                var paperIdss = items.Select(r => r.PaperId).Distinct().ToArray();
                var minMileage = items.Select(r => r.Start).Min();
                var maxMileage = items.Select(r => r.End).Max();

                var paperIds = new Dictionary<string, ObjectIdCollection>();
                foreach (var paperId in paperIdss)
                {
                    paperIds.Add(paperId, new ObjectIdCollection());
                }
                // 以只读方式打开块表   Open the Block table for read
                var acBlkTbl =
                    docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;

                // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                var acBlkTblRec =
                    docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                // 绘制基本坐标系与分界线等
                ConstructWorld(docMdf, acBlkTblRec, categories, minMileage, maxMileage);

                //
                FindIntersections(docMdf, acBlkTblRec, items, categories, paperIds);


                // 将每一个单子中的对象添加到一个组Group中
                var groups = docMdf.acTransaction.GetObject(docMdf.acDataBase.GroupDictionaryId,
                    OpenMode.ForWrite) as DBDictionary;

                var colors = ColorUtils.ClassicalExpand(paperIds.Count);
                var ind = 0;

                foreach (var paperId in paperIds)
                {
                    var gp = new Group(paperId.Key, true);

                    groups.SetAt(paperId.Key, gp);

                    gp.Append(paperId.Value);
                    // gp.Selectable = false; // 默认为 true，如果设置为 false，则即使 PICKSTYLE 系统变量设置为1，这个 group 也不能被作为一个整体进行选择。
                    docMdf.WriteLineIntoDebuger("添加组 ", gp.Name, "成员个数：", gp.GetAllEntityIds().Length.ToString());
                    gp.SetColor(Color.FromColor(colors[ind]));
                    //
                    docMdf.acTransaction.AddNewlyCreatedDBObject(gp, true);
                    ind += 1;
                }
            }
        }

        private void ConstructWorld(DocumentModifier docMdf, BlockTableRecord btr, string[] categories, double minMile,
            double maxMile)
        {
            var l = new Line(new Point3d(0, 0, 0), new Point3d(maxMile, 0, 0));
            btr.AppendEntity(l);
            docMdf.acTransaction.AddNewlyCreatedDBObject(l, true);
            //
            var txt = new DBText
            {
                TextString = "左",
                Height = Item.TextHeight,
                HorizontalMode = TextHorizontalMode.TextRight,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                AlignmentPoint = new Point3d(0, Item.BarHeight/2, 0)
            };
            btr.AppendEntity(txt);
            docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);
            txt = new DBText
            {
                TextString = "右",
                Height = Item.TextHeight,
                HorizontalMode = TextHorizontalMode.TextRight,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                AlignmentPoint = new Point3d(0, -Item.BarHeight/2, 0)
            };
            btr.AppendEntity(txt);
            docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);
            //
            foreach (var category in categories)
            {
                var middleVLeft = Item.GetMiddleV(categories, category, true);
                var middleVRight = Item.GetMiddleV(categories, category, false);

                docMdf.WriteLineIntoDebuger(middleVRight.ToString(), middleVLeft.ToString());

                txt = new DBText
                {
                    TextString = category,
                    Height = Item.TextHeight,
                    HorizontalMode = TextHorizontalMode.TextRight,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = new Point3d(0, middleVLeft, 0)
                };
                btr.AppendEntity(txt);
                docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);

                txt = new DBText
                {
                    TextString = category,
                    Height = Item.TextHeight,
                    HorizontalMode = TextHorizontalMode.TextRight,
                    VerticalMode = TextVerticalMode.TextVerticalMid,
                    AlignmentPoint = new Point3d(0, middleVRight, 0)
                };
                btr.AppendEntity(txt);
                docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="items"></param>
        /// <param name="categories"></param>
        /// <param name="paperIds"></param>
        private void FindIntersections(DocumentModifier docMdf, BlockTableRecord btr, List<Item> items,
            string[] categories, Dictionary<string, ObjectIdCollection> paperIds)
        {
            // 对每一条数据绘制相应的矩形条与填充，最后将其添加到一个组中
            foreach (var item in items)
            {
                var ids = item.Draw(docMdf.acTransaction, btr, categories);

                // 
                var idss = paperIds[item.PaperId];
                foreach (ObjectId id in ids)
                {
                    idss.Add(id);
                }
                // 
            }
        }

        /// <summary> 从Excel表格中提取信息 </summary>
        public static List<Item> GetItemsFromExcel()
        {
            var xlsFile = Utils.ChooseOpenFile("的Excel文件",
                "Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls", false);
            if (xlsFile == null) return null;
            var xls = xlsFile[0];
            if (!File.Exists(xls))
            {
                return null;
            }
            // 打开Excel文件 并 提取表格中的数据
            var app = new Application {Visible = false};
            var wkbk = app.Workbooks.Open(xls, ReadOnly: true);
            var sht = wkbk.Worksheets[1] as Worksheet;
            var xlsV = sht.UsedRange.Value;
            wkbk.Close(false);
            app.Quit();

            var table = RangeValueConverter.GetRangeValue<object>(xlsV) as object[,];
            // 解析表格数据
            var rowCount = table.GetLength(0);
            var colCount = table.GetLength(1);


            //var sb = eZstd.Miscellaneous.DebugUtils.PrintArray(table);
            //MessageBox.Show(sb.ToString());


            // 有效数据有6列，第一行为表头，
            var lastPaperId = table[1, 0].ToString();
            var items = new List<Item>();
            for (var i = 1; i < rowCount; i++)
            {
                double start = 0;
                // 退出准则：没有分类数据。（可能在Excel表中出现了无效行）
                if (table[i, 2] == null || !double.TryParse(table[i, 2].ToString(), out start))
                {
                    break;
                }

                // MessageBox.Show($"{i},{table[i, 0]},{table[i, 1]},{table[i, 2]},{table[i, 3]},{table[i, 4]},{table[i, 5]}");

                var paperId = table[i, 0] == null || string.IsNullOrEmpty(table[i, 0].ToString())
                    ? lastPaperId
                    : table[i, 0].ToString();

                lastPaperId = paperId;
                //
                var it = new Item();

                it.PaperId = paperId;
                it.Subject = table[i, 1].ToString() == "设计通知单" ? Subject.Inform : Subject.Request;
                it.Start = (double) table[i, 2];
                it.End = (double) table[i, 3];
                it.LeftSide = table[i, 4] != null && table[i, 4].ToString() == "左";
                it.Category = table[i, 5].ToString();

                items.Add(it);
            }
            return items;
        }
    }
}