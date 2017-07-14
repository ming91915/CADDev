using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace eZcad.SubgradeQuantity.Utility
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
        public static string LayerName_CenterAxis = "道路中心线";

        /// <summary> 道路中线所对应的路面标高 </summary>
        public static string LayerName_CenterElevation = "中桩高程";

        /// <summary> 道路横断面信息所在的图层名称 </summary>
        public static string LayerName_SectionInfo = "数据栏";

        /// <summary> 边坡线所在的图层名称 </summary>
        public static string LayerName_Slope = "路槽示意";

        /// <summary> 边坡线所在的图层名称 </summary>
        public static string LayerName_RoadSurface = "路槽示意";

        /// <summary> 自然地面线所在的图层名称 </summary>
        public static string LayerName_GroundSurface = "路槽示意";

        /// <summary> 自然地面线所在的图层名称 </summary>
        public static string LayerName_WaterLevel = "水位标志线";

        #region ---   新添加 区分左右

        /// <summary> 自然地面或者清表线之下的台阶 </summary>
        public static string LayerName_Stairs = "台阶";

        /// <summary> 自然地面线所在的图层名称 </summary>
        public static string LayerName_GroundSurface_Left = "左地面线";
        public static string LayerName_GroundSurface_Right = "右地面线";

        /// <summary> 路面所在的图层名称 </summary>
        public static string LayerName_RoadSurface_Left = "左路面";
        public static string LayerName_RoadSurface_Right = "右路面";


        /// <summary> 路槽所在的图层名称 </summary>
        public static string LayerName_RoadCushion_Left = "左路槽";
        public static string LayerName_RoadCushion_Right = "右路槽";

        /// <summary> 边坡线所在的图层名称 </summary>
        public static string LayerName_Slope_Left_Cut = "左挖方边坡";
        public static string LayerName_Slope_Right_Cut = "右挖方边坡";
        public static string LayerName_Slope_Left_Fill = "左填方边坡";
        public static string LayerName_Slope_Right_Fill = "右填方边坡";


        /// <summary> 挡土墙所在的图层名称 </summary>
        public static string LayerName_RetainingWall_Left = "左挡墙";
        public static string LayerName_RetainingWall_Right = "右挡墙";

        /// <summary> 横断面用地界 </summary>
        public static string LayerName_Boundary_Left = "左用地界";
        /// <summary> 横断面用地界 </summary>
        public static string LayerName_Boundary_Right = "右用地界";

        /// <summary> 排水沟（填方坡底） </summary>
        public static string LayerName_DrainageDitch_Left = "左排水沟";
        public static string LayerName_DrainageDitch_Right = "右排水沟";

        /// <summary> 截水沟（挖方坡顶） </summary>
        public static string LayerName_CatchWater_Left = "左截水沟";
        public static string LayerName_CatchWater_Right = "右截水沟";

        /// <summary> 边沟（挖方坡底） </summary>
        public static string LayerName_SideDitch_Left = "左边沟";
        public static string LayerName_SideDitch_Right = "右边沟";
        
        #endregion

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

        #region ---   判断标准——低填浅挖

        /// <summary> 低填浅挖中，判断低填路堤中的中心填方高度（路面与自然地面）的最大值，单位为米 </summary>
        public static double ThinFill_MaxDepth = 1.5;
        /// <summary> 低填浅挖中，判断低填路堤时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n </summary>
        public static double ThinFill_SlopeCriterion_upper = 5;
        /// <summary> 低填浅挖中，判断低填路堤时，从中心线与自然地面交点向下进行倾角放射，射线角度为1:n </summary>
        public static double ThinFill_SlopeCriterion_lower= 5;
        /// <summary> 低填浅挖中，路槽中点（或道路中点）以下要保证0.8m的加固区，当路槽中点与自然地面的高度小于0.8m时，
        /// 需要在自然地面以下进行地基加固处理。此变量对应为0.8m的加固区，单位为米 </summary>
        public static double ThinFill_TreatedDepth = 0.8;


        /// <summary> 低填浅挖中，判断浅挖路堑中的中心挖方高度（路面与自然地面）的最大值，单位为米 </summary>
        public static double ShallowCutl_MaxDepth = 1.5;
        /// <summary> 低填浅挖中，判断浅挖路堑时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n </summary>
        public static double ShallowCutl_SlopeCriterion_upper = 5;

        #endregion
    }
}