using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eZcad.SubgradeQuantity.Entities
{

    public enum StructureType : short
    {
        桥梁 = 1,
        隧道 = 2,
    }

    /// <summary>
    /// 整个道路中的非路基段，比如桥梁或者隧道
    /// </summary>
    public class Structure : StationRangeEntity,ICloneable
    {
        /// <summary> 离此结构最近的后方（较小桩号）的横断面桩号 </summary>
        public double ConnectedBackStaion { get; set; }
        /// <summary> 离此结构最近的前方（较大桩号）的横断面桩号 </summary>
        public double ConnectedFrontStaion { get; set; }

        public StructureType Type { get; set; }

        public Structure(StructureType type, double startStation, double endStation) : base(startStation, endStation)
        {
            Type = type;
            //
            ConnectedBackStaion = startStation;
            ConnectedFrontStaion = endStation;
        }

        public object Clone()
        {
            return MemberwiseClone() as Structure;
        }

        /// <summary> 根据道路中所有的横断面桩号，来确定离结构物最近的横断面桩号 </summary>
        /// <param name="allSortedStations">道路中所有的横断面桩号，小桩号位于集合的前面 </param>
        public void CalculateConnetedStations(double[] allSortedStations)
        {
            // 前面的桩号
            var count = allSortedStations.Length;
            if (StartStation <= allSortedStations[0])
            {
                ConnectedBackStaion = allSortedStations[0];
            }
            else
            {
                for (int i = 1; i < count; i++)
                {
                    if (allSortedStations[i] >= StartStation)
                    {
                        ConnectedBackStaion = allSortedStations[i - 1];
                    }
                }
            }
            // 后面的桩号
            if (EndStation >= allSortedStations[count - 1])
            {
                ConnectedFrontStaion = allSortedStations[count - 1];
            }
            else
            {
                for (int i = count - 2; i >= 0; i--)
                {
                    if (allSortedStations[i] <= StartStation)
                    {
                        ConnectedFrontStaion = allSortedStations[i + 1];
                    }
                }
            }


        }

    }
}
