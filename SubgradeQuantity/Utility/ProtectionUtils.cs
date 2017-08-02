using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Redundant;
using eZcad.Utility;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using Line = Autodesk.AutoCAD.DatabaseServices.Line;
using Windows = eZstd.API.Windows;

namespace eZcad.SubgradeQuantity.Utility
{
    /// <summary> 与边坡防护相关的一些通用性的操作 </summary>
    public static class ProtectionUtils
    {
        /// <summary> 可用的防护形式 </summary>
        public static readonly Dictionary<ProtectionStyle, string> AvailableProtections = new Dictionary
            <ProtectionStyle, string>
        {
            {ProtectionStyle.挂网喷锚6, "挂网喷锚（6m锚杆）"},
            {ProtectionStyle.锚杆网格梁, "锚杆网格梁"},
            {ProtectionStyle.浆砌片石, "浆砌片石"},
            {ProtectionStyle.边坡防护1, "边坡防护1"},
            {ProtectionStyle.边坡防护2, "边坡防护2"},
            {ProtectionStyle.边坡防护3, "边坡防护3"}
        };


        /// <summary> 路基工程量计算系统 环境配置 </summary>
        /// <param name="docMdf"></param>
        public static void SubgradeEnvironmentConfiguration(DocumentModifier docMdf)
        {
            Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, SectionInfo.AppNameGeneral);
            Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, SectionInfo.AppNameLeft);
            Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, SectionInfo.AppNameRight);
            // var app = Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, SlopeDataBackup.AppName);
        }

        #region --- 从界面中提取路基或者边坡等对象

        /// <summary> 从界面中提取路基对象 </summary>
        /// <returns></returns>
        public static List<Line> GetSections(Editor ed)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择要提取的横断面轴线 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n选择要提取的横断面轴线 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            // pso.SingleOnly = true;

            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_CenterAxis),
            };

            // 将过滤条件赋值给SelectionFilter对象
            var acSelFtr = new SelectionFilter(filterType);

            var psr = ed.GetSelection(pso, acSelFtr);

            if (psr.Status == PromptStatus.OK)
            {
                return psr.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead) as Line).ToList();
            }
            return null;
        }

        /// <summary> 从整个项目中获取全部的横断面所对应的桩号（不要求界面全部显示）。集合中的元素未按桩号值进行排序 </summary>
        /// <param name="sort">是否要对所有横断面按桩号从小到大排序</param>
        public static SubgradeSection[] GetAllSections(DocumentModifier docMdf, bool sort)
        {
            var stations = new List<SubgradeSection>();

            var res = docMdf.acEditor.SelectAll(SubgradeSection.Filter);
            if (res.Status == PromptStatus.OK)
            {
                var ids = res.Value.GetObjectIds();
                foreach (var id in ids)
                {
                    SubgradeSection centerLine = null;
                    var l = id.GetObject(OpenMode.ForRead) as Line;
                    if (l != null)
                    {
                        var si = SectionInfo.FromCenterLine(l);
                        if (si != null && si.FullyCalculated)
                        {
                            centerLine = new SubgradeSection(docMdf, l, si);
                            stations.Add(centerLine);
                        }
                    }
                    if (centerLine == null)
                    {
                        MessageBox.Show($"某些道路中心线对象所对应的横断面未进行构造，" +
                                        $"\r\n请先调用“{SectionsConstructor.CommandName}”命令，以构造整个项目的横断面系统。",
                            "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            }
            if (sort)
            {
                stations.Sort(ProtectionUtils.CompareStation);
            }
            return stations.ToArray();
        }


        /// <summary> 从界面中搜索边坡线 </summary>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static List<Polyline> GetSlopeLines(Editor ed)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择多条边坡线 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n选择多条边坡线 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            // pso.SingleOnly = true;

            var psr = ed.GetSelection(pso, SlopeLine.Filter);

            if (psr.Status == PromptStatus.OK)
            {
                return psr.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead) as Polyline).ToList();
            }
            return new List<Polyline>();
        }

        /// <summary> 从界面中选择已经构造好的边坡对象。对于已经计算过的边坡，不再进行重新计算，而保留其原有的数据 </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        public static List<SlopeLine> GetExistingSlopeLines(DocumentModifier docMdf)
        {
            // var allSections = ProtectionUtils.GetAllSections(docMdf);
            var slopeLines = ProtectionUtils.GetSlopeLines(docMdf.acEditor);
            var slpLines = new List<SlopeLine>();
            string errMsg;
            foreach (var sl in slopeLines)
            {
                var slpLine = SlopeLine.Create(docMdf, sl, out errMsg);
                if (slpLine != null)
                {
                    // 将存储的数据导入边坡对象
                    slpLine.ImportSlopeData(slpLine.XData);
                    //
                    if (!slpLine.XData.FullyCalculated)
                    {
                        slpLine.CalculateXData();
                    }
                    slpLines.Add(slpLine);
                }
                else
                {
                    docMdf.WriteNow(errMsg);
                }
            }
            return slpLines;
        }
        #endregion

        #region --- Excel 程序

        private static Application _workingApp;

        /// <summary> 获取全局的 Excel 程序 </summary>
        /// <param name="visible"></param>
        /// <returns>获取失败则返回 null</returns>
        public static Application GetExcelApp(bool visible = false)
        {
            if (_workingApp != null)
            {
                var processId = 0;
                var threadId = Windows.GetWindowThreadProcessId(_workingApp.Hwnd, ref processId);
                var pr = Process.GetProcessById(processId);
                if (pr == null || pr.HasExited)
                {
                    _workingApp = null;
                }
                else
                {
                    _workingApp.Visible = visible;
                    return _workingApp;
                }
            }
            if (_workingApp == null)
            {
                _workingApp = new Application { Visible = visible };
            }
            if (_workingApp == null)
            {
                throw new NullReferenceException($"无法打开 Excel 程序!");
            }
            return _workingApp;
        }

        /// <returns>成功则返回 true</returns>
        public static bool KillActiveExcelApp(Application appToKill)
        {
            if (appToKill != null)
            {
                try
                {
                    // excelApp.Quit();
                    var processId = 0;
                    var threadId = Windows.GetWindowThreadProcessId(appToKill.Hwnd, ref processId);
                    var pr = Process.GetProcessById(processId);
                    pr.Kill();
                    //
                    if (appToKill.Equals(_workingApp))
                    {
                        _workingApp = null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary> 通过匹配 Excel 工作表的名称来获取对应的表对象，如果想要的表不存在，则添加一个新的表 </summary>
        /// <param name="wkbk"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static Worksheet GetOrCreateWorkSheet(Workbook wkbk, string sheetName)
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
            }
            return matchedSheet;
        }

        #endregion

        #region --- 桩号处理

        private static readonly Regex StationReg = new Regex(@"K(\d+)\+(\d*.*)"); // K51+223.392

        /// <summary> 将表示里程的字符数据转换为对应的数值 </summary>
        /// <param name="station"></param>
        /// <returns>如果无法正常解析，则返回 null</returns>
        public static double? GetStationFromString(string station)
        {
            var mt = StationReg.Match(station);
            if (mt.Success)
            {
                int k;
                double m;
                if (int.TryParse(mt.Groups[1].Value, out k) && double.TryParse(mt.Groups[2].Value, out m))
                {
                    return k * 1000 + m;
                }
            }
            return null;
        }

        #endregion
        #region --- 各种比较排序


        /// <summary> 桩号小的在前面 </summary>
        public static int CompareStation<T>(StationInfo<T> x, StationInfo<T> y)
        {
            return x.Station.CompareTo(y.Station);
        }
    
        /// <summary> 桩号小的在前面 </summary>
        public static int CompareStation(SubgradeSection slopeData1, SubgradeSection slopeData2)
        {
            return slopeData1.XData.Station.CompareTo(slopeData2.XData.Station);
        }
        /// <summary> 桩号小的在前面 </summary>
        public static int CompareStation(SlopeLine slopeData1, SlopeLine slopeData2)
        {
            return slopeData1.Station.CompareTo(slopeData2.Station);
        }
        /// <summary> 桩号小的在前面 </summary>
        public static int CompareStation(SlopeDataBackup slopeData1, SlopeDataBackup slopeData2)
        {
            return slopeData1.Station.CompareTo(slopeData2.Station);
        }
        #endregion

    }
}