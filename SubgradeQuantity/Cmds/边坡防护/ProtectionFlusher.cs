using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using Exception = System.Exception;

[assembly: CommandClass(typeof (ProtectionFlusher))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
    public class ProtectionFlusher
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "FlushProtection";

        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(@"防护保存"), Description("将边坡线所绑定的防护方式的文字保存到边坡线数据中")
            , RibbonItem(@"防护保存", "将边坡线所绑定的防护方式的文字保存到边坡线数据中", ProtectionConstants.ImageDirectory + "FlushProtection_32.png")]
        public void FlushProtection()
        {
            DocumentModifier.ExecuteCommand(FlushProtection);
        }
        
        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        public void FlushProtection(DocumentModifier docMdf, SelectionSet impliedSelection)
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