namespace eZcad.SubgradeQuantity.DataExport
{
    partial class CriterionEditor
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
            this.btn_ExportToXml = new System.Windows.Forms.Button();
            this.btn_LoadFromXml = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Size = new System.Drawing.Size(639, 366);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(493, 384);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(574, 384);
            // 
            // btn_ExportToXml
            // 
            this.btn_ExportToXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ExportToXml.Location = new System.Drawing.Point(10, 383);
            this.btn_ExportToXml.Name = "btn_ExportToXml";
            this.btn_ExportToXml.Size = new System.Drawing.Size(75, 23);
            this.btn_ExportToXml.TabIndex = 2;
            this.btn_ExportToXml.Text = "导出";
            this.btn_ExportToXml.UseVisualStyleBackColor = true;
            this.btn_ExportToXml.Click += new System.EventHandler(this.btn_ExportToXml_Click);
            // 
            // btn_LoadFromXml
            // 
            this.btn_LoadFromXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_LoadFromXml.Location = new System.Drawing.Point(91, 384);
            this.btn_LoadFromXml.Name = "btn_LoadFromXml";
            this.btn_LoadFromXml.Size = new System.Drawing.Size(75, 23);
            this.btn_LoadFromXml.TabIndex = 2;
            this.btn_LoadFromXml.Text = "导入";
            this.btn_LoadFromXml.UseVisualStyleBackColor = true;
            this.btn_LoadFromXml.Click += new System.EventHandler(this.btn_LoadFromXml_Click);
            // 
            // CriterionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 419);
            this.Controls.Add(this.btn_LoadFromXml);
            this.Controls.Add(this.btn_ExportToXml);
            this.Name = "CriterionEditor";
            this.Text = "CriterionEditor";
            this.Controls.SetChildIndex(this.propertyGrid1, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btn_ExportToXml, 0);
            this.Controls.SetChildIndex(this.btn_LoadFromXml, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_ExportToXml;
        private System.Windows.Forms.Button btn_LoadFromXml;
    }
}