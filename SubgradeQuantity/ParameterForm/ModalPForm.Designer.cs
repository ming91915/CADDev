namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class ModalPForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btn_RunCmd = new System.Windows.Forms.Button();
            this.btn_CancelCmd = new System.Windows.Forms.Button();
            this.btn_ViewUI = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btn_RunCmd
            // 
            this.btn_RunCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RunCmd.Location = new System.Drawing.Point(285, 277);
            this.btn_RunCmd.Margin = new System.Windows.Forms.Padding(2);
            this.btn_RunCmd.Name = "btn_RunCmd";
            this.btn_RunCmd.Size = new System.Drawing.Size(38, 23);
            this.btn_RunCmd.TabIndex = 0;
            this.btn_RunCmd.Text = "执行";
            this.btn_RunCmd.UseVisualStyleBackColor = true;
            this.btn_RunCmd.Click += new System.EventHandler(this.btn_RunCmd_Click);
            // 
            // btn_CancelCmd
            // 
            this.btn_CancelCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_CancelCmd.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_CancelCmd.Location = new System.Drawing.Point(243, 277);
            this.btn_CancelCmd.Margin = new System.Windows.Forms.Padding(2);
            this.btn_CancelCmd.Name = "btn_CancelCmd";
            this.btn_CancelCmd.Size = new System.Drawing.Size(38, 23);
            this.btn_CancelCmd.TabIndex = 1;
            this.btn_CancelCmd.Text = "取消";
            this.btn_CancelCmd.UseVisualStyleBackColor = true;
            this.btn_CancelCmd.Click += new System.EventHandler(this.btn_CancelCmd_Click);
            // 
            // btn_ViewUI
            // 
            this.btn_ViewUI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ViewUI.Location = new System.Drawing.Point(201, 277);
            this.btn_ViewUI.Margin = new System.Windows.Forms.Padding(2);
            this.btn_ViewUI.Name = "btn_ViewUI";
            this.btn_ViewUI.Size = new System.Drawing.Size(38, 23);
            this.btn_ViewUI.TabIndex = 2;
            this.btn_ViewUI.Text = "查看";
            this.toolTip1.SetToolTip(this.btn_ViewUI, "将焦点转移到 AutoCAD 主界面");
            this.btn_ViewUI.UseVisualStyleBackColor = true;
            this.btn_ViewUI.Click += new System.EventHandler(this.btn_ViewUI_Click);
            // 
            // ModalPForm
            // 
            this.AcceptButton = this.btn_RunCmd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_CancelCmd;
            this.ClientSize = new System.Drawing.Size(334, 311);
            this.Controls.Add(this.btn_ViewUI);
            this.Controls.Add(this.btn_CancelCmd);
            this.Controls.Add(this.btn_RunCmd);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimizeBox = false;
            this.Name = "ModalPForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ParameterForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterForm_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        protected System.Windows.Forms.Button btn_RunCmd;
        protected System.Windows.Forms.Button btn_CancelCmd;
        protected System.Windows.Forms.Button btn_ViewUI;
    }
}