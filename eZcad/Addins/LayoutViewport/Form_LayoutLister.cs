using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Addins.LayoutViewport
{
    public partial class Form_LayoutLister : Form
    {
        private readonly DocumentModifier _docMdf;
        public bool CreateNewLayout;
        public Layout Layout { get; private set; }

        public Form_LayoutLister(DocumentModifier docMdf)
        {
            InitializeComponent();
            //
            _docMdf = docMdf;
            listBox1.LayoutSelected += ListBox1OnLayoutSelected;
            listBox1.LayoutsSetup(docMdf.acDataBase);
        }

        private void ListBox1OnLayoutSelected(Layout layout)
        {
            if (layout == null)
            {
                CreateNewLayout = true;
            }
            else
            {
                CreateNewLayout = false;
                this.Layout = layout;
            }
            //
            Close();
        }

        private void Form_LayoutLister_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CreateNewLayout = false;
                this.Layout = null;
                Close();
            }
        }
    }
}
