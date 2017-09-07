
using System.Windows.Forms;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    partial class SlopeSegLister
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_Ok = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.listBox_slopes = new System.Windows.Forms.ListBox();
            this.btn_ImportProtectRull = new System.Windows.Forms.Button();
            this.AutoProtect = new System.Windows.Forms.Button();
            this.btn_selectAll = new System.Windows.Forms.Button();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.checkBox_SeperateFillCut = new System.Windows.Forms.CheckBox();
            this.cbb_currentSlope = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(376, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "设置边坡或平台的防护";
            // 
            // btn_Ok
            // 
            this.btn_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Ok.Location = new System.Drawing.Point(978, 395);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(75, 23);
            this.btn_Ok.TabIndex = 3;
            this.btn_Ok.Text = "确定";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Clear.Location = new System.Drawing.Point(897, 395);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(75, 23);
            this.button_Clear.TabIndex = 4;
            this.button_Clear.Text = "清除数据";
            this.button_Clear.UseVisualStyleBackColor = true;
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // listBox_slopes
            // 
            this.listBox_slopes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_slopes.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBox_slopes.FormattingEnabled = true;
            this.listBox_slopes.ItemHeight = 16;
            this.listBox_slopes.Location = new System.Drawing.Point(12, 12);
            this.listBox_slopes.Name = "listBox_slopes";
            this.listBox_slopes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_slopes.Size = new System.Drawing.Size(358, 372);
            this.listBox_slopes.TabIndex = 5;
            this.listBox_slopes.SelectedValueChanged += new System.EventHandler(this.listBox_slopes_SelectedValueChanged);
            this.listBox_slopes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_slopes_MouseDoubleClick);
            // 
            // btn_ImportProtectRull
            // 
            this.btn_ImportProtectRull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ImportProtectRull.Location = new System.Drawing.Point(157, 395);
            this.btn_ImportProtectRull.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btn_ImportProtectRull.Name = "btn_ImportProtectRull";
            this.btn_ImportProtectRull.Size = new System.Drawing.Size(75, 23);
            this.btn_ImportProtectRull.TabIndex = 6;
            this.btn_ImportProtectRull.Text = "导入规则";
            this.btn_ImportProtectRull.UseVisualStyleBackColor = true;
            this.btn_ImportProtectRull.Click += new System.EventHandler(this.btn_btn_ImportProtectRull_Click);
            // 
            // AutoProtect
            // 
            this.AutoProtect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoProtect.Location = new System.Drawing.Point(237, 395);
            this.AutoProtect.Name = "AutoProtect";
            this.AutoProtect.Size = new System.Drawing.Size(75, 23);
            this.AutoProtect.TabIndex = 8;
            this.AutoProtect.Text = "自动防护";
            this.AutoProtect.UseVisualStyleBackColor = true;
            this.AutoProtect.Click += new System.EventHandler(this.AutoProtect_Click);
            // 
            // btn_selectAll
            // 
            this.btn_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_selectAll.Location = new System.Drawing.Point(13, 394);
            this.btn_selectAll.Name = "btn_selectAll";
            this.btn_selectAll.Size = new System.Drawing.Size(75, 23);
            this.btn_selectAll.TabIndex = 9;
            this.btn_selectAll.Text = "全选";
            this.btn_selectAll.UseVisualStyleBackColor = true;
            this.btn_selectAll.Click += new System.EventHandler(this.btn_selectAll_Click);
            // 
            // dgv
            // 
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Location = new System.Drawing.Point(376, 40);
            this.dgv.Name = "dgv";
            this.dgv.RowTemplate.Height = 23;
            this.dgv.Size = new System.Drawing.Size(674, 349);
            this.dgv.TabIndex = 7;
            // 
            // checkBox_SeperateFillCut
            // 
            this.checkBox_SeperateFillCut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_SeperateFillCut.AutoSize = true;
            this.checkBox_SeperateFillCut.Checked = true;
            this.checkBox_SeperateFillCut.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_SeperateFillCut.Location = new System.Drawing.Point(320, 401);
            this.checkBox_SeperateFillCut.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_SeperateFillCut.Name = "checkBox_SeperateFillCut";
            this.checkBox_SeperateFillCut.Size = new System.Drawing.Size(156, 16);
            this.checkBox_SeperateFillCut.TabIndex = 10;
            this.checkBox_SeperateFillCut.Text = "统一修改时区分填方挖方";
            this.checkBox_SeperateFillCut.UseVisualStyleBackColor = true;
            // 
            // cbb_currentSlope
            // 
            this.cbb_currentSlope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbb_currentSlope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_currentSlope.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbb_currentSlope.FormattingEnabled = true;
            this.cbb_currentSlope.Location = new System.Drawing.Point(506, 12);
            this.cbb_currentSlope.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbb_currentSlope.Name = "cbb_currentSlope";
            this.cbb_currentSlope.Size = new System.Drawing.Size(545, 24);
            this.cbb_currentSlope.TabIndex = 11;
            this.cbb_currentSlope.SelectedValueChanged += new System.EventHandler(this.cbb_currentSlope_SelectedValueChanged);
            // 
            // SlopeSegLister
            // 
            this.AcceptButton = this.btn_Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 426);
            this.Controls.Add(this.cbb_currentSlope);
            this.Controls.Add(this.checkBox_SeperateFillCut);
            this.Controls.Add(this.btn_selectAll);
            this.Controls.Add(this.AutoProtect);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.btn_ImportProtectRull);
            this.Controls.Add(this.listBox_slopes);
            this.Controls.Add(this.button_Clear);
            this.Controls.Add(this.btn_Ok);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(950, 463);
            this.Name = "SlopeSegLister";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "边坡防护";
            this.Shown += new System.EventHandler(this.ProtectionStyleLister_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProtectionStyleLister_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.Button button_Clear;
        private System.Windows.Forms.ListBox listBox_slopes;
        private System.Windows.Forms.Button btn_ImportProtectRull;
        private DataGridView dgv;
        private System.Windows.Forms.Button AutoProtect;
        private System.Windows.Forms.Button btn_selectAll;
        private CheckBox checkBox_SeperateFillCut;
        private ComboBox cbb_currentSlope;
    }
}