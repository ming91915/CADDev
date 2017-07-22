using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;
using eZstd.MarshalReflection;
using eZstd.Mathematics;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using Exception = System.Exception;
using MessageBox = System.Windows.MessageBox;
using Utils = eZstd.Miscellaneous.Utils;

[assembly: CommandClass(typeof(SlpInfosGetterBackup))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取并计算边坡防护工程量 </summary>
    internal class SlpInfosGetterBackup
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ExportSlopeInfos", CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"边坡表格备份"), Description("将所有的边坡信息提取出来并制成相应表格")]
        public void EcExportSlopeInfos()
        {
            DocumentModifier.ExecuteCommand(ExportSlopeInfos);
        }

        #endregion

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        public void ExportSlopeInfos(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            // 提取边坡数据
            var selectedSlopeDatas = GetAllSlopeData();
            if (selectedSlopeDatas == null || selectedSlopeDatas.Count == 0) return;

            // 所有的断面
            var allMileages = MileagesFinder.GetAllMileages(docMdf);
            if (allMileages == null || allMileages.Length == 0)
            {
                MessageBox.Show("未找到有效的基准横断面的里程数据！");
                return;
            }

            var allMileInfos = allMileages.Select(r => new MileageInfo<double>(r, MileageInfoType.Located, 0)).ToArray();
            // 所有设置的防护形式
            var protectionTypes = selectedSlopeDatas.Select(r => r.Style).Distinct().ToArray();
            var sheet_Infos = new List<WorkSheetData>();

            // ---------------------------------------------------------------------------------------------------------------
            // 1. 先将所有的数据进行一次性导出
            var header = SlopeDataBackup.InfoHeader.Split('\t');
            var allData = SlopeDataBackup.GetAllInfo(selectedSlopeDatas);
            allData = allData.InsertVector<object, string, object>(true, new[] { header, }, new[] { -1f });

            sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SourceData, "CAD原始数据", allData));

            // ---------------------------------------------------------------------------------------------------------------
            var m = new double[allMileages.Length, 0];
            // 添加一些数据列
            m = m.InsertVector<double, double, double>(false, new[] { allMileages, },
                new[] { -1f });
            sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.AllMileages, "桩号", m));

            // ---------------------------------------------------------------------------------------------------------------
            // 道路左侧边坡防护
            // 每一种防护形式所对应的要写入到Excel表中的信息
            var slopeOnleft = selectedSlopeDatas.Where(r => r.OnLeft).ToArray();
            foreach (var tp in protectionTypes) // 道路左边边坡中每一种不同的防护形式
            {
                var slopesArr = ConstructSheetData_SlopeProtection(slopeOnleft, tp, allMileInfos);
                if (slopesArr != null && slopesArr.GetLength(0) > 0)
                {
                    var sheetName = $"{tp}_{"左"}";
                    sheet_Infos.Add(new WorkSheetData(sheetName, slopesArr, tp, true));
                }
            }
            // 道路右侧边坡防护
            var slopeOnRight = selectedSlopeDatas.Where(r => !r.OnLeft).ToArray();
            foreach (var tp in protectionTypes) // 道路右边边坡中每一种不同的防护形式
            {
                var slopesArr = ConstructSheetData_SlopeProtection(slopeOnRight, tp, allMileInfos);
                if (slopesArr != null && slopesArr.GetLength(0) > 0)
                {
                    var sheetName = $"{tp}_{"右"}";
                    sheet_Infos.Add(new WorkSheetData(sheetName, slopesArr, tp, false));
                }
            }
            // ---------------------------------------------------------------------------------------------------------------

            // ---------------------------------------------------------------------------------------------------------------

            // 数据导出
            var errMsg = ExportDataToExcel(sheet_Infos); // , selectedSlopeDatas

            if (errMsg != null)
            {
                var res = MessageBox.Show($"将数据导出到Excel中时出错：{errMsg}，\r\n是否将其以文本的形式导出？", "提示", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);
                if (res == MessageBoxResult.OK)
                {
                    ExportAllDataToDirectory(sheet_Infos);
                }
            }
        }

        #region --- 提取边坡数据

        private List<SlopeDataBackup> GetAllSlopeData()
        {
            var slopes = SlpInfosSetter.GetSlopeLines(_docMdf.acEditor, onlyExisted: true);

            if (slopes != null && slopes.Count > 0)
            {
                var slopeDatas = new List<SlopeDataBackup>();
                // 读取每一个边坡线中的数据
                foreach (var pl in slopes)
                {
                    var buff = pl.GetXDataForApplication(SlopeDataBackup.AppName);
                    if (buff != null)
                    {
                        var slopeData = SlopeDataBackup.FromResultBuffer(buff);
                        // 检查是否有正确的桩号
                        slopeDatas.Add(slopeData);
                    }
                }
                slopeDatas.Sort(comparison: SlopeMileageComparison);
                return slopeDatas;
            }
            return null;
        }

        private static int SlopeMileageComparison(SlopeDataBackup slopeData1, SlopeDataBackup slopeData2)
        {
            return slopeData1.Mileage.CompareTo(slopeData2.Mileage);
        }

        #endregion

        #region --- 构造一个工作表的数据

        /// <summary> 构造Excel工作表中的表格数据：边坡防护工程量表 </summary>
        /// <param name="slopes"></param>
        /// <param name="style"></param>
        /// <param name="allMileInfos"></param>
        /// <returns></returns>
        private Array ConstructSheetData_SlopeProtection(IList<SlopeDataBackup> slopes, ProtectionStyle style,
            IList<MileageInfo<double>> allMileInfos)
        {
            var spData = new List<MileageInfo<double>>();
            //
            var sp_LeftStyles = slopes.Where(r => r.Style == style);

            foreach (var r in sp_LeftStyles)
            {
                MileageInfo<double> mi;
                if (r.FillExcav && ProtectionOptions.ConsiderWaterLevel)
                {
                    mi = new MileageInfo<double>(r.Mileage, MileageInfoType.Measured, r.SlopeLengthBeThinFillTop);
                }
                else
                {
                    mi = new MileageInfo<double>(r.Mileage, MileageInfoType.Measured, r.SlopeLength);
                }
                spData.Add(mi);
            }
            if (!spData.Any()) return null;

            // 对数据进行计算，并构造 Excel 表格中的数据
            spData = Sort_Interpolate(spData, allMileInfos); // 排序与插值
            var segs = Category_Sumup(spData); // 分段并计算对应的面积
                                               //
            var slopesArr = SegmentData<double, double>.ConvertToArr(segs);

            // 桩号信息
            var mileArr =
                segs.Select(r => (
                    $"K{Math.Floor(r.Start / 1000)}+{(r.Start % 1000).ToString("000")}~K{Math.Floor(r.End / 1000)}+{(r.End % 1000).ToString("000")}")
                    as object).ToArray();
            // 桩号分区段的长度
            var segLengthArr = segs.Select(r => (r.End - r.Start) as object).ToArray();

            // 添加一些数据列
            slopesArr = slopesArr.InsertVector<object, object, object>(false, new[] { mileArr, segLengthArr, },
                new[] { 1f, 1.1f });

            // 添加表头信息
            var header = new string[] { "起始", "结束", "桩号", "分段长度", "防护面积" };
            slopesArr = slopesArr.InsertVector<object, string, object>(true, new[] { header },
                new[] { -1f });

            return slopesArr;
        }

        #endregion

        #region --- 排序与插值

        /// <summary>
        /// 对测量里程边坡的数据与路线中所有横断面的里程数据与插值
        /// </summary>
        /// <param name="measuredSlopes">带有测量数据的横断面数据</param>
        /// <param name="locatedSlopes">用来定位的桩号，一般是整个文档中的所有横断面</param>
        /// <returns>返回的集合中包含了图纸中所有的横断面及其对应的边坡防护长度，除此之外，还包含了插值出来的断面</returns>
        private List<MileageInfo<double>> Sort_Interpolate(List<MileageInfo<double>> measuredSlopes, IList<MileageInfo<double>> locatedSlopes)
        {
            measuredSlopes.AddRange(locatedSlopes);
            var slopes = measuredSlopes;
            if (slopes.Count > 0)
            {
                // 1、对横断面数据进行排序，排序后集合中不会有重复的里程
                slopes = Sort_SumDuplicate(slopes);

                // 2、 去掉有测量值的定位断面
                var sortedSections = new Stack<MileageInfo<double>>();
                double duplicateMileage = double.Epsilon;
                for (int i = slopes.Count - 1; i >= 0; i--)
                {
                    var slp = slopes[i];
                    if (slp.Mileage == duplicateMileage)
                    {
                        // 说明出现重复里程，此时从众多重复里程中仅保留有测量值的那个里程断面
                        if (slp.Type == MileageInfoType.Measured)
                        {
                            // 替换掉原集合中的值
                            sortedSections.Peek().Override(slp);
                        }
                    }
                    else
                    {
                        // 说明与上一个桩号不重复
                        sortedSections.Push(slp);
                        duplicateMileage = slp.Mileage;
                    }
                }

                // 3、 进行插值，此时 sortedSlopes 中，较小的里程位于堆的上面，
                // 而且集合中只有“定位”与“测量”两种断面，并没有“插值”断面
                var allSections = new List<MileageInfo<double>>();
                var count = sortedSections.Count;
                var smallestSection = sortedSections.Pop();
                allSections.Add(smallestSection);
                //
                var lastSec = smallestSection;
                var lastType = smallestSection.Type;
                for (int i = 1; i < count; i++)
                {
                    var slp = sortedSections.Pop();

                    if (slp.Type != lastType)
                    {
                        // 说明从定位断面转到了测量断面，或者从测量断面转到了定位断面，此时要进行断面插值
                        var interpMile = (lastSec.Mileage + slp.Mileage) / 2;
                        var interpSec = new MileageInfo<double>(interpMile, MileageInfoType.Interpolated, 0);
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

        /// <summary> 对测量与定位的里程进行排序，并将相同里程的值进行直接相加（模拟同一个里程中，同一位置处，同样的边坡防护形式，分不同的边坡线进行绘制的情况） </summary>
        /// <param name="slopes"></param>
        private List<MileageInfo<double>> Sort_SumDuplicate(List<MileageInfo<double>> slopes)
        {
            slopes.Sort(new MileageCompare<double>());
            // 排序后集合中可能会有重复的里程(比如一个里程中，将一种边坡防护分两级坡分开算)

            // 现在将排序后相同里程的数据进行相加
            var distinctSlopes = new List<MileageInfo<double>>();
            MileageInfo<double> lastSlope = slopes[0];
            double lastMile = lastSlope.Mileage;
            distinctSlopes.Add(lastSlope);
            for (int i = 1; i < slopes.Count; i++)
            {
                var sp = slopes[i];
                if (sp.Mileage == lastMile)
                {
                    lastSlope.Value += sp.Value;
                    if (sp.Type == MileageInfoType.Measured)
                    {
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

        #region --- 分段并计算边坡防护面积

        private List<SegmentData<double, double>> Category_Sumup(IList<MileageInfo<double>> sortedSlopes)
        {
            var slopes = sortedSlopes.Select(r => new KeyValuePair<double, double>(r.Mileage, r.Value)).ToArray();

            if (slopes != null && slopes.Length > 0)
            {
                // 1、对横断面数据进行排序
                Array.Sort(slopes, Comparison);

                // 2、 求分段的面积
                var segs = GetArea(slopes);

                return segs;
            }
            return null;
        }

        private int Comparison(KeyValuePair<double, double> mileLength1, KeyValuePair<double, double> mileLength2)
        {
            return mileLength1.Key.CompareTo(mileLength2.Key);
        }

        /// <summary> 根据桩号与对应的斜坡值来计算分段与对应的面积 </summary>
        /// <param name="mile_length">小桩号在前面</param>
        /// <returns></returns>
        public List<SegmentData<double, double>> GetArea(IList<KeyValuePair<double, double>> mile_length)
        {
            var res = new List<SegmentData<double, double>>();
            if (mile_length.Count < 2)
            {
                throw new InvalidOperationException("必须指定至少两个桩号才能计算分段面积");
            }
            var lastMl = mile_length[0]; // 上一个桩号
            var startMile = lastMl.Key;
            bool lastIsZero = Math.Abs(lastMl.Value) < 0.0001;

            var area = 0.0; // 分段面积
            for (int i = 1; i < mile_length.Count; i++)
            {
                var ml = mile_length[i];
                var m = ml.Key; // 里程桩号
                var l = ml.Value; // 斜坡长度
                                  // 求梯形面积
                area += (lastMl.Value + l) * (m - lastMl.Key) / 2;
                //
                var thisIsZero = Math.Abs(l) < 0.0001;
                if (lastIsZero ^ thisIsZero) // 
                {
                    if (thisIsZero) // 说明到了分段的终点
                    {
                        res.Add(new SegmentData<double, double>(startMile, m, area));
                        area = 0;
                    }
                    else // 说明到了分段的起点
                    {
                        startMile = lastMl.Key;
                    }
                }
                else
                {
                }
                lastIsZero = thisIsZero;
                lastMl = ml;
            }
            // 对最后一个桩号进行操作，即最后一个桩号非零的情况下，其面积还没有闭合
            if (!lastIsZero)
            {
                res.Add(new SegmentData<double, double>(startMile, lastMl.Key, area));
            }
            return res;
        }

        #endregion

        #region --- 分段并计算高填深挖

        #endregion

        #region --- 数据导出到 Excel

        private string ExportDataToExcel(List<WorkSheetData> sheet_Infos)
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

                    //// 1. 先将所有的数据进行一次性导出
                    //var sht = GetWorkSheet(wkbk, "CAD原始数据");
                    //var header = SlopeData.InfoHeader.Split('\t');
                    //RangeValueConverter.FillRange(sht, startRow: 1, startCol: 1, arr: header,
                    //          colPrior: false);
                    //var allData = SlopeData.GetAllInfo(allSlopeDatas);
                    //RangeValueConverter.FillRange(sht, startRow: 2, startCol: 1, arr: allData,
                    //           colPrior: false);
                    //var rg = sht.UsedRange;
                    //rg.EntireColumn.AutoFit();

                    //
                    foreach (var tpInfo in sheet_Infos)
                    {
                        var sht = GetWorkSheet(wkbk, tpInfo.SheetName);
                        if (sht != null)
                        {
                            RangeValueConverter.FillRange(sht, startRow: 1, startCol: 1, arr: tpInfo.Data,
                                colPrior: false);
                            sht.UsedRange.EntireColumn.AutoFit();

                            sht.Activate();
                            var w = excelApp.ActiveWindow as Microsoft.Office.Interop.Excel.Window;
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

        public Workbook GetExcelWorkbook()
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
                        excelApp = new Application() { Visible = false };
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

        public Worksheet GetWorkSheet(Workbook wkbk, string sheetName)
        {
            Worksheet matchedSheet = null;
            foreach (var obj in wkbk.Worksheets)
            {
                var sht = obj as Worksheet;
                if (sht != null && sht.Name.Equals(sheetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    matchedSheet = sht;
                    break;
                }
            }
            if (matchedSheet == null)
            {
                matchedSheet = wkbk.Worksheets.Add();
                matchedSheet.Name = sheetName;
                matchedSheet.Activate();
            }
            return matchedSheet;
        }

        #endregion

        #region --- 数据导出到文本

        private void ExportAllDataToDirectory(List<WorkSheetData> typeInfos)
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

        private void ExportAllDataToTxt(IList<SlopeDataBackup> slopeDatas, string infoPath)
        {
            // var infoPath = Utils.ChooseSaveFile("数据输出的文本", "文本(*.txt) | *.txt");
            if (infoPath == null) return;
            //
            var sb = new StringBuilder();
            sb.AppendLine(SlopeDataBackup.InfoHeader);
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