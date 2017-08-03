using System;
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

[assembly: CommandClass(typeof(InfosGetter_Slope))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取并计算边坡防护工程量 </summary>
    public class InfosGetter_Slope
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "ExportSlopeInfos";

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"边坡防护"), Description("将所有的边坡信息提取出来并制成相应表格")
        , RibbonItem(@"边坡防护", "将所有的边坡信息提取出来并制成相应表格", ProtectionConstants.ImageDirectory + "DataExport_32.png")]
        public void ExportSlopeInfos()
        {
            DocumentModifier.ExecuteCommand(ExportSlopeInfos);
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