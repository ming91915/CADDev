using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof (SlopeEraser))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 将边坡对象清理为一般的边坡线，而删除其中所有与边坡相关的数据 </summary>
    public class SlopeEraser
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "EraseSlope";

        /// <summary> 将边坡对象清理为一般的边坡线，而删除其中所有与边坡相关的数据 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"清理边坡"), Description("将边坡对象清理为一般的边坡线，而删除其中所有与边坡相关的数据")
        ,
         RibbonItem(@"清理边坡", "将边坡对象清理为一般的边坡线，而删除其中所有与边坡相关的数据", ProtectionConstants.ImageDirectory + "EraseSlope_32.png")
        ]
        public void EraseSlope()
        {
            DocumentModifier.ExecuteCommand(EraseSlope);
        }

        #endregion

        /// <summary> 将边坡对象清理为一般的边坡线，而删除其中所有与边坡相关的数据 </summary>
        public void EraseSlope(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
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