using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantities.Cmds;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;
using eZstd.Enumerable;

[assembly: CommandClass(typeof (LongitudinalSectionDrawer))]

namespace eZcad.SubgradeQuantities.Cmds
{
    /// <summary> 沿着道路纵向绘制边坡线 </summary>
    public class LongitudinalSectionDrawer
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 沿着道路纵向绘制边坡线 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "DrawLongitudinalSection", CommandFlags.UsePickSet)
        , DisplayName(@"绘制纵断面"), Description("沿着道路纵向绘制边坡线")]
        public void EcDrawLongitudinalSection()
        {
            DocumentModifier.ExecuteCommand(DrawLongitudinalSection);
        }


        /// <summary> 沿着道路纵向绘制边坡线 </summary>
        public void DrawLongitudinalSection(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);
            var allSections = ProtectionUtils.GetAllSections(docMdf,sort:true);
            if (allSections.Length > 1)
            {
                //
                var allStations = allSections.Select(r => r.XData.Station).ToArray();
                var minStation = allStations.First();

                //
                // 每一个桩号所对应的某一侧的挡墙顶与挡墙底相对于道路中线的高度，如果某桩号中没有挡墙，则其值为 null
                var leftRetainingWalls = new Dictionary<double, double[]>();
                var rightRetainingWalls = new Dictionary<double, double[]>();
                var leftSlps = ConstructLeftSlope(allSections, true, out leftRetainingWalls);
                var rightSlps = ConstructLeftSlope(allSections, false, out rightRetainingWalls);

                // 选择绘图的基点
                Point3d p3;
                var succ = GetBasePt(_docMdf, out p3);
                if (!succ) return;

                var basePt2D = new Point2d(p3.X, p3.Y);
                //
                double rx = 0.25;
                double ry = 2;
                //
                var ss = EditStateIdentifier.GetCurrentEditState(_docMdf);
                ss.CurrentBTR.UpgradeOpen();
                var layer_Slope = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_LongitudinalSlopes);
                docMdf.acDataBase.Clayer = layer_Slope.Id;

                // 绘制中轴线
                var roadCenterPl = CreatePolyline(allStations, new double[allStations.Length], basePt2D, minStation, rx,
                    ry);
                ss.CurrentBTR.AppendEntity(roadCenterPl);
                _docMdf.acTransaction.AddNewlyCreatedDBObject(roadCenterPl, true);
                // 绘制桩号
                var txts = CreateDbTexts(allStations, new double[allStations.Length], basePt2D, minStation, rx, ry);
                foreach (var txt in txts)
                {
                    ss.CurrentBTR.AppendEntity(txt);
                    _docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);
                }
                // 绘制左边坡线
                var leftPl = CreatePolyline(leftSlps.Keys.ToArray(), leftSlps.Values.ToArray(), basePt2D, minStation, rx,
                    ry);
                leftPl.LineWeight = LineWeight.LineWeight013;
                leftPl.Color = Color.FromColor(System.Drawing.Color.Green);
                ss.CurrentBTR.AppendEntity(leftPl);
                _docMdf.acTransaction.AddNewlyCreatedDBObject(leftPl, true);
                // 绘制右边坡线
                var rightPl = CreatePolyline(rightSlps.Keys.ToArray(), rightSlps.Values.ToArray(), basePt2D, minStation,
                    rx, ry);
                rightPl.LineWeight = LineWeight.LineWeight013;
                rightPl.Color = Color.FromColor(System.Drawing.Color.Red);
                ss.CurrentBTR.AppendEntity(rightPl);
                _docMdf.acTransaction.AddNewlyCreatedDBObject(rightPl, true);

                // 绘制左边挡墙
                var leftRetainingWallRanges = GetRetainingWallRanges(leftRetainingWalls);
                foreach (var rwr in leftRetainingWallRanges)
                {
                    var pl = CreatePolyline(ArrayConstructor.GetColumn(rwr, 0), ArrayConstructor.GetColumn(rwr, 1),
                        basePt2D, minStation, rx, ry);
                    pl.Closed = true;
                    pl.Color = Color.FromColor(System.Drawing.Color.LightGreen);
                    pl.LineWeight = LineWeight.LineWeight050;
                    ss.CurrentBTR.AppendEntity(pl);
                    _docMdf.acTransaction.AddNewlyCreatedDBObject(pl, true);
                }
                // 绘制右边挡墙
                var rightRetainingWallRanges = GetRetainingWallRanges(rightRetainingWalls);
                foreach (var rwr in rightRetainingWallRanges)
                {
                    var pl = CreatePolyline(ArrayConstructor.GetColumn(rwr, 0), ArrayConstructor.GetColumn(rwr, 1),
                        basePt2D, minStation, rx, ry);
                    pl.Closed = true;
                    pl.Color = Color.FromColor(System.Drawing.Color.DeepPink);
                    pl.LineWeight = LineWeight.LineWeight050;
                    ss.CurrentBTR.AppendEntity(pl);
                    _docMdf.acTransaction.AddNewlyCreatedDBObject(pl, true);
                }
                //
                ss.CurrentBTR.DowngradeOpen();
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allSections"></param>
        /// <param name="left"></param>
        /// <param name="retainingWalls">每一个桩号所对应的某一侧的挡墙顶与挡墙底相对于道路中线的高度，如果某桩号中没有挡墙，则其值为 null</param>
        /// <returns>每一个桩号所对应的开口线相对于道路中线的高度。</returns>
        private Dictionary<double, double> ConstructLeftSlope(SubgradeSection[] allSections, bool left,
            out Dictionary<double, double[]> retainingWalls)
        {
            var slps = new Dictionary<double, double>();
            retainingWalls = new Dictionary<double, double[]>();
            foreach (var sec in allSections)
            {
                var xdata_Section = sec.XData;
                if (left)
                {
                    // 边坡
                    if (xdata_Section.LeftSlopeExists)
                    {
                        var spline = sec.GetSlopeLine(true);
                        var xdata = spline.XData;
                        var height = 0.0;
                        if (xdata.Slopes.Any())
                        {
                            height = xdata.Slopes.Last().OuterPoint.Y - xdata_Section.CenterY;
                        }
                        else
                        {
                            var edgeElevation = xdata.FillCut ? xdata.BottomElevation : xdata.TopElevation;
                            height = edgeElevation - xdata_Section.CenterElevation_Road;
                        }
                        slps.Add(xdata_Section.Station, height);
                    }
                    else
                    {
                        slps.Add(xdata_Section.Station, 0);
                    }
                    // 挡墙
                    var retainingWall = xdata_Section.LeftRetainingWallExists
                        ? xdata_Section.LeftRetainingWallHandle.GetDBObject<Polyline>(sec.DocMdf.acDataBase)
                        : null;
                    if (retainingWall != null)
                    {
                        var rtw = new RetainingWall(retainingWall);
                        var topY = rtw.GetTopY();
                        var bottomY = rtw.GetBottomY();
                        retainingWalls.Add(xdata_Section.Station,
                            new double[] {topY - xdata_Section.CenterY, bottomY - xdata_Section.CenterY});
                    }
                    else
                    {
                        retainingWalls.Add(xdata_Section.Station, null);
                    }
                }
                else
                {
                    if (xdata_Section.RightSlopeExists)
                    {
                        var spline = sec.GetSlopeLine(false);
                        var xdata = spline.XData;

                        var height = 0.0;
                        if (spline.XData.Slopes.Any())
                        {
                            height = spline.XData.Slopes.Last().OuterPoint.Y - xdata_Section.CenterY;
                        }
                        else
                        {
                            var edgeElevation = xdata.FillCut ? xdata.BottomElevation : xdata.TopElevation;
                            height = edgeElevation - xdata_Section.CenterElevation_Road;
                        }
                        slps.Add(xdata_Section.Station, height);
                    }
                    else
                    {
                        slps.Add(xdata_Section.Station, 0);
                    }
                    // 挡墙
                    var retainingWall = xdata_Section.RightRetainingWallExists
                        ? xdata_Section.RightRetainingWallHandle.GetDBObject<Polyline>(sec.DocMdf.acDataBase)
                        : null;
                    if (retainingWall != null)
                    {
                        var rtw = new RetainingWall(retainingWall);
                        var topY = rtw.GetTopY();
                        var bottomY = rtw.GetBottomY();
                        retainingWalls.Add(xdata_Section.Station,
                            new double[] {topY - xdata_Section.CenterY, bottomY - xdata_Section.CenterY});
                    }
                    else
                    {
                        retainingWalls.Add(xdata_Section.Station, null);
                    }
                }
            }
            return slps;
        }

        private bool GetBasePt(DocumentModifier docMdf, out Point3d pickedPoint3D)
        {
            var op = new PromptPointOptions("选择一个基点")
                ;
            var res = docMdf.acEditor.GetPoint(op);
            if (res.Status == PromptStatus.OK)
            {
                pickedPoint3D = res.Value;
                return true;
            }
            pickedPoint3D = new Point3d();
            return false;
        }

        private Polyline CreatePolyline(double[] srcX, double[] srcY, Point2d basePt, double minStation, double rx,
            double ry)
        {
            // 创建一条有两段的多段线   Create a polyline with two segments (3 points)
            var acPoly = new Polyline();
            for (int i = 0; i < srcX.Length; i++)
            {
                var pt = new Point2d((srcX[i] - minStation)*rx + basePt.X, (srcY[i] - 0)*ry + basePt.Y);
                acPoly.AddVertexAt(i, pt, 0, startWidth: 0, endWidth: 0);
            }
            return acPoly;
        }

        private DBText[] CreateDbTexts(double[] allStations, double[] srcY, Point2d basePt, double minStation, double rx,
            double ry)
        {
            var texts = new DBText[allStations.Length];
            for (int i = 0; i < allStations.Length; i++)
            {
                var pt = new Point2d((allStations[i] - minStation)*rx + basePt.X, (srcY[i] - 0)*ry + basePt.Y);
                var txt = new DBText();

                txt.TextString = allStations[i].ToString("0.###");
                txt.Height = 0.5;
                txt.Rotation = Math.PI/2;
                txt.Position = new Point3d(pt.X, pt.Y, 0);
                texts[i] = txt;
            }
            return texts;
        }

        private List<double[,]> GetRetainingWallRanges(Dictionary<double, double[]> retainingWalls)
        {
            var res = new List<double[,]>();
            bool rtStart = false;
            var XYs_top = new Dictionary<double, double>();
            var XYs_bottom = new Dictionary<double, double>();
            foreach (var lrt in retainingWalls)
            {
                if (lrt.Value != null)
                {
                    rtStart = true;
                    XYs_top.Add(lrt.Key, lrt.Value[0]);
                    XYs_bottom.Add(lrt.Key, lrt.Value[1]);
                }
                else
                {
                    if (rtStart)
                    {
                        // 获得挡墙段
                        var xYs = new double[XYs_top.Count*2, 2];
                        int i = 0;
                        foreach (var key in XYs_top.Keys)
                        {
                            xYs[i, 0] = key;
                            xYs[i, 1] = XYs_top[key];
                            i += 1;
                        }
                        foreach (var key in XYs_bottom.Keys.Reverse())
                        {
                            xYs[i, 0] = key;
                            xYs[i, 1] = XYs_bottom[key];
                            i += 1;
                        }
                        res.Add(xYs);
                        //
                        XYs_top = new Dictionary<double, double>();
                        XYs_bottom = new Dictionary<double, double>();
                        rtStart = false;
                    }
                }
            }
            return res;
        }
    }
}