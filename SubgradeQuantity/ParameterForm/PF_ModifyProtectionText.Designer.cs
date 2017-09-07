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
            this.protLayerLister1 = new eZcad.SubgradeQuantity.SlopeProtection.ItemLister();
            this.groupBox_fillcut.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.groupBox_fillcut.Text = "填挖区分";
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
            this.groupBox2.Location = new System.Drawing.Point(13, 269);
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
            // PF_ModifyProtectionText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 391);
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
            this.Controls.SetChildIndex(this.btn_RunCmd, 0);
            this.Controls.SetChildIndex(this.btn_CancelCmd, 0);
            this.Controls.SetChildIndex(this.btn_ViewUI, 0);
            this.Controls.SetChildIndex(this.groupBox_fillcut, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.protLayerLister1, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.groupBox_fillcut.ResumeLayout(false);
            this.groupBox_fillcut.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private SlopeProtection.ItemLister protLayerLister1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton_Delete;
        private System.Windows.Forms.RadioButton radioButton_highLight;
    }
}