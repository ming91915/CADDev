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

        /// <summary> 将三维折线多段线投影到XY平面上，以转换为二维多段线 </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public static CompositeCurve2d Get2dLinearCurve(this Polyline pl)
        {
            return (pl.GetGeCurve() as CompositeCurve3d).Get2dLinearCurve();
        }

        /// <summary> 将三维折线多段线投影到XY平面上，以转换为二维多段线 </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public static CompositeCurve2d Get2dLinearCurve(this CompositeCurve3d pl)
        {
            LineSegment2d seg2d;
            var curve3ds = pl.GetCurves();
            var seg2ds = new Curve2d[curve3ds.Length];
            Curve3d c;
            for (int i = 0; i < curve3ds.Length; i++)
            {
                c = curve3ds[i];
                seg2d = new LineSegment2d(c.StartPoint.ToXYPlane(), c.EndPoint.ToXYPlane());
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

        #region ---   Exception


        /// <summary> 具体的报错信息与报错位置 </summary>
        /// <returns></returns>
        public static string AppendMessage(this Exception ex)
        {
            return "\r\n" + ex.Message + "\r\n" + ex.StackTrace;
        }

        #endregion

    }
}
