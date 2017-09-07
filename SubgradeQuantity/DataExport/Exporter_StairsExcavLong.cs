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
    public class Exporter_StairsExcavLong
    {
        #region --- Types

        /// <summary> 纵向挖台阶 </summary>
        public class LongitudinalStairExcav : StationRangeEntity, IMergeable
        {
            /// <summary> 纵向挖台阶面积 </summary>
            public double StairArea { get; set; }

            public LongitudinalStairExcav(double startStation, double endStation, double stairArea)
                : base(startStation, endStation)
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
        /// <summary> 由道路中桩设计线与其对应的自然地面线所组成的纵断面图 </summary>
        private readonly LongitudinalSection _longitudinalSection;
        //private readonly Polyline _road;
        //private readonly Polyline _ground;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="longitudinalSection"></param>
        public Exporter_StairsExcavLong(DocumentModifier docMdf, LongitudinalSection longitudinalSection)
        {
            _docMdf = docMdf;
            _longitudinalSection = longitudinalSection;
        }

        /// <summary> 纵向挖台阶处理 </summary>
        public void ExportLongitudinalStairExcav()
        {
            var stairs = new List<LongitudinalStairExcav>();
            var inters = _longitudinalSection.Intersects;
            if (inters.NumberOfIntersectionPoints == 0)
            {
                var middleFillHeight =
                    _longitudinalSection.GetFillHeight((_longitudinalSection.RoadCurve2d.StartPoint.X +
                                                        _longitudinalSection.RoadCurve2d.EndPoint.X) / 2);
                if (middleFillHeight < 0)
                {
                    _docMdf.WriteNow("整个路段都为挖方");
                    return;
                }
                else
                {
                    // 整个路段都为填方
                    CalculateFillRange(_longitudinalSection.RoadCurve2d.StartPoint.X,
                        _longitudinalSection.RoadCurve2d.EndPoint.X, stairs);
                }
            }
            else
            {
                // 对填挖交界点进行处理
                var lastIntersX = _longitudinalSection.RoadCurve2d.StartPoint.X;
                bool fillToCut = false;

                // 1. [道路起点 ~ 最后一个交点]之间的区段
                for (int i = 0; i < inters.NumberOfIntersectionPoints; i++)
                {
                    var ptRoad = inters.GetPointOnCurve1(i);
                    var ptGround = inters.GetPointOnCurve2(i);
                    var intersX = ptRoad.Point.X;

                    //
                    fillToCut = _longitudinalSection.FilltoCut(ptRoad, ptGround);
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
                    CalculateFillRange(lastIntersX, _longitudinalSection.RoadCurve2d.EndPoint.X, stairs);
                }
            }

            // 排除桥梁等区间阻隔：删除所有与桥隧相交的台阶
            var structs = Options_Collections.Structures;
            stairs = stairs.Where(st => !st.IntersectStructureBlocks(structs)).ToList();

            // 区间合并
            stairs = MergeLinkedRanges(stairs);
            _docMdf.WriteNow($"纵向挖台阶数量：{stairs.Count}");
            //

            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new object[] { "起点", "终点", "区间", "段落长度", "纵向挖台阶面积" };
            rows.Add(header);
            foreach (var stair in stairs)
            {
                rows.Add(new object[]
                {
                    stair.StartStation,
                    stair.EndStation,
                    ProtectionUtils.GetStationString(stair.StartStation, stair.EndStation, 0),
                    stair.EndStation - stair.StartStation,
                    stair.StairArea,
                });
            }

            var sheetArr = ArrayConstructor.FromList2D(listOfRows: rows);
            // sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1.5f, });

            // 输出到表格
            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.StairsExcavLong, "纵向挖台阶", sheetArr)
            };
            DataExporter.ExportWorkSheetDatas(sheet_Infos);
        }

        /// <summary>
        /// 计算填方区间的挖台阶量
        /// </summary>
        /// <param name="startStation"></param>
        /// <param name="endStation"></param>
        private void CalculateFillRange(double startStation, double endStation, List<LongitudinalStairExcav> stairs)
        {
            const int rangeInterval = 10; // 挖台阶量的最小分段宽度
            const int stairWidth = 2; // 每一个台阶的宽度。在纵断面中，当原地面纵坡大于12% 时，应按设计要求挖台阶，或设置坡度向内并大于4%、宽度大于2m的台阶。
            const double longitudinalRatio = 0.12;
            if (endStation - startStation < rangeInterval)
            {
                return;
            }
            // 求分段交点（集合中至少有两个值）
            var xy = new List<double[]>();
            for (double x = startStation; x < endStation; x += stairWidth)
            {
                var intersVerticalGround = new CurveCurveIntersector2d(_longitudinalSection.GroundCurve2d,
                    new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                xy.Add(new double[] { x, intersVerticalGround.GetIntersectionPoint(0).Y });
            }

            // 求斜率
            for (int i = 0; i < xy.Count - 1; i++)
            {
                var ratio = Math.Abs((xy[i + 1][1] - xy[i][1]) / ((xy[i + 1][0] - xy[i][0])));
                if (ratio > longitudinalRatio)
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

    }
}