using System.ComponentModel;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(OptionsSetter))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 边坡防护的选项设置 </summary>
    public class OptionsSetter
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "SubgradeOptions";

        /// <summary> 边坡防护的选项设置 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CommandName, CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"设置"), Description("路基工程量相关选项的设置")
        , RibbonItem(@"设置", "路基工程量相关选项的设置", ProtectionConstants.ImageDirectory + "SetOptions_32.png")]
        public void SubgradeOptions()
        {
            DocumentModifier.ExecuteCommand(SubgradeOptions);
        }

        /// <summary> 边坡防护的选项设置 </summary>
        public static void SubgradeOptions(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var f = new SubgradeOptions(docMdf);
            f.ShowDialog(null);
        }

        #endregion
    }
}