namespace eZcad.SubgradeQuantity.SlopeProtection
{
    partial class SchemeRenameForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label_OriginalName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_NewName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(111, 66);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(192, 66);
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "原名称：";
            // 
            // label_OriginalName
            // 
            this.label_OriginalName.AutoSize = true;
            this.label_OriginalName.Location = new System.Drawing.Point(66, 13);
            this.label_OriginalName.Name = "label_OriginalName";
            this.label_OriginalName.Size = new System.Drawing.Size(77, 12);
            this.label_OriginalName.TabIndex = 0;
            this.label_OriginalName.Text = "OriginalName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "新名称：";
            // 
            // textBox_NewName
            // 
            this.textBox_NewName.Location = new System.Drawing.Point(68, 35);
            this.textBox_NewName.Name = "textBox_NewName";
            this.textBox_NewName.Size = new System.Drawing.Size(199, 21);
            this.textBox_NewName.TabIndex = 1;
            // 
            // SchemeRenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 101);
            this.Controls.Add(this.textBox_NewName);
            this.Controls.Add(this.label_OriginalName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SchemeRenameForm";
            this.Text = "重命名";
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label_OriginalName, 0);
            this.Controls.SetChildIndex(this.textBox_NewName, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_OriginalName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_NewName;
    }
}