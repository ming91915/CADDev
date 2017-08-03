using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantityBackup.Cmds;
using eZcad.SubgradeQuantityBackup.DataExport;
using eZcad.SubgradeQuantityBackup.Entities;
using eZcad.SubgradeQuantityBackup.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(InfosGetter_Slope))]

namespace eZcad.SubgradeQuantityBackup.Cmds
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
            
            var psr = ed.GetSelection(pso, SlopeLine.Filter);

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

            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf,sort:true);
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