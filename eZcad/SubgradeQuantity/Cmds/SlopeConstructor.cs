using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (SlopeConstructor))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 创建边坡并设置每一个边坡的数据 </summary>
    public class SlopeConstructor
    {
        #region --- 命令设计

        /// <summary> 设置每一个边坡的数据 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ConstructSlopes", CommandFlags.UsePickSet)
        , DisplayName(@"创建边坡"), Description("创建边坡并设置每一个边坡的数据")]
        public void EcConstructSlopes()
        {
            DocumentModifier.ExecuteCommand(ConstructSlopes);
        }


        /// <summary> 设置每一个边坡的数据 </summary>
        public static void ConstructSlopes(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);
            var justModifyCalculatedSlopes = ModifyOrAdd(docMdf.acEditor);
            var slopeLines = GetSlopeLines(docMdf.acEditor);
            if (slopeLines != null && slopeLines.Count > 0)
            {
                var sc = new SlopeConstructor(docMdf);
                sc.ConfigerSlopes(slopeLines, justModifyCalculatedSlopes);
            }
        }

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

        #region ---   从界面中选择可能的边坡线

        /// <summary> 从界面中搜索边坡线 </summary>
        /// <param name="ed"></param>
        /// <returns></returns>
        public static List<Polyline> GetSlopeLines(Editor ed)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择多条边坡线 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n选择多条边坡线 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            // pso.SingleOnly = true;

            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Left_Cut),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Right_Cut),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Left_Fill),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_Slope_Right_Fill),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            // 将过滤条件赋值给SelectionFilter对象
            var acSelFtr = new SelectionFilter(filterType);

            var psr = ed.GetSelection(pso, acSelFtr);

            if (psr.Status == PromptStatus.OK)
            {
                return psr.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead) as Polyline).ToList();
            }
            return null;
        }

        #endregion

        #endregion

        private readonly DocumentModifier _docMdf;

        /// <summary> 构造函数 </summary>
        public SlopeConstructor(DocumentModifier docMdf)
        {
            _docMdf = docMdf;
        }

        /// <summary> 根据边坡多段线进行道路边坡的筛选、信息提取、防护方式的设置等操作 </summary>
        public void ConfigerSlopes(IList<Polyline> slopeLines, bool justModifyCalculated)
        {
            if (slopeLines == null || slopeLines.Count == 0) return;
            //
            var app = Utils.GetOrCreateAppName(_docMdf.acDataBase, _docMdf.acTransaction, SlopeDataBackup.AppName);

            var slpLines = new List<SlopeLine>();
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
                    slpLines.Add(slpLine);
                }
                else
                {
                    _docMdf.WriteNow(errMsg);
                }
            }
            // 过滤掉没有实际边坡或者平台的对象（比如边坡与挡墙重合的）
            slpLines = slpLines.Where(r => r.XData.Slopes.Count + r.XData.Platforms.Count > 0).ToList();
            if (slpLines.Count == 0) return;

            // 显示界面，以进行填挖方与防护设置
            var listerForm = new SlopesegLister(slpLines);
            listerForm.ShowDialog(null);
            //
            if (listerForm.ValueChanged)
            {
                //
                foreach (var slp in slpLines)
                {
                    if (slp.XData != null)
                    {
                        slp.Pline.UpgradeOpen();
                        if (slp.XData.FillExcav)
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
                        slp.FlushXData();
                        slp.Pline.DowngradeOpen();
                    }
                    if (slp.XDataToBeCleared)
                    {
                        slp.ClearXData();
                    }
                }
            }
        }

        #region --- 基本图层等环境配置

        private LayerTableRecord GetSlopeLayer(DocumentModifier docMdf, string layerName)
        {
            LayerTable layers =
                docMdf.acTransaction.GetObject(docMdf.acDataBase.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (layers.Has(layerName))
            {
                return layers[layerName].GetObject(OpenMode.ForRead) as LayerTableRecord;
            }
            else
            {
                var ltr = new LayerTableRecord();
                ltr.Name = layerName;
                //
                layers.UpgradeOpen();
                layers.Add(ltr);
                layers.DowngradeOpen();
                docMdf.acTransaction.AddNewlyCreatedDBObject(ltr, true);
                return ltr;
            }
        }

        #endregion
    }
}