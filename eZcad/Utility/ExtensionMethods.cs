using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionMethods
    {
        #region ---   ObjectId

        #endregion

        #region ---   Point3d

        /// <summary> 以直接取消Z坐标的方式投影到XY平面 </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point2d ToXYPlane(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        #endregion

        #region ---   Vector3d

        /// <summary> 设置一个向量的长度 </summary>
        /// <param name="originalVec"></param>
        /// <param name="newLength"></param>
        /// <returns></returns>
        public static Vector3d SetLength(this Vector3d originalVec, double newLength)
        {
            var r = newLength / originalVec.Length;
            return new Vector3d(originalVec.X * r, originalVec.Y * r, originalVec.Z * r);
        }

        #endregion

        #region ---   BlockReference

        /// <summary> 根据块参照中属性定义的名称返回对应的项 </summary>
        /// <param name="blk"></param>
        /// <param name="attTag">属性定义的名称</param>
        /// <returns></returns>
        public static AttributeReference GetAttributeReference(this BlockReference blk, string attTag)
        {
            foreach (ObjectId id in blk.AttributeCollection)
            {
                var att = id.GetObject(OpenMode.ForRead) as AttributeReference;
                if (att.Tag == attTag)
                {
                    return att;
                }
            }
            return null;
        }
        #endregion

        #region ---   几何操作

        /// <summary> 将三维多段线投影到XY平面上，以转换为二维多段线 </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public static CompositeCurve2d Get2dCurve(this Polyline pl)
        {
            LineSegment2d seg2d;
            var seg2ds = new Curve2d[pl.NumberOfVertices - 1];
            for (int i = 0; i < pl.NumberOfVertices - 1; i++)
            {
                seg2d = pl.GetLineSegment2dAt(i);
                seg2ds[i] = (seg2d);
            }
            return new CompositeCurve2d(seg2ds);
        }
        #endregion

        #region ---   Handle

        /// <summary> 根据 AutoCAD 中对象的句柄值，返回对应的对象的<seealso cref="ObjectId"/>值 </summary>
        /// <returns></returns>
        public static ObjectId GetObjectId(this Handle handle, Database db)
        {
            return db.GetObjectId(false, handle, 0);
        }

        /// <summary> 根据 AutoCAD 中对象的句柄值，返回对应的对象，未找到对应的对象，或者对象类型转换出错，则返回 null </summary>
        /// <returns></returns>
        public static T GetDBObject<T>(this Handle handle, Database db) where T : DBObject
        {
            var id = handle.GetObjectId(db);
            return id.GetObject(OpenMode.ForRead) as T;
        }
        #endregion

        #region ---   DBText

        /// <summary> 单行文本的宽度，不论此文本的旋转角度是多少，都返回其文字宽度，而不是其水平宽度 </summary>
        /// <remarks>对于旋转角度为0的单行文字，其宽度可以通过Bounds来直接提取，而对于有旋转的单行文字，其Bounds的水平宽度不代表其真实宽度，而应该进行一些折减。本
        /// 算法的大致思路是通过Bounds的矩形对角线长度减去两侧的误差值。
        /// 折减后的宽度与真实宽度的误差可控制在4%以内，文字旋转角度靠近45、135、225、315度时误差最大。</remarks>
        public static double GetTextWidth(this DBText txt)
        {
            var b = txt.Bounds.Value;
            var angT = txt.Rotation;
            // 对角线长度
            var l = b.MaxPoint.DistanceTo(b.MinPoint);
            var angL = Math.Atan((b.MaxPoint.Y - b.MinPoint.Y) / (b.MaxPoint.X - b.MinPoint.X));
            var h = txt.Height;
            // 如果文字旋转角度位于第二或四象限，则要特殊处理
            var reg = GetRigion(angT / Math.PI * 180);
            if (reg == 2 || reg == 4)
            {
                angT = angT - Math.PI / 2;
                angL = Math.PI / 2 - angL;
            }
            // 减去两端的旋转误差
            var ww = (l - 2 * h * Math.Abs(Math.Cos(angT) * Math.Sin(angT))) * Math.Abs(Math.Cos(angL - angT));
            return ww; // 1.0 mm 为两种语言之间的间隔;
        }

        /// <summary> 计算某角度位于哪一象限 </summary>
        private static int GetRigion(double angD)
        {
            // 首先将角度的可能范围设置到[0,360)

            if (angD >= 0 && angD < 90)
            {
                return 1;
            }
            else if (angD >= 90 && angD < 180)
            {
                return 2;
            }
            else if (angD >= 180 && angD < 270)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        /// <summary> 在不改变单行文字定位的情况下，修改其对齐方式。默认为默认的左对齐 </summary>
        /// <param name="txt"></param>
        /// <param name="horiMode">水平对齐方式</param>
        /// <param name="vertMode">竖直对齐方式</param>
        public static void SetAlignment(this DBText txt,
            TextHorizontalMode horiMode = TextHorizontalMode.TextLeft, TextVerticalMode vertMode = TextVerticalMode.TextBase)
        {
            var originalPosition = txt.Position;

            // 设置对齐后，其 AlignmentPoint 会被自动设置到 {0,0,0}，而且其 Position 属性被屏蔽，设置其值无效。
            txt.HorizontalMode = horiMode;
            txt.VerticalMode = vertMode;

            if (txt.IsDefaultAlignment)
            {
                // AlignmentPoint 始终为 {0,0,0}，此时通过设置 Position来控制其定位
                txt.Position = originalPosition;
            }
            else
            {
                // Position 属性无效，此时通过设置 AlignmentPoint 来控制其定位
                txt.AlignmentPoint = originalPosition.Add(txt.Position.GetVectorTo(txt.AlignmentPoint));
            }
        }

        #endregion
    }
}
