using System;
using System.Collections.Generic;
using System.Linq;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 边坡防护长度数据所对应的类型 </summary>
    public enum StationInfoType
    {
        /// <summary> 用户在界面中测量出来的 </summary>
        Measured,
        /// <summary> 横断面上的标识数据，其对应的防护长度均为 0  </summary>
        Located,
        /// <summary> 后期插值计算出来的 </summary>
        Interpolated,
    }

    /// <summary> 每一个桩号所对应的某种工程量数据 </summary>
    public class StationInfo<T>
    {
        public double Station { get; private set; }
        public StationInfoType Type { get; set; }
        public T Value { get; set; }

        public StationInfo(double station, StationInfoType type, T spLength)
        {
            Station = station;
            Type = type;
            Value = spLength;
        }
        /// <summary> 用新的数据替换对象中的原数据 </summary>
        public void Override(StationInfo<T> newSection)
        {
            Station = newSection.Station;
            Type = newSection.Type;
            Value = newSection.Value;
        }

        /// <summary>
        /// 将 边坡横断面集合转换为二维数组，以用来写入 Excel
        /// </summary>
        /// <param name="slopes"></param>
        /// <returns></returns>
        public static object[,] ConvertToArr(IList<StationInfo<T>> slopes)
        {
            var res = new object[slopes.Count(), 3];
            var keys = TypeMapping.Keys.ToArray();
            var values = TypeMapping.Values.ToArray();

            var r = 0;
            foreach (var slp in slopes)
            {
                res[r, 0] = slp.Station;
                res[r, 1] = keys[Array.IndexOf(values, slp.Type)];
                res[r, 2] = slp.Value;
                r += 1;
            }
            return res;
        }

        public static Dictionary<string, StationInfoType> TypeMapping = new Dictionary<string, StationInfoType>
        {
            {"定位", StationInfoType.Located},
            {"测量", StationInfoType.Measured},
            {"插值", StationInfoType.Interpolated},
        };

        public override string ToString()
        {
            return $"{Station},\t{Type},\t{Value}";
        }
    }
    

}
