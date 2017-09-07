using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(SlopeSegMerge))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 将分割开的多个子边坡进行合并 </summary>
    [EcDescription(CommandDescription)]
    public class SlopeSegMerge : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"MergeSlopeSegs";
        private const string CommandText = @"缝合边坡";
        private const string CommandDescription = @"将分割开的多个子边坡进行合并";

        /// <summary> 将分割开的多个子边坡进行合并 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName,
            ProtectionConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "SegMerge_32.png")]
        public void MergeSlopeSegs()
        {
            DocumentModifier.ExecuteCommand(MergeSlopeSegs);
        }
        
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeSegMerge();
            return AddinManagerDebuger.DebugInAddinManager(s.MergeSlopeSegs,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 将分割开的多个子边坡进行合并 </summary>
        public ExternalCmdResult MergeSlopeSegs(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);
            //
            var selectedSlopes = ProtectionUtils.SelecteExistingSlopeLines(docMdf, left: null, sort: true);
            if (selectedSlopes == null || selectedSlopes.Count == 0) return ExternalCmdResult.Cancel;
            //
            MergeProtectionDirection dir;
            var succ = GetProtectionDirection(docMdf, out dir);
            if (!succ) return ExternalCmdResult.Cancel;
            //
            //var layer_Slope = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Slope);
            //var layer_WaterLine = Utils.GetOrCreateLayer_WaterLine(_docMdf);
            //var layer_Platform = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Platform);
            var es = EditStateIdentifier.GetCurrentEditState(_docMdf);
            es.CurrentBTR.UpgradeOpen();

            bool haveSegsMerged = false;
            int mergedCount = 0;
            docMdf.WriteLineIntoDebuger("合并的边坡对象：");

            var protLayers = ProtectionTags.MapProtectionLayers(_docMdf, selectedSlopes);

            foreach (var slp in selectedSlopes)
            {
                haveSegsMerged = MergeSlope(slp.XData, dir);
                if (haveSegsMerged)
                {
                    // 将刷新后的数据保存到 AutoCAD 文档与界面中
                    slp.Pline.UpgradeOpen();
                    //
                    slp.ClearAllWaterlines(_docMdf.acDataBase);
                    SlopeConstructor.SetSlopeUI(slp);
                    slp.PrintProtectionMethod(es.CurrentBTR, protLayers);
                    slp.FlushXData();
                    //
                    slp.Pline.DowngradeOpen();
                    mergedCount += 1;
                    docMdf.WriteLineIntoDebuger(slp);
                }
            }
            docMdf.WriteLineIntoDebuger($"总计：{mergedCount} 个");
            //
            return ExternalCmdResult.Commit;
        }

        /// <summary> 将一侧边坡中的所有子边坡进行缝合 </summary>
        /// <param name="slpDt"></param>
        /// <param name="dir"></param>
        /// <returns>如果成功缝合，则返回 true，如果此侧边坡中原来就没有被分割，则返回 false</returns>
        private bool MergeSlope(SlopeData slpDt, MergeProtectionDirection dir)
        {
            bool merged = false;
            var newSlopes = new List<Slope>();
            var lastSubSlopeSegs = new List<Slope>();
            if (slpDt.Slopes.Count > 0)
            {
                int lastMainLevel = slpDt.Slopes[0].GetMainLevel();
                foreach (var s in slpDt.Slopes)
                {
                    if (s.GetMainLevel() != lastMainLevel)
                    {
                        // 先处理上一级子边坡中的所有更细子边坡
                        if (lastSubSlopeSegs.Count > 0)
                        {
                            var mergedSlope = MergeSubSlopes(lastSubSlopeSegs, slpDt.FillCut, dir);
                            merged = true;
                            newSlopes.Add(mergedSlope);
                            lastSubSlopeSegs = new List<Slope>();
                        }
                        // 
                        lastMainLevel = s.GetMainLevel();
                    }
                    if (s.GetSubLevel() != 0)
                    {
                        SlopeLine.EraseText(s, _docMdf.acDataBase);
                        lastSubSlopeSegs.Add(s);
                    }
                    else
                    {
                        // 将本级子边坡添加进来
                        newSlopes.Add(s);
                    }
                }
                // 处理最后一级的多个细子边坡
                if (lastSubSlopeSegs.Count > 0)
                {
                    var mergedSlope = MergeSubSlopes(lastSubSlopeSegs, slpDt.FillCut, dir);
                    merged = true;
                    newSlopes.Add(mergedSlope);
                }
            }
            slpDt.Slopes = newSlopes;
            return merged;
        }

        /// <summary> 将一级边坡中的所有子边坡进行合并 </summary>
        /// <param name="subSlopes">某一级边坡中的所有子边坡集合</param>
        /// <param name="fill"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private Slope MergeSubSlopes(List<Slope> subSlopes, bool fill, MergeProtectionDirection dir)
        {
            var innerS = subSlopes[0];
            var outerS = subSlopes[subSlopes.Count - 1];
            var s = new Slope(innerS.GetMainLevel(), innerS.InnerPoint, outerS.OuterPoint);
            switch (dir)
            {
                case MergeProtectionDirection.顶部:
                    s.ProtectionMethod = fill ? innerS.ProtectionMethod : outerS.ProtectionMethod;
                    break;
                case MergeProtectionDirection.底部:
                    s.ProtectionMethod = fill ? outerS.ProtectionMethod : innerS.ProtectionMethod;
                    break;
                case MergeProtectionDirection.靠近中线:
                    s.ProtectionMethod = innerS.ProtectionMethod;
                    break;
                case MergeProtectionDirection.远离中线:
                    s.ProtectionMethod = outerS.ProtectionMethod;
                    break;
                default:
                    s.ProtectionMethod = null;
                    break;
            }
            return s;
        }

        /// <summary>
        /// 同一级边坡中的多个细子边坡进行合并时，合并后的这级边坡的防护方式的选择方式
        /// </summary>
        private enum MergeProtectionDirection
        {
            /// <summary> 取消防护 </summary>
            无 = 0,

            /// <summary> 标高位置更低 </summary>
            底部,
            顶部,
            靠近中线,
            远离中线,
        }


        /// <summary> 从多个选项中选择一个 </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示按顶点缩放（默认值），false 表示按长度缩放</returns>
        private static bool GetProtectionDirection(DocumentModifier docMdf, out MergeProtectionDirection dir)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n合并后的防护方式选择 <底部>[无(N) / 底部(B) / 顶部(T) / 靠近中线(I) / 远离中线(O)]",
                globalKeywords: "无 底部 顶部 靠近中线 远离中线"); // 默认值写在前面
            op.AllowArbitraryInput = false;
            op.AllowNone = true;

            var res = docMdf.acEditor.GetKeywords(op);

            dir = MergeProtectionDirection.底部;
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                switch (res.StringResult)
                {
                    case "顶部": dir = MergeProtectionDirection.顶部; break;
                    case "靠近中线": dir = MergeProtectionDirection.靠近中线; break;
                    case "远离中线": dir = MergeProtectionDirection.远离中线; break;
                    case "无": dir = MergeProtectionDirection.无; break;
                    default: dir = MergeProtectionDirection.底部; break;
                }
                return true;
            }
            else if (res.Status == PromptStatus.Cancel)
            {
                return false;
            }
            return true;
        }

    }
}