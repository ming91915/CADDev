using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Utility
{
    /// <summary> AutoCAD 软件中的系统常数 </summary>
    public static class ACadConstants
    {
        /// <summary> 模型空间也属于<seealso cref="Layout"/>，其对应的LayoutName为“Model” </summary>
        public const string LayoutName_Model = "Model";

        /// <summary> AutoCAD 中默认的那个不打印层的名称 </summary>
        public const string LayerName_Defpoints = "Defpoints";

    }
}