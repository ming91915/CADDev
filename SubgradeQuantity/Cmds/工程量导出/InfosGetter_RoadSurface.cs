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

[assembly: CommandClass(typeof (InfosGetter_RoadSurface))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 对于横断面坡度过陡的断面进行判断，并设置土工格栅 </summary>
    [EcDescription(CommandDescription)]
    public class InfosGetter_RoadSurface : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"RoadSurface";

        private const string CommandText = @"路面工程";
        private const string CommandDescription = @"路面工程数量表";

        /// <summary> 对于横断面坡度过陡的断面进行判断，并设置土工格栅 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "RoadSurface_32.png")]
        public void RoadSurface()
        {
            DocumentModifier.ExecuteCommand(RoadSurface);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new InfosGetter_RoadSurface();
            return AddinManagerDebuger.DebugInAddinManager(s.RoadSurface,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将所有的边坡信息提取出来并制成相应表格 </summary>
        public ExternalCmdResult RoadSurface(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            //  ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            var centerLines = ProtectionUtils.SelecteSectionLines(docMdf.acEditor);
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
            var exporter = new Exporter_RoadSurface(docMdf, allSections, handledSections);
            exporter.ExportRoadSurface();

            return ExternalCmdResult.Commit;
        }
    }
}