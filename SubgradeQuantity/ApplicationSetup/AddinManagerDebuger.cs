using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;

namespace eZcad.SubgradeQuantity
{
    public class AddinManagerDebuger
    {
        public static ExternalCommandResult DebugInAddinManager(DocumentModifier.ExternalCommand cmd,
            SelectionSet impliedSelection, ref string errorMessage, ref IList<ObjectId> elementSet)
        {
            var dat = new DllActivator_eZcad();
            dat.ActivateReferences();

            using (var docMdf = new DocumentModifier(true))
            {
                try
                {
                    // 先换个行，显示效果更清爽
                    docMdf.WriteNow("\n");

                    cmd(docMdf, impliedSelection);
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

        /// <summary> 在界面中选择一个对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ed"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T PickObject<T>(Editor ed, string message = "选择一个对象") where T : Entity
        {
            var op = new PromptEntityOptions(message);
            op.SetRejectMessage($"请选择一个 {typeof(T).FullName} 对象");
            op.AddAllowedClass(typeof(T), exactMatch: false);
            var res = ed.GetEntity(op);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId.GetObject(OpenMode.ForRead) as T;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 具体的高度操作的代码模板
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="impliedSelection"></param>
        private void DoSomethingTemplate(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var obj = AddinManagerDebuger.PickObject<Entity>(docMdf.acEditor);
            if (obj != null)
            {
                var blkTb =
                    docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr =
                    docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                var ent = new DBText();

                // 将新对象添加到块表记录和事务
                btr.AppendEntity(ent);
                docMdf.acTransaction.AddNewlyCreatedDBObject(ent, true);
            }
        }
    }
}