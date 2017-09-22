using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;
using eZstd.Mathematics;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 低填浅挖工程量 </summary>
    public class Exporter_ThinFillShallowCut : DataExporter
    {
        #region --- Types

        private enum FillCutType
        {
            /// <summary> 不是低填也不是浅挖 </summary>
            Others,

            /// <summary> 低填 </summary>
            ThinFill,

            /// <summary> 浅挖 </summary>
            ShallowCut
        }

        /// <summary> 半边道路的填挖类型 </summary>
        [Flags]
        private enum HalfFillCut
        {
            /// <summary> 全挖方，而且是浅挖路堑 </summary>
            ShallowCut = 1,

            /// <summary> 全挖方，但是非浅挖 </summary>
            NoneShallowCut = 2,

            /// <summary> 中心挖方，边缘填方，而且填方宽度小于所在半边道路的三分之一。即此侧靠近道路中部大部分还是处于挖方区 </summary>
            FillLess31 = 4,

            /// <summary> 中心挖方，边缘填方，而且填方宽度大于所在半边道路的三分之一。即此侧道路中部的挖方区很少 </summary>
            FillLarger31 = 8
        }

        private class ThinFillShallowCut : HalfValue
        {
            /// <summary> 平均处理宽度 </summary>
            public double AverageWidth { get; set; }
            /// <summary> 平均处理高度 </summary>
            public double AverageHeight { get; set; }
            /// <summary> 低填还是浅挖，或者都不是 </summary>
            public FillCutType Type { get; set; }
            //

            public override void Merge(IMergeable connectedHalf)
            {
                var conn = connectedHalf as ThinFillShallowCut;
                var dist1 = Math.Abs(ParentStation - EdgeStation);
                var dist2 = Math.Abs(conn.EdgeStation - conn.ParentStation);

                AverageWidth = (conn.AverageWidth * dist2 + AverageWidth * dist1) / (dist1 + dist2);
                AverageHeight = (conn.AverageHeight * dist2 + AverageHeight * dist1) / (dist1 + dist2);
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                var nextSec = next as ThinFillShallowCut;
                return nextSec.Type == Type;
            }

            public override void CutByBlock(double blockStation)
            {
                // 对于低填浅挖，这里啥也不用做
            }

            public string GetDescription()
            {
                switch (Type)
                {
                    case FillCutType.ThinFill:
                        return "超挖回填";
                    case FillCutType.ShallowCut:
                        return "翻挖压实";
                    default:
                        return "超挖回填";
                }
            }
        }

        #endregion

        #region --- Fields

        private static readonly Criterion_ThinFillShallowCut _criterion = Criterion_ThinFillShallowCut.UniqueInstance;

        /// <summary> 整个项目中的所有横断面 </summary>
        private readonly IList<SubgradeSection> _sectionsToHandle;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<ThinFillShallowCut>> _sortedRanges;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="sectionsToHandle">要进行处理的断面</param>
        /// <param name="allSections"></param>
        public Exporter_ThinFillShallowCut(DocumentModifier docMdf, List<SubgradeSection> sectionsToHandle,
            IList<SubgradeSection> allSections) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            sectionsToHandle.Sort(ProtectionUtils.CompareStation);
            _sectionsToHandle = sectionsToHandle;
            //
            _sortedRanges = InitializeGeometricRange<ThinFillShallowCut>(AllStations);
        }

        /// <summary> 低填浅挖 </summary>
        public void ExportThinFillShallowCut()
        {
            var cutFillSections = new List<CrossSectionRange<ThinFillShallowCut>>();

            // 断面的判断与计算
            double width;
            double height;
            foreach (var sec in _sectionsToHandle)
            {
                var xdata = sec.XData;
                if (IsThinFill(_docMdf.acDataBase, sec, out width, out height))
                {
                    var thsc = _sortedRanges[xdata.Station];
                    //
                    thsc.BackValue.Type = FillCutType.ThinFill;
                    thsc.BackValue.AverageHeight = height;
                    thsc.BackValue.AverageWidth = width;
                    //
                    thsc.FrontValue.Type = FillCutType.ThinFill;
                    thsc.FrontValue.AverageHeight = height;
                    thsc.FrontValue.AverageWidth = width;
                    //
                    cutFillSections.Add(thsc);
                }
                else if (IsShallowCut(_docMdf.acDataBase, sec, out width, out height))
                {
                    var thsc = _sortedRanges[xdata.Station];
                    //
                    thsc.BackValue.Type = FillCutType.ShallowCut;
                    thsc.BackValue.AverageHeight = height;
                    thsc.BackValue.AverageWidth = width;
                    //
                    thsc.FrontValue.Type = FillCutType.ShallowCut;
                    thsc.FrontValue.AverageHeight = height;
                    thsc.FrontValue.AverageWidth = width;
                    //
                    cutFillSections.Add(thsc);
                }
            }
            var countAll = cutFillSections.Count;
            _docMdf.WriteNow($"低填浅挖断面数量：{countAll}");
            if (countAll == 0) return;

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(cutFillSections, Options_Collections.RangeBlocks);

            // 对于区间进行合并
            cutFillSections = MergeLinkedSections(cutFillSections);


            // 将结果整理为二维数组，用来进行表格输出


            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new string[] { "起始桩号", "结束桩号", "桩号区间", "长度", "处理措施", "平均处理宽度", "平均处理高度" };
            rows.Add(header);

            for (int i = 0; i < cutFillSections.Count; i++)
            {
                var thsc = cutFillSections[i];
                thsc.UnionBackFront();
                //
                rows.Add(new object[]
                {
                    thsc.BackValue.EdgeStation,
                thsc.FrontValue.EdgeStation,
                ProtectionUtils.GetStationString(thsc.BackValue.EdgeStation, thsc.FrontValue.EdgeStation, maxDigits: 0),
                thsc.FrontValue.EdgeStation - thsc.BackValue.EdgeStation,
                thsc.BackValue.GetDescription(),
                thsc.BackValue.AverageWidth,
                thsc.BackValue.AverageHeight,
                });
            }

            var sheetArr = ArrayConstructor.FromList2D(listOfRows: rows);
            // sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1.5f, });

            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.ThinFillShallowCut, "低填浅挖", sheetArr)
            };

            ExportWorkSheetDatas(sheet_Infos);
        }

        #region --- 判断低填

        /// <summary> 低填路堤 </summary>
        /// <param name="centerAxis"></param>
        /// <param name="width"> 左右路堤坡底之间的宽度</param>
        /// <param name="height"> 自然地面以下地基加固的高度</param>
        /// <returns></returns>
        public static bool IsThinFill(Database db, SubgradeSection centerAxis, out double width, out double height)
        {
            var center = centerAxis.XData;
            width = 0.0;
            height = 0.0;
            // 1. 基本判断标准
            var depth = center.CenterElevation_Road - center.CenterElevation_Ground;
            if (depth >= 0 && depth <= _criterion.低填最大高度 &&
                (center.LeftSlopeFill == null || center.LeftSlopeFill.Value || center.RightSlopeFill == null || center.RightSlopeFill.Value))
            {
                // 2. 边坡的坡底点
                var leftBottom = center.LeftSlopeExists
                    ? center.LeftSlopeHandle.GetDBObject<Polyline>(db).EndPoint
                    : center.LeftRoadSurfaceHandle.GetDBObject<Polyline>(db).EndPoint;
                var rightBottom = center.RightSlopeExists
                    ? center.RightSlopeHandle.GetDBObject<Polyline>(db).EndPoint
                    : center.RightRoadSurfaceHandle.GetDBObject<Polyline>(db).EndPoint;

                // 道路中心线与自然地面的交点
                var groundPt = new Point3d(center.CenterX, center.GetYFromElev(center.CenterElevation_Ground), 0);

                // 3. 坡底点是否位于低填区间
                var leftWithinLf = WithinThinFillRange(groundPt, 1 / _criterion.低填射线坡比,
                    1 / _criterion.低填射线坡比, leftBottom);
                var rightWithinLf = WithinThinFillRange(groundPt, 1 / _criterion.低填射线坡比,
                    1 / _criterion.低填射线坡比, rightBottom);

                var leftRoadEdge = center.LeftRoadEdge;
                var rightRoadEdge = center.RightRoadEdge;

                // 对于不同的情况进行分别处理
                if (leftWithinLf * rightWithinLf < 0)
                {
                    // 说明一侧过高，一侧过低，结果为陡坡路堤
                    return false;
                }
                if (leftWithinLf + rightWithinLf == -2)
                {
                    // 说明两侧均过低，结果为陡坡路堤
                    return false;
                }
                if (leftWithinLf + rightWithinLf == 2)
                {
                    // 说明两侧均过高，结果可认为是低填，处理宽度取路基宽度

                    width = rightBottom.X - leftBottom.X;
                }
                // 剩下的至少有一侧为0，即位于低填区间
                else if (leftWithinLf + rightWithinLf >= 0)
                {
                    // 一侧为正常低填，另一侧要么为低填，要么过高
                    // 低填则取中心到坡底宽度，过高则取中心到路基边缘宽度
                    var leftWidth = leftWithinLf == 0 ? center.CenterX - leftBottom.X : center.CenterX - leftRoadEdge.X;
                    var rightWidth = rightWithinLf == 0 ? rightBottom.X - center.CenterX : rightRoadEdge.X - center.CenterX;
                    width = leftWidth + rightWidth;
                }
                else
                {
                    // 另一侧肯定过低，考虑一半断面低填
                    if (leftWithinLf == 0)
                    {
                        // 右侧过低，则只计算左侧的宽度
                        width = center.CenterX - leftBottom.X;
                    }
                    else
                    {
                        // 左侧过低，则只计算右侧的宽度
                        width = rightBottom.X - center.CenterX;
                    }
                }
                //
                if (width > 0)
                {
                    // 低填路堤的自然地面下部地基处理的高度
                    height = _criterion.低填处理高度 - (center.CenterElevation_Road - center.CenterElevation_Ground);
                    if (height < 0)
                    {
                        height = 0;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary> 判断边坡底是否位于低填区 </summary>
        /// <param name="ground"></param>
        /// <param name="upperDir">滑动矢量的上边界，对于 1:5 的极限值，此参数的值为0.2</param>
        /// <param name="lowerDir">滑动矢量的下边界，对于 1:5 的极限值，此参数的值为0.2</param>
        /// <param name="slopeBottom">边坡坡底的点</param>
        /// <returns>1表示位于上边界之上，-1表示位于下边界之下，0 表示位于低填区间内</returns>
        public static sbyte WithinThinFillRange(Point3d ground, double upperDir, double lowerDir, Point3d slopeBottom)
        {
            var dir = (slopeBottom.Y - ground.Y) / Math.Abs(slopeBottom.X - ground.X);
            if (dir > upperDir)
            {
                return 1; // 非低填且向中心滑动
            }
            if (dir < -lowerDir)
            {
                return -1; // 非低填且向两边滑动
            }
            return 0; // 位于低填区间
        }

        #endregion

        #region --- 判断浅挖

        /// <summary> 浅挖路堑 </summary>
        /// <param name="centerAxis"></param>
        /// <param name="width">左右路堑坡顶之间的宽度</param>
        /// <param name="height"> 路堑处理的高度，指 *-**</param>
        /// <returns></returns>
        public static bool IsShallowCut(Database db, SubgradeSection centerAxis, out double width, out double height)
        {
            var center = centerAxis.XData;
            width = 0.0;
            // 浅挖路堑的路槽底部以下地基处理的高度
            height = _criterion.浅挖处理高度; // center.CenterElevation_Ground - center.CenterElevation_Road;
            var cutDepth = center.CenterElevation_Ground - center.CenterElevation_Road;

            // 1. 基本判断标准
            if (cutDepth >= 0 && cutDepth <= _criterion.浅挖最大深度)
            {
                // 计算左右半边道路的挖填情况
                // 当边坡为填方时（且道路中线处为挖方），此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
                // 如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖
                double leftCenterCutWidth;
                double rightCenterCutWidth;
                double leftRoadWidth;
                double rightRoadWidth;
                var leftFillCut = HalfRoadShallowCut(center.LeftSlopeFill,
                    center.LeftGroundSurfaceHandle.GetDBObject<Polyline>(db),
                    center.LeftRoadSurfaceHandle.GetDBObject<Polyline>(db),
                    1 / _criterion.浅挖射线坡比, out leftRoadWidth, out leftCenterCutWidth);
                var rightFillCut = HalfRoadShallowCut(center.RightSlopeFill,
                    center.RightGroundSurfaceHandle.GetDBObject<Polyline>(db),
                    center.RightRoadSurfaceHandle.GetDBObject<Polyline>(db),
                    1 / _criterion.浅挖射线坡比, out rightRoadWidth, out rightCenterCutWidth);

                // 2. 对各种情况进行分别处理

                // 对于不同的情况进行分别处理
                if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.ShallowCut))
                {
                    // 说明两侧都是浅挖
                    width = leftCenterCutWidth + rightCenterCutWidth;
                    return true;
                }
                if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.FillLess31))
                {
                    // 说明一侧浅挖，一侧外缘填方小于三分之一道路宽度
                    width = leftCenterCutWidth + rightCenterCutWidth;
                    return true;
                }
                if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.NoneShallowCut))
                {
                    // 说明一侧浅挖，一侧深挖
                    if (leftFillCut == HalfFillCut.ShallowCut)
                    {
                        width = leftCenterCutWidth;
                    }
                    else
                    {
                        width = rightCenterCutWidth;
                    }
                    return true;
                }
                if ((leftFillCut | rightFillCut) == (HalfFillCut.FillLess31 | HalfFillCut.NoneShallowCut))
                {
                    // 说明一侧深挖，一侧外缘填方小于三分之一道路宽度
                    if (leftFillCut == HalfFillCut.FillLess31)
                    {
                        width = leftCenterCutWidth;
                    }
                    else
                    {
                        width = rightCenterCutWidth;
                    }
                    return true;
                }
                if ((leftFillCut | rightFillCut) == (HalfFillCut.FillLarger31 | HalfFillCut.ShallowCut))
                {
                    // 说明一侧浅挖，另一侧外缘填方大于三分之一道路宽度
                    // 计量时取 浅挖侧路基宽度（包括硬路肩，不包括土路肩）
                    if (leftFillCut == HalfFillCut.ShallowCut)
                    {
                        width = leftCenterCutWidth;
                    }
                    else
                    {
                        width = rightCenterCutWidth;
                    }
                    return true;
                }
                //
                if (width > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary> 判断半边道路的挖填情况 </summary>
        /// <param name="road">路面</param>
        /// <param name="ground">自然地面</param>
        /// <param name="slopeFill">此侧边坡的填挖情况</param>
        /// <param name="halfRoadWidth"> 一侧道路的宽度，考虑路肩的宽度 </param>
        /// <param name="centerCutWidth">
        ///     当边坡为填方时（且道路中线处为挖方），此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
        ///     如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖
        /// </param>
        /// <param name="upperDir">浅挖路堑判断向量的上边界，对于 1:5 的极限值，此参数的值为0.2</param>
        /// <returns></returns>
        private static HalfFillCut HalfRoadShallowCut(bool? slopeFill, Polyline ground, Polyline road, double upperDir,
            out double halfRoadWidth, out double centerCutWidth)
        {
            halfRoadWidth = road.Length; // 路面+路肩
            var pt = road.GetLineSegment2dAt(road.NumberOfVertices - 2).StartPoint; // 路面线最外侧向中间走，第二个顶点，即最后一段的起点，即路面边缘点
            var halfCushionWidth = Math.Abs(road.StartPoint.X - pt.X); // 路基某一侧路面的宽度（不包括路肩）
            centerCutWidth = halfCushionWidth;
            var ground2d = ground.Get2dLinearCurve();
            var groundTop = ground.StartPoint; // 道路中线 与 自然地面 的交点
            if (slopeFill == null || !slopeFill.Value)
            {
                // 此边坡为挖方边坡
                var l = new Ray2d(pt, new Vector2d(0, 1));
                var inters = new CurveCurveIntersector2d(l, ground2d); // 路面边缘点向上的射线与地面线的交点
                if (inters.NumberOfIntersectionPoints > 0) // 正常情况下，肯定会有交点
                {
                    var ints = inters.GetIntersectionPoint(0);
                    var dir = (ints.Y - groundTop.Y) / (ints.X - groundTop.X);
                    if (Math.Abs(dir) <= Math.Abs(upperDir))
                    {
                        return HalfFillCut.ShallowCut;
                    }
                }
                return HalfFillCut.NoneShallowCut;
            }
            else
            {
                // 填方路堤，在保证道路中心处为挖方的情况下，此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
                // 如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖
                var road2d = road.Get2dLinearCurve();
                var inters = new CurveCurveIntersector2d(road2d, ground2d);
                if (inters.NumberOfIntersectionPoints > 0) // 正常情况下，肯定会有交点
                {
                    var ints = inters.GetIntersectionPoint(0);
                    var ratio = Math.Abs(ints.X - groundTop.X) / halfRoadWidth; // 靠近道路中线侧的挖方宽度 相对于 整个（路面+路肩）宽度的比例
                    if (ratio > 0.666666666666666666666666666666666667)
                    {
                        // 说明道路中间以挖方为主
                        centerCutWidth = halfCushionWidth;
                        return HalfFillCut.FillLess31;
                    }
                    // 说明道路中间挖方很少，大部分是外缘的填方
                    centerCutWidth = 0; // 不计入浅挖的量
                    return HalfFillCut.FillLarger31;
                }
                // 正常情况下，不会没有交点，所以这种情况正常条件下不会出现。
                centerCutWidth = halfRoadWidth;
                return HalfFillCut.ShallowCut;
            }
        }

        #endregion

    }
}