using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.SubgradeQuantity;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

[assembly: CommandClass(typeof(ClosedAreaSumup))]

namespace eZcad.Addins
{
    /// <summary> 以点填充的方式选择出指定点所在的封闭图形，并计算此图形的面积，再进行求和 </summary>
    [EcDescription(CommandDescription)]
    public class ClosedAreaSumup : ICADExCommand
    {

        private DocumentModifier _docMdf;
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"SumupClosedArea";
        private const string CommandText = @"面积计算";
        private const string CommandDescription = @"以点填充的方式选择出指定点所在的封闭图形，并计算此图形的面积，再进行求和";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        /// <summary> 沿着道路纵向绘制边坡线 </summary>
        [CommandMethod(SQConstants.eZGroupCommnad, CommandName, CommandFlags.UsePickSet)
        , DisplayName(CommandText), Description(CommandDescription)
            , RibbonItem(CommandText, CommandDescription, SQConstants.ImageDirectory + "ClosedAreaSumup_32.png")]
        public void SumupClosedArea()
        {
            DocumentModifier.ExecuteCommand(SumupClosedArea);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ClosedAreaSumup();
            return SQAddinManagerDebuger.DebugInAddinManager(s.SumupClosedArea,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        public ExternalCmdResult SumupClosedArea(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            double areaSum = 0;
            Point3d pt;
            bool cont;
            int count = 0;
            var polyLines = new List<Entity>();
            pt = GetPoint(docMdf.acEditor, out cont);
            var cs = EditStateIdentifier.GetCurrentEditState(docMdf);
            cs.CurrentBTR.UpgradeOpen();

            while (cont)
            {
                // 选择的点所在的封闭图形
                DBObjectCollection objs = docMdf.acEditor.TraceBoundary(seedPoint: pt, detectIslands: true);
                if (objs.Count == 0)
                {
                    docMdf.WriteNow("\n请点击一个封闭的区域，并将其在界面中显示出来。");
                }
                // Add our boundary objects to the drawing
                foreach (DBObject obj in objs)
                {
                    var ent = obj as Polyline;
                    // 一般来说，ent 都是 Polyline 对象，而且这些多段线对象并没有添加到数据库中
                    if (ent != null)
                    {
                        // Set our boundary objects to be of our auto-incremented colour index
                        ent.ColorIndex = 5;

                        // Set the lineweight of our object
                        ent.LineWeight = LineWeight.LineWeight050;

                        // Add each boundary object to the modelspace and add its ID to a collection
                        cs.CurrentBTR.AppendEntity(ent);
                        docMdf.acTransaction.AddNewlyCreatedDBObject(ent, true);
                        ent.Draw();
                    }
                    var area = ent.Area;
                    areaSum += area;
                    count += 1;
                    docMdf.WriteNow($"\n区域数量：{count}，\t当前区域的面积为：{area},\t面积求和：{areaSum}");
                    //
                    polyLines.Add(ent);
                }

                //
                pt = GetPoint(docMdf.acEditor, out cont);
            };

            // 将所有的线条删除
            bool deleteCurves;
            // deleteCurves = DeleteCurves(docMdf);
            deleteCurves = true;
            if (deleteCurves)
            {
                foreach (var pl in polyLines)
                {
                    pl.UpgradeOpen();
                    pl.Erase();
                    pl.DowngradeOpen();
                }
            }


            cs.CurrentBTR.DowngradeOpen();

            return ExternalCmdResult.Commit;
        }

        private Point3d GetPoint(Editor ed, out bool cont)
        {
            cont = false;
            var op = new PromptPointOptions(message: "\n选择封闭图形中的一个点")
            {
                AllowNone = false,
                UseDashedLine = true,
                AllowArbitraryInput = true
            };
            //
            var res = _docMdf.acEditor.GetPoint(op);
            if (res.Status == PromptStatus.OK)
            {
                cont = true;
                return res.Value;
            }
            else
            {
                cont = false;
                return default(Point3d);
            }
        }


        /// <summary> 从两个选项中选择一个 </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示按顶点缩放（默认值），false 表示按长度缩放</returns>
        private static bool DeleteCurves(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n删除线条[是(Y) / 否(N)]:",
                globalKeywords: "是 否"); // 默认值写在前面
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                if (res.StringResult == "否")
                {
                    return false;
                }
            }
            return true;
        }

    }
}