using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;

namespace eZcad.Examples
{

    /// <summary> 布局中视口的创建与定位 </summary>
    internal class ViewportHandler
    {

        // 开始具体的调试操作
        public static ExternalCmdResult CreateViewport(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            // 打开布局
            var lm = LayoutManager.Current;
            var layout = lm.GetLayoutId(name: "A3").GetObject(OpenMode.ForRead) as Layout;
            lm.SetCurrentLayoutId(layout.Id);
            var brt = layout.BlockTableRecordId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            brt.UpgradeOpen();


            // 从模型空间中获取视口裁剪框
            var handle = ExtensionMethods.ConvertToHandle("a31b");
            var pl_Model = handle.GetDBObject<Curve>(docMdf.acDataBase);

            // 视口的裁剪区域，此区域可以由多段线、圆弧或样条曲线等来定义，而且曲线可以不闭合。
            var layoutClipCurve = Curve.CreateFromGeCurve(geCurve: pl_Model.GetGeCurve());
            brt.AppendEntity(layoutClipCurve);
            docMdf.acTransaction.AddNewlyCreatedDBObject(layoutClipCurve, true);
            var viewExt = new AdvancedExtents3d(layoutClipCurve.GeometricExtents);
            var center = viewExt.GetAnchor(AdvancedExtents3d.Anchor.GeometryCenter);

            // 创建视口
            Viewport acVport = new Viewport();
            brt.AppendEntity(acVport);
            docMdf.acTransaction.AddNewlyCreatedDBObject(acVport, true);
            brt.DowngradeOpen();

            // 设置视口所对应的裁剪图形
            acVport.On = true;
            acVport.NonRectClipEntityId = layoutClipCurve.ObjectId;
            acVport.NonRectClipOn = true;

            // -----------------------------------------------   设置视口的显示区域
            acVport.PerspectiveOn = false;
            // ViewHeight属性– 表示视口内模型空间视图的高度。它决定的视口显示的缩放比例
            // 如果要按1：1显示，则需要将其设置为视口多段线所对应的Extents3d的高度。
            acVport.ViewHeight = viewExt.GetHeight();
            // ViewCenter属性- 表示视口内视图的观察中心。它决定的视口显示的平面定位
            // 如果要视图内容范围完全匹配多段线的区域，则需要将其设置为视口多段线的几何中心点。
            acVport.ViewCenter = center.ToXYPlane();
            //ViewHeight属性– 表示视口内模型空间视图的高度。
            acVport.ViewDirection = new Vector3d(0, 0, 1);
            // ViewTarget属性– 表示视口内视图的目标点的位置。
            acVport.ViewTarget = new Point3d(0, 0, 0);
            acVport.Locked = false;


            // -----------------------------------------------   视口对象在布局中的定位
            // 对视口所绑定的几何曲线的平移和缩放操作可以对视口进行变换，变换过程中视口中的显示内容在布局中的位置也发生同等变换，即是将视口与其中的内容作为一个整体进行变换
            // 但是直接对acVport进行变换，并不会生效。
            layoutClipCurve.TransformBy(Matrix3d.Displacement(new Vector3d(-10, 10, 0)));
            layoutClipCurve.TransformBy(Matrix3d.Scaling(3, center));

            // 对视口所绑定的几何曲线 layoutClipCurve 的Rotation 操作可以对视口进行旋转，但是奇怪的是，在变换过程中，视口中的显示内容相对于布局空间未发生旋转，却进行了平移与缩放。
            // 平移的后的视图中心点依然与视口的几何中心点重合，缩放的比例可以暂且简单理解为"1/cos(angle)"。
            // 而如果要实现视口中内容随视口进行整体旋转，必须对acVport对象进行旋转变换。
            var angle = 45.0 / 180.0 * Math.PI;
            acVport.TransformBy(Matrix3d.Rotation(angle, new Vector3d(0, 0, 1), new Point3d(0, 0, 0)));
            layoutClipCurve.TransformBy(Matrix3d.Rotation(angle, new Vector3d(0, 0, 1), new Point3d(0, 0, 0)));

            //
            return ExternalCmdResult.Commit;
        }

    }
}
