namespace eZcad.Addins
{
    partial class BlockRefEditorForm
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
            this.btn_Ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_SortMultiLevel = new System.Windows.Forms.Button();
            this._eZdgv = new eZstd.UserControls.eZDataGridView();
            this.label_SelectionCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._eZdgv)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_Ok
            // 
            this.btn_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Ok.Location = new System.Drawing.Point(773, 525);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(75, 23);
            this.btn_Ok.TabIndex = 1;
            this.btn_Ok.Text = "确定";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(692, 525);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "取消";
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // btn_SortMultiLevel
            // 
            this.btn_SortMultiLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_SortMultiLevel.Location = new System.Drawing.Point(611, 525);
            this.btn_SortMultiLevel.Name = "btn_SortMultiLevel";
            this.btn_SortMultiLevel.Size = new System.Drawing.Size(75, 23);
            this.btn_SortMultiLevel.TabIndex = 2;
            this.btn_SortMultiLevel.Text = "多级排序";
            this.btn_SortMultiLevel.UseVisualStyleBackColor = true;
            this.btn_SortMultiLevel.Click += new System.EventHandler(this.btn_Sort_Click);
            // 
            // _eZdgv
            // 
            this._eZdgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._eZdgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._eZdgv.KeyDelete = false;
            this._eZdgv.Location = new System.Drawing.Point(12, 12);
            this._eZdgv.ManipulateRows = false;
            this._eZdgv.Name = "_eZdgv";
            this._eZdgv.RowTemplate.Height = 23;
            this._eZdgv.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this._eZdgv.ShowRowNumber = false;
            this._eZdgv.Size = new System.Drawing.Size(836, 507);
            this._eZdgv.SupportPaste = false;
            this._eZdgv.TabIndex = 0;
            // 
            // label_SelectionCount
            // 
            this.label_SelectionCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_SelectionCount.AutoSize = true;
            this.label_SelectionCount.Location = new System.Drawing.Point(13, 536);
            this.label_SelectionCount.Name = "label_SelectionCount";
            this.label_SelectionCount.Size = new System.Drawing.Size(71, 12);
            this.label_SelectionCount.TabIndex = 3;
            this.label_SelectionCount.Text = "选择数量：1";
            // 
            // BlockRefEditorForm
            // 
            this.AcceptButton = this.btn_Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 560);
            this.Controls.Add(this.label_SelectionCount);
            this.Controls.Add(this.btn_SortMultiLevel);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_Ok);
            this.Controls.Add(this._eZdgv);
            this.Name = "BlockRefEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "块属性编辑器";
            ((System.ComponentModel.ISupportInitialize)(this._eZdgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private eZstd.UserControls.eZDataGridView _eZdgv;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_SortMultiLevel;
        private System.Windows.Forms.Label label_SelectionCount;
    }
}