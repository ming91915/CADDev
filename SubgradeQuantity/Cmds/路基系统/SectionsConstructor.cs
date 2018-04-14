using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
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
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "Section_32.png")]
        public void ConstructSections()
        {
            DocumentModifier.ExecuteCommand(ConstructSections);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SectionsConstructor();
            return SQAddinManagerDebuger.DebugInAddinManager(s.ConstructSections,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统 </summary>
        public ExternalCmdResult ConstructSections(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            SQUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 在界面中选择指定的断面
            // var axisLines = ProtectionUtils.SelecteSectionLines(docMdf.acEditor);
            // 直接提取整个文档中所有的断面
            var axisLines = SQUtils.GetAllSectionLines(docMdf.acEditor);

            if (axisLines != null && axisLines.Count > 0)
            {
                // 先检查以前是否进行过横断面构造，免得由于误点击而将原来的数据删除
                bool hasConstructed = true;
                //
                if (hasConstructed)
                {
                    var res = MessageBox.Show("是否要重新构造横断面系统？\r\n此操作会覆盖以前的横断面系统数据！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.No)
                    {
                        return ExternalCmdResult.Cancel;
                    }
                }

                var f = new SectionsConstructorForm(docMdf, axisLines);
                f.ShowDialog();
                // 
                var stations = f.SectionAxes.Select(r => r.XData.Station).ToArray();
                //检查是否有重复的桩号出现
                var duplicated = stations.GroupBy(x => x).Where(g => g.Count() > 1).ToArray();
                if (duplicated.Length > 0)
                {
                    MessageBox.Show("路基系统中出现重复的桩号，请核对后再操作！\r\n重复的桩号见命令行提示。", "提示", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    docMdf.WriteNow("重复的桩号,重复的次数");
                    foreach (var d in duplicated)
                    {
                        docMdf.WriteNow(d.Key, d.Count());
                    }
                }
                else
                {
                    // 将桩号从小到大排序
                    Array.Sort(stations);
                    Options_Collections.AllSortedStations = stations;
                    DbXdata.FlushXData(docMdf, DbXdata.DatabaseXdataType.AllSortedStations);
                }
            }
            else
            {
                MessageBox.Show("未找到有效的横断面轴线，请核对后再操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return ExternalCmdResult.Commit;
        }

    }
}