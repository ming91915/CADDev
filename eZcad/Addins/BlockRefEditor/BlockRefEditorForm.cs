using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using eZstd.Enumerable;
using eZstd.UserControls;
using Autodesk.AutoCAD.DatabaseServices;
using DataColumn = System.Data.DataColumn;

namespace eZcad.Addins
{
    public partial class BlockRefEditorForm : Form
    {
        private List<AttriBlock> _allAttriBlocks;
        private readonly List<string> _allAttDefs;
        private readonly Color _btnColor;
        private SortColumnCollection _sortColumns = new SortColumnCollection();

        /// <summary> 构造函数 </summary>
        /// <param name="attriBlocks">所有要进行操作的块参照</param>
        /// <param name="allAttDefs">要进行读写的块属性定义，以及一些其他字段</param>
        public BlockRefEditorForm(List<AttriBlock> attriBlocks, List<string> allAttDefs)
        {
            InitializeComponent();
            _btnColor = btn_SortMultiLevel.BackColor;
            //
            // _allAttriBlocks = new BindingCollection<AttriBlock>(attriBlocks);
            _allAttriBlocks = attriBlocks;
            _allAttDefs = allAttDefs;

            //
            ConstructeZDataGridView(_eZdgv, allAttDefs, _allAttriBlocks);
        }

        #region ---   eZDataGridView 配置

        private void ConstructeZDataGridView(eZDataGridView eZdgv,
           List<string> attDefs, List<AttriBlock> attBlocks)
        {
            // 基本配置

            //
            _eZdgv.KeyDelete = true;
            _eZdgv.ManipulateRows = false;
            _eZdgv.ShowRowNumber = true;
            _eZdgv.SupportPaste = true;
            _eZdgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            //
            // _eZdgv.EditMode = DataGridViewEditMode.EditOnEnter;
            _eZdgv.AllowUserToAddRows = false;
            _eZdgv.AutoGenerateColumns = true;

            _dataTable = ConstructDataTable(attBlocks, attDefs);
            eZdgv.DataSource = _dataTable;

            // 将坐标显示为三位小数
            var dicimalStyle3 = new DataGridViewCellStyle();
            dicimalStyle3.Format = "0.###";
            eZdgv.Columns["X"].DefaultCellStyle = dicimalStyle3;
            eZdgv.Columns["Y"].DefaultCellStyle = dicimalStyle3;

            //// 设置自动排序
            foreach (DataGridViewColumn col in eZdgv.Columns)
            {
                //设置自动排序
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.MinimumWidth = 70;
            }

            // 事件绑定 -------------------------------------------------------------
            _eZdgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
            _eZdgv.ColumnHeaderMouseClick += EZdgvOnColumnHeaderMouseClick;
            _eZdgv.SelectionChanged += EZdgvOnSelectionChanged;
        }


        #region ---   与 eZDataGridView 相关的事件处理

        private void EZdgvOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // 表头行的行号为 -1
            {
                string columnName = _eZdgv.Columns[e.ColumnIndex].Name;
            }
        }

        /// <summary>
        /// 在表格中Checkbox的值发生改变时立即作出响应.
        /// 如果你想要在用户点击复选框单元格时立即响应，你可以处理CellContentClick 事件，但是此事件会在单元格的值更新之前触发。
        /// </summary>
        private void EZdgvOnCurrentCellDirtyStateChanged(object sender, EventArgs eventArgs)
        {
            // IsCurrentCellDirty属性：Gets a value indicating whether the current cell has uncommitted changes.
            if (_eZdgv.IsCurrentCellDirty)
            {
                _eZdgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

        }

        private void EZdgvOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
            {
                MessageBox.Show("输入的数据不能转换为指定的数据类型！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                e.ThrowException = false;
            }
        }

        private void EZdgvOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            label_SelectionCount.Text = $"选择数量：{_eZdgv.SelectedCells.Count}";
        }

