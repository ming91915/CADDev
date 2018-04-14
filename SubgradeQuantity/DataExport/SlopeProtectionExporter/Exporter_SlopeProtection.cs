using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
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
        private readonly SectionInfo[] AllSectionDatas;


        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="slopesToHandle">要操作的边坡对象</param>
        /// <param name="allSections"></param>
        public Exporter_SlopeProtection(DocumentModifier docMdf,
            IList<SubgradeSection> allSections, IList<SlopeLine> slopesToHandle) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            _slopesToHandle = slopesToHandle.ToArray();
            Array.Sort(_slopesToHandle, SQUtils.CompareStation);

            AllSectionDatas = allSections.Select(r => r.XData).ToArray();
            //
            _allLeftSlopes = new SlopeLine[allSections.Count];
            _allRightSlopes = new SlopeLine[allSections.Count];
            SlopeData data;
            for (int i = 0; i < allSections.Count; i++)
            {
                var sec = allSections[i];
                //
                var slp = sec.GetSlopeLine(true);
                _allLeftSlopes[i] = slp;
                if (slp != null)
                {
                    data = slp.XData;
                    slp.ImportSlopeData(data);
                    if (!data.OnLeft)
                    {
                        throw new ArgumentException($"{data.Station} 断面中的左侧边坡的数据错误地记录为右侧边坡，请对此边坡进行重新构造（删除后重新创建）。");
                    }
                }
                //
                slp = sec.GetSlopeLine(false);
                _allRightSlopes[i] = slp;
                if (slp != null)
                {
                    data = slp.XData;
                    slp.ImportSlopeData(data);
                    if (data.OnLeft)
                    {
                        throw new ArgumentException($"{data.Station} 断面中的右侧边坡的数据错误地记录为左侧边坡，请对此边坡进行重新构造（删除后重新创建）。");
                    }
                }
            }
            //
            ExpandSlopes();
        }

        #region --- ExpandSlopes 构建每一个子边坡所占据的几何区域

        private void ExpandSlopes()
        {
            // 整个道路的所有左边边坡
            _allLeftSlopeExpands = new SlopeExpands[AllSectionDatas.Length];
            for (int i = 0; i < AllSectionDatas.Length; i++)
            {
                _allLeftSlopeExpands[i] = new SlopeExpands(AllSectionDatas[i].Station, _allLeftSlopes[i], onLeft: true);
            }
            for (int i = 0; i < _allLeftSlopes.Length - 1; i++)
            {
                var bs = _allLeftSlopeExpands[i];
                var fs = _allLeftSlopeExpands[i + 1];
                var backSlopeInfo = bs.SlopeInfo;
                var backPlatformInfo = bs.PlatformInfo;
                var frontSlopeInfo = fs.SlopeInfo;
                var frontPlatformInfo = fs.PlatformInfo;
                ExpandSlope(bs.Station, bs.XData,
                    fs.Station, fs.XData,
                    ref backSlopeInfo, ref backPlatformInfo, ref frontSlopeInfo, ref frontPlatformInfo);
            }
            CutByBlocks(_allLeftSlopeExpands);
            ExpandEdge(_allLeftSlopeExpands);

            // 整个道路的所有右边边坡
            _allRightSlopeExpands = new SlopeExpands[AllSectionDatas.Length];
            for (int i = 0; i < AllSectionDatas.Length; i++)
            {
                _allRightSlopeExpands[i] = new SlopeExpands(AllSectionDatas[i].Station, _allRightSlopes[i], onLeft: false);
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
            CutByBlocks(_allRightSlopeExpands);
            ExpandEdge(_allRightSlopeExpands);
        }

        /// <summary> 根据两个断面的边坡对象，来计算其各自所占的边坡区域（几何计算，无关于防护方式） </summary>
        /// <param name="backStation"></param>
        /// <param name="backSlopeData"></param>
        /// <param name="frontStation"></param>
        /// <param name="frontSlopeData"></param>
        /// <param name="backSlopeInfo"></param>
        /// <param name="backPlatformInfo"></param>
        /// <param name="frontSlopeInfo"></param>
        /// <param name="frontPlatformInfo"></param>
        private static void ExpandSlope(double backStation, SlopeData backSlopeData,
            double frontStation, SlopeData frontSlopeData,
            ref Dictionary<double, SlopeSegInfo> backSlopeInfo, ref Dictionary<double, SlopeSegInfo> backPlatformInfo,
            ref Dictionary<double, SlopeSegInfo> frontSlopeInfo, ref Dictionary<double, SlopeSegInfo> frontPlatformInfo)
        {
            double length;
            double area;

            // 1. 从后往前算
            var ssg = new SlopeSegsGeom(backStation, backSlopeData, frontStation, frontSlopeData);
            foreach (var bSlope in backSlopeData.Slopes)
            {
                ssg.GetBackSlopeLengthAndArea(bSlope, out length, out area);
                var ssi = backSlopeInfo[bSlope.Index];
                ssi.FrontStation = backStation + length;
                ssi.FrontArea = area;
            }
            foreach (var bPltfm in backSlopeData.Platforms)
            {
                ssg.GetBackPlatformLengthAndArea(bPltfm, out length, out area);
                var ssi = backPlatformInfo[bPltfm.Index];
                ssi.FrontStation = backStation + length;
                ssi.FrontArea = area;
            }
            // 2. 从前往后算
            ssg = new SlopeSegsGeom(frontStation, frontSlopeData, backStation, backSlopeData);
            foreach (var fSlope in frontSlopeData.Slopes)
            {
                ssg.GetBackSlopeLengthAndArea(fSlope, out length, out area);
                var ssi = frontSlopeInfo[fSlope.Index];
                ssi.BackStation = frontStation - length;
                ssi.BackArea = area;
            }
            foreach (var fPltfm in frontSlopeData.Platforms)
            {
                ssg.GetBackPlatformLengthAndArea(fPltfm, out length, out area);
                var ssi = frontPlatformInfo[fPltfm.Index];
                ssi.BackStation = frontStation - length;
                ssi.BackArea = area;
            }
        }

        /// <summary> 对于道路边界进行截断 </summary>
        /// <param name="allSlopeExpands"></param>
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


        /// <summary> 对于桥梁隧道短链等区间进行截断 </summary>
        private bool CutByBlocks(SlopeExpands[] allSlopeExpands)
        {
            var blocks = Options.Options_Collections.RangeBlocks;
            SlopeData slopeData;
            foreach (var block in blocks)
            {
                // 桥梁后面的横断面（断面桩号较小）
                var bs = allSlopeExpands.FirstOrDefault(r => Math.Abs(r.Station - block.ConnectedBackStaion) < SQConstants.RangeMergeTolerance);
                if (bs != null)
                {
                    slopeData = bs.XData;
                    // Block 与最近断面桩号间的距离
                    var stationDist = block.StartStation - slopeData.Station;
                    foreach (var fSlope in slopeData.Slopes)
                    {
                        var ssi = bs.SlopeInfo[fSlope.Index];
                        ssi.FrontStation = block.StartStation;
                        // 直接用（斜坡长度*桩号距离）作为面积
                        ssi.FrontArea = fSlope.ProtectionLength * stationDist;
                    }
                    foreach (var fSlope in slopeData.Platforms)
                    {
                        var ssi = bs.PlatformInfo[fSlope.Index];
                        ssi.FrontStation = block.StartStation;
                        // 直接用（斜坡长度*桩号距离）作为面积
                        ssi.FrontArea = fSlope.ProtectionLength * stationDist;
                    }
                }

                // 桥梁前面的横断面（断面桩号较大）
                var fs = allSlopeExpands.FirstOrDefault(r => Math.Abs(r.Station - block.ConnectedFrontStaion) < SQConstants.RangeMergeTolerance);
                if (fs != null)
                {
                    slopeData = fs.XData;
                    // Block 与最近断面桩号间的距离
                    var stationDist = slopeData.Station - block.EndStation;
                    foreach (var fSlope in slopeData.Slopes)
                    {
                        var ssi = fs.SlopeInfo[fSlope.Index];
                        ssi.BackStation = block.EndStation;
                        // 直接用（斜坡长度*桩号距离）作为面积
                        ssi.BackArea = fSlope.ProtectionLength * stationDist;
                    }
                    foreach (var fSlope in slopeData.Platforms)
                    {
                        var ssi = fs.PlatformInfo[fSlope.Index];
                        ssi.BackStation = block.EndStation;
                        // 直接用（斜坡长度*桩号距离）作为面积
                        ssi.BackArea = fSlope.ProtectionLength * stationDist;
                    }
                }

            }
            return true;
        }
        #endregion


        /// <summary> 导出数据 </summary>
        public bool ExportData()
        {
            var slopeDatas = _slopesToHandle.Select(r => r.XData).ToArray();

            //// 所有设置的防护形式
            var protectionTypes = ProtectionTags.GetProtectionTypes(slopeDatas);
            var categorizedProtMtd = ProtectionTags.CategorizeProtectionMethods(protectionTypes);

            _docMdf.WriteNow($"共检测到 {categorizedProtMtd.Count} 种防护方式");
            if (categorizedProtMtd.Count == 0) return false;

            var sheet_Infos = new List<WorkSheetData>();

            // ---------------------------------------------------------------------------------------------------------------
            //// 1. 先将所有的数据进行一次性导出
            //var header = SlopeData.InfoHeader.Split('\t');
            //var allData = SlopeData.GetAllInfo(slopeDatas);
            //allData = allData.InsertVector<object, string, object>(true, new[] { header, }, new[] { -1f });

            //sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SourceData, "CAD原始数据", allData));

            // ---------------------------------------------------------------------------------------------------------------
            // 道路左侧边坡防护
            // 每一种防护形式所对应的要写入到Excel表中的信息
            var slopeOnleft = _slopesToHandle.Where(r => r.XData.OnLeft).ToArray();
            foreach (var cpm in categorizedProtMtd)
            {
                var leftRange = new List<DataRow>();
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
                    var slopesArr = DataRow.ConvertToArray(leftRange);
                    slopesArr = slopesArr.InsertVector<object, string, object>(true, new[] { DataRow.GetTableHeader() }, new[] { -1f });
                    var sheetName = $"{cpm.Key}_{"左"}";
                    sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SlopeProtection, sheetName, slopesArr));
                }

            }

            // 道路右侧边坡防护
            var slopeOnRight = _slopesToHandle.Where(r => !r.XData.OnLeft).ToArray();
            foreach (var cpm in categorizedProtMtd)
            {
                var rightRange = new List<DataRow>();
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
                    var slopesArr = DataRow.ConvertToArray(rightRange);
                    slopesArr = slopesArr.InsertVector<object, string, object>(true, new[] { DataRow.GetTableHeader() }, new[] { -1f });
                    var sheetName = $"{cpm.Key}_{"右"}";
                    sheet_Infos.Add(new WorkSheetData(WorkSheetDataType.SlopeProtection, sheetName, slopesArr));
                }
            }
            // ---------------------------------------------------------------------------------------------------------------

            // ---------------------------------------------------------------------------------------------------------------

            // 数据导出
            ExportWorkSheetDatas(sheet_Infos);

            return true;
        }

        #region --- 构造一个工作表的数据

        /// <summary> 构造Excel工作表中的表格数据：边坡防护工程量表 </summary>
        /// <param name="selectedSlopes">路基某一侧的边坡对象</param>
        /// <param name="protectionName">要提取的防护方式，挂网喷锚_6 与 挂网喷锚_8 应分别通过此函数进行计算</param>
        /// <param name="onLeft"></param>
        /// <returns></returns>
        private void GetMatchedSlopeRanges(SlopeLine[] selectedSlopes, string protectionName, bool onLeft,
            ref List<DataRow> existingPmis)
        {
            // 一侧边坡中与指定的防护方式精确匹配的子边坡对象

            DataRow lastProtMtdInfo = existingPmis.Count > 0 ? existingPmis[existingPmis.Count - 1] : null;
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
                    var nextPmi = new DataRow(backEdgeStation, frontEdgeStation, area, protectionName, rg,
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
                if (allPlatform)
                {
                    rg = ProtectionRange.AllSection;
                }
                else if (matchedPlatforms.Count > 0)
                {
                    rg = ProtectionRange.AllSlopes | ProtectionRange.PartialPlatforms;
                }
                else
                {
                    rg = ProtectionRange.AllSlopes;
                }
            }
            else if (allPlatform)
            {
                if (allSlopes)
                {
                    rg = ProtectionRange.AllSection;
                }
                else if (matchedSlopes.Count > 0)
                {
                    rg = ProtectionRange.AllPlatforms | ProtectionRange.PartialSlopes;
                }
                else
                {
                    rg = ProtectionRange.AllPlatforms;
                }
            }
            // 说明既没有全边坡，也没有全平台
            else if (matchedSlopes.Count > 0 && matchedPlatforms.Count == 0)
            {
                rg = ProtectionRange.PartialSlopes;
            }
            else if (matchedPlatforms.Count > 0 && matchedSlopes.Count == 0)
            {
                rg = ProtectionRange.PartialPlatforms;
            }
            else if (matchedPlatforms.Count > 0 && matchedSlopes.Count > 0)
            {
                rg = ProtectionRange.PartialPlatforms | ProtectionRange.PartialSlopes;
            }
            else
            {
                rg = ProtectionRange.None;
            }
            return rg;
        }

        /// <summary> 对最终 Excel 表中的所有行进行排序 </summary>
        /// <returns></returns>
        private int SortRows(DataRow range1, DataRow range2)
        {
            // 第 1 优先级：桩号小的在前面
            if (range1.StartStation < range2.StartStation)
            {
                return -1;
            }
            else if (range1.StartStation > range2.StartStation)
            {
                return 1;
            }

            // 第 2 优先级：桩号相同时，按防护方式的字符进行排序
            else
            {
                return String.Compare(range1.ProtectionName, range2.ProtectionName, StringComparison.Ordinal);
            }
            return -1;
        }

        #endregion
    }
}