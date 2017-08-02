using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eZcad.SubgradeQuantity
{

    /// <summary> 用来描述外部命令的界面显示的信息 </summary>
    public class RibbonItemAttribute : Attribute
    {
        /// <summary> 控件的名称 </summary>
        public string Text { get; }
        /// <summary> 控件弹出的描述性说明 </summary>
        public string Description { get; }
        /// <summary> 控件图像的路径，可以为相对路径。其对应的图像的像素为 32*32 </summary>
        public string LargeImagePath { get; }
        /// <summary> 控件图像的路径，可以为相对路径。其对应的图像的像素为 16*16 </summary>
        public string SmallImagePath { get; }

        public RibbonItemAttribute(string text, string description,
            string largeImagePath = "", string smallImagePath = "")
        {
            Text = text;
            Description = description;
            LargeImagePath = largeImagePath;
            SmallImagePath = smallImagePath;
        }
    }
}
