using System;
using eZcad.SubgradeQuantities.DataExport;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantities
{
    /// <summary> 路基工程量计算的总选项设置 </summary>
    public partial class SubgradeOptions : FormOk
    {
        private readonly DocumentModifier _docMdf;

        #region --- 构造函数

        public SubgradeOptions(DocumentModifier docMdf)
        {
            _docMdf = docMdf;
            InitializeComponent();
            //
            textBoxNum_RoadWidth.Text = ProtectionOptions.RoadWidth.ToString();
            textBox_Waterlevel.Text = ProtectionOptions.WaterLevel.ToString();
            checkBox_FillAboveWater.Checked = ProtectionOptions.ConsiderWaterLevel;
            textBox_FillAboveWater.Text =
                (ProtectionOptions.FillUpperEdge - ProtectionOptions.WaterLevel).ToString("0.###");
        }

        #endregion

        private void button_MatchRules_Click(object sender, EventArgs e)
        {
            var mf = new MatchOptionsForm(_docMdf);
            mf.ShowDialog(null);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //
            ProtectionOptions.RoadWidth = textBoxNum_RoadWidth.ValueNumber;
            ProtectionOptions.WaterLevel = textBox_Waterlevel.ValueNumber;
            ProtectionOptions.ConsiderWaterLevel = checkBox_FillAboveWater.Checked;
            ProtectionOptions.FillUpperEdge = ProtectionOptions.WaterLevel + textBox_FillAboveWater.ValueNumber;

            //
            Close();
        }

        private void checkBox_FillAboveWater_CheckedChanged(object sender, EventArgs e)
        {
            panel_FillWater.Enabled = checkBox_FillAboveWater.Checked;
        }

        #region ---   各类工程量的判断与计量标准

        private void btn_criterion_ThinFillShallowCut_Click(object sender, EventArgs e)
        {
            var u = Criterion_ThinFillShallowCut.UniqueInstance;
            var fsc = new CriterionEditor(u);
            fsc.ShowDialog();
        }

        #endregion

    }
}