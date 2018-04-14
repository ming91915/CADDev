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
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "Structures_32.png")]
        public void SetSubgradeEnvir()
        {
            DocumentModifier.ExecuteCommand(SetSubgradeEnvir);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SubgradeEnvir();
            return SQAddinManagerDebuger.DebugInAddinManager(s.SetSubgradeEnvir,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        private ExternalCmdResult SetSubgradeEnvir(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;

            // var allXdataTypes = DbXdata.GetAllXdataTypes();
            var handledXdataTypes = DbXdata.DatabaseXdataType.RangeBlocks | DbXdata.DatabaseXdataType.SoilRockRange;
            // 在执行此方法前，已经通过“DbXdata.LoadAllOptionsFromDbToMemory”方法，将文档中的通用选项加载到了内存中，所以不需要再特别地调用 RefreshOptionsFromDb()方法了。
            //DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            //
            var fm = new Form_SubgradeEnvir(docMdf);
            var res = fm.ShowDialog();
            if (res == DialogResult.OK)
            {
                DbXdata.FlushXData(docMdf, handledXdataTypes);
            }
            else if (res == DialogResult.Cancel)
            {
                // 将内存中对全局选项的修改进行还原，还原的方法就是重新从数据库中加载修改前的数据。
                DbXdata.RefreshOptionsFromDb(docMdf, handledXdataTypes);
            }
            return ExternalCmdResult.Commit;
        }
    }
}