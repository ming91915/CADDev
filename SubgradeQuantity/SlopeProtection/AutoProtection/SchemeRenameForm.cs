using System;
using System.Windows.Forms;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    public partial class SchemeRenameForm : FormOk
    {
        private readonly string _originalName;
        public string NewName { get; private set; }

        public SchemeRenameForm(string originalName)
        {
            InitializeComponent();
            label_OriginalName.Text = originalName;
            textBox_NewName.Text = originalName;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var newName = textBox_NewName.Text;
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show(@"方案名称不能为空");
                return;
            }
            NewName = textBox_NewName.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}