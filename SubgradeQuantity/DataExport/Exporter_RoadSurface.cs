using System;
using System.Collections.Generic;
using System.Linq;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 路面工程数量表 </summary>
    public class Exporter_RoadSurface : DataExporter
    {
        #region --- Types

        /// <summary> 硬路肩工程量 </summary>
        private class RoadSurface : HalfValue
        {
            /// <summary> true 表示“填方或土质路堑路段”，false 表示“石质路堑路段” </summary>
            public bool FillOrSoilCut { get; set; }

            //
            public override void Merge(IMergeable connectedHalf)
            {
                // 啥也不用做
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                var front = next as RoadSurface;
                if (FillOrSoilCut != front.FillOrSoilCut) return false;

                return true;
            }

            public override void CutByBlock(double blockStation)
            {
                // 啥也不用做
            }
        }

        /// <summary> 硬路肩类型 </summary>
        private enum HardShoulderType
        {
            无,
            挡墙,
            护栏,
            无护栏,
        }

        /// <summary> 硬路肩工程量 </summary>
        private class HardShoulder : HalfValue
        {
            /// <summary> 此区间段的路肩数量（整个路段的所有断面的左右两侧路肩数量之和）  </summary>
            public int Count { get; set; }
            /// <summary> 此区间段的路肩的平均面积（左侧） </summary>
            public double LeftAverageArea { get; set; }
            /// <summary> 此区间段的路肩的平均面积（右侧） </summary>
            public double RightAverageArea { get; set; }
            public HardShoulderType LeftType { get; set; }
            public HardShoulderType RightType { get; set; }

            //
            public override void CutByBlock(double blockStation)
            {
                // 啥也不用做
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                // 不进行合并
                return false;
                var front = next as HardShoulder;
                if (LeftType != front.LeftType) return false;
            }

            public override void Merge(IMergeable connectedHalf)
            {
                var front = connectedHalf as HardShoulder;
                // Sumup += front.Sumup;
            }
        }

        #endregion

        #region --- Fields

        private static readonly Criterion_RoadSurface _criterion = Criterion_RoadSurface.UniqueInstance;

        private readonly List<SubgradeSection> _handledSections;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<RoadSurface>> _sortedRanges_RoadSurface;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<HardShoulder>> _sortedRanges_HardShoulder;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="allSections"></param>
        /// <param name="handledSections"></param>
        public Exporter_RoadSurface(DocumentModifier docMdf, IList<SubgradeSection> allSections,
            List<SubgradeSection> handledSections) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            handledSections.Sort(SQUtils.CompareStation);
            _handledSections = handledSections;
            //
            _sortedRanges_RoadSurface = InitializeGeometricRange<RoadSurface>(AllStations);
            _sortedRanges_HardShoulder = InitializeGeometricRange<HardShoulder>(AllStations);
        }

        /// <summary> 路面工程数量表 </summary>
        public void ExportRoadSurface()
        {
            // -------------------- 路面工程 -------------------------------------
            var fillSurfaces = new List<CrossSectionRange<RoadSurface>>();

            foreach (var sec in _handledSections)
            {
                var isFillOrSoilCut = IsFillOrSoilCut(sec);
                if (true)
                {
                    var hfdc = _sortedRanges_RoadSurface[sec.XData.Station];
                    hfdc.BackValue.FillOrSoilCut = isFillOrSoilCut;
                    //
                    hfdc.FrontValue.FillOrSoilCut = isFillOrSoilCut;
                    //
                    fillSurfaces.Add(hfdc);
                }
            }
            var countAll = fillSurfaces.Count;
            if (countAll == 0)
            {
                _docMdf.WriteNow($"路面工程数量：{countAll}");
                return;
            }

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(fillSurfaces, Options_Collections.RangeBlocks);

            // 将位于桥梁隧道区间之内的断面移除
            fillSurfaces = fillSurfaces.Where(r => !r.IsNull).ToList();

            // 对于区间进行合并
            // steepSlopes = MergeLinkedSections(steepSlopes);
            fillSurfaces = MergeLinkedSections<RoadSurface>(fillSurfaces);

            countAll = fillSurfaces.Count;
            _docMdf.WriteNow($"路面工程：{countAll}");

            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new object[]
            {"起始桩号", "结束桩号", "区间长度", "结构类型"};
            rows.Add(header);

            for (int i = 0; i < fillSurfaces.Count; i++)
            {
                var rg = fillSurfaces[i];
                rg.UnionBackFront();
                //
                var rangeLength = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                rows.Add(new object[]
                {
                    rg.BackValue.EdgeStation,
                    rg.FrontValue.EdgeStation,
                    rangeLength,
                    // ProtectionUtils.GetStationString(rg.BackValue.EdgeStation, rg.FrontValue.EdgeStation, maxDigits: 0),
                    // 路面类型
                    rg.BackValue.FillOrSoilCut ? "填方或土质路堑路段" : "石质路堑路段",
                });
            }

            var sheetArr = ArrayConstructor.FromList2D(listOfRows: rows);
            // sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1.5f, });

            // -------------------- 硬路肩 -------------------------------------
            var hardShoulders = new List<CrossSectionRange<HardShoulder>>();

            int hardShoulerCount = 0;
            double leftHardShoulerArea = 0;
            double rightHardShoulerArea = 0;
            HardShoulderType leftType;
            HardShoulderType rightType;

            foreach (var sec in _handledSections)
            {
                var hasHardShouler = HasHardShoulder(sec, out hardShoulerCount, out leftType, out rightType, out leftHardShoulerArea, out rightHardShoulerArea);
                if (hasHardShouler)
                {
                    var hfdc = _sortedRanges_HardShoulder[sec.XData.Station];
                    hfdc.BackValue.Count = hardShoulerCount;
                    hfdc.BackValue.LeftAverageArea = leftHardShoulerArea;
                    hfdc.BackValue.RightAverageArea = rightHardShoulerArea;
                    hfdc.BackValue.LeftType = leftType;
                    hfdc.BackValue.RightType = rightType;
                    //
                    hfdc.FrontValue.Count = hardShoulerCount;
                    hfdc.FrontValue.LeftAverageArea = leftHardShoulerArea;
                    hfdc.FrontValue.RightAverageArea = rightHardShoulerArea;
                    hfdc.FrontValue.LeftType = leftType;
                    hfdc.FrontValue.RightType = rightType;
                    //
                    hardShoulders.Add(hfdc);
                }
            }
            var countAll2 = hardShoulders.Count;
            if (countAll2 == 0)
            {
                _docMdf.WriteNow($"有硬路肩的断面数量：{countAll2}");
                return;
            }

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(hardShoulders, Options_Collections.RangeBlocks);

            // 将位于桥梁隧道区间之内的断面移除
            hardShoulders = hardShoulders.Where(r => !r.IsNull).ToList();

            // 对于区间进行合并
            // steepSlopes = MergeLinkedSections(steepSlopes);
            hardShoulders = MergeLinkedSections<HardShoulder>(hardShoulders);

            //
            countAll2 = hardShoulders.Count;
            _docMdf.WriteNow($"路面工程：{countAll2}");
            if (countAll + countAll2 == 0) return;

            // 将结果整理为二维数组，用来进行表格输出
            rows = new List<object[]>();
            header = new object[]
           {"起始桩号", "结束桩号", "区间长度", "路肩数量","左侧路肩体积","右侧路肩体积","左路肩类型","右路肩类型"};
            rows.Add(header);
            var hardShoulderType = typeof(HardShoulderType);
            for (int i = 0; i < hardShoulders.Count; i++)
            {
                var rg = hardShoulders[i];
                rg.UnionBackFront();
                //
                var rangeLength = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                rows.Add(new object[]
                {
                    rg.BackValue.EdgeStation,
                    rg.FrontValue.EdgeStation,
                    rangeLength,
                    // ProtectionUtils.GetStationString(rg.BackValue.EdgeStation, rg.FrontValue.EdgeStation, maxDigits: 0),
                    // 路面类型
                    rg.BackValue.Count,
                    rg.BackValue.LeftAverageArea*rangeLength,
                    rg.BackValue.RightAverageArea*rangeLength,
                   Enum.GetName(hardShoulderType,rg.BackValue.LeftType) ,
                   Enum.GetName(hardShoulderType,rg.BackValue.RightType) ,
                });
            }

            var sheetArr_hardshoulder = ArrayConstructor.FromList2D(listOfRows: rows);
            // sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1.5f, });


            // 输出到表格
            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.SteepSlope, "硬路肩", sheetArr_hardshoulder),
                new WorkSheetData(WorkSheetDataType.SteepSlope, "路面工程", sheetArr),
            };
            ExportWorkSheetDatas(sheet_Infos);
        }

        #region --- 判断陡坡路堤

        /// <summary> 填方或土质路堑路段 </summary>
        /// <param name="sec"> 道路中心填方高度</param>
        /// <returns></returns>
        private bool IsFillOrSoilCut(SubgradeSection sec)
        {
            var secData = sec.XData;
            if ((secData.LeftSlopeFill != null && !secData.LeftSlopeFill.Value) &&
                (secData.RightSlopeFill != null && !secData.RightSlopeFill.Value))
            {
                var leftSlope = sec.GetSlopeLine(true); // 断面左侧边坡
                var rightSlope = sec.GetSlopeLine(true); // 断面左侧边坡
                if (leftSlope != null && leftSlope.XData.SoilOrRock == SubgradeType.岩质
                    && rightSlope != null && rightSlope.XData.SoilOrRock == SubgradeType.岩质)
                {
                    return false; // 石质路堑路段
                }
            }
            return true; // 填方或土质路堑路段
        }

        /// <summary> 填方或土质路堑路段 </summary>
        /// <param name="sec"> 道路中心填方高度</param>
        /// <param name="hardShouderCount"> 道路中心填方高度</param>
        /// <returns></returns>
        private bool HasHardShoulder(SubgradeSection sec, out int hardShouderCount,
            out HardShoulderType leftType, out HardShoulderType rightType, out double leftHardShoulerArea, out double rightHardShoulerArea)
        {
            hardShouderCount = 0;
            leftHardShoulerArea = 0;
            rightHardShoulerArea = 0;
            leftType = HardShoulderType.无;
            rightType = HardShoulderType.无;

            var secData = sec.XData;

            // 对左侧进行判断与计量
            if (secData.LeftSlopeFill != null && secData.LeftSlopeFill.Value)
            {
                // 表示是填方路段，接下来判断路肩类型
                if (secData.LeftRetainingWallType == RetainingWallType.路肩墙)
                {
                    leftType = HardShoulderType.挡墙;
                    hardShouderCount += 1;
                    leftHardShoulerArea = _criterion.路肩面积_挡墙;
                }
                else
                {
                    // 通过填方边坡高度来确定是否需要设置护栏
                    double fillHeight;
                    var leftSlp = sec.GetSlopeLine(true);
                    fillHeight = Math.Abs(leftSlp.Pline.StartPoint.Y - leftSlp.Pline.EndPoint.Y);

                    if (fillHeight >= _criterion.设护栏段的填方高度)
                    {
                        leftType = HardShoulderType.护栏;
                        hardShouderCount += 1;
                        leftHardShoulerArea = _criterion.路肩面积_护栏;
                    }
                    else
                    {
                        leftType = HardShoulderType.无护栏;
                        hardShouderCount += 1;
                        leftHardShoulerArea = _criterion.路肩面积_无护栏;
                    }

                }
            }

            // 对右侧进行判断与计量
            if (secData.RightSlopeFill != null && secData.RightSlopeFill.Value)
            {
                // 表示是填方路段，接下来判断路肩类型
                if (secData.RightRetainingWallType == RetainingWallType.路肩墙)
                {
                    rightType = HardShoulderType.挡墙;
                    hardShouderCount += 1;
                    rightHardShoulerArea = _criterion.路肩面积_挡墙;
                }
                else
                {
                    // 通过填方边坡高度来确定是否需要设置护栏
                    double fillHeight;
                    var rightSlp = sec.GetSlopeLine(false);
                    fillHeight = Math.Abs(rightSlp.Pline.StartPoint.Y - rightSlp.Pline.EndPoint.Y);

                    if (fillHeight >= _criterion.设护栏段的填方高度)
                    {
                        rightType = HardShoulderType.护栏;
                        hardShouderCount += 1;
                        rightHardShoulerArea = _criterion.路肩面积_护栏;
                    }
                    else
                    {
                        rightType = HardShoulderType.无护栏;
                        hardShouderCount += 1;
                        rightHardShoulerArea = _criterion.路肩面积_无护栏;
                    }
                }
            }
            //
            return hardShouderCount > 0;
        }

        #endregion
    }
}