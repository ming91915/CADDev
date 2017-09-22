using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.SQControls;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    /// <summary>
    /// 在 AutoCAD 界面中直接对防护方式进行设置或修改
    /// </summary>
    public partial class PF_PlaceProt : ModalPForm
    {
        private SelectionSet _impliedSelection;

        #region ---   窗口的构造、打开与关闭

        private static PF_PlaceProt _uniqueInstance;

        public static PF_PlaceProt GetUniqueInstance(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _uniqueInstance = _uniqueInstance ?? new PF_PlaceProt();
            _uniqueInstance._docMdf = docMdf;
            _uniqueInstance._impliedSelection = impliedSelection;
            //
            return _uniqueInstance;
        }

        /// <summary> 构造函数 </summary>
        private PF_PlaceProt() : base()
        {
            InitializeComponent();
            //
            SetProtectionLister(protectionLister1);
            //
            checkBox_ChooseRangeOnUI.Checked = true;
            checkBox_AllSlopeLevels.Checked = true;
        }


        #endregion

        #region ---   界面操作

        private void checkBox_AllSlopeLevels_CheckedChanged(object sender, EventArgs e)
        {
            textBox_SlopeLevels.Enabled = !checkBox_AllSlopeLevels.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBoxNum_RangeStart.Enabled = !checkBox_ChooseRangeOnUI.Checked;
            textBoxNum_RangeEnd.Enabled = !checkBox_ChooseRangeOnUI.Checked;
        }

        private void button_PlaceProtections_Click(object sender, EventArgs e)
        {
            GetParamAndRun();
        }

        #endregion

        #region ---   ProtectionLister 设置

        private void SetProtectionLister(ItemLister plister)
        {
            plister.ItemRaised += ProtectionLister1OnProtectionMethodRaised;
            plister.ItemDetermined += ProtectionLister1OnProtectionMethodDetermined;
            //
            var prots = Options_Collections.CommonFillProtections.ToList();
            prots.AddRange(Options_Collections.CommonCutProtections);
            plister.ImportItems(prots, prots.Select(r => r as object).ToArray());
        }

        private void ProtectionLister1OnProtectionMethodDetermined(Control label, string s)
        {
            textBox_ProtMethod.Text = s;
            GetParamAndRun();
        }

        private void ProtectionLister1OnProtectionMethodRaised(Control label, string s)
        {
            textBox_ProtMethod.Text = s;
        }

        #endregion

        protected override void OnCmdRun(bool closeWindow)
        {
            base.OnCmdRun(closeWindowWhenFinished: true);
        }

        private void GetParamAndRun()
        {
            // 防护方式
            var protMethod = textBox_ProtMethod.Text;
            if (!string.IsNullOrEmpty(textBox_Style.Text))
            {
                protMethod += ProtectionConstants.ProtectionMethodStyleSeperator + textBox_Style.Text;
            }
            // 如果最后的 protMethod 字符为空，则表示清除对应的防护


            // 子边坡等级
            int[] slopeLevels = null;
            if (checkBox_AllSlopeLevels.Checked)
            {
                slopeLevels = null;
            }
            else
            {
                try
                {
                    var levelStr = textBox_SlopeLevels.Text.Split(',');
                    slopeLevels = levelStr.Select(r => Convert.ToInt32(r)).ToArray();
                }
                catch (Exception)
                {
                    MessageBox.Show($"不同坡级之间请通过“,”进行分隔");
                    return;
                }
            }
            // 要进行设置的边坡
            var slopeLines = FilterSlopeLines();
            if (slopeLines == null || slopeLines.Count == 0) return;
            _docMdf.WriteNow("选择的边坡数量：", slopeLines.Count);
            //
            SetProtectionMethods(slopeLines, protMethod, slopeLevels);
            //
            // Utils.FocusOnMainUIWindow();
            _docMdf.acEditor.UpdateScreen();
        }

        private List<SlopeLine> FilterSlopeLines()
        {

            // 左右侧
            bool? leftOnly = null;
            if (radioButton_right.Checked)
            {
                leftOnly = false;
            }
            else if (radioButton_left.Checked)
            {
                leftOnly = true;
            }

            // 设置区间
            var slps = new List<SlopeLine>();
            if (checkBox_ChooseRangeOnUI.Checked)
            {
                // 通过界面选择边坡
                slps = ProtectionUtils.SelecteExistingSlopeLines(_docMdf, left: leftOnly, sort: true);
            }
            else
            {
                // 通过指定的桩号区间选择边坡
                var startStation = textBoxNum_RangeStart.ValueNumber;
                var endStation = textBoxNum_RangeEnd.ValueNumber;

                if (startStation >= endStation)
                {
                    MessageBox.Show(@"起始桩号必须小于结尾桩号");
                    return null;
                }
                var secs = ProtectionUtils.GetAllSections(_docMdf, sort: true);
                SectionInfo data;
                SlopeLine slp = null;
                foreach (var sec in secs)
                {
                    data = sec.XData;
                    if (data.Station >= startStation && data.Station <= endStation)
                    {
                        if (leftOnly.HasValue)
                        {
                            slp = sec.GetSlopeLine(leftOnly.Value);
                            if (slp != null)
                            {
                                slps.Add(slp);
                            }
                        }
                        else
                        {
                            slp = sec.GetSlopeLine(left: true);
                            if (slp != null)
                            {
                                slps.Add(slp);
                            }
                            slp = sec.GetSlopeLine(left: false);
                            if (slp != null)
                            {
                                slps.Add(slp);
                            }
                        }
                    }
                }
            }

            // 检查填挖方
            if (radioButton_fill.Checked)
            {
                slps = slps.Where(r => r.XData.FillCut).ToList();
            }
            else if (radioButton_cut.Checked)
            {
                slps = slps.Where(r => !r.XData.FillCut).ToList();
            }
            //
            return slps;
        }

        private void SetProtectionMethods(List<SlopeLine> slopeLines, string protMethod, int[] slopeLevels)
        {
            // 提取规则
            var fp = new ForceProtection(protMethod, slopeLevels);
            var es = EditStateIdentifier.GetCurrentEditState(_docMdf);
            es.CurrentBTR.UpgradeOpen();
            //var layer_Slope = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Slope);
            //var layer_Platform = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Platform);

            // 先修改内存中的防护
            foreach (var slp in slopeLines)
            {
                slp.ForceProtect(fp);
            }
            // 再刷新到 AutoCAD 文档与界面中
            var protLayers = ProtectionTags.MapProtectionLayers(_docMdf, slopeLines);
            foreach (var slp in slopeLines)
            {
                // 将数据刷新到界面与边坡线中
                slp.Pline.UpgradeOpen();
                slp.PrintProtectionMethod(es.CurrentBTR, protLayers);
                slp.FlushXData();
                slp.Pline.DowngradeOpen();
            }
            _docMdf.acEditor.UpdateScreen();
        }

    }
}