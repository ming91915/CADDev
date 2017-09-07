using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(InfosGetter_StairExcav))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 横向挖台阶工程量：对于横向边坡坡率位于 1:5 ~ 1:2.5 之间的填方边坡，进行挖台阶处理 </summary>
    [EcDescription(CommandDescription)]
    public class InfosGetter_StairExcav : ICADExCommand
    {

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"StairExcav";
        private const string CommandText = @"横向挖台阶";
        private const string CommandDescription = @"对于横向边坡坡率位于 1:5 ~ 1:2.5 之间的填方边坡，进行挖台阶处理";

        /// <summary> 对于边坡坡率位于 1:5 ~ 1:2.5 之间的填方边坡，进行挖台阶处理 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "StairExcav_32.png")]
        public void StairExcav()
        {
            DocumentModifier.ExecuteCommand(StairExcav);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new InfosGetter_StairExcav();
            return AddinManagerDebuger.DebugInAddinManager(s.StairExcav,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion


        private DocumentModifier _docMdf;
        /// <summary>对于边坡坡率位于 1:5 ~ 1:2.5 之间的填方边坡，进行挖台阶处理 </summary>
        public ExternalCmdResult StairExcav(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            //  ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            var centerLines = ProtectionUtils.SelecteSections(docMdf.acEditor);
            if (centerLines == null || centerLines.Count == 0) return ExternalCmdResult.Cancel;

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
            
            // 将边坡防护数据导出
            var exporter = new Exporter_StairsExcavCross(docMdf, allSections, handledSections);
            exporter.ExportStairCut();
            return ExternalCmdResult.Commit;
        }
    }
}