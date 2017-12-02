using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.Debug;
using eZcad.Utility;

namespace eZcad.Addins
{
    [EcDescription("数值递增")]
    public class NumAdd : ICADExCommand
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
            var srcTxt = AddinManagerDebuger.PickObject<DBText>(docMdf.acEditor, message: "选择一个包含有整数值的单行文字");
            if (srcTxt == null) return;
            var srcStr = srcTxt.TextString;
            string prefix;
            double num;
            double increment;
            string suffix;
            var succ = GetPrefixAndValue(srcStr, out prefix, out num, out suffix);
            //
            var st = EditStateIdentifier.GetCurrentEditState(_docMdf);
            st.CurrentBTR.UpgradeOpen();

            if (succ)
            {
                increment = GetIncrement(_docMdf.acEditor);
                //
                DBText txt = null;
                var conti = GetDbText(_docMdf.acEditor, out txt);
                while (txt != null)
                {
                    num += increment;
                    //
                    txt.UpgradeOpen();
                    txt.TextString = prefix + num.ToString() + suffix;
                    txt.DowngradeOpen();
                    txt.Draw();
                    //
                    conti = GetDbText(_docMdf.acEditor, out txt);
                }
            }
            else
            {
                // 只起到复制的功能
                //
                DBText txt = null;
                var conti = GetDbText(_docMdf.acEditor, out txt);
                while (txt != null)
                {
                    //
                    txt.UpgradeOpen();
                    txt.TextString = srcStr;
                    txt.DowngradeOpen();
                    txt.Draw();
                    //
                    conti = GetDbText(_docMdf.acEditor, out txt);
                }
            }
            st.CurrentBTR.DowngradeOpen();
        }

        private static readonly Regex reg = new Regex(@"\d+");

        private bool GetPrefixAndValue(string txt, out string prefix, out double num, out string suffix)
        {
            prefix = "";
            suffix = "";
            num = 1;
            var ms = reg.Matches(txt);
            if (ms.Count > 0)
            {
                var m = ms[ms.Count - 1];
                prefix = txt.Substring(0, m.Index);
                num = double.Parse(m.Value);
                //
                var endIndex = m.Index + m.Length;
                suffix = txt.Substring(endIndex, txt.Length - endIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 在命令行中获取一个小数值 </summary>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private double GetIncrement(Editor ed)
        {
            double value = 1;
            var op = new PromptDoubleOptions(message: "\n设置增量数值")
            {
                AllowNegative = true,
                AllowNone = true,
                AllowZero = true,
                AllowArbitraryInput = false,
                DefaultValue = 1
            };
            //
            var res = ed.GetDouble(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.Value;
                return value;
            }
            return value;
        }

        private bool GetDbText(Editor ed, out DBText txt)
        {
            bool conti = false;
            txt = null;
            // 点选
            var peO = new PromptEntityOptions("\n 选择一个单行文字 ");
            peO.SetRejectMessage("\n 请选择一个单行文字\n");
            peO.AddAllowedClass(typeof (DBText), exactMatch: false);

            // 请求在图形区域选择对象
            var res = ed.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                txt = res.ObjectId.GetObject(OpenMode.ForRead) as DBText;
                conti = true;
            }
            return conti;
        }
    }
}