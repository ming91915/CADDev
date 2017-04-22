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
    }
}
