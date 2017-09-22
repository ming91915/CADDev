using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 路基边坡线 </summary>
    public class SlopeLine
    {
        #region --- Fields

        public readonly DocumentModifier _docMdf;
        public double Station { get; }

        public Polyline Pline { get; }

        /// <summary> 边坡所对应的横断面 </summary>
        public readonly SubgradeSection Section;

        /// <summary> 边坡侧的挡土墙，如果不存在，则为 null </summary>
        private readonly Polyline _retainingWall;

        /// <summary> 边坡侧的挡土墙所对应的几何图形，如果不存在，则为 null </summary>
        private readonly CompositeCurve3d _retainingWallCurve;

        private readonly bool _onLeft;
        private readonly bool _isFill;
        private readonly RetainingWallType _retainingWallType;

        private static readonly SelectionFilter _filter = new SelectionFilter(new[]{

                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Left_Cut),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Right_Cut),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Left_Fill),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Right_Fill),
                new TypedValue((int) DxfCode.Operator, "OR>")});

        private static readonly SelectionFilter _filterLeft = new SelectionFilter(new[]{

                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Left_Cut),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Left_Fill),
                new TypedValue((int) DxfCode.Operator, "OR>")});

        private static readonly SelectionFilter _filterRight = new SelectionFilter(new[]{

                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Right_Cut),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_Slope_Right_Fill),
                new TypedValue((int) DxfCode.Operator, "OR>")});


        /// <summary> 从 AutoCAD 界面中过滤出边坡对象的过滤规则 </summary>
        public static SelectionFilter Filter
        {
            get
            {
                return _filter;
            }
        }
        /// <summary> 从 AutoCAD 界面中过滤出边坡对象的过滤规则 </summary>
        public static SelectionFilter FilterLeft
        {
            get
            {
                return _filterLeft;
            }
        }
        public static SelectionFilter FilterRight
        {
            get
            {
                return _filterRight;
            }
        }
        /// <summary> 判断指定的图层名是否表示边坡对象 </summary>
        /// <param name="layerName"></param>
        /// <param name="left">true 表示匹配左侧边坡，false 表示匹配右侧边坡，null 表示匹配两侧边坡 </param>
        /// <returns></returns>
        public static bool IsSlopeLineLayer(string layerName, bool? left)
        {
            if (left.HasValue)
            {
                if (left.Value)
                {
                    return (layerName == Options_LayerNames.LayerName_Slope_Left_Cut
                     || layerName == Options_LayerNames.LayerName_Slope_Left_Fill);
                }
                else
                {
                    return (layerName == Options_LayerNames.LayerName_Slope_Right_Cut
                     || layerName == Options_LayerNames.LayerName_Slope_Right_Fill);
                }
            }
            else
            {
                return (layerName == Options_LayerNames.LayerName_Slope_Left_Cut
                        || layerName == Options_LayerNames.LayerName_Slope_Right_Cut
                        || layerName == Options_LayerNames.LayerName_Slope_Left_Fill
                        || layerName == Options_LayerNames.LayerName_Slope_Right_Fill);
            }
        }


        #endregion

        #region --- 构造函数

        /// <summary>构造方式一：从零开始判断与计算（不能成功构造，则返回 null）  </summary>
        /// <param name="docMdf"></param>
        /// <param name="pline"></param>
        /// <param name="errMsg"></param>
        /// <returns>如果创建失败，则返回 null </returns>
        public static SlopeLine Create(DocumentModifier docMdf, Polyline pline, out string errMsg)
        {
            errMsg = "";
            if (pline.Closed)
            {
                errMsg = "无法识别闭合的边坡对象";
                return null;
            }

            // 找到对应的中轴线对象

            var ca = FindCenterAxisOnHandle(docMdf, pline);
            if (ca == null)
            {
                ca = FindCenterAxisOnScreen(docMdf, pline);
                if (ca == null)
                {
                    errMsg = "无法识别对应的道路中心线";
                    return null;
                }
            }
            if (!ca.XData.FullyCalculated)
            {
                ca.CalculateSectionInfoToXData();
                //
                ca.CenterLine.UpgradeOpen();
                ca.FlushXData();
                ca.CenterLine.DowngradeOpen();
            }
            // 判断当前多段线是横断面系统中的哪一个
            var xdata = ca.XData;
            if (ca.XData.LeftSlopeHandle == pline.Handle)
            {
                var retainingWall = xdata.LeftRetainingWallType != RetainingWallType.无
                    ? docMdf.acDataBase.GetObjectId(false, xdata.LeftRetainingWallHandle, 0).GetObject(OpenMode.ForRead)
                        as Polyline
                    : null;
                //
                return new SlopeLine(docMdf, pline, ca, true, ca.XData.LeftSlopeFill.Value, xdata.LeftRetainingWallType, retainingWall);
            }
            if (ca.XData.RightSlopeHandle == pline.Handle)
            {
                var retainingWall = xdata.RightRetainingWallType != RetainingWallType.无
                    ? docMdf.acDataBase.GetObjectId(false, xdata.RightRetainingWallHandle, 0)
                        .GetObject(OpenMode.ForRead) as Polyline
                    : null;
                //
                return new SlopeLine(docMdf, pline, ca, false, ca.XData.RightSlopeFill.Value, xdata.RightRetainingWallType, retainingWall);
            }

            errMsg = $"提取边坡对象出现异常，请重新通过“{SectionsConstructor.CommandName}”命令对横断面系统进行构造";
            return null;
        }

        /// <summary> 构造方式二： 通过<seealso cref="SubgradeSection" /> 进行指定 </summary>
        /// <param name="docMdf"></param>
        /// <param name="pline"></param>
        /// <param name="axis"></param>
        /// <param name="onLeft"></param>
        /// <param name="isFill"></param>
        /// <param name="retainingWall"></param>
        public SlopeLine(DocumentModifier docMdf, Polyline pline, SubgradeSection axis, bool onLeft, bool isFill, RetainingWallType retainingWallType,
            Polyline retainingWall = null)
        {
            _docMdf = docMdf;
            //
            Pline = pline;
            Section = axis;
            Station = axis.XData.Station;
            _onLeft = onLeft;
            _isFill = isFill;
            _retainingWallType = retainingWallType;
            _retainingWall = retainingWall;
            if (retainingWall != null)
            {
                _retainingWallCurve = _retainingWall.GetGeCurve() as CompositeCurve3d;
            }
            // 构造边坡系统
            ConstructSlopeSys(pline, out _slopes, out _platforms);
        }

        /// <summary> 找到边坡线附近所属的中心轴线 </summary>
        /// <param name="pl"></param>
        /// <remarks>未找到则返回 null </remarks>
        private static SubgradeSection FindCenterAxisOnScreen(DocumentModifier docMdf, Polyline pl)
        {
            var pline = pl;
            var bdbl = pline.Bounds.Value.MinPoint; // 左下角点
            bdbl = new Point3d(bdbl.X - Options_General.RoadWidth / 2, bdbl.Y - 0, bdbl.Z);
            var bdtr = pline.Bounds.Value.MaxPoint; // 右上角点
            bdtr = new Point3d(bdtr.X + Options_General.RoadWidth / 2, bdtr.Y + 0, bdtr.Z);
            var res = docMdf.acEditor.SelectCrossingWindow(bdbl, bdtr, SubgradeSection.Filter);

            if (res.Status == PromptStatus.OK)
            {
                var ids = res.Value.GetObjectIds();

                Line minLine = null;
                var minDis = double.MaxValue;
                // 可能找到多条轴线，比较轴线中点到边坡线两个端点距离最小的那个轴线
                foreach (var id in ids)
                {
                    var l = id.GetObject(OpenMode.ForRead) as Line;
                    var dir = l.StartPoint - l.EndPoint;
                    if (dir.IsParallelTo(new Vector3d(0, 1, 0)))
                    {
                        // 根据距离最近的原则来从众多的轴线中寻找匹配项
                        var centMid = l.StartPoint.Add((l.EndPoint - l.StartPoint).DivideBy(2)); // 轴线的中点

                        var d1 = centMid.DistanceTo(pl.StartPoint);
                        var d2 = centMid.DistanceTo(pl.StartPoint);
                        var minD = Math.Min(d1, d2);
                        if (minD < minDis)
                        {
                            minLine = l;
                            minDis = minD;
                        }
                    }
                }


                // 构造 CenterAxis 对象
                SubgradeSection centerLine = null;
                var si = SectionInfo.FromCenterLine(minLine);
                if (si != null && si.FullyCalculated)
                {
                    centerLine = new SubgradeSection(docMdf, minLine, si);
                }

                if (centerLine == null)
                {
                    centerLine = SubgradeSection.Create(docMdf, minLine);
                }
                return centerLine;
            }
            docMdf.WriteLineIntoDebuger("在边坡线周围没有找到中心轴线。请检查边坡线数据中是否保存有道路中线句柄值，或者将视图缩小以在屏幕中显示出对应的轴线。轴线搜索范围：" + "\r\n" + bdbl + " ~ " + bdtr);
            return null;
        }


        /// <summary> 根据边坡线对象中可能存储的轴线对象句柄值来提取对应的轴线对象 </summary>
        /// <param name="pl"></param>
        /// <remarks>未找到则返回 null </remarks>
        private static SubgradeSection FindCenterAxisOnHandle(DocumentModifier docMdf, Polyline pl)
        {
            var data = SlopeData.FromEntity(pl);
            if (data != null && data.FullyCalculated)
            {
                var cl = data.CenterAxisHandle.GetDBObject<Line>(docMdf.acDataBase);
                if (cl != null)
                {
                    var si = SectionInfo.FromCenterLine(cl);
                    if (si != null)
                    {
                        var ss = new SubgradeSection(docMdf, cl, si);
                        return ss;
                    }
                }
            }
            return null;
        }

        #endregion

        #region --- 边坡系统的构造

        /// <summary> 挡墙与自然地面交点相对于边坡线内侧点的高差。对于填方边坡，其值一般为负，且只考虑路肩墙，不考虑路堤墙或护脚墙；
        /// 对于挖方边坡，其值一般为正。如果其值为0，一般即表示此边坡中没有挡墙。 </summary>
        private double _retainingWallHeight;

        /// <summary>
        ///     边坡所对应的几何线段，每一个线段都代表了某一级边坡（不包括马道平台）。
        ///     同时，位于集合中靠前位置的对象，表示此对象更接近路面
        /// </summary>
        private List<Slope> _slopes;
        /// <summary>
        ///     边坡所对应的几何线段，每一个线段都代表了某一级马道平台。
        ///     同时，位于集合中靠前位置的对象，表示此对象更接近路面
        /// </summary>
        private List<Platform> _platforms;

        /// <summary>
        ///     构造边坡系统
        /// </summary>
        /// <param name="pl"></param>
        /// <returns>返回的集合中，包含了边坡与平台，并排除了与挡墙重合的部分 </returns>
        private void ConstructSlopeSys(Polyline pl, out List<Slope> slopes, out List<Platform> platforms)
        {
            slopes = new List<Slope>();
            platforms = new List<Platform>();
            var segs = new List<LineSegment3d>();
            var curve = pl.GetGeCurve() as CompositeCurve3d;

            foreach (var c in curve.GetCurves().OfType<LineSegment3d>())
            {
                if (c.Length > ProtectionConstants.MinSlopeSegLength)
                {
                    segs.Add(c);
                }
            }
            if (segs.Count == 0) return;
            //
            var slopeIndex = 0;
            var retainingWallSegIndex = 0;
            bool lastIsPlatform = false;
            for (int i = 0; i < segs.Count; i++)
            {
                var seg = segs[i];
                if (_retainingWallCurve != null
                    && _retainingWallCurve.IsOn(new Point3d(seg.StartPoint.X, seg.StartPoint.Y, 0))
                    && _retainingWallCurve.IsOn(new Point3d(seg.EndPoint.X, seg.EndPoint.Y, 0)))
                {
                    retainingWallSegIndex = i;
                    // 说明这一段线是与挡墙重合的，不计入边坡
                    _retainingWallHeight = seg.EndPoint.Y - pl.StartPoint.Y;
                    lastIsPlatform = false;
                }
                else
                {
                    // 设定与水平方向平行的向量容差
                    if (seg.Direction.IsParallelTo(ProtectionConstants.HorizontalVec3,
                        new Tolerance(0.0005, Tolerance.Global.EqualPoint)))
                    {
                        // 说明可能是平台，但如果其长度大于设置的平台最宽值，则认为其为一个平坡
                        if (seg.Length <= ProtectionConstants.MaxPlatformLength)
                        {
                            // 说明是一个平台
                            if (lastIsPlatform)
                            {
                                // 有可能会出现在一条边坡多段线中，连续相邻两个段都是平台的情况，此时将二者合并为一个平台
                                platforms[platforms.Count - 1] = new Platform(platforms[platforms.Count - 1].Index, platforms[platforms.Count - 1].InnerPoint, seg.EndPoint);
                            }
                            else
                            {
                                var plf = new Platform(slopeIndex, seg.StartPoint, seg.EndPoint);
                                platforms.Add(plf);
                            }
                            lastIsPlatform = true;
                        }
                        else
                        {
                            // 将其归类为平坡
                            //var topPt = _isFill ? seg.StartPoint : seg.EndPoint;
                            //// 边坡多段线的分段中，越靠前的表示靠近路面，所以填方边坡的多段线分段是向下走的。
                            //var bottomPt = _isFill ? seg.EndPoint : seg.StartPoint;
                            // 边坡多段线的分段中，越靠前的表示靠近路面，所以填方边坡的多段线分段是向下走的。
                            slopeIndex += 1;
                            var slp = new Slope(slopeIndex, innerPt: seg.StartPoint, outerPt: seg.EndPoint);
                            slopes.Add(slp);
                            lastIsPlatform = false;
                            //
                        }
                    }
                    else
                    {
                        // 说明肯定是边坡
                        //var topPt = _isFill ? seg.StartPoint : seg.EndPoint; // 边坡多段线的分段中，越靠前的表示靠近路面，所以填方边坡的多段线分段是向下走的。
                        //var bottomPt = _isFill ? seg.EndPoint : seg.StartPoint;
                        // 边坡多段线的分段中，越靠前的表示靠近路面，所以填方边坡的多段线分段是向下走的。
                        slopeIndex += 1;
                        var slp = new Slope(slopeIndex, innerPt: seg.StartPoint, outerPt: seg.EndPoint);
                        slopes.Add(slp);
                        lastIsPlatform = false;
                        //
                    }
                }
            }
            // 判断挡墙是否为护脚墙
            if (_retainingWall != null && retainingWallSegIndex == segs.Count - 1 && (slopes.Count + platforms.Count) > 0)
            {
                _retainingWallHeight = 0;
            }
        }

        #endregion

        #region --- 标高相关计算

        /// <summary> 边坡线位于某一标高下的斜边长度 </summary>
        /// <param name="elev"></param>
        /// <returns></returns>
        private double GetSlopeLengthBelowElevation(double elev)
        {
            var yw = Section.GetYFromElev(elev); // 水位标高所对应的几何Y坐标
            var l = 0.0;
            foreach (var s in _slopes)
            {
                l += LineLengthBelowY(s, yw);
            }
            return l;
        }

        private static double LineLengthBelowY(Slope l, double waterY)
        {
            if (l.TopPoint.Y <= waterY && l.BottomPoint.Y <= waterY)
            {
                return l.Length;
            }
            if (l.TopPoint.Y >= waterY && l.BottomPoint.Y >= waterY)
            {
                // 说明全部位于水上
                return 0;
            }
            // 说明与水位线相交
            var li = new LineSegment2d(l.BottomPoint.ToXYPlane(), l.TopPoint.ToXYPlane());
            var inters = li.IntersectWith(new Line2d(new Point2d(0, waterY), new Point2d(1, waterY)));
            if (inters != null && inters.Length > 0)
            {
                // var bottomPt = l.StartPoint.Y > l.EndPoint.Y ? l.EndPoint : l.StartPoint;
                return l.BottomPoint.ToXYPlane().GetDistanceTo(inters[0]);
            }
            // 异常情况，不应该没找到交点
            return l.Length / 2; // 随意取个值吧
        }

        private double GetTopElevation()
        {
            var maxY = Math.Max(Pline.StartPoint.Y, Pline.EndPoint.Y);
            return Section.GetEleFromY(maxY);
            //
            var segms = SlopeData.Combine(_slopes, _platforms, sort: false);
            if (segms.Count == 0)
            {
                var topPt = _isFill
                    ? Pline.GetLineSegment2dAt(0).EndPoint
                    : Pline.GetLineSegment2dAt(Pline.NumberOfVertices - 2).StartPoint;
                return Section.GetEleFromY(topPt.Y);
            }
            else
            {
                var topS = _isFill ? segms[0] : segms[segms.Count - 1];
                if (topS is Slope)
                {
                    return Section.GetEleFromY((topS as Slope).TopPoint.Y);
                }
                else
                {
                    return Section.GetEleFromY((topS as Platform).MiddlePoint.Y);
                }
            }
        }

        private double GetBottomElevation()
        {
            var maxY = Math.Min(Pline.StartPoint.Y, Pline.EndPoint.Y);
            return Section.GetEleFromY(maxY);
            //
            var segms = SlopeData.Combine(_slopes, _platforms, sort: false);
            if (segms.Count == 0)
            {
                var bottomPt = _isFill
                    ? Pline.GetLineSegment2dAt(Pline.NumberOfVertices - 2).EndPoint
                    : Pline.GetLineSegment2dAt(0).StartPoint;
                return Section.GetEleFromY(bottomPt.Y);
            }
            else
            {
                var bottomS = _isFill ? segms[segms.Count - 1] : segms[0];
                if (bottomS is Slope)
                {
                    return Section.GetEleFromY((bottomS as Slope).BottomPoint.Y);
                }
                else
                {
                    return Section.GetEleFromY((bottomS as Platform).MiddlePoint.Y);
                }
            }
        }

        #endregion

        #region --- 将边坡防护形式显示在边坡图形的附近

        #region --- 写入界面

        /// <summary> 将边坡防护形式显示在边坡图形的附近 </summary>
        public void PrintProtectionMethod(BlockTableRecord btr, Dictionary<string, ObjectId> protLayers)
        {
            var xdata = XData;
            foreach (var ss in xdata.Slopes)
            {
                WriteProtectionMethod(ss, btr, protLayers);
            }
            foreach (var ss in xdata.Platforms)
            {
                WriteProtectionMethod(ss, btr, protLayers);
            }
        }


        private void WriteProtectionMethod(ISlopeSeg ss, BlockTableRecord btr, Dictionary<string, ObjectId> protLayers)
        {
            if (!string.IsNullOrEmpty(ss.ProtectionMethod))
            {
                var originalText = ss.ProtectionMethodText.GetDBObject<DBText>(_docMdf.acDataBase);
                // 创建或者修改
                if (originalText == null)
                {
                    // 创建
                    var newText = CreateProtectionMethodText(ss);
                    newText.LayerId = protLayers[newText.TextString];

                    // 相关数据
                    var protTd = new ProtTextData(Station, _onLeft, (ss is Slope), ss.Index);
                    ResultBuffer xdata = protTd.ToResultBuffer();
                    newText.XData = xdata;

                    //
                    btr.AppendEntity(newText);
                    _docMdf.acTransaction.AddNewlyCreatedDBObject(newText, true);
                    // 在Draw()方法执行之前，必须确保此对象已经添加到数据库中，否则执行Draw()时，会造成 AutoCAD 的崩溃！
                    // 同时，还应将 Draw() 方法放在 acTransaction.AddNewlyCreatedDBObject 之前，否则，对于新添加到数据库中的对象，它只能在事务提交后才能在界面上显示出来。
                    newText.Draw();

                    //
                    ss.ProtectionMethodText = newText.Handle;
                }
                else
                {
                    // 修改
                    originalText.UpgradeOpen();
                    originalText.TextString = ss.ProtectionMethod;
                    originalText.LayerId = protLayers[originalText.TextString];
                    originalText.Draw();
                    originalText.DowngradeOpen();
                }
            }
            else
            {
                var originalText = ss.ProtectionMethodText.GetDBObject<DBText>(_docMdf.acDataBase);
                // 不作任何操作或者将已有的删除
                if (originalText != null)
                {
                    // 原文本对象存在，但是其防护为空
                    // 删除
                    originalText.UpgradeOpen();
                    originalText.Erase(true);
                    originalText.DowngradeOpen();
                    ss.ProtectionMethodText = default(Handle);
                }
                else
                {
                    // 原文本对象不存在，而且其防护为空
                    // 不作任何操作
                }
            }
        }

        private DBText CreateProtectionMethodText(ISlopeSeg ss)
        {
            var rota = Math.Atan((ss.OuterPoint.Y - ss.InnerPoint.Y) / (ss.OuterPoint.X - ss.InnerPoint.X));// / 2 / Math.PI * 360;
            var txt = new DBText();
            //
            //txt.HorizontalMode = TextHorizontalMode.TextCenter;
            //txt.VerticalMode = TextVerticalMode.TextVerticalMid;
            txt.Justify = AttachmentPoint.TopCenter;
            // 如果要设置对齐，则一定要先设置 HorizontalMode 与 VerticalMode，最后设置 AlignmentPoint。设置了对齐后，Position属性被屏蔽，不论设置为什么值都不起作用。
            txt.AlignmentPoint = ss.MiddlePoint;
            //
            txt.TextString = ss.ProtectionMethod;
            txt.Rotation = rota; // 弧度
            txt.Height = 0.72;
            txt.WidthFactor = 0.7;
            //
            return txt;
        }

        #endregion

        #region --- 从界面提取

        /// <summary> 从AutoCAD界面中的防护文字中提取对应的防护数据 </summary>
        /// <param name="seg"></param>
        /// <param name="db"></param>
        public static void ExtractProtectionFromText(ISlopeSeg seg, Database db)
        {
            DBText text = null;
            try
            {
                text = seg.ProtectionMethodText.GetDBObject<DBText>(db);
            }
            catch (Exception)
            {
            }
            if (text != null && !string.IsNullOrEmpty(text.TextString))
            {
                seg.ProtectionMethod = text.TextString;
            }
            else
            {
                seg.ProtectionMethod = null;
            }
        }

        #endregion

        #region --- 从界面删除

        /// <summary> 强行将边坡的防护方式的显示字符删除 </summary>
        public void ClearProtectionMethodText()
        {
            var xdata = XData;
            foreach (var ss in xdata.Slopes)
            {
                EraseText(ss, _docMdf.acDataBase);
            }
            foreach (var ss in xdata.Platforms)
            {
                EraseText(ss, _docMdf.acDataBase);
            }
        }

        /// <summary> 删除子边坡所绑定的防护文字 </summary>
        /// <param name="seg"></param>
        /// <param name="db"></param>
        public static void EraseText(ISlopeSeg seg, Database db)
        {
            DBText originalText = null;
            try
            {
                originalText = seg.ProtectionMethodText.GetDBObject<DBText>(db);
            }
            catch (Exception)
            {
            }
            if (originalText != null)
            {
                originalText.UpgradeOpen();
                originalText.Erase(true);
                originalText.DowngradeOpen();
                seg.ProtectionMethodText = default(Handle);
            }
        }

        #endregion

        #endregion

        /// <summary> 强行将边坡的绑定的水位线删除 </summary>
        public void ClearAllWaterlines(Database db)
        {
            var xdata = XData;
            foreach (var wl in xdata.Waterlines)
            {
                Line line = null;
                try
                {
                    line = wl.GetDBObject<Line>(db);
                }
                catch (Exception)
                {
                }
                if (line != null)
                {
                    line.UpgradeOpen();
                    line.Erase(true);
                    line.DowngradeOpen();
                }
            }
            xdata.Waterlines = new List<Handle>();
        }

        #region --- XData

        private SlopeData _xData;

        public SlopeData XData
        {
            get
            {
                if (_xData != null)
                {
                    return _xData;
                }
                // 从 AutoCAD 中读取
                try
                {
                    _xData = SlopeData.FromEntity(Pline);
                }
                catch (Exception)
                {
                    _xData = null;
                    //MessageBox.Show("读取图元中的信息失败，现将其设置为默认值");
                    //Pline.UpgradeOpen();
                    //var bf = SlopeData.ClearValue();
                    //Pline.XData = bf;
                    //Pline.DowngradeOpen();
                }
                return _xData ?? new SlopeData(Station, _onLeft) { FullyCalculated = false };
            }
            private set { _xData = value; }
        }

        public bool XDataToBeCleared { get; set; }

        /// <summary> 从边坡线中计算出基本的几何信息 </summary>
        /// <param name="xdata"></param>
        public void CalculateXData()
        {
            var xdata = XData;
            xdata.CenterAxisHandle = Section.CenterLine.Handle;
            xdata.Station = Station;
            xdata.OnLeft = _onLeft;
            xdata.FillCut = _isFill;
            //
            xdata.TopElevation = GetTopElevation();
            xdata.BottomElevation = GetBottomElevation();
            xdata.RetainingWallHeight = _retainingWallHeight;

            // 每一级的边坡与平台信息
            xdata.Slopes = _slopes;
            xdata.Platforms = _platforms;
            //
            xdata.FullyCalculated = true;
        }

        /// <summary> 将<seealso cref="SlopeData" />的信息灌注到对象中 </summary>
        public void ImportSlopeData(SlopeData newData)
        {
            if (newData.FullyCalculated)
            {
                XData = newData;

                // 对可以被用户通过界面进行修改的项进行赋值（主要是对用户自行设置的防护方式与防护长度进行赋值）
                _slopes = newData.Slopes;
                _platforms = newData.Platforms;
            }
        }

        /// <summary> 将数据保存到多段线的 ExtensionDictionary 中 </summary>
        /// <remarks> 本函数可能会导致 AutoCAD中 撤消 时的数据丢失 </remarks>
        public void FlushXData()
        {
            var slopeXdata = XData;

            if (Pline.ExtensionDictionary.IsNull)
            {
                // Pline.UpgradeOpen();
                Pline.CreateExtensionDictionary();
                // Pline.DowngradeOpen();
            }
            // 总的字典
            var extensionDict = Pline.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
            var trans = _docMdf.acTransaction;

            // 一般性的数据 保存到 Dictionary
            var genBuff = slopeXdata.ToResBuff_General();
            var rec = new Xrecord() { Data = genBuff };
            Utils.OverlayDictValue(trans, extensionDict, SlopeData.DictKey_General, rec);

            // 边坡数据 保存到 Dictionary
            slopeXdata.ToDict_Slopes(_docMdf.acTransaction, extensionDict);

            // 水位线数据 保存到 Dictionary
            var waterlines = slopeXdata.ToResBuff_Waterlines();
            rec = new Xrecord() { Data = waterlines };
            Utils.OverlayDictValue(trans, extensionDict, SlopeData.DictKey_Waterlines, rec);
            //
            extensionDict.DowngradeOpen();
        }

        public void ClearXData()
        {
            //
            ClearProtectionMethodText();
            ClearAllWaterlines(_docMdf.acDataBase);
            //
            if (!Pline.ExtensionDictionary.IsNull)
            {
                var extensionDict = Pline.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (extensionDict.Contains(SlopeData.DictKey_General))
                {
                    extensionDict.Remove(SlopeData.DictKey_General);
                }
                if (extensionDict.Contains(SlopeData.DictKey_Slopes))
                {
                    extensionDict.Remove(SlopeData.DictKey_Slopes);
                }
                if (extensionDict.Contains(SlopeData.DictKey_Waterlines))
                {
                    extensionDict.Remove(SlopeData.DictKey_Waterlines);
                }
            }
        }

        public override string ToString()
        {
            return DataInfo;
        }

        /// <summary> 边坡的基本信息 </summary>
        public string DataInfo
        {
            get
            {
                var info = Section.XData.Station.ToString("0.000") + ",\t"
                           + (XData.OnLeft ? "左侧" : "右侧") + ",\t" + (XData.FillCut ? "填方" : "挖方")
                           + ",\t" + (_retainingWall != null ? "挡墙" : "")
                    ;
                return info;
            }
        }

        #endregion

        #region --- 边坡防护形式的确定

        #region --- 根据自动防护规则进行防护

        /// <summary> 自动确定每一级边坡的防护形式 </summary>
        /// <param name="criterion">自动防护的规则</param>
        public void AutoSetProtectionMethods(AutoProtectionCriterions criterion)
        {
            var data = XData;
            var firstProt = GetFirstSlopeProtection(criterion, data);
            foreach (var s in data.Slopes)
            {
                s.ProtectionMethod = firstProt;
            }
            //foreach (var s in data.Platforms)
            //{
            //    s.ProtectionMethod = firstProt;
            //}
        }

        /// <summary>
        /// 将边坡从第一条规则开始过滤，如果任何一整条规则都不符合，则返回 null
        /// </summary>
        /// <param name="criterions"></param>
        /// <returns></returns>
        private string GetFirstSlopeProtection(AutoProtectionCriterions criterions, SlopeData data)
        {
            // 将边坡从第一条规则开始过滤，如果任何一整条规则都不符合，则返回 null
            foreach (var cr in criterions.SlopeCriterions)
            {
                // 填挖方
                if ((cr.Fill == Operator_Bool.是 && !_isFill) || (cr.Fill == Operator_Bool.否 && _isFill))
                {
                    continue;
                }
                // 首级边坡坡比
                if (data.Slopes.Count == 0)
                {
                    continue;
                }
                var firstSlopeRatio = Math.Abs(data.Slopes[0].SlopeRatio);
                if (!InRangeCollection(cr.FirstSlopeRatio.AndRange, firstSlopeRatio))
                {
                    continue;
                }

                // 边坡高度
                var slopeHeight = Math.Abs(Pline.StartPoint.Y - Pline.EndPoint.Y);
                if (!InRangeCollection(cr.SlopeHeight.AndRange, slopeHeight))
                {
                    continue;
                }

                // 边坡总坡级
                var slopeLevelCount = data.Slopes.Count;
                if (!InRangeCollection(cr.SlopeLevel.AndRange, slopeLevelCount))
                {
                    continue;
                }

                // 所有条件都满足，确定防护方式
                return cr.ProtectionMethod;
            }
            return null;
        }

        private bool InRangeCollection(XmlList<CriterionRange> criterions, double value)
        {
            foreach (var rg in criterions)
            {
                if (!rg.InRange(value))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region --- 根据手动防护规则进行防护

        public void ForceProtect(ForceProtection criterion)
        {
            var data = XData;
            if (criterion.SlopeLevels == null)
            {
                foreach (var s in data.Slopes)
                {
                    s.ProtectionMethod = criterion.ProtMethod;
                }
            }
            else
            {
                foreach (var s in data.Slopes)
                {
                    if (criterion.SlopeLevels.Contains(s.GetMainLevel()))
                    {
                        s.ProtectionMethod = criterion.ProtMethod;
                    }
                }
            }
        }

        #endregion

        #region --- 强制手动防护

        /// <summary> 自动确定每一级边坡的防护形式 </summary>
        /// <param name="criterion">自动防护的规则</param>
        public void AutoSetProtectionMethods()
        {

            // 每一级的边坡与平台信息
            var slopes = XData.Slopes;
            if (slopes.Count > 0)
            {
                slopes[0].ProtectionMethod = SetProtectionMethods_Slope(slopes[0], _isFill);
            }
            for (var i = 1; i < slopes.Count; i++)
            {
                slopes[i].ProtectionMethod = slopes[0].ProtectionMethod;
            }

            ////
            //foreach (var slp in slopes)
            //{
            //    slp.ProtectionMethod = SetProtectionMethods_Slope(slp, _isFill);
            //}

            // 平台防护
            var platforms = XData.Platforms;
            foreach (var plf in platforms)
            {
                bool foundInnerSlope = false;
                for (int i = slopes.Count - 1; i >= 0; i--)
                {
                    if (slopes[i].Index <= plf.Index)
                    {
                        foundInnerSlope = true;
                        plf.ProtectionMethod = slopes[i].ProtectionMethod;
                        break;
                    }
                }
                if (!foundInnerSlope)
                {
                    plf.ProtectionMethod = SetProtectionMethods_Platform(plf, _isFill);
                }
                // plf.ProtectionMethod = SetProtectionMethods_Platform(plf, _isFill);
            }

            // 将数据进行刷新保存
            XData.Slopes = slopes;
            XData.Platforms = platforms;
        }

        private static string SetProtectionMethods_Slope(Slope slp, bool fill)
        {
            var dir = slp.SlopeRatio; // Math.Abs(slp.SlopeRatio.X) / slp.SlopeRatio.Y; // 边坡坡率为 1:dir
            if (!fill)
            {
                if (dir <= 0.6)
                {
                    return "挂网喷锚";
                }
                else
                {
                    return "锚杆网格梁";
                }
            }
            else
            {
                return "干砌片石";
            }
            return $"{dir.ToString("0.00")} 坡比防护";
        }

        private static string SetProtectionMethods_Platform(Platform plf, bool fill)
        {
            return "";
        }
        #endregion

        #endregion
    }
}