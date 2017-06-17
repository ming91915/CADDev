using System.ComponentModel;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins.SlopeProtection;
using eZcad.Utility;

[assembly: CommandClass(typeof (OptionsSetter))]

namespace eZcad.Addins.SlopeProtection
{
    /// <summary> 边坡防护的选项设置 </summary>
    public class OptionsSetter
    {
        #region --- 命令设计

        /// <summary> 边坡防护的选项设置 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "SetSlopeOptions", CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"边坡设置"), Description("边坡防护的选项设置")]
        public void EcSetSlopeOptions()
        {
            DocumentModifier.ExecuteCommand(SetSlopeOptions);
        }

        /// <summary> 边坡防护的选项设置 </summary>
        public static void SetSlopeOptions(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var f = new Options(docMdf);
            f.ShowDialog(null);
        }

        #endregion
    }
}