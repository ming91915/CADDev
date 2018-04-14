using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.Utility
{
    /// <summary> 对空间立方体进行一系列的信息提取 </summary>
    public class AdvancedExtents3d
    {

        public readonly Extents3d Ext;
        public readonly Point3d MaxP;
        public readonly Point3d MinP;

        public enum Anchor
        {
            /// <summary> 几何中心 </summary>
            GeometryCenter,
        }


        public AdvancedExtents3d(Extents3d ext)
        {
            Ext = ext;
            MaxP = Ext.MaxPoint;
            MinP = Ext.MinPoint;
        }

        /// <summary> 获取空间立方体的特征角点 </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public Point3d GetAnchor(Anchor anchor)
        {

            switch (anchor)
            {
                case Anchor.GeometryCenter:
                    return new Point3d(
               (MinP.X + MaxP.X) / 2,
               (MinP.Y + MaxP.Y) / 2,
               (MinP.Z + MaxP.Z) / 2
               );

                default:
                    return MinP;

            }
        }

        /// <summary> 高度 </summary>
        /// <returns></returns>
        public double GetHeight()
        {
            return MaxP.Y - MinP.Y;
        }
    }
}
