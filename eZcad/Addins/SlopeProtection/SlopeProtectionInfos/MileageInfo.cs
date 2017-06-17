using System;
using System.Collections.Generic;
using System.Linq;

namespace eZcad.Addins.SlopeProtection.Entities
{
    /// <summary> 边坡防护长度数据所对应的类型 </summary>
    public enum MileageInfoType
    {
        /// <summary> 用户在界面中测量出来的 </summary>
        Measured,
        /// <summary> 横断面上的标识数据，其对应的防护长度均为 0  </summary>
        Located,
        /// <summary> 后期插值计算出来的 </summary>
        Interpolated,
    }

    /// <summary> 每一个桩号所对应的防护长度数据 </summary>
    public class MileageInfo
    {
        public double Mileage { get; private set; }
        public MileageInfoType Type { get; set; }
        public double SpLength { get; set; }

        public MileageInfo(double mileage, MileageInfoType type, double spLength)
        {
            Mileage = mileage;
            Type = type;
            SpLength = spLength;
        }
        /// <summary> 用新的数据替换对象中的原数据 </summary>
        public void Override(MileageInfo newSection)
        {
            Mileage = newSection.Mileage;
            Type = newSection.Type;
            SpLength = newSection.SpLength;
        }

        /// <summary>
        /// 将 边坡横断面集合转换为二维数组，以用来写入 Excel
        /// </summary>
        /// <param name="slopes"></param>
        /// <returns></returns>
        public static object[,] ConvertToArr(IList<MileageInfo> slopes)
        {
            var res = new object[slopes.Count(), 3];
            var keys = TypeMapping.Keys.ToArray();
            var values = TypeMapping.Values.ToArray();

            var r = 0;
            foreach (var slp in slopes)
            {
                res[r, 0] = slp.Mileage;
                res[r, 1] = keys[Array.IndexOf(values, slp.Type)];
                res[r, 2] = slp.SpLength;
                r += 1;
            }
            return res;
        }

        public static Dictionary<string, MileageInfoType> TypeMapping = new Dictionary<string, MileageInfoType>
        {
            {"定位", MileageInfoType.Located},
            {"测量", MileageInfoType.Measured},
            {"插值", MileageInfoType.Interpolated},
        };

        public override string ToString()
        {
            return $"{Mileage},\t{Type},\t{SpLength}";
        }
    }


    /// <summary> 里程桩号小的在前面 </summary>
    public class MileageCompare : IComparer<MileageInfo>
    {
        public int Compare(MileageInfo x, MileageInfo y)
        {
            return x.Mileage.CompareTo(y.Mileage);
        }
    }

}
