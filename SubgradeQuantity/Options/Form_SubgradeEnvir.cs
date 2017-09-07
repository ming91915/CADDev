using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.UserControls;

namespace eZcad.SubgradeQuantity.Options
{
    public partial class Form_SubgradeEnvir : FormOk
    {
        #region --- Fields

        DocumentModifier _docMdf;

        private BindingList<SoilRockRange> _soilRockRanges = new BindingList<SoilRockRange>(Options_Collections.SoilRockRanges);
        private BindingList<Structure> _structures = new BindingList<Structure>(Options_Collections.Structures);

        #endregion

        #region --- 构造函数与窗口的显示、关闭

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        public Form_SubgradeEnvir(DocumentModifier docMdf)
        {
            InitializeComponent();
            _docMdf = docMdf;
            //
            _soilRockRanges.AddingNew += SoilRockRangesOnAddingNew;
            DgvSetup_SoilRockRange(dgv_SoilRockRange, _soilRockRanges);
            _structures.AddingNew += StructuresOnAddingNew;
            DgvSetup_Structure(dgv_Structures, _structures);
        }

        #endregion

        #region ---   eZDataGridView_SoilRockRanges

        private void DgvSetup_SoilRockRange(eZDataGridView eZdgv, IList<SoilRockRange> datasource)
        {
            eZdgv.AutoGenerateColumns = false;
            eZdgv.EditMode = DataGridViewEditMode.EditOnEnter;
            //
            eZdgv.DataSource = datasource;
            //
            eZdgv.ManipulateRows = true;
            eZdgv.ShowRowNumber = true;
            // -------------------------
            var column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "StartStation";
            column.Name = "起始桩号";
            eZdgv.Columns.Add(column);
            //
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "EndStation";
            column.Name = "末尾桩号";
            eZdgv.Columns.Add(column);

            // -------------------------
            var combo = new DataGridViewComboBoxColumn();
            combo.DataSource = Enum.GetValues(typeof(SubgradeType));
            combo.DataPropertyName = "Type";
            combo.Name = "类型";
            combo.Width = 100;
            eZdgv.Columns.Add(combo);
            // 如果要设置对应单元格的值为某枚举项：combo.Item(combo.Index,行号).Value = Gender.Male;

            // -------------------------
            combo = new DataGridViewComboBoxColumn();
            combo.DataSource = Enum.GetValues(typeof(SoilRockRange.Distribution));
            combo.DataPropertyName = "SideDistribution";
            combo.Name = "分布";
            combo.Width = 100;
            eZdgv.Columns.Add(combo);
            // 如果要设置对应单元格的值为某枚举项：combo.Item(combo.Index,行号).Value = Gender.Male;

            // 事件绑定 -------------------------------------------------------------
            eZdgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
            // eZdgv.CellContentClick += EZdgvOnCellContentClick; // 响应表格中的按钮按下事件
        }

        private void SoilRockRangesOnAddingNew(object sender, AddingNewEventArgs e)
        {
            var ss = _soilRockRanges.LastOrDefault();
            if (ss == null)
            {
                ss = new SoilRockRange(0, 0, SoilRockRange.Distribution.左右两侧, SubgradeType.岩质);
            }
            else
            {
                ss = (SoilRockRange)ss.Clone();
                ss.StartStation = ss.EndStation;
            }
            e.NewObject = ss;
        }

        #endregion

        #region ---   eZDataGridView_Structures

        private void DgvSetup_Structure(eZDataGridView eZdgv, IList<Structure> datasource)
        {
            eZdgv.AutoGenerateColumns = false;
            eZdgv.EditMode = DataGridViewEditMode.EditOnEnter;
            //
            eZdgv.DataSource = datasource;
            //
            eZdgv.ManipulateRows = true;
            eZdgv.ShowRowNumber = true;
            // -------------------------
            var column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "StartStation";
            column.Name = "起始桩号";
            eZdgv.Columns.Add(column);
            //
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "EndStation";
            column.Name = "末尾桩号";
            eZdgv.Columns.Add(column);

            // -------------------------
            var combo = new DataGridViewComboBoxColumn();
            combo.DataSource = Enum.GetValues(typeof(StructureType));
            combo.DataPropertyName = "Type";
            combo.Name = "类型";
            combo.Width = 100;
            eZdgv.Columns.Add(combo);
            // 如果要设置对应单元格的值为某枚举项：combo.Item(combo.Index,行号).Value = Gender.Male;

            // 事件绑定 -------------------------------------------------------------
            eZdgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
            // eZdgv.CellContentClick += EZdgvOnCellContentClick; // 响应表格中的按钮按下事件
        }

        private void StructuresOnAddingNew(object sender, AddingNewEventArgs e)
        {
            var ss = _structures.LastOrDefault();
            if (ss == null)
            {
                ss = new Structure(StructureType.桥梁, 0, 0);
            }
            else
            {
                ss = (Structure)ss.Clone();
                ss.StartStation = ss.EndStation;
            }
            e.NewObject = ss;
        }

        #endregion

        private void EZdgvOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            return;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // 表头行的行号为 -1
            {
                var dgv = sender as DataGridView;

                var c = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;
            }
        }

        private void EZdgvOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
        }

        #region ---   确定并关闭

        private void btnOk_Click(object sender, EventArgs e)
        {
            string errMsg;
            var succ = CheckData(out errMsg);
            if (!succ)
            {
                MessageBox.Show(errMsg, @"数据不符合规范", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //
            var allSlopes = ProtectionUtils.GetAllExistingSlopeLines(_docMdf, sort: true);
            SetSlopeSoilRock(allSlopes);
            //
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool CheckData(out string errMsg)
        {
            errMsg = "";
            int index = 0;
            foreach (var rr in Options_Collections.SoilRockRanges)
            {
                index += 1;
                if (rr.StartStation >= rr.EndStation)
                {
                    errMsg = $"第{index}行的岩土分区的起始桩号值必须比末尾桩号值小";
                    return false;
                }
            }
            index = 0;
            foreach (var s in Options_Collections.Structures)
            {
                index += 1;
                if (s.StartStation >= s.EndStation)
                {
                    errMsg = $"第{index}行的结构物的起点桩号值必须比终点桩号值小";
                    return false;

                }
            }
            return true;
        }

        /// <summary> 为道路中的所有边坡设置土质或者岩质属性 </summary>
        private void SetSlopeSoilRock(List<SlopeLine> allSlopes)
        {
            SoilRockRange.SetSlopeSoilRock(Options_Collections.SoilRockRanges, allSlopes.Select(r => r.XData).ToArray());

            // 保存数据
            foreach (var s in allSlopes)
            {
                s.Pline.UpgradeOpen();
                s.FlushXData();
                s.Pline.DowngradeOpen();
            }
        }

        #endregion
    }
}