using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 陡坡路堤工程量 </summary>
    public class Exporter_SteepSlope : DataExporter
    {
        #region --- Types

        private class SteepSlope : HalfValue
        {
            public SectionSide Reinforcement { get; set; }

            /// <summary> 需要挖台阶处理的位置 </summary>
            public SectionSide Treatment { get; set; }

            /// <summary> 挖台阶平均处理宽度，每一个断面的宽度表示挖台阶的左边缘到右边缘的水平距离  </summary>
            public double AverageTreatedWidth { get; set; }

            /// <summary> 此区间中挖台阶处理的平均面积（通过计算每一个小三角形的面积之和求得） </summary>
            public double AverageStairArea { get; set; }

            //

            public override void Merge(IMergeable connectedHalf)
            {
                var conn = (SteepSlope)connectedHalf;
                var dist1 = Math.Abs(ParentStation - EdgeStation);
                var dist2 = Math.Abs(conn.EdgeStation - conn.ParentStation);
                AverageTreatedWidth = (conn.AverageTreatedWidth * dist2 + AverageTreatedWidth * dist1) / (dist1 + dist2);
                AverageStairArea = (conn.AverageStairArea * dist2 + AverageStairArea * dist1) / (dist1 + dist2);
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                var front = next as SteepSlope;
                // 处理位置不同的区间要分开
                if (Treatment != front.Treatment) return false;
                // 有土工格栅与没有土工格栅的区间要分开
                if ((Reinforcement == SectionSide.无 && front.Reinforcement != SectionSide.无)
                    || (Reinforcement != SectionSide.无 && front.Reinforcement == SectionSide.无))
                {
                    return false;
                }
                return true;
            }

            public override void CutByBlock(double blockStation)
            {
                // 啥也不用做
            }
        }

        #endregion

        #region --- Fields

        private static readonly Criterion_SteepFill _criterion = Criterion_SteepFill.UniqueInstance;

        private readonly List<SubgradeSection> _handledSections;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<SteepSlope>> _sortedRanges;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="allSections"></param>
        /// <param name="handledSections"></param>
        public Exporter_SteepSlope(DocumentModifier docMdf, IList<SubgradeSection> allSections,
            List<SubgradeSection> handledSections) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            handledSections.Sort(SQUtils.CompareStation);
            _handledSections = handledSections;
            //
            _sortedRanges = InitializeGeometricRange<SteepSlope>(AllStations);
        }

        /// <summary> 陡坡路堤 </summary>
        public void ExportSteepSlope()
        {
            var steepSlopes = new List<CrossSectionRange<SteepSlope>>();

            // 断面的判断与计算
            double treatedWidth;
            double stairArea;
            SectionSide treatedSide;
            SectionSide reinforcementSide;

            foreach (var sec in _handledSections)
            {
                var isSteep = IsSteepSlope(sec, out treatedSide, out treatedWidth, out stairArea, out reinforcementSide);
                if (isSteep)
                {
                    var hfdc = _sortedRanges[sec.XData.Station];
                    hfdc.BackValue.AverageTreatedWidth = treatedWidth;
                    hfdc.BackValue.Treatment = treatedSide;
                    hfdc.BackValue.Reinforcement = reinforcementSide;
                    hfdc.BackValue.AverageStairArea = stairArea;
                    //
                    hfdc.FrontValue.AverageTreatedWidth = treatedWidth;
                    hfdc.FrontValue.Treatment = treatedSide;
                    hfdc.FrontValue.Reinforcement = reinforcementSide;
                    hfdc.FrontValue.AverageStairArea = stairArea;
                    //
                    steepSlopes.Add(hfdc);
                }
            }
            var countAll = steepSlopes.Count;
            if (countAll == 0)
            {
                _docMdf.WriteNow($"陡坡路堤断面数量：{countAll}");
                return;
            }

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(steepSlopes, Options_Collections.RangeBlocks);

            // 将位于桥梁隧道区间之内的断面移除
            steepSlopes = steepSlopes.Where(r => !r.IsNull).ToList();

            // 对于区间进行合并
            // steepSlopes = MergeLinkedSections(steepSlopes);
            steepSlopes = MergeLinkedSections<SteepSlope>(steepSlopes);

            countAll = steepSlopes.Count;
            _docMdf.WriteNow($"陡坡路堤断面数量：{countAll}");
            if (countAll == 0) return;

            // 将结果整理为二维数组，用来进行表格输出
            var rows = new List<object[]>();
            var header = new object[]
            {"起始桩号", "结束桩号", "桩号区间", "左侧挖台阶", "右侧挖台阶",  "左侧土工格栅", "右侧土工格栅",
                "段落长度","挖台阶平均处理宽度", "挖台阶土方量",
               "土工格栅数量"};
            rows.Add(header);

            for (int i = 0; i < steepSlopes.Count; i++)
            {
                var rg = steepSlopes[i];
                rg.UnionBackFront();
                //
                var reinfCount = 0;  // 土工格栅数量
                if (rg.BackValue.Reinforcement == SectionSide.左 || rg.BackValue.Reinforcement == SectionSide.右)
                {
                    reinfCount = 1;
                }
                else if (rg.BackValue.Reinforcement == SectionSide.全幅)
                {
                    reinfCount = 2;
                }
                var rangeLength = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                rows.Add(new object[]
                {
                    rg.BackValue.EdgeStation,
                    rg.FrontValue.EdgeStation,
                    SQUtils.GetStationString(rg.BackValue.EdgeStation, rg.FrontValue.EdgeStation, maxDigits: 0),
                    // 挖台阶位置
                    (rg.BackValue.Treatment & SectionSide.左)>0 ? SQConstants.CheckMark: null,
                    (rg.BackValue.Treatment & SectionSide.右)>0 ? SQConstants.CheckMark:null,
                    // Enum.GetName(typeof (SectionSide), rg.BackValue.Treatment),
                    
                    // 土工格栅位置
                     (rg.BackValue.Reinforcement & SectionSide.左)>0 ? SQConstants.CheckMark: null,
                     (rg.BackValue.Reinforcement & SectionSide.右)>0 ? SQConstants.CheckMark:null,
                    // Enum.GetName(typeof (SectionSide), rg.BackValue.Reinforcement),

                    rangeLength,
                    //
                    rg.BackValue.AverageTreatedWidth,
                    rg.BackValue.AverageStairArea * rangeLength,
                    //
                    reinfCount,
                });
            }

            var sheetArr = ArrayConstructor.FromList2D(listOfRows: rows);
            // sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { header }, new[] { -1.5f, });

            // 输出到表格
            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.SteepSlope, "陡坡路堤", sheetArr)
            };
            ExportWorkSheetDatas(sheet_Infos);
        }

        #region --- 判断陡坡路堤

        /// <summary> 陡坡路堤 </summary>
        /// <param name="sec"> 道路中心填方高度</param>
        /// <param name="treatedSide"> 哪一侧需要进行陡坡路堤处理 </param>
        /// <param name="treatedWidth"> 陡坡路堤处理宽度 </param>
        /// <param name="stairArea"> 挖台阶的面积 </param>
        /// <param name="reinforcementSide"> 哪一侧需要设置加筋结构，比如铺设三层土工格栅</param>
        /// <returns></returns>
        private bool IsSteepSlope(SubgradeSection sec, out SectionSide treatedSide, out double treatedWidth,
            out double stairArea,
            out SectionSide reinforcementSide)
        {
            bool isSteep = false;
            treatedWidth = 0;
            stairArea = 0;
            double sideTreatedWidth = 0;
            double sideStairArea = 0;
            treatedSide = SectionSide.无;
            reinforcementSide = SectionSide.无;
            bool leftSetReinforcement;
            bool rightSetReinforcement;
            var secData = sec.XData;
            //
            SlopeLine sideSlope = null;
            Polyline sideGround = null;
            if (secData.LeftRetainingWallType != RetainingWallType.路肩墙)
            {
                sideSlope = sec.GetSlopeLine(true); // 断面左侧边坡
                sideGround = secData.LeftGroundSurfaceHandle.GetDBObject<Polyline>(_docMdf.acDataBase);
                if (IsSteepFill(secData, true, sideSlope, sideGround, out sideTreatedWidth, out sideStairArea,
                    out leftSetReinforcement))
                {
                    isSteep = true;
                    treatedSide = treatedSide | SectionSide.左;
                    reinforcementSide = leftSetReinforcement ? reinforcementSide | SectionSide.左 : reinforcementSide;
                    treatedWidth += sideTreatedWidth;
                    stairArea += sideStairArea;
                }
            }
            if (secData.RightRetainingWallType != RetainingWallType.路肩墙)
            {
                sideSlope = sec.GetSlopeLine(false); // 断面右侧边坡
                sideGround = secData.RightGroundSurfaceHandle.GetDBObject<Polyline>(_docMdf.acDataBase);
                if (IsSteepFill(secData, false, sideSlope, sideGround, out sideTreatedWidth, out sideStairArea,
                    out rightSetReinforcement))
                {
                    isSteep = true;
                    treatedSide = treatedSide | SectionSide.右;
                    reinforcementSide = rightSetReinforcement ? reinforcementSide | SectionSide.右 : reinforcementSide;
                    treatedWidth += sideTreatedWidth;
                    stairArea += sideStairArea;
                }
            }
            return isSteep;
        }

        /// <summary>
        /// 判断某一侧边坡是否为陡坡路堤
        /// </summary>
        /// <param name="slp">某一侧边坡，其值可能为null，表示此侧没有边坡线 </param>
        /// <param name="ground">边坡所对应的自然地面线</param>
        /// <param name="treatedWidth"></param>
        /// <param name="setReinforcement">是否要设置加筋结构，比如铺设三层土工格栅</param>
        /// <returns></returns>
        private bool IsSteepFill(SectionInfo sec, bool left, SlopeLine slp, Polyline ground, out double treatedWidth,
            out double stairArea, out bool setReinforcement)
        {
            treatedWidth = 0;
            stairArea = 0;
            setReinforcement = false;
            var slopeFill = (slp == null) || slp.XData.FillCut;

            // ----------------------------------------------------------------------------------------
            // 确定进行搜索的左右边界：路基边缘（或边坡脚） 到 道路中线
            var cGround = ground.Get2dLinearCurve();
            double edgeXleft;
            double edgeXright;
            var succ = sec.GetFillSlopeXRange(left, slp, cGround, _docMdf.acDataBase, out edgeXleft, out edgeXright);
            if (!succ) return false;
            // ----------------------------------------------------------------------------------------
            // ---------此时 [edgeXleft ~ edgeXright] 区间内应该都是填方区域 --------------------------

            //
            var segCounts = (int)Math.Ceiling((edgeXright - edgeXleft) / _criterion.最小迭代宽度); // 其值最小为1
            var xInterval = (edgeXright - edgeXleft) / segCounts;
            var xYs = new List<double[]>();
            var cSlope = slp?.Pline.Get2dLinearCurve();
            for (int i = 0; i <= segCounts; i++)
            {
                var x = edgeXleft + i * xInterval;
                double yGround;
                var inters = new CurveCurveIntersector2d(cGround, new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));
                // 没有交点的情况一般不会出现，因为自然地面线的范围很广
                yGround = inters.NumberOfIntersectionPoints == 0 ? 0 : inters.GetIntersectionPoint(0).Y;

                double ySlope;
                if (cSlope == null) // 表示没有边坡线
                {
                    ySlope = 0;
                }
                else
                {
                    inters = new CurveCurveIntersector2d(cSlope, new Line2d(new Point2d(x, 0), new Vector2d(0, 1)));

                    // 没有交点的情况必然会出现，即x对应路基位置，而不是对应边坡位置，比如当路基下的自然地面线是向路基中心倾斜时。
                    ySlope = inters.NumberOfIntersectionPoints == 0 ? sec.CenterY : inters.GetIntersectionPoint(0).Y;
                }
                //
                xYs.Add(new double[3] { x, yGround, ySlope });
            }

            // 每隔x间距时，自然地面的Y值
            var xyGround = xYs.Select(r => new double[] { r[0], r[1] }).ToList();

            // 开始求斜率（集合中至少有两个元素）。每个元素为三分量向量，分别为 X 坐标、X对应的自然地面的Y坐标，X对应的边坡的Y坐标
            double maxRatio = 0;
            double maxYFill;

            var lastX = xYs[0];
            maxYFill = lastX[2] - lastX[1];
            for (int i = 1; i < xYs.Count; i++)
            {
                var thisX = xYs[i];
                maxRatio = Math.Max(maxRatio, Math.Abs((thisX[1] - lastX[1]) / (thisX[0] - lastX[0])));
                maxYFill = Math.Max(maxYFill, Math.Abs(thisX[2] - thisX[1]));
                //
                lastX = thisX;
            }
            // 判断是否为陡坡路堤
            if (maxRatio >= (1 / _criterion.陡坡坡比))
            {
                // 道路中心与边坡均为填方
                treatedWidth = edgeXright - edgeXleft;
                stairArea = CalculateStairArea(xyGround, edgeXleft, edgeXright);

                if (slopeFill && treatedWidth > 0 && maxYFill >= _criterion.加筋体对应填方段最小高度)
                {
                    setReinforcement = true;
                }
                return true;
            }
            //
            return false;
        }

        #endregion

        /// <summary>
        /// 计算横断面中，填方区的三角形台阶面积
        /// </summary>
        /// <param name="xyGround">集合中起码有两个元素，其中每一个元素都代表一个包含两个数值的向量，
        /// 向量中第一个元素代表地面线的某点的几何X坐标（横断面中的此X没有物理意义，纵断面中的此X一般代表纵断面中的桩号），第二个元素代表此点的Y坐标</param>
        /// <param name="xLeft"></param>
        /// <param name="xRight"></param>
        /// <returns></returns>
        public static double CalculateStairArea(List<double[]> xyGround, double xLeft, double xRight)
        {
            if (xyGround.Count < 2) return 0.0;
            var stairArea = 0.0;
            double x1;
            double x2;
            for (int i = 0; i < xyGround.Count - 1; i++)
            {
                x2 = xyGround[i + 1][0];
                x1 = xyGround[i][0];
                if (x1 >= xLeft && x2 <= xRight)
                {
                    // 相邻两个X之差代表台阶的宽度
                    var dx = x2 - x1;
                    var dy = xyGround[i + 1][1] - xyGround[i][1];
                    stairArea += Math.Abs(dx * dy / 2);
                }
            }
            return stairArea;
        }
    }
}