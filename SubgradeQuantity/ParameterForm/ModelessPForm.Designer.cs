namespace eZcad.SubgradeQuantity.ParameterForm
{
    partial class ModelessPForm
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
            this.SuspendLayout();
            // 
            // ParameterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "ParameterForm";
            this.Text = "ParameterForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParameterForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ParameterForm_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ParameterForm_KeyDown);
            this.MouseEnter += new System.EventHandler(this.ParameterForm_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ParameterForm_MouseLeave);
            this.ResumeLayout(false);

        }

        #endregion
    }
}