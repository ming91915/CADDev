using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    public partial class ItemLister : UserControl
    {
        public ItemLister()
        {
            InitializeComponent();
            //
        }

        /// <summary> 当用户双击某一项时触发此事件 </summary>
        public event Action<Control, string> ItemDetermined;

        /// <summary> 当用户单击某一项时触发此事件 </summary>
        public event Action<Control, string> ItemRaised;

        /// <summary> 导入所有的防护方式列表 </summary>
        public void ImportItems(IList<string> itemTexts, IList<object> itemValues)
        {

            for (int i = 0; i < itemTexts.Count; i++)
            {
                var itemText = itemTexts[i];
                var itemValue = itemValues[i];
                var label = new Label()
                {
                    AutoSize = false,
                    Size = new Size(100, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    //
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = itemText,
                    Tag = itemValue,
                };
                label.MouseDoubleClick += BtnOnMouseDoubleClick;
                label.MouseClick += BtnOnMouseClick;
                toolTip1.SetToolTip(label, itemValue.ToString());
                //
                flowLayoutPanel1.Controls.Add(label);
            }
        }

        #region ---   点击事件

        private Control _lastActivatedControl;
        private readonly Color _activeColor = Color.Salmon;
        private readonly Color _deactiveColor = SystemColors.Control;

        private void BtnOnMouseClick(object sender, MouseEventArgs e)
        {
            var btn = sender as Control;
            var prot = btn.Tag as string;
            // 界面显示
            SetButtonUI(btn);
            if (e.Button == MouseButtons.Left)
            {
                // 触发事件
                ItemRaised?.Invoke(btn, prot);
            }
        }

        private void BtnOnMouseDoubleClick(object sender, EventArgs eventArgs)
        {
            var label = sender as Control;
            var prot = label.Tag as string;
            //// 界面显示
            SetButtonUI(label);
            // 触发事件
            ItemDetermined?.Invoke(label, prot);
        }

        private void SetButtonUI(Control activeButton)
        {
            if (_lastActivatedControl != null)
            {
                _lastActivatedControl.BackColor = _deactiveColor;
            }
            _lastActivatedControl = activeButton;
            //
            activeButton.BackColor = _activeColor;
        }

        #endregion
    }
}