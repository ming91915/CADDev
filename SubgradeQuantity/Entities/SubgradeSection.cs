using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 路基横断面 </summary>
    public class SubgradeSection
    {
        #region --- Fields

        public readonly DocumentModifier DocMdf;

        private readonly Handle _infoBlock;

        private readonly double _station;

        /// <summary> 道路中心线 </summary>
        public Line CenterLine { get; private set; }

        /// <summary> 中心轴线在AutoCAD中的几何X坐标 </summary>
        private readonly double _centerX;

        /// <summary> 中心轴线在AutoCAD中的顶部点 </summary>
        private readonly Point3d _topPt;
        /// <summary> 中心轴线在AutoCAD中的底部点 </summary>
        private readonly Point3d _bottomPt;
        /// <summary> 中心轴线在AutoCAD中的中间点 </summary>
        private readonly Point3d _MiddlePt;

        /// <summary> 道路中心线对应的路面标高 </summary>
        private readonly double _centerEle;
        /// <summary> 道路中心线对应的路面标高位置在AutoCAD中的Y坐标 </summary>
        private readonly double _centerY;


        private static readonly SelectionFilter _filter = new SelectionFilter(new[]{
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_CenterAxis)
                });
        /// <summary> 从 AutoCAD 界面中过滤出道路中线对象的过滤规则 </summary>
        public static SelectionFilter Filter
        {
            get
            {
                return _filter;
            }
        }

        #endregion

        #region --- XData

        private SectionInfo _xData;

        public SectionInfo XData
        {
            get
            {
                if (_xData != null) return _xData;
                //
                try
                {
                    _xData = SectionInfo.FromCenterLine(CenterLine);
                }
                catch (Exception)
                {
                    MessageBox.Show(@"读取图元中的信息失败，现将其设置为默认值");
                    CenterLine.UpgradeOpen();
                    var bf = SectionInfo.ClearValue(clearAll: true);
                    CenterLine.XData = bf;
                    CenterLine.DowngradeOpen();
                    _xData = null;
                }
                // 如果构造失败，则设置为默认值
                return _xData ?? (_xData = new SectionInfo() { FullyCalculated = false });
            }
            private set { _xData = value; }
        }

        public bool XDataToBeCleared { get; set; }

        public void FlushXData()
        {
            if (XData != null)
            {
                // XData.StationStr = CenterLine.Station;
                //
                CenterLine.XData = XData.ToResultBuffer(SectionInfo.InfoType.General);
                CenterLine.XData = XData.ToResultBuffer(SectionInfo.InfoType.Left);
                CenterLine.XData = XData.ToResultBuffer(SectionInfo.InfoType.Right);
            }
        }

        /// <summary> 清除 XData 中的数据 </summary>
        /// <param name="clearAll">如果其值为 true，则<paramref name="type"/>值不起作用</param>
        /// <param name="type"></param>
        public void ClearXData(bool clearAll, SectionInfo.InfoType type = SectionInfo.InfoType.General)
        {
            CenterLine.XData = SectionInfo.ClearValue(clearAll, type);
            XData = null;
        }

        #endregion

        #region --- 构造函数

        /// <summary>
        /// 构造方式一：从零开始判断与计算（不能成功构造，则返回 null）
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="centerLine"></param>
        /// <returns></returns>
        public static SubgradeSection Create(DocumentModifier docMdf, Line centerLine)
        {
            //
            var infoBlock = FindInfoBlock(docMdf.acEditor, centerLine);
            if (infoBlock != null)
            {
                var att = infoBlock.GetAttributeReference(Options_General.StationFieldDef);
                string station = att?.TextString;

                if (station != null)
                {
                    var slp = new SubgradeSection(docMdf, centerLine, infoBlock, station);
                    return slp;
                }
            }
            return null;
        }

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="line"></param>
        /// <param name="infoBlock"></param>
        /// <param name="station"></param>
        private SubgradeSection(DocumentModifier docMdf, Line line, BlockReference infoBlock, string station) : this(line)
        {
            DocMdf = docMdf;
            CenterLine = line;
            _infoBlock = infoBlock.Handle;
            _station = ProtectionUtils.GetStationFromString(station).Value;
            //
            var blr = FindCenterAxisElevation(docMdf.acEditor);
            if (blr != null)
            {
                _centerY = blr.Position.Y;
                _centerEle =
                    double.Parse(
                        (blr.AttributeCollection[0].GetObject(OpenMode.ForRead) as AttributeReference).TextString);
            }
        }

        /// <summary>
        /// 构造方式二：对于已经计算过的横断面，可以直接构造并使用计算后存储的数据直接赋值
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="centerLine"></param>
        /// <param name="sectionInfo"></param>
        public SubgradeSection(DocumentModifier docMdf, Line centerLine, SectionInfo sectionInfo) : this(centerLine)
        {
            //
            DocMdf = docMdf;
            CenterLine = centerLine;
            XData = sectionInfo;
            //
            _infoBlock = sectionInfo.InfoBlockHandle;
            _station = XData.Station;
            //
            _centerX = sectionInfo.CenterX;
            _centerY = sectionInfo.CenterY;
            _centerEle = sectionInfo.CenterElevation_Road;
        }

        /// <summary> 不是构造函数，只是为了解决对只读属性的赋值 </summary>
        /// <param name="line"></param>
        private SubgradeSection(Line line)
        {
            _centerX = line.StartPoint.X;
            if (line.StartPoint.Y < line.EndPoint.Y)
            {
                _topPt = line.EndPoint;
                _bottomPt = line.StartPoint;
            }
            else
            {
                _topPt = line.StartPoint;
                _bottomPt = line.EndPoint;
            }
            _MiddlePt = new Point3d(_centerX, (_topPt.Y + _bottomPt.Y) / 2, 0);
        }

        #endregion

        #region --- CalculateSectionInfoToXData

        /// <summary> !!! 对横断面中的关键对象进行搜索与计算 </summary>
        public void CalculateSectionInfoToXData()
        {
            Extents3d extSection; // 整个横断面的搜索范围
            // 道路中轴线的搜索范围
            Extents3d extCenterAxis = new Extents3d(
                min: new Point3d(_centerX - ProtectionConstants.CoincideTolerance,
                        _bottomPt.Y - ProtectionConstants.CoincideTolerance, 0),
                max: new Point3d(_centerX + ProtectionConstants.CoincideTolerance,
                        _topPt.Y + ProtectionConstants.CoincideTolerance, 0));

            Polyline leftGroundSurf;
            Polyline rightGroundSurf;
            double centerGroundY;  // 道路中线所对应的自然地面标高
            var succ = FindGroundSurf(extCenterAxis, out leftGroundSurf, out rightGroundSurf, out extSection, out centerGroundY);
            if (!succ) return;
            //
            var xdata = XData;

            // 通用
            xdata.Station = _station;
            xdata.CenterElevation_Road = _centerEle;
            xdata.CenterX = _centerX;
            xdata.CenterY = _centerY;
            xdata.CenterElevation_Ground = GetEleFromY(centerGroundY);
            xdata.InfoBlockHandle = _infoBlock;

            // 自然地表
            xdata.LeftGroundSurfaceExists = true;
            xdata.LeftGroundSurfaceHandle = leftGroundSurf.Handle;
            xdata.RightGroundSurfaceExists = true;
            xdata.RightGroundSurfaceHandle = rightGroundSurf.Handle;

            // 路面
            Polyline leftRoadSurf;
            Polyline rightRoadSurf;
            succ = FindRoadSurf(extCenterAxis, out leftRoadSurf, out rightRoadSurf);
            if (!succ) return;  // 必须有路面对象
            xdata.LeftRoadSurfaceExists = true;
            xdata.LeftRoadSurfaceHandle = leftRoadSurf.Handle;
            xdata.LeftRoadEdge = leftRoadSurf.EndPoint;
            xdata.RightRoadSurfaceExists = true;
            xdata.RightRoadSurfaceHandle = rightRoadSurf.Handle;
            xdata.RightRoadEdge = rightRoadSurf.EndPoint;

            // 路槽
            Polyline leftRoadCushion;
            Polyline rightRoadCushion;
            Point3d cushionBottom; //  道路中心线所对应的自然地面标高;
            succ = FindRoadCushion(extCenterAxis, out leftRoadCushion, out rightRoadCushion, out cushionBottom);
            if (succ) // 路槽可以没有
            {
                xdata.CenterElevation_Cushion = GetEleFromY(cushionBottom.Y);
                //
                if (leftRoadCushion != null)
                {
                    xdata.LeftRoadCushionExists = true;
                    xdata.LeftRoadCushionHandle = leftRoadCushion.Handle;
                    //
                }
                if (rightRoadCushion != null)
                {
                    xdata.RightRoadCushionExists = true;
                    xdata.RightRoadCushionHandle = rightRoadCushion.Handle;
                }
            }

            // 自然地面或清表线以下的 台阶
            IList<Polyline> stairs;
            succ = FindStairs(extSection, leftGroundSurf, rightGroundSurf, out stairs);
            if (succ && stairs != null && stairs.Count > 0) // 对于一个横断面而言，可以没有台阶
            {
                xdata.StairExists = true;
                xdata.StairHandles = stairs.Select(r => r.Handle).ToArray();
            }

            // 用地界
            Polyline leftBoundary;
            Polyline rightBoundary;
            succ = FindBoundary(extSection, out leftBoundary, out rightBoundary);
            if (succ)
            {
                if (leftBoundary != null)
                {
                    xdata.LeftBoundaryExists = true;
                    xdata.LeftBoundaryHandle = leftBoundary.Handle;
                }
                if (rightBoundary != null)
                {
                    xdata.RightBoundaryExists = true;
                    xdata.RightBoundaryHandle = rightBoundary.Handle;
                }
            }


            // 边坡
            Polyline leftSlope;
            bool leftFill;
            Polyline rightSlope;
            bool rightFill;
            succ = FindSlopes(extSection, out leftSlope, out leftFill, out rightSlope, out rightFill);
            if (succ) // 对于一个横断面而言，可以没有边坡线，即不进行挖填
            {
                if (leftSlope != null)
                {
                    xdata.LeftSlopeExists = true;
                    xdata.LeftSlopeHandle = leftSlope.Handle;
                    xdata.LeftSlopeFill = leftFill;
                }
                if (rightSlope != null)
                {
                    xdata.RightSlopeExists = true;
                    xdata.RightSlopeHandle = rightSlope.Handle;
                    xdata.RightSlopeFill = rightFill;
                }
            }

            // 挡土墙
            Polyline leftRetainingWall;
            Polyline rightRetainingWall;
            succ = FindRetainingWall(extSection, out leftRetainingWall, out rightRetainingWall);
            if (succ)  // 对于一个横断面而言，可以没有挡墙
            {
                if (leftRetainingWall != null)
                {
                    xdata.LeftRetainingWallExists = true;
                    xdata.LeftRetainingWallHandle = leftRetainingWall.Handle;
                }
                if (rightRetainingWall != null)
                {
                    xdata.RightRetainingWallExists = true;
                    xdata.RightRetainingWallHandle = rightRetainingWall.Handle;
                }
            }

            // 边沟、排水沟、截水沟 
            Polyline leftDrainageDitch; Polyline rightDrainageDitch;
            Polyline leftCatchWater; Polyline rightCatchWater;
            Polyline leftSideDitch; Polyline rightSideDitch;
            succ = FindDitches(extSection, out leftDrainageDitch, out rightDrainageDitch, out leftCatchWater, out rightCatchWater, out leftSideDitch, out rightSideDitch);
            if (succ)
            {
                if (leftDrainageDitch != null)
                {
                    xdata.LeftDrainageDitchExists = true;
                    xdata.LeftDrainageDitchHandle = leftDrainageDitch.Handle;
                }
                if (rightDrainageDitch != null)
                {
                    xdata.RightDrainageDitchExists = true;
                    xdata.RightDrainageDitchHandle = rightDrainageDitch.Handle;
                }
                if (leftCatchWater != null)
                {
                    xdata.LeftCatchWaterExists = true;
                    xdata.LeftCatchWaterHandle = leftCatchWater.Handle;
                }
                if (rightCatchWater != null)
                {
                    xdata.RightCatchWaterExists = true;
                    xdata.RightCatchWaterHandle = rightCatchWater.Handle;
                }
                if (leftSideDitch != null)
                {
                    xdata.LeftSideDitchExists = true;
                    xdata.LeftSideDitchHandle = leftSideDitch.Handle;
                }
                if (rightSideDitch != null)
                {
                    xdata.RightSideDitchExists = true;
                    xdata.RightSideDitchHandle = rightSideDitch.Handle;
                }
            }
            //
            xdata.FullyCalculated = true;
        }

        #region --- 在界面中搜索并过滤相关对象

        /// <summary> 搜索记录横断面信息的块参照 </summary>
        private static BlockReference FindInfoBlock(Editor ed, Line l)
        {
            var filterWall = new[]
            {
                new TypedValue((int) DxfCode.Start, "INSERT"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_SectionInfo),
                // new TypedValue((int) DxfCode.BlockName, ProtectionOptions.BlockName_SectionInfo),
                // 黄色 HDMD.HDM.SHUJ..横断面绘图模板 - 3点带横坡  "HDMD.HDM.SHUJ..横断面绘图模板-带标高"
            };
            var bottomP = l.EndPoint;
            var pt1 = new Point3d(bottomP.X - 5, bottomP.Y - 5, bottomP.Z);
            var pt2 = new Point3d(bottomP.X + 5, bottomP.Y, bottomP.Z);

            var res = ed.SelectCrossingWindow(
                pt1: pt1,
                pt2: pt2,
                filter: new SelectionFilter(filterWall)); // 确保想要被选中的图元的一部分出现在屏幕中
            if (res.Status == PromptStatus.OK)
            {
                // 可能会选中多个块参照
                var ids = res.Value.GetObjectIds();

                BlockReference closestBlk = null;
                double minDis = double.MaxValue;
                var lineBottom = l.StartPoint.Y < l.EndPoint.Y ? l.StartPoint : l.EndPoint;
                // 可能找到多条轴线，比较轴线中点到边坡线两个端点距离最小的那个轴线
                foreach (var id in ids)
                {
                    var blk = id.GetObject(OpenMode.ForRead) as BlockReference;
                    var blkTop = blk.Position;

                    var dist = lineBottom.DistanceTo(blkTop);

                    if (dist < minDis)
                    {
                        closestBlk = blk;
                        minDis = dist;
                    }
                }
                return closestBlk;
            }
            return null;
        }

        /// <summary>
        /// 道路路面中心的标高
        /// </summary>
        private BlockReference FindCenterAxisElevation(Editor ed)
        {
            var filterWall = new[]
            {
                new TypedValue((int) DxfCode.Start, "INSERT"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_CenterElevation),
                // new TypedValue((int) DxfCode.BlockName, ProtectionOptions.BlockName_CenterElevation),
                // 黄色  "HDMD.ITEM.ZLJ.ZFG..横断面绘图模板-带标高"
            };

            var pt1 = new Point3d(_bottomPt.X - ProtectionConstants.CoincideTolerance, _bottomPt.Y, _bottomPt.Z);
            var pt2 = new Point3d(_topPt.X + ProtectionConstants.CoincideTolerance, _topPt.Y, _topPt.Z);

            var res = ed.SelectCrossingWindow(
                pt1: pt1,
                pt2: pt2,
                filter: new SelectionFilter(filterWall));
            if (res.Status == PromptStatus.OK)
            {
                // 由于界面显示精度，当界面缩放得过小时，可能会搜索到多个对象
                var blrs = res.Value.GetObjectIds();
                // Y 值位于轴线Y值范围之内，而且X值距离轴线最近
                BlockReference closestBlk = null;
                double minDis = double.MaxValue;
                foreach (var id in blrs)
                {
                    var blr = id.GetObject(OpenMode.ForRead) as BlockReference;
                    var p = blr.Position;
                    if (p.Y < _topPt.Y && p.Y > _bottomPt.Y) // 位于轴线的Y值范围之内
                    {
                        var dis = Math.Abs(p.X - _centerX);
                        if (dis < minDis)
                        {
                            minDis = dis;
                            closestBlk = blr;
                        }
                    }
                }
                return closestBlk;
            }
            return null;
        }

        /// <summary> 搜索自然地面对象，以及整个断面所对应的图形区域 </summary>
        /// <param name="centerGroundEle">自然地面对象与道路中线的交点所对应的标高值</param>
        private bool FindGroundSurf(Extents3d extCenterAxis, out Polyline leftGroundSurf, out Polyline rightGroundSurf, out Extents3d ext, out double centerGroundEle)
        {
            centerGroundEle = double.MaxValue;
            leftGroundSurf = null;
            rightGroundSurf = null;
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_GroundSurface_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_GroundSurface_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 请求在图形区域选择对象
            var res = DocMdf.acEditor.SelectCrossingWindow(
                pt1: extCenterAxis.MinPoint, pt2: extCenterAxis.MaxPoint,
                filter: new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();

                var lefts = lines.Where(r => r.Layer == Options_LayerNames.LayerName_GroundSurface_Left);
                Point3d inters;
                foreach (var l in lefts)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        centerGroundEle = inters.Y;
                        leftGroundSurf = l;
                        break;
                    }
                }

                var rights = lines.Where(r => r.Layer == Options_LayerNames.LayerName_GroundSurface_Right);
                foreach (var l in rights)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        centerGroundEle = inters.Y;
                        rightGroundSurf = l;
                        break;
                    }
                }
                // 构造 整个横断面的 几何范围
                if (leftGroundSurf != null && rightGroundSurf != null)
                {
                    var ys = new double[]
                    {
                        leftGroundSurf.StartPoint.Y, leftGroundSurf.EndPoint.Y,
                        rightGroundSurf.StartPoint.Y, rightGroundSurf.EndPoint.Y,
                        _topPt.Y, _bottomPt.Y,
                    };
                    var xs = new double[]
                    {
                        leftGroundSurf.StartPoint.X, leftGroundSurf.EndPoint.X,
                        rightGroundSurf.StartPoint.X, rightGroundSurf.EndPoint.X,
                        _topPt.X, _bottomPt.X,
                    };
                    ext = new Extents3d(new Point3d(xs.Min(), ys.Min(), 0), new Point3d(xs.Max(), ys.Max(), 0));
                    return true;
                }
            }
            leftGroundSurf = null;
            rightGroundSurf = null;
            ext = new Extents3d();
            return false;
        }

        /// <summary> 在横断面构造完成后，求解整个横断面（包括其中的各种元素）在界面中所占据的几何空间 </summary>
        /// <returns></returns>
        public Extents3d GetExtends()
        {
            var xdata = XData;
            if (xdata.LeftGroundSurfaceExists && xdata.RightGroundSurfaceExists)
            {
                var pl = xdata.LeftGroundSurfaceHandle.GetDBObject<Polyline>(DocMdf.acDataBase);
                var minPt = pl.GetLineSegmentAt(pl.NumberOfVertices - 2).EndPoint;
                pl = xdata.RightGroundSurfaceHandle.GetDBObject<Polyline>(DocMdf.acDataBase);
                var maxPt = pl.GetLineSegmentAt(pl.NumberOfVertices - 2).EndPoint;
                return new Extents3d(
                    new Point3d(Math.Min(minPt.X, maxPt.X), Math.Min(minPt.Y, maxPt.Y), Math.Min(minPt.Z, maxPt.Z)),
                    new Point3d(Math.Max(minPt.X, maxPt.X), Math.Max(minPt.Y, maxPt.Y), Math.Max(minPt.Z, maxPt.Z)));
            }
            // 出错的情况下随便给一个默认值
            return new Extents3d(new Point3d(-1, -1, 0), new Point3d(1, 1, 0));
        }

        /// <summary> 搜索路面对象 </summary>
        private bool FindRoadSurf(Extents3d extCenterAxis, out Polyline leftRoadSurf, out Polyline rightRoadSurf)
        {
            leftRoadSurf = null;
            rightRoadSurf = null;
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RoadSurface_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RoadSurface_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 请求在图形区域选择对象
            var res = DocMdf.acEditor.SelectCrossingWindow(pt1: extCenterAxis.MinPoint, pt2: extCenterAxis.MaxPoint,
                filter: new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();

                var lefts = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RoadSurface_Left);
                Point3d inters;
                foreach (var l in lefts)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        leftRoadSurf = l;
                        break;
                    }
                }

                var rights = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RoadSurface_Right);
                foreach (var l in rights)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        rightRoadSurf = l;
                        break;
                    }
                }
                if (leftRoadSurf != null && rightRoadSurf != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary> 搜索路槽对象 </summary>
        /// <param name="cushionBottom">道路中心线所对应的自然地面标高</param>
        private bool FindRoadCushion(Extents3d extCenterAxis, out Polyline leftRoadCushion, out Polyline rightRoadCushion,
            out Point3d cushionBottom)
        {
            leftRoadCushion = null;
            rightRoadCushion = null;

            // 随便指定一个位置
            cushionBottom = new Point3d(double.MinValue, double.MinValue, double.MinValue);

            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RoadCushion_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RoadCushion_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 请求在图形区域选择对象
            var res = DocMdf.acEditor.SelectCrossingWindow(pt1: extCenterAxis.MinPoint, pt2: extCenterAxis.MaxPoint,
                filter: new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();

                var lefts = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RoadCushion_Left);
                Point3d inters;
                foreach (var l in lefts)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        cushionBottom = inters;
                        leftRoadCushion = l;
                        break;
                    }
                }

                var rights = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RoadCushion_Right);
                foreach (var l in rights)
                {
                    if (IntersectCenterLine(l.StartPoint, l.EndPoint, out inters))
                    {
                        cushionBottom = inters;
                        rightRoadCushion = l;
                        break;
                    }
                }
                if (leftRoadCushion != null && rightRoadCushion != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary> 搜索用地界对象 </summary>
        private bool FindBoundary(Extents3d extCenterAxis, out Polyline leftBoundary, out Polyline rightBoundary)
        {
            leftBoundary = null;
            rightBoundary = null;
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Boundary_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Boundary_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 请求在图形区域选择对象
            var res = DocMdf.acEditor.SelectCrossingWindow(pt1: extCenterAxis.MinPoint, pt2: extCenterAxis.MaxPoint,
                filter: new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();

                // 从多个挡墙线中搜索某个端点距离中轴线中心最近的那一条
                var lefts = lines.Where(r => r.Layer == Options_LayerNames.LayerName_Boundary_Left);
                var slp = GetClosestPolyLine(lefts, _MiddlePt);
                if (slp != null)
                {
                    leftBoundary = slp as Polyline;
                }
                //
                var rights = lines.Where(r => r.Layer == Options_LayerNames.LayerName_Boundary_Right);
                slp = GetClosestPolyLine(rights, _MiddlePt);
                if (slp != null)
                {
                    rightBoundary = slp as Polyline;
                }
                return true;
            }
            return false;
        }

        /// <summary> 搜索自然地面或者清表线之下的台阶 </summary>
        private bool FindStairs(Extents3d extSection, Polyline leftGroundSurf, Polyline rightGroundSurf, out IList<Polyline> stairs)
        {
            stairs = new List<Polyline>();
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Stairs),
            };

            var res = DocMdf.acEditor.SelectCrossingWindow(
                pt1: extSection.MinPoint,
                pt2: extSection.MaxPoint,
                filter: new SelectionFilter(filterType));

            if (res.Status == PromptStatus.OK)
            {
                var lines = res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();
                // 台阶位于 自然地面或者清表线之下，而且距离不会太远， 一般为0.25m，判断时取最大间距为1.0m
                var distTol = 1.0;
                double dist;
                foreach (var l in lines)
                {
                    var searchPt = l.StartPoint;
                    bool found = false;
                    if (!found)
                    {
                        dist = leftGroundSurf.GetClosestPointTo(searchPt, extend: false).DistanceTo(searchPt);
                        if (dist < distTol)
                        {
                            found = true;
                            stairs.Add(l);
                        }
                    }
                    if (!found)
                    {
                        dist = rightGroundSurf.GetClosestPointTo(searchPt, extend: false).DistanceTo(searchPt);
                        if (dist < distTol)
                        {
                            found = true;
                            stairs.Add(l);
                        }
                    }
                    if (!found)
                    {
                        searchPt = l.EndPoint;
                        dist = leftGroundSurf.GetClosestPointTo(searchPt, extend: false).DistanceTo(searchPt);
                        if (dist < distTol)
                        {
                            found = true;
                            stairs.Add(l);
                        }
                    }
                    if (!found)
                    {
                        dist = rightGroundSurf.GetClosestPointTo(searchPt, extend: false).DistanceTo(searchPt);
                        if (dist < distTol)
                        {
                            found = true;
                            stairs.Add(l);
                        }
                    }
                }
            }
            // 对于一个横断面而言，可以没有边坡线，即不进行挖填
            return true;
        }

        /// <summary> 搜索挡土墙对象 </summary>
        private bool FindRetainingWall(Extents3d extSection, out Polyline leftRetainingWall, out Polyline rightRetainingWall)
        {
            leftRetainingWall = null;
            rightRetainingWall = null;
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RetainingWall_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RetainingWall_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 请求在图形区域选择对象
            var res = DocMdf.acEditor.SelectCrossingWindow(pt1: extSection.MinPoint, pt2: extSection.MaxPoint,
                filter: new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().Where(r => r.Closed).ToList();

                // 从多个挡墙线中搜索某个端点距离中轴线中心最近的那一条
                var lefts = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RetainingWall_Left);
                var slp = GetClosestPolyLine(lefts, _MiddlePt);
                if (slp != null)
                {
                    leftRetainingWall = slp as Polyline;
                }
                //
                var rights = lines.Where(r => r.Layer == Options_LayerNames.LayerName_RetainingWall_Right);
                slp = GetClosestPolyLine(rights, _MiddlePt);
                if (slp != null)
                {
                    rightRetainingWall = slp as Polyline;
                }
            }
            return true;
        }

        /// <summary> 搜索边坡线 </summary>
        /// <param name="leftFill">左边边坡为填方还是挖方</param>
        /// <param name="rightFill">右边边坡为填方还是挖方</param>
        private bool FindSlopes(Extents3d extSection, out Polyline leftSlope, out bool leftFill, out Polyline rightSlope, out bool rightFill)
        {
            leftSlope = null;
            rightSlope = null;
            leftFill = false;
            rightFill = false;

            var res = DocMdf.acEditor.SelectCrossingWindow(
                pt1: extSection.MinPoint,
                pt2: extSection.MaxPoint,
                filter: SlopeLine.Filter);

            if (res.Status == PromptStatus.OK)
            {
                var lines = res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();
                var lefts =
                    lines.Where(l => SlopeLine.IsSlopeLineLayer(l.Layer, left: true)).ToArray();
                if (lefts.Length > 0)
                {
                    // 从多个边坡线中搜索某个端点距离中轴线中心最近的那一条
                    var slp = GetClosestPolyLine(lefts, _MiddlePt);
                    if (slp != null)
                    {
                        leftSlope = slp as Polyline;
                        leftFill = slp.Layer == Options_LayerNames.LayerName_Slope_Left_Fill;
                    }
                }
                var rights =
                 lines.Where(l => SlopeLine.IsSlopeLineLayer(l.Layer, left: false)).ToArray();
                if (rights.Length > 0)
                {
                    // 从多个边坡线中搜索某个端点距离中轴线中心最近的那一条
                    var slp = GetClosestPolyLine(rights, _MiddlePt);
                    if (slp != null)
                    {
                        rightSlope = slp as Polyline;
                        rightFill = slp.Layer == Options_LayerNames.LayerName_Slope_Right_Fill;
                    }
                }
            }
            // 对于一个横断面而言，可以没有边坡线，即不进行挖填
            return true;
        }

        /// <summary>
        /// 搜索边沟、排水沟、截水沟 
        /// </summary>
        /// <param name="extSection"></param>
        /// <param name="leftDrainageDitch">排水沟（填方坡底）</param>
        /// <param name="rightDrainageDitch">排水沟（填方坡底）</param>
        /// <param name="leftCatchWater">截水沟（挖方坡顶）</param>
        /// <param name="rightCatchWater">截水沟（挖方坡顶）</param>
        /// <param name="leftSideDitch">边沟（挖方坡底）</param>
        /// <param name="rightSideDitch">边沟（挖方坡底）</param>
        /// <returns></returns>
        private bool FindDitches(Extents3d extSection, out Polyline leftDrainageDitch, out Polyline rightDrainageDitch,
           out Polyline leftCatchWater, out Polyline rightCatchWater,
           out Polyline leftSideDitch, out Polyline rightSideDitch)
        {
            leftDrainageDitch = null;
            rightDrainageDitch = null;
            leftCatchWater = null;
            rightCatchWater = null;
            leftSideDitch = null;
            rightSideDitch = null;

            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_DrainageDitch_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_DrainageDitch_Right),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_CatchWater_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_CatchWater_Right),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_SideDitch_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_SideDitch_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            var res = DocMdf.acEditor.SelectCrossingWindow(
                    pt1: extSection.MinPoint,
                    pt2: extSection.MaxPoint,
                    filter: new SelectionFilter(filterType));

            if (res.Status == PromptStatus.OK)
            {
                var lines = res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToList();
                leftDrainageDitch = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_DrainageDitch_Left).ToArray()) as Polyline;
                rightDrainageDitch = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_DrainageDitch_Right).ToArray()) as Polyline;
                leftCatchWater = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_CatchWater_Left).ToArray()) as Polyline;
                rightCatchWater = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_CatchWater_Right).ToArray()) as Polyline;
                leftSideDitch = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_SideDitch_Left).ToArray()) as Polyline;
                rightSideDitch = GetDitch(lines.Where(l => l.Layer == Options_LayerNames.LayerName_SideDitch_Right).ToArray()) as Polyline;
            }
            return true;
        }

        // ---------------------------------------------------------------------------------------
        /// <summary> 从曲线集合中匹配沟渠对象 </summary>
        private Curve GetDitch(IList<Polyline> curves)
        {
            if (curves == null || curves.Count == 0)
            {
                return null;
            }
            if (curves.Count == 1)
            {
                return curves[0];
            }
            else
            {
                // 集合中的数量大于1
                // 从多个边坡线中搜索某个端点距离中轴线中心最近的那一条
                var slp = GetClosestPolyLine(curves, _MiddlePt);
                if (slp != null)
                {
                    return slp;
                }
            }
            return null;
        }


        /// <summary> 与中轴线相交的对象判断 </summary>
        /// <param name="startP"></param>
        /// <param name="endP"></param>
        /// <param name="intersectP">如果相交，则为对应的交点</param>
        /// <returns></returns>
        private bool IntersectCenterLine(Point3d startP, Point3d endP, out Point3d intersectP)
        {
            var x = startP.X;
            var y = startP.Y;
            if (y <= _topPt.Y && y >= _bottomPt.Y && Math.Abs(x - _centerX) < ProtectionConstants.CoincideTolerance)
            {
                intersectP = startP;
                return true;
            }
            x = endP.X;
            y = endP.Y;
            if (y <= _topPt.Y && y >= _bottomPt.Y && Math.Abs(x - _centerX) < ProtectionConstants.CoincideTolerance)
            {
                intersectP = endP;
                return true;
            }
            intersectP = new Point3d();
            return false;
        }

        /// <summary>
        /// 从多个多段线对象中搜索某个端点距离中轴线中心最近的那一条多段线
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        private static Curve GetClosestPolyLine(IEnumerable<Curve> curves, Point3d axisMiddlePt)
        {
            Curve closetSlope = null;
            var minDis = double.MaxValue;
            double dist;
            foreach (var sp in curves)
            {
                dist = sp.StartPoint.DistanceTo(axisMiddlePt);
                if (dist <= minDis)
                {
                    minDis = dist;
                    closetSlope = sp;
                }
                dist = sp.EndPoint.DistanceTo(axisMiddlePt);
                if (dist <= minDis)
                {
                    minDis = dist;
                    closetSlope = sp;
                }
            }
            return closetSlope;
        }

        #endregion

        #endregion

        #region --- 与边坡相关的操作

        /// <summary> 提取断面中对应的边坡对象。在某些断面中，可能根本就不会画出边坡线，此时返回 null </summary>
        /// <param name="left">true 表示提取左边的边坡， false 表示提取右边的边坡</param>
        /// <returns></returns>
        public SlopeLine GetSlopeLine(bool left)
        {
            var xdata = XData;
            var db = DocMdf.acDataBase;
            if (left)
            {
                if (xdata.LeftSlopeExists)
                {
                    var pl = xdata.LeftSlopeHandle.GetDBObject<Polyline>(db);
                    var retainingWall = xdata.LeftRetainingWallExists
                        ? xdata.LeftRetainingWallHandle.GetDBObject<Polyline>(db)
                        : null;
                    var slp = new SlopeLine(DocMdf, pl, this, onLeft: true, isFill: xdata.LeftSlopeFill, retainingWall: retainingWall);
                    return slp;
                }
            }
            else
            {
                if (xdata.RightSlopeExists)
                {
                    var pl = xdata.RightSlopeHandle.GetDBObject<Polyline>(db);
                    var retainingWall = xdata.RightRetainingWallExists
                        ? xdata.RightRetainingWallHandle.GetDBObject<Polyline>(db)
                        : null;
                    var slp = new SlopeLine(DocMdf, pl, this, onLeft: false, isFill: xdata.RightSlopeFill, retainingWall: retainingWall);
                    return slp;
                }
            }
            return null;
        }

        #endregion

        #region --- AutoCAD中几何坐标 与 横断面图中的 标高进行对应

        /// <summary> 根据标高值返回对应的几何Y值 </summary>
        public double GetYFromElev(double elevation)
        {
            return _centerY - _centerEle + elevation;
        }

        /// <summary> 根据几何坐标的Y值返回对应的标高值 </summary>
        public double GetEleFromY(double y)
        {
            return _centerEle - _centerY + y;
        }

        #endregion

        public void DrawWaterLevel(double waterLevel, ObjectId layer)
        {
            var wy = GetYFromElev(waterLevel);
            var l = new Line(new Point3d(_centerX - 15, wy, 0), new Point3d(_centerX + 15, wy, 0));
            //
            var tr = DocMdf.acTransaction;
            var btrT = tr.GetObject(DocMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr = tr.GetObject(btrT[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //
            l.LayerId = layer;
            btr.AppendEntity(l);
            tr.AddNewlyCreatedDBObject(l, true);
            l.Draw();
        }

        public override string ToString()
        {
            return _station.ToString();
        }
    }
}