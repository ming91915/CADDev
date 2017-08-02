using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantities.Cmds;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Utility;
using eZcad.Utility;
using Exception = System.Exception;

[assembly: CommandClass(typeof (ProtectionMethodGetter))]

namespace eZcad.SubgradeQuantities.Cmds
{
    /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
    public class ProtectionMethodGetter
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "GetProtectionMethodFromText", CommandFlags.UsePickSet)
        , DisplayName(@"提取防护文字"), Description("从边坡线所绑定的防护方式的文字对象来设置防护")]
        public void EcGetProtectionMethodFromText()
        {
            DocumentModifier.ExecuteCommand(GetProtectionMethodFromText);
        }
        
        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        public void GetProtectionMethodFromText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            ProtectionUtils.SubgradeEnvironmentConfiguration(docMdf);
            // var allSections = ProtectionUtils.GetAllSections(docMdf);
            var slopeLines = ProtectionUtils.GetExistingSlopeLines(docMdf);

            // 从文字中提取边坡防护方式的数据
            foreach (var slp in slopeLines)
            {
                var xdata = slp.XData;
                var slpSegs = SlopeData.Combine(xdata.Slopes, xdata.Platforms, sort: false);
                foreach (var s in slpSegs)
                {
                    DBText text = null;
                    try
                    {
                        text = s.ProtectionMethodText.GetDBObject<DBText>(docMdf.acDataBase);
                    }
                    catch (Exception)
                    {
                    }
                    if (text != null && !string.IsNullOrEmpty(text.TextString))
                    {
                        s.ProtectionMethod = text.TextString;
                    }
                    else
                    {
                        s.ProtectionMethod = null;
                    }
                }
                // 将数据保存下来
                slp.Pline.UpgradeOpen();
                slp.FlushXData();
                slp.Pline.DowngradeOpen();
            }
        }

        #endregion
    }
}