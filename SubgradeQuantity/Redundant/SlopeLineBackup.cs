using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;

namespace eZcad.SubgradeQuantity.Redundant
{
    /// <summary>
    /// 边坡线（通过自身的几何信息进行判断与构造而成）
    /// </summary>
    public class SlopeLineBackup
    {
        #region --- Fields

        /// <summary> 在AutoCAD界面中选择边坡线的过滤条件 </summary>
        public static TypedValue[] SlopeLineFilter = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Color, 2), // 黄色
                new TypedValue((int) DxfCode.Color, 3), // 绿色
                new TypedValue((int) DxfCode.Operator, "OR>"),
            };

        private readonly DocumentModifier _docMdf;
        private readonly Editor _ed;

        public Polyline Pline { get; private set; }

        public SubgradeSection AxisLine { get; private set; }

        /// <summary> 边坡所对应的几何线段，每一个线段都代表了一级边坡（不包括马道） </summary>
        private readonly LineSegment2d[] _slopeSegs;


        #endregion

        #region --- 构造函数

        /// <summary>
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="pline"></param>
        /// <returns>如果创建失败，则返回 null </returns>
        public static SlopeLineBackup Create(DocumentModifier docMdf, Polyline pline)
        {
            if (pline.Closed) return null;

            //
            var segs = FindSlopeSegs(pline);
            if (segs != null)
            {
                var ca = FindCenterAxisOnBounds(docMdf, pline);
                // var ca = FindCenterAxisOnConnection(docMdf, pline);
                if (ca != null)
                {
                    var slp = new SlopeLineBackup(docMdf, pline, segs, ca);
                    return slp;
                }
            }
            return null;
        }

        private SlopeLineBackup(DocumentModifier docMdf, Polyline pline, LineSegment2d[] slopeSegs, SubgradeSection centerAxis)
        {
            _docMdf = docMdf;
            _ed = docMdf.acEditor;
            //
            Pline = pline;
            AxisLine = centerAxis;
            _slopeSegs = slopeSegs;

            //
            CalculateXData(XData);
        }

        #endregion

        #region --- 边坡线的判断准则

        // 准则1：对于边坡线而言，不论其斜率如何，其走向都是一致的。比如一条边坡线是沿左上方走，则其所有的子线段中不可能出现向右或者向下走的线段

        /// <summary>
        /// 对多段线是否为边坡线进行判断，如果是边坡线，则返回每一级边坡所对应的线段
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        private static LineSegment2d[] FindSlopeSegs(Polyline pl)
        {
            var segs = new LineSegment2d[pl.NumberOfVertices - 1];
            for (int i = 0; i < pl.NumberOfVertices - 1; i++)
            {
                segs[i] = pl.GetLineSegment2dAt(i);
            }
            if (segs.Length == 0) return null;
            //
            var baseDir = FindBaseDirection(segs);  // 其可能的值为1，2，3，4，分别代表第一、二、三、四象限。如果没有找到，则返回0
            if (baseDir == 0) return null;
            //
            var slopeSegs = new List<LineSegment2d>();
            foreach (var seg in segs)
            {
                if (!HarmanyDirection(seg, baseDir)) return null;
                //
                if (IsSlope(seg))
                {
                    slopeSegs.Add(seg);
                }
            }
            if (slopeSegs.Count > 0)
            {
                return slopeSegs.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary> 朝着右上方的边坡的坡比 </summary>
        private static readonly Vector2d[] TopRightSlopes = new Vector2d[]
        {
            new Vector2d(0.3, 1),
            new Vector2d(0.5, 1),
            new Vector2d(0.75, 1),
            new Vector2d(1.5, 1),
            new Vector2d(1.75, 1),
            new Vector2d(2.0, 1),
            new Vector2d(2.5, 1),
        };

        /// <summary> 朝着左上方的边坡的坡比 </summary>
        private static readonly Vector2d[] TopLeftSlopes = new Vector2d[]
        {
            new Vector2d(-0.3, 1),
            new Vector2d(-0.5, 1),
            new Vector2d(-0.75, 1),
            new Vector2d(-1.5, 1),
            new Vector2d(-1.75, 1),
            new Vector2d(-2.0, 1),
            new Vector2d(-2.5, 1),
        };

        /// <summary> 搜索多段线的基准走向 </summary>
        /// <param name="segs"></param>
        /// <returns>其可能的值为1，2，3，4，分别代表第一、二、三、四象限。如果没有找到，则返回0</returns>
        private static int FindBaseDirection(LineSegment2d[] segs)
        {
            foreach (var seg in segs)
            {
                var dir = seg.Direction;
                if (dir.X > 0 && dir.Y > 0) return 1;
                if (dir.X < 0 && dir.Y > 0) return 2;
                if (dir.X < 0 && dir.Y < 0) return 3;
                if (dir.X > 0 && dir.Y < 0) return 4;
            }
            return 0;
        }

        /// <summary> 线段是否与指定的基准方向指向相同 </summary>
        /// <param name="l"></param>
        /// <param name="baseDir">其可能的值为1，2，3，4，分别代表第一、二、三、四象限</param>
        /// <returns></returns>
        private static bool HarmanyDirection(LineSegment2d l, int baseDir)
        {
            var dir = l.Direction;
            if (
                (baseDir == 1 && dir.X >= 0 && dir.Y >= 0) ||
                (baseDir == 2 && dir.X <= 0 && dir.Y >= 0) ||
                (baseDir == 3 && dir.X <= 0 && dir.Y <= 0) ||
                (baseDir == 4 && dir.X >= 0 && dir.Y <= 0)
                )
            {
                return true;
            }
            else if (l.Length <= Tolerance.Global.EqualPoint)
            {
                // 当线段的长度极小时，其对应的方向矢量可能为任意值
                // Tolerance.Global.EqualPoint 的值为 1E-10
                // 而 l.Length 的值可以为 0.000000000003637978807091713
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="topRight">如果边坡线指向第一或第三象限，则返回true；如果指向XY轴线方向，则返回null</param>
        /// <returns></returns>
        private static bool IsSlope(LineSegment2d l)
        {
            var dir = l.Direction;
            //
            if (
                dir.IsParallelTo(TopRightSlopes[0]) ||
                dir.IsParallelTo(TopRightSlopes[1]) ||
                dir.IsParallelTo(TopRightSlopes[2]) ||
                dir.IsParallelTo(TopRightSlopes[3]) ||
                dir.IsParallelTo(TopRightSlopes[4]) ||
                dir.IsParallelTo(TopRightSlopes[5]) ||
                dir.IsParallelTo(TopRightSlopes[6]))
            {
                return true;
            }
            else if (
                dir.IsParallelTo(TopLeftSlopes[0]) ||
                dir.IsParallelTo(TopLeftSlopes[1]) ||
                dir.IsParallelTo(TopLeftSlopes[2]) ||
                dir.IsParallelTo(TopLeftSlopes[3]) ||
                dir.IsParallelTo(TopLeftSlopes[4]) ||
                dir.IsParallelTo(TopLeftSlopes[5]) ||
                dir.IsParallelTo(TopLeftSlopes[6]))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region --- 根据边坡线搜索道路中心轴线

        /// <summary> 找到边坡线附近所属的中心轴线 </summary>
        /// <param name="pl"></param>
        /// <remarks>未找到则返回 null </remarks>
        private static SubgradeSection FindCenterAxisOnBounds(DocumentModifier docMdf, Polyline pl)
        {
            var filterCenterAxis = new[]
            {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_CenterAxis),
                new TypedValue((int) DxfCode.Color, "1"), // 红色
            };
            var pline = pl;
            var bdbl = pline.Bounds.Value.MinPoint;
            var bdtr = pline.Bounds.Value.MaxPoint;
            var res = docMdf.acEditor.SelectCrossingWindow(
                pt1: new Point3d(bdbl.X - ProtectionOptions.RoadWidth / 2, bdbl.Y - 0, bdbl.Z),
                pt2: new Point3d(bdtr.X + ProtectionOptions.RoadWidth / 2, bdtr.Y + 0, bdtr.Z),
                filter: new SelectionFilter(filterCenterAxis));

            if (res.Status == PromptStatus.OK)
            {
                var ids = res.Value.GetObjectIds();

                Line minLine = null;
                double minDis = double.MaxValue;
                // 可能找到多条轴线，比较轴线中点到边坡线两个端点距离最小的那个轴线
                foreach (var id in ids)
                {
                    var l = id.GetObject(OpenMode.ForRead) as Line;
                    var dir = l.StartPoint - l.EndPoint;
                    if (dir.IsParallelTo(new Vector3d(0, 1, 0)))
                    {
                        // 根据距离最近的原则来从众多的轴线中寻找匹配项
                        var centMid = l.StartPoint.Add((l.EndPoint - l.StartPoint).DivideBy(2)); // 轴线的中点

                        var d1 = centMid.DistanceTo(bdtr);
                        var d2 = centMid.DistanceTo(bdbl);
                        var minD = Math.Min(d1, d2);
                        if (minD < minDis)
                        {
                            minLine = l;
                            minDis = minD;
                        }
                    }
                }
                var centerLine = SubgradeSection.Create(docMdf, minLine);
                return centerLine;
            }
            else
            {
                docMdf.WriteLineIntoDebuger("在边坡线周围没有找到中心轴线" + "\r\n" + bdbl + "\r\n" + bdtr);
                return null;
            }
            return null;
        }

        /// <summary> 从边坡线找到其相连的道路路面线，再搜索到路面线相连的道路中心轴线 </summary>
        /// <param name="pl"></param>
        private static SubgradeSection FindCenterAxisOnConnection(DocumentModifier docMdf, Polyline pl)
        {
            // 先找到与边坡线相连的路面线
            var roadSurfFlt = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_RoadSurface),
                new TypedValue((int) DxfCode.Color, "2"), // 黄色
            };
            var pline = pl;
            var bdbl = pline.Bounds.Value.MinPoint;
            var bdtr = pline.Bounds.Value.MaxPoint;
            var res = docMdf.acEditor.SelectCrossingWindow(
                pt1: new Point3d(bdbl.X - 0.1, bdbl.Y - 0.1, bdbl.Z),
                pt2: new Point3d(bdtr.X + 0.1, bdtr.Y + 0.1, bdtr.Z),
                filter: new SelectionFilter(roadSurfFlt));

            if (res.Status == PromptStatus.OK)
            {
                var roadSurf = res.Value.GetObjectIds()[0].GetObject(OpenMode.ForRead) as Polyline;
                if (roadSurf == null)
                {
                    return null;
                }
                // 找到路面线，接下来搜索路面线旁边的道路中线
                var centerAxisFlt = new[]
                {
                    new TypedValue((int) DxfCode.Start, "LINE"),
                    new TypedValue((int) DxfCode.LayerName, "0"),
                    new TypedValue((int) DxfCode.Color, "1"), // 红色
                };
                bdbl = roadSurf.Bounds.Value.MinPoint;
                bdtr = roadSurf.Bounds.Value.MaxPoint;
                res = docMdf.acEditor.SelectCrossingWindow(
                    pt1: new Point3d(bdbl.X - 0.1, bdbl.Y, bdbl.Z),
                    pt2: new Point3d(bdtr.X + 0.1, bdtr.Y, bdtr.Z),
                    filter: new SelectionFilter(centerAxisFlt));

                if (res.Status == PromptStatus.OK)
                {
                    var l = res.Value.GetObjectIds()[0].GetObject(OpenMode.ForRead) as Line;
                    if (l != null)
                    {
                        var direc = l.StartPoint - l.EndPoint;
                        if (direc.IsParallelTo(new Vector3d(0, 1, 0)))
                        {
                            var centerLine = SubgradeSection.Create(docMdf, l);
                            return centerLine;
                        }
                    }
                }
            }
            else
            {
                docMdf.WriteLineIntoDebuger("在边坡线周围没有找到中心轴线" + "\r\n" + bdbl + "\r\n" + bdtr);
                return null;
            }
            return null;
        }

        #endregion

        #region --- Slope 线条几何信息

        private double GetSlopesLength()
        {
            var l = 0.0;
            foreach (var s in _slopeSegs)
            {
                l += s.Length;
            }
            return l;
        }

        /// <summary> 边坡线位于某一标高下的斜边长度 </summary>
        /// <param name="elev"></param>
        /// <returns></returns>
        private double GetSlopeLengthBelowElevation(double elev)
        {
            var yw = AxisLine.GetYFromElev(elev); // 水位标高所对应的几何Y坐标

            var l = 0.0;
            foreach (var s in _slopeSegs)
            {
                l += LineLengthBelowY(s, yw);
            }
            return l;
        }

        private static double LineLengthBelowY(LineSegment2d l, double waterY)
        {
            if (l.StartPoint.Y <= waterY && l.EndPoint.Y <= waterY)
            {
                return l.Length;
            }
            else if (l.StartPoint.Y >= waterY && l.EndPoint.Y >= waterY)
            {
                // 说明全部位于水上
                return 0;
            }
            else
            {
                // 说明与水位线相交
                var inters = l.IntersectWith(new Line2d(new Point2d(0, waterY), new Point2d(1, waterY)));
                if (inters != null && inters.Length > 0)
                {
                    var bottomPt = l.StartPoint.Y > l.EndPoint.Y ? l.EndPoint : l.StartPoint;
                    return bottomPt.GetDistanceTo(inters[0]);
                }
                else
                {
                    // 异常情况，不应该没找到交点
                    return l.Length / 2; // 随意取个值吧
                }
            }
        }

        private double GetTopElevation()
        {
            var maxY = double.MinValue;
            foreach (var s in _slopeSegs)
            {
                if (s.StartPoint.Y > maxY)
                {
                    maxY = s.StartPoint.Y;
                }
                if (s.EndPoint.Y > maxY)
                {
                    maxY = s.EndPoint.Y;
                }
            }
            return AxisLine.GetEleFromY(maxY);
        }

        private double GetBottomElevation()
        {
            var minY = double.MaxValue;
            foreach (var s in _slopeSegs)
            {
                if (s.StartPoint.Y < minY)
                {
                    minY = s.StartPoint.Y;
                }
                if (s.EndPoint.Y < minY)
                {
                    minY = s.EndPoint.Y;
                }
            }
            return AxisLine.GetEleFromY(minY);
        }

        #endregion

        /// <summary>
        /// 搜索边坡线周围的挡墙的顶部标高
        /// </summary>
        /// <returns></returns>
        private Polyline FindWallTop()
        {
            var filterWall = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.LayerName, "1"),
                new TypedValue((int) DxfCode.Color, "2"), // 黄色
            };
            var filter = new SelectionFilter(filterWall);
            var bdbl = Pline.Bounds.Value.MinPoint;
            var bdtr = Pline.Bounds.Value.MaxPoint;
            var res = _ed.SelectCrossingWindow(
                pt1: bdbl,
                pt2: bdtr,
                filter: new SelectionFilter(filterWall));
            if (res.Status == PromptStatus.OK)
            {
                var pl = res.Value.GetObjectIds()[0].GetObject(OpenMode.ForRead) as Polyline;
                if (pl != null && pl.Closed) // 找到了挡墙对象
                {
                    // 得到挡墙的最高点位置
                    return pl;
                }
            }
            return null;
        }

        #region --- XData


        private SlopeDataBackup _xData;

        public SlopeDataBackup XData
        {
            get
            {
                if (_xData == null)
                {
                    // 从 AutoCAD 中读取
                    var xd = Pline.GetXDataForApplication(SlopeDataBackup.AppName);
                    if (xd != null)
                    {
                        try
                        {
                            _xData = SlopeDataBackup.FromResultBuffer(xd);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("读取图元中的信息失败，现将其设置为默认值");
                            Pline.UpgradeOpen();
                            var bf = SlopeDataBackup.ClearValue();
                            Pline.XData = bf;
                            Pline.DowngradeOpen();
                        }
                    }
                }
                // 设置为默认值
                return _xData ?? (_xData = new SlopeDataBackup());
            }
            private set { _xData = value; }
        }

        public bool XDataToBeCleared { get; set; }

        /// <summary>
        /// 从边坡线中获取基本的几何信息
        /// </summary>
        /// <param name="xdata"></param>
        private void CalculateXData(SlopeDataBackup xdata)
        {
            xdata.CenterElevation = AxisLine.XData.CenterElevation_Road;
            xdata.TopElevation = GetTopElevation();
            xdata.BottomElevation = GetBottomElevation();
            xdata.NaturalSurfElevation = AxisLine.XData.CenterElevation_Ground;
            //
            xdata.SlopeLength = GetSlopesLength();
            xdata.SlopeLengthBelowWaterLevel = GetSlopeLengthBelowElevation(ProtectionOptions.WaterLevel);
            xdata.SlopeLengthBeThinFillTop = GetSlopeLengthBelowElevation(ProtectionOptions.FillUpperEdge);
            //
            var centerP = Pline.GetPointAtParameter((Pline.StartParam + Pline.EndParam) / 2);
            xdata.OnLeft = centerP.X < AxisLine.CenterLine.StartPoint.X;

            //
            xdata.Station = AxisLine.XData.Station;
            //
            xdata.CenterAxisHandle = AxisLine.CenterLine.Handle;
            xdata.InfoBlockHandle = AxisLine.XData.InfoBlockHandle;
        }
        
        public void FlushXData()
        {
            if (XData != null)
            {
                // XData.StationStr = CenterLine.Station;
                //
                Pline.XData = XData.ToResultBuffer();
            }
        }

        public void ClearXData()
        {
            Pline.XData = SlopeDataBackup.ClearValue();
        }

        public string DataInfo
        {
            get
            {
                var info = XData.Station.ToString("0.000") + ",\t"
                    + (XData.OnLeft ? "左侧" : "右侧") + ",\t"
                    + XData.SlopeLength.ToString("0.000")
                    ;
                return info;
            }
        }

        #endregion
    }
}