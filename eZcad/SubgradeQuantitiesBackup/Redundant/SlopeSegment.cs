using System;
using System.Collections.Generic;
using System.IO;
using eZcad.SubgradeQuantityBackup.Redundant;
using eZcad.SubgradeQuantityBackup.Utility;
using eZstd.Miscellaneous;
using eZx_API.Entities;
using Microsoft.Office.Interop.Excel;

namespace eZcad.SubgradeQuantityBackup.Redundant
{
    /// <summary> 边坡防护的分区信息 </summary>
    public class SlopeSegment
    {
        /// <summary> 起始桩号 </summary>
        public double StartMile { get; private set; }

        /// <summary> 结尾桩号 </summary>
        public double EndMile { get; private set; }

        /// <summary> true表示此防护形式适用于道路左侧，false表示此防护形式适用于道路右侧，null表示此防护形式同时适用于道路左侧与右侧 </summary>
        public bool? OnLeft { get; private set; }

        /// <summary> 防护形式 </summary>
        public ProtectionStyle Style { get; private set; }

        /// <summary> 构造函数 </summary>
        /// <param name="startMile"></param>
        /// <param name="endMile"></param>
        /// <param name="onLeft"></param>
        /// <param name="style"></param>
        public SlopeSegment(double startMile, double endMile, bool? onLeft, ProtectionStyle style)
        {
            StartMile = startMile;
            EndMile = endMile;
            OnLeft = onLeft;
            Style = style;
        }

        #region --- 从Excel中读取边坡分段信息

        /// <summary> 从Excel中读取边坡分段信息 </summary>
        public static List<SlopeSegment> GetSlopeSegmentsFromExcel()
        {
            var sss = new List<SlopeSegment>();
            var wkbk = GetExcelWorkbook();
            if (wkbk != null)
            {
                Worksheet sht = wkbk.Worksheets[1];
                var v = sht.UsedRange.Value;
                object[,] arr = RangeValueConverter.GetRangeValue<object>(v);

                // 第一行为表头，不进行解析
                for (int r = 1; r < arr.GetLength(0); r++)
                {
                    SlopeSegment ss = null;
                    try
                    {
                        if (arr[r, 0] == null || arr[r, 0] == null) break;

                        double startM = (double) arr[r, 0];
                        double endM = (double) arr[r, 1];
                        bool? onLeft = null;
                        if (arr[r, 2] != null)
                        {
                            if (arr[r, 2].ToString() == "左")
                            {
                                onLeft = true;
                            }
                            else if (arr[r, 2].ToString() == "右")
                            {
                                onLeft = false;
                            }
                        }

                        var ps = (ProtectionStyle) Enum.Parse(typeof (ProtectionStyle), arr[r, 3].ToString());
                        ss = new SlopeSegment(startM, endM, onLeft, ps);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                    if (ss != null)
                    {
                        sss.Add(ss);
                    }
                }
                // 关闭工作簿
                try
                {
                    var excelApp = wkbk.Application;
                    wkbk.Close(SaveChanges: false);
                    var succ = ProtectionUtils.KillActiveExcelApp(excelApp);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            //
            return sss;
        }

        private static Workbook GetExcelWorkbook()
        {
            var filePaths = Utils.ChooseOpenFile(
                title: "选择存储有边坡防护分区信息的表格",
                filter: "Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls",
                multiselect: false);
            // Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls
            Workbook wkbk = null;
            if (filePaths != null && File.Exists(filePaths[0]))
            {
                var filePath = filePaths[0];
                try
                {
                    // excelApp = excelApp ?? new Application() { Visible = false };
                    var excelApp = ProtectionUtils.GetExcelApp();
                    wkbk = excelApp.Workbooks.Open(filePath, ReadOnly: true);
                    wkbk.Application.Windows[wkbk.Name].Visible = true;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return wkbk;
        }

        #endregion

        public bool ModifySlopeLine(SlopeLineBackup sl)
        {
            var xd = sl.XData;
            var m = xd.Station;
            if (m >= StartMile && m <= EndMile
                && (!OnLeft.HasValue || xd.OnLeft == OnLeft.Value))
            {
                xd.Style = Style;
                return true;
            }
            else
            {
                // 边坡线不在此区间内
                return false;
            }
        }
    }
}