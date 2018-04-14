using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad;
using eZcad.AddinManager;
using eZcad.Addins.HaveATry;
using eZcad.Utility;

namespace eZcad.Debug
{
    [EcDescription("将每一个通知单或变更单绘制在CAD中")]
    public class Ec_intersectionFinder : ICADExCommand
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
            var intSectionFinder = new IntersectionFinder();
            intSectionFinder.Execute(docMdf);
        }
    }
}