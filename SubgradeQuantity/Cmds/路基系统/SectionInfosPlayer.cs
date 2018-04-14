using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using Utils = eZstd.Miscellaneous.Utils;

[assembly: CommandClass(typeof(SectionInfosPlayer))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取所有的横断面的相关信息 </summary>
    [EcDescription(CommandDescription)]
    public class SectionInfosPlayer : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "AllSectionsInfo";
        private const string CommandText = @"导出断面";
        private const string CommandDescription = @"导出所有横断面的信息";

        /// <summary> 提取所有的横断面的相关信息 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet),
         DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "SectionInfos_32.png")]
        public void AllSectionsInfo()
        {
            DocumentModifier.ExecuteCommand(AllSectionsInfo);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SectionInfosPlayer();
            return SQAddinManagerDebuger.DebugInAddinManager(s.AllSectionsInfo,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 提取所有的横断面的相关信息 </summary>
        public ExternalCmdResult AllSectionsInfo(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var allSections = SQUtils.GetAllSections(docMdf, sort: true);
            _docMdf.WriteNow($"\n找到{allSections.Length}个横断面对象！\n");
            ExportSectionInfoToText(allSections);
            return ExternalCmdResult.Commit;
        }

        private void ExportSectionInfoToText(SubgradeSection[] allSections)
        {
            if (allSections.Length > 0)
            {
                var infoPath = Utils.ChooseSaveFile("数据输出的文本", "文本(*.csv) | *.csv");
                if (infoPath == null) return;
                //
                var methods = new List<dynamic>();
                var properties = typeof(SectionInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var fields = typeof(SectionInfo).GetFields(BindingFlags.Public | BindingFlags.Instance);
                methods.AddRange(properties);
                methods.AddRange(fields);
                methods.Sort(SortPropertiesByName);
                //
                var sb = new StringBuilder();
                // 添加表头
                foreach (var f in methods)
                {
                    sb.Append(f.Name + ",");
                }
                sb.AppendLine();

                // 添加数据行
                foreach (var ss in allSections)
                {
                    var xdata = ss.XData;
                    foreach (var f in methods)
                    {
                        string strValue = null;
                        var v = f.GetValue(xdata);
                        if (v is double)
                        {
                            strValue = v.ToString("0.###");
                        }
                        else if (v is Handle)
                        {
                            strValue = "'" + ((Handle)v).ToString();
                        }
                        else if (v == null)
                        {
                            strValue = "NULL";
                        }
                        else
                        {
                            strValue = v.ToString();
                        }
                        sb.Append($"{strValue},");
                    }
                    sb.AppendLine();
                }

                using (var sw = new StreamWriter(infoPath))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
        }

        private int SortPropertiesByName(dynamic prop1, dynamic prop2)
        {
            string n1 = prop1.Name;
            string n2 = prop2.Name;
            return n1.CompareTo(n2);
        }
    }
}