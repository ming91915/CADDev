using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(DimAlignment))]

namespace eZcad.Addins
{
    /// <summary> 标注对象的对齐 </summary>
    public class DimAlignment
    {
        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        [CommandMethod("eZcad", "AlignDim", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void EcAlignDim()
        {
            DocumentModifier.ExecuteCommand(AlignDim);
        }

        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        public static void AlignDim(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var c = PickOneCurve(docMdf);
            if (c == null) return;
            c.Highlight();
            //
            var dims = SelectDims(docMdf);
            c.Unhighlight();
            if (dims == null || dims.Length == 0) return;
            //
            foreach (var dimId in dims)
            {
                var dim = docMdf.acTransaction.GetObject(dimId, OpenMode.ForRead);
                if ((dim is RotatedDimension))
                {
                    dim.UpgradeOpen();
                    AlignDimsToLine(docMdf, dim as RotatedDimension, c);
                    dim.DowngradeOpen();
                }
                else if (dim is AlignedDimension)
                {
                    dim.UpgradeOpen();
                    AlignDimsToLine(docMdf, dim as AlignedDimension, c);
                    dim.DowngradeOpen();
                }
            }
        }

        private static void AlignDimsToLine(DocumentModifier docMdf, RotatedDimension dim, Curve c)
        {
            // 找到标注的两个顶点
            var pt2 = FindIntersect(c, dim.DimLinePoint, dim.XLine2Point); // DimLinePoint 是 XLine2Point 所对应的点
            var pt1 = FindIntersect(c, dim.DimLinePoint + (dim.XLine1Point - dim.XLine2Point), dim.XLine1Point);
            if (pt1 != null)
            {
                dim.XLine1Point = pt1.Value;
            }

            if (pt2 != null)
            {
                dim.XLine2Point = pt2.Value;
            }
            //
        }
        private static void AlignDimsToLine(DocumentModifier docMdf, AlignedDimension dim, Curve c)
        {
            // 找到标注的两个顶点
            var pt2 = FindIntersect(c, dim.DimLinePoint, dim.XLine2Point);
            var pt1 = FindIntersect(c, dim.DimLinePoint + (dim.XLine1Point - dim.XLine2Point), dim.XLine1Point);
            if (pt1 != null)
            {
                dim.XLine1Point = pt1.Value;
            }
            if (pt2 != null)
            {
                dim.XLine2Point = pt2.Value;
            }
            //
        }

        #region ---   界面交互

        /// <summary> 通过点选的方式选择一条曲线 </summary>
        [CommandMethod("PickOneCurve")]
        public static Curve PickOneCurve(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一条曲线 ");
            peO.SetRejectMessage("\n 请选择一个曲线对象\n");
            peO.AddAllowedClass(typeof(Curve), false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                return docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as Curve;
            }
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static ObjectId[] SelectDims(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "DIMENSION"),
                // 将标注类型限制为转角标注与对齐标注
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue(100, "AcDbAlignedDimension)"),
                new TypedValue(100, "AcDbRotatedDimension))"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择标注对象"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。


            var res = ed.GetSelection(pso, filter);

            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            return null;
        }

        #endregion

        /// <summary> 计算一条直线与一条曲线的交点 </summary>
        /// <param name="c"></param>
        /// <param name="basePt"></param>
        /// <param name="dimPt"></param>
        /// <returns></returns>
        private static Point3d? FindIntersect(Curve c, Point3d basePt, Point3d dimPt)
        {
            // var l = new Line(basePt, dimPt); // 创建一条线用来计算交点，但是并不将其添加到数据库
            var line3d = new Line3d(basePt, dimPt);
            var c3d = c.GetGeCurve();
            var pts = c3d.GetClosestPointTo(line3d).Select(r=>r.GetPoint()).ToArray();
            // var pts = new Point3dCollection();
            // c.IntersectWith(l, Intersect.ExtendArgument, c.GetPlane(), pts, IntPtr.Zero, IntPtr.Zero);
            // l.Erase(true); 会出现报错：eNoDatabase，因为它根本就没有添加到数据库，所以自然也删不掉。

            if (pts.Length > 0) // 说明找到了相交点
            {
                // 在多个相交点中取距离最近的那个点
                var closestPt = pts[0];
                var closestDist = basePt.DistanceTo(pts[0]);
                for (int i = 1; i < pts.Length; i++)
                {
                    var newD = basePt.DistanceTo(pts[i]);
                    if (newD < closestDist)
                    {
                        closestDist = newD;
                        closestPt = pts[i];
                    }
                }
                return closestPt;
            }
            else
            {
                return null;
            }
        }
    }
}