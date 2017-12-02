using System;
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

                AverageWidth = (conn.AverageWidth*dist2 + AverageWidth*dist1)/(dist1 + dist2);
                AverageHeight = (conn.AverageHeight*dist2 + AverageHeight*dist1)/(dist1 + dist2);
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
            if (countAll == 0)
            {
                _docMdf.WriteNow($"低填浅挖断面数量：{countAll}");
                return;
            }

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(cutFillSections, Options_Collections.RangeBlocks);

            // 将位于桥梁隧道区间之内的断面移除
            cutFillSections = cutFillSections.Where(r => !r.IsNull).ToList();

            // 对于区间进行合并
            cutFillSections = MergeLinkedSections(cutFillSections);


            countAll = cutFillSections.Count;
            _docMdf.WriteNow($"低填浅挖断面数量：{countAll}");
            if (countAll == 0) return;

            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new string[] {"起始桩号", "结束桩号", "桩号区间", "长度", "处理措施", "平均处理宽度", "平均处理高度"};
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
                    ProtectionUtils.GetStationString(thsc.BackValue.EdgeStation, thsc.FrontValue.EdgeStation,
                        maxDigits: 0),
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
            height = 0.0;
            width = 0.0;
            // 1. 基本判断标准
            var depth = center.CenterElevation_Road - center.CenterElevation_Ground;

            double leftWidth1 = 0.0;
            double rightWidth1 = 0.0;

            // 道路中心为填方，而且左右至少有一侧为填方边坡
            if (depth >= 0 && depth <= _criterion.低填最大高度 &&
                ((center.LeftSlopeFill == null || center.LeftSlopeFill.Value) ||
                 (center.RightSlopeFill == null || center.RightSlopeFill.Value)))
            {
                // 道路中心线 与 自然地面线的交点
                var centerGroundPt = new Point3d(center.CenterX, center.GetYFromElev(center.CenterElevation_Ground), 0);

                // 左侧低填处理的宽度
                var leftRoadWidth = center.CenterX - center.LeftRoadEdge.X;
                if (center.LeftSlopeFill == null || !center.LeftSlopeFill.Value)
                {
                    // 无边坡线或者是挖方
                    leftWidth1 = leftRoadWidth;
                }
                else
                {
                    var leftSlope = center.LeftSlopeHandle.GetDBObject<Polyline>(db);
                    if (leftSlope == null)
                    {
                        leftWidth1 = leftRoadWidth;
                    }
                    else
                    {
                        // 先计算填方边坡线与
                        var leftInters = new CurveCurveIntersector2d(leftSlope.Get2dLinearCurve(),
                            new Ray2d(centerGroundPt.ToXYPlane(), new Vector2d(-1, 0)));
                        if (leftInters.NumberOfIntersectionPoints == 0)
                        {
                            leftWidth1 = leftRoadWidth;
                        }
                        else
                        {
                            leftWidth1 = Math.Abs(leftInters.GetIntersectionPoint(0).X - center.CenterX);
                        }
                    }
                }

                // 右侧低填处理的宽度
                var rightRoadWidth = center.RightRoadEdge.X - center.CenterX;
                if (center.RightSlopeFill == null || !center.RightSlopeFill.Value)
                {
                    // 无边坡线或者是挖方
                    rightWidth1 = rightRoadWidth;
                }
                else
                {
                    var rightSlope = center.RightSlopeHandle.GetDBObject<Polyline>(db);
                    if (rightSlope == null)
                    {
                        rightWidth1 = rightRoadWidth;
                    }
                    else
                    {
                        // 先计算填方边坡线与
                        var rightInters = new CurveCurveIntersector2d(rightSlope.Get2dLinearCurve(),
                            new Ray2d(centerGroundPt.ToXYPlane(), new Vector2d(1, 0)));
                        if (rightInters.NumberOfIntersectionPoints == 0)
                        {
                            rightWidth1 = rightRoadWidth;
                        }
                        else
                        {
                            rightWidth1 = Math.Abs(rightInters.GetIntersectionPoint(0).X - center.CenterX);
                        }
                    }
                }

                width = leftWidth1 + rightWidth1;
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

            // 1. 基本判断标准：零填及挖方小于0.5m，路槽底应进行翻挖压实，翻挖深度以保证路槽下处理深度不小于0.8m
            if (cutDepth >= 0 && cutDepth <= _criterion.浅挖最大深度)
            {
                // 浅挖路基的翻挖压实处理的最底部距离路面顶的深度，一般为1.5m左右
                var tb = (center.GetEleFromY(center.CenterY) - center.CenterElevation_Cushion) + _criterion.浅挖处理高度;
                ;

                // 计算左右半边道路的挖填情况
                // 当边坡为填方时（且道路中线处为挖方），此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
                // 如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖
                var leftGround = center.LeftGroundSurfaceHandle.GetDBObject<Polyline>(db);
                var rightGround = center.RightGroundSurfaceHandle.GetDBObject<Polyline>(db);
                double leftCenterCutWidth = HalfRoadShallowCut(center, true, center.LeftSlopeFill, tb, leftGround);
                double rightCenterCutWidth = HalfRoadShallowCut(center, false, center.RightSlopeFill, tb, rightGround);
                //
                width = leftCenterCutWidth + rightCenterCutWidth;
                return true;
            }
            return false;
        }

        /// <summary> 判断半边道路的挖填情况 </summary>
        /// <param name="center"></param>
        /// <param name="leftSide">左侧还是右侧</param>
        /// <param name="slopeFill">此侧边坡的填挖情况</param>
        /// <param name="treatmentDepth">浅挖路基的翻挖压实处理的最底部距离路面顶的深度，一般为1.5m左右</param>
        /// <param name="ground">自然地面</param>
        /// <returns>此侧路基中，需要进行翻挖压实处理的路基宽度。
        ///     当边坡为填方时（且道路中线处为挖方），此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
        ///     如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖</returns>
        private static double HalfRoadShallowCut(SectionInfo center, bool leftSide, bool? slopeFill,
            double treatmentDepth, Polyline ground)
        {
            var centerCutWidth = 0.0;

            var halfRoadWidth = leftSide
                ? center.CenterX - center.LeftRoadEdge.X
                : center.RightRoadEdge.X - center.CenterX; // 路面+路肩
            if (slopeFill == null || !slopeFill.Value)
            {
                // 说明不是填方
                centerCutWidth = halfRoadWidth;
            }
            else
            {
                // 说明中心为挖方，而边坡为填方
                var bottomPt = new Point2d(center.CenterX, center.CenterY - treatmentDepth);
                var inters = new CurveCurveIntersector2d(ground.Get2dLinearCurve(),
                    new Ray2d(bottomPt, new Vector2d(leftSide ? -1 : 1, 0)));
                if (inters.NumberOfIntersectionPoints == 0)
                {
                    centerCutWidth = halfRoadWidth;
                }
                else
                {
                    centerCutWidth = Math.Abs(inters.GetIntersectionPoint(0).X - center.CenterX);
                    if (centerCutWidth > halfRoadWidth)
                    {
                        centerCutWidth = halfRoadWidth;
                    }
                }
            }
            return centerCutWidth;
        }

        #endregion
    }
}