using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using eZcad.SubgradeQuantityBackup.Entities;
using eZcad.SubgradeQuantityBackup.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantityBackup.DataExport
{
    /// <summary>
    /// 将AutoCAD中的与边坡防护相关的工程量数据导出到 Excel 或者 文本中
    /// </summary>
    public partial class Exporter_SlopeProtection : DataExporter
    {
        #region --- Fields

        /// <summary> 用户选择的要提取边坡数据的对象，小桩号位于集合前面。 </summary>
        private readonly SlopeLine[] _slopesToHandle;

        /// <summary> 整个道路中所有断面的左边边坡对象，小桩号位于集合前面。
        /// 集合中包括了所有的桩号，但是对应桩号下的边坡对象可能为null，即此桩号下无对应边坡 </summary>
        private readonly SlopeLine[] _allLeftSlopes;

        private SlopeExpands[] _allLeftSlopeExpands;

        /// <summary> 整个道路中所有断面的右边边坡对象，小桩号位于集合前面。
        /// 集合中包括了所有的桩号，但是对应桩号下的边坡对象可能为null，即此桩号下无对应边坡 </summary>
        private readonly SlopeLine[] _allRightSlopes;

        private SlopeExpands[] _allRightSlopeExpands;

        /// <summary> 整个道路中所有横断面的数据，小桩号位于集合前面 </summary>
        private readonly SectionInfo[] _allSectionDatas;

        /// <summary> 整个道路中所有的桩号，小桩号位于集合前面 </summary>
        private readonly double[] _allStations;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="slopesToHandle">要操作的边坡对象</param>
        /// <param name="allSections"></param>
        public Exporter_SlopeProtection(DocumentModifier docMdf,
            IList<SubgradeSection> allSections, IList<SlopeLine> slopesToHandle) : base(docMdf, allSections)
        {
            _slopesToHandle = slopesToHandle.ToArray();
            Array.Sort(_slopesToHandle, ProtectionUtils.CompareStation);

            _allSectionDatas = allSections.Select(r => r.XData).ToArray();
            _allStations = _allSectionDatas.Select(r => r.Station).ToArray();
            //
            _allLeftSlopes = allSections.Select(r => r.GetSlopeLine(true)).ToArray();
            foreach (var als in _allLeftSlopes.Where(als => als != null))
            {
                als.ImportSlopeData(als.XData);
            }

            _allRightSlopes = allSections.Select(r => r.GetSlopeLine(false)).ToArray();
            foreach (var ars in _allRightSlopes.Where(ars => ars != null))
            {
                ars.ImportSlopeData(ars.XData);
            }

            ExpandSlopes();
        }

        #region --- ExpandSlopes

        private void ExpandSlopes()
        {
            // 整个道路的所有左边边坡
            _allLeftSlopeExpands = new SlopeExpands[_allSectionDatas.Length];
            for (int i = 0; i < _allSectionDatas.Length; i++)
            {
                _allLeftSlopeExpands[i] = new SlopeExpands(_allSectionDatas[i].Station, _allLeftSlopes[i]);
            }
            for (int i = 0; i < _allLeftSlopes.Length - 1; i++)
            {
                var bs = _allLeftSlopeExpands[i];
                var fs = _allLeftSlopeExpands[i + 1];
                var backSlopeInfo = bs.SlopeInfo;
                var backPlatformInfo = bs.PlatformInfo;
                var frontSlopeInfo = fs.SlopeInfo;
                var frontPlatformInfo = fs.PlatformInfo;
                ExpandSlope(bs.Station, bs.XData, fs.Station, fs.XData,
                    ref backSlopeInfo, ref backPlatformInfo, ref frontSlopeInfo, ref frontPlatformInfo);
            }
            ExpandEdge(_allLeftSlopeExpands);

            // 整个道路的所有右边边坡
            _allRightSlopeExpands = new SlopeExpands[_allSectionDatas.Length];
            for (int i = 0; i < _allSectionDatas.Length; i++)
            {
                _allRightSlopeExpands[i] = new SlopeExpands(_allSectionDatas[i].Station, _allRightSlopes[i]);
            }

            for (int i = 0; i < _allRightSlopes.Length - 1; i++)
            {
                var bs = _allRightSlopeExpands[i];
                var fs = _allRightSlopeExpands[i + 1];
                var backSlopeInfo = bs.SlopeInfo;
                var backPlatformInfo = bs.PlatformInfo;
                var frontSlopeInfo = fs.SlopeInfo;
                var frontPlatformInfo = fs.PlatformInfo;
                ExpandSlope(bs.Station, bs.XData, fs.Station, fs.XData,
                    ref backSlopeInfo, ref backPlatformInfo, ref frontSlopeInfo, ref frontPlatformInfo);
            }
            ExpandEdge(_allRightSlopeExpands);
        }

        /// <summary> 根据两个断面的边坡对象，来计算其各自所占的边坡区域（几何计算，无关于防护方式） </summary>
        /// <param name="frontStation"></param>
        /// <param name="frontSlope"></param>
        /// <param name="backStation"></param>
        /// <param name="backSlope"></param>
        /// <param name="backSlopeInfo"></param>
        /// <param name="backPlatformInfo"></param>
        /// <param name="frontSlopeInfo"></param>
        /// <param name="frontPlatformInfo"></param>
        private static void ExpandSlope(double backStation, SlopeData backSlope, double frontStation,
            SlopeData frontSlope,
            ref Dictionary<double, SlopeSegInfo> backSlopeInfo, ref Dictionary<double, SlopeSegInfo> backPlatformInfo,
            ref Dictionary<double, SlopeSegInfo> frontSlopeInfo, ref Dictionary<double, SlopeSegInfo> frontPlatformInfo)
        {
            double length;
            double area;
      
            // 1. 从后往前算
            var ssg = new SlopeSegsGeom(backStation, backSlope, frontStation, frontSlope);
            foreach (var bSlope in backSlope.Slopes)
            {
                ssg.GetBackSlopeLengthAndArea(bSlope, out length, out area);
                var ssi = backSlopeInfo[bSlope.Index];
                ssi.FrontStation = backStation + length;
                ssi.FrontArea = area;
            }
            foreach (var bPltfm in backSlope.Platforms)
            {
                ssg.GetBackPlatformLengthAndArea(bPltfm, out length, out area);
                var ssi = backPlatformInfo[bPltfm.Index];
                ssi.FrontStation = backStation + length;
                ssi.FrontArea = area;
            }
            // 2. 从前往后算
            ssg = new SlopeSegsGeom(frontStation, frontSlope, backStation, backSlope);
            foreach (var fSlope in frontSlope.Slopes)
            {
                ssg.GetBackSlopeLengthAndArea(fSlope, out length, out area);
                var ssi = frontSlopeInfo[fSlope.Index];
                ssi.BackStation = frontStation - length;
                ssi.BackArea = area;
            }
            foreach (var fPltfm in frontSlope.Platforms)
            {
                ssg.GetBackPlatformLengthAndArea(fPltfm, out length, out area);
                var ssi = frontPlatformInfo[fPltfm.Index];
                ssi.BackStation = frontStation - length;
                ssi.BackArea = area;
            }
        }

        private static void ExpandEdge(SlopeExpands[] allSlopeExpands)
        {
            // 起始边界
            var start = allSlopeExpands[0];
            foreach (var s in start.SlopeInfo)
            {
                s.Value.BackStation = start.Station;
                s.Value.BackArea = 0;
            }
            foreach (var s in start.PlatformInfo)
            {
                s.Value.BackStation = start.Station;
                s.Value.BackArea = 0;
            }

            // 结尾边界
            var end = allSlopeExpands[allSlopeExpands.Length - 1];
            foreach (var s in end.SlopeInfo)
            {
                s.Value.FrontStation = end.Station;
                s.Value.FrontArea = 0;
            }
            foreach (var s in end.PlatformInfo)
            {
                s.Value.FrontStation = end.Station;
                s.Value.FrontArea = 0;
            }
        }

        #endregion

        /// <summary> 导出数据 </summary>
        public bool ExportData()
        {
            var slopeDatas = _slopesToHandle.Select(r => r.XData).ToArray();

            //// 所有设置的防护形式
            var protectionTypes = slopeDatas.SelectMany(r => SlopeData.Combine(r.Slopes, r.Platforms, false))
                .Select(r => r.ProtectionMethod).Distinct().ToArray();
            var categorizedProtMtd = CategorizeProtectionMethods(protectionTypes);

            var sheet_Infos = new List<WorkSheetData>();

            // ---------------------------------------------------------------------------------------------------------------
            // 1. 先将所有的数据进行一次性导出
            var header = SlopeData.InfoHeader.Split('\t');
            var allData = SlopeData.GetAllInfo(slopeDatas);
            allData = allData.InsertVector<object, string, object>(true, new[] { header, }, new[] { -1f });

            sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SourceData, "CAD原始数据", allData));

            // ---------------------------------------------------------------------------------------------------------------
            var m = new double[_allStations.Length, 0];
            // 添加一些数据列
            m = m.InsertVector<double, double, double>(false, new[] { _allStations, },
                new[] { -1f });
            sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.AllStations, "桩号", m));

            // ---------------------------------------------------------------------------------------------------------------
            // 道路左侧边坡防护
            // 每一种防护形式所对应的要写入到Excel表中的信息
            var slopeOnleft = _slopesToHandle.Where(r => r.XData.OnLeft).ToArray();
            foreach (var cpm in categorizedProtMtd)
            {
                var leftRange = new List<ProtMtdInfo>();
                foreach (var tp in cpm.Value) // 道路左边边坡中每一种不同的防护形式
                {
                    // 挂网喷锚_6 与 挂网喷锚_8
                    GetMatchedSlopeRanges(slopeOnleft, tp, true, ref leftRange);
                }
                if (leftRange.Count > 0)
                {
                    // 一个防护方式的工作表数据构造完成，下面对其进行排序
                    leftRange.Sort(SortRows);

                    // 将排序后的数据导出
                    var slopesArr = ProtMtdInfo.ConvertToArray(leftRange);
                    var sheetName = $"{cpm.Key}_{"左"}";
                    sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SlopeProtection, sheetName, slopesArr));
                }

            }

            // 道路右侧边坡防护
            var slopeOnRight = _slopesToHandle.Where(r => !r.XData.OnLeft).ToArray();
            foreach (var cpm in categorizedProtMtd)
            {
                var rightRange = new List<ProtMtdInfo>();
                foreach (var tp in cpm.Value) // 道路左边边坡中每一种不同的防护形式
                {
                    // 挂网喷锚_6 与 挂网喷锚_8
                    GetMatchedSlopeRanges(slopeOnRight, tp, false, ref rightRange);
                }
                if (rightRange.Count > 0)
                {
                    // 一个防护方式的工作表数据构造完成，下面对其进行排序
                    rightRange.Sort(SortRows);

                    // 将排序后的数据导出
                    var slopesArr = ProtMtdInfo.ConvertToArray(rightRange);
                    var sheetName = $"{cpm.Key}_{"右"}";
                    sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SlopeProtection, sheetName, slopesArr));
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
            return true;
        }

        #region --- 构造一个工作表的数据

        /// <summary> 构造Excel工作表中的表格数据：边坡防护工程量表 </summary>
        /// <param name="selectedSlopes">路基某一侧的边坡对象</param>
        /// <param name="protectionName">要提取的防护方式，挂网喷锚_6 与 挂网喷锚_8 应分别通过此函数进行计算</param>
        /// <param name="onLeft"></param>
        /// <returns></returns>
        private void GetMatchedSlopeRanges(SlopeLine[] selectedSlopes, string protectionName, bool onLeft,
            ref List<ProtMtdInfo> existingPmis)
        {
            // 一侧边坡中与指定的防护方式精确匹配的子边坡对象

            ProtMtdInfo lastProtMtdInfo = existingPmis.Count > 0 ? existingPmis[existingPmis.Count - 1] : null;
            List<double> matchedSlopes;
            List<double> matchedPlatforms;
            double backEdgeStation;
            double frontEdgeStation;
            double area;
            var ssps = onLeft ? _allLeftSlopeExpands : _allRightSlopeExpands;
            foreach (var ss in selectedSlopes)
            {
                var sExp = ssps.First(r => r.Station == ss.Station);

                var rg = IdentifyProtectionRange(ss.XData, sExp, protectionName,
                    out matchedSlopes, out matchedPlatforms,
                    out backEdgeStation, out frontEdgeStation, out area);

                if (rg != ProtectionRange.None)
                {

                    // 此断面的边坡中包含有与指定防护相匹配的子边坡
                    var nextPmi = new ProtMtdInfo(backEdgeStation, frontEdgeStation, area, protectionName, rg,
                                      matchedSlopes.ToArray(), matchedPlatforms.ToArray());
                    //
                    if (lastProtMtdInfo == null)
                    {
                        lastProtMtdInfo = nextPmi;
                        existingPmis.Add(nextPmi);
                    }
                    else
                    {
                        // 多个子区域进行合并
                        var same = lastProtMtdInfo.Merge(nextPmi);
                        if (!same)
                        {
                            lastProtMtdInfo = nextPmi;
                            existingPmis.Add(nextPmi);
                        }
                    }
                }


            }
        }

        /// <summary> 将所有相互不同的防护方式进行分类。返回的字典中，Key代表某个基本防护方式，其对应的值代表基本类型下的不同规格 </summary>
        /// <param name="allProtMethods">集合中没有相同的元素，但是有类似的，比如“挂网喷锚_6”与“挂网喷锚_8”</param>
        /// <returns></returns>
        private Dictionary<string, List<string>> CategorizeProtectionMethods(string[] allProtMethods)
        {
            var baseProts = new Dictionary<string, List<string>>();
            string baseProt;
            foreach (var pt in allProtMethods)
            {
                var i = pt.IndexOf('_');
                if (i >= 0) // 说明是 “挂网喷锚_6” 的形式
                {
                    baseProt = pt.Substring(0, i);
                    if (!baseProts.Keys.Contains(baseProt))
                    {
                        var bps = new List<string>() { pt };
                        baseProts.Add(baseProt, bps);
                    }
                    else
                    {
                        baseProts[baseProt].Add(pt);
                    }
                }
                else // 说明是 “挂网喷锚” 的形式
                {
                    if (!baseProts.Keys.Contains(pt))
                    {
                        var bps = new List<string>() { pt };
                        baseProts.Add(pt, bps);
                    }
                    else
                    {
                        baseProts[pt].Add(pt);
                    }
                }
            }
            return baseProts;
        }

        /// <summary> 某一个断面边坡中，与指定防护相匹配的子边坡的信息 </summary>
        /// <param name="sd"></param>
        /// <param name="se"></param>
        /// <param name="protectionMethod"></param>
        /// <param name="matchedSlopes">在此断面中，与指定防护相匹配的子边坡的 Index</param>
        /// <param name="matchedPlatforms">在此断面中，与指定防护相匹配的子平台的 Index</param>
        /// <param name="backEdgeStation">在此断面中，指定防护方式所占据的最小的桩号位置</param>
        /// <param name="frontEdgeStation">在此断面中，指定防护方式所占据的最大的桩号位置</param>
        /// <param name="area">在此断面中，指定防护方式在断面左右所占据的总面积</param>
        /// <returns></returns>
        private ProtectionRange IdentifyProtectionRange(SlopeData sd, SlopeExpands se, string protectionMethod,
            out List<double> matchedSlopes, out List<double> matchedPlatforms, out double backEdgeStation,
            out double frontEdgeStation, out double area)
        {
            var rg = ProtectionRange.None;
            matchedSlopes = new List<double>();
            matchedPlatforms = new List<double>();
            frontEdgeStation = AllStations[0];
            backEdgeStation = AllStations[AllStations.Length - 1];
            area = 0;
            //
            bool allSlopes = sd.Slopes.Count > 0;
            foreach (var s in sd.Slopes)
            {
                if (s.ProtectionMethod != protectionMethod)
                {
                    allSlopes = false;
                }
                else
                {
                    var ssinfo = se.SlopeInfo[s.Index];
                    backEdgeStation = Math.Min(backEdgeStation, ssinfo.BackStation);
                    frontEdgeStation = Math.Max(frontEdgeStation, ssinfo.FrontStation);
                    area += ssinfo.BackArea + ssinfo.FrontArea;
                    matchedSlopes.Add(s.Index);
                }
            }

            bool allPlatform = sd.Platforms.Count > 0;
            foreach (var p in sd.Platforms)
            {
                if (p.ProtectionMethod != protectionMethod)
                {
                    allPlatform = false;
                }
                else
                {
                    var ssinfo = se.PlatformInfo[p.Index];
                    backEdgeStation = Math.Min(backEdgeStation, ssinfo.BackStation);
                    frontEdgeStation = Math.Max(frontEdgeStation, ssinfo.FrontStation);
                    area += ssinfo.BackArea + ssinfo.FrontArea;
                    matchedPlatforms.Add(p.Index);
                }
            }

            if (allSlopes)
            {
                rg = allPlatform ? ProtectionRange.AllSection : ProtectionRange.AllSlopes;
            }
            else if (allPlatform)
            {
                rg = ProtectionRange.AllPlatforms;
            }
            else if (matchedSlopes.Count + matchedPlatforms.Count > 0)
            {
                rg = ProtectionRange.PartialSlopeSegs;
            }
            else
            {
                rg = ProtectionRange.None;
            }
            return rg;
        }

        /// <summary> 对最终 Excel 表中的所有行进行排序 </summary>
        /// <returns></returns>
        private int SortRows(ProtMtdInfo range1, ProtMtdInfo range2)
        {
            // 第 1 优先级：桩号小的在前面
            if (range1.StartStation < range2.StartStation)
            {
                return -1;
            }
            // 第 2 优先级：相同防护方式的在前面
            else
            {
                return String.Compare(range1.ProtectionName, range2.ProtectionName, StringComparison.Ordinal);
            }
            return -1;
        }

        #endregion
    }
}