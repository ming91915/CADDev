using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.MarshalReflection;
using eZstd.Mathematics;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using MessageBox = System.Windows.MessageBox;
using Utils = eZstd.Miscellaneous.Utils;
using Window = Microsoft.Office.Interop.Excel.Window;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 将AutoCAD中的相关工程量数据导出到 Excel 或者 文本中 </summary>
    public class DataExporter
    {
        #region ---   Types

        /// <summary> 可以执行插值操作的断面数据 </summary>
        public interface IInterpolatableSection
        {
            /// <summary> 断面桩号 </summary>
            double Mileage { get; }

            /// <summary> 在两个断面之间进行插值，以生成一个新的用来计算的断面 </summary>
            IInterpolatableSection InterpolateWith(IInterpolatableSection section2);
        }
        #endregion

        #region ---   Fields

        protected readonly DocumentModifier _docMdf;
        protected readonly IList<SubgradeSection> _sectionsToHandle;

        /// <summary> 整个项目中的所有横断面 </summary>
        protected readonly IList<SubgradeSection> _allSections;

        protected readonly double[] _allMileages;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="sectionsToHandle">要进行处理的断面</param>
        /// <param name="allSections">整个项目中的所有断面 </param>
        public DataExporter(DocumentModifier docMdf, IList<SubgradeSection> sectionsToHandle,
            IList<SubgradeSection> allSections)
        {
            _docMdf = docMdf;
            _sectionsToHandle = sectionsToHandle;
            _allSections = allSections;
            //
            _allMileages = allSections.Select(r => r.XData.Mileage).ToArray();
            ;
        }

        #region --- 排序与插值

        /// <summary>
        /// 对测量里程边坡的数据与路线中所有横断面的里程数据与插值
        /// </summary>
        /// <param name="measuredSlopes">带有测量数据的横断面数据</param>
        /// <param name="locatedSlopes">用来定位的桩号，一般是整个文档中的所有横断面</param>
        /// <returns>返回的集合中包含了图纸中所有的横断面及其对应的边坡防护长度，除此之外，还包含了插值出来的断面</returns>
        protected List<MileageInfo<T>> Sort_Interpolate<T>(List<MileageInfo<T>> measuredSlopes,
            IList<MileageInfo<T>> locatedSlopes) where T : IInterpolatableSection
        {
            measuredSlopes.AddRange(locatedSlopes);
            var slopes = measuredSlopes;
            if (slopes.Count > 0)
            {
                // 1、对横断面数据进行排序，排序后集合中不会有重复的里程
                slopes = Sort_FilterMeasured(slopes);
                // 此时的集合中包含 整个项目的所有断面的数据，有的断面有测量数据，而有的只是用来标识或插值。集合中，桩号小的元素下标值较小

                //// 2、 去掉有测量值的定位断面
                //var sortedSections = new Stack<MileageInfo<T>>();
                //double duplicateMileage = double.Epsilon;
                //for (int i = slopes.Count - 1; i >= 0; i--)
                //{
                //    var slp = slopes[i];
                //    if (slp.Mileage == duplicateMileage)
                //    {
                //        // 说明出现重复里程，此时从众多重复里程中仅保留有测量值的那个里程断面
                //        if (slp.Type == MileageInfoType.Measured)
                //        {
                //            // 替换掉原集合中的值
                //            sortedSections.Peek().Override(slp);
                //        }
                //    }
                //    else
                //    {
                //        // 说明与上一个桩号不重复
                //        sortedSections.Push(slp);
                //        duplicateMileage = slp.Mileage;
                //    }
                //}

                // 3、 进行插值，此时 sortedSlopes 中，较小的里程位于堆的上面，
                // 而且集合中只有“定位”与“测量”两种断面，并没有“插值”断面
                var allSections = new List<MileageInfo<T>>();
                var count = slopes.Count;
                var smallestSection = slopes[0];
                allSections.Add(smallestSection);
                //
                var lastSec = smallestSection;
                var lastType = smallestSection.Type;
                for (int i = 1; i < count; i++)
                {
                    var slp = slopes[i];

                    if (slp.Type != lastType)
                    {
                        // 说明从定位断面转到了测量断面，或者从测量断面转到了定位断面，此时要进行断面插值
                        // var interpMile = (lastSec.Mileage + slp.Mileage) / 2;
                        var interpObj = (T)lastSec.Value.InterpolateWith(slp.Value);
                        var interpSec = new MileageInfo<T>(interpObj.Mileage, MileageInfoType.Interpolated, interpObj);
                        allSections.Add(interpSec);
                    }
                    //
                    allSections.Add(slp);

                    lastSec = slp;
                    lastType = slp.Type;
                }
                return allSections;

                // 4、将结果写回 Excel
                //var slopesArr = MileageInfo.ConvertToArr(allSections);
                //var sht = _excelApp.ActiveSheet;
                //;
                //RangeValueConverter.FillRange(sht, topLeftCell.Row, topLeftCell.Column + 3, slopesArr, false);
            }

            return null;
        }

        /// <summary> 对测量与定位的里程进行排序，并将相同里程的值进行过滤，多个重复项中只保存有测量值的项 </summary>
        /// <param name="sections"></param>
        /// <remarks>整个项目的所有断面的数据，有的断面有测量数据，而有的只是用来标识或插值。集合中，桩号小的元素下标值较小</remarks>
        private List<MileageInfo<T>> Sort_FilterMeasured<T>(List<MileageInfo<T>> sections)
        {
            sections.Sort(new MileageCompare<T>());
            // 排序后集合中可能会有重复的里程(比如一个里程中，将一种边坡防护分两级坡分开算)

            // 现在将排序后相同里程的数据进行相加
            var distinctSlopes = new List<MileageInfo<T>>();
            var lastSlope = sections[0];
            double lastMile = lastSlope.Mileage;
            distinctSlopes.Add(lastSlope);
            for (int i = 1; i < sections.Count; i++)
            {
                var sp = sections[i];
                if (sp.Mileage == lastMile)
                {
                    if (sp.Type == MileageInfoType.Measured)
                    {
                        lastSlope.Value = sp.Value;
                        lastSlope.Type = MileageInfoType.Measured;
                    }
                }
                else
                {
                    lastSlope = sp;
                    lastMile = sp.Mileage;
                    distinctSlopes.Add(lastSlope);
                }
            }

            return distinctSlopes;
        }

        #endregion

        #region --- 数据导出到 Excel

        /// <summary> 将所有表格中记录的数据导出到指定Excel工作簿的多个工作表中 </summary>
        /// <param name="sheet_Infos"></param>
        /// <returns></returns>
        protected static string ExportDataToExcel(List<WorkSheetData> sheet_Infos)
        {
            string errMsg = null;
            Application excelApp = null;
            try
            {
                var wkbk = GetExcelWorkbook();
                if (wkbk != null)
                {
                    excelApp = wkbk.Application;
                    excelApp.ScreenUpdating = false;

                    //
                    foreach (var tpInfo in sheet_Infos)
                    {
                        var sht = ProtectionUtils.GetOrCreateWorkSheet(wkbk, tpInfo.SheetName);
                        if (sht != null)
                        {
                            RangeValueConverter.FillRange(sht, startRow: 1, startCol: 1, arr: tpInfo.Data,
                                colPrior: false);
                            sht.UsedRange.EntireColumn.AutoFit();

                            sht.Activate();
                            var w = excelApp.ActiveWindow as Window;
                            w.SplitColumn = 0;
                            w.SplitRow = 1;
                            w.FreezePanes = true;
                        }
                        else
                        {
                            errMsg = $"未找到工作表：{tpInfo.SheetName}";
                        }
                    }
                    //
                    wkbk.Save();
                }
                else
                {
                    errMsg = $"未能打开或者创建Excel工作簿";
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                if (excelApp != null)
                {
                    excelApp.Visible = true;
                    excelApp.ScreenUpdating = true;
                }
            }
            return errMsg;
        }

        private static Workbook GetExcelWorkbook()
        {
            var filePath = Utils.ChooseSaveFile(title: "将数据保存到Excel中", filter: "Excel文件(*.xls)| *.xls");
            // Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls
            Workbook wkbk = null;
            if (filePath != null)
            {
                Application excelApp = null;
                bool fileExists = File.Exists(filePath);
                try
                {
                    if (fileExists)
                    {
                        wkbk = Interaction.GetObjectFromFile<Workbook>(filePath);
                        excelApp = wkbk.Application;
                        excelApp.Visible = false;
                    }
                    else
                    {
                        excelApp = ProtectionUtils.GetExcelApp(visible: false);
                        // excelApp = new Application() { Visible = false };
                        wkbk = excelApp.Workbooks.Add();
                        wkbk.SaveAs(Filename: filePath, FileFormat: XlFileFormat.xlAddIn8);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                if (wkbk != null)
                {
                    wkbk.Application.Windows[wkbk.Name].Visible = true;
                }
            }
            return wkbk;
        }

        #endregion

        #region --- 数据导出到文本

        /// <summary>
        /// 将所有表格中记录的数据导出到指定文件夹中的多个文本中
        /// </summary>
        /// <param name="typeInfos"></param>
        protected static void ExportAllDataToDirectory(List<WorkSheetData> typeInfos)
        {
            string dirPath = null;
            var fbd = new FolderBrowserDialog()
            {
                Description = @"选择一个文件夹，以导出所有的边坡防护数据",
                ShowNewFolderButton = true,
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                dirPath = fbd.SelectedPath;
            }
            if (dirPath != null)
            {
                // 1. 先将所有的数据进行一次性导出
                // ExportAllDataToTxt(allSlopeDatas, "CAD原始数据.txt");
                const char sep = ',';
                // 2. 将计算后的数据分开到多个文件中导出
                foreach (var ti in typeInfos)
                {
                    var data = ti.Data;
                    var sb = new StringBuilder();
                    for (int r = 0; r < data.GetLength(0); r++)
                    {
                        for (int c = 0; c < data.GetLength(1); c++)
                        {
                            sb.Append(data.GetValue(r, c).ToString() + sep);
                        }
                        sb.AppendLine();
                    }

                    var infoPath = Path.Combine(dirPath, ti.SheetName + ".csv");
                    using (var sw = new StreamWriter(infoPath, append: false, encoding: Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                    }
                }
            }
        }

        private static void ExportAllDataToTxt(IList<SlopeData> slopeDatas, string infoPath)
        {
            // var infoPath = Utils.ChooseSaveFile("数据输出的文本", "文本(*.txt) | *.txt");
            if (infoPath == null) return;
            //
            var sb = new StringBuilder();
            sb.AppendLine(SlopeData.InfoHeader);
            foreach (var data in slopeDatas)
            {
                sb.AppendLine(data.GetInfo());
            }
            using (var sw = new StreamWriter(infoPath, append: false, encoding: Encoding.UTF8))
            {
                sw.Write(sb.ToString());
            }
        }

        #endregion
    }
}