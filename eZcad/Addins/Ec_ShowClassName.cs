using System.Collections.Generic;
using System.ComponentModel;
using eZcad.Utility;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Debug;
using Exception = System.Exception;

namespace eZcad.Addins
{
    /// <summary> 显示选择的对象的类型 </summary>
    [EcDescription(CommandDescription)]
    public class ClassNameLister : ICADExCommand
    {

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ShowClassName";
        private const string CommandText = @"对象类型";
        private const string CommandDescription = @"显示选择的对象的类型";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void ShowClassName()
        {
            DocumentModifier.ExecuteCommand(ShowClassName);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ClassNameLister();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.ShowClassName,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        public ExternalCmdResult ShowClassName(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            Editor ed = docMdf.acActiveDocument.Editor;

            // Create our options object
            PromptSelectionOptions pso = new PromptSelectionOptions();

            // Add our keywords
            pso.Keywords.Add("Single");
            pso.Keywords.Add("Multiple");

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择对象 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。

            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            pso.SingleOnly = true;
            pso.SinglePickInSpace = true;

            // 是否一次显示选择的多个对象
            bool showMultiple = false;

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    if (e.Input == "Multiple")
                    {
                        showMultiple = true;
                    }
                    else if (e.Input == "Single")
                    {
                        showMultiple = false;
                    }
                    // ed.WriteMessage("\nKeyword entered: {0}", e.Input);
                };
            bool contin = false;
            do
            {
                contin = GetShow(ed, docMdf.acTransaction, pso, showMultiple: showMultiple);
            } while (contin);

            return ExternalCmdResult.Commit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="tran"></param>
        /// <param name="pso"></param>
        /// <param name="showMultiple">是否一次显示选择的多个对象</param>
        /// <returns></returns>
        private static bool GetShow(Editor ed, Transaction tran, PromptSelectionOptions pso, bool showMultiple)
        {
            // Finally run the selection and show any results
            PromptSelectionResult psr = ed.GetSelection(pso);

            if (psr.Status == PromptStatus.OK)
            {
                var ids = psr.Value.GetObjectIds();
                string msg;
                if (ids.Length > 0)
                {
                    ed.WriteMessage("------------- 选择结果 -------------------\r\n");
                    if (!showMultiple)
                    {
                        var id = ids[0];

                        DBObject obj = tran.GetObject(id, OpenMode.ForRead);
                        msg = $"DxfName: {id.ObjectClass.DxfName}; " +
                              $"\r\nClassName:{id.ObjectClass.Name};" +
                              $"\r\nObjectType: {obj.GetType().FullName}\r\n";
                        //
                        // MessageBox.Show(msg);
                        ed.WriteMessage(msg);
                        if (obj is Curve)
                        {
                            var c = obj as Curve;
                            ed.WriteMessage("\ncorresponding geometric object: " + c.GetGeCurve().GetType().FullName);
                            ed.WriteMessage($"\nstart:{c.StartParam}-{c.StartPoint};\t end:{c.EndParam}-{c.EndPoint}");
                        }
                    }
                    else
                    {
                        foreach (var id in ids)
                        {
                            DBObject obj = tran.GetObject(id, OpenMode.ForRead);
                            msg = $"\r\nDxfName: {id.ObjectClass.DxfName}; " +
                                  $"\r\nClassName:{id.ObjectClass.Name};" +
                                  $"\r\nObjectType: {obj.GetType().FullName}\r\n----------\r\n";
                            //
                            // MessageBox.Show(msg);
                            ed.WriteMessage(msg);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}