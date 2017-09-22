using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.ParameterForm;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof(SectionsConstructor))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary>
    /// 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统
    /// </summary>
    [EcDescription(CommandDescription)]
    public class SectionsConstructor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "ConstructSections";

        private const string CommandText = @"路基构造";
        private const string CommandDescription = @"根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统";


        /// <summary> 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "Section_32.png")]
        public void ConstructSections()
        {
            DocumentModifier.ExecuteCommand(ConstructSections);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SectionsConstructor();
            return AddinManagerDebuger.DebugInAddinManager(s.ConstructSections,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统 </summary>
        public ExternalCmdResult ConstructSections(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 在界面中选择指定的断面
            // var axes = ProtectionUtils.SelecteSectionLines(docMdf.acEditor);
            // 直接提取整个文档中所有的断面
            var axes = ProtectionUtils.GetAllSectionLines(docMdf.acEditor);

            if (axes != null && axes.Count > 0)
            {
                //
                var f = new SectionsConstructorForm(docMdf, axes);
                f.ShowDialog();
                // 
                Options_Collections.AllSortedStations = f.SectionAxes.Select(r => r.XData.Station).ToArray();
                Array.Sort(Options_Collections.AllSortedStations);
                DbXdata.FlushXData(docMdf, DbXdata.DatabaseXdataType.AllSortedStations);
            }
            return ExternalCmdResult.Commit;
        }
    }
}