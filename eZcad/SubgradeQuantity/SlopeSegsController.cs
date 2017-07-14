using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity
{
    public class SlopeSegsController : DataGridView
    {
        public SlopeSegsController()
        {

        }

        #region ---   eZDataGridView 配置

        public BindingList<ISlopeSeg> Slopes { get; private set; }

        private bool _uIConstructed;
        public void SetDataSource(IList<ISlopeSeg> slopes)
        {
            Slopes = new BindingList<ISlopeSeg>(slopes) { AllowNew = false };
            //
            if (!_uIConstructed)
            {
                ConstructeZDataGridView(this);
                _uIConstructed = true;
            }
            this.DataSource = Slopes;
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
            column.DataPropertyName = "Length";
            column.Name = "边坡长度(m)";
            column.Width = 100;
            column.DefaultCellStyle = dicimalStyle3;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ProtectionLength";
            column.Name = "防护长度(m)";
            column.Width = 100;
            column.DefaultCellStyle = dicimalStyle3;
            column.ValueType = typeof(double); // 限定此列的数据类型必须为数值
            dgv.Columns.Add(column);

            // -------------------------
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ProtectionMethod";
            column.Name = "防护";
            column.MinimumWidth = 100;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            column.ValueType = typeof(int); // 限定此列的数据类型必须为整数值
            dgv.Columns.Add(column);

            // -------------------------

            // 事件绑定 -------------------------------------------------------------
            dgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
            //dgv.CellContentClick += EZdgvOnCellContentClick;  // 响应表格中的按钮按下事件
            //dgv.CurrentCellDirtyStateChanged += EZdgvOnCurrentCellDirtyStateChanged; // 在表格中Checkbox的值发生改变时立即作出响应

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
            if (this.IsCurrentCellDirty)
            {
                this.CommitEdit(DataGridViewDataErrorContexts.Commit);
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

        #endregion

    }
}
