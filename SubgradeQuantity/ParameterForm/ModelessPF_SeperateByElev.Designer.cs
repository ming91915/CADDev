namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class ModelessPF_SeperateByElev
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
            this.groupBox_fillcut = new System.Windows.Forms.GroupBox();
            this.radioButton_fillcut = new System.Windows.Forms.RadioButton();
            this.radioButton_cut = new System.Windows.Forms.RadioButton();
            this.radioButton_fill = new System.Windows.Forms.RadioButton();
            this.textBoxNum_waterLineLength = new eZstd.UserControls.TextBoxNum();
            this.label2 = new System.Windows.Forms.Label();
            this.button_RunCmd = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxNum_cutElev = new eZstd.UserControls.TextBoxNum();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_cutMethods = new System.Windows.Forms.ComboBox();
            this.groupBox_fillcut.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_fillcut
            // 
            this.groupBox_fillcut.Controls.Add(this.radioButton_fillcut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_cut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_fill);
            this.groupBox_fillcut.Location = new System.Drawing.Point(16, 15);
            this.groupBox_fillcut.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_fillcut.Name = "groupBox_fillcut";
            this.groupBox_fillcut.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_fillcut.Size = new System.Drawing.Size(253, 64);
            this.groupBox_fillcut.TabIndex = 2;
            this.groupBox_fillcut.TabStop = false;
            this.groupBox_fillcut.Text = "填挖区分";
            // 
            // radioButton_fillcut
            // 
            this.radioButton_fillcut.AutoSize = true;
            this.radioButton_fillcut.Location = new System.Drawing.Point(183, 26);
            this.radioButton_fillcut.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton_fillcut.Name = "radioButton_fillcut";
            this.radioButton_fillcut.Size = new System.Drawing.Size(58, 19);
            this.radioButton_fillcut.TabIndex = 0;
            this.radioButton_fillcut.Text = "填挖";
            this.radioButton_fillcut.UseVisualStyleBackColor = true;
            // 
            // radioButton_cut
            // 
            this.radioButton_cut.AutoSize = true;
            this.radioButton_cut.Location = new System.Drawing.Point(96, 25);
            this.radioButton_cut.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton_cut.Name = "radioButton_cut";
            this.radioButton_cut.Size = new System.Drawing.Size(73, 19);
            this.radioButton_cut.TabIndex = 0;
            this.radioButton_cut.Text = "仅挖方";
            this.radioButton_cut.UseVisualStyleBackColor = true;
            // 
            // radioButton_fill
            // 
            this.radioButton_fill.AutoSize = true;
            this.radioButton_fill.Checked = true;
            this.radioButton_fill.Location = new System.Drawing.Point(9, 26);
            this.radioButton_fill.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton_fill.Name = "radioButton_fill";
            this.radioButton_fill.Size = new System.Drawing.Size(73, 19);
            this.radioButton_fill.TabIndex = 0;
            this.radioButton_fill.TabStop = true;
            this.radioButton_fill.Text = "仅填方";
            this.radioButton_fill.UseVisualStyleBackColor = true;
            // 
            // textBoxNum_waterLineLength
            // 
            this.textBoxNum_waterLineLength.Location = new System.Drawing.Point(107, 174);
            this.textBoxNum_waterLineLength.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxNum_waterLineLength.Name = "textBoxNum_waterLineLength";
            this.textBoxNum_waterLineLength.PositiveOnly = true;
            this.textBoxNum_waterLineLength.Size = new System.Drawing.Size(121, 25);
            this.textBoxNum_waterLineLength.TabIndex = 5;
            this.textBoxNum_waterLineLength.Text = "5";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 179);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "水位线长度";
            // 
            // button_RunCmd
            // 
            this.button_RunCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_RunCmd.Location = new System.Drawing.Point(206, 220);
            this.button_RunCmd.Name = "button_RunCmd";
            this.button_RunCmd.Size = new System.Drawing.Size(64, 29);
            this.button_RunCmd.TabIndex = 9;
            this.button_RunCmd.Text = "执行";
            this.button_RunCmd.UseVisualStyleBackColor = true;
            this.button_RunCmd.Click += new System.EventHandler(this.button3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(236, 181);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "(m)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 101);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "高度值";
            // 
            // textBoxNum_cutElev
            // 
            this.textBoxNum_cutElev.Location = new System.Drawing.Point(107, 96);
            this.textBoxNum_cutElev.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxNum_cutElev.Name = "textBoxNum_cutElev";
            this.textBoxNum_cutElev.PositiveOnly = true;
            this.textBoxNum_cutElev.Size = new System.Drawing.Size(121, 25);
            this.textBoxNum_cutElev.TabIndex = 5;
            this.textBoxNum_cutElev.Text = "1740";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 140);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "剪切方式";
            // 
            // comboBox_cutMethods
            // 
            this.comboBox_cutMethods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_cutMethods.FormattingEnabled = true;
            this.comboBox_cutMethods.Location = new System.Drawing.Point(107, 136);
            this.comboBox_cutMethods.Name = "comboBox_cutMethods";
            this.comboBox_cutMethods.Size = new System.Drawing.Size(121, 23);
            this.comboBox_cutMethods.TabIndex = 10;
            // 
            // PF_SeperateByElev
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 261);
            this.Controls.Add(this.comboBox_cutMethods);
            this.Controls.Add(this.button_RunCmd);
            this.Controls.Add(this.textBoxNum_cutElev);
            this.Controls.Add(this.textBoxNum_waterLineLength);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox_fillcut);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PF_SeperateByElev";
            this.Text = "边坡剪切";
            this.groupBox_fillcut.ResumeLayout(false);
            this.groupBox_fillcut.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_fillcut;
        private System.Windows.Forms.RadioButton radioButton_fillcut;
        private System.Windows.Forms.RadioButton radioButton_cut;
        private System.Windows.Forms.RadioButton radioButton_fill;
        private eZstd.UserControls.TextBoxNum textBoxNum_waterLineLength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_RunCmd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private eZstd.UserControls.TextBoxNum textBoxNum_cutElev;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_cutMethods;
    }
}