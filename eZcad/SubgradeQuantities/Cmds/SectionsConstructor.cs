using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantities;
using eZcad.SubgradeQuantities.Cmds;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(SectionsConstructor))]

namespace eZcad.SubgradeQuantities.Cmds
{

    /// <summary>
    /// 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统
    /// </summary>
    public class SectionsConstructor
    {

        /// <summary> 命令行命令名称 </summary>
        public const string CommandName = "ConstructSections";

        #region --- 命令设计

        /// <summary> 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
            , DisplayName(@"构造路基断面"), Description("根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统")]
        public void EcConstructSections()
        {
            DocumentModifier.ExecuteCommand(ConstructSections);
        }

        /// <summary> 根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统 </summary>
        public void ConstructSections(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            //
            var axes = GetCenterAxes(docMdf.acEditor);
            if (axes != null && axes.Count > 0)
            {
                var sectionAxes = new List<SubgradeSection>();
                foreach (var axis in axes)
                {
                    var cenA = SubgradeSection.Create(docMdf, axis);
                    if (cenA != null)
                    {
                        cenA.CenterLine.UpgradeOpen();

                        cenA.ClearXData(true);
                        cenA.CalculateSectionInfoToXData();
                        cenA.FlushXData();

                        cenA.CenterLine.DowngradeOpen();

                        sectionAxes.Add(cenA);
                    }
                }
                MessageBox.Show($"添加{sectionAxes.Count}个横断面", @"成功");
            }
        }

        #endregion

        private IList<Line> GetCenterAxes(Editor ed)
        {
            var filterType = new[]
        {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_CenterAxis),
            };

            // 请求在图形区域选择对象
            var op = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = op.Keywords.GetDisplayString(true);
            op.MessageForAdding = "\n选择横断面中轴线线 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = "\n选择横断面中轴线线 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            // pso.SingleOnly = true;

            var res = ed.GetSelection(op, new SelectionFilter(filterType));
            if (res.Status == PromptStatus.OK)
            {
                var lines =
                  res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Line>().ToArray();
                return lines;
            }
            return null;
        }
    }
}