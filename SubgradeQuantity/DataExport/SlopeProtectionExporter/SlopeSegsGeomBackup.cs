using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;

namespace eZcad.SubgradeQuantity.DataExport
{
    public partial class Exporter_SlopeProtection
    {
        /// <summary> 在两个相邻的边坡之间计算其每一个子边坡所占的宽度与几何面积。
        /// 计算算法为将两个断面的边坡构造为一个二维的几何模型。 </summary>
        private class SlopeSegsGeomBackup
        {
            #region ---   Fields

            private readonly double _backStation;
            /// <summary> 其值可能为null，表示没有此边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有） </summary>
            private readonly SlopeData _backSlope;
            private readonly double _frontStation;
            /// <summary> 其值可能为null，表示没有此边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有） </summary>
            private readonly SlopeData _frontSlope;


            /// <summary> 前面两个边坡之间的桩号距离 </summary>
            private readonly double _mLength;

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
            private double _maxBackSlopeIndex;

            /// <summary> 前面边坡的坡顶或坡底所对应的Index，当没有任何对应子边坡时，其值为 0  </summary>
            private double _maxFrontSlopeIndex;

            private Line2d _开口线;

            #endregion

            /// <summary> 在两个相邻的边坡之间计算其每一个子边坡所占的宽度与几何面积 </summary>
            /// <param name="backStation"></param>
            /// <param name="backSlope">其值可能为null，表示没有此边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）</param>
            /// <param name="frontStation">其值可能为null，表示没有此边坡对象（是几何图形都没有，而不是因为挡墙覆盖而没有）</param>
            /// <param name="frontSlope"></param>
            public SlopeSegsGeomBackup(double backStation, SlopeData backSlope, double frontStation, SlopeData frontSlope)
            {
                _backStation = backStation;
                _backSlope = backSlope;

                _frontStation = frontStation;
                _frontSlope = frontSlope;

                //
                _mLength = Math.Abs(backStation - frontStation);
                ConstructSlopeSystem();
            }

