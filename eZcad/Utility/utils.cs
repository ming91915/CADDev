using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADDev.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utils
    {
        /// <summary> 设置一个向量的长度 </summary>
        /// <param name="originalVec"></param>
        /// <param name="newLength"></param>
        /// <returns></returns>
        public static Vector3d SetLength(this Vector3d originalVec, double newLength)
        {
            var r = newLength / originalVec.Length;
            return new Vector3d(originalVec.X * r, originalVec.Y * r, originalVec.Z * r);
        }

        /// <summary> 从字符中解析出坐标点，比如“1.2, 2.3, 5” </summary>
        public static Point3d? GetPointFromString(string coord)
        {
            var s = coord.Split(',');
            var xyz = new List<double>();
            double c = 0;
            foreach (var v in s)
            {
                if (double.TryParse(v, out c))
                {
                    xyz.Add(c);
                }
            }
            //
            switch (xyz.Count)
            {
                case 2: return new Point3d(xyz[0], xyz[1], 0);
                case 3: return new Point3d(xyz[0], xyz[1], xyz[2]);
                default:
                    return null;
            }
            return null;
        }
    }
}
