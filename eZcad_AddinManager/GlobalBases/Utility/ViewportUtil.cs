using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.Utility
{
    /// <summary> 将多个块的属性值进行统一编辑 </summary>
    public static class ViewportUtil
    {
        /// <summary> 旋转视口，并保持视口内容相对于视口的位置不变 </summary>
        /// <param name="vp">要进行旋转的视口</param>
        /// <param name="layout">视口所在的那个布局空间</param>
        /// <param name="basePt">旋转的基点在图纸空间中的坐标</param>
        /// <param name="angle">要旋转的角度，单位为弧度，按右手准则进行旋转</param>
        public static void RotateViewport(this Viewport vp, DocumentModifier docMdf, Layout layout, Point2d basePt,
            double angle)
        {
            ViewportTableRecord vpr;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // 对视口所绑定的几何曲线 layoutClipCurve 的Rotation 操作可以对视口进行旋转，但是奇怪的是，在变换过程中，视口中的显示内容相对于布局空间未发生旋转，却进行了平移与缩放。
            // 平移的后的视图中心点依然与视口的几何中心点重合，缩放的比例可以暂且简单理解为"1/cos(angle)"。
            // 而如果要实现视口中内容随视口进行整体旋转，必须对acVport对象进行旋转变换。
            vp.TransformBy(Matrix3d.Rotation(angle, axis: new Vector3d(0, 0, 1), center: new Point3d(0, 0, 0)));

            vp.UcsIconVisible = true;
            vp.UcsIconAtOrigin = true;
            Curve curve = null;
            if (vp.NonRectClipOn)
            {
                curve = vp.NonRectClipEntityId.GetObject(OpenMode.ForRead) as Curve;
                if (curve != null)
                {
                    curve.UpgradeOpen();
                    docMdf.WriteNow("找到视口对应的多段线");
                    //
                    // curve.TransformBy(Matrix3d.Rotation(angle, axis: new Vector3d(0, 0, 1), center: new Point3d(0, 0, 0)));
                }
            }
            //
            var ucsW = docMdf.acEditor.CurrentUserCoordinateSystem;
            Point3d ucsO = new Point3d(100, 100, 0);
            Vector3d ucsX = new Vector3d(1, 0, 0);
            Vector3d ucsY = new Vector3d(0, 1, 0);
            UcsTableRecord ucs = GetOrCreateUCS(docMdf.acTransaction, docMdf.acDataBase, "新ucs");
            ucs.Origin = new Point3d(100, 100, 0); ;
            ucs.XAxis = new Vector3d(1, 0, 0);
            ucs.YAxis = new Vector3d(0, 1, 0);
            vp.SetUcs(ucs.Id);
            // docMdf.acEditor.UpdateTiledViewportsFromDatabase();
            vp.UcsFollowModeOn = true;
            docMdf.WriteNow($"结束. {vp.UcsName}");
        }

        public static UcsTableRecord GetOrCreateUCS(Transaction trans, Database acCurDb, string ucsName)
        {
            UcsTable acUCSTbl;
            acUCSTbl = acCurDb.UcsTableId.GetObject(OpenMode.ForRead) as UcsTable;
            UcsTableRecord acUCSTblRec;
            // 检查UCS表中是否有“New_UCS”这条记录
            if (acUCSTbl.Has(ucsName) == false)
            {
                acUCSTblRec = new UcsTableRecord();
                acUCSTblRec.Name = ucsName;
                // 以写模式打开UCSTable
                acUCSTbl.UpgradeOpen();
                // 往UCSTable添加新记录
                acUCSTbl.Add(acUCSTblRec);
                trans.AddNewlyCreatedDBObject(acUCSTblRec, true);
                acUCSTbl.DowngradeOpen();
            }
            else
            {
                acUCSTblRec = trans.GetObject(acUCSTbl[ucsName], OpenMode.ForRead) as UcsTableRecord;
            }
            return acUCSTblRec;
        }

        /// <summary> 绘图图框 </summary>
        internal class DrawingBorder
        {
            /// <summary> 绘图区的中心点 </summary>
            public Point3d CenterPoint { get; }

            /// <summary> 绘图图框的插入点 </summary>
            public Point3d InsertPoint { get; }

            public double BorderWidth { get; }
            public double BorderHeight { get; }
            public double DrawingWidth { get; }
            public double DrawingHeight { get; }

            public DrawingBorder(Point3d centerPoint, Point3d insertPoint,
                double borderWidth, double borderHeight, double drawingWidth, double drawingHeight)
            {
                CenterPoint = centerPoint;
                InsertPoint = insertPoint;
                BorderWidth = borderWidth;
                BorderHeight = borderHeight;
                DrawingWidth = drawingWidth;
                DrawingHeight = drawingHeight;
            }
        }

        internal enum InsertStyle
        {
            绝对路径外部参照,
            相对路径外部参照,
            内部图块,
        }
    }
}