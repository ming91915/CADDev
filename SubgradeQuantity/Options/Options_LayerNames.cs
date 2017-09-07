using System;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.SubgradeQuantity.Options
{
    public static class Options_LayerNames
    {
        /// <summary> 道路中心轴线所在的图层 </summary>
        public static string LayerName_CenterAxis = "道路中心线";

        /// <summary> 道路中线所对应的路面标高 </summary>
        public static string LayerName_CenterElevation = "中桩高程";

        /// <summary> 道路横断面信息所在的图层名称 </summary>
        public static string LayerName_SectionInfo = "数据栏";

        /// <summary> 自然地面或者清表线之下的台阶 </summary>
        public static string LayerName_Stairs = "台阶";

        #region ---   新添加 区分左右

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

        /// <summary> 将静态类中的数据保存到<seealso cref="Xrecord"/>对象中 </summary>
        /// <returns></returns>
        public static ResultBuffer ToResultBuffer()
        {
            var tp = typeof (Options_LayerNames);
            var fields = tp.GetFields(BindingFlags.Static | BindingFlags.Public);

            var generalBuff = new ResultBuffer();
            foreach (var f in fields)
            {
                var v = f.GetValue(null);
                generalBuff.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, v));
            }
            return generalBuff;
        }

        /// <summary> 将<seealso cref="Xrecord"/>对象中的数据刷新到内存中的静态类中 </summary>
        public static void FromXrecord(Xrecord xrec)
        {
            var buffs = xrec.Data.AsArray();
            var tp = typeof (Options_LayerNames);
            var fields = tp.GetFields(BindingFlags.Static | BindingFlags.Public);
            int index = 0;
            try
            {
                for (index = 0; index < fields.Length; index++)
                {
                    var v = (string) buffs[index].Value;
                    fields[index].SetValue(null, v);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"刷新选项数据“{fields[index].Name}”出错。\r\n{ex.StackTrace}");
            }
        }
    }
}