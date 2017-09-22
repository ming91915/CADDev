using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.Utility;
using eZstd.Data;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    public partial class SlopeSegLister : Form
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
        public SlopeSegLister(List<SlopeLine> slopeLines)
        {
            InitializeComponent();
            KeyPreview = true;
            ValueChanged = false;
            //
            slopeLines.Sort(SlopeStationComparison);
            _slopeLines = slopeLines;
            SetSlopeLinesData(slopeLines);
            //
            ImportAutoProtFromXml();
        }

        private void ImportAutoProtFromXml()
        {
            var sb = new StringBuilder();
            var succ = false;
            var fpath = @"C:\Users\Administrator\Desktop\1.autoP";
            var newData = XmlSerializer.ImportFromXml(fpath, typeof(AutoProtectionCriterions), out succ, ref sb);
            if (succ)
            {
                _autoSpCriterion = newData as AutoProtectionCriterions;
            }
        }

        private static int SlopeStationComparison(SlopeLine sl1, SlopeLine sl2)
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
                return sl1.Section.XData.Station.CompareTo(sl2.Section.XData.Station);
            }
        }

        /// <summary> 将所有的边坡对象显示在列表中 </summary>
        /// <param name="slopeLines"></param>
        private void SetSlopeLinesData(List<SlopeLine> slopeLines)
        {
            listBox_slopes.DataSource = slopeLines;
            listBox_slopes.DisplayMember = "DataInfo";
            // 在赋值到另一个控件中作为数据源之前，必须要先复制一份副本
            var slopes = new SlopeLine[slopeLines.Count];
            slopeLines.CopyTo(slopes);
            cbb_currentSlope.DataSource = slopes;
            cbb_currentSlope.DisplayMember = "DataInfo";
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
            var s = SlopeData.Combine(xdata.Slopes, xdata.Platforms, true);
            SetDGVDataSource(spl, s);
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
            if (listBox_slopes.SelectedItems.Count == 1)
            {
                var cs = listBox_slopes.SelectedItem as SlopeLine;
                cbb_currentSlope.SelectedItem = cs;
                //
                // Utils.ShowExtentsInView(cs._docMdf.acEditor, cs.Section.GetExtends());
            }
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

        #region --- combobox_currentSlope 事件处理

        private void cbb_currentSlope_SelectedValueChanged(object sender, EventArgs e)
        {
            var cs = cbb_currentSlope.SelectedItem as SlopeLine;
            SetCurrentSlopeUI(cs);
            //
        }

        #endregion

        #region --- 操作 边坡与平台 信息的 Datagridview 控件

        #region ---   eZDataGridView 配置

        private bool _uIConstructed;
        private void SetDGVDataSource(SlopeLine spl, IList<ISlopeSeg> slopes)
        {
            dgv.Tag = spl;
            var Slopes = new BindingList<ISlopeSeg>(slopes) { AllowNew = false };
            //
            if (!_uIConstructed)
            {
                ConstructeZDataGridView(dgv);
                _uIConstructed = true;
            }
            dgv.DataSource = Slopes;
        }

        private void ConstructeZDataGridView(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;

            var dicimalStyle3 = new DataGridViewCellStyle();
            dicimalStyle3.Format = "0.###";
            var dicimalStyle2 = new DataGridViewCellStyle();
            dicimalStyle2.Format = "0.##";

            // 创建数据列并绑定到数据源 ----------------------------------------------
            // -------------------------
            var column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Type";
            column.Name = "类型";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgv.Columns.Add(column);
            // column.SortMode= DataGridViewColumnSortMode.Automatic;

            // -------------------------
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Index";
            column.ReadOnly = true;
            column.Name = "序号";
            column.Width = 70;
            dgv.Columns.Add(column);
            // -------------------------

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "SlopeRatio";
            column.Name = "坡率";
            column.Width = 70;
            column.DefaultCellStyle = dicimalStyle2;
            dgv.Columns.Add(column);
            // -------------------------

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "SegHeight";
            column.Name = "坡高(m)";
            column.Width = 70;
            column.DefaultCellStyle = dicimalStyle2;
            dgv.Columns.Add(column);

            // -------------------------
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Length";
            column.Name = "边坡长度(m)";
            column.Width = 100;
            column.DefaultCellStyle = dicimalStyle3;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ProtectionLength";
            column.Name = "防护长度(m)";
            column.ReadOnly = true;
            column.ToolTipText = @"如果子边坡并不需要全长设置防护，则可将其设置为需要防护的长度值";
            column.Width = 100;
            column.DefaultCellStyle = dicimalStyle3;
            column.ValueType = typeof(double); // 限定此列的数据类型必须为数值
            dgv.Columns.Add(column);

            // -------------------------
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ProtectionMethod";
            column.Name = "ProtectionMethod";
            column.HeaderText = @"防护";

            column.MinimumWidth = 100;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            column.ValueType = typeof(int); // 限定此列的数据类型必须为整数值
            dgv.Columns.Add(column);

            // -------------------------

            // 事件绑定 -------------------------------------------------------------
            dgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
                                               //dgv.CellContentClick += EZdgvOnCellContentClick;  // 响应表格中的按钮按下事件
                                               //dgv.CurrentCellDirtyStateChanged += EZdgvOnCurrentCellDirtyStateChanged; // 在表格中Checkbox的值发生改变时立即作出响应
            dgv.CellValueChanged += DgvOnCellValueChanged;

        }

        #endregion

        #region ---   与 eZDataGridView 相关的事件处理

        private void EZdgvOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // 表头行的行号为 -1
            //{
            //    string columnName = this.Columns[e.ColumnIndex].Name;

            //    if (columnName == "Details") // 显示选中的行的数据信息
            //    {
            //        DataGridViewRow r = _eZdgv.Rows[e.RowIndex];
            //        Person p = r.DataBoundItem as Person;
            //        if (p != null)
            //        {
            //            MessageBox.Show(p.ToString());
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 在表格中Checkbox的值发生改变时立即作出响应.
        /// 如果你想要在用户点击复选框单元格时立即响应，你可以处理CellContentClick 事件，但是此事件会在单元格的值更新之前触发。
        /// </summary>
        private void EZdgvOnCurrentCellDirtyStateChanged(object sender, EventArgs eventArgs)
        {
            // IsCurrentCellDirty属性：Gets a value indicating whether the current cell has uncommitted changes.
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }

        private void EZdgvOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show(@"输入的数据不能转换为指定的数据类型！", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                e.ThrowException = false;
            }
        }

        /// <summary> 单元格中的内容发生变化 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvOnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name == "ProtectionMethod")
            {
                var ss = dgv.Rows[e.RowIndex].DataBoundItem as ISlopeSeg;
                if (ss != null)
                {
                    ChangeSelectedSlopeProtection(ss);
                }
            }
        }

        #endregion

        #endregion

        private void ChangeSelectedSlopeProtection(ISlopeSeg baseSeg)
        {
            var changedSlp = dgv.Tag as SlopeLine;
            var changedXdata = changedSlp.XData;
            var seperateFillCut = checkBox_SeperateFillCut.Checked; // 在统一修改时区分填方与挖方
            //
            foreach (SlopeLine slp in listBox_slopes.SelectedItems)
            {
                var xdata = slp.XData;
                if (!seperateFillCut || changedXdata.FillCut == xdata.FillCut)
                {
                    var slopes = SlopeData.Combine(slp.XData.Slopes, slp.XData.Platforms, false);
                    foreach (var s in slopes)
                    {
                        if (s.Type == baseSeg.Type && s.Index == baseSeg.Index)
                        {
                            s.ProtectionMethod = baseSeg.ProtectionMethod;
                        }
                    }
                }
            }
        }

        #region --- 边坡 分区段 进行自动防护

        /// <summary> 对边坡进行自动防护的规则 </summary>
        private AutoProtectionCriterions _autoSpCriterion;

        /// <summary> 导入自动防护规则 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_btn_ImportProtectRull_Click(object sender, EventArgs e)
        {
            var f = AutoProtectionForm.GetUniqueInstance();
            var res = f.ShowDialog();
            if (res == DialogResult.OK)
            {
                _autoSpCriterion = f.ActiveSpCriterion;
            }
        }


        /// <summary> 根据指定的匹配规进行边坡防护的自动匹配 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoProtect_Click(object sender, EventArgs e)
        {
            if (_autoSpCriterion != null)
            {
                foreach (var obj in listBox_slopes.SelectedItems)
                {
                    var spl = obj as SlopeLine;
                    spl.AutoSetProtectionMethods(_autoSpCriterion);
                }
                dgv.Refresh();
                //
                MessageBox.Show($"成功对{listBox_slopes.SelectedItems.Count}个边坡进行了自动防护", @"恭喜", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var res = MessageBox.Show(@"请先选择自动防护规则", @"提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (res == DialogResult.OK)
                {
                    btn_btn_ImportProtectRull_Click(null, null);
                }
            }
        }

        #endregion

    }
}
