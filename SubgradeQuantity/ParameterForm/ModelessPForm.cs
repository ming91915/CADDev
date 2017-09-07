using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    public partial class ModelessPForm : Form
    {
        #region ---   Fields

        private DocumentModifier _docMdf;

        protected DocumentModifier DocMdf
        {
            get
            {
                if (_docMdf == null)
                {
                    StartCommand();
                }
                return _docMdf;
            }
            private set { _docMdf = value; }
        }

        //protected SelectionSet ImpliedSelection { get; private set; }

        #endregion

        #region ---   构造函数

        public ModelessPForm()
        {
            InitializeComponent();
            //

        }

        /// <summary> 以非模态、不阻塞的方式显示窗口 </summary>
        public new void Show()
        {
            Application.ShowModelessDialog(null, this);
            // 
            Application.DocumentManager.MdiActiveDocument.CommandWillStart += MdiActiveDocumentOnCommandWillStart;

            StartCommand();
        }

        #endregion

        #region ---   用户强行切换到其他命令

        private void MdiActiveDocumentOnCommandWillStart(object sender, CommandEventArgs commandEventArgs)
        {
            Close();
            CanceleCommand();
        }

        #endregion
        #region ---   外部命令的执行

        protected delegate void CommandRuner(object sender, EventArgs e);

        private bool _uiGetCommandRunning = false;

        protected void ControlRunCmd_UIGet(CommandRuner uiGetCmd, Control sender, EventArgs eArgs)
        {
            if (!_uiGetCommandRunning)
            {
                _uiGetCommandRunning = true;
                ControlRunCmd(uiGetCmd, sender, eArgs);
                _uiGetCommandRunning = false;
            }
        }

        protected void ControlRunCmd(CommandRuner uiGetCmd, Control sender, EventArgs eArgs)
        {
            if (sender != null)
            {
                sender.Enabled = false;
                uiGetCmd.Invoke(sender, eArgs);
                sender.Enabled = true;
            }
            else
            {
                uiGetCmd.Invoke(sender, eArgs);
            }
        }

        #endregion

        #region ---   外部命令（事务）的启动与提交

        private bool _transactionStarted = false;
        private bool _transactionTobeCanceled = false;

        protected void StartCommand()
        {
            if (!_transactionStarted)
            {
                MessageBox.Show("启动事务");
                _docMdf = new DocumentModifier(openDebugerText: true);
                var ImpliedSelection = DocMdf.acEditor.SelectImplied().Value;
                _transactionStarted = true;
            }
        }

        protected void CommitCommand()
        {
            if (_transactionTobeCanceled)
            {
                CanceleCommand();
            }
            else
            {
                if (DocMdf != null && DocMdf.acTransaction != null && !DocMdf.acTransaction.IsDisposed)
                {
                    try
                    {
                        MessageBox.Show("提交事务");
                        DocMdf.acTransaction.Commit();
                        DocMdf.Dispose();
                        DocMdf = null;
                        _transactionStarted = false;
                    }
                    catch (Exception ex)
                    {
                        DocMdf.acTransaction.Abort(); // Abort the transaction and rollback to the previous state
                        string errorMessage = ex.Message + "\r\n\r\n" + ex.StackTrace;
                        MessageBox.Show(errorMessage, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                CommandFinished();
            }
        }

        protected void CanceleCommand()
        {
            if (_transactionStarted)
            {
                if (DocMdf != null && DocMdf.acTransaction != null && !DocMdf.acTransaction.IsDisposed)
                {
                    MessageBox.Show("取消事务");
                    DocMdf.acTransaction.Abort();
                    DocMdf.Dispose();
                    DocMdf = null;
                    _transactionStarted = false;
                }
            }
            CommandFinished();
        }

        private void CommandFinished()
        {
            Application.DocumentManager.MdiActiveDocument.CommandWillStart -= MdiActiveDocumentOnCommandWillStart;
        }

        #endregion

        #region ---   鼠标移动事件响应

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.MouseMove += ControlOnMouseeMove;
        }

        private void ControlOnMouseeMove(object sender, EventArgs eventArgs)
        {
            ShowWindows();
        }

        private void ParameterForm_MouseEnter(object sender, EventArgs e)
        {
            ShowWindows();
        }

        private void ParameterForm_MouseLeave(object sender, EventArgs e)
        {
            HideWindows();
        }

        private void ShowWindows()
        {
            if (Opacity != 1)
            {
                Opacity = 1;
            }
        }

        private void HideWindows()
        {
            if (Opacity != 0.75)
            {
                Opacity = 0.75;
            }
        }

        #endregion

        #region ---   窗口关闭事件

        private void ParameterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            if (_uiGetCommandRunning)
            {
                // 模拟按下 ESC 以强行结束 editor.GetSelection() 等命令
                var doc = Application.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute(((char)27).ToString(), true, wrapUpInactiveDoc: false, echoCommand: true);
            }
        }

        private void ParameterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DocMdf != null && DocMdf.acTransaction != null && !DocMdf.acTransaction.IsDisposed)
            {
                //CanceleCommand();
                DocMdf.Dispose();
            }
        }

        private void ParameterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        #endregion
    }
}