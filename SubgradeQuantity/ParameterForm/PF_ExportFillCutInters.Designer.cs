namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class PF_ExportFillCutInters
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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label_roadCenter = new System.Windows.Forms.Label();
            this.label_ground = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxNum_StartStation = new eZstd.UserControls.TextBoxNum();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxNum_xScale = new eZstd.UserControls.TextBoxNum();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxNum_yScale = new eZstd.UserControls.TextBoxNum();
            this.panel_Transform = new System.Windows.Forms.Panel();
            this.textBoxNum_StartElevation = new eZstd.UserControls.TextBoxNum();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView_Excludes = new System.Windows.Forms.DataGridView();
            this.button_DeleteRange = new System.Windows.Forms.Button();
            this.button_ExtractFromCurve = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_LongitudinalStairExcav = new System.Windows.Forms.RadioButton();
            this.radioButton_FillCutInters = new System.Windows.Forms.RadioButton();
            this.panel_Transform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Excludes)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_RunCmd
            // 
            this.btn_RunCmd.Location = new System.Drawing.Point(330, 331);
            // 
            // btn_CancelCmd
            // 
            this.btn_CancelCmd.Location = new System.Drawing.Point(288, 331);
            // 
            // btn_ViewUI
            // 
            this.btn_ViewUI.Location = new System.Drawing.Point(246, 331);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "道路中桩设计线";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "地面线";
            // 
            // label_roadCenter
            // 
            this.label_roadCenter.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label_roadCenter.Location = new System.Drawing.Point(113, 17);
            this.label_roadCenter.Name = "label_roadCenter";
            this.label_roadCenter.Size = new System.Drawing.Size(61, 20);
            this.label_roadCenter.TabIndex = 11;
            this.label_roadCenter.Text = "***";
            this.label_roadCenter.Click += new System.EventHandler(this.label_roadCenter_Click);
            // 
            // label_ground
            // 
            this.label_ground.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label_ground.Location = new System.Drawing.Point(113, 42);
            this.label_ground.Name = "label_ground";
            this.label_ground.Size = new System.Drawing.Size(61, 20);
            this.label_ground.TabIndex = 11;
            this.label_ground.Text = "***";
            this.label_ground.Click += new System.EventHandler(this.label_ground_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 121);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "排除区段";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 11;
            this.label8.Text = "起点桩号";
            // 
            // textBoxNum_StartStation
            // 
            this.textBoxNum_StartStation.Location = new System.Drawing.Point(80, 9);
            this.textBoxNum_StartStation.Name = "textBoxNum_StartStation";
            this.textBoxNum_StartStation.Size = new System.Drawing.Size(99, 21);
            this.textBoxNum_StartStation.TabIndex = 12;
            this.textBoxNum_StartStation.Text = "0";
            this.textBoxNum_StartStation.TextChanged += new System.EventHandler(this.textBoxNum_Transform_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 62);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 12);
            this.label9.TabIndex = 11;
            this.label9.Text = "X缩放比例";
            // 
            // textBoxNum_xScale
            // 
            this.textBoxNum_xScale.Location = new System.Drawing.Point(80, 59);
            this.textBoxNum_xScale.Name = "textBoxNum_xScale";
            this.textBoxNum_xScale.Size = new System.Drawing.Size(99, 21);
            this.textBoxNum_xScale.TabIndex = 12;
            this.textBoxNum_xScale.Text = "1";
            this.textBoxNum_xScale.TextChanged += new System.EventHandler(this.textBoxNum_Transform_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 87);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 12);
            this.label10.TabIndex = 11;
            this.label10.Text = "Y缩放比例";
            // 
            // textBoxNum_yScale
            // 
            this.textBoxNum_yScale.Location = new System.Drawing.Point(80, 83);
            this.textBoxNum_yScale.Name = "textBoxNum_yScale";
            this.textBoxNum_yScale.Size = new System.Drawing.Size(99, 21);
            this.textBoxNum_yScale.TabIndex = 12;
            this.textBoxNum_yScale.Text = "1";
            this.textBoxNum_yScale.TextChanged += new System.EventHandler(this.textBoxNum_Transform_TextChanged);
            // 
            // panel_Transform
            // 
            this.panel_Transform.Controls.Add(this.textBoxNum_yScale);
            this.panel_Transform.Controls.Add(this.textBoxNum_xScale);
            this.panel_Transform.Controls.Add(this.textBoxNum_StartElevation);
            this.panel_Transform.Controls.Add(this.textBoxNum_StartStation);
            this.panel_Transform.Controls.Add(this.label9);
            this.panel_Transform.Controls.Add(this.label1);
            this.panel_Transform.Controls.Add(this.label8);
            this.panel_Transform.Controls.Add(this.label10);
            this.panel_Transform.Enabled = false;
            this.panel_Transform.Location = new System.Drawing.Point(183, 12);
            this.panel_Transform.Name = "panel_Transform";
            this.panel_Transform.Size = new System.Drawing.Size(182, 113);
            this.panel_Transform.TabIndex = 13;
            // 
            // textBoxNum_StartElevation
            // 
            this.textBoxNum_StartElevation.Location = new System.Drawing.Point(80, 32);
            this.textBoxNum_StartElevation.Name = "textBoxNum_StartElevation";
            this.textBoxNum_StartElevation.Size = new System.Drawing.Size(99, 21);
            this.textBoxNum_StartElevation.TabIndex = 12;
            this.textBoxNum_StartElevation.Text = "0";
            this.textBoxNum_StartElevation.TextChanged += new System.EventHandler(this.textBoxNum_Transform_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "起点高程";
            // 
            // dataGridView_Excludes
            // 
            this.dataGridView_Excludes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_Excludes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Excludes.Location = new System.Drawing.Point(19, 143);
            this.dataGridView_Excludes.Name = "dataGridView_Excludes";
            this.dataGridView_Excludes.RowTemplate.Height = 23;
            this.dataGridView_Excludes.Size = new System.Drawing.Size(346, 134);
            this.dataGridView_Excludes.TabIndex = 14;
            // 
            // button_DeleteRange
            // 
            this.button_DeleteRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_DeleteRange.Location = new System.Drawing.Point(63, 330);
            this.button_DeleteRange.Name = "button_DeleteRange";
            this.button_DeleteRange.Size = new System.Drawing.Size(38, 23);
            this.button_DeleteRange.TabIndex = 15;
            this.button_DeleteRange.Text = "删除";
            this.button_DeleteRange.UseVisualStyleBackColor = true;
            this.button_DeleteRange.Click += new System.EventHandler(this.button_DeleteRange_Click);
            // 
            // button_ExtractFromCurve
            // 
            this.button_ExtractFromCurve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_ExtractFromCurve.Enabled = false;
            this.button_ExtractFromCurve.Location = new System.Drawing.Point(19, 331);
            this.button_ExtractFromCurve.Name = "button_ExtractFromCurve";
            this.button_ExtractFromCurve.Size = new System.Drawing.Size(38, 23);
            this.button_ExtractFromCurve.TabIndex = 16;
            this.button_ExtractFromCurve.Text = "提取";
            this.button_ExtractFromCurve.UseVisualStyleBackColor = true;
            this.button_ExtractFromCurve.Click += new System.EventHandler(this.button_ExtractFromCurve_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButton_LongitudinalStairExcav);
            this.groupBox1.Controls.Add(this.radioButton_FillCutInters);
            this.groupBox1.Location = new System.Drawing.Point(19, 284);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(346, 40);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "导出项目";
            // 
            // radioButton_LongitudinalStairExcav
            // 
            this.radioButton_LongitudinalStairExcav.AutoSize = true;
            this.radioButton_LongitudinalStairExcav.Checked = true;
            this.radioButton_LongitudinalStairExcav.Location = new System.Drawing.Point(108, 18);
            this.radioButton_LongitudinalStairExcav.Name = "radioButton_LongitudinalStairExcav";
            this.radioButton_LongitudinalStairExcav.Size = new System.Drawing.Size(83, 16);
            this.radioButton_LongitudinalStairExcav.TabIndex = 0;
            this.radioButton_LongitudinalStairExcav.TabStop = true;
            this.radioButton_LongitudinalStairExcav.Text = "纵向挖台阶";
            this.radioButton_LongitudinalStairExcav.UseVisualStyleBackColor = true;
            // 
            // radioButton_FillCutInters
            // 
            this.radioButton_FillCutInters.AutoSize = true;
            this.radioButton_FillCutInters.Location = new System.Drawing.Point(7, 18);
            this.radioButton_FillCutInters.Name = "radioButton_FillCutInters";
            this.radioButton_FillCutInters.Size = new System.Drawing.Size(71, 16);
            this.radioButton_FillCutInters.TabIndex = 0;
            this.radioButton_FillCutInters.Text = "填挖交界";
            this.radioButton_FillCutInters.UseVisualStyleBackColor = true;
            // 
            // PF_ExportFillCutInters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 364);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_ExtractFromCurve);
            this.Controls.Add(this.label_roadCenter);
            this.Controls.Add(this.button_DeleteRange);
            this.Controls.Add(this.panel_Transform);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label_ground);
            this.Controls.Add(this.dataGridView_Excludes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "PF_ExportFillCutInters";
            this.Text = "填挖交界";
            this.Controls.SetChildIndex(this.dataGridView_Excludes, 0);
            this.Controls.SetChildIndex(this.label_ground, 0);
            this.Controls.SetChildIndex(this.btn_RunCmd, 0);
            this.Controls.SetChildIndex(this.btn_CancelCmd, 0);
            this.Controls.SetChildIndex(this.btn_ViewUI, 0);
            this.Controls.SetChildIndex(this.label7, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.panel_Transform, 0);
            this.Controls.SetChildIndex(this.button_DeleteRange, 0);
            this.Controls.SetChildIndex(this.label_roadCenter, 0);
            this.Controls.SetChildIndex(this.button_ExtractFromCurve, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.panel_Transform.ResumeLayout(false);
            this.panel_Transform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Excludes)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label_roadCenter;
        private System.Windows.Forms.Label label_ground;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private eZstd.UserControls.TextBoxNum textBoxNum_StartStation;
        private System.Windows.Forms.Label label9;
        private eZstd.UserControls.TextBoxNum textBoxNum_xScale;
        private System.Windows.Forms.Label label10;
        private eZstd.UserControls.TextBoxNum textBoxNum_yScale;
        private System.Windows.Forms.Panel panel_Transform;
        private System.Windows.Forms.DataGridView dataGridView_Excludes;
        private System.Windows.Forms.Button button_DeleteRange;
        private eZstd.UserControls.TextBoxNum textBoxNum_StartElevation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_ExtractFromCurve;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_LongitudinalStairExcav;
        private System.Windows.Forms.RadioButton radioButton_FillCutInters;
    }
}