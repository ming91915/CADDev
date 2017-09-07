using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.Utility;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    public partial class ModalPForm : Form
    {
        protected DocumentModifier _docMdf;

        #region ---   构造函数

        public ModalPForm()
        {
            InitializeComponent();
            SetTransparancy(false);
        }

        /// <summary> 以非模态、不阻塞的方式显示窗口 </summary>
        public new void Show()
        {
            ShowDialog();
        }

        /// <summary> 以非模态、不阻塞的方式显示窗口 </summary>
        public new DialogResult ShowDialog()
        {
            this.Opacity = 1;
            return Application.ShowModalDialog(null, this);
            // 
        }

        #endregion

        #region ---   界面在失去焦点时透明

        private void SetTransparancy(bool trans)
        {
            if (trans)
            {
                this.ControlAdded += OnControlAdded;
                this.MouseEnter += new System.EventHandler(this.ParameterForm_MouseEnter);
                this.MouseLeave += new System.EventHandler(this.ParameterForm_MouseLeave);
            }
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
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

        #region ---   窗口切换与关闭事件

        private void btn_ViewUI_Click(object sender, EventArgs e)
        {
            ShowACAD();
        }

        /// <summary> 将焦点由本模态窗口转移到 AutoCAD 界面 </summary>
        protected void ShowACAD()
        {
            _docMdf.acEditor.GetEntity("\n查看界面");
        }

        private void btn_CancelCmd_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }

        private void ParameterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        #endregion

        #region ---   执行最终的命令

        private void btn_RunCmd_Click(object sender, EventArgs e)
        {
            OnCmdRun(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="closeWindowWhenFinished">命令执行完成后，是否还要再显示出本窗口</param>
        protected virtual void OnCmdRun(bool closeWindowWhenFinished)
        {
            if (closeWindowWhenFinished)
            {
                //  将本窗口进行隐藏，此方法一般在执行最终的计算操作之前（而不是之后）执行
                Opacity = 0;
                Close();
                DialogResult = DialogResult.OK;
            }
            Utils.FocusOnMainUIWindow();
        }

        #endregion
    }
}