namespace eZcad.SubgradeQuantity.Options
{
    partial class Form_ProjectInfos
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
            this.dgv_LayerOptions = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_StationFieldDef = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBoxNum_RoadWidth = new eZstd.UserControls.TextBoxNum();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_Waterlevel = new eZstd.UserControls.TextBoxNum();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_FillAboveWater = new System.Windows.Forms.CheckBox();
            this.panel_FillWater = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_FillAboveWater = new eZstd.UserControls.TextBoxNum();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_LayerOptions)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel_FillWater.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(497, 400);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(578, 400);
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // dgv_LayerOptions
            // 
            this.dgv_LayerOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_LayerOptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_LayerOptions.Location = new System.Drawing.Point(308, 40);
            this.dgv_LayerOptions.Name = "dgv_LayerOptions";
            this.dgv_LayerOptions.RowTemplate.Height = 23;
            this.dgv_LayerOptions.Size = new System.Drawing.Size(347, 354);
            this.dgv_LayerOptions.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(306, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "图层名";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "里程属性定义名";
            // 
            // textBox_StationFieldDef
            // 
            this.textBox_StationFieldDef.Location = new System.Drawing.Point(115, 165);
            this.textBox_StationFieldDef.Name = "textBox_StationFieldDef";
            this.textBox_StationFieldDef.Size = new System.Drawing.Size(102, 21);
            this.textBox_StationFieldDef.TabIndex = 4;
            this.toolTip1.SetToolTip(this.textBox_StationFieldDef, "横断面信息的块参照中，标识里程信息的属性定义的名称");
            // 
            // textBoxNum_RoadWidth
            // 
            this.textBoxNum_RoadWidth.Location = new System.Drawing.Point(90, 114);
            this.textBoxNum_RoadWidth.Name = "textBoxNum_RoadWidth";
            this.textBoxNum_RoadWidth.PositiveOnly = true;
            this.textBoxNum_RoadWidth.Size = new System.Drawing.Size(100, 21);
            this.textBoxNum_RoadWidth.TabIndex = 6;
            this.toolTip1.SetToolTip(this.textBoxNum_RoadWidth, "道路的大致宽度，用来搜索边坡线附近的道路中线。其值不要小于道路的最大宽度。但其值越大搜索范围越广，也越耗时。");
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBox_Waterlevel);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.checkBox_FillAboveWater);
            this.panel1.Controls.Add(this.textBoxNum_RoadWidth);
            this.panel1.Controls.Add(this.panel_FillWater);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(275, 144);
            this.panel1.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "水位标高";
            // 
            // textBox_Waterlevel
            // 
            this.textBox_Waterlevel.Location = new System.Drawing.Point(72, 12);
            this.textBox_Waterlevel.Name = "textBox_Waterlevel";
            this.textBox_Waterlevel.Size = new System.Drawing.Size(100, 21);
            this.textBox_Waterlevel.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(198, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "m";
            // 
            // checkBox_FillAboveWater
            // 
            this.checkBox_FillAboveWater.AutoSize = true;
            this.checkBox_FillAboveWater.Checked = true;
            this.checkBox_FillAboveWater.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_FillAboveWater.Location = new System.Drawing.Point(12, 44);
            this.checkBox_FillAboveWater.Name = "checkBox_FillAboveWater";
            this.checkBox_FillAboveWater.Size = new System.Drawing.Size(120, 16);
            this.checkBox_FillAboveWater.TabIndex = 3;
            this.checkBox_FillAboveWater.Text = "填方防护考虑水位";
            this.checkBox_FillAboveWater.UseVisualStyleBackColor = true;
            this.checkBox_FillAboveWater.CheckedChanged += new System.EventHandler(this.checkBox_FillAboveWater_CheckedChanged);
            // 
            // panel_FillWater
            // 
            this.panel_FillWater.Controls.Add(this.label4);
            this.panel_FillWater.Controls.Add(this.label6);
            this.panel_FillWater.Controls.Add(this.textBox_FillAboveWater);
            this.panel_FillWater.Location = new System.Drawing.Point(4, 66);
            this.panel_FillWater.Name = "panel_FillWater";
            this.panel_FillWater.Size = new System.Drawing.Size(269, 32);
            this.panel_FillWater.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "填方顶位于水位标高之上";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(252, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(11, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "m";
            // 
            // textBox_FillAboveWater
            // 
            this.textBox_FillAboveWater.Location = new System.Drawing.Point(147, 7);
            this.textBox_FillAboveWater.Name = "textBox_FillAboveWater";
            this.textBox_FillAboveWater.Size = new System.Drawing.Size(100, 21);
            this.textBox_FillAboveWater.TabIndex = 1;
            this.textBox_FillAboveWater.Text = "1.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 117);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 5;
            this.label7.Text = "道路最大宽度";
            // 
            // Form_ProjectInfos
            // 
            this.AcceptButton = null;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 435);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgv_LayerOptions);
            this.Controls.Add(this.textBox_StationFieldDef);
            this.Controls.Add(this.label3);
            this.MinimumSize = new System.Drawing.Size(438, 394);
            this.Name = "Form_ProjectInfos";
            this.Text = "项目信息";
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.textBox_StationFieldDef, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.dgv_LayerOptions, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.panel1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_LayerOptions)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel_FillWater.ResumeLayout(false);
            this.panel_FillWater.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_LayerOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_StationFieldDef;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private eZstd.UserControls.TextBoxNum textBox_Waterlevel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_FillAboveWater;
        private eZstd.UserControls.TextBoxNum textBoxNum_RoadWidth;
        private System.Windows.Forms.Panel panel_FillWater;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private eZstd.UserControls.TextBoxNum textBox_FillAboveWater;
        private System.Windows.Forms.Label label7;
    }
}