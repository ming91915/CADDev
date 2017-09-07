using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 在两个相邻的边坡之间计算其每一个子边坡所占的宽度与几何面积。
    /// 计算算法为将两个断面的边坡构造为一个二维的几何模型。 </summary>
    public class SlopeSegsGeom
    {
        #region ---   Fields

        private readonly double _backStation;

        /// <summary> 此属性不会为 null，如果某桩号某一侧没有边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）时，则此属性的值为<seealso cref="SlopeData"/>的默认实例对象 </summary>
        private readonly SlopeData _backSlope;

        private readonly double _frontStation;

        /// <summary> 此属性不会为 null，如果某桩号某一侧没有边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）时，则此属性的值为<seealso cref="SlopeData"/>的默认实例对象 </summary>
        private readonly SlopeData _frontSlope;

        /// <summary> 前后边坡之间的中点桩号 </summary>
        private readonly double _middleStation;

        /// <summary> 前面两个边坡之间的桩号距离 </summary>
        private readonly double _stationWidth;

        /// <summary> 后面边坡的坡顶或坡底 </summary>
        private Point2d _backEdge;

        /// <summary> 前面边坡的坡顶或坡底 </summary>
        private Point2d _frontEdge;

        /// <summary> 字典中 key 表示每一个边坡的Index值，其中正值表示边坡，负值表示平台；
        /// value 表示关键点在构造的二维边坡系统中的坐标，数组中有两个坐标值，
        /// 第一个的Y值表示此子边坡的内侧点所在的斜坡长度 ，第二个的Y值表示此子边坡的外侧点所在的斜坡长度，
        /// Y值中正值表示挖方边坡，负值表示填方边坡 </summary>
        private Dictionary<double, Point2d[]> _backSlopePoints;

        /// <summary> 字典中 key 表示每一个边坡的Index值，其中正值表示边坡，负值表示平台；
        /// value 表示关键点在构造的二维边坡系统中的坐标，数组中有两个坐标值，
        /// 第一个的Y值表示此子边坡的内侧点所在的斜坡长度 ，第二个的Y值表示此子边坡的外侧点所在的斜坡长度，
        /// Y值中正值表示挖方边坡，负值表示填方边坡 </summary>
        private Dictionary<double, Point2d[]> _frontSlopePoints;

        /// <summary> 后面边坡的坡顶或坡底所对应的Index，当没有任何对应子边坡时，其值为 0 </summary>
        //private double _maxBackSlopeIndex;

        /// <summary> 前面边坡的坡顶或坡底所对应的Index，当没有任何对应子边坡时，其值为 0  </summary>
        //private double _maxFrontSlopeIndex;

        private Line2d _开口线;

        #endregion

        /// <summary> 在两个相邻的边坡之间计算其每一个子边坡所占的宽度与几何面积 </summary>
        /// <param name="backStation"></param>
        /// <param name="backSlope">此属性不会为 null，如果某桩号某一侧没有边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）时，则此属性的值为<seealso cref="SlopeData"/>的默认实例对象</param>
        /// <param name="frontStation">其值可能为null，表示没有此边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）</param>
        /// <param name="frontSlope">此属性不会为 null，如果某桩号某一侧没有边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）时，则此属性的值为<seealso cref="SlopeData"/>的默认实例对象</param>
        public SlopeSegsGeom(double backStation, SlopeData backSlope,double frontStation, SlopeData frontSlope)
        {
            _backStation = backStation;
            _backSlope = backSlope;

            _frontStation = frontStation;
            _frontSlope = frontSlope;
            //
            _stationWidth = Math.Abs(backStation - frontStation);
            _middleStation = (backStation + frontStation) / 2;
            ConstructSlopeSystem();
        }

        /// <summary> 根据前后边坡对象构造二维边坡系统 </summary>
        private void ConstructSlopeSystem()
        {
            double backBaseHeight = Math.Abs(_backSlope.RetainingWallHeight);

            // 后方边坡
            double innerLength = backBaseHeight; // 斜坡长度
            double outterLength = innerLength; // 斜坡长度

            var backSign = _backSlope.FillCut ? -1 : 1;
            _backSlopePoints = new Dictionary<double, Point2d[]>();
            var segs = SlopeData.Combine(_backSlope.Slopes, _backSlope.Platforms, true);
            foreach (var bs in segs)
            {
                if (bs.Type == SlopeSegType.边坡)
                {
                    outterLength = innerLength + bs.Length;
                    _backSlopePoints.Add(bs.Index,
                        new Point2d[]
                        {new Point2d(_backStation, backSign*innerLength), new Point2d(_backStation, backSign*outterLength)});
                    innerLength = outterLength;
                }
                else
                {
                    // Point2d[] 数组中两个值是相同的
                    _backSlopePoints.Add(-bs.Index,
                        new Point2d[]
                        {new Point2d(_backStation, backSign*innerLength), new Point2d(_backStation, backSign*innerLength)});
                }
            }
            //_maxBackSlopeIndex = _backSlope.Slopes.Count > 0 ? _backSlope.Slopes.Last().Index : 0;
            // _backEdge = _maxBackSlopeIndex > 0 ? _backSlopePoints[_maxBackSlopeIndex][1] : new Point2d(_backStation, 0);
            _backEdge = new Point2d(_backStation, backSign * outterLength);

            // 前方边坡
            var frontSign = _frontSlope.FillCut ? -1 : 1;
            double frontBaseHeight = Math.Abs(_frontSlope.RetainingWallHeight);
            innerLength = frontBaseHeight;
            outterLength = innerLength;
            _frontSlopePoints = new Dictionary<double, Point2d[]>();

            segs = SlopeData.Combine(_frontSlope.Slopes, _frontSlope.Platforms, true);
            foreach (var bs in segs)
            {
                if (bs.Type == SlopeSegType.边坡)
                {
                    outterLength = innerLength + bs.Length;
                    _frontSlopePoints.Add(bs.Index,
                        new Point2d[]
                        {
                                new Point2d(_frontStation, frontSign*innerLength),new Point2d(_frontStation, frontSign*outterLength)
                        });
                    innerLength = outterLength;
                }
                else
                {
                    // Point2d[] 数组中两个值是相同的
                    _frontSlopePoints.Add(-bs.Index,
                        new Point2d[]
                        {new Point2d(_frontStation, frontSign*innerLength), new Point2d(_frontStation, frontSign*innerLength)});
                }
            }
            //_maxFrontSlopeIndex = _frontSlope.Slopes.Count > 0 ? _frontSlope.Slopes.Last().Index : 0;

            // _frontEdge = _maxFrontSlopeIndex > 0 ? _frontSlopePoints[_maxFrontSlopeIndex][1] : new Point2d(_frontStation, 0);
            _frontEdge = new Point2d(_frontStation, frontSign * outterLength);


            // 开口线
            _开口线 = new Line2d(_backEdge, _frontEdge);
        }

        /// <summary> 后方平台向前走的长度与对应占据的几何面积 </summary>
        public void GetBackPlatformLengthAndArea(Platform backPltfm, out double frontWidth, out double frontArea)
        {
            if (_backSlope.FillCut != _frontSlope.FillCut)
            {
                // 填挖交界
                var pt = _backSlopePoints[-backPltfm.Index][0];
                var inters = _开口线.IntersectWith(new Line2d(pt, new Vector2d(1, 0)));
                if (inters != null)
                {
                    frontWidth = Math.Abs(pt.X - inters.First().X);
                }
                else
                {
                    frontWidth = _stationWidth;
                }
                frontArea = frontWidth * backPltfm.ProtectionLength;
            }
            else
            {
                var pt = _backSlopePoints[-backPltfm.Index][1];
                // 说明前后边坡同为填方或者同为挖方
                if (Math.Abs(_frontEdge.Y) > Math.Abs(pt.Y))
                {
                    // 说明前方边坡最外边界的Y值大于此后方平台的Y值
                    // 说明有对应的平台
                    frontWidth = _stationWidth / 2;
                    // 后边坡向前的平台面积：前后边坡的对应平台所构成的梯形（一般为矩形）被中线截开后的面积
                    frontArea = backPltfm.ProtectionLength * frontWidth;
                }
                else
                {
                    // 说明前方边坡最外边界的Y值都小于此后方平台的Y值
                    // 前方边坡肯定无对应的平台
                    // 先求水平平台与开口线的交点，再取最小水平长度
                    var inters = _开口线.IntersectWith(new Line2d(pt, new Vector2d(1, 0)));
                    if (inters != null)
                    {
                        var intersWidth = Math.Abs(pt.X - inters.First().X);
                        frontWidth = Math.Min(intersWidth, _stationWidth);
                    }
                    else
                    {
                        // 说明开口线是水平的，可认为边坡顶部即为水平平台。
                        frontWidth = _stationWidth;
                    }
                    frontArea = backPltfm.ProtectionLength * frontWidth;
                }
            }
        }

        /// <summary> 后方边坡向前走的长度与对应占据的几何面积 </summary>
        public void GetBackSlopeLengthAndArea(Slope backSlope, out double frontWidth, out double frontArea)
        {
            if (_backSlope.FillCut != _frontSlope.FillCut)
            {
                // 填挖交界
                var pts = _backSlopePoints[backSlope.Index];
                var inters0 = _开口线.IntersectWith(new Line2d(pts[0], new Vector2d(1, 0)));
                var inters1 = _开口线.IntersectWith(new Line2d(pts[1], new Vector2d(1, 0)));
                //
                var width0 = inters0 != null ? Math.Abs(inters0.First().X - pts[0].X) : _stationWidth;
                var width1 = inters1 != null ? Math.Abs(inters1.First().X - pts[1].X) : _stationWidth;
                //width0 = Math.Max(width0, _stationWidth);
                //width1 = Math.Max(width1, _stationWidth);

                //
                frontWidth = Math.Max(width0, width1);
                frontArea = (width0 + width1) * backSlope.ProtectionLength / 2;
            }
            else // 前后边坡均为填方或挖方
            {
                var backInnerPt = _backSlopePoints[backSlope.Index][0];
                var backoutterPt = _backSlopePoints[backSlope.Index][1];
                if (Math.Abs(backInnerPt.Y) >= Math.Abs(_frontEdge.Y))
                {
                    // 1.1、1.2 ：说明后方边坡靠内侧点的Y值比前方边坡最外边界的Y值都要大，因此此边坡所占据的几何面积为与开口线相交的梯形或三角形
                    var innerInters = _开口线.IntersectWith(new Line2d(backInnerPt, new Vector2d(1, 0)));
                    var innerWidth = (innerInters != null)
                        ? Math.Abs(backInnerPt.X - innerInters.First().X)
                        : _stationWidth;
                    var outterInters = _开口线.IntersectWith(new Line2d(backoutterPt, new Vector2d(1, 0)));
                    var outerWidth = (outterInters != null)
                        ? Math.Abs(backInnerPt.X - outterInters.First().X)
                        : _stationWidth;
                    frontWidth = Math.Max(innerWidth, outerWidth);
                    frontArea = (innerWidth + outerWidth) * backSlope.ProtectionLength / 2;
                    // 
                }
                else if (Math.Abs(backoutterPt.Y) <= Math.Abs(_frontEdge.Y))
                {
                    // 说明后方边坡靠外侧点的Y值比前方边坡最外边界的Y值都要小
                    if (backSlope.Index == _backSlopePoints.Keys.Max())
                    {
                        // 此子边坡外侧还有其他子边坡，此时根据前方边坡的具体情况，可以分为两种情形
                        var ys = _frontSlopePoints.Values.Select(r => r).ToList();
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                            _backStation.ToString() + "," + _frontStation.ToString() + "\n");

                        Point2d? frontOutter = null; // 正常情况下，至少会有一个点，如果没有，说明前方边坡为挡墙
                        foreach (var v in _frontSlopePoints.Values)
                        {
                            if (Math.Abs(v[1].Y) >= Math.Abs(backoutterPt.Y))
                            {
                                frontOutter = v[1];
                                break;
                            }
                        }
                        var frontOutterPt = frontOutter ?? _frontEdge;

                        var middleTop =
                            _开口线.IntersectWith(new Line2d(new Point2d(_middleStation, 0), new Vector2d(0, 1)));
                        var middleTopY = middleTop[0].Y;
                        if (Math.Abs(middleTopY) <= Math.Abs(frontOutterPt.Y))
                        {
                            // 3.3： 所占区域为一个梯形
                            frontWidth = _stationWidth / 2;
                            frontArea = (backSlope.ProtectionLength + Math.Abs(middleTopY - backInnerPt.Y)) * frontWidth / 2;
                        }
                        else
                        {
                            // 3.2： 所占区域为一个五边形，其面积通过矩形剪去三角形来得到
                            frontWidth = _stationWidth / 2;
                            var middleTopInters = _开口线.IntersectWith(new Line2d(new Point2d(0, frontOutterPt.Y), new Vector2d(-1, 0)));
                            var middleTopInter = middleTopInters != null ? middleTopInters[0] : backoutterPt;

                            var areaRec = Math.Abs(frontOutterPt.Y - backInnerPt.Y) * frontWidth;
                            var areaTri = Math.Abs(backoutterPt.X - middleTopInter.X) * Math.Abs(middleTopInter.Y - backoutterPt.Y) / 2;
                            frontArea = areaRec - areaTri;
                        }
                    }
                    else
                    {
                        // 3.1：因此此边坡所占据的几何面积标准矩形
                        frontWidth = _stationWidth / 2;
                        frontArea = frontWidth * backSlope.ProtectionLength;
                    }
                }
                else
                {
                    // 说明后方边坡靠外侧点的Y值比前方边坡最外边界的Y值要大，而后方边坡靠内侧点的Y值比前方边坡最外边界的Y值要小
                    // 此时根据后方边坡的外侧点与开口线的交点不同，可以分为两种情况
                    var middleTopInters =
                        _开口线.IntersectWith(new Line2d(new Point2d(_middleStation, 0), new Vector2d(0, 1)))[0];
                    var middleTopIntersY = middleTopInters.Y;

                    if (Math.Abs(middleTopIntersY) >= Math.Abs(backoutterPt.Y))
                    {
                        // 2.1：后方边坡所占据的几何面积标准矩形
                        frontWidth = _stationWidth / 2;
                        frontArea = frontWidth * backSlope.ProtectionLength;
                    }
                    else
                    {
                        // 2.2： 所占区域为一个五边形，其面积通过矩形剪去三角形来得到
                        frontWidth = _stationWidth / 2;

                        var backTopInters = _开口线.IntersectWith(new Line2d(backoutterPt, new Vector2d(1, 0)));
                        var backTopInter = backTopInters != null ? backTopInters[0] : middleTopInters;

                        var areaRec = backSlope.ProtectionLength * frontWidth;
                        var areaTri = Math.Abs(backTopInter.X - middleTopInters.X) * Math.Abs(backTopInter.Y - middleTopInters.Y) / 2;
                        frontArea = areaRec - areaTri;
                    }
                }
            }
        }
    }
}