        private void EZdgvOnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex >= 0 && _sortColuMultiLevel)
            {
                var col = _eZdgv.Columns[e.ColumnIndex];
                var sc = _sortColumns.AddSortColumn(col.Name);
                //
                foreach (DataGridViewColumn c in _eZdgv.Columns)
                {
                    var cc = _sortColumns.SortColumns.FirstOrDefault(r => r.Field == c.Name);
                    if (cc != null)
                    {
                        var s = cc.Ascend ? "▲" : "▼";
                        c.HeaderText = $"{cc.Field} ({cc.Index}) {s}";
                    }
                }

                // 执行多级排序
                var sortCrit = _sortColumns.GetSortCriterior();
                if (sortCrit != null)
                {
                    var view = _dataTable.DefaultView;
                    view.Sort = sortCrit;
                    // view.Sort = "State ASC, ZipCode DESC";
                    // Gets or sets the sort column or columns, and sort order for the DataView.
                    // A string that contains the column name followed by "ASC" (ascending) or "DESC" (descending). Columns are sorted ascending by default. Multiple columns can be separated by commas. 
                }
            }
        }

        #endregion

        #endregion

        #region ---   DataTable 与 排序

        System.Data.DataTable _dataTable;

        /// <summary> 将集合数据构造为 DataTable 表格 </summary>
        /// <param name="attriBlocks"></param>
        /// <param name="attDefs"></param>
        /// <returns></returns>
        public System.Data.DataTable ConstructDataTable(List<AttriBlock> attriBlocks, List<string> attDefs)
        {
            var dt = new System.Data.DataTable("BlockReferences");
            //
            var column = new DataColumn("IdSort");
            column.DataType = typeof(int);
            column.ReadOnly = false;
            column.Unique = false;
            column.AutoIncrement = true;
            column.AutoIncrementSeed = 1;
            column.AutoIncrementStep = 1;
            dt.Columns.Add(column);
            //
            column = new DataColumn("Space");
            column.DataType = typeof(string);
            column.ReadOnly = true;
            column.Unique = false;
            dt.Columns.Add(column);
            //
            column = new DataColumn("Handle");
            column.DataType = typeof(Handle);
            column.ReadOnly = true;
            column.Unique = true;
            dt.Columns.Add(column);
            //
            column = new DataColumn("X");
            column.DataType = typeof(double);
            column.ReadOnly = true;
            column.Unique = false;
            dt.Columns.Add(column);
            //
            column = new DataColumn("Y");
            column.DataType = typeof(double);
            column.ReadOnly = true;
            column.Unique = false;
            dt.Columns.Add(column);
            //
            // 添加所有的属性定义
            foreach (var attDef in attDefs)
            {
                column = new DataColumn(attDef);
                column.DataType = typeof(string);
                column.ReadOnly = false;
                column.Unique = false;
                column.DefaultValue = null;
                dt.Columns.Add(column);
            }
            // 向表格中添加值
            DataRow row;
            foreach (var ab in attriBlocks)
            {
                row = dt.NewRow();
                row["Space"] = ab.Space;
                row["Handle"] = ab.Handle;
                row["X"] = ab.X;
                row["Y"] = ab.Y;
                //  添加属性值
                foreach (var ar in ab.AttRefs)
                {
                    row[ar.Tag] = ar.TextString;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary> True 表示处理多级排序模式 </summary>
        private bool _sortColuMultiLevel;

        private void btn_Sort_Click(object sender, EventArgs e)
        {
            _sortColuMultiLevel = !_sortColuMultiLevel;
            if (_sortColuMultiLevel)
            {
                // 启动 多级排序模式
                btn_SortMultiLevel.BackColor = Color.Yellow;
                foreach (DataGridViewColumn c in _eZdgv.Columns)
                {
                    c.SortMode = DataGridViewColumnSortMode.Programmatic;
                }
            }
            else
            {
                // 禁用 多级排序模式，开启单级自动排序
                btn_SortMultiLevel.BackColor = _btnColor;
                foreach (DataGridViewColumn c in _eZdgv.Columns)
                {
                    c.HeaderText = c.Name;
                    c.SortMode = DataGridViewColumnSortMode.Automatic;
                }
                _sortColumns.ClearSortColumns();
            }
        }

        #endregion

        #region ---   确定

        /// <summary> 将表格控件中修改后的值提取出来 </summary>
        private Dictionary<AttriBlock, Dictionary<string, string>> GetAttValuesFromEzdgv(eZDataGridView eZdgv)
        {
            var va = new Dictionary<AttriBlock, Dictionary<string, string>>();
            //
            var dt = eZdgv.DataSource as System.Data.DataTable;
            Dictionary<string, string> attDef_attValue;
            foreach (DataRow row in dt.Rows)
            {
                var hand = (Handle)row["Handle"];
                var attriBlock = _allAttriBlocks.FirstOrDefault(r => r.Handle == hand);
                attDef_attValue = new Dictionary<string, string>();
                if (attriBlock != null)
                {
                    foreach (var attDef in _allAttDefs)
                    {
                        var c = row[attDef];
                        attDef_attValue.Add(attDef, c.ToString());
                    }
                }
                //
                va.Add(attriBlock, attDef_attValue);
            }
            //
            return va;
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            // 将表格中的数据刷新到块属性定义中
            var va = GetAttValuesFromEzdgv(_eZdgv);
            foreach (var v in va)
            {
                var attBlock = v.Key;
                var allDef_value = v.Value;
                attBlock.RefreshAttValue(allDef_value);
            }
            //
            DialogResult = DialogResult.OK;

            Close();
        }

        #endregion

    }
}