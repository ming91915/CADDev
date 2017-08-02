using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (SlopeConstructor))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary>
    /// 创建边坡并设置每一个边坡的数据
    /// </summary>
    public class SlopeConstructor
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "ConstructSlopes";

        /// <summary> 创建边坡并设置每一个边坡的数据 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(@"创建边坡"), Description("创建边坡并设置每一个边坡的数据")
        , RibbonItem(@"创建边坡", "创建边坡并设置每一个边坡的数据", ProtectionConstants.ImageDirectory + "ConstructSlopes_32.png")]
        public void ConstructSlopes()
        {
            DocumentModifier.ExecuteCommand(ConstructSlopes);
        }

        /// <summary> 创建边坡并设置每一个边坡的数据 </summary>
        public void ConstructSlopes(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);
            var justModifyCalculatedSlopes = ModifyOrAdd(docMdf.acEditor);
            var slopeLines = ProtectionUtils.GetSlopeLines(docMdf.acEditor);
            if (slopeLines != null && slopeLines.Count > 0)
            {
                ConfigerSlopes(slopeLines, justModifyCalculatedSlopes);
            }
        }

        #endregion

        /// <summary> 是要添加边坡线 还是 对已有边坡线进行修改 </summary>
        private static bool ModifyOrAdd(Editor ed)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n构造边坡数据<添加>[修改(M) / 添加(A)]:",
                globalKeywords: "修改 添加");
            //           
            op.AllowNone = true;
            op.AllowArbitraryInput = false;
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                if (res.StringResult == "修改")
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary> 根据边坡多段线进行道路边坡的筛选、信息提取、防护方式的设置等操作 </summary>
        public void ConfigerSlopes(IList<Polyline> slopeLines, bool justModifyCalculated)
        {
            if (slopeLines == null || slopeLines.Count == 0) return;
            //
            var app = Utils.GetOrCreateAppName(_docMdf.acDataBase, _docMdf.acTransaction, SlopeData.AppName);

            var selectedSlpLines = new List<SlopeLine>();
            string errMsg;
            foreach (var sl in slopeLines)
            {
                var slpLine = SlopeLine.Create(_docMdf, sl, out errMsg);
                if (slpLine != null)
                {
                    // 将存储的数据导入边坡对象
                    slpLine.ImportSlopeData(slpLine.XData);
                    //
                    if (!slpLine.XData.FullyCalculated
                        || !justModifyCalculated)
                    {
                        slpLine.CalculateXData();
                    }
                    selectedSlpLines.Add(slpLine);
                }
                else
                {
                    _docMdf.WriteNow(errMsg);
                }
            }
            if (selectedSlpLines.Count == 0) return;

            var slopesWithSlopesegs = new List<SlopeLine>();
            var slopesWithoutSlopesegs = new List<SlopeLine>();
            foreach (var slp in selectedSlpLines)
            {
                if (slp.XData.Slopes.Count + slp.XData.Platforms.Count > 0)
                {
                    slopesWithSlopesegs.Add(slp);
                }
                else
                {
                    slopesWithoutSlopesegs.Add(slp);
                }
            }
            //
            var es = EditStateIdentifier.GetCurrentEditState(_docMdf);
            es.CurrentBTR.UpgradeOpen();
            var layer_Slope = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Slope);
            var layer_Platform = Utils.GetOrCreateLayer(_docMdf, ProtectionConstants.LayerName_ProtectionMethod_Platform);
            _docMdf.acDataBase.Clayer = layer_Slope.ObjectId;
            //
            // 1. 对有子边坡的边坡对象进行操作：显示界面，以进行填挖方与防护设置
            var listerForm = new SlopeSegLister(slopesWithSlopesegs);
            listerForm.ShowDialog(null);
            if (listerForm.ValueChanged)
            {
                foreach (var slp in slopesWithSlopesegs)
                {
                    slp.Pline.UpgradeOpen();
                    SetSlopeUI(slp);
                    slp.PrintProtectionMethod(es.CurrentBTR, layer_Slope.Id, layer_Platform.Id);
                    //
                    slp.FlushXData();
                    slp.Pline.DowngradeOpen();

                    if (slp.XDataToBeCleared)
                    {
                        slp.ClearProtectionMethodText();
                        slp.ClearXData();
                    }
                }
            }
            // 2. 对没有子边坡的边坡对象进行操作
            foreach (var slp in slopesWithoutSlopesegs)
            {
                slp.Pline.UpgradeOpen();
                SetSlopeUI(slp);
                slp.FlushXData();
                slp.Pline.DowngradeOpen();
            }

            es.CurrentBTR.DowngradeOpen();
        }

        /// <summary> 对边坡线的显示样式进行设置 </summary>
        /// <param name="slp"></param>
        private void SetSlopeUI(SlopeLine slp)
        {
            if (slp.XData.FillCut)
            {
                // 填方边坡
                // slp.CenterLine.DrawWaterLevel(WaterLevel, waterLevelLayer.Id); // 绘制水位线
                slp.Pline.ColorIndex = 2; // 黄色
                slp.Pline.LineWeight = LineWeight.LineWeight070;
            }
            else
            {
                // 挖方
                slp.Pline.ColorIndex = 3; // 绿色
                slp.Pline.LineWeight = LineWeight.LineWeight070;
            }
        }
    }
}