using System;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class PF_PlaceProt
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_bothsides = new System.Windows.Forms.RadioButton();
            this.radioButton_right = new System.Windows.Forms.RadioButton();
            this.radioButton_left = new System.Windows.Forms.RadioButton();
            this.textBox_ProtMethod = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_Style = new System.Windows.Forms.TextBox();
            this.checkBox_AllSlopeLevels = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBox_SlopeLevels = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.textBoxNum_RangeEnd = new eZstd.UserControls.TextBoxNum();
            this.textBoxNum_RangeStart = new eZstd.UserControls.TextBoxNum();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_ChooseRangeOnUI = new System.Windows.Forms.CheckBox();
            this.button_PlaceProtections = new System.Windows.Forms.Button();
            this.protectionLister1 = new eZcad.SubgradeQuantity.SQControls.ItemLister();
            this.groupBox_fillcut.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_RunCmd
            // 
            this.btn_RunCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_RunCmd.Location = new System.Drawing.Point(179, 357);
            this.btn_RunCmd.TabIndex = 9;
            // 
            // btn_CancelCmd
            // 
            this.btn_CancelCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_CancelCmd.Location = new System.Drawing.Point(137, 357);
            this.btn_CancelCmd.TabIndex = 8;
            // 
            // btn_ViewUI
            // 
            this.btn_ViewUI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ViewUI.Location = new System.Drawing.Point(95, 357);
            this.btn_ViewUI.TabIndex = 7;
            // 
            // groupBox_fillcut
            // 
            this.groupBox_fillcut.Controls.Add(this.radioButton_fillcut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_cut);
            this.groupBox_fillcut.Controls.Add(this.radioButton_fill);
            this.groupBox_fillcut.Location = new System.Drawing.Point(12, 12);
            this.groupBox_fillcut.Name = "groupBox_fillcut";
            this.groupBox_fillcut.Size = new System.Drawing.Size(190, 51);
            this.groupBox_fillcut.TabIndex = 1;
            this.groupBox_fillcut.TabStop = false;
            this.groupBox_fillcut.Text = "填挖区分";
            // 
            // radioButton_fillcut
            // 
            this.radioButton_fillcut.AutoSize = true;
            this.radioButton_fillcut.Location = new System.Drawing.Point(137, 21);
            this.radioButton_fillcut.Name = "radioButton_fillcut";
            this.radioButton_fillcut.Size = new System.Drawing.Size(47, 16);
            this.radioButton_fillcut.TabIndex = 2;
            this.radioButton_fillcut.Text = "填挖";
            this.radioButton_fillcut.UseVisualStyleBackColor = true;
            // 
            // radioButton_cut
            // 
            this.radioButton_cut.AutoSize = true;
            this.radioButton_cut.Location = new System.Drawing.Point(72, 20);
            this.radioButton_cut.Name = "radioButton_cut";
            this.radioButton_cut.Size = new System.Drawing.Size(59, 16);
            this.radioButton_cut.TabIndex = 1;
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "防护方式";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "坡级";
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
            // textBox_ProtMethod
            // 
            this.textBox_ProtMethod.Location = new System.Drawing.Point(68, 2);
            this.textBox_ProtMethod.Name = "textBox_ProtMethod";
            this.textBox_ProtMethod.Size = new System.Drawing.Size(92, 21);
            this.textBox_ProtMethod.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "规格";
            // 
            // textBox_Style
            // 
            this.textBox_Style.Location = new System.Drawing.Point(68, 29);
            this.textBox_Style.Name = "textBox_Style";
            this.textBox_Style.Size = new System.Drawing.Size(92, 21);
            this.textBox_Style.TabIndex = 3;
            // 
            // checkBox_AllSlopeLevels
            // 
            this.checkBox_AllSlopeLevels.AutoSize = true;
            this.checkBox_AllSlopeLevels.Location = new System.Drawing.Point(68, 5);
            this.checkBox_AllSlopeLevels.Name = "checkBox_AllSlopeLevels";
            this.checkBox_AllSlopeLevels.Size = new System.Drawing.Size(48, 16);
            this.checkBox_AllSlopeLevels.TabIndex = 14;
            this.checkBox_AllSlopeLevels.Text = "任意";
            this.checkBox_AllSlopeLevels.UseVisualStyleBackColor = true;
            this.checkBox_AllSlopeLevels.CheckedChanged += new System.EventHandler(this.checkBox_AllSlopeLevels_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBox_Style);
            this.panel1.Controls.Add(this.textBox_ProtMethod);
            this.panel1.Location = new System.Drawing.Point(12, 273);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(166, 56);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.textBox_SlopeLevels);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.checkBox_AllSlopeLevels);
            this.panel2.Location = new System.Drawing.Point(12, 212);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(166, 51);
            this.panel2.TabIndex = 4;
            // 
            // textBox_SlopeLevels
            // 
            this.textBox_SlopeLevels.Location = new System.Drawing.Point(68, 27);
            this.textBox_SlopeLevels.Name = "textBox_SlopeLevels";
            this.textBox_SlopeLevels.Size = new System.Drawing.Size(92, 21);
            this.textBox_SlopeLevels.TabIndex = 17;
            this.textBox_SlopeLevels.Text = "1";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.textBoxNum_RangeEnd);
            this.panel3.Controls.Add(this.textBoxNum_RangeStart);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.checkBox_ChooseRangeOnUI);
            this.panel3.Location = new System.Drawing.Point(12, 126);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(166, 76);
            this.panel3.TabIndex = 3;
            // 
            // textBoxNum_RangeEnd
            // 
            this.textBoxNum_RangeEnd.Location = new System.Drawing.Point(68, 52);
            this.textBoxNum_RangeEnd.Name = "textBoxNum_RangeEnd";
            this.textBoxNum_RangeEnd.Size = new System.Drawing.Size(92, 21);
            this.textBoxNum_RangeEnd.TabIndex = 5;
            // 
            // textBoxNum_RangeStart
            // 
            this.textBoxNum_RangeStart.Location = new System.Drawing.Point(68, 27);
            this.textBoxNum_RangeStart.Name = "textBoxNum_RangeStart";
            this.textBoxNum_RangeStart.Size = new System.Drawing.Size(92, 21);
            this.textBoxNum_RangeStart.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-2, 55);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 4;
            this.label6.Text = "结尾桩号";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-2, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "起始桩号";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "区间";
            // 
            // checkBox_ChooseRangeOnUI
            // 
            this.checkBox_ChooseRangeOnUI.AutoSize = true;
            this.checkBox_ChooseRangeOnUI.Location = new System.Drawing.Point(68, 5);
            this.checkBox_ChooseRangeOnUI.Name = "checkBox_ChooseRangeOnUI";
            this.checkBox_ChooseRangeOnUI.Size = new System.Drawing.Size(72, 16);
            this.checkBox_ChooseRangeOnUI.TabIndex = 0;
            this.checkBox_ChooseRangeOnUI.Text = "界面选择";
            this.checkBox_ChooseRangeOnUI.UseVisualStyleBackColor = true;
            this.checkBox_ChooseRangeOnUI.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button_PlaceProtections
            // 
            this.button_PlaceProtections.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_PlaceProtections.Location = new System.Drawing.Point(9, 357);
            this.button_PlaceProtections.Name = "button_PlaceProtections";
            this.button_PlaceProtections.Size = new System.Drawing.Size(75, 23);
            this.button_PlaceProtections.TabIndex = 6;
            this.button_PlaceProtections.Text = "放置";
            this.button_PlaceProtections.UseVisualStyleBackColor = true;
            this.button_PlaceProtections.Click += new System.EventHandler(this.button_PlaceProtections_Click);
            // 
            // protectionLister1
            // 
            this.protectionLister1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.protectionLister1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.protectionLister1.Location = new System.Drawing.Point(222, 0);
            this.protectionLister1.Name = "protectionLister1";
            this.protectionLister1.Size = new System.Drawing.Size(221, 391);
            this.protectionLister1.TabIndex = 12;
            // 
            // PF_PlaceProt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 391);
            this.Controls.Add(this.button_PlaceProtections);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.protectionLister1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_fillcut);
            this.Margin = new System.Windows.Forms.Padding(3);
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(460, 430);
            this.Name = "PF_PlaceProt";
            this.Text = "防护布置";
            this.Controls.SetChildIndex(this.btn_RunCmd, 0);
            this.Controls.SetChildIndex(this.btn_CancelCmd, 0);
            this.Controls.SetChildIndex(this.btn_ViewUI, 0);
            this.Controls.SetChildIndex(this.groupBox_fillcut, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.protectionLister1, 0);
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.panel3, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.button_PlaceProtections, 0);
            this.groupBox_fillcut.ResumeLayout(false);
            this.groupBox_fillcut.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.GroupBox groupBox_fillcut;
        private System.Windows.Forms.RadioButton radioButton_fillcut;
        private System.Windows.Forms.RadioButton radioButton_cut;
        private System.Windows.Forms.RadioButton radioButton_fill;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_bothsides;
        private System.Windows.Forms.RadioButton radioButton_right;
        private System.Windows.Forms.RadioButton radioButton_left;
        private SQControls.ItemLister protectionLister1;
        private System.Windows.Forms.TextBox textBox_ProtMethod;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_Style;
        private System.Windows.Forms.CheckBox checkBox_AllSlopeLevels;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBox_SlopeLevels;
        private System.Windows.Forms.Panel panel3;
        private eZstd.UserControls.TextBoxNum textBoxNum_RangeEnd;
        private eZstd.UserControls.TextBoxNum textBoxNum_RangeStart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_ChooseRangeOnUI;
        private System.Windows.Forms.Button button_PlaceProtections;
    }
}