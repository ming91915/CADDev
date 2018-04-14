using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    public partial class PF_SeperateByElev : ModalPForm
    {
        private SelectionSet _impliedSelection;

        #region ---   窗口的构造、打开与关闭

        private static PF_SeperateByElev _uniqueInstance;

        public static PF_SeperateByElev GetUniqueInstance(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _uniqueInstance = _uniqueInstance ?? new PF_SeperateByElev();
            _uniqueInstance._docMdf = docMdf;
            _uniqueInstance._impliedSelection = impliedSelection;
            //
            return _uniqueInstance;
        }

        private PF_SeperateByElev() : base()
        {
            InitializeComponent();
            //
            comboBox_cutMethods.DataSource = Enum.GetValues(typeof(CutMethod));
        }

        #endregion

        protected override void OnCmdRun(bool closeWindow)
        {
            base.OnCmdRun(closeWindowWhenFinished: true);

            List<SlopeLine> selectedSlopes;
            // 左右侧
            if (radioButton_left.Checked)
            {
                selectedSlopes = SQUtils.SelecteExistingSlopeLines(_docMdf, left: true, sort: true);
            }
            else if (radioButton_right.Checked)
            {
                selectedSlopes = SQUtils.SelecteExistingSlopeLines(_docMdf, left: false, sort: true);
            }
            else
            {
                selectedSlopes = SQUtils.SelecteExistingSlopeLines(_docMdf, left: null, sort: true);
            }
            if (selectedSlopes == null || selectedSlopes.Count == 0) return;
            var handledSlopes = selectedSlopes.ToArray();

            // 区分填挖
            if (radioButton_cut.Checked)
            {
                handledSlopes = selectedSlopes.Where(r => !r.XData.FillCut).ToArray();
            }
            else if (radioButton_fill.Checked)
            {
                handledSlopes = selectedSlopes.Where(r => r.XData.FillCut).ToArray();
            }
            if (handledSlopes.Length == 0) return;

            // 剪切方法
            var method = (CutMethod)comboBox_cutMethods.SelectedItem;

            // 执行操作
            CutSlopes(_docMdf, handledSlopes, method, textBoxNum_cutElev.ValueNumber,
                textBoxNum_waterLineLength.ValueNumber);
        }

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        private void CutSlopes(DocumentModifier docMdf, SlopeLine[] selectedSlopes, CutMethod method, double elev,
            double waterLineLength)
        {
            // ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            //
            //var layer_Slope = Utils.GetOrCreateLayer(docMdf, ProtectionConstants.LayerName_ProtectionMethod_Slope);
            var layer_WaterLine = eZcad.SubgradeQuantity.Utility.SQUtils.GetOrCreateLayer_WaterLine(docMdf);
            //var layer_Platform = Utils.GetOrCreateLayer(docMdf, ProtectionConstants.LayerName_ProtectionMethod_Platform);
            var es = EditStateIdentifier.GetCurrentEditState(docMdf);
            es.CurrentBTR.UpgradeOpen();

            //
            Point2d intersPt;
            Slope intersSlopeSeg;
            int cutCount = 0;
            docMdf.WriteLineIntoDebuger("被分割的边坡对象：");
            var protLayers = ProtectionTags.MapProtectionLayers(_docMdf, selectedSlopes);
            foreach (var slp in selectedSlopes)
            {
                var inters = ValidateElevation(slp, method, elev, out intersPt, out intersSlopeSeg);
                if (inters)
                {
                    // 将子边坡按指定的交点进行剪切
                    var newSegs = CutSlopeSegment(slp.XData, intersSlopeSeg, new Point3d(intersPt.X, intersPt.Y, 0));
                    slp.XData.Slopes = newSegs;

                    // 将被剪切掉的子边坡所绑定的文字删除
                    SlopeLine.EraseText(intersSlopeSeg, docMdf.acDataBase);

                    // 绘制水位线
                    var line = new Line(new Point3d(intersPt.X - waterLineLength / 2, intersPt.Y, 0),
                        new Point3d(intersPt.X + waterLineLength / 2, intersPt.Y, 0));
                    line.LayerId = layer_WaterLine.Id;
                    es.CurrentBTR.AppendEntity(line);
                    docMdf.acTransaction.AddNewlyCreatedDBObject(line, true);
                    slp.XData.Waterlines.Add(line.Handle);

                    // 将刷新后的数据保存到 AutoCAD 文档与界面中
                    slp.Pline.UpgradeOpen();
                    SlopeConstructor.SetSlopeUI(slp);
                    slp.PrintProtectionMethod(es.CurrentBTR, protLayers);
                    slp.FlushXData();
                    slp.Pline.DowngradeOpen();
                    //
                    cutCount += 1;
                    docMdf.WriteLineIntoDebuger(slp);
                }
            }
            es.CurrentBTR.DowngradeOpen();
            docMdf.WriteLineIntoDebuger($"总计：{cutCount} 个");
        }

        /// <summary> 检查边坡是否与指定的标高相交，并返回交点在 AutoCAD 中的几何坐标 </summary>
        /// <param name="slp"></param>
        /// <param name="ele"></param>
        /// <param name="intersPt">返回交点</param>
        /// <returns></returns>
        private static bool ValidateElevation(SlopeLine slp, CutMethod method, double ele, out Point2d intersPt,
            out Slope slopeSeg)
        {
            intersPt = Point2d.Origin;
            slopeSeg = null;
            var slpData = slp.XData;
            var sec = slp.Section;
            var cutY = GetCutY(slpData, sec, method, ele); //  sec.GetYFromElev(ele); // 指定的标高在此断面中所对应的 AutoCAD 中的坐标Y值
            const double tolerance = 0.01;
            foreach (var seg in slpData.Slopes)
            {
                bool within = (seg.TopPoint.Y - cutY >= tolerance) && (cutY - seg.BottomPoint.Y >= tolerance);
                if (within) // 说明此子边坡与指定的标高相交
                {
                    var l = new LineSegment2d(new Point2d(seg.BottomPoint.X, seg.BottomPoint.Y),
                        new Point2d(seg.TopPoint.X, seg.TopPoint.Y));
                    var intersPts = l.IntersectWith(new Line2d(new Point2d(0, cutY), new Vector2d(1, 0)));
                    if (intersPts != null)
                    {
                        slopeSeg = seg;
                        intersPt = intersPts[0];
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary> 将子边坡按指定的交点进行剪切，并返回剪切后的新的子边坡集合 </summary>
        /// <param name="data"></param>
        /// <param name="intersSeg"></param>
        /// <param name="intersPt"></param>
        public List<Slope> CutSlopeSegment(SlopeData data, Slope intersSeg, Point3d intersPt)
        {
            var slopeSegs = data.Slopes;
            var newSegs = new List<Slope>();
            int mainLevel = intersSeg.GetMainLevel();
            // 1. 先添加交叉边坡之前的子边坡
            int sumBefore = 0;
            for (int id = 0; id < slopeSegs.Count; id++)
            {
                var slp = slopeSegs[id];
                if (slp.GetMainLevel() != mainLevel)
                {
                    newSegs.Add(slp);
                    sumBefore += 1;
                }
                else
                {
                    break;
                }
            }
            // 2. 对交叉处的子边坡进行处理
            var sumMainLevel = slopeSegs.Count(r => r.GetMainLevel() == mainLevel); // 此子边坡中已经有多少个更细的子边坡
            int subLevelInters = intersSeg.GetSubLevel();
            // 分割后，最内侧的子边坡的Index为 n.1,最外侧的子边坡的Index为 n.9,
            int subL = 1;
            for (int i = 0; i < sumMainLevel; i++)
            {
                // 此循环的过程中改写有一个子边坡与指定的相交边坡是同一个边坡
                var seg = slopeSegs[sumBefore + i];
                if (seg.GetSubLevel() == subLevelInters)
                {
                    var seg1 = new Slope(Slope.GetIndex(mainLevel, subL), intersSeg.InnerPoint, intersPt);
                    var seg9 = new Slope(Slope.GetIndex(mainLevel, subL + 1), intersPt, intersSeg.OuterPoint);
                    SetProtectionMethod(intersSeg, seg1, seg9);
                    //
                    newSegs.Add(seg1);
                    newSegs.Add(seg9);
                    subL += 2;
                }
                else
                {
                    seg.SetIndex(mainLevel, subL);
                    newSegs.Add(seg);
                    subL += 1;
                }
            }
            // 修改最后一个的Index
            // newSegs[sumBefore + sumMainLevel].SetIndex(mainLevel, 9);

            // 3. 接着添加交叉边坡之后的子边坡
            for (int i = sumBefore + sumMainLevel; i < slopeSegs.Count; i++)
            {
                newSegs.Add(slopeSegs[i]);
            }
            return newSegs;
        }

        /// <summary> 为分割后的边坡设置防护形式 </summary>
        /// <param name="src"></param>
        /// <param name="innerSlp"></param>
        /// <param name="outerSlp"></param>
        private void SetProtectionMethod(Slope src, Slope innerSlp, Slope outerSlp)
        {
            innerSlp.ProtectionMethod = src.ProtectionMethod;
            outerSlp.ProtectionMethod = src.ProtectionMethod;
        }


        private enum CutMethod
        {
            标高,
            从坡顶向下,
            从坡底向上,
            从中心向外
        }

        private static double GetCutY(SlopeData slp, SubgradeSection sec, CutMethod method, double value)
        {
            double cutY = 0;
            switch (method)
            {
                case CutMethod.从中心向外:
                    if (slp.FillCut)
                    {
                        cutY = sec.GetYFromElev(slp.TopElevation - value);
                        break;
                    }
                    else
                    {
                        cutY = sec.GetYFromElev(slp.BottomElevation + value);
                        break;
                    }
                case CutMethod.从坡底向上:
                    cutY = sec.GetYFromElev(slp.BottomElevation + value);
                    break;
                case CutMethod.从坡顶向下:
                    cutY = sec.GetYFromElev(slp.TopElevation - value);
                    break;
                default: // 按绝对标高
                    cutY = sec.GetYFromElev(value);
                    break;
            }
            return cutY;
        }
    }
}