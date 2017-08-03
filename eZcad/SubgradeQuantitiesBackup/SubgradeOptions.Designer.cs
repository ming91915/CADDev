using eZstd.UserControls;

namespace eZcad.SubgradeQuantityBackup
{
    partial class SubgradeOptions
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_Waterlevel = new eZstd.UserControls.TextBoxNum();
            this.button_MatchRules = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_FillAboveWater = new eZstd.UserControls.TextBoxNum();
            this.checkBox_FillAboveWater = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel_FillWater = new System.Windows.Forms.Panel();
            this.textBoxNum_RoadWidth = new eZstd.UserControls.TextBoxNum();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.flp_Criterions = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_criterion_ThinFillShallowCut = new System.Windows.Forms.Button();
            this.btn_criterion_ThickFillDeepCut = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.panel_FillWater.SuspendLayout();
            this.flp_Criterions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(131, 286);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(212, 286);
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "水位标高";
            // 
            // textBox_Waterlevel
            // 
            this.textBox_Waterlevel.Location = new System.Drawing.Point(71, 19);
            this.textBox_Waterlevel.Name = "textBox_Waterlevel";
            this.textBox_Waterlevel.Size = new System.Drawing.Size(100, 21);
            this.textBox_Waterlevel.TabIndex = 1;
            // 
            // button_MatchRules
            // 
            this.button_MatchRules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_MatchRules.Location = new System.Drawing.Point(12, 286);
            this.button_MatchRules.Name = "button_MatchRules";
            this.button_MatchRules.Size = new System.Drawing.Size(75, 23);
            this.button_MatchRules.TabIndex = 2;
            this.button_MatchRules.Text = "图元匹配";
            this.button_MatchRules.UseVisualStyleBackColor = true;
            this.button_MatchRules.Click += new System.EventHandler(this.button_MatchRules_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "填方顶位于水位标高之上";
            // 
            // textBox_FillAboveWater
            // 
            this.textBox_FillAboveWater.Location = new System.Drawing.Point(146, 7);
            this.textBox_FillAboveWater.Name = "textBox_FillAboveWater";
            this.textBox_FillAboveWater.Size = new System.Drawing.Size(100, 21);
            this.textBox_FillAboveWater.TabIndex = 1;
            this.textBox_FillAboveWater.Text = "1.0";
            // 
            // checkBox_FillAboveWater
            // 
            this.checkBox_FillAboveWater.AutoSize = true;
            this.checkBox_FillAboveWater.Checked = true;
            this.checkBox_FillAboveWater.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_FillAboveWater.Location = new System.Drawing.Point(14, 60);
            this.checkBox_FillAboveWater.Name = "checkBox_FillAboveWater";
            this.checkBox_FillAboveWater.Size = new System.Drawing.Size(120, 16);
            this.checkBox_FillAboveWater.TabIndex = 3;
            this.checkBox_FillAboveWater.Text = "填方防护考虑水位";
            this.checkBox_FillAboveWater.UseVisualStyleBackColor = true;
            this.checkBox_FillAboveWater.CheckedChanged += new System.EventHandler(this.checkBox_FillAboveWater_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "m";
            // 
            // panel_FillWater
            // 
            this.panel_FillWater.Controls.Add(this.label2);
            this.panel_FillWater.Controls.Add(this.label3);
            this.panel_FillWater.Controls.Add(this.textBox_FillAboveWater);
            this.panel_FillWater.Location = new System.Drawing.Point(14, 82);
            this.panel_FillWater.Name = "panel_FillWater";
            this.panel_FillWater.Size = new System.Drawing.Size(269, 32);
            this.panel_FillWater.TabIndex = 4;
            // 
            // textBoxNum_RoadWidth
            // 
            this.textBoxNum_RoadWidth.Location = new System.Drawing.Point(95, 140);
            this.textBoxNum_RoadWidth.Name = "textBoxNum_RoadWidth";
            this.textBoxNum_RoadWidth.PositiveOnly = true;
            this.textBoxNum_RoadWidth.Size = new System.Drawing.Size(100, 21);
            this.textBoxNum_RoadWidth.TabIndex = 6;
            this.toolTip1.SetToolTip(this.textBoxNum_RoadWidth, "道路的大致宽度，用来搜索边坡线附近的道路中线。其值不要小于道路的最大宽度。但其值越大搜索范围越广，也越耗时。");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "道路最大宽度";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(201, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "m";
            // 
            // flp_Criterions
            // 
            this.flp_Criterions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flp_Criterions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flp_Criterions.Controls.Add(this.btn_criterion_ThinFillShallowCut);
            this.flp_Criterions.Controls.Add(this.btn_criterion_ThickFillDeepCut);
            this.flp_Criterions.Controls.Add(this.button3);
            this.flp_Criterions.Location = new System.Drawing.Point(12, 198);
            this.flp_Criterions.Name = "flp_Criterions";
            this.flp_Criterions.Size = new System.Drawing.Size(275, 82);
            this.flp_Criterions.TabIndex = 7;
            // 
            // btn_criterion_ThinFillShallowCut
            // 
            this.btn_criterion_ThinFillShallowCut.Location = new System.Drawing.Point(3, 3);
            this.btn_criterion_ThinFillShallowCut.Name = "btn_criterion_ThinFillShallowCut";
            this.btn_criterion_ThinFillShallowCut.Size = new System.Drawing.Size(75, 23);
            this.btn_criterion_ThinFillShallowCut.TabIndex = 0;
            this.btn_criterion_ThinFillShallowCut.Text = "低填浅挖";
            this.btn_criterion_ThinFillShallowCut.UseVisualStyleBackColor = true;
            this.btn_criterion_ThinFillShallowCut.Click += new System.EventHandler(this.btn_criterion_ThinFillShallowCut_Click);
            // 
            // btn_criterion_ThickFillDeepCut
            // 
            this.btn_criterion_ThickFillDeepCut.Location = new System.Drawing.Point(84, 3);
            this.btn_criterion_ThickFillDeepCut.Name = "btn_criterion_ThickFillDeepCut";
            this.btn_criterion_ThickFillDeepCut.Size = new System.Drawing.Size(75, 23);
            this.btn_criterion_ThickFillDeepCut.TabIndex = 0;
            this.btn_criterion_ThickFillDeepCut.Text = "高填深挖";
            this.btn_criterion_ThickFillDeepCut.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(165, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 0;
            this.button3.Text = "button1";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 177);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(161, 12);
            this.label6.TabIndex = 5;
            this.label6.Text = "各类工程量的判断与计量标准";
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 321);
            this.Controls.Add(this.flp_Criterions);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxNum_RoadWidth);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel_FillWater);
            this.Controls.Add(this.checkBox_FillAboveWater);
            this.Controls.Add(this.button_MatchRules);
            this.Controls.Add(this.textBox_Waterlevel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Options";
            this.Text = "设置";
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.textBox_Waterlevel, 0);
            this.Controls.SetChildIndex(this.button_MatchRules, 0);
            this.Controls.SetChildIndex(this.checkBox_FillAboveWater, 0);
            this.Controls.SetChildIndex(this.panel_FillWater, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.textBoxNum_RoadWidth, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.flp_Criterions, 0);
            this.panel_FillWater.ResumeLayout(false);
            this.panel_FillWater.PerformLayout();
            this.flp_Criterions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private TextBoxNum textBox_Waterlevel;
        private System.Windows.Forms.Button button_MatchRules;
        private System.Windows.Forms.Label label2;
        private TextBoxNum textBox_FillAboveWater;
        private System.Windows.Forms.CheckBox checkBox_FillAboveWater;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel_FillWater;
        private TextBoxNum textBoxNum_RoadWidth;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.FlowLayoutPanel flp_Criterions;
        private System.Windows.Forms.Button btn_criterion_ThinFillShallowCut;
        private System.Windows.Forms.Button btn_criterion_ThickFillDeepCut;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label6;
    }
}