using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Debug;
using eZcad.Examples;
using eZcad.Utility;

namespace eZcad.Addins.LayoutViewport
{
    /// <summary> AddinManager 调试代码模板 </summary>
    [EcDescription(CommandDescription)]
    public class ViewportCreator : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CreateViewport";

        private const string CommandText = CommandName;
        private const string CommandDescription = @"根据选择的平面曲线生成对应的视口";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor |
            CommandFlags.NoPaperSpace)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void CreateViewport()
        {
            DocumentModifier.ExecuteCommand(CreateViewport);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ViewportCreator();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.CreateViewport,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        // 开始具体的调试操作
        private ExternalCmdResult CreateViewport(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;

            //// var vp1 = AddinManagerDebuger.PickObject<Entity>(docMdf.acEditor);
            //var hd = Utils.ConvertToHandle("AA07");
            //var vp2 = hd.GetObjectId(docMdf.acDataBase).GetObject(OpenMode.ForRead) as Viewport;
            //vp2.UpgradeOpen();
            //var lot = LayoutManager.Current.GetLayoutId("NewLayout4011").GetObject(OpenMode.ForRead) as Layout;
            //vp2.RotateViewport(_docMdf, lot, new Point2d(0, 0), 30.0 / 180 * Math.PI);

            //return ExternalCmdResult.Commit;
            // 从模型空间中获取视口裁剪框
            var pl_Model = AddinManagerDebuger.PickObject<Curve>(docMdf.acEditor);
            Point3d bottomLeftPt = default(Point3d);
            Point3d bottomRightPt = default(Point3d);
            double bottomLength = 0;
            var succ = GraphicalElementsSelector.GetPoint(docMdf.acEditor, "选择图纸的左下角点", out bottomLeftPt);
            if (!succ) return ExternalCmdResult.Cancel;
            succ = GraphicalElementsSelector.GetPoint(docMdf.acEditor, "选择图纸的右下角点", out bottomRightPt);
            if (!succ) return ExternalCmdResult.Cancel;
            succ = GraphicalElementsSelector.GetDouble(docMdf.acEditor, "图纸宽度（布局空间的单位）", out bottomLength,
                defaultValue: 420, allowNegative: false);
            if (!succ) return ExternalCmdResult.Cancel;

            var modelUcs = docMdf.acEditor.GetCurrentView().Ucs;

            // 打开布局
            var lm = LayoutManager.Current;
            Layout layout;
            ObjectId layoutId;
            Form_LayoutLister f = new Form_LayoutLister(docMdf);
            f.ShowDialog();
            if (f.CreateNewLayout)
            {
                var layoutName = "NewLayout" + DateTime.Now.Minute + DateTime.Now.Second;
                layoutId = lm.GetLayoutId(layoutName);
                if (!layoutId.IsValid)
                {
                    // 重启事务
                    docMdf.RestartTransaction(commitCancel: false);
                    //
                    layoutId = LayoutUtil.CreateLayout(layoutName);
                    layout = layoutId.GetObject(OpenMode.ForRead) as Layout;
                    // 
                    LayoutUtil.SetPlotSettings(layout, "A3", "monochrome.ctb", "交通院道路室 121");
                }
                else
                {
                    layout = LayoutManager.Current.GetLayoutId(name: layoutName).GetObject(OpenMode.ForRead) as Layout;
                }
            }
            else if (f.Layout != null)
            {
                layout = f.Layout;
            }
            else
            {
                return ExternalCmdResult.Cancel;
            }

            // 创建视口
            lm.SetCurrentLayoutId(layout.Id);
            CreateViewport(docMdf, modelUcs, layout, pl_Model, bottomLeftPt, bottomRightPt, bottomLength);
            // LayoutUtil.SwitchLayout();
            return ExternalCmdResult.Commit;
        }


        /// <summary> 根据给定的参数创建出视口 </summary>
        /// <param name="docMdf"></param>
        /// <param name="layout"></param>
        /// <param name="clipCurveInModel"></param>
        /// <param name="bottomLeftPt">图框的左下角点</param>
        /// <param name="bottomRightPt">图框的右下角点</param>
        /// <param name="bottomLength"></param>
        private void CreateViewport(DocumentModifier docMdf, CoordinateSystem3d modelUcs, Layout layout, Curve clipCurveInModel,
            Point3d bottomLeftPt, Point3d bottomRightPt, double bottomLength)
        {
            var brt = layout.BlockTableRecordId.GetObject(OpenMode.ForRead) as BlockTableRecord;
            brt.UpgradeOpen();
            // 视口的裁剪区域，此区域可以由多段线、圆弧或样条曲线等来定义，而且曲线可以不闭合。
            var layoutClipCurve = Curve.CreateFromGeCurve(geCurve: clipCurveInModel.GetGeCurve());
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
            acVport.NonRectClipEntityId = layoutClipCurve.ObjectId;
            acVport.NonRectClipOn = true;
            acVport.On = true;

            // 将视口放置到不打印层
            acVport.Layer = ACadConstants.LayerName_Defpoints;
            layoutClipCurve.Layer = ACadConstants.LayerName_Defpoints;

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
            acVport.Locked = true;

            // -----------------------------------------------   视口对象在布局中的定位
            // 对视口所绑定的几何曲线的平移和缩放操作可以对视口进行变换，变换过程中视口中的显示内容在布局中的位置也发生同等变换，即是将视口与其中的内容作为一个整体进行变换
            // 但是直接对acVport进行变换，并不会生效。
            var scale = bottomLength / bottomLeftPt.DistanceTo(bottomRightPt);
            var layoutOrigin = new Point3d(0, 0, 0);
            var disp = bottomLeftPt.GetVectorTo(layoutOrigin).Subtract(modelUcs.Origin.GetAsVector());
            docMdf.WriteNow(scale, disp, bottomLeftPt, modelUcs.Origin);
            layoutClipCurve.TransformBy(Matrix3d.Displacement(disp));
            layoutClipCurve.TransformBy(Matrix3d.Scaling(scaleAll: scale, center: (new Point3d(0, 0, 0))));
            //
            //var angle = origin.GetVectorTo(bottomRightPt).GetAngleTo(new Vector3d(1, 0, 0));
            //ViewportUtil.RotateViewport(acVport, _docMdf, layout, new Point2d(0, 0), angle);
        }
    }
}