using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins.Table;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(PolylineRemesh2))]

namespace eZcad.Addins.Table
{
    /// <summary> 将选择的曲线按指定的长度分割为多段线 </summary>
    [EcDescription(CommandDescription)]
    public class PolylineRemesh2 : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CreatePolylineFromCurveByIntervals";

        private const string CommandText = @"分割曲线";
        private const string CommandDescription = @"将选择的曲线按指定的长度分割为多段线";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void SumupArea()
        {
            DocumentModifier.ExecuteCommand(CreatePolylineFromCurveByIntervals);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new PolylineRemesh2();
            return AddinManagerDebuger.DebugInAddinManager(s.CreatePolylineFromCurveByIntervals,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将选择的曲线按指定的长度分割为多段线 </summary>
        public ExternalCmdResult CreatePolylineFromCurveByIntervals(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            // var pl = AddinManagerDebuger.PickObject<Curve>(docMdf.acEditor);

            // AutoCAD中的一列数据，并从高到低排序
            var cv = SelectCurve();
            if (cv == null)
            {
                return ExternalCmdResult.Cancel;
            }
            var segLength = GetDistance(docMdf);
            var curve = cv.GetGeCurve();
            var cp = Utils.GetThinedPolyline(curve, segLength);

            // 绘制多段线
            var pline = Polyline.CreateFromGeCurve(cp);
            if (pline == null)
            {
                return ExternalCmdResult.Cancel;
            }
            var cs = EditStateIdentifier.GetCurrentEditState(docMdf);
            cs.CurrentBTR.UpgradeOpen();
            cs.CurrentBTR.AppendEntity(pline);
            docMdf.acTransaction.AddNewlyCreatedDBObject(pline, true);
            cs.CurrentBTR.DowngradeOpen();

            return ExternalCmdResult.Commit;
        }


        private Curve SelectCurve()
        {
            var op = new PromptEntityOptions("\n选择一条曲线");
            op.SetRejectMessage("选择任意一种类型的曲线");
            op.AddAllowedClass(typeof(Curve), false);

            var res = _docMdf.acEditor.GetEntity(op);
            if (res.Status == PromptStatus.OK)
            {

                var c = res.ObjectId.GetObject(OpenMode.ForRead) as Curve;
                return c;
            }

            return null;
        }


        /// <summary> 分段的长度 </summary>
        private static double GetDistance(DocumentModifier docMdf)
        {
            var op = new PromptDistanceOptions("\n每个分段的长度")
            {
                AllowNegative = false,
                AllowNone = false,
                AllowZero = false,
                AllowArbitraryInput = false
            };
            //
            var res = docMdf.acEditor.GetDistance(op);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            return 0;
        }


    }
}