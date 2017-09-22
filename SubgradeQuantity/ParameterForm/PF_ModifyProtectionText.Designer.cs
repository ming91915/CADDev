using System;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class PF_ModifyProtectionText
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
            this.groupBox_fillcut = new System.Windows.Forms.GroupBox();
            this.radioButton_allSegtype = new System.Windows.Forms.RadioButton();
            this.radioButton_platform = new System.Windows.Forms.RadioButton();
            this.radioButton_Slope = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_bothsides = new System.Windows.Forms.RadioButton();
            this.radioButton_right = new System.Windows.Forms.RadioButton();
            this.radioButton_left = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton_Delete = new System.Windows.Forms.RadioButton();
            this.radioButton_highLight = new System.Windows.Forms.RadioButton();
            this.protLayerLister1 = new eZcad.SubgradeQuantity.SQControls.ItemLister();
            this.panelSlopeLevel = new System.Windows.Forms.Panel();
            this.textBox_SlopeLevels = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_AllSlopeLevels = new System.Windows.Forms.CheckBox();
            this.checkBox_preserveHighlighted = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_Execute = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox_fillcut.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panelSlopeLevel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_RunCmd
            // 
            this.btn_RunCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_RunCmd.Location = new System.Drawing.Point(179, 357);
            this.btn_RunCmd.TabIndex = 9;
            this.btn_RunCmd.Text = "确定";
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
            this.groupBox_fillcut.Controls.Add(this.radioButton_allSegtype);
            this.groupBox_fillcut.Controls.Add(this.radioButton_platform);
            this.groupBox_fillcut.Controls.Add(this.radioButton_Slope);
            this.groupBox_fillcut.Location = new System.Drawing.Point(12, 12);
            this.groupBox_fillcut.Name = "groupBox_fillcut";
            this.groupBox_fillcut.Size = new System.Drawing.Size(190, 51);
            this.groupBox_fillcut.TabIndex = 1;
            this.groupBox_fillcut.TabStop = false;
            this.groupBox_fillcut.Text = "边坡类型";
            // 
            // radioButton_allSegtype
            // 
            this.radioButton_allSegtype.AutoSize = true;
            this.radioButton_allSegtype.Checked = true;
            this.radioButton_allSegtype.Location = new System.Drawing.Point(137, 21);
            this.radioButton_allSegtype.Name = "radioButton_allSegtype";
            this.radioButton_allSegtype.Size = new System.Drawing.Size(47, 16);
            this.radioButton_allSegtype.TabIndex = 2;
            this.radioButton_allSegtype.TabStop = true;
            this.radioButton_allSegtype.Text = "所有";
            this.radioButton_allSegtype.UseVisualStyleBackColor = true;
            // 
            // radioButton_platform
            // 
            this.radioButton_platform.AutoSize = true;
            this.radioButton_platform.Location = new System.Drawing.Point(72, 20);
            this.radioButton_platform.Name = "radioButton_platform";
            this.radioButton_platform.Size = new System.Drawing.Size(59, 16);
            this.radioButton_platform.TabIndex = 1;
            this.radioButton_platform.Text = "仅平台";
            this.radioButton_platform.UseVisualStyleBackColor = true;
            // 
            // radioButton_Slope
            // 
            this.radioButton_Slope.AutoSize = true;
            this.radioButton_Slope.Location = new System.Drawing.Point(7, 21);
            this.radioButton_Slope.Name = "radioButton_Slope";
            this.radioButton_Slope.Size = new System.Drawing.Size(59, 16);
            this.radioButton_Slope.TabIndex = 0;
            this.radioButton_Slope.Text = "仅边坡";
            this.radioButton_Slope.UseVisualStyleBackColor = true;
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
            this.radioButton_bothsides.Checked = true;
            this.radioButton_bothsides.Location = new System.Drawing.Point(137, 21);
            this.radioButton_bothsides.Name = "radioButton_bothsides";
            this.radioButton_bothsides.Size = new System.Drawing.Size(47, 16);
            this.radioButton_bothsides.TabIndex = 0;
            this.radioButton_bothsides.TabStop = true;
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
            this.radioButton_left.Location = new System.Drawing.Point(7, 21);
            this.radioButton_left.Name = "radioButton_left";
            this.radioButton_left.Size = new System.Drawing.Size(59, 16);
            this.radioButton_left.TabIndex = 0;
            this.radioButton_left.Text = "仅左侧";
            this.radioButton_left.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton_Delete);
            this.groupBox2.Controls.Add(this.radioButton_highLight);
            this.groupBox2.Location = new System.Drawing.Point(13, 126);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(189, 53);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "操作";
            // 
            // radioButton_Delete
            // 
            this.radioButton_Delete.AutoSize = true;
            this.radioButton_Delete.Location = new System.Drawing.Point(63, 20);
            this.radioButton_Delete.Name = "radioButton_Delete";
            this.radioButton_Delete.Size = new System.Drawing.Size(47, 16);
            this.radioButton_Delete.TabIndex = 0;
            this.radioButton_Delete.Text = "删除";
            this.radioButton_Delete.UseVisualStyleBackColor = true;
            this.radioButton_Delete.CheckedChanged += new System.EventHandler(this.radioButton_Delete_CheckedChanged);
            // 
            // radioButton_highLight
            // 
            this.radioButton_highLight.AutoSize = true;
            this.radioButton_highLight.Checked = true;
            this.radioButton_highLight.Location = new System.Drawing.Point(10, 20);
            this.radioButton_highLight.Name = "radioButton_highLight";
            this.radioButton_highLight.Size = new System.Drawing.Size(47, 16);
            this.radioButton_highLight.TabIndex = 0;
            this.radioButton_highLight.TabStop = true;
            this.radioButton_highLight.Text = "高亮";
            this.radioButton_highLight.UseVisualStyleBackColor = true;
            // 
            // protLayerLister1
            // 
            this.protLayerLister1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.protLayerLister1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.protLayerLister1.Location = new System.Drawing.Point(222, 0);
            this.protLayerLister1.Name = "protLayerLister1";
            this.protLayerLister1.Size = new System.Drawing.Size(221, 391);
            this.protLayerLister1.TabIndex = 12;
            // 
            // panelSlopeLevel
            // 
            this.panelSlopeLevel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSlopeLevel.Controls.Add(this.textBox_SlopeLevels);
            this.panelSlopeLevel.Controls.Add(this.label1);
            this.panelSlopeLevel.Controls.Add(this.checkBox_AllSlopeLevels);
            this.panelSlopeLevel.Location = new System.Drawing.Point(3, 27);
            this.panelSlopeLevel.Name = "panelSlopeLevel";
            this.panelSlopeLevel.Size = new System.Drawing.Size(166, 56);
            this.panelSlopeLevel.TabIndex = 15;
            // 
            // textBox_SlopeLevels
            // 
            this.textBox_SlopeLevels.Enabled = false;
            this.textBox_SlopeLevels.Location = new System.Drawing.Point(69, 30);
            this.textBox_SlopeLevels.Name = "textBox_SlopeLevels";
            this.textBox_SlopeLevels.Size = new System.Drawing.Size(92, 21);
            this.textBox_SlopeLevels.TabIndex = 17;
            this.textBox_SlopeLevels.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "边坡（平台）等级";
            // 
            // checkBox_AllSlopeLevels
            // 
            this.checkBox_AllSlopeLevels.AutoSize = true;
            this.checkBox_AllSlopeLevels.Checked = true;
            this.checkBox_AllSlopeLevels.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_AllSlopeLevels.Location = new System.Drawing.Point(7, 32);
            this.checkBox_AllSlopeLevels.Name = "checkBox_AllSlopeLevels";
            this.checkBox_AllSlopeLevels.Size = new System.Drawing.Size(48, 16);
            this.checkBox_AllSlopeLevels.TabIndex = 14;
            this.checkBox_AllSlopeLevels.Text = "任意";
            this.checkBox_AllSlopeLevels.UseVisualStyleBackColor = true;
            this.checkBox_AllSlopeLevels.CheckedChanged += new System.EventHandler(this.checkBox_AllSlopeLevels_CheckedChanged);
            // 
            // checkBox_preserveHighlighted
            // 
            this.checkBox_preserveHighlighted.AutoSize = true;
            this.checkBox_preserveHighlighted.Location = new System.Drawing.Point(4, 5);
            this.checkBox_preserveHighlighted.Name = "checkBox_preserveHighlighted";
            this.checkBox_preserveHighlighted.Size = new System.Drawing.Size(72, 16);
            this.checkBox_preserveHighlighted.TabIndex = 14;
            this.checkBox_preserveHighlighted.Text = "保留选中";
            this.toolTip1.SetToolTip(this.checkBox_preserveHighlighted, "直接将当前界面中已经选择的对象作为筛选数据源进行删除");
            this.checkBox_preserveHighlighted.UseVisualStyleBackColor = true;
            this.checkBox_preserveHighlighted.CheckedChanged += new System.EventHandler(this.checkBox_AllSlopeLevels_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelSlopeLevel);
            this.panel1.Controls.Add(this.checkBox_preserveHighlighted);
            this.panel1.Location = new System.Drawing.Point(12, 185);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(190, 100);
            this.panel1.TabIndex = 16;
            this.panel1.Visible = false;
            // 
            // button_Execute
            // 
            this.button_Execute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Execute.Location = new System.Drawing.Point(12, 357);
            this.button_Execute.Name = "button_Execute";
            this.button_Execute.Size = new System.Drawing.Size(38, 23);
            this.button_Execute.TabIndex = 17;
            this.button_Execute.Text = "执行";
            this.toolTip1.SetToolTip(this.button_Execute, "或通过双击某防护类型以执行操作");
            this.button_Execute.UseVisualStyleBackColor = true;
            this.button_Execute.Click += new System.EventHandler(this.button_Execute_Click);
            // 
            // PF_ModifyProtectionText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 391);
            this.Controls.Add(this.button_Execute);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.protLayerLister1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_fillcut);
            this.Margin = new System.Windows.Forms.Padding(3);
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(460, 430);
            this.Name = "PF_ModifyProtectionText";
            this.Text = "文字修改";
            this.Load += new System.EventHandler(this.PF_ModifyProtText_Load);
            this.Controls.SetChildIndex(this.groupBox_fillcut, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.protLayerLister1, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.btn_RunCmd, 0);
            this.Controls.SetChildIndex(this.btn_CancelCmd, 0);
            this.Controls.SetChildIndex(this.btn_ViewUI, 0);
            this.Controls.SetChildIndex(this.button_Execute, 0);
            this.groupBox_fillcut.ResumeLayout(false);
            this.groupBox_fillcut.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panelSlopeLevel.ResumeLayout(false);
            this.panelSlopeLevel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.GroupBox groupBox_fillcut;
        private System.Windows.Forms.RadioButton radioButton_allSegtype;
        private System.Windows.Forms.RadioButton radioButton_platform;
        private System.Windows.Forms.RadioButton radioButton_Slope;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_bothsides;
        private System.Windows.Forms.RadioButton radioButton_right;
        private System.Windows.Forms.RadioButton radioButton_left;
        private SQControls.ItemLister protLayerLister1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton_Delete;
        private System.Windows.Forms.RadioButton radioButton_highLight;
        private System.Windows.Forms.Panel panelSlopeLevel;
        private System.Windows.Forms.TextBox textBox_SlopeLevels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_AllSlopeLevels;
        private System.Windows.Forms.CheckBox checkBox_preserveHighlighted;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_Execute;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}