            /// <summary> 根据前后边坡对象构造二维边坡系统 </summary>
            private void ConstructSlopeSystem()
            {
                double innerLength = 0.0; // 斜坡长度
                double outterLength = 0.0; // 斜坡长度
                // 后方边坡
                var sign = _backSlope.FillCut ? -1 : 1;
                _backSlopePoints = new Dictionary<double, Point2d[]>();
                var segs = SlopeData.Combine(_backSlope.Slopes, _backSlope.Platforms, true);
                foreach (var bs in segs)
                {
                    if (bs.Type == SlopeSegType.边坡)
                    {
                        outterLength = innerLength + bs.Length;
                        _backSlopePoints.Add(bs.Index,
                            new Point2d[]
                            {new Point2d(_backStation, sign*innerLength), new Point2d(_backStation, sign*outterLength)});
                        innerLength = outterLength;
                    }
                    else
                    {
                        // Point2d[] 数组中两个值是相同的
                        _backSlopePoints.Add(-bs.Index,
                            new Point2d[]
                            {new Point2d(_backStation, sign*innerLength), new Point2d(_backStation, sign*innerLength)});
                    }
                }
                _maxBackSlopeIndex = _backSlope.Slopes.Count > 0 ? _backSlope.Slopes.Last().Index : 0;
                _backEdge = _maxBackSlopeIndex > 0
                    ? _backSlopePoints[_maxBackSlopeIndex][1]
                    : new Point2d(_backStation, 0);

                // 前方边坡
                sign = _frontSlope.FillCut ? -1 : 1;
                innerLength = 0.0;
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
                                new Point2d(_frontStation, sign*innerLength),
                                new Point2d(_frontStation, sign*outterLength)
                            });
                        innerLength = outterLength;
                    }
                    else
                    {
                        // Point2d[] 数组中两个值是相同的
                        _frontSlopePoints.Add(-bs.Index,
                            new Point2d[]
                            {new Point2d(_frontStation, sign*innerLength), new Point2d(_frontStation, sign*innerLength)});
                    }
                }
                _maxFrontSlopeIndex = _frontSlope.Slopes.Count > 0 ? _frontSlope.Slopes.Last().Index : 0;

                _frontEdge = _maxFrontSlopeIndex > 0
                    ? _frontSlopePoints[_maxFrontSlopeIndex][1]
                    : new Point2d(_frontStation, 0);

                // 开口线
                _开口线 = new Line2d(_backEdge, _frontEdge);
            }

            /// <summary> 后方平台向前走的长度与对应占据的几何面积 </summary>
            public void GetBackPlatformLengthAndArea(Platform backPltfm, out double length, out double area)
            {
                if (_backSlope.FillCut != _frontSlope.FillCut)
                {
                    // 填挖交界
                    var pt = _backSlopePoints[-backPltfm.Index][0];
                    var inters = _开口线.IntersectWith(new Line2d(pt, new Vector2d(1, 0)));
                    if (inters != null)
                    {
                        length = Math.Abs(pt.X - inters.First().X);
                    }
                    else
                    {
                        length = _mLength;
                    }
                    area = length * backPltfm.ProtectionLength;
                }
                else
                {
                    var pfront = _frontSlope.Platforms.FirstOrDefault(r => r.Index == backPltfm.Index);
                    if (pfront != null)
                    {
                        // 说明有对应的平台
                        length = _mLength / 2;
                        // 后边坡向前的平台面积：前后边坡的对应平台所构成的梯形（一般为矩形）被中线截开后的面积
                        area = (3 * backPltfm.ProtectionLength + 2 * pfront.ProtectionLength) * length / 4;
                    }
                    else
                    {
                        // 说明无对应的平台
                        // 先求水平平台与开口线的交点，再取最小水平长度
                        var pt = _backSlopePoints[-backPltfm.Index][1];
                        var inters = _开口线.IntersectWith(new Line2d(pt, new Vector2d(1, 0)));
                        if (inters != null)
                        {
                            var intersLength = Math.Abs(pt.X - inters.First().X);
                            length = Math.Min(intersLength, _mLength);
                        }
                        else
                        {
                            // 说明开口线是水平的，可认为边坡顶部即为水平平台。
                            length = _mLength;
                        }
                        area = backPltfm.ProtectionLength * length;
                    }
                }
            }

            /// <summary> 后方边坡向前走的长度与对应占据的几何面积 </summary>
            public void GetBackSlopeLengthAndArea(Slope backSlope, out double length, out double area)
            {
                if (_backSlope.FillCut != _frontSlope.FillCut)
                {
                    // 填挖交界
                    var pts = _backSlopePoints[backSlope.Index];
                    var inters0 = _开口线.IntersectWith(new Line2d(pts[0], new Vector2d(1, 0)));
                    var inters1 = _开口线.IntersectWith(new Line2d(pts[1], new Vector2d(1, 0)));
                    //
                    var width0 = inters0 != null ? Math.Abs(inters0.First().X - pts[0].X) : _mLength;
                    var width1 = inters1 != null ? Math.Abs(inters1.First().X - pts[1].X) : _mLength;
                    width0 = Math.Max(width0, _mLength);
                    width1 = Math.Max(width1, _mLength);

                    //
                    length = Math.Max(width0, width1);
                    area = (width0 + width1) * backSlope.ProtectionLength / 2;
                }
                else // 前后边坡均为填方或挖方
                {
                    // 下面进行匹配时将 double 类型的Index进行(int)取整操作，是考虑到某一级边坡中可能有多个子边坡的情况
                    var slpFront = _frontSlope.Slopes.FirstOrDefault(r => (int)r.Index == (int)backSlope.Index);

                    if (slpFront != null) // 说明前方边坡中有对应的子边坡
                    {
                        // 说明前方边坡中有对应的子边坡
                        length = _mLength / 2;

                        // 如果此子边坡不是最外侧的子边坡，则其所对的几何区域为矩形；而如果其是最外侧的子边坡，则其所对的几何区域为梯形或五边形
                        if (backSlope.Index < _maxBackSlopeIndex)
                        {
                            // 说明此子边坡不是最外侧的子边坡，则其所对的几何区域为矩形；
                            area = length * backSlope.ProtectionLength;
                        }
                        else // 说明此子边坡是最外侧的子边坡，则其所对的几何区域为梯形或五边形
                        {
                            // 前方边坡在比后方边坡高一级的子边坡底
                            if ((int)_maxFrontSlopeIndex > (int)backSlope.Index)
                            {
                                // ----------------------------------------------------------
                                // 前方边坡的最外侧子边坡比此子边坡为高一个大级别，即此子边坡之上还有前方边坡的一个平台
                                // 此时此子边坡所对的几何区域为五边形
                                // ----------------------------------------------------------

                                var frontOutter =
                                    _frontSlopePoints.Keys.FirstOrDefault(r => (int)r > (int)backSlope.Index);
                                var frontOutterPt = _frontSlopePoints[frontOutter][0]; // 子边坡底部点
                                var backBottomPt = _backSlopePoints[backSlope.Index][0];
                                var middleBottomPt = new Point2d((_backStation + _frontStation) / 2, backBottomPt.Y);
                                var middleTopPt = new Point2d(middleBottomPt.X, frontOutterPt.Y);
                                var inters = _开口线.IntersectWith(new Line2d(middleTopPt, new Vector2d(1, 0)));
                                //
                                var topWidth = inters != null
                                    ? Math.Abs(inters.First().X - middleBottomPt.X)
                                    : length;
                                topWidth = Math.Max(topWidth, length);

                                var middleHeight = Math.Abs(middleTopPt.Y - middleBottomPt.Y);
                                var backHeight = backSlope.ProtectionLength;
                                // 用矩形减三角形，以求五边形的面积
                                var areaRec = Math.Max(length, topWidth) * Math.Max(middleHeight, backHeight);
                                var areaTri = Math.Abs(length - topWidth) * Math.Abs(middleHeight - backHeight) / 2;
                                area = areaRec - areaTri;
                            }
                            else
                            {
                                // 前方边坡的最外侧子边坡与此子边坡为同一级别（均为最外侧边坡），此时此子边坡所对的几何区域为梯形
                                var backBottomPt = _backSlopePoints[backSlope.Index][0];
                                var middleBottomPt = new Point2d((_backStation + _frontStation) / 2, backBottomPt.Y);
                                var inters1 = _开口线.IntersectWith(new Line2d(middleBottomPt, new Vector2d(0, 1)));
                                var backHeight = Math.Abs(_backEdge.Y - backBottomPt.Y);
                                var middleHeight = inters1 != null
                                    ? Math.Abs(inters1.First().Y - middleBottomPt.Y)
                                    : 0;
                                area = (backHeight + middleHeight) * length / 2;
                            }
                        }
                    }
                    else
                    {
                        // 说明无对应的边坡，此时后边坡所占的几何区域为三角形或者梯形

                        // 离此边坡最近的靠近内侧的子边坡对象的顶点坐标
                        var outerPt = _backSlopePoints[backSlope.Index][1];
                        var innerPt = _backSlopePoints[backSlope.Index][0];
                        var innerInters = _开口线.IntersectWith(new Line2d(innerPt, new Vector2d(1, 0)));
                        var innerLength = (innerInters != null)
                            ? Math.Abs(innerPt.X - innerInters.First().X)
                            : _mLength;
                        innerLength = Math.Max(innerLength, _mLength);
                        if (backSlope.Index < _maxBackSlopeIndex)
                        {
                            // 说明此子边坡靠外还有子边坡，此时对应的几何区域为一个梯形

                            // 求梯形靠内与靠外侧点与开口线的交点
                            var outerInters = _开口线.IntersectWith(new Line2d(outerPt, new Vector2d(1, 0)));
                            var outerLength = (outerInters != null)
                                ? Math.Abs(outerPt.X - outerInters.First().X)
                                : _mLength;
                            outerLength = Math.Max(outerLength, _mLength);
                            //
                            length = Math.Max(innerLength, outerLength);
                            area = (innerLength + outerLength) * backSlope.ProtectionLength / 2;
                        }
                        else
                        {
                            // 说明此子边坡靠外没有子边坡，此时的子边坡为整个边坡最外侧的子边坡，即对应的几何区域为一个三角形
                            length = innerLength;
                            area = (innerLength + 0) * backSlope.ProtectionLength / 2;
                        }
                    }
                }
            }
        }

    }
}
