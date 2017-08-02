using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantities.Cmds;
using eZcad.SubgradeQuantities.DataExport;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(InfosGetter_ThinFill))]

namespace eZcad.SubgradeQuantities.Cmds
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

            var centerLines = ProtectionUtils.GetSections(docMdf.acEditor);
            if (centerLines == null || centerLines.Count == 0) return;

            // 
            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf, sort: true);
            // var allStations = allSections.Select(r => r.XData.Station).ToArray();

            // 要处理的断面
            var handledSections = new List<SubgradeSection>();
            string errMsg = null;
            foreach (var sl in centerLines)
            {
                var info = SectionInfo.FromCenterLine(sl);
                // 方式一：重新构造
                // var axis = new CenterAxis(docMdf, sl, info);

                // 方式二：从总集合中索引
                var axis = allSections.FirstOrDefault(r => r.XData.Station == info.Station);

                if (axis != null)
                {
                    handledSections.Add(axis);
                }
            }
            //
            // 过滤 

            // 将边坡防护数据导出
            var exporter = new Exporter_ThinFillShallowCut(docMdf, handledSections, allSections);
            exporter.ExportThinFillShallowCut();
        }


        #region ---   从界面中选择可能的道路中线轴线


        #endregion
    }
}