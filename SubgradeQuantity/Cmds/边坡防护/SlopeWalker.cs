using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(SlopeWalker))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 提取所有的横断面块参照的信息 </summary>
    [EcDescription(CommandDescription)]
    public class SlopeWalker : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "SlopeWalk";
        private const string CommandText = @"边坡漫游";
        private const string CommandDescription = @"查看指定的边坡对象的信息";

        /// <summary> 提取所有的横断面块参照的信息 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "SlopeWalk_32.png")]
        public void SlopeWalk()
        {
            DocumentModifier.ExecuteCommand(SlopeWalk);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeWalker();
            return AddinManagerDebuger.DebugInAddinManager(s.SlopeWalk,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 提取所有的横断面块参照的信息 </summary>
        public ExternalCmdResult SlopeWalk(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            bool cont;
            var pl = GetSlopeLine(docMdf.acEditor, out cont);

            while (cont)
            {
                if (pl != null)
                {
                    //

                    var slpData = SlopeData.FromEntity(pl);

                    var formAddDefinition = new PropertyEditor("边坡参数", slpData);
                    //
                    var res = formAddDefinition.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // var newSlpDa = formAddDefinition.Instance;
                        pl.UpgradeOpen();

                        //  pl.XData = slpData.ToResultBuffer();
                        pl.DowngradeOpen();
                    }
                }
                //
                pl = GetSlopeLine(docMdf.acEditor, out cont);
            }
            return ExternalCmdResult.Commit;

        }



        private Polyline GetSlopeLine(Editor ed, out bool cont)
        {
            var op = new PromptEntityOptions("\n选择一条边坡线以进行信息读写");
            op.SetRejectMessage($"\n选择的多段线必须包含{SlopeData.AppName}的外部扩展数据");
            op.AddAllowedClass(typeof(Polyline), exactMatch: true);
            var res = ed.GetEntity(op);

            cont = true;
            if (res.Status == PromptStatus.OK)
            {
                var pl = res.ObjectId.GetObject(OpenMode.ForRead) as Polyline;
                if (pl != null)
                {
                    if (SlopeLine.IsSlopeLineLayer(pl.Layer, left: null))
                    {
                        cont = true;
                        return pl;
                    }
                }
            }
            else if (res.Status == PromptStatus.Cancel)
            {
                cont = false;
            }

            return null;
        }
    }
}