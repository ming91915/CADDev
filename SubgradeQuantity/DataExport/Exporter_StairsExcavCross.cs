﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 横向挖台阶工程量 </summary>
    public class Exporter_StairsExcavCross : DataExporter
    {
        #region --- Types

        private enum FillSlopeType
        {
            Other,
            StairExcav,
            /// <summary> 陡坡路堤 </summary>
            SteepSlope,
        }

        private class StairsExcav : HalfValue
        {
            /// <summary> 需要挖台阶处理的位置 </summary>
            public SectionSide StairCutSide { get; set; }

            /// <summary> 挖台阶平均处理宽度，每一个断面的宽度表示挖台阶的左边缘到右边缘的水平距离  </summary>
            public double AverageTreatedWidth { get; set; }
            //
            /// <summary> 此区间中挖台阶处理的平均面积（通过计算每一个小三角形的面积之和求得） </summary>
            public double AverageStairArea { get; set; }


            public override void Merge(IMergeable connectedHalf)
            {
                var conn = (StairsExcav)connectedHalf;
                var dist1 = Math.Abs(ParentStation - EdgeStation);
                var dist2 = Math.Abs(conn.EdgeStation - conn.ParentStation);
                AverageTreatedWidth = (conn.AverageTreatedWidth * dist2 + AverageTreatedWidth * dist1) / (dist1 + dist2);
                AverageStairArea = (conn.AverageStairArea * dist2 + AverageStairArea * dist1) / (dist1 + dist2);
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                var front = next as StairsExcav;
                // 处理位置不同的区间要分开
                if (StairCutSide != front.StairCutSide) return false;
                return true;
            }

            public override void CutByBlock(double blockStation)
            {
                // 啥也不用做
            }
        }

        #endregion

        #region --- Fields

        private static readonly Criterion_StairExcav _criterion = Criterion_StairExcav.UniqueInstance;

        private readonly List<SubgradeSection> _handledSections;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<StairsExcav>> _sortedRanges;

        #endregion

        /// <summary> 横向挖台阶工程量 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="allSections"></param>
        /// <param name="handledSections"></param>
        public Exporter_StairsExcavCross(DocumentModifier docMdf, IList<SubgradeSection> allSections,
            List<SubgradeSection> handledSections) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            handledSections.Sort(SQUtils.CompareStation);
            _handledSections = handledSections;
            //
            _sortedRanges = InitializeGeometricRange<StairsExcav>(AllStations);
        }

        /// <summary> 除陡坡路堤外需要挖台阶处理的范围 </summary>
        public void ExportStairCut()
        {
            var stairsCut = new List<CrossSectionRange<StairsExcav>>();

            // 断面的判断与计算
            double treatedWidth;
            double stairArea;
            SectionSide treatedSide;

            foreach (var sec in _handledSections)
            {
                var fillSlopeType = GetFillSlopeType(sec, out treatedSide, out treatedWidth, out stairArea);
                if (fillSlopeType == FillSlopeType.StairExcav)
                {
                    var hfdc = _sortedRanges[sec.XData.Station];
                    hfdc.BackValue.AverageTreatedWidth = treatedWidth;
                    hfdc.BackValue.StairCutSide = treatedSide;
                    hfdc.BackValue.AverageStairArea = stairArea;
                    //
                    hfdc.FrontValue.AverageTreatedWidth = treatedWidth;
                    hfdc.FrontValue.StairCutSide = treatedSide;
                    hfdc.FrontValue.AverageStairArea = stairArea;
                    //
                    stairsCut.Add(hfdc);
                }
            }
            var countAll = stairsCut.Count;
            if (countAll == 0)
            {
                _docMdf.WriteNow($"需要挖台阶的横断面数量：{countAll}");
                return;
            }

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(stairsCut, Options_Collections.RangeBlocks);


            // 将位于桥梁隧道区间之内的断面移除
            stairsCut = stairsCut.Where(r => !r.IsNull).ToList();

            // 对于区间进行合并
            // steepSlopes = MergeLinkedSections(steepSlopes);
            stairsCut = MergeLinkedSections<StairsExcav>(stairsCut);

            countAll = stairsCut.Count;
            _docMdf.WriteNow($"需要挖台阶的横断面数量：{countAll}");
            if (countAll == 0) return;


            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new string[] { "起始桩号", "结束桩号", "桩号区间", "段落长度", "左", "右", "挖台阶平均处理宽度", "挖台阶土方量" };
            rows.Add(header);

            // 输出数据

            for (int i = 0; i < stairsCut.Count; i++)
            {
                var rg = stairsCut[i];
                rg.UnionBackFront();
                //
                var rangeLength = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                rows.Add(new object[]
                {
                    rg.BackValue.EdgeStation,
                    rg.FrontValue.EdgeStation,
                    SQUtils.GetStationString(rg.BackValue.EdgeStation, rg.FrontValue.EdgeStation, 0),
                   rangeLength,

                    (int) (rg.BackValue.StairCutSide & SectionSide.左) > 0 ? SQConstants.CheckMark : null,
                    (int) (rg.BackValue.StairCutSide & SectionSide.右) > 0 ? SQConstants.CheckMark : null,
                    rg.BackValue.AverageTreatedWidth,
                    rg.BackValue.AverageStairArea * rangeLength,
                });
                // 
            }

            var sheetArr = ArrayConstructor.FromList2D(listOfRows: rows);
            // 输出到表格
            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.StairsExcavCross, "挖台阶_横断面", sheetArr)
            };
            ExportWorkSheetDatas(sheet_Infos);
        }

        #region --- 判断填方边坡类型

        /// <summary> 填方边坡类型 </summary>
        /// <param name="sec"> 道路中心填方高度</param>
        /// <param name="treatedWidth"> 陡坡路堤处理宽度 </param>
        /// <param name="treatedSide"> 要进行挖台阶处理的是断面的哪一侧 </param>
        /// <param name="stairArea">某一侧边坡所挖台阶面积</param>
        /// <returns></returns>
        private FillSlopeType GetFillSlopeType(SubgradeSection sec, out SectionSide treatedSide, out double treatedWidth, out double stairArea)
        {
            var sectionfillSlopeType = FillSlopeType.Other;
            treatedWidth = 0;
            stairArea = 0.0;
            double sideTreatedWidth = 0;
            double sideStairArea = 0.0;
            treatedSide = SectionSide.无;
            var secData = sec.XData;
            //
            var sideSlope = sec.GetSlopeLine(true); // 断面左侧边坡
            var sideGround = secData.LeftGroundSurfaceHandle.GetDBObject<Polyline>(_docMdf.acDataBase);
            var fillSlopeType = GetFillSlopeType(secData, true, sideSlope, sideGround, out sideTreatedWidth, out sideStairArea);
            if (fillSlopeType == FillSlopeType.StairExcav)
            {
                sectionfillSlopeType = FillSlopeType.StairExcav;
                treatedSide = treatedSide | SectionSide.左;
                treatedWidth += sideTreatedWidth;
                stairArea += sideStairArea;
            }

            sideSlope = sec.GetSlopeLine(false); // 断面右侧边坡
            sideGround = secData.RightGroundSurfaceHandle.GetDBObject<Polyline>(_docMdf.acDataBase);
            fillSlopeType = GetFillSlopeType(secData, false, sideSlope, sideGround, out sideTreatedWidth, out sideStairArea);
            if (fillSlopeType == FillSlopeType.StairExcav)
            {
                sectionfillSlopeType = FillSlopeType.StairExcav;
                treatedSide = treatedSide | SectionSide.右;
                treatedWidth += sideTreatedWidth;
                stairArea += sideStairArea;
            }

            return sectionfillSlopeType;
        }

        /// <summary>
        /// 判断某一侧边坡的填方边坡类型
        /// </summary>
        /// <param name="slp">某一侧边坡，其值可能为null，表示此侧没有边坡线 </param>
        /// <param name="ground">边坡所对应的自然地面线</param>
        /// <param name="treatedWidth"></param>
        /// <param name="stairArea">某一侧边坡所挖台阶面积</param>
        /// <returns></returns>
        private FillSlopeType GetFillSlopeType(SectionInfo sec, bool left, SlopeLine slp, Polyline ground,
            out double treatedWidth, out double stairArea)
        {
            treatedWidth = 0;
            stairArea = 0.0;
            double edgeXleft;
            double edgeXright;
            var cGround = ground.Get2dLinearCurve();
            var succ = sec.GetFillSlopeXRange(left, slp, cGround, _docMdf.acDataBase, out edgeXleft, out edgeXright);
            if (!succ) return FillSlopeType.Other;

            // 必须是填方边坡
            if ((left && (sec.LeftSlopeFill == null || !sec.LeftSlopeFill.Value))
                || (!left && (sec.RightSlopeFill == null || !sec.RightSlopeFill.Value)))
            {
                return FillSlopeType.Other;
            }

            // 有路肩墙
            if ((left && sec.LeftRetainingWallType == RetainingWallType.路肩墙) || (!left && sec.RightRetainingWallType == RetainingWallType.路肩墙))
            {
                return FillSlopeType.Other;
            }

            //
            var segCounts = (int)Math.Ceiling((edgeXright - edgeXleft) / _criterion.最小迭代宽度); // 其值最小为1
            var xInterval = (edgeXright - edgeXleft) / segCounts;
            var xYs = new List<double[]>();
            for (int i = 0; i <= segCounts; i++)
            {
                var x = edgeXleft + i * xInterval;
                double yGround;
                var inters = new CurveCurveIntersector2d(cGround, new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                // 没有交点的情况一般不会出现，因为自然地面线的范围很广
                yGround = inters.NumberOfIntersectionPoints == 0 ? 0 : inters.GetIntersectionPoint(0).Y;
                //
                xYs.Add(new double[] { x, yGround });
            }
            // 开始求斜率（集合中至少有两个元素）。每个元素为三分量向量，分别为 X 坐标、X对应的自然地面的Y坐标，X对应的边坡的Y坐标
            double maxRatio = 0;
            bool hasStairExcav = false;
            var lastX = xYs[0];
            for (int i = 1; i < xYs.Count; i++)
            {
                var thisX = xYs[i];
                var segRatio = Math.Abs((thisX[1] - lastX[1]) / (thisX[0] - lastX[0]));
                if ((segRatio > 1 / _criterion.填方坡比下限) && (segRatio > 1 / _criterion.填方坡比下限))
                {
                    hasStairExcav = true;
                }
                maxRatio = Math.Max(maxRatio, segRatio);
                //
                lastX = thisX;
            }
            if (!hasStairExcav)
            {
                return FillSlopeType.Other;
            }
            else
            {

                //  判断是否为陡坡路堤，如果是，则不计入挖台阶工程量表中（因为在陡坡路堤工程量表中已经计入）
                if (maxRatio >= (1 / _criterion.陡坡坡比))
                {
                    treatedWidth = 0;
                    return FillSlopeType.SteepSlope;
                }
                else
                {
                    // 
                    treatedWidth = edgeXright - edgeXleft;
                    stairArea = Exporter_SteepSlope.CalculateStairArea(xYs, edgeXleft, edgeXright);
                    return FillSlopeType.StairExcav;
                }
            }


        }

        #endregion
    }
}