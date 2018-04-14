using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using eZstd.UserControls;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace eZcad.Addins.LayoutViewport
{
    public partial class LayoutLister : ListBox
    {
        /// <summary> 用户选择了一个布局之后触发，如果参数为null，则表示要新建一个布局 </summary>
        public event Action<Layout> LayoutSelected;
        private List<ListControlValue<Layout>> _layouts;

        public LayoutLister()
        {
            InitializeComponent();
        }

        public void LayoutsSetup(Database db)
        {
            _layouts = new List<ListControlValue<Layout>>();
            var id = db.LayoutDictionaryId;
            var layouts = id.GetObject(OpenMode.ForRead) as DBDictionary;
            foreach (DBDictionaryEntry dde in layouts)
            {
                var layoutName = dde.Key;
                Layout lo = dde.Value.GetObject(OpenMode.ForRead) as Layout;
                // 其中，模型空间也会列于此集合中，其对应的LayoutName为“Model”。
                var lv = new ListControlValue<Layout>(layoutName, lo);
                _layouts.Add(lv);
            }
            //
            var lv1 = new ListControlValue<Layout>("新建布局", null);
            _layouts.Add(lv1);
            FillListbox(_layouts);
        }

        private void FillListbox(List<ListControlValue<Layout>> layouts)
        {
            DataSource = layouts;
            DisplayMember = ListControlValue<Layout>.DisplayMember;
            ValueMember = ListControlValue<Layout>.ValueMember;
        }

        private void LayoutLister_Click(object sender, System.EventArgs e)
        {
            var item = SelectedItem as ListControlValue<Layout>;
            var la = item.Value as Layout;
            //
            if (LayoutSelected != null) LayoutSelected(la);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LayoutLister
            // 
            this.ItemHeight = 12;
            this.Size = new System.Drawing.Size(120, 88);
            this.Click += new System.EventHandler(this.LayoutLister_Click);
            this.ResumeLayout(false);

        }
    }
}