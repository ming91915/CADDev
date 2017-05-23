using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(AutoSwitchIME))]

namespace eZcad.Addins
{
    /// <summary> 自动切换输入法 </summary>
    internal class AutoSwitchIME
    {
        #region ---   Fields

        private InputLanguage _textLanguage;
        private InputLanguage _drawingLanguage;

        /// <summary> 所有要设置为中文输入法的操作 </summary>
        private static readonly string[] TextCommands =
        {
            "EATTEDIT", // 编辑块属性 _eattedit
            "TABLEDIT", // 编辑表格中的内容
            "TEXT", // 创建 单行文字
            "MTEXT", // 创建 单行文字
            "DDEDIT", // 编辑 单行文字
            "MTEDIT", // 编辑 多行文字
            "MLEADERCONTENTEDIT", // 编辑 多重引线
            "TEXTEDIT", // 编辑 标注文字
        };
        #endregion

        public AutoSwitchIME()
        {
            SetLanguages();
            //
            Application.DocumentManager.DocumentBecameCurrent += DocumentManagerOnDocumentBecameCurrent;
            Application.DocumentManager.DocumentDestroyed += DocumentManagerOnDocumentDestroyed;
        }

        private void SetLanguages()
        {
            _textLanguage = InputLanguage.FromCulture(CultureInfo.GetCultureInfo("zh-CN"));
            _drawingLanguage = InputLanguage.FromCulture(CultureInfo.GetCultureInfo("en-US"));

            if (_textLanguage == null)
            {
                MessageBox.Show("未找到中文输入法，请检查系统的语言设置（不是键盘设置）。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (_drawingLanguage == null)
            {
                MessageBox.Show("未找到英文输入法，请检查系统的语言设置（不是键盘设置）。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // 整个插件加载时将 AutoCAD 输入法设置为英文
                InputLanguage.CurrentInputLanguage = _drawingLanguage;
            }
        }

        #region ---   AlwaysInEnglishWhenDrawing

        /// <summary> 在除开文字编辑以外的所有情况下，都以英文输入法操作 </summary>
        public static bool AlwaysInEnglishWhenDrawing = true;

        /// <summary> 在除开文字编辑以外的所有情况下，都以英文输入法操作 </summary>
        [CommandMethod("AddinManager", "AlwaysInEnglishWhenDrawing", CommandFlags.Modal)]
        public void SetEnglishWhenDrawing() // This method can have any name
        {
            var engWhenDrawing = new PromptKeywordOptions(
                messageAndKeywords: "\n除文字编辑以外均以英文输入法操作? [是(Y) / 否(N) / 禁用(D)]:",
                globalKeywords: "是 否 禁用");

            engWhenDrawing.AllowNone = true;

            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = editor.GetKeywords(engWhenDrawing);
            if (res.Status == PromptStatus.OK)
            {
                switch (res.StringResult)
                {
                    case "禁用":
                        Enabled = false;
                        break;
                    case "是":
                        Enabled = true;
                        AlwaysInEnglishWhenDrawing = true;
                        break;
                    default:
                        Enabled = true;
                        AlwaysInEnglishWhenDrawing = false;
                        break;
                }
            }
        }

        #endregion

        #region ---   文档的激活、切换、关闭等操作时的 事件绑定

        private List<Document> _bindedDocuments = new List<Document>();

        private void DocumentManagerOnDocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            var doc = e.Document;
            if (!_bindedDocuments.Contains(doc))
            {
                doc.CommandWillStart += DocOnCommandWillStart;
                doc.CommandEnded += DocOnCommandEnded;
                doc.CommandCancelled += DocOnCommandEnded;
                //
                _bindedDocuments.Add(doc);
            }
        }

        private void DocumentManagerOnDocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            // 将失效的文档从集合中删除
            var docsToBeRemove = _bindedDocuments.Where(doc => doc.IsDisposed).ToList();
            //
            foreach (var doc in docsToBeRemove)
            {
                _bindedDocuments.Remove(doc);
            }
        }

        #endregion

        #region ---   命令的启动与结束

        /// <summary> 用户刚执行完文本编辑 </summary>
        private bool _justEditedText;

        /// <summary> 启用此功能 </summary>
        private static bool Enabled = true;

        private void DocOnCommandEnded(object sender, CommandEventArgs e)
        {
            if (Enabled)
            {
                if (AlwaysInEnglishWhenDrawing || _justEditedText)
                {
                    InputLanguage.CurrentInputLanguage = _drawingLanguage;
                }
            }
        }

        private void DocOnCommandWillStart(object sender, CommandEventArgs e)
        {
            //  MessageBox.Show((sender as Document).CommandInProgress + ";" + e.GlobalCommandName);

            if (Enabled)
            {
                if (TextCommands.Contains(e.GlobalCommandName))
                {
                    InputLanguage.CurrentInputLanguage = _textLanguage;
                    _justEditedText = true;
                }
                else
                {
                    _justEditedText = false;
                }
            }
        }

        #endregion
    }
}