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

[assembly: CommandClass(typeof(InfosGetter_ThinFill))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取并计算边坡防护工程量 </summary>
    public class InfosGetter_ThinFill
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ExportThinFill", CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"低填浅挖"), Description("提取低填浅挖工程数量表")]
        public void EcExportSlopeInfos()
        {
            DocumentModifier.ExecuteCommand(ExportThinFill);
        }

        #endregion

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        public void ExportThinFill(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            var centerLines = GetSections(docMdf.acEditor);
            if (centerLines == null || centerLines.Count == 0) return;
            //

            // 
            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf);
            // var allMileages = allSections.Select(r => r.XData.Mileage).ToArray();

            // 要处理的断面
            var handledSections = new List<SubgradeSection>();
            string errMsg = null;
            foreach (var sl in centerLines)
            {
                var info = SectionInfo.FromCenterLine(sl);
                // 方式一：重新构造
                // var axis = new CenterAxis(docMdf, sl, info);
                
                // 方式二：从总集合中索引
                var axis = allSections.FirstOrDefault(r => r.XData.Mileage == info.Mileage);

                if (axis != null)
                {
                    handledSections.Add(axis);
                }
            }
            //
            // 过滤 

            // 将边坡防护数据导出
            var exporter = new Exporter_ThinFillShallowCut(docMdf, handledSections, allSections);
            exporter.ExportThinFill();
        }


        #region ---   从界面中选择可能的道路中线轴线

        /// <summary> 从界面中搜索边坡线 </summary>
        /// <returns></returns>
        public static List<Line> GetSections(Editor ed)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择要提取的横断面轴线 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n选择要提取的横断面轴线 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            // pso.SingleOnly = true;

            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_CenterAxis),
            };

            // 将过滤条件赋值给SelectionFilter对象
            var acSelFtr = new SelectionFilter(filterType);

            var psr = ed.GetSelection(pso, acSelFtr);

            if (psr.Status == PromptStatus.OK)
            {
                return psr.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead) as Line).ToList();
            }
            return null;
        }

        #endregion
    }
}