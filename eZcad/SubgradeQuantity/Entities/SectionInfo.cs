using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
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
        //private const string ctg_Others = "其他";

        #region --- XData Fields

        #region --- General

        /// <summary> 是否进行过一次完整的计算 </summary>
        [Category(ctg_General), ReadOnly(true), Description("计算值")]
        public bool FullyCalculated { get; set; }

        [Category(ctg_General), ReadOnly(false), Description("桩号")]
        public double Mileage { get; set; }

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
        [Category(ctg_General), ReadOnly(false), Description("道路中心线在图形坐标系中的 X 值")]
        public double CenterX { get; set; }

        /// <summary> 道路中心线与路面交点在图形坐标系中的 Y 值 </summary>
        [Category(ctg_General), ReadOnly(false), Description("道路中心线与路面交点在图形坐标系中的 Y 值")]
        public double CenterY { get; set; }

        /// <summary> 横断面信息的块参数对象 </summary>
        public Handle InfoBlockHandle;

        public bool StairExists { get; set; }
        /// <summary> 自然地面下面的台阶（台阶对象由多个多段线构成） </summary>
        public Handle[] StairHandles = new Handle[0];

        #endregion

        #region --- Left

        public bool LeftGroundSurfaceExists { get; set; }
        /// <summary> 自然地表面 </summary>
        public Handle LeftGroundSurfaceHandle;

        public bool LeftRoadSurfaceExists { get; set; }
        /// <summary> 路面 </summary>
        public Handle LeftRoadSurfaceHandle;

        public bool LeftRoadCushionExists { get; set; }
        /// <summary> 路槽 </summary>
        public Handle LeftRoadCushionHandle;

        public bool LeftRetainingWallExists { get; set; }
        /// <summary> 挡土墙 </summary>
        public Handle LeftRetainingWallHandle;

        public bool LeftBoundaryExists { get; set; }
        /// <summary> 用地界 </summary>
        public Handle LeftBoundaryHandle;

        // ---------------------------------------------------------------------------
        /// <summary> 左边坡对象存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长） </summary>
        public bool LeftSlopeExists { get; set; }
        /// <summary> 边坡 </summary>
        public Handle LeftSlopeHandle;
        /// <summary> 边坡为填方还是挖方 </summary>
        public bool LeftSlopeFill = true;

        // ---------------------------------------------------------------------------
        public bool LeftDrainageDitchExists { get; set; }
        /// <summary> 排水沟（填方坡底） </summary>
        public Handle LeftDrainageDitchHandle;

        public bool LeftCatchWaterExists { get; set; }
        /// <summary> 截水沟（挖方坡顶） </summary>
        public Handle LeftCatchWaterHandle;

        public bool LeftSideDitchExists { get; set; }
        /// <summary> 边沟（挖方坡底） </summary>
        public Handle LeftSideDitchHandle;

        #endregion

        #region --- Right

        public bool RightGroundSurfaceExists { get; set; }
        /// <summary> 自然地表面 </summary>
        public Handle RightGroundSurfaceHandle;

        public bool RightRoadSurfaceExists { get; set; }
        /// <summary> 路面 </summary>
        public Handle RightRoadSurfaceHandle;

        public bool RightRoadCushionExists { get; set; }
        /// <summary> 路槽 </summary>
        public Handle RightRoadCushionHandle;

        public bool RightRetainingWallExists { get; set; }
        /// <summary> 挡土墙 </summary>
        public Handle RightRetainingWallHandle;

        public bool RightBoundaryExists { get; set; }
        /// <summary> 用地界 </summary>
        public Handle RightBoundaryHandle;

        // ---------------------------------------------------------------------------
        /// <summary> 右边坡对象存在，但是可能没有实际的边坡（比如边坡线非常短，短于指定的最小坡长） </summary>
        public bool RightSlopeExists { get; set; }
        /// <summary> 边坡 </summary>
        public Handle RightSlopeHandle;
        /// <summary> 边坡为填方还是挖方 </summary>
        public bool RightSlopeFill = true;

        // ---------------------------------------------------------------------------
        /// <summary> 排水沟（填方坡底） </summary>
        public bool RightDrainageDitchExists { get; set; }
        /// <summary> 排水沟（填方坡底） </summary>
        public Handle RightDrainageDitchHandle;

        /// <summary> 截水沟（挖方坡顶） </summary>
        public bool RightCatchWaterExists { get; set; }
        /// <summary> 截水沟（挖方坡顶） </summary>
        public Handle RightCatchWaterHandle;

        /// <summary> 边沟（挖方坡底） </summary>
        public bool RightSideDitchExists { get; set; }
        /// <summary> 边沟（挖方坡底） </summary>
        public Handle RightSideDitchHandle;

        #endregion

        #endregion

        #region ---   数据 与 ResultBuffer 的转换

        /// <summary> 从横断面轴线对象中提取对应的信息，如果轴线中没有任何数据，则返回 null </summary>
        /// <param name="centerline"></param>
        /// <returns></returns>
        public static SectionInfo FromCenterLine(Line centerline)
        {
            var xData = new SectionInfo();
            bool foundAtLeastOne = false;
            // 从 AutoCAD 中读取
            var xd = centerline.GetXDataForApplication(SectionInfo.AppNameGeneral);
            if (xd != null)
            {
                foundAtLeastOne = true;
                xData.FromResultBuffer(xd);
            }
            xd = centerline.GetXDataForApplication(SectionInfo.AppNameLeft);
            if (xd != null)
            {
                foundAtLeastOne = true;
                xData.FromResultBuffer(xd);
            }
            xd = centerline.GetXDataForApplication(SectionInfo.AppNameRight);
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
                    sl.FullyCalculated = (Int16)buffs[1].Value == 1;
                    sl.Mileage = (double)buffs[2].Value;
                    sl.CenterElevation_Road = (double)buffs[3].Value;
                    sl.CenterElevation_Ground = (double)buffs[4].Value;
                    sl.CenterElevation_Cushion = (double)buffs[5].Value;
                    sl.CenterX = (double)buffs[6].Value;
                    sl.CenterY = (double)buffs[7].Value;
                    sl.InfoBlockHandle = Utils.ConvertToHandle(buffs[8].Value.ToString());

                    // 台阶
                    baseId = 8;
                    sl.StairExists = (Int16)buffs[baseId + 1].Value == 1;
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
                    sl.LeftGroundSurfaceExists = (Int16)buffs[1].Value == 1;
                    sl.LeftGroundSurfaceHandle = Utils.ConvertToHandle(buffs[2].Value.ToString());

                    sl.LeftRoadSurfaceExists = (Int16)buffs[3].Value == 1;
                    sl.LeftRoadSurfaceHandle = Utils.ConvertToHandle(buffs[4].Value.ToString());

                    sl.LeftRoadCushionExists = (Int16)buffs[5].Value == 1;
                    sl.LeftRoadCushionHandle = Utils.ConvertToHandle(buffs[6].Value.ToString());

                    sl.LeftRetainingWallExists = (Int16)buffs[7].Value == 1;
                    sl.LeftRetainingWallHandle = Utils.ConvertToHandle(buffs[8].Value.ToString());

                    sl.LeftBoundaryExists = (Int16)buffs[9].Value == 1;
                    sl.LeftBoundaryHandle = Utils.ConvertToHandle(buffs[10].Value.ToString());

                    // 边坡相关
                    sl.LeftSlopeExists = (Int16)buffs[11].Value == 1;
                    sl.LeftSlopeHandle = Utils.ConvertToHandle(buffs[12].Value.ToString());
                    sl.LeftSlopeFill = (Int16)buffs[13].Value == 1;

                    // 排水沟相关
                    baseId = 13;
                    sl.LeftDrainageDitchExists = (Int16)buffs[baseId + 1].Value == 1;
                    sl.LeftDrainageDitchHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());
                    sl.LeftCatchWaterExists = (Int16)buffs[baseId + 3].Value == 1;
                    sl.LeftCatchWaterHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());
                    sl.LeftSideDitchExists = (Int16)buffs[baseId + 5].Value == 1;
                    sl.LeftSideDitchHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());
                }
                catch (Exception ex)
                {
                }
            }
            else if (appName == AppNameRight)
            {
                var sl = this;
                try
                {
                    sl.RightGroundSurfaceExists = (Int16)buffs[1].Value == 1;
                    sl.RightGroundSurfaceHandle = Utils.ConvertToHandle(buffs[2].Value.ToString());

                    sl.RightRoadSurfaceExists = (Int16)buffs[3].Value == 1;
                    sl.RightRoadSurfaceHandle = Utils.ConvertToHandle(buffs[4].Value.ToString());

                    sl.RightRoadCushionExists = (Int16)buffs[5].Value == 1;
                    sl.RightRoadCushionHandle = Utils.ConvertToHandle(buffs[6].Value.ToString());

                    sl.RightRetainingWallExists = (Int16)buffs[7].Value == 1;
                    sl.RightRetainingWallHandle = Utils.ConvertToHandle(buffs[8].Value.ToString());

                    sl.RightBoundaryExists = (Int16)buffs[9].Value == 1;
                    sl.RightBoundaryHandle = Utils.ConvertToHandle(buffs[10].Value.ToString());

                    // 边坡相关
                    sl.RightSlopeExists = (Int16)buffs[11].Value == 1;
                    sl.RightSlopeHandle = Utils.ConvertToHandle(buffs[12].Value.ToString());
                    sl.RightSlopeFill = (Int16)buffs[13].Value == 1;

                    // 排水沟相关
                    baseId = 13;
                    sl.RightDrainageDitchExists = (Int16)buffs[baseId + 1].Value == 1;
                    sl.RightDrainageDitchHandle = Utils.ConvertToHandle(buffs[baseId + 2].Value.ToString());
                    sl.RightCatchWaterExists = (Int16)buffs[baseId + 3].Value == 1;
                    sl.RightCatchWaterHandle = Utils.ConvertToHandle(buffs[baseId + 4].Value.ToString());
                    sl.RightSideDitchExists = (Int16)buffs[baseId + 5].Value == 1;
                    sl.RightSideDitchHandle = Utils.ConvertToHandle(buffs[baseId + 6].Value.ToString());
                }
                catch (Exception ex)
                {
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
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameGeneral),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, FullyCalculated),
                        new TypedValue((int)DxfCode.ExtendedDataReal, Mileage),
                        new TypedValue((int)DxfCode.ExtendedDataReal, CenterElevation_Road),
                        new TypedValue((int)DxfCode.ExtendedDataReal, CenterElevation_Ground),
                        new TypedValue((int)DxfCode.ExtendedDataReal, CenterElevation_Cushion),
                        new TypedValue((int)DxfCode.ExtendedDataReal, CenterX),
                        new TypedValue((int)DxfCode.ExtendedDataReal, CenterY),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, InfoBlockHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, StairExists)
                        );
                    // 台阶
                    for (int i = 0; i < StairHandles.Length; i++)
                    {
                        buff.Add(new TypedValue((int)DxfCode.ExtendedDataHandle, StairHandles[i]));
                    }
                    break;
                case InfoType.Left:
                    buff = new ResultBuffer
                        (
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameLeft),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftGroundSurfaceExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftGroundSurfaceHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftRoadSurfaceExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftRoadSurfaceHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftRoadCushionExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftRoadCushionHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftRetainingWallExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftRetainingWallHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftBoundaryExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftBoundaryHandle),
                        // 边坡相关
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftSlopeExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftSlopeHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftSlopeFill),
                        // 排水沟相关
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftDrainageDitchExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftDrainageDitchHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftCatchWaterExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftCatchWaterHandle),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, LeftSideDitchExists),
                        new TypedValue((int)DxfCode.ExtendedDataHandle, LeftSideDitchHandle)
                        );
                    break;
                case InfoType.Right:
                    buff = new ResultBuffer
                   (
                   new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameRight),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightGroundSurfaceExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightGroundSurfaceHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightRoadSurfaceExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightRoadSurfaceHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightRoadCushionExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightRoadCushionHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightRetainingWallExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightRetainingWallHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightBoundaryExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightBoundaryHandle),
                   // 边坡相关
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightSlopeExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightSlopeHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightSlopeFill),
                   // 排水沟相关
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightDrainageDitchExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightDrainageDitchHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightCatchWaterExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightCatchWaterHandle),
                   new TypedValue((int)DxfCode.ExtendedDataInteger16, RightSideDitchExists),
                   new TypedValue((int)DxfCode.ExtendedDataHandle, RightSideDitchHandle)
                   );
                    break;
            }
            return buff;
        }

        public static ResultBuffer ClearValue(bool clearAll, InfoType type = InfoType.General)
        {
            ResultBuffer buff = null;
            if (clearAll)
            {
                buff =
              new ResultBuffer(
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameGeneral),
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameLeft),
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameRight));
                // 此时，Entity中的XData集合里，对应AppName下的所有数据，连同AppName这一项本身，都在实体中删除了。
                // 但是此AppName在 RegAppTable 中对应的 RegAppTableRecord 定义还是存在的。
            }
            else
            {
                switch (type)
                {
                    case InfoType.General: buff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameGeneral)); break;
                    case InfoType.Left: buff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameLeft)); break;
                    case InfoType.Right: buff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppNameRight)); break;
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
                //Mileage, "测量", SlopeLength, TopElevation - BottomElevation, left, fill, Style.ToString(),
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

        #endregion
    }
}