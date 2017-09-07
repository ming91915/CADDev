using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Entities;

[assembly: CommandClass(typeof(SoilRockRange))]

namespace eZcad.SubgradeQuantity.Entities
{
    public enum SubgradeType : short
    {
        岩质 = 0,
        土质 = 1,
    }

    /// <summary> 记录道路中岩质边坡与土质边坡的分类与所属区间 </summary>
    public class SoilRockRange : StationRangeEntity, ICloneable
    {
        public Distribution SideDistribution { get; set; }
        public SubgradeType Type { get; set; }

        public enum Distribution : short
        {
            /// <summary> 边坡属性同时适用于左、右侧边坡 </summary>
            左右两侧 = 0,

            /// <summary> 边坡属性只适用于左侧边坡 </summary>
            左侧 = 1,

            /// <summary> 边坡属性只适用于右侧边坡 </summary>
            右侧 = 2,
        }

        public SoilRockRange(double startStation, double endStation, Distribution distribution, SubgradeType type) : base(startStation, endStation)
        {
            SideDistribution = distribution;
            Type = type;
        }

        public object Clone()
        {
            return MemberwiseClone() as SoilRockRange;
        }

        #region --- SetSlopeSoilRock

        /// <summary> 根据项目选项中设置的土质边坡与岩质边坡的分区，为指定的边坡设置对应的土质类型 </summary>
        /// <param name="allSoilRockRanges"></param>
        /// <param name="slopes">要进行设置的边坡</param>
        public static void SetSlopeSoilRock(List<SoilRockRange> allSoilRockRanges, params SlopeData[] slopes)
        {
            foreach (var slpData in slopes)
            {
                var m =
                    allSoilRockRanges.FirstOrDefault(
                        r =>
                            (slpData.Station >= r.StartStation && slpData.Station <= r.EndStation) &&
                            MatchSide(slpData, r.SideDistribution));
                if (m != null)
                {
                    slpData.SoilOrRock = m.Type;
                }
                else
                {
                    slpData.SoilOrRock = SubgradeType.岩质;
                }
            }
        }

        private static bool MatchSide(SlopeData slopeData, Distribution distr)
        {
            switch (distr)
            {
                case Distribution.左侧:
                    return slopeData.OnLeft;
                case Distribution.右侧:
                    return !slopeData.OnLeft;
                default:
                    return true;
            }
        }

        #endregion
    }
}