using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;

namespace eZcad.Debug
{
    public class eZcadAddinManagerDebuger
    {
        public static ExternalCommandResult DebugInAddinManager(ExternalCommand cmd,
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

                    var canCommit = cmd(docMdf, impliedSelection);
                    //
                    switch (canCommit)
                    {
                        case ExternalCmdResult.Commit:
                            docMdf.acTransaction.Commit();
                            return ExternalCommandResult.Succeeded;
                            break;
                        case ExternalCmdResult.Cancel:
                            docMdf.acTransaction.Abort();
                            return ExternalCommandResult.Cancelled;
                            break;
                        default:
                            docMdf.acTransaction.Abort();
                            return ExternalCommandResult.Cancelled;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    docMdf.acTransaction.Abort(); // Abort the transaction and rollback to the previous state
                    errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace;
                    return ExternalCommandResult.Failed;
                }
            }
        }
    }
}