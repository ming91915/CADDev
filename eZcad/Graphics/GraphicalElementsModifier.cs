using System;
using System.Collections.Generic;
using System.Windows;
using AutoCADDev;
using AutoCADDev.Graphics;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(GraphicalElementsModifier))]

namespace AutoCADDev.Graphics
{
    /// <summary> 对界面中的各种图形进行修改 </summary>
    internal static class GraphicalElementsModifier
    {
        /// <summary> 提示用户在界面中选择多个椭圆形，并将其转换为多段线 </summary>
        /// <remarks></remarks>
        [CommandMethod("EllipseToPolyLine", CommandFlags.UsePickSet)]
        public static void EllipseToPolyLine()
        {

            // 获得当前文档和数据库   Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            Database acDb = acDoc.Database;

            using (var locker = acDoc.LockDocument())
            {
                //启动一个事务   Start a transaction
                using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
                {
                    //
                    List<Circle> listCircle = new List<Circle>();
                    List<Polyline> listPoly = new List<Polyline>();

                    // ----------------- 获得 PickFirst 选择集    Get the PickFirst selection set
                    Editor acDocEd = acDoc.Editor;
                    PromptSelectionResult acSSPrompt = acDocEd.SelectImplied();
                    // 如果提示状态是 OK，那么对象在命令启动前已经被选择了
                    if (acSSPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        ObjectId[] Ids = acSSet.GetObjectIds();
                        foreach (ObjectId id in Ids)
                        {
                            
                            if (string.Compare(id.ObjectClass.DxfName, "Circle", true) == 0)
                            {
                                listCircle.Add((Circle)(id.GetObject(OpenMode.ForWrite)));
                            }
                        }
                    }
                    else
                    {
                        // 清除 PickFirst 选择集    Clear the PickFirst selection set
                        ObjectId[] idarrayEmpty = null;
                        acDocEd.SetImpliedSelection(idarrayEmpty);

                        //选择要分解的圆形
                        PromptSelectionOptions pSelectOpts = new PromptSelectionOptions();
                        pSelectOpts.MessageForAdding = "选择要分解的圆形";
                        PromptSelectionResult pSelectionRes = acDoc.Editor.GetSelection(pSelectOpts);
                        //
                        if (pSelectionRes.Status == PromptStatus.OK)
                        {
                            ObjectId[] SelectIds = pSelectionRes.Value.GetObjectIds();
                            foreach (ObjectId id in SelectIds)
                            {
                                if (string.Compare(id.ObjectClass.DxfName, "Circle", true) == 0)
                                {
                                    listCircle.Add((Circle)(id.GetObject(OpenMode.ForWrite)));
                                }
                            }
                        }
                    }

                    if (listCircle.Count > 0)
                    {
                        // 设置一个整数值，代表要将圆形转换为多段线的段数
                        PromptIntegerOptions pKeyOpts = new PromptIntegerOptions("选择多段线段数");
                        pKeyOpts.DefaultValue = 24;
                        PromptIntegerResult pKeyRes = acDoc.Editor.GetInteger(pKeyOpts);
                        int n = Convert.ToInt32(pKeyRes.Value);
                        //
                        if (n > 3)
                        {
                            //开始绘制多段线
                            foreach (Circle c in listCircle)
                            {
                                Point3d center = c.Center;
                                double R = Convert.ToDouble(c.Radius);
                                //删除圆形
                                c.Erase();
                                double ang = (double)360 / n;
                                Polyline poly = new Polyline(n);
                                //设置图形的默认属性
                                poly.SetDatabaseDefaults();
                                for (int i = 0; i <= n; i++)
                                {
                                    Point2d p = new Point2d(center.X + R * Math.Cos(Math.PI / 180 * ang * i),
                                        center.Y + R * Math.Sin(Math.PI / 180 * ang * i));
                                    //为多段线添加节点
                                    poly.AddVertexAt(i, p, 0, 0, 0);
                                }
                                poly.Closed = true; // 将整个多段线进行封闭（不会将第一个点与最后一个点合并为同一个点）

                                // 以只读方式打开块表
                                var acBlkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                                // 以可写方式打开块表记录
                                var acBlkTblRec =
                                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                        BlockTableRecord;

                                // 添加新对象父对象中
                                acBlkTblRec.AppendEntity(poly);
                                //将新对象添加到事务中
                                acTrans.AddNewlyCreatedDBObject(poly, true);
                                listPoly.Add(poly);
                            }
                        }
                    }
                    // 提交事务中所作的所有更改
                    acTrans.Commit();
                }
            }
        }

