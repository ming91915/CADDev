using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

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

        #region ---   BlockTableRecord



        /// <summary> 根据块参照定义中所有的属性定义 </summary>
        /// <param name="btr">要进行提取的块定义</param>
        /// <returns></returns>
        public static List<AttributeDefinition> GetAttributeDefinitions(this BlockTableRecord btr)
        {
            // return btr.Cast<AttributeDefinition>().ToList();

            var attDefs = new List<AttributeDefinition>();
            var attDefTp = RXObject.GetClass(typeof(AttributeDefinition));

            foreach (ObjectId id in btr)
            {
                // 判断该实体是否是块属性定义
                if (id.ObjectClass.Equals(attDefTp))
                {
                    var attDef = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if (attDef != null)
                    {
                        attDefs.Add(attDef);
                    }
                }
            }
            return attDefs;
        }

        /// <summary> 根据块参照定义中 指定的 Tag 的属性定义 </summary>
        /// <param name="btr">要进行提取的块定义</param>
        /// <param name="tag">要进行提取的块定义</param>
        /// <returns>如果未找到，则返回 null </returns>
        public static AttributeDefinition GetAttributeDefinitions(this BlockTableRecord btr, string tag)
        {
            var attDefTp = RXObject.GetClass(typeof(AttributeDefinition));

            foreach (ObjectId id in btr)
            {
                // 判断该实体是否是块属性定义
                if (id.ObjectClass.Equals(attDefTp))
                {
                    var attDef = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if (attDef != null && attDef.Tag == tag)
                    {
                        return attDef;
                    }
                }
            }
            return null;
        }

        #endregion

        #region ---   BlockReference

        /// <summary> 根据块参照中属性定义的名称返回对应的项 </summary>
        /// <param name="blk"></param>
        /// <param name="attTag">属性定义的名称</param>
        /// <returns></returns>
        public static List<AttributeReference> GetAttributeReferences(this BlockReference blk)
        {
            var attRefs = new List<AttributeReference>();
            foreach (ObjectId id in blk.AttributeCollection)
            {
                var att = id.GetObject(OpenMode.ForRead) as AttributeReference;
                attRefs.Add(att);
            }
            return attRefs;
        }

        /// <summary> 根据块参照中属性定义的名称返回对应的项 </summary>
        /// <param name="blk"></param>
        /// <param name="attTag">属性定义的名称</param>
        /// <returns></returns>
        public static AttributeReference GetAttributeReference(this BlockReference blk, string attTag)
        {
            foreach (ObjectId id in blk.AttributeCollection)
            {
                var att = id.GetObject(OpenMode.ForRead) as AttributeReference;
                if (att != null && att.Tag == attTag)
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

        /// <summary> 将表示句柄值的字符转换为句柄 </summary>
        /// <param name="handle">表示句柄的字符，即16进制的数值，比如“409E”。最小的句柄值为1。</param>
        public static Handle ConvertToHandle(string handle)
        {
            return new Handle(Convert.ToInt64(handle, 16));
        }


        /// <summary> 根据 AutoCAD 中对象的句柄值，返回对应的对象的<seealso cref="ObjectId"/>值，如果不存在，则返回 <seealso cref="ObjectId.Null"/> </summary>
        /// <returns></returns>
        private static ObjectId GetObjectId(this Handle handle, Database db)
        {
            ObjectId res = ObjectId.Null;
            // var succ = db.TryGetObjectId(handle, out res);
            db.TryGetObjectId(handle, out res);
            // 如果失败则输出 ObjectId.Null
            return res;
            // return db.GetObjectId(false, handle, 0);
        }

        /// <summary> 根据 AutoCAD 中对象的句柄值，返回对应的对象，未找到对应的对象，或者对象类型转换出错，则返回 null </summary>
        /// <returns></returns>
        public static T GetDBObject<T>(this Handle handle, Database db) where T : DBObject
        {
            try
            {
                var id = db.GetObjectId(false, handle, 0);
                return id.GetObject(OpenMode.ForRead) as T;
            }
            catch (Exception)
            {
                return null;
                // ignored
            }
            //var id = handle.GetObjectId(db);
            //return (id == ObjectId.Null) ? null : id.GetObject(OpenMode.ForRead) as T;
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

        /// <summary> 在不改变单行文字定位的情况下，修改其对正方式。默认为默认的左对齐 </summary>
        /// <param name="txt"></param>
        /// <param name="justify"> 整体对正方式 </param>
        public static void SetAlignment(this DBText txt, AttachmentPoint justify)
        {
            switch (justify)
            {
                case AttachmentPoint.BaseLeft:
                    SetAlignment(txt, TextVerticalMode.TextBase, TextHorizontalMode.TextLeft);
                    break;
                case AttachmentPoint.BaseFit:
                    SetAlignment(txt, TextVerticalMode.TextBase, TextHorizontalMode.TextFit);
                    break;
            }
        }

        /// <summary> 在不改变单行文字定位的情况下，修改其对齐方式。默认为默认的左对齐 </summary>
        /// <param name="txt"></param>
        /// <param name="horiMode">水平对齐方式</param>
        /// <param name="vertMode">竖直对齐方式</param>
        public static void SetAlignment(this DBText txt,
           TextVerticalMode vertMode = TextVerticalMode.TextBase, TextHorizontalMode horiMode = TextHorizontalMode.TextLeft)
        {
            var originalPosition = txt.Position;
            // 1. 计算出变形后的 AlignmentPoint。
            // 注：除了“布满”与“对齐”这两种对正方式外，其他的对齐方式在互相转换时，AlignmentPoint 是不变的，其计算方式如下面代码；
            // 而“布满”与“对齐”这两种对正方式，会改变单行文字的宽度，所以在切换时，AlignmentPoint 会略有变化。
            Point3d alignPt = txt.IsDefaultAlignment ? originalPosition : txt.AlignmentPoint;

            // 2. 设置对齐后，其 AlignmentPoint 会被自动设置到 {0,0,0}，而且其 Position 属性被屏蔽，设置其值无效。
            txt.HorizontalMode = horiMode;
            txt.VerticalMode = vertMode;

            // 3. 调整文字定位
            if (txt.IsDefaultAlignment)
            {
                // AlignmentPoint 始终为 {0,0,0}，此时通过设置 Position来控制其定位
                txt.Position = originalPosition;
            }
            else
            {
                // Position 属性无效，此时通过设置 AlignmentPoint 来控制其定位
                // 注意设置对齐后的 txt.AlignmentPoint 也可能是无效的{0,0,0}，所以要用前面计算得到的 对齐点
                txt.AlignmentPoint = originalPosition.Add(txt.Position.GetVectorTo(alignPt));
            }
        }

        #endregion

        #region ---   Extents3d

        /// <summary> 是否包含某一点（包括边界） </summary>
        public static bool Contains(this Extents3d ext, Point3d pt)
        {
            return (pt.X >= ext.MinPoint.X && pt.X <= ext.MaxPoint.X) &&
                 (pt.Y >= ext.MinPoint.Y && pt.Y <= ext.MaxPoint.Y) &&
                 (pt.Z >= ext.MinPoint.Z && pt.Z <= ext.MaxPoint.Z);
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
