using System;
using System.Text;
using System.Windows;
using eZcad.SubgradeQuantities;
using eZcad.SubgradeQuantities.Utility;
using eZstd.Data;
using eZstd.Miscellaneous;

namespace eZcad.SubgradeQuantities.DataExport
{
    public partial class CriterionEditor : PropertyEditor
    {

        private readonly StaticCriterion _instance;

        public CriterionEditor(StaticCriterion instance) : base(instance.FormTitle, instance)
        {
            InitializeComponent();
            Text = instance.FormTitle;
            //
            _instance = instance;
        }

        #region ---   数据文件的导入与导出
        private void btn_ExportToXml_Click(object sender, EventArgs e)
        {
            var o = propertyGrid1.SelectedObject;
            if (o != null)
            {
                var fpath = Utils.ChooseSaveFile("导出数据到到 xml 文件", _instance.FileExtension);
                if (fpath != null)
                {
                    var sb = new StringBuilder();
                    var succ = XmlSerializer.ExportToXmlFile(fpath, o, ref sb);
                    if (succ)
                    {
                        MessageBox.Show("数据导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
            }
        }

        private void btn_LoadFromXml_Click(object sender, EventArgs e)
        {
            var o = propertyGrid1.SelectedObject;
            if (o != null)
            {
                var fpath = Utils.ChooseOpenFile("从 xml 文件中导入数据", _instance.FileExtension, false);
                if (fpath != null)
                {
                    var sb = new StringBuilder();
                    var succ = false;

                    var newData = XmlSerializer.ImportFromXml(fpath[0], o.GetType(), out succ, ref sb);
                    if (succ)
                    {
                        propertyGrid1.SelectedObject = newData;
                    }
                }
            }
        }
#endregion
    }
}