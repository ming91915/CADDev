﻿using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using Exception = System.Exception;

[assembly: CommandClass(typeof(ProtectionFlusher))]

namespace eZcad.SubgradeQuantity.Cmds
{
    /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
    [EcDescription("从边坡线所绑定的防护方式的文字对象来设置防护")]
    public class ProtectionFlusher : ICADExCommand
    {
        private DocumentModifier _docMdf;

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = "FlushProtection";
        private const string CommandText = @"防护保存";
        private const string CommandDescription = @"将边坡线所绑定的防护方式的文字保存到边坡线数据中";

        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
            , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "FlushProtection_32.png")]
        public void FlushProtection()
        {
            DocumentModifier.ExecuteCommand(FlushProtection);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ProtectionFlusher();
            return SQAddinManagerDebuger.DebugInAddinManager(s.FlushProtection,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 从边坡线所绑定的防护方式的文字对象来设置防护 </summary>
        public ExternalCmdResult FlushProtection(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            SQUtils.SubgradeEnvironmentConfiguration(docMdf);
            // var allSections = ProtectionUtils.GetAllSections(docMdf);
            var slopeLines = SQUtils.SelecteExistingSlopeLines(docMdf, left: null, sort: true);

            // 从文字中提取边坡防护方式的数据
            foreach (var slp in slopeLines)
            {
                var xdata = slp.XData;
                var slpSegs = SlopeData.Combine(xdata.Slopes, xdata.Platforms, sort: false);
                foreach (var s in slpSegs)
                {
                    SlopeLine.ExtractProtectionFromText(s, _docMdf.acDataBase);
                }
                // 将数据保存下来
                slp.Pline.UpgradeOpen();
                slp.FlushXData();
                slp.Pline.DowngradeOpen();
            }
            return ExternalCmdResult.Commit;
        }

    }
}