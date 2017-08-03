using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantityBackup.Redundant;
using eZcad.SubgradeQuantityBackup.RedundantCmds;
using eZcad.SubgradeQuantityBackup.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (SlpInfosSetter))]

namespace eZcad.SubgradeQuantityBackup.RedundantCmds
{
    /// <summary> 创建边坡并设置每一个边坡的数据 </summary>
    internal class SlpInfosSetter
    {
        #region --- 命令设计

        public SlpInfosSetter()
        {
        }

        /// <summary> 设置每一个边坡的数据 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "SetSlopeProtection", CommandFlags.UsePickSet)
        , DisplayName(@"创建边坡2"), Description("创建边坡并设置每一个边坡的数据")]
        public void EcSetSlopeProtection()
        {
            DocumentModifier.ExecuteCommand(SetSlopeProtection);
        }

        /// <summary> 设置每一个边坡的数据 </summary>
        public static void SetSlopeProtection(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var modifyExistedSlopes = ModifyOrAdd(docMdf.acEditor);
            IList<Polyline> slopeLines = GetSlopeLines(docMdf.acEditor, modifyExistedSlopes);
            if (slopeLines != null && slopeLines.Count > 0)
            {
                var sps = new SlpInfosSetter(docMdf);
                sps.ConfigerSlopes(slopeLines);
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
        /// <param name="onlyExisted">过滤出包含<see cref="SlopeDataBackup.AppName"/>的线条</param>
        /// <returns></returns>
        public static List<Polyline> GetSlopeLines(Editor ed, bool onlyExisted)
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
            var acTypValAr = SlopeLineBackup.SlopeLineFilter;

            if (onlyExisted)
            {
                var lst = new List<TypedValue>(acTypValAr)
                {
                    new TypedValue((int) DxfCode.ExtendedDataRegAppName, SlopeDataBackup.AppName)
                };
                acTypValAr = lst.ToArray();
            }
            // 将过滤条件赋值给SelectionFilter对象
            var acSelFtr = new SelectionFilter(acTypValAr);

            ed.SelectionAdded += EdOnSelectionAdded;
            // Finally run the selection and show any results
            var psr = ed.GetSelection(pso, acSelFtr);
            ed.SelectionAdded -= EdOnSelectionAdded;

            if (psr.Status == PromptStatus.OK)
            {
                var pls = new List<Polyline>();
                foreach (var id in psr.Value.GetObjectIds())
                {
                    pls.Add(id.GetObject(OpenMode.ForRead) as Polyline);
                }
                return pls;
            }
            return null;
        }

        private static SelectionSet _selectedSlopes;

        private static void EdOnSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            // _docMdf.WriteNow("\n", e.AddedObjects.Count, e.Selection.Count);
            _selectedSlopes = e.Selection;
        }

        #endregion

        #endregion

        private readonly DocumentModifier _docMdf;

        /// <summary> 构造函数 </summary>
        public SlpInfosSetter(DocumentModifier docMdf)
        {
            _docMdf = docMdf;
        }

        /// <summary> 根据边坡多段线进行道路边坡的筛选、信息提取、防护方式的设置等操作 </summary>
        public void ConfigerSlopes(IList<Polyline> slopeLines)
        {
            if (slopeLines == null || slopeLines.Count == 0) return;
            //
            ValidateAppName(_docMdf, SlopeDataBackup.AppName);

            List<SlopeLineBackup> slpLines = new List<SlopeLineBackup>();
            foreach (var sl in slopeLines)
            {
                var slpLine = SlopeLineBackup.Create(_docMdf, sl);
                if (slpLine != null)
                {
                    if (Math.Abs(slpLine.XData.SlopeLength) > 1e-6) // 说明确实是一条边坡线
                    {
                        // 将不受用户操作影响的相关数据写入 XData 中
                        slpLines.Add(slpLine);
                    }
                }
            }

            if (slpLines.Count == 0) return;

            // 显示界面，以进行填挖方与防护设置
            var listerForm = new ProtectionStyleLister(slpLines);
            listerForm.ShowDialog(null);
            //
            if (listerForm.ValueChanged)
            {
                var waterLevelLayer = GetLayer(_docMdf, ProtectionOptions.LayerName_WaterLevel);
                // 将数据存入 AutoCAD 中
                foreach (var slp in slpLines)
                {
                    if (slp.XData != null && slp.XData.SlopeLength > 0)
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
                        slp.Pline.LineWeight = LineWeight.ByLayer;
                        slp.Pline.ColorIndex = 2;
                    }
                }
            }
        }

        #region --- 基本图层等环境配置

        private void ValidateAppName(DocumentModifier docMdf, string appName)
        {
            var apptable = docMdf.acDataBase.RegAppTableId.GetObject(OpenMode.ForWrite) as RegAppTable;

            // RegAppTableRecord 的创建
            if (!apptable.Has(appName))
            {
                var app1 = new RegAppTableRecord() {Name = appName,};
                apptable.Add(app1);
                docMdf.acTransaction.AddNewlyCreatedDBObject(app1, true);
            }
        }

        /// <summary> 如果数据库中有此图层，则直接返回，如果没有，则创建对应图层 </summary>
        /// <param name="docMdf"></param>
        /// <param name="layerName"></param>
        public static LayerTableRecord GetLayer(DocumentModifier docMdf, string layerName)
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