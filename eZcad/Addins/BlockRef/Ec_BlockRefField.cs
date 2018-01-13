using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using eZcad.Utility;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using Exception = System.Exception;

namespace eZcad.Debug
{
    [EcDescription("修改属性块中某属性的值")]
    public class Ec_BlockRefField : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var dat = new DllActivator_eZcad();
            dat.ActivateReferences();

            using (var docMdf = new DocumentModifier(true))
            {
                try
                {
                    DoSomething(docMdf, impliedSelection);
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

        // 开始具体的调试操作
        private void DoSomething(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var acBlkTbl = docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var modeSpce = docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            var block = docMdf.acTransaction.GetObject(acBlkTbl["A3图框"], OpenMode.ForWrite) as BlockTableRecord;


            // 新建一个块参照实例
            var blkRef = new BlockReference(position: new Point3d(0, 0, 0), blockTableRecord: block.ObjectId);
            var id = modeSpce.AppendEntity(blkRef);
            docMdf.acTransaction.AddNewlyCreatedDBObject(blkRef, true);

            // 初始创建一个块参照时，其中是没有任何的块属性定义的。
            // 除非执行 ATTSYNC 进行块定义中的属性定义的同步，否则 一个块定义的多个块参数实例之间，可以拥有不同的块属性文本。

            // 遍历块记录中的所有实体，并将其中的所有块属性加入块参照实例中  
            foreach (ObjectId idTemp in block)
            {
                // 判断该实体是否是块属性定义
                if (idTemp.ObjectClass.Equals(RXObject.GetClass(typeof(AttributeDefinition))))
                {
                    AttributeDefinition adDef = docMdf.acTransaction.GetObject(idTemp, OpenMode.ForRead) as AttributeDefinition;
                    if (adDef != null)
                    {
                        AttributeReference ar = new AttributeReference(adDef.Position, "哈哈", adDef.Tag, new ObjectId());
                        blkRef.AttributeCollection.AppendAttribute(attributeToAddToBlockReference: ar);
                    }
                }
            }
            // 为这个块参数实例单独添加一个块属性实例
            AttributeReference atd = new AttributeReference(new Point3d(0, 0, 0), "属性value", "属性tag", new ObjectId());
            blkRef.AttributeCollection.AppendAttribute(attributeToAddToBlockReference: atd);

            return;
            var propertyName = "PILENUM";
            var startNum = 16;

            var conti = false;
            do
            {
                startNum += 1;
                conti = SetBlockRefAttibute(docMdf, propertyName, startNum);
            } while (conti);
        }

        private bool SetBlockRefAttibute(DocumentModifier docMdf, string attTag, int tagNum)
        {
            var bkr = PickBlockRef(docMdf);
            if (bkr != null)
            {
                foreach (ObjectId attId in bkr.AttributeCollection)
                {
                    var att = docMdf.acTransaction.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                    if (att != null && att.Tag == attTag)
                    {
                        att.UpgradeOpen();
                        att.TextString = tagNum.ToString();
                        att.DowngradeOpen();

                        // 将修改后的结果在界面中显示
                        att.Draw();
                        // docMdf.acEditor.UpdateScreen();
                        return true;
                    }
                }
            }
            return false;
        }

        private BlockReference PickBlockRef(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一个块 ");
            peO.SetRejectMessage("\n 请选择一个包含有属性的块参照对象\n");
            peO.AddAllowedClass(typeof(BlockReference), false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            BlockReference curve = null;
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                curve = docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as BlockReference;
            }
            else
            {
                return null;
            }
            return curve;
        }
    }
}