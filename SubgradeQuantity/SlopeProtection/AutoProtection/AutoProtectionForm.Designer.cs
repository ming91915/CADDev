using System;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    partial class AutoProtectionForm
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
            this.cmb_Operator = new System.Windows.Forms.ComboBox();
            this.listBox_Criterions = new System.Windows.Forms.ListBox();
            this.textBoxNum_Start = new eZstd.UserControls.TextBoxNum();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxNum_End = new eZstd.UserControls.TextBoxNum();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_delete = new System.Windows.Forms.Button();
            this.btn_append = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_ImportData = new System.Windows.Forms.Button();
            this.btn_ExportData = new System.Windows.Forms.Button();
            this.cbb_AutoSchemes = new System.Windows.Forms.ComboBox();
            this.btn_AddScheme = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dgv_Slope = new eZcad.SubgradeQuantity.SlopeProtection.AutoProtectionForm.SlopeCriterionController();
            this.button_RenameScheme = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Slope)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(920, 536);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Size = new System.Drawing.Size(100, 29);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(1028, 536);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Size = new System.Drawing.Size(100, 29);
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cmb_Operator
            // 
            this.cmb_Operator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmb_Operator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Operator.FormattingEnabled = true;
            this.cmb_Operator.Location = new System.Drawing.Point(67, 9);
            this.cmb_Operator.Margin = new System.Windows.Forms.Padding(4);
            this.cmb_Operator.Name = "cmb_Operator";
            this.cmb_Operator.Size = new System.Drawing.Size(171, 23);
            this.cmb_Operator.TabIndex = 2;
            // 
            // listBox_Criterions
            // 
            this.listBox_Criterions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_Criterions.FormattingEnabled = true;
            this.listBox_Criterions.ItemHeight = 15;
            this.listBox_Criterions.Location = new System.Drawing.Point(4, 129);
            this.listBox_Criterions.Margin = new System.Windows.Forms.Padding(4);
            this.listBox_Criterions.Name = "listBox_Criterions";
            this.listBox_Criterions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_Criterions.Size = new System.Drawing.Size(248, 274);
            this.listBox_Criterions.TabIndex = 3;
            // 
            // textBoxNum_Start
            // 
            this.textBoxNum_Start.Location = new System.Drawing.Point(67, 54);
            this.textBoxNum_Start.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxNum_Start.Name = "textBoxNum_Start";
            this.textBoxNum_Start.Size = new System.Drawing.Size(75, 25);
            this.textBoxNum_Start.TabIndex = 4;
            this.textBoxNum_Start.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "运算符";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBoxNum_End);
            this.panel1.Controls.Add(this.textBoxNum_Start);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cmb_Operator);
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(249, 92);
            this.panel1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "值";
            // 
            // textBoxNum_End
            // 
            this.textBoxNum_End.Location = new System.Drawing.Point(151, 54);
            this.textBoxNum_End.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxNum_End.Name = "textBoxNum_End";
            this.textBoxNum_End.Size = new System.Drawing.Size(76, 25);
            this.textBoxNum_End.TabIndex = 4;
            this.textBoxNum_End.Text = "1";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.btn_delete);
            this.panel2.Controls.Add(this.btn_append);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.listBox_Criterions);
            this.panel2.Location = new System.Drawing.Point(868, 15);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(260, 445);
            this.panel2.TabIndex = 7;
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_delete.Location = new System.Drawing.Point(4, 409);
            this.btn_delete.Margin = new System.Windows.Forms.Padding(4);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(100, 29);
            this.btn_delete.TabIndex = 7;
            this.btn_delete.Text = "删除";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // btn_append
            // 
            this.btn_append.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_append.Location = new System.Drawing.Point(153, 409);
            this.btn_append.Margin = new System.Windows.Forms.Padding(4);
            this.btn_append.Name = "btn_append";
            this.btn_append.Size = new System.Drawing.Size(100, 29);
            this.btn_append.TabIndex = 7;
            this.btn_append.Text = "添加";
            this.btn_append.UseVisualStyleBackColor = true;
            this.btn_append.Click += new System.EventHandler(this.btn_append_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 105);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "多条件 And 连接";
            // 
            // btn_ImportData
            // 
            this.btn_ImportData.Location = new System.Drawing.Point(640, 12);
            this.btn_ImportData.Margin = new System.Windows.Forms.Padding(4);
            this.btn_ImportData.Name = "btn_ImportData";
            this.btn_ImportData.Size = new System.Drawing.Size(100, 29);
            this.btn_ImportData.TabIndex = 10;
            this.btn_ImportData.Text = "导入";
            this.btn_ImportData.UseVisualStyleBackColor = true;
            this.btn_ImportData.Click += new System.EventHandler(this.btn_ImportData_Click);
            // 
            // btn_ExportData
            // 
            this.btn_ExportData.Location = new System.Drawing.Point(748, 12);
            this.btn_ExportData.Margin = new System.Windows.Forms.Padding(4);
            this.btn_ExportData.Name = "btn_ExportData";
            this.btn_ExportData.Size = new System.Drawing.Size(100, 29);
            this.btn_ExportData.TabIndex = 10;
            this.btn_ExportData.Text = "导出";
            this.btn_ExportData.UseVisualStyleBackColor = true;
            this.btn_ExportData.Click += new System.EventHandler(this.btn_ExportData_Click);
            // 
            // cbb_AutoSchemes
            // 
            this.cbb_AutoSchemes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbb_AutoSchemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_AutoSchemes.FormattingEnabled = true;
            this.cbb_AutoSchemes.Location = new System.Drawing.Point(99, 14);
            this.cbb_AutoSchemes.Margin = new System.Windows.Forms.Padding(4);
            this.cbb_AutoSchemes.Name = "cbb_AutoSchemes";
            this.cbb_AutoSchemes.Size = new System.Drawing.Size(319, 23);
            this.cbb_AutoSchemes.TabIndex = 11;
            cbb_AutoSchemes.SelectedValueChanged += CbbAutoSchemesOnSelectedValueChanged;
            // 
            // btn_AddScheme
            // 
            this.btn_AddScheme.Location = new System.Drawing.Point(426, 12);
            this.btn_AddScheme.Margin = new System.Windows.Forms.Padding(4);
            this.btn_AddScheme.Name = "btn_AddScheme";
            this.btn_AddScheme.Size = new System.Drawing.Size(100, 29);
            this.btn_AddScheme.TabIndex = 12;
            this.btn_AddScheme.Text = "添加方案";
            this.btn_AddScheme.UseVisualStyleBackColor = true;
            this.btn_AddScheme.Click += new System.EventHandler(this.btn_AddScheme_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 13;
            this.label6.Text = "当前方案";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 57);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(382, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "边坡防护规则（添加行时先设置其填挖方或者防护方式）";
            // 
            // dgv_Slope
            // 
            this.dgv_Slope.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_Slope.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Slope.KeyDelete = false;
            this.dgv_Slope.Location = new System.Drawing.Point(20, 82);
            this.dgv_Slope.ManipulateRows = false;
            this.dgv_Slope.Margin = new System.Windows.Forms.Padding(4);
            this.dgv_Slope.Name = "dgv_Slope";
            this.dgv_Slope.RowTemplate.Height = 23;
            this.dgv_Slope.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_Slope.ShowRowNumber = false;
            this.dgv_Slope.Size = new System.Drawing.Size(836, 483);
            this.dgv_Slope.SupportPaste = false;
            this.dgv_Slope.TabIndex = 14;
            // 
            // button_RenameScheme
            // 
            this.button_RenameScheme.Location = new System.Drawing.Point(533, 12);
            this.button_RenameScheme.Name = "button_RenameScheme";
            this.button_RenameScheme.Size = new System.Drawing.Size(100, 29);
            this.button_RenameScheme.TabIndex = 16;
            this.button_RenameScheme.Text = "重命名";
            this.button_RenameScheme.UseVisualStyleBackColor = true;
            this.button_RenameScheme.Click += new System.EventHandler(this.button_RenameScheme_Click);
            // 
            // AutoProtectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 580);
            this.Controls.Add(this.button_RenameScheme);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dgv_Slope);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btn_AddScheme);
            this.Controls.Add(this.cbb_AutoSchemes);
            this.Controls.Add(this.btn_ExportData);
            this.Controls.Add(this.btn_ImportData);
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AutoProtectionForm";
            this.Text = "边坡自动防护规则";
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.btn_ImportData, 0);
            this.Controls.SetChildIndex(this.btn_ExportData, 0);
            this.Controls.SetChildIndex(this.cbb_AutoSchemes, 0);
            this.Controls.SetChildIndex(this.btn_AddScheme, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.dgv_Slope, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.button_RenameScheme, 0);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Slope)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmb_Operator;
        private System.Windows.Forms.ListBox listBox_Criterions;
        private eZstd.UserControls.TextBoxNum textBoxNum_Start;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private eZstd.UserControls.TextBoxNum textBoxNum_End;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_append;
        private System.Windows.Forms.Button btn_ImportData;
        private System.Windows.Forms.Button btn_ExportData;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbb_AutoSchemes;
        private System.Windows.Forms.Button btn_AddScheme;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private SlopeCriterionController dgv_Slope;
        private System.Windows.Forms.Button button_RenameScheme;
    }
}