using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Addins.Geometry;
using eZcad.Debug;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(ReverseCurve))]

namespace eZcad.Addins.Geometry
{
    /// <summary> 将指定的曲线的起始点反转 </summary>
    [EcDescription(CommandDescription)]
    public class ReverseCurve : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ReverseCurveEnds";
        private const string CommandText = @"反转端点";
        private const string CommandDescription = @"将指定的曲线的起始点反转";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void ReverseCurveEnds()
        {
            DocumentModifier.ExecuteCommand(ReverseCurveEnds);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ReverseCurve();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.ReverseCurveEnds,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将指定的曲线的起始点反转 </summary>
        /// <param name="docMdf"></param>
        /// <param name="impliedSelection"> 用户在执行方法之前已经选择好的对象。</param>
        private ExternalCmdResult ReverseCurveEnds(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            docMdf.acEditor.Command();

            Curve c = null;
            if (impliedSelection != null)
            {
                foreach (var id in impliedSelection.GetObjectIds())
                {
                    c = docMdf.acTransaction.GetObject(id, OpenMode.ForRead) as Curve;
                    if (c != null)
                    {
                        break;
                    }
                }
            }
            if (c == null)
            {
                c = PickOneCurve(docMdf);
            }

            if (c != null)
            {
                docMdf.acTransaction.GetObject(c.Id, OpenMode.ForWrite);
                c.ReverseCurve();
                // 提示信息
                string msg = $"\n反转后曲线起点：{c.StartPoint.ToString()}，终点：{c.EndPoint.ToString()}";
                docMdf.WriteNow(msg);

                c.DowngradeOpen();
            }
            return ExternalCmdResult.Commit;
        }

        private static Curve PickOneCurve(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一条曲线 ");
            peO.SetRejectMessage("\n 请选择一个曲线对象\n");
            peO.AddAllowedClass(typeof(Curve), exactMatch: false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            Curve curve = null;
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                curve = docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as Curve;
            }
            return curve;
        }
    }
}