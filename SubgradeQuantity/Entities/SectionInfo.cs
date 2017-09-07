using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 一个横断面的信息 </summary>
    public class SectionInfo
    {
        /// <summary>
        /// 路基信息的分区
        /// </summary>
        public enum InfoType : byte
        {
            /// <summary> 一般性的信息，不分左右 </summary>
            General,

            /// <summary> 路基左侧对象数据 </summary>
            Left,

            /// <summary> 路基右侧对象数据 </summary>
            Right,
        }

        public const string AppNameGeneral = "eZSubgrade_General";
        public const string AppNameLeft = "eZSubgrade_Left";
        public const string AppNameRight = "eZSubgrade_Right";

        private const string ctg_General = "通用";
        private const string ctg_Left = "左侧";
        private const string ctg_Right = "右侧";

        #region --- XData Fields

        #region --- General

        /// <summary> 是否进行过一次完整的计算 </summary>
        [Category(ctg_General), ReadOnly(true), Description("是否进行过一次完整的计算")]
        public bool FullyCalculated { get; set; }

        [Category(ctg_General), ReadOnly(true), Description("桩号")]
        public double Station { get; set; }

        /// <summary> 道路中心线所对应的路面标高 </summary>
        [Category(ctg_General), ReadOnly(false), Description("道路中心线所对应的路面标高")]
        public double CenterElevation_Road { get; set; }

        /// <summary> 道路中心线所对应的自然地面标高 </summary>
        [Category(ctg_General), ReadOnly(false), Description("道路中心线所对应的自然地面标高")]
        public double CenterElevation_Ground { get; set; }

        /// <summary> 道路中心线与路槽底的交点的标高 </summary>
        [Category(ctg_General), ReadOnly(false), Description("道路中心线与路槽底的交点的标高")]
        public double CenterElevation_Cushion { get; set; }

        /// <summary> 道路中心线在图形坐标系中的 X 值 </summary>
        [Category(ctg_General), ReadOnly(true), Description("道路中心线在图形坐标系中的 X 值")]
        public double CenterX { get; set; }

        /// <summary> 道路中心线与路面交点在图形坐标系中的 Y 值 </summary>
        [Category(ctg_General), ReadOnly(true), Description("道路中心线与路面交点在图形坐标系中的 Y 值")]
        public double CenterY { get; set; }

        /// <summary> 横断面信息的块参照对象 </summary>
        [Category(ctg_General), ReadOnly(false), Description(" 横断面信息的块参照对象")]
        public Handle InfoBlockHandle { get; set; }

        /// <summary> 自然地面下存在台阶 </summary>
        [Category(ctg_General), ReadOnly(true), Description(" 自然地面下存在台阶")]
        public bool StairExists { get; set; }

        /// <summary> 自然地面下面的台阶（台阶对象由多个多段线构成） </summary>
        [Category(ctg_General), ReadOnly(true), Description(" 自然地面下面的台阶线条集合（台阶对象由多个多段线构成）")]
        public Handle[] StairHandles { get; set; }

        #endregion

        #region --- Left

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧自然地表线（必须有）")]
        public bool LeftGroundSurfaceExists { get; set; }

        /// <summary> 自然地表面 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧自然地表线的句柄值")]
        public Handle LeftGroundSurfaceHandle { get; set; }

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧路面线（必须有）")]
        public bool LeftRoadSurfaceExists { get; set; }

        /// <summary> 路面 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧路面线的句柄值")]
        public Handle LeftRoadSurfaceHandle { get; set; }

        /// <summary> 左路面边缘点（包括土路肩的最边缘点）的几何坐标 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("左路面边缘点（包括土路肩的最边缘点）的几何坐标")]
        public Point3d LeftRoadEdge { get; set; }

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧路槽线")]
        public bool LeftRoadCushionExists { get; set; }

        /// <summary> 路槽 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧路槽线的句柄值")]
        public Handle LeftRoadCushionHandle { get; set; }

        /// <summary> 有挡墙，但是不确定是路肩墙、路堑墙，还是路堤墙 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("左侧有挡墙，但是不确定是路肩墙、路堑墙，还是路堤墙")]
        public bool LeftRetainingWallExists { get; set; }

        /// <summary> 挡土墙 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("左侧挡墙的句柄值，但是不确定是路肩墙、路堑墙，还是路堤墙")]
        public Handle LeftRetainingWallHandle { get; set; }

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧用地界线")]
        public bool LeftBoundaryExists { get; set; }

        /// <summary> 用地界 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧用地界线的句柄值")]
        public Handle LeftBoundaryHandle { get; set; }

        // ---------------------------------------------------------------------------
        /// <summary> 左边坡线存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长；又或者边坡线与挡墙线重合，即没有真实的物理边坡） </summary>
        [Category(ctg_Left), ReadOnly(true),
         Description("左边坡线存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长；又或者边坡线与挡墙线重合，即没有真实的物理边坡）")]
        public bool LeftSlopeExists { get; set; }

        /// <summary> 边坡 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("左侧边坡线的句柄值")]
        public Handle LeftSlopeHandle { get; set; }

        /// <summary> 左侧边坡为填方还是挖方，如果没有边坡线，默认为填方 </summary>
        [Category(ctg_Left), ReadOnly(true), Description("左侧边坡为填方还是挖方，如果没有边坡线，默认为填方")]
        public bool LeftSlopeFill { get; set; }

        // ---------------------------------------------------------------------------
        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧排水沟（填方坡底）")]
        public bool LeftDrainageDitchExists { get; set; }

        /// <summary> 排水沟（填方坡底） </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧排水沟（填方坡底）的句柄值")]
        public Handle LeftDrainageDitchHandle { get; set; }

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧截水沟（挖方坡顶）")]
        public bool LeftCatchWaterExists { get; set; }

        /// <summary> 截水沟（挖方坡顶） </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧截水沟（挖方坡顶）的句柄值")]
        public Handle LeftCatchWaterHandle { get; set; }

        [Category(ctg_Left), ReadOnly(true), Description("是否有匹配的左侧边沟（挖方坡底）")]
        public bool LeftSideDitchExists { get; set; }

        /// <summary> 边沟（挖方坡底） </summary>
        [Category(ctg_Left), ReadOnly(true), Description("匹配的左侧边沟（挖方坡底）的句柄值")]
        public Handle LeftSideDitchHandle { get; set; }

        #endregion

        #region --- Right

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧自然地表线（必须有）")]
        public bool RightGroundSurfaceExists { get; set; }

        /// <summary> 自然地表面 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧自然地表线的句柄值")]
        public Handle RightGroundSurfaceHandle { get; set; }

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧路面线（必须有）")]
        public bool RightRoadSurfaceExists { get; set; }

        /// <summary> 路面 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧路面线的句柄值")]
        public Handle RightRoadSurfaceHandle { get; set; }

        /// <summary> 左路面边缘点（包括土路肩的最边缘点）的几何坐标 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("左路面边缘点（包括土路肩的最边缘点）的几何坐标")]
        public Point3d RightRoadEdge { get; set; }

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧路槽线")]
        public bool RightRoadCushionExists { get; set; }

        /// <summary> 路槽 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧路槽线的句柄值")]
        public Handle RightRoadCushionHandle { get; set; }

        /// <summary> 有挡墙，但是不确定是路肩墙、路堑墙，还是路堤墙 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("右侧有挡墙，但是不确定是路肩墙、路堑墙，还是路堤墙")]
        public bool RightRetainingWallExists { get; set; }

        /// <summary> 挡土墙 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("右侧挡墙的句柄值，但是不确定是路肩墙、路堑墙，还是路堤墙")]
        public Handle RightRetainingWallHandle { get; set; }

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧用地界线")]
        public bool RightBoundaryExists { get; set; }

        /// <summary> 用地界 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧用地界线的句柄值")]
        public Handle RightBoundaryHandle { get; set; }

        // ---------------------------------------------------------------------------
        /// <summary> 左边坡线存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长；又或者边坡线与挡墙线重合，即没有真实的物理边坡） </summary>
        [Category(ctg_Right), ReadOnly(true),
         Description("左边坡线存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长；又或者边坡线与挡墙线重合，即没有真实的物理边坡）")]
        public bool RightSlopeExists { get; set; }

        /// <summary> 边坡 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("右侧边坡线的句柄值")]
        public Handle RightSlopeHandle { get; set; }

        /// <summary> 右侧边坡为填方还是挖方，如果没有边坡线，默认为填方 </summary>
        [Category(ctg_Right), ReadOnly(true), Description("右侧边坡为填方还是挖方，如果没有边坡线，默认为填方")]
        public bool RightSlopeFill { get; set; }

        // ---------------------------------------------------------------------------
        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧排水沟（填方坡底）")]
        public bool RightDrainageDitchExists { get; set; }

        /// <summary> 排水沟（填方坡底） </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧排水沟（填方坡底）的句柄值")]
        public Handle RightDrainageDitchHandle { get; set; }

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧截水沟（挖方坡顶）")]
        public bool RightCatchWaterExists { get; set; }

        /// <summary> 截水沟（挖方坡顶） </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧截水沟（挖方坡顶）的句柄值")]
        public Handle RightCatchWaterHandle { get; set; }

        [Category(ctg_Right), ReadOnly(true), Description("是否有匹配的右侧边沟（挖方坡底）")]
        public bool RightSideDitchExists { get; set; }

        /// <summary> 边沟（挖方坡底） </summary>
        [Category(ctg_Right), ReadOnly(true), Description("匹配的右侧边沟（挖方坡底）的句柄值")]
        public Handle RightSideDitchHandle { get; set; }

        #endregion

        #endregion

        /// <summary> 构造函数 </summary>
        public SectionInfo()
        {
            LeftSlopeFill = true;
            RightSlopeFill = true;
            StairHandles = new Handle[0];
        }

        #region ---   数据 与 ResultBuffer 的转换

        /// <summary> 从横断面轴线对象中提取对应的信息，如果轴线中没有任何数据，则返回 null </summary>
        /// <param name="centerline"></param>
        /// <returns></returns>
        public static SectionInfo FromCenterLine(Line centerline)
        {
            var xData = new SectionInfo();
            bool foundAtLeastOne = false;
            // 从 AutoCAD 中读取
            var xd = centerline.GetXDataForApplication(AppNameGeneral);
            if (xd != null)
            {
                foundAtLeastOne = true;
                xData.FromResultBuffer(xd);
            }
            xd = centerline.GetXDataForApplication(AppNameLeft);
            if (xd != null)
            {
                foundAtLeastOne = true;
                xData.FromResultBuffer(xd);
            }
            xd = centerline.GetXDataForApplication(AppNameRight);
            if (xd != null)
            {
                foundAtLeastOne = true;
                xData.FromResultBuffer(xd);
            }
            //
            return foundAtLeastOne ? xData : null;
        }

        /// <summary>
        /// 利用<seealso cref="buff"/>中的数据对<seealso cref="SectionInfo"/>对象进行数据填充
        /// </summary>
        /// <param name="buff"></param>
        public void FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            if (buffs.Length == 0) return;
            ushort baseId;
            var appName = buffs[0].Value.ToString();
            if (appName == AppNameGeneral)
            {
                var sl = this;
                try
                {
                    sl.FullyCalculated = Utils.GetExtendedDataBool(buffs[1]);
                    sl.Station = (double) buffs[2].Value;
                    sl.CenterElevation_Road = (double) buffs[3].Value;
                    sl.CenterElevation_Ground = (double) buffs[4].Value;
                    sl.CenterElevation_Cushion = (double) buffs[5].Value;
                    sl.CenterX = (double) buffs[6].Value;
                    sl.CenterY = (double) buffs[7].Value;
                    sl.InfoBlockHandle = Utils.ConvertToHandle(buffs[8].Value.ToString());

                    // 台阶
                    baseId = 8;
                    sl.StairExists = Utils.GetExtendedDataBool(buffs[baseId + 1]);
                    var stairsCount = buffs.Length - baseId - 2;
                    var stairHandles = new Handle[stairsCount];
                    for (int i = 0; i < stairsCount; i++)
                    {
                        stairHandles[i] = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());
                    }
                    sl.StairHandles = stairHandles;
                }
                catch (Exception ex)
                {
                }
            }
            else if (appName == AppNameLeft)
            {
                var sl = this;

                try
                {
                    sl.LeftGroundSurfaceExists = Utils.GetExtendedDataBool(buffs[1]);
                    sl.LeftGroundSurfaceHandle = Utils.ConvertToHandle(buffs[2].Value.ToString());

                    sl.LeftRoadSurfaceExists = Utils.GetExtendedDataBool(buffs[3]);
                    sl.LeftRoadSurfaceHandle = Utils.ConvertToHandle(buffs[4].Value.ToString());
                    sl.LeftRoadEdge = (Point3d) buffs[5].Value;

                    baseId = 5;
                    sl.LeftRoadCushionExists = Utils.GetExtendedDataBool(buffs[baseId + 1]);
                    sl.LeftRoadCushionHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());

                    sl.LeftRetainingWallExists = Utils.GetExtendedDataBool(buffs[baseId + 3]);
                    sl.LeftRetainingWallHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());

                    sl.LeftBoundaryExists = Utils.GetExtendedDataBool(buffs[baseId + 5]);
                    sl.LeftBoundaryHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());

                    // 边坡相关
                    sl.LeftSlopeExists = Utils.GetExtendedDataBool(buffs[baseId + 7]);
                    sl.LeftSlopeHandle = Utils.ConvertToHandle(buffs[baseId + 8].Value.ToString());
                    sl.LeftSlopeFill = Utils.GetExtendedDataBool(buffs[baseId + 9]);

                    // 排水沟相关
                    baseId = 14;
                    sl.LeftDrainageDitchExists = Utils.GetExtendedDataBool(buffs[baseId + 1]);
                    sl.LeftDrainageDitchHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());
                    sl.LeftCatchWaterExists = Utils.GetExtendedDataBool(buffs[baseId + 3]);
                    sl.LeftCatchWaterHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());
                    sl.LeftSideDitchExists = Utils.GetExtendedDataBool(buffs[baseId + 5]);
                    sl.LeftSideDitchHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());
                }
                catch (Exception ex)
                {
                    Debug.Print("提取横断面左侧数据出错" + "\r\n" + ex.Message + ex.StackTrace);
                }
            }
            else if (appName == AppNameRight)
            {
                var sl = this;
                try
                {
                    sl.RightGroundSurfaceExists = Utils.GetExtendedDataBool(buffs[1]);
                    sl.RightGroundSurfaceHandle = Utils.ConvertToHandle(buffs[2].Value.ToString());

                    sl.RightRoadSurfaceExists = Utils.GetExtendedDataBool(buffs[3]);
                    sl.RightRoadSurfaceHandle = Utils.ConvertToHandle(buffs[4].Value.ToString());
                    sl.RightRoadEdge = (Point3d) buffs[5].Value;

                    baseId = 5;
                    sl.RightRoadCushionExists = Utils.GetExtendedDataBool(buffs[baseId + 1]);
                    sl.RightRoadCushionHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());

                    sl.RightRetainingWallExists = Utils.GetExtendedDataBool(buffs[baseId + 3]);
                    sl.RightRetainingWallHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());

                    sl.RightBoundaryExists = Utils.GetExtendedDataBool(buffs[baseId + 5]);
                    sl.RightBoundaryHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());

                    // 边坡相关
                    sl.RightSlopeExists = Utils.GetExtendedDataBool(buffs[baseId + 7]);
                    sl.RightSlopeHandle = Utils.ConvertToHandle(buffs[baseId + 8].Value.ToString());
                    sl.RightSlopeFill = Utils.GetExtendedDataBool(buffs[baseId + 9]);

                    // 排水沟相关
                    baseId = 14;
                    sl.RightDrainageDitchExists = Utils.GetExtendedDataBool(buffs[baseId + 1]);
                    sl.RightDrainageDitchHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());
                    sl.RightCatchWaterExists = Utils.GetExtendedDataBool(buffs[baseId + 3]);
                    sl.RightCatchWaterHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());
                    sl.RightSideDitchExists = Utils.GetExtendedDataBool(buffs[baseId + 5]);
                    sl.RightSideDitchHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());
                }
                catch (Exception ex)
                {
                    Debug.Print("提取横断面右侧数据出错" + "\r\n" + ex.Message + ex.StackTrace);
                }
            }
        }

        public ResultBuffer ToResultBuffer(InfoType type)
        {
            ResultBuffer buff = null;
            switch (type)
            {
                case InfoType.General:
                    buff = new ResultBuffer
                        (
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameGeneral),
                        Utils.SetExtendedDataBool(FullyCalculated),
                        new TypedValue((int) DxfCode.ExtendedDataReal, Station),
                        new TypedValue((int) DxfCode.ExtendedDataReal, CenterElevation_Road),
                        new TypedValue((int) DxfCode.ExtendedDataReal, CenterElevation_Ground),
                        new TypedValue((int) DxfCode.ExtendedDataReal, CenterElevation_Cushion),
                        new TypedValue((int) DxfCode.ExtendedDataReal, CenterX),
                        new TypedValue((int) DxfCode.ExtendedDataReal, CenterY),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, InfoBlockHandle),
                        Utils.SetExtendedDataBool(StairExists)
                        );
                    // 台阶
                    for (int i = 0; i < StairHandles.Length; i++)
                    {
                        buff.Add(new TypedValue((int) DxfCode.ExtendedDataHandle, StairHandles[i]));
                    }
                    break;
                case InfoType.Left:

                    buff = new ResultBuffer
                        (
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameLeft),
                        Utils.SetExtendedDataBool(LeftGroundSurfaceExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftGroundSurfaceHandle),
                        Utils.SetExtendedDataBool(LeftRoadSurfaceExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftRoadSurfaceHandle),
                        new TypedValue((int) DxfCode.ExtendedDataXCoordinate, LeftRoadEdge),
                        Utils.SetExtendedDataBool(LeftRoadCushionExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftRoadCushionHandle),
                        Utils.SetExtendedDataBool(LeftRetainingWallExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftRetainingWallHandle),
                        Utils.SetExtendedDataBool(LeftBoundaryExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftBoundaryHandle),
                        // 边坡相关
                        Utils.SetExtendedDataBool(LeftSlopeExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftSlopeHandle),
                        Utils.SetExtendedDataBool(LeftSlopeFill),
                        // 排水沟相关
                        Utils.SetExtendedDataBool(LeftDrainageDitchExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftDrainageDitchHandle),
                        Utils.SetExtendedDataBool(LeftCatchWaterExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftCatchWaterHandle),
                        Utils.SetExtendedDataBool(LeftSideDitchExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, LeftSideDitchHandle)
                        );
                    break;
                case InfoType.Right:
                    buff = new ResultBuffer
                        (
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameRight),
                        Utils.SetExtendedDataBool(RightGroundSurfaceExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightGroundSurfaceHandle),
                        Utils.SetExtendedDataBool(RightRoadSurfaceExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightRoadSurfaceHandle),
                        new TypedValue((int) DxfCode.ExtendedDataXCoordinate, RightRoadEdge),
                        Utils.SetExtendedDataBool(RightRoadCushionExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightRoadCushionHandle),
                        Utils.SetExtendedDataBool(RightRetainingWallExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightRetainingWallHandle),
                        Utils.SetExtendedDataBool(RightBoundaryExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightBoundaryHandle),
                        // 边坡相关
                        Utils.SetExtendedDataBool(RightSlopeExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightSlopeHandle),
                        Utils.SetExtendedDataBool(RightSlopeFill),
                        // 排水沟相关
                        Utils.SetExtendedDataBool(RightDrainageDitchExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightDrainageDitchHandle),
                        Utils.SetExtendedDataBool(RightCatchWaterExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightCatchWaterHandle),
                        Utils.SetExtendedDataBool(RightSideDitchExists),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, RightSideDitchHandle)
                        );
                    break;
            }
            return buff;
        }

        /// <summary> 清除 XData 中的数据 </summary>
        /// <param name="clearAll">如果其值为 true，则<paramref name="type"/>值不起作用</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ResultBuffer ClearValue(bool clearAll, InfoType type = InfoType.General)
        {
            ResultBuffer buff = null;
            if (clearAll)
            {
                buff =
                    new ResultBuffer(
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameGeneral),
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameLeft),
                        new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameRight));
                // 此时，Entity中的XData集合里，对应AppName下的所有数据，连同AppName这一项本身，都在实体中删除了。
                // 但是此AppName在 RegAppTable 中对应的 RegAppTableRecord 定义还是存在的。
            }
            else
            {
                switch (type)
                {
                    case InfoType.General:
                        buff = new ResultBuffer(new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameGeneral));
                        break;
                    case InfoType.Left:
                        buff = new ResultBuffer(new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameLeft));
                        break;
                    case InfoType.Right:
                        buff = new ResultBuffer(new TypedValue((int) DxfCode.ExtendedDataRegAppName, AppNameRight));
                        break;
                }
            }
            return buff;
        }

        #endregion

        #region --- AutoCAD中几何坐标 与 横断面图中的 标高进行对应

        /// <summary> 根据标高值返回对应的几何Y值 </summary>
        public double GetYFromElev(double elevation)
        {
            return CenterY - CenterElevation_Road + elevation;
        }

        /// <summary> 根据几何坐标的Y值返回对应的标高值 </summary>
        public double GetEleFromY(double y)
        {
            return CenterElevation_Road - CenterY + y;
        }

        #endregion

        #region --- 常用方法

        /// <summary> 道路中心为填方 </summary>
        /// <returns></returns>
        public bool IsCenterFill()
        {
            return CenterElevation_Road >= CenterElevation_Ground;
        }

        /// <summary>
        /// 计算道路横断面的某一侧中，从路面中心到边坡外边缘的范围内，属于填方的区域在 AutoCAD 几何中的 X 范围
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="left"></param>
        /// <param name="slp"> 某一侧边坡，其值可能为null，表示此侧没有边坡线 </param>
        /// <param name="cGround"></param>
        /// <param name="db"></param>
        /// <param name="edgeXleft">此侧边坡的填方左边界</param>
        /// <param name="edgeXright">此侧边坡的填方右边界</param>
        /// <returns>如果没有填方区域，则返回 false </returns>
        public bool GetFillSlopeXRange(bool left, SlopeLine slp, CompositeCurve2d cGround, Database db,
            out double edgeXleft, out double edgeXright)
        {
            edgeXleft = 0.0;
            edgeXright = 0.0;
            var centerFill = IsCenterFill();
            var slopeFill = slp == null || slp.XData.FillCut;

            if (!centerFill && (slp == null || !slopeFill)) return false;

            // 确定进行搜索的左右边界：路基边缘（或边坡脚） 到 道路中线
            double roadEdge = left ? LeftRoadEdge.X : RightRoadEdge.X;

            double slopeEdge = roadEdge; // 边坡的坡脚的 X 值
            if (slp != null && slp.XData.Slopes.Count > 0)
            {
                var data = slp.XData;
                slopeEdge = data.Slopes[data.Slopes.Count - 1].OuterPoint.X;
            }
            if (centerFill && slopeFill)
            {
                // 道路中心与边坡均为填方
                double edgeX1 = CenterX;

                edgeXleft = Math.Min(edgeX1, slopeEdge);
                edgeXright = Math.Max(edgeX1, slopeEdge);
            }
            else
            {
                // 说明 坡脚与道路中心这二者中有一个为挖方，另一个为填方
                var roadSurfHandle = left ? LeftRoadSurfaceHandle : RightRoadSurfaceHandle;
                var roadSurf = roadSurfHandle.GetDBObject<Polyline>(db);
                var cRoad = roadSurf.Get2dLinearCurve();
                var inters = new CurveCurveIntersector2d(cRoad, cGround);
                double iX;
                if (inters.NumberOfIntersectionPoints > 0)
                {
                    iX = inters.GetIntersectionPoint(0).X;
                }
                else
                {
                    // 这种情况极少会出现，但测试中确实会有，即自然地面线与挖方坡底边沟相交，而不与路面相交
                    iX = roadEdge;
                }
                // 自然地面与路面的交点
                var roadWidth = Math.Abs((roadEdge - CenterX));
                var innerRatio = Math.Abs((iX - CenterX)/roadWidth);
                if ((centerFill && innerRatio > 0.5))
                {
                    // 靠道路中心为填方，边坡为挖方
                    if (left)
                    {
                        edgeXleft = iX;
                        edgeXright = CenterX;
                    }
                    else
                    {
                        edgeXleft = CenterX;
                        edgeXright = iX;
                    }
                }
                else if (!centerFill && innerRatio <= 0.5)
                {
                    // 靠道路中心为挖方，边坡为填方
                    if (left)
                    {
                        edgeXleft = slopeEdge;
                        edgeXright = iX;
                    }
                    else
                    {
                        edgeXleft = iX;
                        edgeXright = slopeEdge;
                    }
                }
                else
                {
                    // 填方区域太小
                    return false;
                }
            }
            return true;
        }

        /// <summary> 某侧边坡是否有路肩墙（因为某侧的挡土墙对象有可能是护脚墙） </summary>
        /// <param name="left"></param>
        /// <param name="slp">其值可以为null，表明此侧没有边坡对象</param>
        /// <returns></returns>
        public bool HasShoulderWall(bool left, SlopeLine slp)
        {
            // 有挡墙，但不一定是路肩墙
            var hasRetainingWall = left ? LeftRetainingWallExists : RightRetainingWallExists;
            if (hasRetainingWall && (slp != null && slp.XData.FillCut && slp.XData.Slopes.Count == 0))
            {
                // 说明有路肩墙
                return true;
            }
            return false;
        }

        #endregion

        #region --- 信息输出

        public const string InfoHeader = "桩号\t标识\t边坡长度\t边坡高度\t位置\t填挖\t防护" +
                                         "\t坡顶标高\t坡底标高" +
                                         "\t水下坡长\t填方顶以下坡长\t中心路面标高\t中心自然标高\t中心填方高度";

        /// <summary>
        /// 将
        /// </summary>
        /// <returns></returns>
        public string GetInfo()
        {
            var v = GetInfoVector();
            var sb = new StringBuilder();
            foreach (var s in v)
            {
                sb.Append(s.ToString() + ',');
            }
            return sb.ToString();
        }

        public object[] GetInfoVector()
        {
            //var left = OnLeft ? "L" : "R";
            //var fill = FillExcav ? "填方" : "挖方";

            return new object[]
            {
                //Station, "测量", SlopeLength, TopElevation - BottomElevation, left, fill, Style.ToString(),
                //TopElevation, BottomElevation,
                //SlopeLengthBelowWaterLevel, SlopeLengthBeThinFillTop, CenterElevation, NaturalSurfElevation,
                //CenterElevation - NaturalSurfElevation
            };
        }

        public static object[,] GetAllInfo(IList<SectionInfo> sectionInfos)
        {
            var colCount = InfoHeader.Split('\t').Length;
            var rowCount = sectionInfos.Count;
            var res = new object[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                var sd = sectionInfos[r];
                var rowData = sd.GetInfoVector();
                for (int c = 0; c < colCount; c++)
                {
                    res[r, c] = rowData[c];
                }
            }
            return res;
        }

        public override string ToString()
        {
            return $"桩号: {Station},路面标高: {CenterElevation_Road}";
        }

        #endregion
    }
}