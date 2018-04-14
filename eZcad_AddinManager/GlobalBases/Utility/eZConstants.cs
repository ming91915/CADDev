using Autodesk.AutoCAD.Runtime;

namespace eZcad.Utility
{
    public static class eZConstants
    {
        /// <summary>
        /// 在<see cref="CommandMethodAttribute"/>中设置的外部命令的 GroupName。
        /// </summary>
        public const string eZGroupCommnad = "eZcad";

        /// <summary>
        /// 外部命令在 AutoCAD 界面中对应的控件的图片所在的文件夹。
        /// 当引用某个图片文件时，直接通过“<seealso cref="ImageDirectory"/> + "picture.png"”即可
        /// </summary>
        /// <remarks>“.\”表示当前正在执行的程序集所在的文件夹，“..\”表示当前正在执行的程序集所在的文件夹</remarks>
        public const string ImageDirectory = @"..\Resources\icons\";
        // @"D:\GithubProjects\CADDev\SubgradeQuantity\Resources\icons\"; // @"..\SubgradeQuantity\Resources\icons\";

    }
}