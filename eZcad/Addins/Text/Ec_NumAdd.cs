using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.Debug;
using eZcad.Utility;

namespace eZcad.Addins.Text
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
            object srcTxt = null;
            var conti = GetText(_docMdf.acEditor, out srcTxt);
            if (!conti) return;
            string srcStr = GetText(srcTxt); ;
            if (srcStr == null) return;

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
                // txt 为 单行文字 或者 多选文字 对象
                object txt = null;
                conti = GetText(_docMdf.acEditor, out txt);
                while (txt != null)
                {
                    num += increment;
                    var newText = prefix + num.ToString() + suffix;
                    RefreshText(txt, newText);
                    //
                    conti = GetText(_docMdf.acEditor, out txt);
                }
            }
            else
            {
                // 只起到复制的功能
                //
                object txt = null;
                conti = GetText(_docMdf.acEditor, out txt);
                while (txt != null)
                {
                    RefreshText(txt, srcStr);
                    //
                    conti = GetText(_docMdf.acEditor, out txt);
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

        /// <summary> 修改单行或者多行文字的字符值 </summary>
        /// <param name="txt"></param>
        /// <param name="newText"></param>
        private void RefreshText(object txt, string newText)
        {
            if (txt is DBText)
            {
                var dTxt = txt as DBText;
                dTxt.UpgradeOpen();
                dTxt.TextString = newText;
                dTxt.DowngradeOpen();
                dTxt.Draw();
            }
            else if (txt is MText)
            {
                var mTxt = txt as MText;
                mTxt.UpgradeOpen();
                mTxt.Contents = newText;
                mTxt.DowngradeOpen();
                mTxt.Draw();
            }
        }
        /// <summary> 提取单行或者多行文字的字符值 </summary>
        /// <param name="txt"></param>
        private string GetText(object txt)
        {
            if (txt is DBText)
            {
                return ((DBText)txt).TextString;
            }
            else if (txt is MText)
            {
                return ((MText)txt).Text;
            }
            return null;
        }

        #region ---   界面操作

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

        /// <summary> 在界面中选择一个单行或者多行文字 </summary>
        /// <param name="ed"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        private bool GetText(Editor ed, out object txt)
        {
            bool conti = false;
            txt = null;
            // 点选
            var peO = new PromptEntityOptions("\n 选择一个单行或多行文字文字 ");
            peO.SetRejectMessage("\n 请选择一个单行或多行文字文字\n");
            peO.AddAllowedClass(typeof(DBText), exactMatch: false);
            peO.AddAllowedClass(typeof(MText), exactMatch: false);

            // 请求在图形区域选择对象
            var res = ed.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                txt = res.ObjectId.GetObject(OpenMode.ForRead);
                conti = true;
            }
            return conti;
        }
        #endregion
    }
}