using System;

namespace eZcad.SubgradeQuantities.Entities
{
    /// <summary> 整条道路中所有的横断面 </summary>
    public class AllStations
    {
        /// <summary> 整条道路中所有的横断面的桩号，且桩号从小到大排列 </summary>
        public double[] Stations { get; }

        /// <summary> 构造函数 </summary>
        /// <param name="allStations">整条道路中所有的横断面的桩号 </param>
        public AllStations(double[] allStations)
        {
            Stations = allStations;
            Array.Sort(Stations);
        }

        /// <summary> 搜索所有桩号集合中最接近指定桩号且大于等于指定桩号的值 </summary>
        /// <returns>若没有匹配值，则返回 null</returns>
        public double? MatchFront(double wantedStation)
        {
            foreach (var s in Stations)
            {
                if (s >= wantedStation)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary> 搜索所有桩号集合中最接近指定桩号且小于等于指定桩号的值 </summary>
        /// <returns>若没有匹配值，则返回 null</returns>
        public double? MatchBack(double wantedStation)
        {
            for (int i = Stations.Length - 1; i >= 0; i--)
            {
                if (Stations[i] <= wantedStation)
                {
                    return Stations[i];
                }
            }
            return null;
        }

        /// <summary> 搜索所有桩号集合中与指定桩号最接近的值，其值可能比指定值小，也可能比指定值大 </summary>
        /// <returns>若没有匹配值，则返回 null</returns>
        public double MatchClosest(double wantedStation)
        {
            var closedStation = Stations[0];
            var minDis = double.MaxValue;
            foreach (var s in Stations)
            {
                var dist = Math.Abs(s - wantedStation);
                if (dist <= minDis)
                {
                    minDis = dist;
                    closedStation = s;
                }
            }
            return closedStation;
        }
    }
}