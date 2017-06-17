using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace eZcad.Addins.SlopeProtection
{
    public static class ProtectionOptions
    {
     
        #region ---   BlockName

        /// <summary> 道路横断面信息的块参照的名称 </summary>
        public static string BlockName_SectionInfo = "断面信息"; // 断面信息

        /// <summary> 路线中心标高的块参照的块名称 </summary>
        public static string BlockName_CenterElevation = "路线中心标高"; // 路线中心标高

        #endregion

        /// <summary> 横断面信息的块参照中，标识里程信息的属性定义的名称 </summary>
        public static string MileageFieldDef = "0006";

        #region ---   LayerName

        /// <summary> 道路中心轴线所在的图层 </summary>
        public static string LayerName_CenterAxis = "0";

        /// <summary> 道路横断面信息所在的图层名称 </summary>
        public static string LayerName_SectionInfo = "路槽示意";

        /// <summary> 边坡线所在的图层名称 </summary>
        public static string LayerName_Slope = "路槽示意";

        /// <summary> 边坡线所在的图层名称 </summary>
        public static string LayerName_RoadSurface = "路槽示意";

        /// <summary> 自然地面线所在的图层名称 </summary>
        public static string LayerName_GroundSurface = "路槽示意";

        /// <summary> 自然地面线所在的图层名称 </summary>
        public static string LayerName_WaterLevel = "水位标志线";

        #endregion

        /// <summary> 道路大致宽度，单位为 m </summary>
        public static double RoadWidth = 30;

        #region ---   WaterLevel

        /// <summary> 水位标高，单位为m </summary>
        public static double WaterLevel = 1736.8;

        /// <summary> 填方防护考虑水位 </summary>
        public static bool ConsiderWaterLevel = true;

        /// <summary> 填方边坡防护的最高标高，其值一般是相对于水位标高而言的，比如位于水位标高之上1.0m </summary>
        public static double FillUpperEdge = 1738;

        #endregion
    }
}