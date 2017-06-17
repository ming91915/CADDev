using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoCAD;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.Utility;
using eZstd.Miscellaneous;

namespace eZcad.Addins.SlopeProtection
{
    public partial class MatchOptionsForm : FormOk
    {
        #region --- Fields

        DocumentModifier _docMdf;

        #endregion

        #region --- 构造函数与窗口的显示、关闭

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        public MatchOptionsForm(DocumentModifier docMdf)
        {
            InitializeComponent();
            _docMdf = docMdf;
            //
            textBox_MileageFieldDef.Text = ProtectionOptions.MileageFieldDef;
            
            //
            DatagridviewSetup(dgv_LayerOptions, LayerOptions, GetLayers(docMdf));
            DatagridviewSetup(dgv_BlockOptions, BlockOptions, GetBlocks(docMdf));
        }

        private void DatagridviewSetup(DataGridView eZdgv, IList<OptionDatasource> datasource, object comboboxDatasource)
        {

            eZdgv.AutoGenerateColumns = false;
            eZdgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            eZdgv.EditMode = DataGridViewEditMode.EditOnEnter;
            //
            eZdgv.DataSource = datasource;
            //

            // -------------------------
            var column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "OptionName";
            column.Name = "类型";
            column.MinimumWidth = 120;
            eZdgv.Columns.Add(column);

            // -------------------------
            var combo = new DataGridViewComboBoxColumn();
            combo.DataSource = comboboxDatasource;
            combo.DataPropertyName = "OptionValue";
            combo.Name = "值";
            combo.MinimumWidth = 120;
            eZdgv.Columns.Add(combo);
            // 如果要设置对应单元格的值为某枚举项：combo.Item(combo.Index,行号).Value = Gender.Male;
            for (int i = 0; i < datasource.Count; i++)
            {
                var c = eZdgv.Rows[i].Cells[combo.Index] as DataGridViewComboBoxCell;
                c.Value = datasource[i].OptionValue;
            }

            // 事件绑定 -------------------------------------------------------------
            eZdgv.DataError += EZdgvOnDataError; // 响应表格中的数据类型不匹配等出错的情况
            eZdgv.CellContentClick += EZdgvOnCellContentClick;  // 响应表格中的按钮按下事件
        }

        private List<string> GetLayers(DocumentModifier docMdf)
        {
            var layerNames = new List<string>();
            var lt = docMdf.acTransaction.GetObject(docMdf.acDataBase.LayerTableId, OpenMode.ForRead) as LayerTable;
            foreach (dynamic ltr in lt)
            {
                layerNames.Add(ltr.Name);
            }
            return layerNames;
        }

        private List<string> GetBlocks(DocumentModifier docMdf)
        {
            var blockNames = new List<string>();
            var lt = docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            foreach (dynamic btr in lt)
            {
                blockNames.Add(btr.Name);
            }
            // 剔除 *Model_Space 与 *Paper_Space
            var removedName = new List<int>();
            for (int i = 0; i < blockNames.Count; i++)
            {
                if ((blockNames[i].IndexOf("*Model_Space", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    || (blockNames[i].IndexOf("*Paper_Space", StringComparison.CurrentCultureIgnoreCase) >= 0))
                {
                    removedName.Add(i);
                }
            }
            for (int i = removedName.Count - 1; i >= 0; i--)
            {
                blockNames.RemoveAt(removedName[i]);
            }
            return blockNames;
        }

        #region ---   与 eZDataGridView 相关的事件处理

        private void EZdgvOnCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // 表头行的行号为 -1
            {
                var dgv = sender as DataGridView;

                var c = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;

            }
        }

        private void EZdgvOnDataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        #endregion
        #endregion

        #region --- 设置选项

        private class OptionDatasource
        {

            public string OptionName { get; }
            public string OptionValue { get; set; }

            public OptionDatasource(string optionName, string defaultValue)
            {
                OptionName = optionName;
                OptionValue = defaultValue;
            }
        }

        /// <summary>
        /// 用来显示在Datagridview控件中的数据
        /// </summary>
        private BindingList<OptionDatasource> LayerOptions = new BindingList<OptionDatasource>()
        {
            new OptionDatasource( "道路中心线", ProtectionOptions.LayerName_CenterAxis ),
            new OptionDatasource( "横断面信息", ProtectionOptions.LayerName_SectionInfo ),
            new OptionDatasource( "边坡线", ProtectionOptions.LayerName_Slope ),
            new OptionDatasource( "路面线",ProtectionOptions.LayerName_RoadSurface ),
            new OptionDatasource( "自然地面线", ProtectionOptions.LayerName_GroundSurface ),
            new OptionDatasource( "水位标志线", ProtectionOptions.LayerName_WaterLevel ),
        };

        /// <summary>
        /// 用来显示在Datagridview控件中的数据
        /// </summary>
        private BindingList<OptionDatasource> BlockOptions = new BindingList<OptionDatasource>()
        {
            new OptionDatasource( "断面信息", ProtectionOptions.BlockName_SectionInfo ),
            new OptionDatasource( "路线中心标高", ProtectionOptions.BlockName_CenterElevation ),
        };

        #endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            //
            ProtectionOptions.LayerName_CenterAxis = LayerOptions[0].OptionValue;
            ProtectionOptions.LayerName_SectionInfo = LayerOptions[1].OptionValue;
            ProtectionOptions.LayerName_Slope = LayerOptions[2].OptionValue;
            ProtectionOptions.LayerName_RoadSurface = LayerOptions[3].OptionValue;
            ProtectionOptions.LayerName_GroundSurface = LayerOptions[4].OptionValue;
            ProtectionOptions.LayerName_WaterLevel = LayerOptions[5].OptionValue;
            //
            ProtectionOptions.BlockName_SectionInfo = BlockOptions[0].OptionValue;
            ProtectionOptions.BlockName_CenterElevation = BlockOptions[1].OptionValue;
            //
            ProtectionOptions.MileageFieldDef = textBox_MileageFieldDef.Text;
            //
            Close();
        }
    }
}