using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eZcad.SubgradeQuantity.Entities
{

    /// <summary> 可以进行合并的对象 </summary>
    public interface IMergeable
    {
        /// <summary> 可以进行合并的对象 </summary>
        void Merge(IMergeable next);

        /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
        /// <param name="next">与本区间紧密相连的下一个区间</param>
        bool IsMergeable(IMergeable next);
    }

    /// <summary> 与桩号区间相关的数据 </summary>
    public class StationRangeEntity
    {
        /// <summary> 区间的起始桩号（较小） </summary>
        public double StartStation { get; set; }
        /// <summary> 区间的末尾桩号（较大） </summary>
        public double EndStation { get; set; }

        /// <summary> 区间长度，因为区间范围的起止桩号可以动态修改，所以其区间长度不能作为一个常数返回 </summary>
        public double GetLength()
        {
            return EndStation - StartStation;
        }

        public StationRangeEntity(double startStation, double endStation)
        {
            StartStation = startStation;
            EndStation = endStation;
        }

        /// <summary> 此区间与某一个桥梁隧道等结构物相交 </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public bool IntersectStructureBlocks(IEnumerable<StationRangeEntity> blocks)
        {
            foreach (var b in blocks)
            {
                if (!(StartStation >= b.EndStation || EndStation <= b.StartStation))
                {
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"{StartStation.ToString("0.0")}~{EndStation.ToString("0.0")}";
        }
    }
}
