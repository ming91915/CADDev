using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantities.Cmds;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(StationNavigator))]

namespace eZcad.SubgradeQuantities.Cmds
{
    /// <summary>
    /// 在 AutoCAD 界面中快速导航到指定的桩号
    /// </summary>
    public class StationNavigator
    {
        /// <summary> 命令行命令名称 </summary>
        public const string CommandName = "NavigateStation";

        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 在 AutoCAD 界面中快速导航到指定的桩号 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(@"断面导航"), Description("在 AutoCAD 界面中快速导航到指定的桩号")]
        public void EcConstructSections()
        {
            DocumentModifier.ExecuteCommand(NavigateStation);
        }

        /// <summary> 在 AutoCAD 界面中快速导航到指定的桩号 </summary>
        public void NavigateStation(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
         
            SubgradeSection matchedSection = null;
            bool? start;
            var wantedStation = SetStation(docMdf.acEditor, out start);

            // 所有的断面
            var allSections = ProtectionUtils.GetAllSections(docMdf, sort: true);

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