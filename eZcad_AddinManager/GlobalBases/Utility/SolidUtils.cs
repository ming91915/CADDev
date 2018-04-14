using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace eZcad.Utility
{
    /// <summary>
    /// 与实体、曲面、网格相关的操作
    /// </summary>
    public static class SolidUtils
    {
        /// <summary> 将任意实体转换为网格 </summary>
        public static void CreateMeshFromSolid()
        {
            //Select a solid.
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions opts = new PromptEntityOptions("\r\n" + "Select Solid:");
            opts.SetRejectMessage("\r\n" + "That\'s not a solid!");
            opts.AddAllowedClass(typeof(Solid3d), false);
            PromptEntityResult res = ed.GetEntity(opts);

            //Exit sub if user cancelled selection.
            if (res.Status != PromptStatus.OK)
            {
                return;
            }

            //Usual transaction stuff
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Solid3d mySolid = tr.GetObject(res.ObjectId, OpenMode.ForRead, false) as Solid3d;
                Extents3d ext = mySolid.Bounds.Value;
                Vector3d vec = ext.MaxPoint - ext.MinPoint;

                // 实体转换为网格的生成算法，即平滑或插值的参数
                //Define params governing mesh generation algorithm(See ObjectARX helpfiles for explanation of params you may need to change them depending on the scale of the solid)
                MeshFaceterData myFaceterData = new MeshFaceterData(0.01 * vec.Length, 40 * Math.PI / 180, 2, 2, 15, 5, 5, 0);

                //Create new mesh from solid (smoothing level 1)
                MeshDataCollection meshData = SubDMesh.GetObjectMesh(mySolid, myFaceterData);
                SubDMesh myMesh = new SubDMesh();
                myMesh.SetSubDMesh(meshData.VertexArray, meshData.FaceArray, 1);

                //Add mesh to database. (Don't remove solid).
                myMesh.SetDatabaseDefaults();
                var btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(myMesh);
                tr.AddNewlyCreatedDBObject(myMesh, true);

                //Our work here is done
                tr.Commit();
            }
        }

        /// <summary> 将多面网格转换为细分网格 </summary>
        public static SubDMesh ConvertToSubDMesh(this PolyFaceMesh pfMesh)
        {
            var vertexArr = new Point3d[pfMesh.NumVertices];  // 多面网格中的所有顶点集合
            var faceArr = new List<int>();  // 多面网格中的所有面集合
            var vv = pfMesh.Cast<ObjectId>().ToArray();
            for (int i = 0; i < pfMesh.NumVertices; i++)
            {
                var v = vv[i].GetObject(OpenMode.ForRead) as PolyFaceMeshVertex;
                vertexArr[i] = v.Position;
            }
            for (int i = pfMesh.NumVertices; i < vv.Length; i++)
            {
                var f = vv[i].GetObject(OpenMode.ForRead) as FaceRecord;
                var lastVertice = f.GetVertexAt(3);

                // Face corner indices start with 0, so faceIndex must be 0, 1, 2, or 3.PolyFaceMesh vertex indices start with 1, and will be negative if the edge is invisible or positive if the edge is visible.
                if (lastVertice == 0) // indicates that face is a trangular face.
                {
                    faceArr.AddRange(new[] { 3,
                            Math.Abs(f.GetVertexAt(0)) - 1,
                            Math.Abs(f.GetVertexAt(1)) - 1,
                            Math.Abs(f.GetVertexAt(2)) - 1 });
                }
                else
                {
                    faceArr.AddRange(new[] { 4,
                            Math.Abs(f.GetVertexAt(0))-1,
                            Math.Abs(f.GetVertexAt(1))-1,
                            Math.Abs(f.GetVertexAt(2))-1,
                            Math.Abs(lastVertice)-1});
                }
            }

            var subDMesh = new SubDMesh();

            // SubDMesh 对象（或者Surface、Solid3D、PolygonMesh等）的创建，大致都是先构造一个空的实例，然后通过 Set***()方法向其中添加成员。
            subDMesh.SetSubDMesh(
                vertexArray: new Point3dCollection(vertexArr),
                indexArray: new Int32Collection(faceArr.ToArray()),
                smoothLevel: 0);
            return subDMesh;
        }

        /// <summary> 将多面网格转换为细分网格 </summary>
        public static PolyFaceMesh ConvertToPolyFaceMesh(this SubDMesh subDMesh, BlockTableRecord btr, Transaction trans)
        {

            // 创建多面网格
            var acPFaceMesh = new PolyFaceMesh();

            // 将新对象添加到块表记录和事务
            btr.AppendEntity(acPFaceMesh);
            trans.AddNewlyCreatedDBObject(acPFaceMesh, true);
            // 添加顶点前，必须先将多边形网格添加到块表记录

            foreach (Point3d acPt3d in subDMesh.Vertices)
            {
                var acPMeshVer = new PolyFaceMeshVertex(acPt3d);
                acPFaceMesh.AppendVertex(acPMeshVer);
                trans.AddNewlyCreatedDBObject(acPMeshVer, true);
            }

            // FaceRecord constructor initializes the FaceRecord to use the mesh vertices specified by the indices vertex0 through vertex3 as the four corner points of the face. The vertex indices start with 1. Negative index numbers can be used to indicate that the edge that starts at that vertex is to be invisible.
            // If the face this FaceRecord is to represent has only three vertices, then set vertex3 to 0.
            var faceVertices = subDMesh.FaceArray.ToArray();
            var anchorIndex = 0;
            FaceRecord faceRec;
            while (anchorIndex < faceVertices.Length)
            {
                var verCount = faceVertices[anchorIndex];
                if (verCount == 3)
                {
                    // 构造一个三角面
                    faceRec = new FaceRecord(
                        (short)(faceVertices[anchorIndex + 1] + 1),
                        (short)(faceVertices[anchorIndex + 2] + 1),
                        (short)(faceVertices[anchorIndex + 3] + 1),
                        0);
                }
                else if (verCount == 4)
                {
                    // 构造一个四边形的面
                    faceRec = new FaceRecord(
                       (short)(faceVertices[anchorIndex + 1] + 1),
                       (short)(faceVertices[anchorIndex + 2] + 1),
                       (short)(faceVertices[anchorIndex + 3] + 1),
                       (short)(faceVertices[anchorIndex + 4] + 1));
                }
                else
                {
                    throw new InvalidOperationException("网格中只能有三角形或者四边形");
                }
                // 添加到数据库中
                acPFaceMesh.AppendFaceRecord(faceRec);
                trans.AddNewlyCreatedDBObject(faceRec, true);
                // 下一个面的集合下标
                anchorIndex += verCount + 1;
            }
            return acPFaceMesh;
        }
    }
}

