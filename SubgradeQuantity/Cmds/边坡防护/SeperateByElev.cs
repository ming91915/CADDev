using System;
using System.ComponentModel;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (SlopeSeperator))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
    public class SlopeSeperator
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        public const string CommandName = "SeperateByElev";

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"标高剪切"), Description("按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式")
        ,RibbonItem(@"标高剪切", "按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式",
             ProtectionConstants.ImageDirectory + "SepByElev_32.png")]
        public void SeperateByElev()
        {
            DocumentModifier.ExecuteCommand(SeperateByElev);
        }

        #endregion

        /// <summary> 按标高将边坡对象进行分割，以实现同一级边坡中分别设置不同的防护形式 </summary>
        public void SeperateByElev(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            throw new NotImplementedException();
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);

            // 所有的断面
            // var allSections = ProtectionUtils.GetAllSections(docMdf,sort:true);
            var slopes = ProtectionUtils.GetExistingSlopeLines(docMdf);
            var es = EditStateIdentifier.GetCurrentEditState(_docMdf);
            es.CurrentBTR.UpgradeOpen();
            foreach (var slp in slopes)
            {
                slp.ClearXData();
            }
            es.CurrentBTR.DowngradeOpen();
        }
    }
}