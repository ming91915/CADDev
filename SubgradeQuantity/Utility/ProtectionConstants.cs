using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace eZcad.SubgradeQuantity.Utility
{
    public static class ProtectionConstants
    {
        /// <summary> 在<see cref="CommandMethodAttribute"/>中设置的外部命令的 GroupName。 </summary>
        public const string eZGroupCommnad = "SQ";

        /// <summary>
        /// 外部命令在 AutoCAD 界面中对应的控件的图片所在的文件夹。
        /// 当引用某个图片文件时，直接通过“<seealso cref="ImageDirectory"/> + "picture.png"”即可
        /// </summary>
        /// <remarks>“.\”表示当前正在执行的程序集所在的文件夹，“..\”表示当前正在执行的程序集所在的文件夹</remarks>
        public const string ImageDirectory = @"..\SubgradeQuantity\Resources\icons\";
            // @"D:\GithubProjects\CADDev\SubgradeQuantity\Resources\icons\"; // @"..\SubgradeQuantity\Resources\icons\";

        /// <summary> 在界面中进行 Editor.Select 搜索时，对于重合的区域所给出的容差 </summary>
        public const double CoincideTolerance = 1e-6;

        public const string LayerName_ProtectionMethod_Slope = "边坡防护形式";
        public const string LayerName_ProtectionMethod_Platform = "平台防护形式";
        public const string LayerName_LongitudinalSlopes = "边坡纵剖面示意图";

        /// <summary> 边坡平台的最长宽度 </summary>
        public const double MaxPlatformLength = 3.0;

        /// <summary> 边坡或者平台的最小长度，小于此长度则被忽略 </summary>
        public const double MinSlopeSegLength = 0.05;

        /// <summary> 水平向量 </summary>
        public static readonly Vector3d HorizontalVec3 = new Vector3d(1, 0, 0);

        /// <summary> 水平向量 </summary>
        public static readonly Vector2d HorizontalVec2 = new Vector2d(1, 0);
    }
}