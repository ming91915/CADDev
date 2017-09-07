namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class PF_SeperateByElev
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
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxNum_cutElev = new eZstd.UserControls.TextBoxNum();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_cutMethods = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_bothsides = new System.Windows.Forms.RadioButton();
            this.radioButton_right = new System.Windows.Forms.RadioButton();
            this.radioButton_left = new System.Windows.Forms.RadioButton();
            this.groupBox_fillcut.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_RunCmd
            // 
            this.btn_RunCmd.Location = new System.Drawing.Point(169, 224);
            // 
            // btn_CancelCmd
            // 
            this.btn_CancelCmd.Location = new System.Drawing.Point(127, 224);
            // 
            // btn_ViewUI
            // 
            this.btn_ViewUI.Location = new System.Drawing.Point(85, 224);
            // 
            // groupBox_fillcut
            // 
            this.groupBox_fillcut.Controls.Add(this.radioButton_fillcut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_cut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_fill);
            this.groupBox_fillcut.Location = new System.Drawing.Point(12, 12);
            this.groupBox_fillcut.Name = "groupBox_fillcut";
            this.groupBox_fillcut.Size = new System.Drawing.Size(190, 51);
            this.groupBox_fillcut.TabIndex = 2;
            this.groupBox_fillcut.TabStop = false;
            this.groupBox_fillcut.Text = "填挖区分";
            // 
            // radioButton_fillcut
            // 
            this.radioButton_fillcut.AutoSize = true;
            this.radioButton_fillcut.Location = new System.Drawing.Point(137, 21);
            this.radioButton_fillcut.Name = "radioButton_fillcut";
            this.radioButton_fillcut.Size = new System.Drawing.Size(47, 16);
            this.radioButton_fillcut.TabIndex = 0;
            this.radioButton_fillcut.Text = "填挖";
            this.radioButton_fillcut.UseVisualStyleBackColor = true;
            // 
            // radioButton_cut
            // 
            this.radioButton_cut.AutoSize = true;
            this.radioButton_cut.Location = new System.Drawing.Point(72, 20);
            this.radioButton_cut.Name = "radioButton_cut";
            this.radioButton_cut.Size = new System.Drawing.Size(59, 16);
            this.radioButton_cut.TabIndex = 0;
            this.radioButton_cut.Text = "仅挖方";
            this.radioButton_cut.UseVisualStyleBackColor = true;
            // 
            // radioButton_fill
            // 
            this.radioButton_fill.AutoSize = true;
            this.radioButton_fill.Checked = true;
            this.radioButton_fill.Location = new System.Drawing.Point(7, 21);
            this.radioButton_fill.Name = "radioButton_fill";
            this.radioButton_fill.Size = new System.Drawing.Size(59, 16);
            this.radioButton_fill.TabIndex = 0;
            this.radioButton_fill.TabStop = true;
            this.radioButton_fill.Text = "仅填方";
            this.radioButton_fill.UseVisualStyleBackColor = true;
            // 
            // textBoxNum_waterLineLength
            // 
            this.textBoxNum_waterLineLength.Location = new System.Drawing.Point(80, 190);
            this.textBoxNum_waterLineLength.Name = "textBoxNum_waterLineLength";
            this.textBoxNum_waterLineLength.PositiveOnly = true;
            this.textBoxNum_waterLineLength.Size = new System.Drawing.Size(92, 21);
            this.textBoxNum_waterLineLength.TabIndex = 5;
            this.textBoxNum_waterLineLength.Text = "5";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "水位线长度";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(177, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "(m)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "高度值";
            // 
            // textBoxNum_cutElev
            // 
            this.textBoxNum_cutElev.Location = new System.Drawing.Point(80, 128);
            this.textBoxNum_cutElev.Name = "textBoxNum_cutElev";
            this.textBoxNum_cutElev.PositiveOnly = true;
            this.textBoxNum_cutElev.Size = new System.Drawing.Size(92, 21);
            this.textBoxNum_cutElev.TabIndex = 5;
            this.textBoxNum_cutElev.Text = "1740";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 163);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "剪切方式";
            // 
            // comboBox_cutMethods
            // 
            this.comboBox_cutMethods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_cutMethods.FormattingEnabled = true;
            this.comboBox_cutMethods.Location = new System.Drawing.Point(80, 160);
            this.comboBox_cutMethods.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_cutMethods.Name = "comboBox_cutMethods";
            this.comboBox_cutMethods.Size = new System.Drawing.Size(92, 20);
            this.comboBox_cutMethods.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_bothsides);
            this.groupBox1.Controls.Add(this.radioButton_right);
            this.groupBox1.Controls.Add(this.radioButton_left);
            this.groupBox1.Location = new System.Drawing.Point(12, 69);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(190, 51);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "左右";
            // 
            // radioButton_bothsides
            // 
            this.radioButton_bothsides.AutoSize = true;
            this.radioButton_bothsides.Location = new System.Drawing.Point(137, 21);
            this.radioButton_bothsides.Name = "radioButton_bothsides";
            this.radioButton_bothsides.Size = new System.Drawing.Size(47, 16);
            this.radioButton_bothsides.TabIndex = 0;
            this.radioButton_bothsides.Text = "两侧";
            this.radioButton_bothsides.UseVisualStyleBackColor = true;
            // 
            // radioButton_right
            // 
            this.radioButton_right.AutoSize = true;
            this.radioButton_right.Location = new System.Drawing.Point(72, 20);
            this.radioButton_right.Name = "radioButton_right";
            this.radioButton_right.Size = new System.Drawing.Size(59, 16);
            this.radioButton_right.TabIndex = 0;
            this.radioButton_right.Text = "仅右侧";
            this.radioButton_right.UseVisualStyleBackColor = true;
            // 
            // radioButton_left
            // 
            this.radioButton_left.AutoSize = true;
            this.radioButton_left.Checked = true;
            this.radioButton_left.Location = new System.Drawing.Point(7, 21);
            this.radioButton_left.Name = "radioButton_left";
            this.radioButton_left.Size = new System.Drawing.Size(59, 16);
            this.radioButton_left.TabIndex = 0;
            this.radioButton_left.TabStop = true;
            this.radioButton_left.Text = "仅左侧";
            this.radioButton_left.UseVisualStyleBackColor = true;
            // 
            // PF_SeperateByElev
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 258);
            this.Controls.Add(this.comboBox_cutMethods);
            this.Controls.Add(this.textBoxNum_cutElev);
            this.Controls.Add(this.textBoxNum_waterLineLength);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_fillcut);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3);
            this.MaximizeBox = false;
            this.Name = "PF_SeperateByElev";
            this.Text = "边坡剪切";
            this.Controls.SetChildIndex(this.btn_RunCmd, 0);
            this.Controls.SetChildIndex(this.btn_CancelCmd, 0);
            this.Controls.SetChildIndex(this.btn_ViewUI, 0);
            this.Controls.SetChildIndex(this.groupBox_fillcut, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.textBoxNum_waterLineLength, 0);
            this.Controls.SetChildIndex(this.textBoxNum_cutElev, 0);
            this.Controls.SetChildIndex(this.comboBox_cutMethods, 0);
            this.groupBox_fillcut.ResumeLayout(false);
            this.groupBox_fillcut.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private eZstd.UserControls.TextBoxNum textBoxNum_cutElev;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_cutMethods;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_bothsides;
        private System.Windows.Forms.RadioButton radioButton_right;
        private System.Windows.Forms.RadioButton radioButton_left;
    }
}