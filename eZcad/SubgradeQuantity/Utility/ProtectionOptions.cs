namespace eZcad.SubgradeQuantity.Utility
{
    public static class ProtectionOptions
    {
        /// <summary> 横断面信息的块参照中，标识里程信息的属性定义的名称 </summary>
        public static string MileageFieldDef = "0006";

        /// <summary> 道路大致宽度，单位为 m </summary>
        public static double RoadWidth = 30;

        #region ---   BlockName

        /// <summary> 道路横断面信息的块参照的名称 </summary>
        public static string BlockName_SectionInfo = "断面信息"; // 断面信息

        /// <summary> 路线中心标高的块参照的块名称 </summary>
        public static string BlockName_CenterElevation = "路线中心标高"; // 路线中心标高

        #endregion

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