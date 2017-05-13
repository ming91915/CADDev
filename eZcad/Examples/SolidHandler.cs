using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace eZcad.Examples
{
    public class SolidHandler
    {
        //定义、查询2D多段线/3D多段线的坐标
        [CommandMethod("Polyline_2D_3D")]
        public static void Polyline_2D_3D()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 用2条线段（3个点）创建一条多段线
                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(1, 1), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(1, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(2, 2), 0, 0, 0);
                acPoly.ColorIndex = 1;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);

                // 用2条线段（3个点）创建一条3D多段线
                var acPoly3d = new Polyline3d();
                acPoly3d.ColorIndex = 5;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acPoly3d);
                acTrans.AddNewlyCreatedDBObject(acPoly3d, true);

                // Before adding vertexes, the polyline must be in the drawing
                // 先创建多段线对象，然后才能给它添加顶点
                var acPts3dPoly = new Point3dCollection();
                acPts3dPoly.Add(new Point3d(1, 1, 0));
                acPts3dPoly.Add(new Point3d(2, 1, 0));
                acPts3dPoly.Add(new Point3d(2, 2, 0));

                foreach (Point3d acPt3d in acPts3dPoly)
                {
                    var acPolVer3d = new PolylineVertex3d(acPt3d);
                    acPoly3d.AppendVertex(acPolVer3d);
                    acTrans.AddNewlyCreatedDBObject(acPolVer3d, true);
                }

                // 获取多段线的顶点坐标
                var acPts2d = new Point2dCollection();
                for (var nCnt = 0; nCnt < acPoly.NumberOfVertices; nCnt++)
                {
                    acPts2d.Add(acPoly.GetPoint2dAt(nCnt));
                }

                // 获取3D多段线的顶点坐标
                var acPts3d = new Point3dCollection();
                foreach (ObjectId acObjIdVert in acPoly3d)
                {
                    PolylineVertex3d acPolVer3d;
                    acPolVer3d = acTrans.GetObject(acObjIdVert,
                        OpenMode.ForRead) as PolylineVertex3d;

                    acPts3d.Add(acPolVer3d.Position);
                }

                // 显示坐标
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog("2D polyline (red): \n" +
                                                                                      acPts2d[0] + "\n" +
                                                                                      acPts2d[1] + "\n" +
                                                                                      acPts2d[2]);

                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog("3D polyline (blue): \n" +
                                                                                      acPts3d[0] + "\n" +
                                                                                      acPts3d[1] + "\n" +
                                                                                      acPts3d[2]);

                //提交事务
                acTrans.Commit();
            }
        }
        
        //将OCS坐标转变为WCS坐标
        [CommandMethod("TranslateCoordinates")]
        public static void TranslateCoordinates()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建2D多段线
                var acPoly2d = new Polyline2d();

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acPoly2d);
                acTrans.AddNewlyCreatedDBObject(acPoly2d, true);

                // 先将多段线添加到块表记录，然后才能给它添加顶点
                var acPts2dPoly = new Point3dCollection();
                acPts2dPoly.Add(new Point3d(1, 1, 0));
                acPts2dPoly.Add(new Point3d(1, 2, 0));
                acPts2dPoly.Add(new Point3d(2, 2, 0));
                acPts2dPoly.Add(new Point3d(3, 2, 0));
                acPts2dPoly.Add(new Point3d(4, 4, 0));

                foreach (Point3d acPt3d in acPts2dPoly)
                {
                    var acVer2d = new Vertex2d(acPt3d, 0, 0, 0, 0);
                    acPoly2d.AppendVertex(acVer2d);
                    acTrans.AddNewlyCreatedDBObject(acVer2d, true);
                }

                // Set the normal of the 2D polyline
                acPoly2d.Normal = new Vector3d(0, 1, 2);

                // Get the first coordinate of the 2D polyline
                var acPts3d = new Point3dCollection();
                Vertex2d acFirstVer = null;
                foreach (ObjectId acObjIdVert in acPoly2d)
                {
                    acFirstVer = acTrans.GetObject(acObjIdVert,
                        OpenMode.ForRead) as Vertex2d;

                    acPts3d.Add(acFirstVer.Position);

                    break;
                }

                // Get the first point of the polyline and 
                // use the elevation for the Z value
                var pFirstVer = new Point3d(acFirstVer.Position.X,
                    acFirstVer.Position.Y,
                    acPoly2d.Elevation);

                // Translate the OCS to WCS
                var mWPlane = Matrix3d.WorldToPlane(acPoly2d.Normal);
                var pWCSPt = pFirstVer.TransformBy(mWPlane);

                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(
                    "The first vertex has the following " +
                    "coordinates:" +
                    "\nOCS: " + pFirstVer +
                    "\nWCS: " + pWCSPt);

                // 提交事务
                acTrans.Commit();
            }
        }
        

        //创建多边形网格
        [CommandMethod("PolygoCreatenMesh")]
        public static void CreatePolygonMesh()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建多边形网格
                var acPolyMesh = new PolygonMesh();
                acPolyMesh.MSize = 4;
                acPolyMesh.NSize = 4;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acPolyMesh);
                acTrans.AddNewlyCreatedDBObject(acPolyMesh, true);

                // 添加顶点前，必须先将多边形网格添加到块表记录
                var acPts3dPMesh = new Point3dCollection();
                acPts3dPMesh.Add(new Point3d(0, 0, 0));
                acPts3dPMesh.Add(new Point3d(2, 0, 1));
                acPts3dPMesh.Add(new Point3d(4, 0, 0));
                acPts3dPMesh.Add(new Point3d(6, 0, 1));

                acPts3dPMesh.Add(new Point3d(0, 2, 0));
                acPts3dPMesh.Add(new Point3d(2, 2, 1));
                acPts3dPMesh.Add(new Point3d(4, 2, 0));
                acPts3dPMesh.Add(new Point3d(6, 2, 1));

                acPts3dPMesh.Add(new Point3d(0, 4, 0));
                acPts3dPMesh.Add(new Point3d(2, 4, 1));
                acPts3dPMesh.Add(new Point3d(4, 4, 0));
                acPts3dPMesh.Add(new Point3d(6, 4, 0));

                acPts3dPMesh.Add(new Point3d(0, 6, 0));
                acPts3dPMesh.Add(new Point3d(2, 6, 1));
                acPts3dPMesh.Add(new Point3d(4, 6, 0));
                acPts3dPMesh.Add(new Point3d(6, 6, 0));

                foreach (Point3d acPt3d in acPts3dPMesh)
                {
                    var acPMeshVer = new PolygonMeshVertex(acPt3d);
                    acPolyMesh.AppendVertex(acPMeshVer);
                    acTrans.AddNewlyCreatedDBObject(acPMeshVer, true);
                }

                // 打开当前视口
                ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                    OpenMode.ForWrite) as ViewportTableRecord;

                // 旋转当前视口的观察方向
                acVportTblRec.ViewDirection = new Vector3d(-1, -1, 1);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // 提交事务
                acTrans.Commit();
            }
        }

        //创建多面网格
        [CommandMethod("CreatePolyfaceMesh")]
        public static void CreatePolyfaceMesh()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建多面网格
                var acPFaceMesh = new PolyFaceMesh();

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acPFaceMesh);
                acTrans.AddNewlyCreatedDBObject(acPFaceMesh, true);

                // 添加顶点前，必须先将多边形网格添加到块表记录
                var acPts3dPFMesh = new Point3dCollection();
                acPts3dPFMesh.Add(new Point3d(4, 7, 0));
                acPts3dPFMesh.Add(new Point3d(5, 7, 0));
                acPts3dPFMesh.Add(new Point3d(6, 7, 0));

                acPts3dPFMesh.Add(new Point3d(4, 6, 0));
                acPts3dPFMesh.Add(new Point3d(5, 6, 0));
                acPts3dPFMesh.Add(new Point3d(6, 6, 1));

                foreach (Point3d acPt3d in acPts3dPFMesh)
                {
                    var acPMeshVer = new PolyFaceMeshVertex(acPt3d);
                    acPFaceMesh.AppendVertex(acPMeshVer);
                    acTrans.AddNewlyCreatedDBObject(acPMeshVer, true);
                }

                // FaceRecord constructor initializes the FaceRecord to use the mesh vertices specified by the indices vertex0 through vertex3 as the four corner points of the face. The vertex indices start with 1. Negative index numbers can be used to indicate that the edge that starts at that vertex is to be invisible.
                // If the face this FaceRecord is to represent has only three vertices, then set vertex3 to 0.
                FaceRecord acFaceRec1 = new FaceRecord(1, 4, 5, 2);
                acPFaceMesh.AppendFaceRecord(acFaceRec1);
                acTrans.AddNewlyCreatedDBObject(acFaceRec1, true);

                var acFaceRec2 = new FaceRecord(2, 5, 6, 0);
                acFaceRec2.MakeEdgeInvisibleAt(3); //设置第4个顶点开始的边不可见
                acPFaceMesh.AppendFaceRecord(acFaceRec2);
                acTrans.AddNewlyCreatedDBObject(acFaceRec2, true);

                // 打开当前视口
                ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                    OpenMode.ForWrite) as ViewportTableRecord;

                // 旋转当前视口的观察方向
                acVportTblRec.ViewDirection = new Vector3d(-1, -1, 1);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // 提交事务
                acTrans.Commit();
            }
        }
        
        /// <summary> 创建一个四面体的网格，并转换为实体 </summary>
        [CommandMethod("CreateSubDMesh")]
        public static void CreateSubDMesh()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord btr = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                var subDMesh = new SubDMesh();
                // 将新对象添加到块表记录和事务
                btr.AppendEntity(subDMesh);
                acTrans.AddNewlyCreatedDBObject(subDMesh, true);

                // 添加顶点前，必须先将多边形网格添加到块表记录
                var vertexArr = new Point3dCollection
                {
                    new Point3d(-1, -1, 0),
                    new Point3d(1, -1, 0),
                    new Point3d(0, 1, 0),
                    new Point3d(0, 0, 1)
                };

                // 根据顶点信息构建三棱锥网格中的四个面
                var faceIndices = new int[]
                {
                    3, 0, 1, 2,   // 最前面的 3 表示这个面所包含的顶点的个数，后面的“0,1,2”表示对应数量的顶点在 indexArray 集合中的位置（第一个元素下标为0）。
                    3, 0, 1, 3,
                    3, 0, 2, 3,
                    3, 1, 2, 3
                };
                var faceArray = new Int32Collection(faceIndices);
                // SubDMesh 对象（或者Surface、Solid3D、PolygonMesh等）的创建，大致都是先构造一个空的实例，然后通过 Set***()方法向其中添加成员。
                subDMesh.SetSubDMesh(vertexArray: vertexArr, indexArray: faceArray, smoothLevel: 0);

                //// 将网格对象直接转换为实体对象
                //Solid3d solid = subDMesh.ConvertToSolid(convertAsSmooth: false, optimize: false);
                //btr.AppendEntity(solid); // 将新对象添加到块表记录和事务
                //acTrans.AddNewlyCreatedDBObject(solid, true);

                //// 删除原来的网格（如果不删除，则网格对象与实体对象会同时存在）
                //subDMesh.Erase(erasing: true);

                // 提交事务
                acTrans.Commit();
            }
        }
        
        // 创建楔形实体
        [CommandMethod("CreateWedge")]
        public static void CreateWedge()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                //创建3D楔形实体wedge
                var acSol3D = new Solid3d();
                acSol3D.CreateWedge(10, 15, 20);

                // 3D实体的中心点放在(5,5,0) 
                acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) - Point3d.Origin));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3D);
                acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                // 打开当前视口
                ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                    OpenMode.ForWrite) as ViewportTableRecord;

                // 旋转当前视口的观察方向
                acVportTblRec.ViewDirection = new Vector3d(-1, -1, 1);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // 提交事务
                acTrans.Commit();
            }
        }
        
        // 旋转3D箱体
        [CommandMethod("Rotate_3DBox")]
        public static void Rotate_3DBox()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建一个3D实体箱子
                var acSol3D = new Solid3d();
                acSol3D.CreateBox(5, 7, 10);

                // 3D实体的中心点放在(5,5,0)
                acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) - Point3d.Origin));

                var curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
                var curUCS = curUCSMatrix.CoordinateSystem3d;

                // 将3D箱体绕点(-3,4,0)和点(-3,-4,0)定义的轴旋转30度
                var vRot = new Point3d(-3, 4, 0).
                    GetVectorTo(new Point3d(-3, -4, 0));

                acSol3D.TransformBy(Matrix3d.Rotation(0.5236, vRot, new Point3d(-3, 4, 0)));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3D);
                acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                // 提交事务
                acTrans.Commit();
            }
        }

        //三维阵列对象
        private static Point2d PolarPoints(Point2d pPt, double dAng, double dDist)
        {
            return new Point2d(pPt.X + dDist * Math.Cos(dAng),
                pPt.Y + dDist * Math.Sin(dAng));
        }

        [CommandMethod("CreateRectangular3DArray")]
        public static void CreateRectangular3DArray()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建圆，圆心(2,2,0)，半径0.5
                var acCirc = new Circle();
                acCirc.Center = new Point3d(2, 2, 0);
                acCirc.Radius = 0.5;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acCirc);
                acTrans.AddNewlyCreatedDBObject(acCirc, true);

                // 创建4行4列3层的矩形阵列
                var nRows = 4;
                var nColumns = 4;
                var nLevels = 3;

                // 设置行、列、层3个方向的偏移量及阵列基角
                double dRowOffset = 1;
                double dColumnOffset = 1;
                double dLevelsOffset = 4;
                double dArrayAng = 0;

                // 获取当前UCS坐标系X轴的角度 
                var curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
                var curUCS = curUCSMatrix.CoordinateSystem3d;
                var acVec2dAng = new Vector2d(curUCS.Xaxis.X,
                    curUCS.Xaxis.Y);

                // 如果UCS被旋转了，相应地调整阵列角度
                dArrayAng = dArrayAng + acVec2dAng.Angle;

                // 使用对象界限的左上角作为阵列基点
                var acExts = acCirc.Bounds.GetValueOrDefault();
                var acPt2dArrayBase = new Point2d(acExts.MinPoint.X,
                    acExts.MaxPoint.Y);

                // 跟踪为每列创建的对象
                var acDBObjCollCols = new DBObjectCollection();
                acDBObjCollCols.Add(acCirc);

                // 创建首列对象
                var nColumnsCount = 1;
                while (nColumns > nColumnsCount)
                {
                    var acEntClone = acCirc.Clone() as Entity;
                    acDBObjCollCols.Add(acEntClone);

                    // 给复制的对象计算新位置
                    var acPt2dTo = PolarPoints(acPt2dArrayBase,
                        dArrayAng,
                        dColumnOffset * nColumnsCount);

                    var acVec2d = acPt2dArrayBase.GetVectorTo(acPt2dTo);
                    var acVec3d = new Vector3d(acVec2d.X, acVec2d.Y, 0);
                    acEntClone.TransformBy(Matrix3d.Displacement(acVec3d));

                    acBlkTblRec.AppendEntity(acEntClone);
                    acTrans.AddNewlyCreatedDBObject(acEntClone, true);

                    nColumnsCount = nColumnsCount + 1;
                }

                // 90度的弧度值
                var dAng = 1.5708;

                // 跟踪每行和列中创建的对象
                var acDBObjCollLvls = new DBObjectCollection();

                foreach (DBObject acObj in acDBObjCollCols)
                {
                    acDBObjCollLvls.Add(acObj);
                }

                // 创建每行对象
                foreach (Entity acEnt in acDBObjCollCols)
                {
                    var nRowsCount = 1;

                    while (nRows > nRowsCount)
                    {
                        var acEntClone = acEnt.Clone() as Entity;
                        acDBObjCollLvls.Add(acEntClone);

                        // 给复制的对象计算新位置
                        var acPt2dTo = PolarPoints(acPt2dArrayBase,
                            dArrayAng + dAng,
                            dRowOffset * nRowsCount);

                        var acVec2d = acPt2dArrayBase.GetVectorTo(acPt2dTo);
                        var acVec3d = new Vector3d(acVec2d.X, acVec2d.Y, 0);
                        acEntClone.TransformBy(Matrix3d.Displacement(acVec3d));

                        acBlkTblRec.AppendEntity(acEntClone);
                        acTrans.AddNewlyCreatedDBObject(acEntClone, true);

                        nRowsCount = nRowsCount + 1;
                    }
                }

                // 创建3D阵列的层
                foreach (Entity acEnt in acDBObjCollLvls)
                {
                    var nLvlsCount = 1;

                    while (nLevels > nLvlsCount)
                    {
                        var acEntClone = acEnt.Clone() as Entity;

                        var acVec3d = new Vector3d(0, 0, dLevelsOffset * nLvlsCount);
                        acEntClone.TransformBy(Matrix3d.Displacement(acVec3d));

                        acBlkTblRec.AppendEntity(acEntClone);
                        acTrans.AddNewlyCreatedDBObject(acEntClone, true);

                        nLvlsCount = nLvlsCount + 1;
                    }
                }

                // 提交事务
                acTrans.Commit();
            }
        }
        
        //在3D空间镜像对象
        [CommandMethod("MirrorABox3D")]
        public static void MirrorABox3D()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建3D箱体
                var acSol3D = new Solid3d();
                acSol3D.CreateBox(5, 7, 10);

                // 3D实体的中心点放在(5,5,0)
                acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) -
                                                          Point3d.Origin));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3D);
                acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                // 创建原3D箱体的拷贝并修改颜色
                var acSol3DCopy = acSol3D.Clone() as Solid3d;
                acSol3DCopy.ColorIndex = 1;

                // 定义镜像平面
                var acPlane = new Plane(new Point3d(1.25, 0, 0),
                    new Point3d(1.25, 2, 0),
                    new Point3d(1.25, 2, 2));

                // 沿平面镜像3D实体
                acSol3DCopy.TransformBy(Matrix3d.Mirroring(acPlane));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3DCopy);
                acTrans.AddNewlyCreatedDBObject(acSol3DCopy, true);

                // 提交事务
                acTrans.Commit();
            }
        }
        
        //求两实体间的干涉
        [CommandMethod("FindInterferenceBetweenSolids")]
        public static void FindInterferenceBetweenSolids()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建3D箱体
                var acSol3DBox = new Solid3d();
                acSol3DBox.CreateBox(5, 7, 10);
                acSol3DBox.ColorIndex = 7;

                // 3D实体的中心点放在(5,5,0) 
                acSol3DBox.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) -
                                                             Point3d.Origin));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3DBox);
                acTrans.AddNewlyCreatedDBObject(acSol3DBox, true);

                // 创建3D圆柱体
                // 默认构造函数的中心点为(0,0,0)
                var acSol3DCyl = new Solid3d();
                acSol3DCyl.CreateFrustum(20, 5, 5, 5);
                acSol3DCyl.ColorIndex = 4;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3DCyl);
                acTrans.AddNewlyCreatedDBObject(acSol3DCyl, true);

                // 用箱体和圆柱体的干涉创建一个3D实体
                var acSol3DCopy = acSol3DCyl.Clone() as Solid3d;

                // 检查箱体和圆柱体是否有重叠部分
                if (acSol3DCopy.CheckInterference(acSol3DBox))
                {
                    acSol3DCopy.BooleanOperation(BooleanOperationType.BoolIntersect,
                        acSol3DBox.Clone() as Solid3d);
                    acSol3DCopy.ColorIndex = 1;
                }

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3DCopy);
                acTrans.AddNewlyCreatedDBObject(acSol3DCopy, true);

                // 提交事务
                acTrans.Commit();
            }
        }
        
        //将一个实体切割成两个实体
        [CommandMethod("SliceABox")]
        public static void SliceABox()
        {
            // 获取当前文档和数据库，启动事务
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开块表
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // 以写模式打开块表记录模型空间
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // 创建3D箱体
                var acSol3D = new Solid3d();
                acSol3D.CreateBox(5, 7, 10);
                acSol3D.ColorIndex = 7;

                // 3D实体的中心点放在(5,5,0) 
                acSol3D.TransformBy(Matrix3d.Displacement(new Point3d(5, 5, 0) -
                                                          Point3d.Origin));

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3D);
                acTrans.AddNewlyCreatedDBObject(acSol3D, true);

                // 定义镜像平面
                var acPlane = new Plane(new Point3d(1.5, 7.5, 0),
                    new Point3d(1.5, 7.5, 10),
                    new Point3d(8.5, 2.5, 10));

                var acSol3DSlice = acSol3D.Slice(acPlane, true);
                acSol3DSlice.ColorIndex = 1;

                // 将新对象添加到块表记录和事务
                acBlkTblRec.AppendEntity(acSol3DSlice);
                acTrans.AddNewlyCreatedDBObject(acSol3DSlice, true);

                // 提交事务
                acTrans.Commit();
            }
        }
    }
}
