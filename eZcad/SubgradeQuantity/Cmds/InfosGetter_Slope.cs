using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (InfosGetter_Slope))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取并计算边坡防护工程量 </summary>
    public class InfosGetter_Slope
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ExportSlopeInfos", CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"边坡表格"), Description("将所有的边坡信息提取出来并制成相应表格")]
        public void EcExportSlopeInfos()
        {
            DocumentModifier.ExecuteCommand(ExportSlopeInfos);
        }

        #endregion

        #region ---   从界面中选择可能的边坡线

        /// <summary> 从界面中搜索边坡线 </summary>
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

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        public void ExportSlopeInfos(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            var slopeLines = GetSlopeLines(docMdf.acEditor);
            if (slopeLines == null || slopeLines.Count == 0) return;
            //
            var slpLines = new List<SlopeLine>();
            string errMsg = null;
            foreach (var sl in slopeLines)
            {
                var slpLine = SlopeLine.Create(_docMdf, sl, out errMsg);
                if (slpLine != null)
                {
                    slpLine.ImportSlopeData(slpLine.XData);
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

            // 
            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf);
            var allMileages = allSections.Select(r => r.XData.Mileage).ToArray();

            // 将边坡防护数据导出
            var exporter = new Exporter_SlopeProtection(docMdf, slpLines, allMileages);
            exporter.ExportData();
        }
    }
}