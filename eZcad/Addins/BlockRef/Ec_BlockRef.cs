using System;
using System.Collections.Generic;
using eZcad.Utility;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal.DatabaseServices;
using eZcad.AddinManager;

namespace eZcad.Debug
{
    [EcDescription("对块参照进行操作")]
    public class Ec_BlockRef : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage, ref IList<ObjectId> elementSet)
        {
            var dat = new DllActivator_eZcad();
            dat.ActivateReferences();

            using (DocumentModifier docMdf = new DocumentModifier(openDebugerText: true))
            {
                try
                {
                    
                    BlockRefenceTest(docMdf.acDataBase, docMdf.acEditor, docMdf.acTransaction);
                    //
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

        /// <summary> 块参照实例对象 与 块定义对象的 创建、修改 </summary>
        public void BlockRefenceTest(Database db, Editor editor, Transaction trans)
        {
            var line = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 0));
            string newBlockName = "新块1";
            // 1. 创建块定义 ----------------------------------
            ObjectId btrId = CreateBTR(db, newBlockName, new[] {line});

            // 2. 添加一个块参照实例到 AutoCAD 中 ----------------------------------
            // 以只读方式打开块表   Open the Block table for read
            var acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

            // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
            var modelBtr =
                trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                    BlockTableRecord;

            ObjectId blkRef = InsertBlockRef(spaceId: modelBtr.Id, layer: "0", insertedBlockName: newBlockName,
                postion: new Point3d(0, 0, 0), scale: new Scale3d(1, 1, 1), rotateAngle: Math.PI/2);

            // 3. 修改块定义  ----------------------------------
            ModifyBTR(btrId);

            // 4. 选择一个块参照，并修改其对应的块定义的信息  ----------------------------------
            ModifyBlockRef(editor, trans);
        }

        /// <summary> 创建块定义 </summary>
        public static ObjectId CreateBTR(Database db, string blockName, IEnumerable<Entity> ents)
        {
            BlockTable blockTable = (BlockTable) db.BlockTableId.GetObject(OpenMode.ForRead);

            if (!blockTable.Has(blockName))
            {
                var btr = new BlockTableRecord
                {
                    Name = blockName
                };
                //将列表中的实体加入到新建的BlockTableRecord
                foreach (var ent in ents)
                {
                    btr.AppendEntity(ent);
                }

                blockTable.UpgradeOpen(); //切换块为写的状态
                blockTable.Add(btr);
                db.TransactionManager.AddNewlyCreatedDBObject(btr, true);
                blockTable.DowngradeOpen(); //切换块为读的状态
            }
            return blockTable[blockName];
        }

        /// <summary> 插入一个块参照到CAD图形中 </summary>
        /// <param name="spaceId">模型空间Id</param>
        /// <param name="layer">要插入的图层名</param>
        /// <param name="insertedBlockName">块名</param>
        /// <param name="postion">插入点</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns> 返回添加的块参照的ID  </returns>
        public static ObjectId InsertBlockRef(ObjectId spaceId, string layer, string insertedBlockName,
            Point3d postion, Scale3d scale, double rotateAngle)
        {
            ObjectId blockRefId; //存储要插入的块参照的Id

            Database db = spaceId.Database; //获取数据库对象

            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            
            if (!bt.Has(insertedBlockName))

                return ObjectId.Null; //如果没有blockName的块，程序返回

            //以写的方式打开空间
            BlockTableRecord btr = spaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            //创建块参照的插入点
            BlockReference br = new BlockReference(postion, bt[insertedBlockName]);
            br.ScaleFactors = scale;

            br.Layer = layer;

            br.Rotation = rotateAngle;
            blockRefId = btr.AppendEntity(br);

            db.TransactionManager.AddNewlyCreatedDBObject(br, true);

            btr.DowngradeOpen();

            return blockRefId; //返回添加的块参照的ID 
        }

        /// <summary> 修改块定义 </summary>
        private void ModifyBTR(ObjectId btrId)
        {
            // 以写方式打开模型空间块表记录
            var btr = btrId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

            var line = new Line(new Point3d(0, 0, 0), new Point3d(-100, 100, 0));

            // 添加新对象到块表记录和事务中
            btr.AppendEntity(line);
            btr.DowngradeOpen();
            btrId.Database.TransactionManager.AddNewlyCreatedDBObject(line, true);
        }

        /// <summary> 选择一个块参照，并修改其对应的块定义的信息 </summary>
        private void ModifyBlockRef(Editor editor, Transaction trans)
        {
            // 选择一个块参照的实例对象
            BlockReference br = PickBlockRef(editor, trans);

            // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
            var btr = trans.GetObject(br.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord;

            var circle = new Circle(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), 150); // 图形的原点为块定义自身空间坐标系的原点，而不是其所属的模型空间的坐标系。当在块定义中添加了“基点”后，其原点即成为了“基点”所对应的位置。
            
            // 添加新对象到块表记录和事务中   Add the new object to the block table record and the transaction
            btr.AppendEntity(circle);
            btr.DowngradeOpen();
            trans.AddNewlyCreatedDBObject(circle, true);
        }

        /// <summary> 通过点选的方式选择一条曲线 </summary>
        private BlockReference PickBlockRef(Editor editor, Transaction trans)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一个块参照 ");
            peO.SetRejectMessage("\n 请选择一个块参照对象\n");
            peO.AddAllowedClass(typeof (BlockReference), exactMatch: false);

            // 请求在图形区域选择对象
            var res = editor.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                return trans.GetObject(res.ObjectId, OpenMode.ForRead) as BlockReference;
            }
            return null;
        }
    }
}