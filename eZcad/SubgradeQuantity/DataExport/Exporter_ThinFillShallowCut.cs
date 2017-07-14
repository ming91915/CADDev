using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;
using eZstd.Mathematics;

namespace eZcad.SubgradeQuantity.DataExport
{
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
            ShallowCut,
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
            FillLarger31 = 8,
        }

        private class ThinFillShallowCut : IInterpolatableSection
        {
            //
            public double Mileage { get; }
            public FillCutType Type { get; }
            public double Width { get; }
            public double Height { get; }

            //
            public ThinFillShallowCut(double mileage, FillCutType type, double width, double height)
            {
                Mileage = mileage;
                Type = type;
                Width = width;
                Height = height;
            }

            public string GetDescription()
            {
                switch (Type)
                {
                    case FillCutType.ThinFill: return "超挖回填";
                    case FillCutType.ShallowCut: return "翻挖压实";
                    default: return "超挖回填";
                }
            }

            /// <summary> 在两个断面之间进行插值，以生成一个新的用来计算的断面 </summary>
            /// <param name="section2"></param>
            /// <returns></returns>
            public IInterpolatableSection InterpolateWith(IInterpolatableSection section2)
            {
                var s2 = section2 as ThinFillShallowCut;
                var m = (Mileage + s2.Mileage) / 2;
                if (Type != FillCutType.Others)
                {
                    return new ThinFillShallowCut(m, Type, Width, Height);
                }
                else
                {
                    return new ThinFillShallowCut(m, s2.Type, s2.Width, s2.Height);
                }
            }
        }

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="sectionsToHandle"></param>
        /// <param name="allSections"></param>
        public Exporter_ThinFillShallowCut(DocumentModifier docMdf, IList<SubgradeSection> sectionsToHandle,
            IList<SubgradeSection> allSections) : base(docMdf, sectionsToHandle, allSections)
        {
        }

        /// <summary> 低填浅挖 </summary>
        public void ExportThinFill()
        {
            var cutFillSections = new SortedDictionary<double, ThinFillShallowCut>();

            // 断面的判断与计算
            double width;
            double height;
            foreach (var cl in _sectionsToHandle)
            {
                var xdata = cl.XData;
                if (IsThinFill2(_docMdf.acDataBase, cl, out width, out height))
                {
                    cutFillSections.Add(xdata.Mileage,
                        new ThinFillShallowCut(xdata.Mileage, FillCutType.ThinFill, width, height));

                    // MessageBox.Show($"是低填，宽度{width}，高度{height}");
                    _docMdf.WriteNow(xdata.Mileage, FillCutType.ThinFill, width, height);
                }
                else if (IsShallowCut(_docMdf.acDataBase, cl, out width, out height))
                {
                    cutFillSections.Add(xdata.Mileage,
                        new ThinFillShallowCut(xdata.Mileage, FillCutType.ShallowCut, width, height));

                    // MessageBox.Show($"是浅挖");
                    _docMdf.WriteNow(xdata.Mileage, FillCutType.ShallowCut, width, height);
                }
                else
                {
                }
            }
            if (cutFillSections.Count == 0) return;

            // 断面的排序与分区
            var selectedSection =
                cutFillSections.Select(
                    r => new MileageInfo<ThinFillShallowCut>(r.Key, MileageInfoType.Measured, r.Value))
                    .ToList();
            var allSections = _allSections.Select(r => new MileageInfo<ThinFillShallowCut>(
                r.XData.Mileage, MileageInfoType.Located,
                new ThinFillShallowCut(r.XData.Mileage, FillCutType.Others, 0, 0))).ToList();

            // ！！！关键计算
            var cfData = Sort_Interpolate(selectedSection, allSections); // 排序与插值
            var segs = Category_Sumup1(cfData); // 分段并计算对应的面积
            if (segs == null || segs.Count == 0) return;

            // 将数据构造为数组，用来进行导出
            var sheetArr = ConstructSheetData(segs);
            var sheet_Infos = new List<WorkSheetData>()
            {
                new WorkSheetData(WorkSheetDataType.SlopeProtection, "低填浅挖", sheetArr)
            };

            // 数据导出到 Excel 
            var errMsg = ExportDataToExcel(sheet_Infos);

            if (errMsg != null)
            {
                var res = MessageBox.Show($"将数据导出到Excel中时出错：{errMsg}，\r\n是否将其以文本的形式导出？", "提示", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);
                if (res == MessageBoxResult.OK)
                {
                    // 数据导出到 Txt 
                    ExportAllDataToDirectory(sheet_Infos);
                }
            }
        }

        #region --- 判断低填

        /// <summary> 低填路堤 </summary>
        /// <param name="centerAxis"></param>
        /// <param name="width"> 左右路堤坡底之间的宽度</param>
        /// <param name="height"> 路堤处理的高度，指 *-**</param>
        /// <returns></returns>
        private static bool IsThinFill1(SubgradeSection centerAxis, out double width, out double height)
        {
            var center = centerAxis.XData;
            width = 0.0;
            height = 0.0;
            // 1. 基本判断标准
            if ((center.CenterElevation_Road - center.CenterElevation_Ground) <= ProtectionOptions.ThinFill_MaxDepth
                && center.LeftSlopeExists && center.LeftSlopeFill
                && center.RightSlopeExists && center.RightSlopeFill)
            {
                // 2. 以 1:5 作为低填的标准，保证坡线与(中心自然地面以1:5倾角放出的射线)的交点位于交点X处的对应的自然地面的上方
                var groundPt = new Point2d(center.CenterX, center.GetYFromElev(center.CenterElevation_Ground));
                var leftRay = new Ray2d(groundPt, new Vector2d(-ProtectionOptions.ThinFill_SlopeCriterion_lower, -1));
                var rightRay = new Ray2d(groundPt, new Vector2d(ProtectionOptions.ThinFill_SlopeCriterion_lower, -1));

                var leftPl = centerAxis.GetSlopeLine(true);
                var rightPl = centerAxis.GetSlopeLine(false);

                // 与左边坡的相关计算
                var pl = leftPl.Pline;
                var curve2d = pl.Get2dCurve();
                var inters = new CurveCurveIntersector2d(leftRay, curve2d);
                if (inters.NumberOfIntersectionPoints > 0)
                {
                    // 说明有交点，即不是低填
                    // return false;
                }
                else
                {
                    if (!leftPl.XData.FullyCalculated)
                    {
                        leftPl.CalculateXData();
                    }
                    width += centerAxis.XData.CenterX - leftPl.XData.Slopes.Last().BottomPoint.X;
                    // width += rightPl.XData.Slopes.Last().BottomPoint.X - leftPl.XData.Slopes.Last().BottomPoint.X;
                }

                // 与右边坡的相关计算
                pl = rightPl.Pline;
                curve2d = pl.Get2dCurve();
                inters = new CurveCurveIntersector2d(rightRay, curve2d);
                if (inters.NumberOfIntersectionPoints > 0)
                {
                    // 说明有交点，即不是低填
                    // return false;
                }
                else
                {
                    // 说明是属于低填路堤，下面计算其宽度与深度
                    if (!rightPl.XData.FullyCalculated)
                    {
                        rightPl.CalculateXData();
                    }
                    // 左右边坡坡底与自然地面交点之间的 X 间距
                    width += rightPl.XData.Slopes.Last().BottomPoint.X - centerAxis.XData.CenterX;
                }
                //
                if (width > 0)
                {
                    var topEle = center.LeftRoadCushionExists
                        ? center.CenterElevation_Cushion
                        : center.CenterElevation_Road;
                    // 低填路堤的自然地面下部处理高度
                    height = ProtectionOptions.ThinFill_TreatedDepth - (topEle - center.CenterElevation_Ground);
                    return true;
                }
            }
            return false;
        }

        /// <summary> 低填路堤 </summary>
        /// <param name="centerAxis"></param>
        /// <param name="width"> 左右路堤坡底之间的宽度</param>
        /// <param name="height"> 自然地面以下地基加固的高度</param>
        /// <returns></returns>
        private static bool IsThinFill2(Database db, SubgradeSection centerAxis, out double width, out double height)
        {
            var center = centerAxis.XData;
            width = 0.0;
            height = 0.0;
            // 1. 基本判断标准
            var depth = center.CenterElevation_Road - center.CenterElevation_Ground;
            if ((depth >= 0 && depth <= ProtectionOptions.ThinFill_MaxDepth) &&
                  center.LeftSlopeFill && center.RightSlopeFill)
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
                var leftWithinLf = WithinThinFillRange(groundPt, 1 / ProtectionOptions.ThinFill_SlopeCriterion_upper,
                    1 / ProtectionOptions.ThinFill_SlopeCriterion_upper, leftBottom);
                var rightWithinLf = WithinThinFillRange(groundPt, 1 / ProtectionOptions.ThinFill_SlopeCriterion_upper,
                    1 / ProtectionOptions.ThinFill_SlopeCriterion_upper, rightBottom);

                // 对于不同的情况进行分别处理
                if (leftWithinLf * rightWithinLf < 0)
                {
                    // 说明一侧过高，一侧过低，结果为陡坡路堤
                    return false;
                }
                else if (leftWithinLf + rightWithinLf == -2)
                {
                    // 说明两侧均过低，结果为陡坡路堤
                    return false;
                }
                else if (leftWithinLf + rightWithinLf == 2)
                {
                    // 说明两侧均过高，结果可认为是低填
                    width = rightBottom.X - leftBottom.X;
                }
                // 剩下的至少有一侧为0，即位于低填区间
                else if (leftWithinLf + rightWithinLf >= 0)
                {
                    // 另一侧要么为低填，要么过高，结果为全断面低填
                    width = rightBottom.X - leftBottom.X;
                }
                else
                {
                    // 另一侧肯定过低，考虑一半断面低填
                    if (leftWithinLf == 0)
                    {
                        // 右侧过低
                        width = center.CenterX - leftBottom.X;
                    }
                    else
                    {
                        // 左侧过低
                        width = rightBottom.X - center.CenterX;
                    }
                }
                //
                if (width > 0)
                {
                    var topEle = center.LeftRoadCushionExists
                     ? center.CenterElevation_Cushion
                     : center.CenterElevation_Road;
                    // 低填路堤的自然地面下部地基处理的高度
                    height = ProtectionOptions.ThinFill_TreatedDepth - (topEle - center.CenterElevation_Ground);
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
        /// <param name="slopeBottom"></param>
        /// <returns>1表示位于上边界之上，-1表示位于下边界之下，0 表示位于低填区间内</returns>
        private static sbyte WithinThinFillRange(Point3d ground, double upperDir, double lowerDir, Point3d slopeBottom)
        {
            var dir = (slopeBottom.Y - ground.Y) / Math.Abs((slopeBottom.X - ground.X));
            if (dir > upperDir)
            {
                return 1; // 非低填且向中心滑动
            }
            else if (dir < -lowerDir)
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
        private static bool IsShallowCut(Database db, SubgradeSection centerAxis, out double width, out double height)
        {
            var center = centerAxis.XData;
            width = 0.0;
            height = center.CenterElevation_Ground - center.CenterElevation_Road;
            // 1. 基本判断标准

            if (height >= 0 && height <= ProtectionOptions.ThinFill_MaxDepth)
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
                    1 / ProtectionOptions.ShallowCutl_SlopeCriterion_upper, out leftRoadWidth, out leftCenterCutWidth);
                var rightFillCut = HalfRoadShallowCut(center.RightSlopeFill,
                   center.RightGroundSurfaceHandle.GetDBObject<Polyline>(db),
                   center.RightRoadSurfaceHandle.GetDBObject<Polyline>(db),
                   1 / ProtectionOptions.ShallowCutl_SlopeCriterion_upper, out rightRoadWidth, out rightCenterCutWidth);

                // 2. 对各种情况进行分别处理

                // 对于不同的情况进行分别处理
                if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.ShallowCut))
                {
                    // 说明两侧都是浅挖
                    width = leftCenterCutWidth + rightCenterCutWidth;
                    return true;
                }
                else if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.FillLess31))
                {
                    // 说明一侧浅挖，一侧外缘填方小于三分之一道路宽度
                    width = leftCenterCutWidth + rightCenterCutWidth;
                    return true;
                }
                else if ((leftFillCut | rightFillCut) == (HalfFillCut.ShallowCut | HalfFillCut.NoneShallowCut))
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
                else if ((leftFillCut | rightFillCut) == (HalfFillCut.FillLess31 | HalfFillCut.NoneShallowCut))
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
                //
                if (width > 0)
                {
                    // 低填路堤的自然地面下部地基处理的高度
                    height = center.CenterElevation_Ground - center.CenterElevation_Road;
                    if (height < 0)
                    {
                        height = 0;
                    }
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
        /// <param name="centerCutWidth">当边坡为填方时（且道路中线处为挖方），此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
        /// 如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖</param>
        /// <param name="upperDir">浅挖路堑判断向量的上边界，对于 1:5 的极限值，此参数的值为0.2</param>
        /// <returns></returns>
        private static HalfFillCut HalfRoadShallowCut(bool slopeFill, Polyline ground, Polyline road, double upperDir,
            out double halfRoadWidth, out double centerCutWidth)
        {
            halfRoadWidth = road.Length;  // 路面+路肩
            var pt = road.GetLineSegment2dAt(road.NumberOfVertices - 2).StartPoint; // 路面线最外侧向中间走，第二个顶点，即最后一段的起点
            var halfCushionWidth = road.StartPoint.DistanceTo(new Point3d(pt.X, pt.Y, 0));  // 一半路面的宽度
            centerCutWidth = halfCushionWidth;
            var ground2d = ground.Get2dCurve();
            var groundTop = ground.StartPoint; // 道路中线 与 自然地面 的交点
            if (!slopeFill)
            {
                var l = new Ray2d(pt, new Vector2d(0, 1));
                var inters = new CurveCurveIntersector2d(l, ground2d);
                if (inters.NumberOfIntersectionPoints > 0) // 正常情况下，肯定会有交点
                {
                    var ints = inters.GetIntersectionPoint(0);
                    var dir = (ints.Y - groundTop.Y) / (ints.X - groundTop.X);
                    if (dir <= upperDir)
                    {
                        return HalfFillCut.ShallowCut;
                    }
                }
                else
                {
                    return HalfFillCut.NoneShallowCut;
                }
                return HalfFillCut.ShallowCut;
            }
            else
            {
                // 填方路堤，在保证道路中心处为挖方的情况下，此时自然地面线一般会与路面线相交，返回相交后靠近道路中线侧的挖方宽度；
                // 如果自然地面线与路面线不相交（），则将此侧道路按全挖方考虑，且为全浅挖
                var road2d = road.Get2dCurve();
                var inters = new CurveCurveIntersector2d(road2d, ground2d);
                if (inters.NumberOfIntersectionPoints > 0) // 正常情况下，肯定会有交点
                {
                    var ints = inters.GetIntersectionPoint(0);
                    var ratio = Math.Abs(ints.X - groundTop.X) / halfRoadWidth;  // 靠近道路中线侧的挖方宽度 相对于 整个（路面+路肩）宽度的比例
                    if (ratio > 0.6666666666666666666666666666666666666666666667)
                    {
                        // 说明道路中间以挖方为主
                        centerCutWidth = halfCushionWidth;
                        return HalfFillCut.FillLess31;
                    }
                    else
                    {
                        // 说明道路中间挖方很少，大部分是外缘的填方
                        centerCutWidth = 0;  // 不计入浅挖的量
                        return HalfFillCut.FillLarger31;
                    }
                }
                else
                {
                    // 正常情况下，不会没有交点
                    centerCutWidth = halfRoadWidth;
                    return HalfFillCut.ShallowCut;
                }
            }
        }
        #endregion

        #region --- 分区并计算每一区段的结果

        /// <summary>
        /// 根据整个项目的所有断面（包括测量、标识和插值类型）上的数据，将整个项目进行分区，并计算每个分区的相关工程量
        /// </summary>
        /// <param name="sortedSections">整个项目的所有断面（包括测量、标识和插值类型），所以集合中的元素的数量大于等于整个项目中所有横断面数量</param>
        /// <returns>以工程量从0到非0时，非0值所对应的桩号作为区段的起点，并不考虑从0到非0段的三角形面积</returns>
        private List<SegmentData<double, object[]>> Category_Sumup1(
            IList<MileageInfo<ThinFillShallowCut>> sortedSections)
        {
            if (sortedSections != null && sortedSections.Count > 0)
            {
                if (sortedSections.Count < 2)
                {
                    MessageBox.Show("必须指定至少两个桩号才能计算分段面积");
                    return null;
                }
                var res = new List<SegmentData<double, object[]>>();
                var lastMl = sortedSections[0]; // 上一个桩号
                var startMile = lastMl.Mileage;
                // var lastIsZero = lastMl.Value.Type == FillCutType.Others;

                var areaWidth = 0.0; // 低填浅挖断面的处理宽度 所对应的面积
                var areaHeight = 0.0; // 低填浅挖断面的处理高度 所对应的面积
                for (int i = 1; i < sortedSections.Count; i++)
                {
                    var ml = sortedSections[i];
                    var m = ml.Mileage; // 桩号
                    var w = ml.Value.Width; // 处理宽度
                    var h = ml.Value.Height; // 处理高度

                    //
                    // var thisIsZero = ml.Value.Type == FillCutType.Others;
                    var withinRange = (ml.Value.Type == lastMl.Value.Type); //  说明位于0区段或者非0区段中间，而不是到了区段的起点或终点
                    if (withinRange) // 
                    {
                        // 说明位于0区段或者非0区段中间，而不是到了区段的起点或终点
                        if (ml.Value.Type != FillCutType.Others)
                        {
                            // 说明现在正位于非0区段

                            // 求梯形面积（当前一个桩号的工程量为0时，即为三角形面积）
                            areaWidth += (lastMl.Value.Width + w) * (m - lastMl.Mileage) / 2;
                            areaHeight += (lastMl.Value.Height + h) * (m - lastMl.Mileage) / 2;
                        }
                    }
                    else
                    {
                        if (lastMl.Value.Type == FillCutType.Others)  // 说明到了分段的起点
                        {
                            startMile = m; // lastMl.Mileage;
                            areaWidth = 0;
                            areaHeight = 0;
                        }
                        else if (ml.Value.Type == FillCutType.Others) // 说明到了分段的终点
                        {
                            res.Add(new SegmentData<double, object[]>(startMile, lastMl.Mileage,
                             new object[] { lastMl.Value.GetDescription(), areaWidth, areaHeight }));
                            areaWidth = 0;
                            areaHeight = 0;
                        }
                        else // 说明到了分段的终点，同时也是另一种形式的起点，比如从低填过渡到浅挖
                        {
                            res.Add(new SegmentData<double, object[]>(startMile, lastMl.Mileage,
                            new object[] { lastMl.Value.GetDescription(), areaWidth, areaHeight }));
                            //
                            startMile = m; // lastMl.Mileage;
                            areaWidth = 0;
                            areaHeight = 0;
                        }
                    }
                    lastMl = ml;
                }
                // 对最后一个桩号进行操作，即最后一个桩号非零的情况下，其面积还没有闭合
                if (lastMl.Value.Type != FillCutType.Others)
                {
                    res.Add(new SegmentData<double, object[]>(startMile, lastMl.Mileage,
                        new object[] { lastMl.Value.GetDescription(), areaWidth, areaHeight }));
                }
                return res;
            }
            return null;
        }

        /// <summary>
        /// 根据整个项目的所有断面（包括测量、标识和插值类型）上的数据，将整个项目进行分区，并计算每个分区的相关工程量
        /// </summary>
        /// <param name="sortedSections">整个项目的所有断面（包括测量、标识和插值类型），所以集合中的元素的数量大于等于整个项目中所有横断面数量</param>
        /// <returns>以工程量从0到非0时，0所对应的桩号作为区段的起点，并考虑从0到非0段的三角形面积</returns>
        private List<SegmentData<double, double[]>> Category_Sumup2(
            IList<MileageInfo<ThinFillShallowCut>> sortedSections)
        {
            if (sortedSections != null && sortedSections.Count > 0)
            {
                if (sortedSections.Count < 2)
                {
                    MessageBox.Show("必须指定至少两个桩号才能计算分段面积");
                    return null;
                }
                var res = new List<SegmentData<double, double[]>>();
                var lastMl = sortedSections[0]; // 上一个桩号
                var startMile = lastMl.Mileage;
                bool lastIsZero = lastMl.Value.Type == FillCutType.Others;

                var areaWidth = 0.0; // 低填浅挖断面的处理宽度 所对应的面积
                var areaHeight = 0.0; // 低填浅挖断面的处理高度 所对应的面积
                for (int i = 1; i < sortedSections.Count; i++)
                {
                    var ml = sortedSections[i];
                    var m = ml.Mileage; // 桩号
                    var w = ml.Value.Width; // 处理宽度
                    var h = ml.Value.Height; // 处理高度

                    // 求梯形面积（当前一个桩号的工程量为0时，即为三角形面积）
                    areaWidth += (lastMl.Value.Width + w) * (m - lastMl.Mileage) / 2;
                    areaHeight += (lastMl.Value.Height + h) * (m - lastMl.Mileage) / 2;
                    //
                    var thisIsZero = ml.Value.Type == FillCutType.Others;
                    if (lastIsZero ^ thisIsZero) // 
                    {
                        if (thisIsZero) // 说明到了分段的终点
                        {
                            res.Add(new SegmentData<double, double[]>(startMile, m, new double[] { areaWidth, areaHeight }));
                            areaWidth = 0;
                            areaHeight = 0;
                        }
                        else // 说明到了分段的起点
                        {
                            startMile = lastMl.Mileage;
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
                    res.Add(new SegmentData<double, double[]>(startMile, lastMl.Mileage,
                        new double[] { areaWidth, areaHeight }));
                }
                return res;
            }
            return null;
        }

        #endregion

        #region --- 构造一个工作表的数据

        /// <summary> 构造Excel工作表中的表格数据：低填浅挖工程量表 </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        private Array ConstructSheetData(List<SegmentData<double, object[]>> segs)
        {
            // var segs = Category_Sumup(spData); // 分段并计算对应的面积
            //
            // var sectionsArr = SegmentData<double, double[]>.ConvertToArr(segs);
            var arr = new object[segs.Count, 5];
            for (int i = 0; i < segs.Count; i++)
            {
                var s = segs[i];
                var kilo =
                    $"K{Math.Floor(s.Start / 1000)}+{(s.Start % 1000).ToString("000")}~K{Math.Floor(s.End / 1000)}+{(s.End % 1000).ToString("000")}";
                // 桩号区段
                var length = s.End - s.Start;
                var desc = s.Data[0];
                var avgWidth = (double)(s.Data[1]) / (s.End - s.Start);
                var avgHeight = (double)(s.Data[2]) / (s.End - s.Start);
                arr[i, 0] = kilo; // 桩号区段
                arr[i, 1] = length;
                arr[i, 2] = desc; // 分段的描述
                arr[i, 3] = avgWidth; // 低填浅挖区段中，边坡宽度的平均值
                arr[i, 4] = avgHeight; // 低填浅挖区段中，边坡处理高度的平均值
            }

            // 添加表头信息
            var header = new string[] { "区间", "长度", "方式", "边坡平均宽度", "中心平均加固高度" };
            arr = arr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1f });

            return arr;
        }

        #endregion
    }
}