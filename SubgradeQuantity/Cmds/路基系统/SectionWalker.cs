using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof (SectionWalker))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 查看指定的横断面的相关信息 </summary>
    [EcDescription(CommandDescription)]
    public class SectionWalker : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "SectionWalk";

        private const string CommandText = @"断面查看";
        private const string CommandDescription = @"查看指定的横断面的相关信息";

        /// <summary> 查看指定的横断面的相关信息 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet),
         DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "SectionInfos_32.png")]
        public void SectionWalk()
        {
            DocumentModifier.ExecuteCommand(SectionWalk);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SectionWalker();
            return AddinManagerDebuger.DebugInAddinManager(s.SectionWalk,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 查看指定的横断面的相关信息 </summary>
        public ExternalCmdResult SectionWalk(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            SubgradeSection sec;
            bool cont;
            // var allSections = ProtectionUtils.GetAllSections(docMdf, sort: true);
            sec = GetSection(_docMdf, out cont);
            while (cont)
            {
                if (sec != null)
                {
                    var formAddDefinition = new PropertyEditor("横断面信息", sec.XData);
                    //
                    var res = formAddDefinition.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // var newSlpDa = formAddDefinition.Instance;
                        sec.CenterLine.UpgradeOpen();

                        //  pl.XData = slpData.ToResultBuffer();
                        sec.CenterLine.DowngradeOpen();
                    }
                }
                //
                sec = GetSection(_docMdf, out cont);
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary> 从 AutoCAD 界面中选择横断面轴线 </summary>
        public static SubgradeSection GetSection(DocumentModifier docMdf, out bool cont)
        {
            cont = true;
            SubgradeSection sec = null;
            var op = new PromptEntityOptions("\n选择要提取的横断面轴线");
            op.SetRejectMessage("\n选择要提取的横断面轴线");
            op.AddAllowedClass(typeof (Line), true);

            var res = docMdf.acEditor.GetEntity(op);
            if (res.Status == PromptStatus.OK)
            {
                var line = res.ObjectId.GetObject(OpenMode.ForRead) as Line;
                if (line != null && line.Layer == Options_LayerNames.LayerName_CenterAxis)
                {
                    var si = SectionInfo.FromCenterLine(line);
                    if (si != null && si.FullyCalculated)
                    {
                        sec = new SubgradeSection(docMdf, line, si);
                    }
                    else
                    {
                        MessageBox.Show($"选择的道路中心线对象所对应的横断面未进行构造，" +
                                        $"\r\n请先调用“{SectionsConstructor.CommandName}”命令，以构造整个项目的横断面系统。",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cont = false;
                    }
                }
            }
            else if (res.Status == PromptStatus.Cancel)
            {
                cont = false;
            }
            return sec;
        }
    }
}