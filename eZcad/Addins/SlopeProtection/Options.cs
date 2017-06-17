using System;
using eZcad.Utility;

namespace eZcad.Addins.SlopeProtection
{
    public partial class Options : FormOk
    {
        private readonly DocumentModifier _docMdf;

        #region --- 构造函数

        public Options(DocumentModifier docMdf)
        {
            _docMdf = docMdf;
            InitializeComponent();
            //
            textBoxNum_RoadWidth.Text = ProtectionOptions.RoadWidth.ToString();
            textBox_Waterlevel.Text = ProtectionOptions.WaterLevel.ToString();
            checkBox_FillAboveWater.Checked = ProtectionOptions.ConsiderWaterLevel;
            textBox_FillAboveWater.Text =
                (ProtectionOptions.FillUpperEdge - ProtectionOptions.WaterLevel).ToString();
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
    }
}