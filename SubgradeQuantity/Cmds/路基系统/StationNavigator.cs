using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(StationNavigator))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary>
    /// 在 AutoCAD 界面中快速导航到指定的桩号
    /// </summary>
    public class StationNavigator
    {
        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "NavigateStation";

        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 在 AutoCAD 界面中快速导航到指定的桩号 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(@"断面导航"), Description("在 AutoCAD 界面中快速导航到指定的桩号")
            , RibbonItem(@"断面导航", "在 AutoCAD 界面中快速导航到指定的桩号", SQConstants.ImageDirectory + "Navigator_32.png")]
        public void NavigateStation()
        {
            DocumentModifier.ExecuteCommand(NavigateStation);
        }

        /// <summary> 在 AutoCAD 界面中快速导航到指定的桩号 </summary>
        public ExternalCmdResult NavigateStation(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;

            SubgradeSection matchedSection = null;
            bool? start;
            var wantedStation = SetStation(docMdf.acEditor, out start);

            // 所有的断面
            var allSections = SQUtils.GetAllSections(docMdf, sort: true);
            if (allSections != null && allSections.Length > 0)
            {

                if (start.HasValue)
                {
                    // 匹配起始或者结尾桩号
                    if (start.Value)
                    {
                        matchedSection = allSections[0];
                    }
                    else
                    {
                        matchedSection = allSections[allSections.Length - 1];
                    }
                }
                else
                {
                    // 匹配指定数值最近的桩号
                    if (wantedStation.HasValue)
                    {
                        var allStations = new AllStations(allSections.Select(r => r.XData.Station).ToArray());
                        var closestStation = allStations.MatchClosest(wantedStation.Value);
                        matchedSection = allSections.FirstOrDefault(r => r.XData.Station == closestStation);
                    }
                }
                //
                if (matchedSection != null)
                {
                    var ext = matchedSection.GetExtends();
                    ext.TransformBy(Matrix3d.Scaling(
                       scaleAll: 1.2,
                     center: ext.MinPoint.Add((ext.MaxPoint - ext.MinPoint) / 2)));

                    Utils.ShowExtentsInView(docMdf.acEditor, ext);
                }
            }
            return ExternalCmdResult.Commit;

        }

        #endregion

        private double? SetStation(Editor ed, out bool? start)
        {
            start = null;
            var op = new PromptDoubleOptions(
                messageAndKeywords: "\n设置要切换到的桩号[起始(S) / 结尾(E)]:",
                globalKeywords: "起始 结尾"); // 默认值写在前面
            op.AllowNone = true;
            op.AllowArbitraryInput = false;

            var res = ed.GetDouble(op);
            if (res.Status == PromptStatus.OK)
            {
                start = null;
                return res.Value;
            }
            else if (res.Status == PromptStatus.Keyword)
            {
                if (res.StringResult == "结尾")
                {
                    start = false;
                }
                else
                {
                    start = true;
                }
            }
            return null;
        }
    }
}