using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace eZcad.AddinManager
{
    /// <summary> 选择程序集文件的窗口 </summary>
    /// <remarks>此类是完全从 Revit AddinManager中移植过来并稍微修改的</remarks>
    public class AssemblySelectorForm : Form
    {
        #region ---   Fields
        private string m_assemName;
        private bool m_found;
        public string m_resultPath;
        private IContainer components;
        private Button okButton;
        private Button cancelButton;
        private TextBox assemPathTextBox;
        private Button browseButton;
        private Label missingAssemDescripLabel;
        private TextBox assemNameTextBox;
        private Label selectAssemLabel;
        #endregion

        #region ---   构造函数与初始化
        /// <summary> 构造函数 </summary>
        /// <param name="assemName"></param>
        public AssemblySelectorForm(string assemName)
        {
            this.InitializeComponent();
            this.m_assemName = assemName;
            this.assemNameTextBox.Text = assemName;
        }

        private void InitializeComponent()
        {
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.assemPathTextBox = new TextBox();
            this.browseButton = new Button();
            this.missingAssemDescripLabel = new Label();
            this.assemNameTextBox = new TextBox();
            this.selectAssemLabel = new Label();
            base.SuspendLayout();
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Location = new Point(213, 100);
            this.okButton.Name = "okButton";
            this.okButton.Size = new Size(63, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new EventHandler(this.okButton_Click);
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = new Point(282, 100);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(62, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.assemPathTextBox.Location = new Point(9, 74);
            this.assemPathTextBox.Name = "assemPathTextBox";
            this.assemPathTextBox.Size = new Size(290, 20);
            this.assemPathTextBox.TabIndex = 2;
            this.browseButton.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 134);
            this.browseButton.Location = new Point(305, 74);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new Size(39, 20);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "&...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new EventHandler(this.browseButton_Click);
            this.missingAssemDescripLabel.AutoSize = true;
            this.missingAssemDescripLabel.Location = new Point(6, 6);
            this.missingAssemDescripLabel.Name = "missingAssemDescripLabel";
            this.missingAssemDescripLabel.Size = new Size(309, 13);
            this.missingAssemDescripLabel.TabIndex = 4;
            this.missingAssemDescripLabel.Text = "The following assembly name can not be resolved automatically:";
            this.assemNameTextBox.BorderStyle = BorderStyle.None;
            this.assemNameTextBox.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 134);
            this.assemNameTextBox.ForeColor = SystemColors.WindowText;
            this.assemNameTextBox.Location = new Point(9, 23);
            this.assemNameTextBox.Multiline = true;
            this.assemNameTextBox.Name = "assemNameTextBox";
            this.assemNameTextBox.ReadOnly = true;
            this.assemNameTextBox.Size = new Size(294, 28);
            this.assemNameTextBox.TabIndex = 5;
            this.assemNameTextBox.Text = "I'm a text box!\r\nI'm a text box!";
            this.selectAssemLabel.AutoSize = true;
            this.selectAssemLabel.Location = new Point(6, 56);
            this.selectAssemLabel.Name = "selectAssemLabel";
            this.selectAssemLabel.Size = new Size(197, 13);
            this.selectAssemLabel.TabIndex = 6;
            this.selectAssemLabel.Text = "Please select the assembly file manually:";
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(354, 129);
            base.Controls.Add(this.selectAssemLabel);
            base.Controls.Add(this.assemNameTextBox);
            base.Controls.Add(this.missingAssemDescripLabel);
            base.Controls.Add(this.browseButton);
            base.Controls.Add(this.assemPathTextBox);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.okButton);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AssemblySelectorForm";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Assembly File Selector";
            base.FormClosing += new FormClosingEventHandler(this.AssemblySelectorForm_FormClosing);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void AssemblySelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.m_found)
            {
                this.ShowWarning();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.mcl)|*.dll;*.exe;*.mcl|All files|*.*||";
                string str = this.m_assemName.Substring(0, this.m_assemName.IndexOf(','));
                openFileDialog.FileName = str + ".*";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    this.ShowWarning();
                }
                this.assemPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(this.assemPathTextBox.Text))
            {
                this.m_resultPath = this.assemPathTextBox.Text;
                this.m_found = true;
            }
            else
            {
                this.ShowWarning();
            }
            base.Close();
        }

        private void ShowWarning()
        {
            string text = new StringBuilder("The dependent assembly can't be loaded: \"").Append(this.m_assemName).AppendFormat("\".", new object[0]).ToString();
            MessageBox.Show(text, "Add-in Manager Internal", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
