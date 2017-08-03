using System;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (InfosGetter_FillCutInters))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
    public class InfosGetter_FillCutInters
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "ExportFillCutIntersections";

        /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"填挖交界"), Description("将所有的填挖交界信息提取出来并制成相应表格")
        , RibbonItem(@"填挖交界", "将所有的填挖交界信息提取出来并制成相应表格", ProtectionConstants.ImageDirectory + "ExportFillCutIntersections_32.png")]
        public void ExportFillCutIntersections()
        {
            DocumentModifier.ExecuteCommand(ExportFillCutIntersections);
        }

        #endregion

        /// <summary> 将所有的填挖交界信息提取出来并制成相应表格 </summary>
        public void ExportFillCutIntersections(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            throw new NotImplementedException();
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf, sort: true);
            var slopeLines = ProtectionUtils.GetExistingSlopeLines(docMdf);

            // 过滤掉没有实际边坡或者平台的对象（比如边坡与挡墙重合的）
            slopeLines = slopeLines.Where(r => r.XData.Slopes.Count + r.XData.Platforms.Count > 0).ToList();
            if (slopeLines.Count == 0) return;

            // 将边坡防护数据导出
            var exporter = new Exporter_SlopeProtection(docMdf, allSections, slopeLines);
            exporter.ExportData();
        }
    }
}