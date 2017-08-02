using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

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

        private static readonly SelectionFilter _filter = new SelectionFilter(new[]{

                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Left_Cut),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Right_Cut),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Left_Fill),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Right_Fill),
                new TypedValue((int) DxfCode.Operator, "OR>")});
        /// <summary> 从 AutoCAD 界面中过滤出边坡对象的过滤规则 </summary>
        public static SelectionFilter Filter
        {
            get
            {
                return _filter;
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
                    return (layerName == ProtectionOptions.LayerName_Slope_Left_Cut
                     || layerName == ProtectionOptions.LayerName_Slope_Left_Fill);
                }
                else
                {
                    return (layerName == ProtectionOptions.LayerName_Slope_Right_Cut
                     || layerName == ProtectionOptions.LayerName_Slope_Right_Fill);
                }
            }
            else
            {
                return (layerName == ProtectionOptions.LayerName_Slope_Left_Cut
                        || layerName == ProtectionOptions.LayerName_Slope_Right_Cut
                        || layerName == ProtectionOptions.LayerName_Slope_Left_Fill
                        || layerName == ProtectionOptions.LayerName_Slope_Right_Fill);
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
                var retainingWall = xdata.LeftRetainingWallExists
                    ? docMdf.acDataBase.GetObjectId(false, xdata.LeftRetainingWallHandle, 0).GetObject(OpenMode.ForRead)
                        as Polyline
                    : null;
                //
                return new SlopeLine(docMdf, pline, ca, true, ca.XData.LeftSlopeFill, retainingWall);
            }
            if (ca.XData.RightSlopeHandle == pline.Handle)
            {
                var retainingWall = xdata.RightRetainingWallExists
                    ? docMdf.acDataBase.GetObjectId(false, xdata.RightRetainingWallHandle, 0)
                        .GetObject(OpenMode.ForRead) as Polyline
                    : null;
                //
                return new SlopeLine(docMdf, pline, ca, false, ca.XData.RightSlopeFill, retainingWall);
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
        public SlopeLine(DocumentModifier docMdf, Polyline pline, SubgradeSection axis, bool onLeft, bool isFill,
            Polyline retainingWall)
        {
            _docMdf = docMdf;
            //
            Pline = pline;
            Section = axis;
            Station = axis.XData.Station;
            _onLeft = onLeft;
            _isFill = isFill;
            _retainingWall = retainingWall;
            if (retainingWall != null)
            {
                _retainingWallCurve = _retainingWall.GetGeCurve() as CompositeCurve3d;
            }
            // 构造边坡系统
            _slopes = ConstructSlopeSys(pline);
        }

        /// <summary> 找到边坡线附近所属的中心轴线 </summary>
        /// <param name="pl"></param>
        /// <remarks>未找到则返回 null </remarks>
        private static SubgradeSection FindCenterAxisOnScreen(DocumentModifier docMdf, Polyline pl)
        {
            var filterCenterAxis = new[]
            {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_CenterAxis)
            };
            var pline = pl;
            var bdbl = pline.Bounds.Value.MinPoint;
            var bdtr = pline.Bounds.Value.MaxPoint;
            var res =
                docMdf.acEditor.SelectCrossingWindow(
                    new Point3d(bdbl.X - ProtectionOptions.RoadWidth / 2, bdbl.Y - 0, bdbl.Z),
                    new Point3d(bdtr.X + ProtectionOptions.RoadWidth / 2, bdtr.Y + 0, bdtr.Z),
                    new SelectionFilter(filterCenterAxis));

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
            docMdf.WriteLineIntoDebuger("在边坡线周围没有找到中心轴线" + "\r\n" + bdbl + "\r\n" + bdtr);
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

        /// <summary>
        ///     边坡所对应的几何线段，每一个线段都代表了某一级边坡（包括马道平台）。
        ///     同时，位于集合中靠前位置的对象，表示此对象更接近路面
        /// </summary>
        private List<ISlopeSeg> _slopes;

        /// <summary>
        ///     构造边坡系统
        /// </summary>
        /// <param name="pl"></param>
        /// <returns>返回的集合中，包含了边坡与平台，并排除了与挡墙重合的部分 </returns>
        private List<ISlopeSeg> ConstructSlopeSys(Polyline pl)
        {
            var slopes = new List<ISlopeSeg>();
            var segs = new List<LineSegment3d>();
            var curve = pl.GetGeCurve() as CompositeCurve3d;

            foreach (var c in curve.GetCurves().OfType<LineSegment3d>())
            {
                if (c.Length > ProtectionConstants.MinSlopeSegLength)
                {
                    segs.Add(c);
                }
            }
            if (segs.Count == 0) return slopes;
            //
            var slopeIndex = 0;
            foreach (var seg in segs)
            {
                if (_retainingWallCurve != null
                    && _retainingWallCurve.IsOn(new Point3d(seg.StartPoint.X, seg.StartPoint.Y, 0))
                    && _retainingWallCurve.IsOn(new Point3d(seg.EndPoint.X, seg.EndPoint.Y, 0)))
                {
                    // 说明这一段线是与挡墙重合的，不计入边坡
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
                            var plf = new Platform(slopeIndex, seg.StartPoint, seg.EndPoint);
                            slopes.Add(plf);
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
                        //
                    }
                }
            }
            return slopes;
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
            foreach (var s in _slopes.OfType<Slope>())
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
            if (_slopes.Count == 0)
            {
                var topPt = _isFill
                    ? Pline.GetLineSegment2dAt(0).EndPoint
                    : Pline.GetLineSegment2dAt(Pline.NumberOfVertices - 2).StartPoint;
                return Section.GetEleFromY(topPt.Y);
            }
            else
            {
                var topS = _isFill ? _slopes[0] : _slopes[_slopes.Count - 1];
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
            if (_slopes.Count == 0)
            {
                var bottomPt = _isFill
                    ? Pline.GetLineSegment2dAt(Pline.NumberOfVertices - 2).EndPoint
                    : Pline.GetLineSegment2dAt(0).StartPoint;
                return Section.GetEleFromY(bottomPt.Y);
            }
            else
            {
                var bottomS = _isFill ? _slopes[_slopes.Count - 1] : _slopes[0];
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

        /// <summary> 将边坡防护形式显示在边坡图形的附近 </summary>
        public void PrintProtectionMethod(BlockTableRecord btr, ObjectId layerId_Slope, ObjectId layerId_Platform)
        {
            var xdata = XData;
            foreach (var ss in xdata.Slopes)
            {
                WriteProtectionMethod(ss, btr, layerId_Slope);
            }
            foreach (var ss in xdata.Platforms)
            {
                WriteProtectionMethod(ss, btr, layerId_Platform);
            }
        }

        private void WriteProtectionMethod(ISlopeSeg ss, BlockTableRecord btr, ObjectId layerId)
        {
            DBText originalText = null;
            try
            {
                originalText = ss.ProtectionMethodText.GetDBObject<DBText>(_docMdf.acDataBase);
            }
            catch (Exception)
            {
            }
            if (!string.IsNullOrEmpty(ss.ProtectionMethod))
            {
                // 创建或者修改
                if (originalText == null)
                {
                    // 创建
                    var newText = CreateProtectionMethodText(ss);
                    newText.LayerId = layerId;
                    btr.AppendEntity(newText);
                    _docMdf.acTransaction.AddNewlyCreatedDBObject(newText, true);
                    //
                    ss.ProtectionMethodText = newText.Handle;
                }
                else
                {
                    // 修改
                    originalText.UpgradeOpen();
                    originalText.TextString = ss.ProtectionMethod;
                    originalText.DowngradeOpen();
                }
            }
            else
            {
                // 不作任何操作或者将已有的删除
                if (originalText != null)
                {
                    // 删除
                    originalText.UpgradeOpen();
                    originalText.Erase(true);
                    originalText.DowngradeOpen();
                    ss.ProtectionMethodText = default(Handle);
                }
                else
                {
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

        /// <summary> 强行将边坡的防护方式的显示字符删除 </summary>
        public void ClearProtectionMethodText()
        {
            var xdata = XData;
            foreach (var ss in xdata.Slopes)
            {
                EraseText(ss);
            }
            foreach (var ss in xdata.Platforms)
            {
                EraseText(ss);
            }
        }

        private void EraseText(ISlopeSeg ss)
        {
            DBText originalText = null;
            try
            {
                originalText = ss.ProtectionMethodText.GetDBObject<DBText>(_docMdf.acDataBase);
            }
            catch (Exception)
            {
            }
            if (originalText != null)
            {
                originalText.UpgradeOpen();
                originalText.Erase(true);
                originalText.DowngradeOpen();
                ss.ProtectionMethodText = default(Handle);
            }
        }
        #endregion

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
                return _xData ?? new SlopeData(Station) { FullyCalculated = false };
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
            //
            var a = ProtectionOptions.WaterLevel;
            var b = ProtectionOptions.FillUpperEdge;
            //xdata.SlopeLengthBelowWaterLevel = GetSlopeLengthBelowElevation(ProtectionOptions.WaterLevel);
            //xdata.SlopeLengthBelowFillTop = GetSlopeLengthBelowElevation(ProtectionOptions.FillUpperEdge);

            // 每一级的边坡与平台信息
            xdata.Slopes = _slopes.OfType<Slope>().ToList();
            xdata.Platforms = _slopes.OfType<Platform>().ToList();
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
                if (newData.Slopes.Count + newData.Platforms.Count == _slopes.Count)
                {
                    _slopes = SlopeData.Combine(newData.Slopes, newData.Platforms, true);
                }
            }
        }

        public void FlushXData()
        {
            var xdata = XData;
            if (xdata != null)
            {
                if (Pline.ExtensionDictionary.IsNull)
                {
                    // Pline.UpgradeOpen();
                    Pline.CreateExtensionDictionary();
                    // Pline.DowngradeOpen();
                }
                // 总的字典
                var extensionDict = Pline.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;

                // 一般性的数据
                var gen = xdata.ToXrecord_General();
                extensionDict.SetAt(SlopeData.DictKey_General, gen);
                _docMdf.acTransaction.AddNewlyCreatedDBObject(gen, true);

                // 边坡数据
                xdata.ToDict_Slopes(_docMdf.acTransaction, extensionDict);

                extensionDict.DowngradeOpen();
            }
        }

        public void ClearXData()
        {
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
                if (extensionDict.Contains(SlopeData.DictKey_Platforms))
                {
                    extensionDict.Remove(SlopeData.DictKey_Platforms);
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

        /// <summary> 自动确定每一级边坡的防护形式 </summary>
        /// <param name="criterion">自动防护的规则</param>
        public void AutoSetProtectionMethods(AutoProtectionCriterions criterion)
        {
            // 每一级的边坡与平台信息
            var slopes = _slopes.OfType<Slope>().ToList();
            if (slopes.Count == 1)
            {
                slopes[0].ProtectionMethod = SetProtectionMethods_Slope(slopes[0], _isFill);
            }
            if (slopes.Count > 1)
            {
                for (var i = 1; i < slopes.Count; i++)
                {
                    slopes[i].ProtectionMethod = slopes[0].ProtectionMethod;
                }
            }
            //
            foreach (var slp in slopes)
            {
                slp.ProtectionMethod = SetProtectionMethods_Slope(slp, _isFill);
            }

            // 平台防护
            var platforms = _slopes.OfType<Platform>().ToList();
            foreach (var plf in platforms)
            {
                plf.ProtectionMethod = SetProtectionMethods_Platform(plf, _isFill);
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
                if (dir > 0.6)
                {
                    return "锚杆网格梁";
                }
            }
            else
            {
                return "浆砌片石";
            }
            return $"{dir.ToString("0.00")} 坡比防护";
        }

        private static string SetProtectionMethods_Platform(Platform plf, bool fill)
        {
            return "平台防护";
        }

        #endregion
    }
}