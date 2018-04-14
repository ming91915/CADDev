using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Debug;
using eZcad.Utility;

namespace eZcad.OnCode
{
    /// <summary> 点击界面中的点以生成对应的标高 </summary>
    [EcDescription(CommandDescription)]
    public class ElevationPlacer : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"PlaceElevation";
        private const string CommandText = @"放置标高";
        private const string CommandDescription = @"点击界面中的点以生成对应的标高";

        /// <summary> 点击界面中的点以生成对应的标高 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void PlaceElevation()
        {
            DocumentModifier.ExecuteCommand(PlaceElevation);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ElevationPlacer();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.PlaceElevation,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 点击界面中的点以生成对应的标高 </summary>
        private ExternalCmdResult PlaceElevation(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            // 以只读方式打开块表   Open the Block table for read
            var acBlkTbl =
                docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;

            // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
            var acBlkTblRec =
                docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                    BlockTableRecord;

            var pt = GetElevationPoint(docMdf);

            while (pt != null)
            {
                var ele = pt.Value.Y / 1000;
                var txt = new DBText
                {
                    TextString = ele.ToString("000.000"),
                    Position = pt.Value,
                    Height = 1000,
                    WidthFactor = 0.7
                };
                // txt.SetDatabaseDefaults();

                // 添加新对象到块表记录和事务中   Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(txt);
                docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);

                txt.Draw();

                pt = GetElevationPoint(docMdf);
            }

            return ExternalCmdResult.Commit;
        }

        private Point3d? GetElevationPoint(DocumentModifier docMdf)
        {
            var op = new PromptPointOptions("\n选择一个点")
            {
                AllowNone = true
            };
            var res = docMdf.acEditor.GetPoint(op);
            if (res.Status == PromptStatus.OK)
            {
                var pt = res.Value;
                return pt;
            }
            return null;
        }
    }
}