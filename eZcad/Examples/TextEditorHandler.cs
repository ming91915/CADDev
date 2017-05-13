using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace eZcad.Examples
{
    /// <summary>
    /// 多行文字编辑器
    /// </summary>
    public class TextEditorHandler
    {
        [CommandMethod("CTU")]
        public void ChangeToUppercase()
        {
            ChangeCase(true);
        }

        [CommandMethod("CTL")]
        public void ChangeToLowercase()
        {
            ChangeCase(false);
        }

        private void ChangeCase(bool upper)
        {
            Document doc =Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Specifically select an MText object

            var peo =new PromptEntityOptions(string.Format("\nSelect MText to change to {0}case: ",upper ? "upper" : "lower"));
            peo.SetRejectMessage("\nObject must be MText.");
            peo.AddAllowedClass(typeof (MText), false);

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK)
                return;

            Transaction tr =doc.TransactionManager.StartTransaction();
            using (tr)
            {
                // We only need our MText open for read

                DBObject obj =tr.GetObject(per.ObjectId, OpenMode.ForRead, false);
                MText mt = obj as MText;
                if (mt == null)
                    return;

                // Create a text editor object for the MText
                TextEditor te = TextEditor.CreateTextEditor(mt);
                
                if (te == null)
                    return;

                // Select the entire contents of the MText

                te.SelectAll();
                TextEditorSelection sel = te.Selection;
                if (sel == null)
                    return;
                
                // Check whether we can change the selection's
                // case, and then do so

                if (sel.CanChangeCase)
                {
                    if (upper)
                        sel.ChangeToUppercase();
                    else
                        sel.ChangeToLowercase();
                }

                // Be sure to save the results from the editor

                te.Close(TextEditor.ExitStatus.ExitSave);
                tr.Commit();
            }
        }
    }
}