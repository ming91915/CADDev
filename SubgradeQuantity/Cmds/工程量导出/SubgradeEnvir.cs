using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.XPath;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Options;
using eZcad.SubgradeQuantity.ParameterForm;
using eZcad.SubgradeQuantity.Utility;

[assembly: CommandClass(typeof(SubgradeEnvir))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
    [EcDescription(CommandDescription)]
    public class SubgradeEnvir : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"SetSubgradeEnvir";

        private const string CommandText = @"道路环境";
        private const string CommandDescription = @"设置整个道路中的结构物、岩土环境等信息";

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, ProtectionConstants.ImageDirectory + "Structures_32.png")]
        public void SetSubgradeEnvir()
        {
            DocumentModifier.ExecuteCommand(SetSubgradeEnvir);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SubgradeEnvir();
            return AddinManagerDebuger.DebugInAddinManager(s.SetSubgradeEnvir,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        private ExternalCmdResult SetSubgradeEnvir(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;

            var sss = Options_Collections.SoilRockRanges;
            // var allXdataTypes = DbXdata.GetAllXdataTypes();
            var handledXdataTypes = DbXdata.DatabaseXdataType.Structures | DbXdata.DatabaseXdataType.SoilRockRange;
            DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            //
            var fm = new Form_SubgradeEnvir(docMdf);
            var res = fm.ShowDialog();
            if (res == DialogResult.OK)
            {
                DbXdata.FlushXData(docMdf, handledXdataTypes);
            }
            else if (res == DialogResult.Cancel)
            {
                DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            }
            return ExternalCmdResult.Commit;
        }
    }
}