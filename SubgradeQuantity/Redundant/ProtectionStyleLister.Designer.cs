namespace eZcad.SubgradeQuantity.Redundant
{
    partial class ProtectionStyleLister
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
            this.flp_ProtectionStyles = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_Excav = new System.Windows.Forms.RadioButton();
            this.radioButton_Fill = new System.Windows.Forms.RadioButton();
            this.btn_Ok = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.listBox_slopes = new System.Windows.Forms.ListBox();
            this.btn_ImportBoundary = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flp_ProtectionStyles
            // 
            this.flp_ProtectionStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flp_ProtectionStyles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flp_ProtectionStyles.Location = new System.Drawing.Point(341, 122);
            this.flp_ProtectionStyles.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flp_ProtectionStyles.Name = "flp_ProtectionStyles";
            this.flp_ProtectionStyles.Size = new System.Drawing.Size(293, 258);
            this.flp_ProtectionStyles.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(339, 90);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "选择一种防护形式";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButton_Excav);
            this.groupBox1.Controls.Add(this.radioButton_Fill);
            this.groupBox1.Location = new System.Drawing.Point(340, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(295, 60);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "填挖方";
            // 
            // radioButton_Excav
            // 
            this.radioButton_Excav.AutoSize = true;
            this.radioButton_Excav.Checked = true;
            this.radioButton_Excav.Location = new System.Drawing.Point(79, 25);
            this.radioButton_Excav.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButton_Excav.Name = "radioButton_Excav";
            this.radioButton_Excav.Size = new System.Drawing.Size(58, 19);
            this.radioButton_Excav.TabIndex = 0;
            this.radioButton_Excav.TabStop = true;
            this.radioButton_Excav.Text = "挖方";
            this.radioButton_Excav.UseVisualStyleBackColor = true;
            this.radioButton_Excav.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButton_Fill
            // 
            this.radioButton_Fill.AutoSize = true;
            this.radioButton_Fill.Location = new System.Drawing.Point(8, 25);
            this.radioButton_Fill.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButton_Fill.Name = "radioButton_Fill";
            this.radioButton_Fill.Size = new System.Drawing.Size(58, 19);
            this.radioButton_Fill.TabIndex = 0;
            this.radioButton_Fill.Text = "填方";
            this.radioButton_Fill.UseVisualStyleBackColor = true;
            this.radioButton_Fill.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // btn_Ok
            // 
            this.btn_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Ok.Location = new System.Drawing.Point(538, 388);
            this.btn_Ok.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(100, 29);
            this.btn_Ok.TabIndex = 3;
            this.btn_Ok.Text = "确定";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Clear.Location = new System.Drawing.Point(16, 388);
            this.button_Clear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(100, 29);
            this.button_Clear.TabIndex = 4;
            this.button_Clear.Text = "清除数据";
            this.button_Clear.UseVisualStyleBackColor = true;
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // listBox_slopes
            // 
            this.listBox_slopes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_slopes.FormattingEnabled = true;
            this.listBox_slopes.ItemHeight = 15;
            this.listBox_slopes.Location = new System.Drawing.Point(16, 15);
            this.listBox_slopes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listBox_slopes.Name = "listBox_slopes";
            this.listBox_slopes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_slopes.Size = new System.Drawing.Size(315, 364);
            this.listBox_slopes.TabIndex = 5;
            this.listBox_slopes.SelectedValueChanged += new System.EventHandler(this.listBox_slopes_SelectedValueChanged);
            this.listBox_slopes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_slopes_MouseDoubleClick);
            // 
            // btn_ImportBoundary
            // 
            this.btn_ImportBoundary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ImportBoundary.Location = new System.Drawing.Point(231, 388);
            this.btn_ImportBoundary.Name = "btn_ImportBoundary";
            this.btn_ImportBoundary.Size = new System.Drawing.Size(100, 29);
            this.btn_ImportBoundary.TabIndex = 6;
            this.btn_ImportBoundary.Text = "导入";
            this.btn_ImportBoundary.UseVisualStyleBackColor = true;
            this.btn_ImportBoundary.Click += new System.EventHandler(this.btn_ImportBoundary_Click);
            // 
            // ProtectionStyleLister
            // 
            this.AcceptButton = this.btn_Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 426);
            this.Controls.Add(this.btn_ImportBoundary);
            this.Controls.Add(this.listBox_slopes);
            this.Controls.Add(this.button_Clear);
            this.Controls.Add(this.btn_Ok);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flp_ProtectionStyles);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ProtectionStyleLister";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "边坡防护";
            this.Shown += new System.EventHandler(this.ProtectionStyleLister_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProtectionStyleLister_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flp_ProtectionStyles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_Excav;
        private System.Windows.Forms.RadioButton radioButton_Fill;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.Button button_Clear;
        private System.Windows.Forms.ListBox listBox_slopes;
        private System.Windows.Forms.Button btn_ImportBoundary;
    }
}