        /// <summary> 提示用户在界面中选择多个圆形，并将其转换为多段线 </summary>
        /// <remarks></remarks>
        [CommandMethod("CircleToPolyLine", CommandFlags.UsePickSet)]
        public static void CircleToPolyLine()
        {
            // 获得当前文档和数据库   Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            using (var locker = acDoc.LockDocument())
            {
                //启动一个事务   Start a transaction
                using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
                {
                    //
                    List<Circle> listCircle = new List<Circle>();
                    List<Polyline> listPoly = new List<Polyline>();

                    // ----------------- 获得 PickFirst 选择集    Get the PickFirst selection set
                    Editor acDocEd = acDoc.Editor;
                    PromptSelectionResult acSSPrompt = acDocEd.SelectImplied();
                    // 如果提示状态是 OK，那么对象在命令启动前已经被选择了
                    if (acSSPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        ObjectId[] Ids = acSSet.GetObjectIds();
                        foreach (ObjectId id in Ids)
                        {
                            if (string.Compare(id.ObjectClass.DxfName, "Circle", true) == 0)
                            {
                                listCircle.Add((Circle)(id.GetObject(OpenMode.ForWrite)));
                            }
                        }
                    }
                    else
                    {
                        // 清除 PickFirst 选择集    Clear the PickFirst selection set
                        ObjectId[] idarrayEmpty = null;
                        acDocEd.SetImpliedSelection(idarrayEmpty);

                        //选择要分解的圆形
                        PromptSelectionOptions pSelectOpts = new PromptSelectionOptions();
                        pSelectOpts.MessageForAdding = "选择要分解的圆形";
                        PromptSelectionResult pSelectionRes = acDoc.Editor.GetSelection(pSelectOpts);
                        //
                        if (pSelectionRes.Status == PromptStatus.OK)
                        {
                            ObjectId[] SelectIds = pSelectionRes.Value.GetObjectIds();
                            foreach (ObjectId id in SelectIds)
                            {
                                RXClass c = id.ObjectClass;
                                if (string.Compare(id.ObjectClass.DxfName, "Circle", true) == 0)
                                {
                                    listCircle.Add((Circle)(id.GetObject(OpenMode.ForWrite)));
                                }
                            }
                        }
                    }

                    if (listCircle.Count > 0)
                    {
                        // 设置一个整数值，代表要将圆形转换为多段线的段数
                        PromptIntegerOptions pKeyOpts = new PromptIntegerOptions("选择多段线段数");
                        pKeyOpts.DefaultValue = 24;
                        PromptIntegerResult pKeyRes = acDoc.Editor.GetInteger(pKeyOpts);
                        int n = Convert.ToInt32(pKeyRes.Value);
                        //
                        if (n > 3)
                        {
                            //开始绘制多段线
                            foreach (Circle c in listCircle)
                            {
                                Point3d center = c.Center;
                                double R = Convert.ToDouble(c.Radius);
                                //删除圆形
                                c.Erase();
                                double ang = (double)360 / n;
                                Polyline poly = new Polyline(n);
                                //设置图形的默认属性
                                poly.SetDatabaseDefaults();
                                for (int i = 0; i <= n; i++)
                                {
                                    Point2d p = new Point2d(center.X + R * Math.Cos(Math.PI / 180 * ang * i),
                                        center.Y + R * Math.Sin(Math.PI / 180 * ang * i));
                                    //为多段线添加节点
                                    poly.AddVertexAt(i, p, 0, 0, 0);
                                }
                                poly.Closed = true; // 将整个多段线进行封闭（不会将第一个点与最后一个点合并为同一个点）

                                // 以只读方式打开块表
                                var acBlkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                                // 以可写方式打开块表记录
                                var acBlkTblRec =
                                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                                        BlockTableRecord;

                                // 添加新对象父对象中
                                acBlkTblRec.AppendEntity(poly);
                                //将新对象添加到事务中
                                acTrans.AddNewlyCreatedDBObject(poly, true);
                                listPoly.Add(poly);
                            }
                        }
                    }
                    // 提交事务中所作的所有更改
                    acTrans.Commit();
                }
            }
        }
    }
}