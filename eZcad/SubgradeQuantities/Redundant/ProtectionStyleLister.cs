using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZstd.MarshalReflection;
using eZstd.Miscellaneous;
using Microsoft.Office.Interop.Excel;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Forms.Button;

namespace eZcad.SubgradeQuantities.Redundant
{
    public partial class ProtectionStyleLister : Form
    {
        private readonly List<SlopeLineBackup> _slopeLines;

        #region --- Fields

        /// <summary> 边坡对象的数据已经发生了修改 </summary>
        public bool ValueChanged { get; private set; }

        public bool ClearValue { get; private set; }

        private static Array ProtectionStyles;

        #endregion

        #region --- CurrentSlope

        private SlopeLineBackup CurrentSlope
        {
            get
            {
                var v = listBox_slopes.SelectedValue as SlopeLineBackup;
                return v;
            }
            set
            {
                CurrentIsFill = value.XData.FillExcav;
                CurrentStyle = value.XData.Style;
                //
                listBox_slopes.SelectedItem = value;
            }
        }

        private ProtectionStyle _currentStyle;

        public ProtectionStyle CurrentStyle
        {
            get { return _currentStyle; }
            private set
            {
                _currentStyle = value;
                SetCurrentStyleUI(_currentStyle);
                CurrentSlope.XData.Style = value;
            }
        }

        /// <summary> 填方边坡 </summary>
        private bool _currentIsFill;

        /// <summary> 填方边坡 </summary>
        public bool CurrentIsFill
        {
            get { return _currentIsFill; }
            set
            {
                CurrentSlope.XData.FillExcav = value;
                if (value)
                {
                    radioButton_Fill.Checked = true;
                }
                else
                {
                    radioButton_Excav.Checked = true;
                }
                _currentIsFill = value;
            }
        }

        #endregion

        #region --- 构造函数与窗口加载

        public ProtectionStyleLister(List<SlopeLineBackup> slopeLines)
        {
            InitializeComponent();
            KeyPreview = true;
            ValueChanged = false;
            //
            slopeLines.Sort(SlopeStationComparison);
            _slopeLines = slopeLines;
            SetSlopeLinesData(slopeLines);
            CurrentSlope = slopeLines[0];
            //
            ProtectionStyles = Enum.GetValues(typeof(ProtectionStyle));
            SetProtectionStyles();
            //
        }

        private static int SlopeStationComparison(SlopeLineBackup sl1, SlopeLineBackup sl2)
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
                return sl1.XData.Station.CompareTo(sl2.XData.Station);
            }
        }

        private void SetProtectionStyles()
        {
            var styles = ProtectionStyles;
            foreach (var st in styles)
            {
                var name = Enum.GetName(typeof(ProtectionStyle), st);
                var btn = new Button()
                {
                    Size = new Size(75, 50),
                    Text = name,
                    Tag = st,
                };
                toolTip1.SetToolTip(btn, ProtectionUtils.AvailableProtections[(ProtectionStyle)st]);
                btn.Click += StyleBtnOnClick;
                flp_ProtectionStyles.Controls.Add(btn);
            }
        }

        private void SetSlopeLinesData(List<SlopeLineBackup> slopeLines)
        {
            listBox_slopes.DataSource = slopeLines;
            listBox_slopes.DisplayMember = "DataInfo";
            //
            for (int i = 0; i < slopeLines.Count; i++)
            {
                listBox_slopes.SetSelected(i, true);
            }
        }

        private void ProtectionStyleLister_Shown(object sender, EventArgs e)
        {
            SetCurrentStyleUI(_currentStyle);
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

        /// <summary> 确定某一种防护方式 </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void StyleBtnOnClick(object sender, EventArgs eventArgs)
        {
            var btn = sender as Button;
            CurrentStyle = (ProtectionStyle)btn.Tag;

            foreach (SlopeLineBackup sl in listBox_slopes.SelectedItems)
            {
                sl.XData.Style = CurrentStyle;
                //var slpDt = new SlopeData(CurrentStyle, CurrentIsFill, sl.CenterAxisLine.IsOnLeft(sl.Pline), sl.Station);
                //sl.XData = slpDt;
            }
            //
            SetCurrentStyleUI(_currentStyle);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            CurrentIsFill = radioButton_Fill.Checked;

            foreach (SlopeLineBackup sl in listBox_slopes.SelectedItems)
            {
                sl.XData.FillExcav = CurrentIsFill;
            }
        }

        private void SetCurrentStyleUI(ProtectionStyle style)
        {
            foreach (Control btn in flp_ProtectionStyles.Controls)
            {
                if ((ProtectionStyle)btn.Tag == style)
                {
                    btn.BackColor = Color.Coral;
                    btn.Focus();
                }
                else
                {
                    btn.BackColor = DefaultBackColor;
                }
            }
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
                var slpRemove = new List<SlopeLineBackup>();
                var slpRemain = new List<SlopeLineBackup>();

                var selectedSlp = listBox_slopes.SelectedItems;
                foreach (SlopeLineBackup slp in listBox_slopes.DataSource as List<SlopeLineBackup>)
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

        private void btn_ImportBoundary_Click(object sender, EventArgs e)
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

        #endregion

        #region --- listBox 事件处理

        private void listBox_slopes_SelectedValueChanged(object sender, EventArgs e)
        {
            var cs = listBox_slopes.SelectedItem as SlopeLineBackup;
            CurrentSlope = cs;
        }

        private void listBox_slopes_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cs = listBox_slopes.SelectedItem as SlopeLineBackup;
            var sd = cs.XData;

            //var formAddDefinition = new SlopeDataEditor(sd);
            ////
            //var res = formAddDefinition.ShowDialog();
            //var newSlpDa = formAddDefinition.Instance;

        }

        #endregion

        #region --- 边坡分区段处理

        private void HandleSlopeSegments(List<SlopeSegment> segs)
        {
            foreach (var sl in _slopeLines)
            {
                // 将边坡线在每一个区间中进行一次判断或修改
                foreach (var seg in segs)
                {
                    var inSeg = seg.ModifySlopeLine(sl);
                }
            }
        }

        #endregion
    }
}