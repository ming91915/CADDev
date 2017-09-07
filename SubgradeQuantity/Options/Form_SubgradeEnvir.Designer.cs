using eZstd.UserControls;

namespace eZcad.SubgradeQuantity.Options
{
    partial class Form_SubgradeEnvir
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
            this.dgv_Structures = new eZstd.UserControls.eZDataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_SoilRockRanges = new System.Windows.Forms.TabPage();
            this.dgv_SoilRockRange = new eZstd.UserControls.eZDataGridView();
            this.tabPage_Structures = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Structures)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage_SoilRockRanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SoilRockRange)).BeginInit();
            this.tabPage_Structures.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(476, 464);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Size = new System.Drawing.Size(100, 29);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(584, 464);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Size = new System.Drawing.Size(100, 29);
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // dgv_Structures
            // 
            this.dgv_Structures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Structures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Structures.KeyDelete = false;
            this.dgv_Structures.Location = new System.Drawing.Point(3, 2);
            this.dgv_Structures.Margin = new System.Windows.Forms.Padding(4);
            this.dgv_Structures.Name = "dgv_Structures";
            this.dgv_Structures.RowTemplate.Height = 23;
            this.dgv_Structures.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_Structures.Size = new System.Drawing.Size(659, 411);
            this.dgv_Structures.SupportPaste = false;
            this.dgv_Structures.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_SoilRockRanges);
            this.tabControl1.Controls.Add(this.tabPage_Structures);
            this.tabControl1.Location = new System.Drawing.Point(15, 14);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(673, 444);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage_SoilRockRanges
            // 
            this.tabPage_SoilRockRanges.Controls.Add(this.dgv_SoilRockRange);
            this.tabPage_SoilRockRanges.Location = new System.Drawing.Point(4, 25);
            this.tabPage_SoilRockRanges.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_SoilRockRanges.Name = "tabPage_SoilRockRanges";
            this.tabPage_SoilRockRanges.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_SoilRockRanges.Size = new System.Drawing.Size(665, 415);
            this.tabPage_SoilRockRanges.TabIndex = 1;
            this.tabPage_SoilRockRanges.Text = "岩土分区";
            this.tabPage_SoilRockRanges.UseVisualStyleBackColor = true;
            // 
            // dgv_SoilRockRange
            // 
            this.dgv_SoilRockRange.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_SoilRockRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_SoilRockRange.KeyDelete = false;
            this.dgv_SoilRockRange.Location = new System.Drawing.Point(3, 2);
            this.dgv_SoilRockRange.Margin = new System.Windows.Forms.Padding(4);
            this.dgv_SoilRockRange.Name = "dgv_SoilRockRange";
            this.dgv_SoilRockRange.RowTemplate.Height = 23;
            this.dgv_SoilRockRange.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_SoilRockRange.Size = new System.Drawing.Size(659, 411);
            this.dgv_SoilRockRange.SupportPaste = false;
            this.dgv_SoilRockRange.TabIndex = 1;
            // 
            // tabPage_Structures
            // 
            this.tabPage_Structures.Controls.Add(this.dgv_Structures);
            this.tabPage_Structures.Location = new System.Drawing.Point(4, 25);
            this.tabPage_Structures.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_Structures.Name = "tabPage_Structures";
            this.tabPage_Structures.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_Structures.Size = new System.Drawing.Size(665, 415);
            this.tabPage_Structures.TabIndex = 0;
            this.tabPage_Structures.Text = "结构物";
            this.tabPage_Structures.UseVisualStyleBackColor = true;
            // 
            // Form_SubgradeEnvir
            // 
            this.AcceptButton = null;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 508);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(578, 481);
            this.Name = "Form_SubgradeEnvir";
            this.Text = "道路环境配置";
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.tabControl1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Structures)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_SoilRockRanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SoilRockRange)).EndInit();
            this.tabPage_Structures.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private eZDataGridView dgv_Structures;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_Structures;
        private System.Windows.Forms.TabPage tabPage_SoilRockRanges;
        private eZDataGridView dgv_SoilRockRange;
    }
}