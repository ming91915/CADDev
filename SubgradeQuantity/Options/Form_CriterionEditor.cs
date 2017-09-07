using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using eZcad.Utility;
using eZstd.Data;
using Utils = eZstd.Miscellaneous.Utils;

namespace eZcad.SubgradeQuantity.Options
{
    /// <summary> 路基工程量计算的总选项设置 </summary>
    public partial class Form_CriterionEditor : FormOk
    {
        private readonly DocumentModifier _docMdf;
        private const string FormText = "各类工程量的判断与计量标准";
        private StaticCriterion _activeCriterion;
        private StaticCriterion ActiveCriterion
        {
            get { return _activeCriterion; }
            set
            {
                if (value != null)
                {
                    Text = $"{FormText} - {value.FormTitle}"; // FormText+""
                    propertyGrid1.SelectedObject = value;
                    _activeCriterion = value;
                }
            }
        }

        private Dictionary<Button, int> _criterionButtons;

        #region --- 构造函数

        public Form_CriterionEditor(DocumentModifier docMdf)
        {
            _docMdf = docMdf;
            InitializeComponent();
            //
            AddStaticCriterions(flp_Criterions, StaticCriterions.UniqueInstance.Criterions);
            if (_criterionButtons.Count > 0)
            {
                CriterionBtnOnClick(_criterionButtons.First().Key, null);
            }
        }

        #endregion

        private void AddStaticCriterions(FlowLayoutPanel panel, StaticCriterion[] criterions)
        {
            _criterionButtons = new Dictionary<Button, int>();
            for (int i = 0; i < criterions.Length; i++)
            {
                var cr = criterions[i];
                string btnName = cr.FormTitle;
                var btn = new Button()
                {
                    Text = btnName,
                    Tag = cr,
                };
                btn.Click += CriterionBtnOnClick;
                //
                panel.Controls.Add(btn);
                _criterionButtons.Add(btn, i);
            }

        }

        private void CriterionBtnOnClick(object sender, EventArgs eventArgs)
        {
            var instance = (sender as Button).Tag as StaticCriterion;
            ActiveCriterion = instance;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //
            //
            DialogResult = DialogResult.OK;
            Close();
        }

        #region ---   数据文件的导入与导出

        private void btn_Import_Click(object sender, EventArgs e)
        {
            var fpath = Utils.ChooseOpenFile("从 xml 文件中导入数据", StaticCriterions.FileExtensionFilter, false);
            if (fpath != null)
            {
                var sb = new StringBuilder();
                var succ = false;
                var newData = XmlSerializer.ImportFromXml(fpath[0], typeof(StaticCriterions), out succ, ref sb) as StaticCriterions;
                if (succ)
                {
                    Button activeButton = null;
                    foreach (var cr in _criterionButtons)
                    {
                        cr.Key.Tag = newData.Criterions[cr.Value];
                        if (ActiveCriterion.GetType() == newData.Criterions[cr.Value].GetType())
                        {
                            activeButton = cr.Key;
                        }
                    }
                    if (activeButton != null)
                    {
                        CriterionBtnOnClick(activeButton, null);
                    }
                }
            }
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            var scs = StaticCriterions.UniqueInstance;
            if (scs != null)
            {
                var fpath = Utils.ChooseSaveFile("导出数据到到 xml 文件", StaticCriterions.FileExtensionFilter);
                if (fpath != null)
                {
                    var errMsg = new StringBuilder();
                    var succ = XmlSerializer.ExportToXmlFile(fpath, scs, ref errMsg);
                    if (succ)
                    {
                        System.Windows.Forms.MessageBox.Show("数据导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("数据导出出错！" +
                           "\r\n" + errMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion
    }
}