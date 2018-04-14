using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof(InfosGetter_Slope))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取并计算边坡防护工程量 </summary>
    [EcDescription(CommandDescription)]
    public class InfosGetter_Slope : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ExportSlopeInfos";
        private const string CommandText = @"边坡防护";
        private const string CommandDescription = @"将所有的边坡信息提取出来并制成相应表格";

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName,
            SQConstants.ModelState | CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "DataExport_32.png")]
        public void ExportSlopeInfos()
        {
            DocumentModifier.ExecuteCommand(ExportSlopeInfos);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var sp = new InfosGetter_Slope();
            return SQAddinManagerDebuger.DebugInAddinManager(sp.ExportSlopeInfos,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        public ExternalCmdResult ExportSlopeInfos(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            SQUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 所有的断面
            var allSections = SQUtils.GetAllSections(docMdf, sort: true);
            var slopeLines = SQUtils.SelecteExistingSlopeLines(docMdf, left: null, sort: true);

            // 过滤掉没有实际边坡或者平台的对象（比如边坡与挡墙重合的）
            slopeLines = slopeLines.Where(r => r.XData.Slopes.Count + r.XData.Platforms.Count > 0).ToList();
            if (slopeLines.Count == 0) return ExternalCmdResult.Cancel;

            // 将边坡防护数据导出
            var exporter = new Exporter_SlopeProtection(docMdf, allSections, slopeLines);
            exporter.ExportData();
            return ExternalCmdResult.Commit;
        }
    }
}