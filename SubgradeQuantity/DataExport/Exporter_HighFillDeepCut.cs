using System;
using System.Collections.Generic;
using System.Linq;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 高填深挖工程量 </summary>
    public class Exporter_HighFillDeepCut : DataExporter
    {
        #region --- Types

        private class HighFillDeepCut : HalfValue
        {
            public double MaxCenterHeight { get; set; }
            public double MaxSlopeHeight { get; set; }
            //

            public override void Merge(IMergeable connectedHalf)
            {
                MaxCenterHeight = Math.Max(((HighFillDeepCut)connectedHalf).MaxCenterHeight, MaxCenterHeight);
                MaxSlopeHeight = Math.Max(((HighFillDeepCut)connectedHalf).MaxSlopeHeight, MaxSlopeHeight);
            }

            /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
            /// <param name="next">与本区间紧密相连的下一个区间</param>
            public override bool IsMergeable(IMergeable next)
            {
                return true;
            }

            public override void CutByBlock(double blockStation)
            {

                // 啥也不用做
            }

        }

        #endregion

        #region --- Fields

        private static readonly Criterion_HighFillDeepCut _criterion = Criterion_HighFillDeepCut.UniqueInstance;

        private readonly List<SubgradeSection> _handledSections;

        /// <summary> 整个道路中选择的断面的左边边坡对象，小桩号位于集合前面。
        /// 集合中包括了所有的桩号，但是对应桩号下的边坡对象可能为null，即此桩号下无对应边坡 </summary>
        private readonly SlopeLine[] _handledLeftSlopes;

        /// <summary> 整个道路中选择的断面的右边边坡对象，小桩号位于集合前面。
        /// 集合中包括了所有的桩号，但是对应桩号下的边坡对象可能为null，即此桩号下无对应边坡 </summary>
        private readonly SlopeLine[] _handledRightSlopes;

        /// <summary> 整个道路中所有断面所占的几何区间， 以及对应的初始化的工程量数据 </summary>
        private readonly SortedDictionary<double, CrossSectionRange<HighFillDeepCut>> _sortedRanges;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="allSections"></param>
        /// <param name="handledSections"></param>
        public Exporter_HighFillDeepCut(DocumentModifier docMdf, IList<SubgradeSection> allSections,
            List<SubgradeSection> handledSections) : base(docMdf, allSections.Select(r => r.XData.Station).ToArray())
        {
            handledSections.Sort(ProtectionUtils.CompareStation);
            _handledSections = handledSections;
            _handledLeftSlopes = handledSections.Select(r => r.GetSlopeLine(true)).ToArray();
            _handledRightSlopes = handledSections.Select(r => r.GetSlopeLine(false)).ToArray();
            //
            _sortedRanges = InitializeGeometricRange<HighFillDeepCut>(AllStations);
        }

        /// <summary> 低填浅挖 </summary>
        public void ExportHighFillDeepCut()
        {
            var highFillSections_Left = new List<CrossSectionRange<HighFillDeepCut>>();
            var deepCutSections_Left = new List<CrossSectionRange<HighFillDeepCut>>();
            var highFillSections_Right = new List<CrossSectionRange<HighFillDeepCut>>();
            var deepCutSections_Right = new List<CrossSectionRange<HighFillDeepCut>>();


            // 断面的判断与计算
            double centerHight;
            double slopeHight;

            foreach (var slp in _handledLeftSlopes)
            {
                if (IsHighFill(slp, out centerHight, out slopeHight))
                {
                    var hfdc = _sortedRanges[slp.Station];
                    hfdc.BackValue.MaxCenterHeight = centerHight;
                    hfdc.BackValue.MaxSlopeHeight = slopeHight;
                    hfdc.FrontValue.MaxCenterHeight = centerHight;
                    hfdc.FrontValue.MaxSlopeHeight = slopeHight;
                    //
                    highFillSections_Left.Add(hfdc);
                }
                else if (IsDeepCut(slp, out centerHight, out slopeHight))
                {
                    var hfdc = _sortedRanges[slp.Station];
                    hfdc.BackValue.MaxCenterHeight = centerHight;
                    hfdc.BackValue.MaxSlopeHeight = slopeHight;
                    hfdc.FrontValue.MaxCenterHeight = centerHight;
                    hfdc.FrontValue.MaxSlopeHeight = slopeHight;
                    //
                    deepCutSections_Left.Add(hfdc);
                }
            }
            foreach (var slp in _handledRightSlopes)
            {
                if (IsHighFill(slp, out centerHight, out slopeHight))
                {
                    var hfdc = _sortedRanges[slp.Station];
                    hfdc.BackValue.MaxCenterHeight = centerHight;
                    hfdc.BackValue.MaxSlopeHeight = slopeHight;
                    hfdc.FrontValue.MaxCenterHeight = centerHight;
                    hfdc.FrontValue.MaxSlopeHeight = slopeHight;
                    //
                    highFillSections_Right.Add(hfdc);
                }
                else if (IsDeepCut(slp, out centerHight, out slopeHight))
                {
                    var hfdc = _sortedRanges[slp.Station];
                    hfdc.BackValue.MaxCenterHeight = centerHight;
                    hfdc.BackValue.MaxSlopeHeight = slopeHight;
                    hfdc.FrontValue.MaxCenterHeight = centerHight;
                    hfdc.FrontValue.MaxSlopeHeight = slopeHight;
                    //
                    deepCutSections_Right.Add(hfdc);
                }
            }

            var countAll = highFillSections_Left.Count + deepCutSections_Left.Count + highFillSections_Right.Count +
                           deepCutSections_Right.Count;
            if (countAll == 0)
            {
                _docMdf.WriteNow($"高填深挖断面数量：{countAll}");
                return;
            }
            

            // 对桥梁隧道结构进行处理：截断对应的区间
            CutWithBlocks(highFillSections_Left, Options_Collections.RangeBlocks);
            CutWithBlocks(deepCutSections_Left, Options_Collections.RangeBlocks);
            CutWithBlocks(highFillSections_Right, Options_Collections.RangeBlocks);
            CutWithBlocks(deepCutSections_Right, Options_Collections.RangeBlocks);


            // 将位于桥梁隧道区间之内的断面移除
            highFillSections_Left = highFillSections_Left.Where(r => !r.IsNull).ToList();
            deepCutSections_Left = deepCutSections_Left.Where(r => !r.IsNull).ToList();
            highFillSections_Right = highFillSections_Right.Where(r => !r.IsNull).ToList();
            deepCutSections_Right = deepCutSections_Right.Where(r => !r.IsNull).ToList();
            
            // 对于区间进行合并
            highFillSections_Left = MergeLinkedSections(highFillSections_Left);
            highFillSections_Right = MergeLinkedSections(highFillSections_Right);
            deepCutSections_Left = MergeLinkedSections(deepCutSections_Left);
            deepCutSections_Right = MergeLinkedSections(deepCutSections_Right);

            //
            countAll = highFillSections_Left.Count + deepCutSections_Left.Count + highFillSections_Right.Count +
                       deepCutSections_Right.Count;
            _docMdf.WriteNow($"高填深挖断面数量：{countAll}");
            if (countAll == 0) return;

            // 将结果整理为二维数组，用来进行表格输出
            var sheetArr = new object[countAll + 2, 6];
            // 高填部分
            int baseRow = 0;
            for (int i = 0; i < highFillSections_Left.Count; i++)
            {
                var rg = highFillSections_Left[i];
                rg.UnionBackFront();
                sheetArr[baseRow + i, 0] = rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 1] = rg.FrontValue.EdgeStation;
                sheetArr[baseRow + i, 2] = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 3] = null;
                sheetArr[baseRow + i, 4] = rg.BackValue.MaxCenterHeight;
                sheetArr[baseRow + i, 5] = rg.BackValue.MaxSlopeHeight;
            }
            baseRow += highFillSections_Left.Count;
            for (int i = 0; i < highFillSections_Right.Count; i++)
            {
                var rg = highFillSections_Right[i];
                rg.UnionBackFront();
                sheetArr[baseRow + i, 0] = rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 1] = rg.FrontValue.EdgeStation;
                sheetArr[baseRow + i, 2] = null;
                sheetArr[baseRow + i, 3] = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 4] = rg.BackValue.MaxCenterHeight;
                sheetArr[baseRow + i, 5] = rg.BackValue.MaxSlopeHeight;
            }
            // 深挖部分
            baseRow += highFillSections_Right.Count;
            for (int i = 0; i < deepCutSections_Left.Count; i++)
            {
                var rg = deepCutSections_Left[i];
                rg.UnionBackFront();
                sheetArr[baseRow + i, 0] = rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 1] = rg.FrontValue.EdgeStation;
                sheetArr[baseRow + i, 2] = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 3] = null;
                sheetArr[baseRow + i, 4] = rg.BackValue.MaxCenterHeight;
                sheetArr[baseRow + i, 5] = rg.BackValue.MaxSlopeHeight;
            }
            baseRow += deepCutSections_Left.Count;
            for (int i = 0; i < deepCutSections_Right.Count; i++)
            {
                var rg = deepCutSections_Right[i];
                rg.UnionBackFront();
                sheetArr[baseRow + i, 0] = rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 1] = rg.FrontValue.EdgeStation;
                sheetArr[baseRow + i, 2] = null;
                sheetArr[baseRow + i, 3] = rg.FrontValue.EdgeStation - rg.BackValue.EdgeStation;
                sheetArr[baseRow + i, 4] = rg.BackValue.MaxCenterHeight;
                sheetArr[baseRow + i, 5] = rg.BackValue.MaxSlopeHeight;
            }
            // 插入表头
            var headerFill = new string[] { "起始桩号", "结束桩号", "左", "右", "中心最大填方高度", "路堤最大填方高度" };
            var headerCut = new string[] { "起始桩号", "结束桩号", "左", "右", "中心最大挖方高度", "路堑边坡最大高度" };
            sheetArr = sheetArr.InsertVector<object, string, object>(true, new[] { headerFill, headerCut },
                new[] { -1.5f, highFillSections_Left.Count + highFillSections_Right.Count - 1 });
            // 输出到表格
            var sheet_Infos = new List<WorkSheetData>
            {
                new WorkSheetData(WorkSheetDataType.HighFillDeepCut, "高填深挖", sheetArr)
            };
            ExportWorkSheetDatas(sheet_Infos);
        }

        #region --- 判断高填与深挖

        /// <summary> 高填路堤 </summary>
        /// <param name="slp"></param>
        /// <param name="centerHight"> 道路中心填方高度</param>
        /// <param name="slopeHight"> 边坡高度</param>
        /// <returns></returns>
        public static bool IsHighFill(SlopeLine slp, out double centerHight, out double slopeHight)
        {
            centerHight = 0;
            slopeHight = 0;
            if (slp == null) return false;
            var data = slp.XData;
            if (!data.FillCut) return false;
            //
            slopeHight = data.TopElevation - data.BottomElevation;
            if (slopeHight < _criterion.填方最低高度) return false;
            //

            var secData = slp.Section.XData;
            centerHight = secData.CenterElevation_Road - secData.CenterElevation_Ground;
            return true;
        }

        /// <summary> 深挖路堤 </summary>
        /// <param name="slp"></param>
        /// <param name="centerHight"> 道路中心挖方高度</param>
        /// <param name="slopeHight"> 边坡高度</param>
        /// <returns></returns>
        public static bool IsDeepCut(SlopeLine slp, out double centerHight, out double slopeHight)
        {
            centerHight = 0;
            slopeHight = 0;
            if (slp == null) return false;
            var data = slp.XData;
            if (data.FillCut) return false;
            //
            slopeHight = data.TopElevation - data.BottomElevation;
            if (data.SoilOrRock == SubgradeType.土质 && slopeHight < _criterion.土质挖方最低高度)
            {
                return false;
            }
            else if (data.SoilOrRock == SubgradeType.岩质 && slopeHight < _criterion.岩质挖方最低高度)
            {
                return false;
            }
            //

            var secData = slp.Section.XData;
            centerHight = secData.CenterElevation_Ground - secData.CenterElevation_Road;
            return true;
        }

        #endregion

        ///// <summary> 将多个断面区间进行合并 </summary>
        ///// <param name="selectedSlopes">路基某一侧的高填或深挖边坡对象</param>
        ///// <returns></returns>
        //private List<CrossSectionRange<HighFillDeepCut>> MergeLinkedSections(
        //    List<CrossSectionRange<HighFillDeepCut>> selectedSlopes)
        //{
        //    if (selectedSlopes.Count == 0) return selectedSlopes;

        //    var res = new List<CrossSectionRange<HighFillDeepCut>>();
        //    var lastRange = selectedSlopes[0];
        //    res.Add(lastRange);
        //    for (int i = 1; i < selectedSlopes.Count; i++)
        //    {
        //        var rg = selectedSlopes[i];
        //        var succ = lastRange.TryMerge(rg);
        //        if (!succ)
        //        {
        //            res.Add(rg);
        //            lastRange = rg;
        //        }
        //    }
        //    return res;
        //}
    }
}