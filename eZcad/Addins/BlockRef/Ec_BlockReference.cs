using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.Utility;

namespace eZcad.Debug
{
    [EcDescription("将纬地挡墙模板中的各种块的文字属性的样式格式进行修改")]
    public class Ec_BlockReference : ICADExCommand
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
            // ---------------------------------------------------------------
            var e = AddinManagerDebuger.PickObject<BlockReference>(docMdf.acEditor);
            var atts = e.AttributeCollection;
            var btr = e.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            var ents = btr.Cast<ObjectId>();
            var attDefs = ents.Select(r=>r.GetObject(OpenMode.ForRead)).OfType<AttributeDefinition>().ToArray();
            MessageBox.Show(attDefs.Count().ToString());

            btr.DowngradeOpen();

            // ---------------------------------------------------------------
            var st = EditStateIdentifier.GetCurrentEditState(_docMdf);
            st.CurrentBTR.UpgradeOpen();

            st.CurrentBTR.DowngradeOpen();

            // ---------------------------------------------------------------
            var dimStyles = docMdf.acTransaction.GetObject
               (docMdf.acDataBase.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            foreach (var dimStyleId in dimStyles)
            {
                var dimStyle = docMdf.acTransaction.GetObject(dimStyleId, OpenMode.ForWrite) as DimStyleTableRecord;
                dimStyle.Dimadec = 0;
                if (dimStyle.Name.Contains("m"))
                {
                    // 修改箭头大小
                    dimStyle.Dimdec = 3;
                }
                else
                {
                    dimStyle.Dimdec = 0;
                }
            }
            return;
        }
    }
}