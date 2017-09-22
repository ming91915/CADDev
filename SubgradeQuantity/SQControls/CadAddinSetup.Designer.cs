namespace eZcad.SubgradeQuantity
{
    partial class CadAddinSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CadAddinSetup));
            this.btn_AddinLoad = new System.Windows.Forms.Button();
            this.btn_AddinUnLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox_ChooseAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btn_AddinLoad
            // 
            this.btn_AddinLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_AddinLoad.Location = new System.Drawing.Point(177, 227);
            this.btn_AddinLoad.Name = "btn_AddinLoad";
            this.btn_AddinLoad.Size = new System.Drawing.Size(75, 23);
            this.btn_AddinLoad.TabIndex = 4;
            this.btn_AddinLoad.Text = "安装";
            this.btn_AddinLoad.UseVisualStyleBackColor = true;
            this.btn_AddinLoad.Click += new System.EventHandler(this.btn_AddinLoad_Click);
            // 
            // btn_AddinUnLoad
            // 
            this.btn_AddinUnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_AddinUnLoad.Location = new System.Drawing.Point(96, 227);
            this.btn_AddinUnLoad.Name = "btn_AddinUnLoad";
            this.btn_AddinUnLoad.Size = new System.Drawing.Size(75, 23);
            this.btn_AddinUnLoad.TabIndex = 3;
            this.btn_AddinUnLoad.Text = "卸载";
            this.btn_AddinUnLoad.UseVisualStyleBackColor = true;
            this.btn_AddinUnLoad.Click += new System.EventHandler(this.btn_AddinUnLoad_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "选择需要安装的 AutoCAD 版本";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(15, 29);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(237, 192);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // checkBox_ChooseAll
            // 
            this.checkBox_ChooseAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_ChooseAll.AutoSize = true;
            this.checkBox_ChooseAll.Location = new System.Drawing.Point(15, 231);
            this.checkBox_ChooseAll.Name = "checkBox_ChooseAll";
            this.checkBox_ChooseAll.Size = new System.Drawing.Size(48, 16);
            this.checkBox_ChooseAll.TabIndex = 2;
            this.checkBox_ChooseAll.Text = "全选";
            this.checkBox_ChooseAll.UseVisualStyleBackColor = true;
            this.checkBox_ChooseAll.CheckedChanged += new System.EventHandler(this.checkBox_ChooseAll_CheckedChanged);
            // 
            // CadAddinSetup
            // 
            this.AcceptButton = this.btn_AddinLoad;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 262);
            this.Controls.Add(this.checkBox_ChooseAll);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_AddinUnLoad);
            this.Controls.Add(this.btn_AddinLoad);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CadAddinSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQuantity 注册";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_AddinLoad;
        private System.Windows.Forms.Button btn_AddinUnLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBox_ChooseAll;
    }
}