using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.AddinManager;

namespace eZcad.Examples
{
    [EcDescription("设置用户坐标系，并在用户坐标系上绘图")]
    public class DrawInUCS : ICADExCommand
    {
        private DocumentModifier _docMdf;

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var dat = new DllActivator_eZcad();
            dat.ActivateReferences();

            using (var docMdf = new DocumentModifier(true))
            {
                try
                {
                    _docMdf = docMdf;
                    DoSomething(docMdf, impliedSelection);

                    docMdf.acTransaction.Commit();
                    return ExternalCommandResult.Succeeded;
                }
                catch (Exception ex)
                {
                    docMdf.acTransaction.Abort(); // Abort the transaction and rollback to the previous state
                    errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace;
                    return ExternalCommandResult.Failed;
                }
            }
        }

        // 开始具体的调试操作
        private void DoSomething(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var acTrans = docMdf.acTransaction;

            // 以读模式打开UCSTable
            UcsTable acUCSTbl;
            acUCSTbl = acTrans.GetObject(docMdf.acDataBase.UcsTableId, OpenMode.ForRead) as UcsTable;
            UcsTableRecord acUCSTblRec;
            // 检查UCS表中是否有“New_UCS”这条记录
            if (acUCSTbl.Has("New_UCS") == false)
            {
                acUCSTblRec = new UcsTableRecord();
                acUCSTblRec.Name = "New_UCS";
                // 以写模式打开UCSTable
                acUCSTbl.UpgradeOpen();
                // 往UCSTable添加新记录
                acUCSTbl.Add(acUCSTblRec);
                acTrans.AddNewlyCreatedDBObject(acUCSTblRec, true);
            }
            else
            {
                acUCSTblRec = acTrans.GetObject(acUCSTbl["New_UCS"], OpenMode.ForWrite) as UcsTableRecord;
            }
            acUCSTblRec.Origin = new Point3d(4, 5, 3);
            acUCSTblRec.XAxis = new Vector3d(1, 0, 0);
            acUCSTblRec.YAxis = new Vector3d(0, 1, 0);

            // 打开当前视口
            ViewportTableRecord acVportTblRec = acTrans.GetObject(docMdf.acEditor.ActiveViewportId, OpenMode.ForWrite) as ViewportTableRecord;

            // 在当前视口的原点显示UCS图标
            acVportTblRec.IconAtOrigin = true;
            acVportTblRec.IconEnabled = true;

            // 设置UCS为当前坐标系
            acVportTblRec.SetUcs(acUCSTblRec.ObjectId);
            docMdf.acEditor.UpdateTiledViewportsFromDatabase();

            // 显示当前UCS坐标系的名称
            UcsTableRecord acUCSTblRecActive = acTrans.GetObject(acVportTblRec.UcsName, OpenMode.ForRead) as UcsTableRecord;
            Application.ShowAlertDialog("The current UCS is: " + acUCSTblRecActive.Name);

            // 另一种方式显示出当前用户坐标系的参数
            Matrix3d curUCSMatrix = docMdf.acEditor.CurrentUserCoordinateSystem;
            CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

            // 在用户坐标系下绘制一条线
            var btrTable = acTrans.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr = acTrans.GetObject(btrTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            var line = new Line(new Point3d(0, 0, 0), new Point3d(1, 0, 0));

            // Entity.TransformBy() 用来进行坐标变换，如果不进行变换，则上面的line的定位是相对于世界坐标系的。
            line.TransformBy(curUCSMatrix);
            btr.AppendEntity(line);
            acTrans.AddNewlyCreatedDBObject(line,true);

            // 提示选取一个点
            var pPtOpts = new PromptPointOptions("");
            pPtOpts.Message = "\nEnter a point: ";
            var pPtRes = docMdf.acEditor.GetPoint(pPtOpts);
            if (pPtRes.Status == PromptStatus.OK)
            {
                // 获得的点的坐标是在当前UCS下定义的
                var pt3dUCS = pPtRes.Value;

                // 将该点的当前UCS坐标转变为WCS坐标
                var newMatrix = Matrix3d.AlignCoordinateSystem(Point3d.Origin,
                     Vector3d.XAxis,
                     Vector3d.YAxis,
                     Vector3d.ZAxis,
                     acVportTblRec.Ucs.Origin,
                     acVportTblRec.Ucs.Xaxis,
                     acVportTblRec.Ucs.Yaxis,
                     acVportTblRec.Ucs.Zaxis);
                var pt3dWCS = pt3dUCS.TransformBy(newMatrix);

                Application.ShowAlertDialog("The WCS coordinates are: \n" + pt3dWCS.ToString() + "\n" +
                                            "The UCS coordinates are: \n" + pt3dUCS.ToString());
            }
        }
    }
}