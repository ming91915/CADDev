using System;
using System.Windows.Forms;
using eZcad.SubgradeQuantity.Entities;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity
{
    /// <summary> 参数信息的添加或者对已有参数文件的编辑 </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PropertyEditor : FormOk
    {
        #region ---   Property

        public object Instance
        {
            get { return propertyGrid1.SelectedObject; }
        }

        #endregion

        #region ---   构造函数


        public PropertyEditor()
        {
            InitializeComponent();
        }

        /// <summary> 构造函数 </summary>
        /// <param name="instance">要进行绑定和参数设置的那个对象的实例</param>
        public PropertyEditor(string formTitle, object instance) : this()
        {
            //
            Text = formTitle;
            //
            if (instance == null)
            {
                throw new NullReferenceException("进行属性编辑的对象不能为空");
            }
            //
            propertyGrid1.SelectedObject = instance;
        }
        #endregion

        #region ---   属性值发生变化

        /// <summary> 当绑定的对象的属性值发生变化时 </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //if (e.ChangedItem.Label == "Type")
            //{
            //}
        }

        #endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}