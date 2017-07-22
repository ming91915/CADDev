using System;
using System.Collections.Generic;
using System.Windows.Forms;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity
{
    public partial class SlopesegLister : Form
    {
        private readonly List<SlopeLine> _slopeLines;

        #region --- Fields

        /// <summary> 边坡对象的数据已经发生了修改 </summary>
        public bool ValueChanged { get; private set; }

        public bool ClearValue { get; private set; }

        private static Array ProtectionStyles;

        #endregion

        #region --- 构造函数 与 窗口加载

        /// <summary> 构造函数 </summary>
        public SlopesegLister(List<SlopeLine> slopeLines)
        {
            InitializeComponent();
            KeyPreview = true;
            ValueChanged = false;
            //
            slopeLines.Sort(SlopeMileageComparison);
            _slopeLines = slopeLines;
            SetSlopeLinesData(slopeLines);
            //
        }

        private static int SlopeMileageComparison(SlopeLine sl1, SlopeLine sl2)
        {
            if (sl1.XData.OnLeft ^ sl2.XData.OnLeft)
            {
                // 道路左侧的放在前面
                if (sl1.XData.OnLeft)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                // 都在道路的同一侧则比较桩号
                return sl1.Section.XData.Mileage.CompareTo(sl2.Section.XData.Mileage);
            }
        }

        /// <summary> 将所有的边坡对象显示在列表中 </summary>
        /// <param name="slopeLines"></param>
        private void SetSlopeLinesData(List<SlopeLine> slopeLines)
        {
            listBox_slopes.DataSource = slopeLines;
            listBox_slopes.DisplayMember = "DataInfo";
            ////
            //for (int i = 0; i < slopeLines.Count; i++)
            //{
            //    listBox_slopes.SetSelected(i, true);
            //}
        }

        private void ProtectionStyleLister_Shown(object sender, EventArgs e)
        {
        }

        private void ProtectionStyleLister_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ValueChanged = false;
                Close();
            }
        }

        #endregion

        private void SetCurrentSlopeUI(SlopeLine spl)
        {
            var xdata = spl.XData;
            var s = SlopeData.Combine(xdata.Slopes, xdata.Platforms);
            dgv.SetDataSource(s);
        }

        #region --- Button 事件处理

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            ValueChanged = true;
            Close();
        }

        /// <summary> 清除边坡数据 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Clear_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(@"将清除选择边坡线的边坡数据!", @"提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.OK)
            {
                ValueChanged = true;
                //
                var slpRemove = new List<SlopeLine>();
                var slpRemain = new List<SlopeLine>();

                var selectedSlp = listBox_slopes.SelectedItems;
                foreach (var slp in listBox_slopes.DataSource as List<SlopeLine>)
                {
                    if (selectedSlp.Contains(slp))
                    {
                        slpRemove.Add(slp);
                    }
                    else
                    {
                        slpRemain.Add(slp);
                    }
                }

                // 清除数据
                foreach (var sl in slpRemove)
                {
                    sl.XDataToBeCleared = true;
                }
                //
                if (slpRemain.Count == 0)
                {
                    Close();
                }
                else
                {
                    SetSlopeLinesData(slpRemain);
                }
            }
        }

        /// <summary>
        /// 选择所有边坡线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_selectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox_slopes.Items.Count; i++)
            {
                listBox_slopes.SetSelected(i, true);
            }
        }

        #endregion

        #region --- listBox 事件处理

        private void listBox_slopes_SelectedValueChanged(object sender, EventArgs e)
        {
            var cs = listBox_slopes.SelectedItem as SlopeLine;
            SetCurrentSlopeUI(cs);
        }

        private void listBox_slopes_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cs = listBox_slopes.SelectedItem as SlopeLine;
            var sd = cs.XData;

            var formAddDefinition = new PropertyEditor("边坡参数", sd);
            //
            var res = formAddDefinition.ShowDialog();
            var newSlpDa = formAddDefinition.Instance;
        }

        #endregion

        #region --- 边坡 分区段 进行自动防护

        /// <summary> 导入自动防护规则 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_btn_ImportProtectRull_Click(object sender, EventArgs e)
        {
            var segs = SlopeSegment.GetSlopeSegmentsFromExcel();
            if (segs == null || segs.Count == 0)
            {
                MessageBox.Show(@"无法提取有效的区间信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 根据指定的分区信息对列表中的边坡进行设置
            HandleSlopeSegments(segs);
        }


        private void HandleSlopeSegments(List<SlopeSegment> segs)
        {
            foreach (var sl in _slopeLines)
            {
                // 将边坡线在每一个区间中进行一次判断或修改
                foreach (var seg in segs)
                {
                    //  var inSeg = seg.ModifySlopeLine(sl);
                }
            }
        }

        /// <summary> 根据指定的匹配规进行边坡防护的自动匹配 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoProtect_Click(object sender, EventArgs e)
        {
            foreach (var obj in listBox_slopes.SelectedItems)
            {
                var spl = obj as SlopeLine;
                spl.AutoSetProtectionMethods();
            }
            dgv.Refresh();
        }

        #endregion

    }
}