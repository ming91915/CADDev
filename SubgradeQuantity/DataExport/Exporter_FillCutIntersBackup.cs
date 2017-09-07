using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Converters;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 纵向填挖交界 </summary>
    public class Exporter_FillCutIntersBackup
    {

        #region --- Types

        /// <summary> 纵向填挖交界 </summary>
        public class FillCutInters : StationRangeEntity
        {
            public FillCutInters(double startStation, double endStation) : base(startStation, endStation)
            {
            }
        }

        /// <summary> 纵向挖台阶 </summary>
        public class LongitudinalStairExcav : StationRangeEntity, IMergeable
        {
            /// <summary> 纵向挖台阶面积 </summary>
            public double StairArea { get; set; }

            public LongitudinalStairExcav(double startStation, double endStation, double stairArea) : base(startStation, endStation)
            {
                StairArea = stairArea;
            }

            public bool IsMergeable(IMergeable next)
            {
                var nextRange = next as LongitudinalStairExcav;
                if ((nextRange.StartStation - EndStation > ProtectionConstants.RangeMergeTolerance)
                    || (StartStation - nextRange.EndStation > ProtectionConstants.RangeMergeTolerance))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public void Merge(IMergeable next)
            {
                var nextRange = next as LongitudinalStairExcav;
                StairArea += nextRange.StairArea;
                EndStation = nextRange.EndStation;
            }
        }

        #endregion

        #region --- Fields

        private readonly DocumentModifier _docMdf;
        private readonly CompositeCurve2d _roadCurve2d;
        private readonly CompositeCurve2d _groundCurve2d;
        //private readonly Polyline _road;
        //private readonly Polyline _ground;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="roadCurve"></param>
        /// <param name="groundCurve"></param>
        public Exporter_FillCutIntersBackup(DocumentModifier docMdf, CompositeCurve3d roadCurve, CompositeCurve3d groundCurve)
        {
            _docMdf = docMdf;
            _roadCurve2d = roadCurve.Get2dLinearCurve();
            _groundCurve2d = groundCurve.Get2dLinearCurve();
            //_road = Curve.CreateFromGeCurve(roadCurve) as Polyline;
            //_ground = Curve.CreateFromGeCurve(groundCurve) as Polyline;
        }

        #region --- 纵向填挖交界

        /// <summary> 纵向填挖交界 </summary>
        public void ExportFillCutInters()
        {

            var inters = new CurveCurveIntersector2d(_roadCurve2d, _groundCurve2d);
            if (inters.NumberOfIntersectionPoints == 0)
            {
                // 整个路段都是填方或者挖方
                _docMdf.WriteNow("没有填挖交界交点");
                return;
            }
            //
            // 对填挖交界点进行处理
            _docMdf.WriteLineIntoDebuger("填挖交界点的坐标：");

            int interval = 2;
            var fillLargerThan = 5.0;
            int fillCheckLength = 10;
            var arrCutToFill = ArrayConstructor.FromRangeAri(0, fillCheckLength, interval);
            var arrFillToCut = ArrayConstructor.FromRangeAri(0, -fillCheckLength, -interval);

            for (int i = 0; i < inters.NumberOfIntersectionPoints; i++)
            {
                var ptRoad = inters.GetPointOnCurve1(i);
                var ptGround = inters.GetPointOnCurve2(i);

                // 排除桥梁等结构区域
                var withInStructure = false;
                if (withInStructure)
                {
                    break;
                }
                //
                var fillToCut = FilltoCut(ptRoad, ptGround);
                var arrDx = fillToCut ? arrFillToCut : arrCutToFill;
                var intersX = ptRoad.Point.X;
                var maxVerticalDiff = 0.0;
                // 填挖交界处的路基，在填方段10m范围内高度H＜5m时，按断面A实施，H＞5m时，按断面B实施。
                foreach (var dx in arrDx)
                {
                    var x = intersX + dx;
                    var intersVerticalRoad = new CurveCurveIntersector2d(_roadCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    var intersVerticalGround = new CurveCurveIntersector2d(_groundCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                        intersVerticalGround.NumberOfIntersectionPoints == 0)
                    {
                        break;
                    }
                    else
                    {
                        var verticalDiff = Math.Abs(intersVerticalGround.GetIntersectionPoint(0).Y - intersVerticalRoad.GetIntersectionPoint(0).Y);
                        if (verticalDiff > maxVerticalDiff)
                        {
                            maxVerticalDiff = verticalDiff;
                        }
                    }
                }
                string fill = fillToCut ? "填 - 挖" : "挖 - 填";
                var reinforce = (maxVerticalDiff > fillLargerThan) ? "超挖换填 + 土工格栅" : "超挖换填";
                _docMdf.WriteLineIntoDebuger(ptRoad.Point.X, fill, reinforce);
                //
            }

            // 断面的判断与计算
            _docMdf.WriteNow($"填挖交界交点数量：{inters.NumberOfIntersectionPoints}");
            _docMdf.WriteNow("交界点桩号", "交点形式", "处理措施");
        }

        #endregion

        #region --- 纵向挖台阶处理

        /// <summary> 纵向挖台阶处理 </summary>
        public void ExportLongitudinalStairExcav()
        {
            var stairs = new List<LongitudinalStairExcav>();
            var inters = new CurveCurveIntersector2d(_roadCurve2d, _groundCurve2d);
            if (inters.NumberOfIntersectionPoints == 0)
            {
                var middleFillHeight = GetFillHeight((_roadCurve2d.StartPoint.X + _roadCurve2d.EndPoint.X) / 2);
                if (middleFillHeight < 0)
                {
                    _docMdf.WriteNow("整个路段都为挖方");
                    return;
                }
                else
                {
                    CalculateFillRange(_roadCurve2d.StartPoint.X, _roadCurve2d.EndPoint.X, stairs);
                }
            }
            else
            {
                // 对填挖交界点进行处理
                var lastIntersX = _roadCurve2d.StartPoint.X;
                bool fillToCut = false;

                // 1. [道路起点 ~ 最后一个交点]之间的区段
                for (int i = 0; i < inters.NumberOfIntersectionPoints; i++)
                {
                    var ptRoad = inters.GetPointOnCurve1(i);
                    var ptGround = inters.GetPointOnCurve2(i);
                    var intersX = ptRoad.Point.X;

                    //
                    fillToCut = FilltoCut(ptRoad, ptGround);
                    if (fillToCut)
                    {
                        // 此交点与其前面一个交点之间为填方区
                        CalculateFillRange(lastIntersX, intersX, stairs);
                    }
                    lastIntersX = intersX;

                    //
                }
                // 2. [最后一个交点 ~ 道路终点]之间的区段
                if (!fillToCut)
                {
                    CalculateFillRange(lastIntersX, _roadCurve2d.EndPoint.X, stairs);
                }
            }

            // 排除桥梁等区间阻隔
            var structs = Options_Collections.Structures;
            stairs = stairs.Where(st => !st.IntersectStructureBlocks(structs)).ToList();
            
            // 区间合并
            _docMdf.WriteNow($"合并前的纵向挖台阶数量：{stairs.Count}");
            stairs = MergeLinkedRanges(stairs);
            _docMdf.WriteNow($"合并后的纵向挖台阶数量：{stairs.Count}");
            //

            // 断面的判断与计算
            _docMdf.WriteLineIntoDebuger($"纵向挖台阶数量：{stairs.Count}");
                _docMdf.WriteLineIntoDebuger("起点", "终点", "区间", "段落长度", "纵向挖台阶面积");
            foreach (var st in stairs)
            {
                _docMdf.WriteLineIntoDebuger(st.StartStation, st.EndStation, ProtectionUtils.GetStationString(st.StartStation, st.EndStation,0), 
                    st.EndStation - st.StartStation, st.StairArea);
            }


            // 将结果整理为二维数组，用来进行表格输出
            var sheetArr = new object[stairs.Count + 1, 8];

        }

        /// <summary>
        /// 计算填方区间的挖台阶量
        /// </summary>
        /// <param name="startStation"></param>
        /// <param name="endStation"></param>
        private void CalculateFillRange(double startStation, double endStation, List<LongitudinalStairExcav> stairs)
        {
            int interval = 10;
            if (endStation - startStation < interval)
            {
                return;
            }
            // 求分段交点（集合中至少有两个值）
            var xy = new List<double[]>();
            for (double x = startStation; x < endStation; x += interval)
            {
                var intersVerticalGround = new CurveCurveIntersector2d(_groundCurve2d, new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                xy.Add(new double[] { x, intersVerticalGround.GetIntersectionPoint(0).Y });
            }

            // 求斜率
            for (int i = 0; i < xy.Count - 1; i++)
            {
                var ratio = Math.Abs((xy[i + 1][1] - xy[i][1]) / ((xy[i + 1][0] - xy[i][0])));
                if (ratio > 0.12)
                {
                    var area = Math.Abs((xy[i + 1][0] - xy[i][0]) * (xy[i + 1][1] - xy[i][1])) / 2;
                    stairs.Add(new LongitudinalStairExcav(xy[i][0], xy[i + 1][0], area));
                }
            }
        }


        /// <summary> 将多个断面区间进行合并 </summary>
        /// <param name="stairs">路基某一侧的高填或深挖边坡对象</param>
        /// <returns></returns>
        private List<LongitudinalStairExcav> MergeLinkedRanges(List<LongitudinalStairExcav> stairs)
        {
            if (stairs.Count == 0) return stairs;

            var res = new List<LongitudinalStairExcav>();
            var lastRange = stairs[0];
            res.Add(lastRange);
            for (int i = 1; i < stairs.Count; i++)
            {
                var rg = stairs[i];
                var mergeable = lastRange.IsMergeable(rg);
                if (mergeable)
                {
                    lastRange.Merge(rg);
                }
                else
                {
                    res.Add(rg);
                    lastRange = rg;
                }
            }
            return res;
        }

        #endregion

        /// <summary> 交界点是从填进行挖，还是从挖进入填 </summary>
        /// <returns></returns>
        private bool FilltoCut(PointOnCurve2d ptRoad, PointOnCurve2d ptGround)
        {
            var ratioRoad = ptRoad.GetDerivative(1);
            var ratioGround = ptGround.GetDerivative(1);
            if (Math.Tan(ratioGround.Angle) > Math.Tan(ratioRoad.Angle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 纵断面中某个桩号所对应的填方高度，如果为负值，则代表挖方高度 </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        private double GetFillHeight(double station)
        {
            var intersVerticalRoad = new CurveCurveIntersector2d(_roadCurve2d,
                     new Line2d(new Point2d(station, 0), new Vector2d(0, 1)));
            var intersVerticalGround = new CurveCurveIntersector2d(_groundCurve2d,
                new Line2d(new Point2d(station, 0), new Vector2d(0, 1)));
            if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                intersVerticalGround.NumberOfIntersectionPoints == 0)
            {
                // 这种情况一般不会出现
            }
            else
            {
                var yRoad = intersVerticalRoad.GetIntersectionPoint(0).Y;
                var yGround = intersVerticalGround.GetIntersectionPoint(0).Y;
                return yRoad - yGround;
            }
            return 0.0;
        }
    }
}