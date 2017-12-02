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

        /// <summary> 某一区间的纵向挖台阶处理部分 </summary>
        public class LongitudinalStairExcav : StationRangeEntity, IMergeable
        {
            /// <summary> 纵向挖台阶的多个三角形面积 </summary>
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

            public override string ToString()
            {
                return base.ToString() + $",{StairArea.ToString("0.0")}";
            }
        }

        #endregion

        #region --- Fields

        private readonly DocumentModifier _docMdf;

        /// <summary> 由道路中桩设计线与其对应的自然地面线所组成的纵断面图 </summary>
        private readonly LongitudinalSection _longitudinalSection;

        private static readonly Criterion_StairExcavLong _criterion = Criterion_StairExcavLong.UniqueInstance;

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
            List<LongitudinalStairExcav> stairsInOneFillZone;
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
                    stairsInOneFillZone = CalculateFillRange(_longitudinalSection.RoadCurve2d.StartPoint.X,
                        _longitudinalSection.RoadCurve2d.EndPoint.X);
                    stairs = stairsInOneFillZone;
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
                        stairsInOneFillZone = CalculateFillRange(lastIntersX, intersX);
                        if (stairsInOneFillZone != null)
                        {
                            stairs.AddRange(stairsInOneFillZone);
                        }
                    }
                    lastIntersX = intersX;

                    //
                }
                // 2. [最后一个交点 ~ 道路终点]之间的区段
                if (!fillToCut)
                {
                    stairsInOneFillZone = CalculateFillRange(lastIntersX, _longitudinalSection.RoadCurve2d.EndPoint.X);
                    if (stairsInOneFillZone != null)
                    {
                        stairs.AddRange(stairsInOneFillZone);
                    }
                }
            }

            // 排除桥梁等区间阻隔：删除所有与桥隧相交的台阶
            var structs = Options_Collections.RangeBlocks;
            stairs = stairs.Where(st => !st.IntersectStructureBlocks(structs)).ToList();

            // 区间合并
            stairs = MergeLinkedStairs(stairs);
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
        /// 计算一段填方区间（填方段的两个边界交点之间的区间）的挖台阶量
        /// </summary>
        /// <param name="startStation"></param>
        /// <param name="endStation"></param>
        private List<LongitudinalStairExcav> CalculateFillRange(double startStation, double endStation)
        {
            if (endStation - startStation < _criterion.最小区间宽度)
            {
                return null;
            }

            // 求分段交点（集合中至少有两个值）
            var xy = new List<double[]>();
            for (double x = startStation; x < endStation; x += _criterion.台阶宽度)
            {
                var intersVerticalGround = new CurveCurveIntersector2d(_longitudinalSection.GroundCurve2d,
                    new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                xy.Add(new double[] { x, intersVerticalGround.GetIntersectionPoint(0).Y });
            }

            var stairsInFillZone = new List<LongitudinalStairExcav>();

            // 计算此区间内所有坡度较陡的台阶
            for (int i = 0; i < xy.Count - 1; i++)
            {
                var ratio = Math.Abs((xy[i + 1][1] - xy[i][1]) / ((xy[i + 1][0] - xy[i][0])));
                if (ratio > _criterion.临界纵坡)
                {
                    // 挖台阶的三角形面积
                    var area = Math.Abs((xy[i + 1][0] - xy[i][0]) * (xy[i + 1][1] - xy[i][1])) / 2;
                    stairsInFillZone.Add(new LongitudinalStairExcav(xy[i][0], xy[i + 1][0], stairArea: area));
                }
            }

            // 这里为了避免区间太密，作出一个强制处理：对同一个填方桩号区间内的所有满足条件的台阶区间进行合并，保证合并后的每一个区间的长度不小于设定的最小宽度
            // 但是处理之后，总的挖台阶面积还是此区间内有效的台阶的三角形面积之和
            int count = stairsInFillZone.Count;
            if (count == 0 ||
                stairsInFillZone[count - 1].EndStation - stairsInFillZone[0].StartStation < _criterion.最小区间宽度)
            {
                // 表示此填方区间内，所有的台阶（算上分离的）之间的距离都小于 10 m，此时认为不作挖台阶处理
                return null;
            }
            else
            {
                var stairsInFillZone1 = new List<LongitudinalStairExcav>();
                // 将一些分离的台阶进行合并，确保其距离不小于10m；但是处理之后，总的挖台阶面积还是此区间内有效的台阶的三角形面积之和
                // 1. 将直接相连的台阶进行合并
                var lastStair = stairsInFillZone[0];
                LongitudinalStairExcav thisStair;
                stairsInFillZone1.Add(lastStair);
                for (int i = 1; i < count; i++)
                {
                    thisStair = stairsInFillZone[i];
                    if (lastStair.IsMergeable(thisStair))
                    {
                        lastStair.Merge(thisStair);
                    }
                    else
                    {
                        lastStair = thisStair;
                        stairsInFillZone1.Add(thisStair);
                    }
                }
                // 合并后，整个填方区间内，剩下相互分离的多个台阶子区间
                // 2. 对于初步合并后自身宽度不足10m，左右10m内又没有其他台阶的项，直接删除
                count = stairsInFillZone1.Count;
                if (count == 1)
                {
                    // 如果合并后只剩一段，则直接返回
                    return stairsInFillZone1;
                }
                else
                {
                    // 从小桩号往大桩号前进式合并
                    stairsInFillZone = new List<LongitudinalStairExcav>();
                    LongitudinalStairExcav validatedStair;
                    LongitudinalStairExcav backStair = stairsInFillZone1[0];
                    LongitudinalStairExcav frontStair = stairsInFillZone1[1];
                    // 整个填方段起码有三段分离的台阶子区间
                    for (int i = 0; i < count - 1; i++)
                    {
                        frontStair = stairsInFillZone1[i + 1];
                        validatedStair = MergeSeperatedStair(backStair, ref frontStair);
                        if (validatedStair != null)
                        {
                            stairsInFillZone.Add(validatedStair);
                        }
                        backStair = frontStair;
                    }
                    // 最后一个子区间
                    if (frontStair.GetLength() >= _criterion.最小区间宽度)
                    {
                        stairsInFillZone.Add(frontStair);
                    }
                    return stairsInFillZone;
                }
            }
        }

        /// <summary>
        /// 将两个序号相邻，但几何相离的两个台阶子区间进行合并
        /// </summary>
        /// <param name="backStair">桩号较小</param>
        /// <param name="frontStair">桩号较大</param>
        /// <returns></returns>
        /// <remarks>计算完成后，<param name="frontStair">成为下一阶段的<param name="backStair"></param></param></remarks>
        private LongitudinalStairExcav MergeSeperatedStair(LongitudinalStairExcav backStair,
            ref LongitudinalStairExcav frontStair)
        {
            var dist = frontStair.StartStation - backStair.EndStation;
            if (dist < _criterion.最小区间宽度)
            {
                // 合并两个区间
                frontStair = new LongitudinalStairExcav(backStair.StartStation, frontStair.EndStation,
                    backStair.StairArea + frontStair.StairArea);
                return null;
            }
            else
            {
                // 如果后面台阶宽度小于10m，则删除，否则作为一个单独的区间提取出来
                if (backStair.GetLength() < _criterion.最小区间宽度)
                {
                    return null;
                }
                else
                {
                    // 说明此部分工程量不能忽略
                    return backStair;
                }
            }
        }

        /// <summary> 将多个断面区间进行合并 </summary>
        /// <param name="stairs">路基某一侧的高填或深挖边坡对象</param>
        /// <returns></returns>
        private List<LongitudinalStairExcav> MergeLinkedStairs(List<LongitudinalStairExcav> stairs)
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