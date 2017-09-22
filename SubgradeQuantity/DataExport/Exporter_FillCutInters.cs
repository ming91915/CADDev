using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 纵向填挖交界 </summary>
    public class Exporter_FillCutInters : DataExporter
    {
        #region --- Types

        private class FillCutIntersects : HalfValue
        {
            /// <summary> this is a fill zone or cut zone </summary>
            public bool FillCut { get; set; }

            /// <summary> it is before or after the intersection point </summary>
            public bool BackFront { get; set; }

            public bool HasReinforcement { get; set; }
            public int ReinforcementLayers { get; set; }
            public double TreatedArea { get; set; }
            //

            public override void Merge(IMergeable connectedHalf)
            {
                var conn = (FillCutIntersects)connectedHalf;
                //    var dist1 = Math.Abs(ParentStation - EdgeStation);
                //    var dist2 = Math.Abs(conn.EdgeStation - conn.ParentStation);
                //    TreatedArea = (conn.TreatedArea * dist2 + TreatedArea * dist1) / (dist1 + dist2);
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                var front = next as FillCutIntersects;
                // 处理位置不同的区间要分开
                // if (StairCutSide != front.StairCutSide) return false;
                return true;
            }

            public override void CutByBlock(double blockStation)
            {
                // 啥也不用做
            }
        }

        #endregion

        #region --- Fields
        private static readonly Criterion_FillCutIntersect _criterion = Criterion_FillCutIntersect.UniqueInstance;

        private readonly DocumentModifier _docMdf;
        private readonly LongitudinalSection _longitudinalSection;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="longitudinalSection"></param>
        public Exporter_FillCutInters(DocumentModifier docMdf, LongitudinalSection longitudinalSection)
            : base(docMdf, longitudinalSection.IntersPoints.Keys.ToArray())
        {
            _docMdf = docMdf;
            _longitudinalSection = longitudinalSection;
            //
        }

        /// <summary> 纵向填挖交界 </summary>
        public void ExportFillCutInters()
        {
            var sortedRanges = ConstructIntersectRange(_longitudinalSection);
            return;

            var inters = _longitudinalSection.Intersects;
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
                var fillToCut = _longitudinalSection.FilltoCut(ptRoad, ptGround);
                var arrDx = fillToCut ? arrFillToCut : arrCutToFill;
                var intersX = ptRoad.Point.X;
                var maxVerticalDiff = 0.0;
                // 填挖交界处的路基，在填方段10m范围内高度H＜5m时，按断面A实施，H＞5m时，按断面B实施。
                foreach (var dx in arrDx)
                {
                    var x = intersX + dx;
                    var intersVerticalRoad = new CurveCurveIntersector2d(_longitudinalSection.RoadCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    var intersVerticalGround = new CurveCurveIntersector2d(_longitudinalSection.GroundCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                        intersVerticalGround.NumberOfIntersectionPoints == 0)
                    {
                        break;
                    }
                    else
                    {
                        var verticalDiff =
                            Math.Abs(intersVerticalGround.GetIntersectionPoint(0).Y -
                                     intersVerticalRoad.GetIntersectionPoint(0).Y);
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

        /// <summary> 初始化每一个填挖交界点所占据的几何区间 </summary>
        /// <param name="longitudinalSection"></param>
        /// <returns></returns>
        private SortedDictionary<double, CrossSectionRange<FillCutIntersects>>
            ConstructIntersectRange(LongitudinalSection longitudinalSection)
        {
            var allIntersectsSections = new SortedDictionary<double, CrossSectionRange<FillCutIntersects>>();
            FillCutIntersects backValue;
            FillCutIntersects frontValue;
            //

            var inters = longitudinalSection.Intersects;
            if (inters.NumberOfIntersectionPoints == 0)
            {
                // 整个路段都是填方或者挖方
                _docMdf.WriteNow("没有填挖交界交点");
                return null;
            }
            //
            // 对填挖交界点进行处理
            _docMdf.WriteLineIntoDebuger("填挖交界点的坐标：");
            _docMdf.WriteLineIntoDebuger("交界点坐标", "交界方式", "10m填方段最大高度", "10m挖方段最大高度", "处理方式");

            int interval = 2;
            var fillLargerThan = 5.0;
            int fillCheckLength = 10;
            var arrCutToFill = ArrayConstructor.FromRangeAri(0, fillCheckLength, interval);
            var arrFillToCut = ArrayConstructor.FromRangeAri(0, -fillCheckLength, -interval);
            var blocks = Options_Collections.RangeBlocks;
            for (int i = 0; i < inters.NumberOfIntersectionPoints; i++)
            {
                var ptRoad = inters.GetPointOnCurve1(i);
                var ptGround = inters.GetPointOnCurve2(i);

                // 排除桥梁等结构区域
                if (blocks.Any(r => r.ContainsStation(ptRoad.Point.X)))
                {
                    continue;
                }
                //
                var fillToCut = longitudinalSection.FilltoCut(ptRoad, ptGround);
                var arrDx = fillToCut ? arrFillToCut : arrCutToFill;
                var intersX = ptRoad.Point.X;

                // 填挖交界处的路基，在填方段10m范围内高度H＜5m时，按断面A实施，H＞5m时，按断面B实施。
                var maxVerticalDiff_Fill = 0.0;
                foreach (var dx in arrDx)
                {
                    var x = intersX + dx;
                    var intersVerticalRoad = new CurveCurveIntersector2d(longitudinalSection.RoadCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    var intersVerticalGround = new CurveCurveIntersector2d(longitudinalSection.GroundCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                        intersVerticalGround.NumberOfIntersectionPoints == 0)
                    {
                        break;
                    }
                    else
                    {
                        var verticalDiff = intersVerticalRoad.GetIntersectionPoint(0).Y -
                                           intersVerticalGround.GetIntersectionPoint(0).Y;
                        if (verticalDiff > maxVerticalDiff_Fill)
                        {
                            maxVerticalDiff_Fill = verticalDiff;
                        }
                    }
                }

                var maxVerticalDiff_Cut = 0.0;
                foreach (var dx in arrDx)
                {
                    var x = intersX - dx;
                    var intersVerticalRoad = new CurveCurveIntersector2d(longitudinalSection.RoadCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    var intersVerticalGround = new CurveCurveIntersector2d(longitudinalSection.GroundCurve2d,
                        new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                    if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                        intersVerticalGround.NumberOfIntersectionPoints == 0)
                    {
                        break;
                    }
                    else
                    {
                        var verticalDiff = intersVerticalGround.GetIntersectionPoint(0).Y - 
                                            intersVerticalRoad.GetIntersectionPoint(0).Y;

                        if (verticalDiff > maxVerticalDiff_Cut)
                        {
                            maxVerticalDiff_Cut = verticalDiff;
                        }
                    }
                }

                string fill = fillToCut ? "填 - 挖" : "挖 - 填";
                var reinforce = (maxVerticalDiff_Fill > fillLargerThan) ? "超挖换填 + 土工格栅" : "超挖换填";
                _docMdf.WriteLineIntoDebuger(ptRoad.Point.X, fill, maxVerticalDiff_Fill, maxVerticalDiff_Cut, reinforce);
                //
            }

            // 断面的判断与计算
            _docMdf.WriteNow($"填挖交界交点数量：{inters.NumberOfIntersectionPoints}");
            _docMdf.WriteNow("交界点桩号", "交点形式", "处理措施");

            var allIntersects = longitudinalSection.IntersPoints.Keys.ToArray();
            var count = allIntersects.Length;
            double lastStation = allIntersects[0];
            for (int i = 0; i < count - 1; i++)
            {
                var nextStation = (allIntersects[i] + allIntersects[i + 1]) / 2;
                backValue = new FillCutIntersects();
                backValue.SetParentStation(allIntersects[i]);
                backValue.EdgeStation = lastStation;
                //
                frontValue = new FillCutIntersects();
                frontValue.SetParentStation(allIntersects[i]);
                frontValue.EdgeStation = nextStation;
                //
                var s1 = new CrossSectionRange<FillCutIntersects>(allIntersects[i], backValue, frontValue);
                allIntersectsSections.Add(allIntersects[i], s1);
                lastStation = nextStation;
            }
            // 最后一个区间
            backValue = new FillCutIntersects();
            backValue.SetParentStation(allIntersects[count - 1]);
            backValue.EdgeStation = lastStation;
            //
            frontValue = new FillCutIntersects();
            frontValue.SetParentStation(allIntersects[count - 1]);
            frontValue.EdgeStation = allIntersects[count - 1];
            var s2 = new CrossSectionRange<FillCutIntersects>(allIntersects[count - 1], backValue, frontValue);
            allIntersectsSections.Add(allIntersects[count - 1], s2);
            //
            return allIntersectsSections;
        }

        private void CheckLinkedIntersectPoints()
        {

        }
    }
}