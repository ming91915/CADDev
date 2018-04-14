using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Addins.Geometry;
using eZcad.Debug;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(PerpendicularLine))]

namespace eZcad.Addins.Geometry
{
    /// <summary> 为指定的曲线添加垂线 </summary>
    [EcDescription(CommandDescription)]
    public class PerpendicularLine : ICADExCommand
    {

        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"DrawPerpendicularLine";

        private const string CommandText = @"面积求和";
        private const string CommandDescription = @"为指定的曲线添加垂线";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void DrawPerpendicularLine()
        {
            DocumentModifier.ExecuteCommand(DrawPerpendicularLine);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new PerpendicularLine();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.DrawPerpendicularLine,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        public ExternalCmdResult DrawPerpendicularLine(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var pl = new PerpendicularLineDrawer(onLeft: null, length: 30);
            pl.AddPerpendicularLineOnACurve(docMdf);
            return ExternalCmdResult.Commit;
        }

        /// <summary> 为指定的曲线添加垂线 </summary>
        private class PerpendicularLineDrawer
        {
            /// <summary> 垂线的长度 </summary>
            private double _length;

            /// <summary> true 表示垂线总是指向第二、三象限；false 表示垂线总是指向第一、四象限；null表示在曲线上的点两侧均绘制一条曲线 </summary>
            private bool? _onLeft;


            /// <summary> 无参数的构造函数 </summary>
            public PerpendicularLineDrawer()
            {
                _length = 10;
            }

            /// <summary> 构造函数 </summary>
            /// <param name="onLeft">true 表示垂线总是指向第二、三象限；false 表示垂线总是指向第一、四象限；null表示在曲线上的点两侧均绘制一条曲线</param>
            /// <param name="length">垂线的长度</param>
            public PerpendicularLineDrawer(bool? onLeft, double length) : this()
            {
                _onLeft = onLeft;
                _length = length;
            }

            /// <summary> 为指定的曲线添加垂线 </summary>
            /// <param name="docMdf"></param>
            [CommandMethod(eZConstants.eZGroupCommnad, "AddPerpendicularLineOnACurve", CommandFlags.Modal | CommandFlags.UsePickSet)]
            public void AddPerpendicularLineOnACurve(DocumentModifier docMdf)
            {
                var curve = PickOneCurve(docMdf);
                if (curve == null) return;
                // 以只读方式打开块表   Open the Block table for read
                var acBlkTbl =
                    docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
                // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                var acBlkTblRec =
                    docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                // 在界面中选择一个点，并生成对应位置处曲线的垂线
                var line = PickAndAddLine(docMdf.acEditor, curve);
                while (line != null)
                {
                    // 添加新 对象到块表记录和事务中   Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(line);
                    docMdf.acTransaction.AddNewlyCreatedDBObject(line, true);

                    // 如果想在用户每次生成一条直线时，都在界面中实时显示出这条线，则可以通过Draw方法。acEditor.UpdateScreen();并不能达到此效果。
                    // 如果不使用Draw()，则界面上会在所有的线条都添加完成并结束API代码时，一次性将全部线条刷新到界面上。
                    // 在Draw()方法执行之前，必须确保此对象已经添加到数据库中（但是可以在 acTransaction.AddNewlyCreatedDBObject 之前），否则执行Draw()时，会千万 AutoCAD 的崩溃！
                    line.Draw();
                    docMdf.acEditor.PostCommandPrompt();
                    //
                    line = PickAndAddLine(docMdf.acEditor, curve);
                }
            }

            private Curve PickOneCurve(DocumentModifier docMdf)
            {
                // 点选
                var peO = new PromptEntityOptions("\n 选择一条曲线 ");
                peO.SetRejectMessage("\n 请选择一个曲线对象\n");
                peO.AddAllowedClass(typeof(Curve), false);

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

            /// <summary> 在曲线附近选一个点，并绘制其垂线 </summary>
            /// <param name="editor"></param>
            /// <param name="baseCurve"></param>
            /// <returns></returns>
            private Line PickAndAddLine(Editor editor, Curve baseCurve)
            {
                var continuPickPoint = false;
                Point3d? pt;
                do
                {
                    pt = GetOnePoint(editor, ref _onLeft, ref _length, out continuPickPoint);
                } while (continuPickPoint);


                if (pt == null) return null;
                //

                var closestPt = baseCurve.GetClosestPointTo(pt.Value, false);
                //
                var tangVec = baseCurve.GetFirstDerivative(closestPt);


                // 创建一条有两段的多段线   Create a polyline with two segments (3 points)
                var planeNorm = baseCurve.IsPlanar ? baseCurve.GetPlane().Normal : new Vector3d(0, 0, 1);

                // 将切线方向反转到第一、二象限，以确定左侧垂线的方向
                Point3d p1 = default(Point3d), p2 = default(Point3d);
                var perpendVec = default(Vector3d);
                switch (_onLeft)
                {
                    case true:
                        perpendVec = tangVec.Y >= 0 // 如果切线矢量指向第一、二象限，则向左转
                            ? tangVec.RotateBy(Math.PI / 2, planeNorm)
                            : tangVec.RotateBy(-Math.PI / 2, planeNorm);

                        perpendVec = perpendVec.SetLength(_length);
                        p1 = closestPt;
                        p2 = closestPt.Add(perpendVec);
                        break;
                    case false:
                        perpendVec = tangVec.Y >= 0 // 如果切线矢量指向第一、二象限，则向右转
                            ? tangVec.RotateBy(-Math.PI / 2, planeNorm)
                            : tangVec.RotateBy(Math.PI / 2, planeNorm);

                        perpendVec = perpendVec.SetLength(_length);
                        p1 = closestPt;
                        p2 = closestPt.Add(perpendVec);
                        break;
                    case null:
                        perpendVec = tangVec.RotateBy(Math.PI / 2, planeNorm);
                        perpendVec = perpendVec.SetLength(_length);
                        p1 = closestPt.Add(perpendVec);
                        p2 = closestPt.Subtract(perpendVec);
                        break;
                }
                return new Line(p1, p2);
            }

            /// <summary> 在界面中选择一个点，用来在其附近创建曲线的垂线 </summary>
            /// <param name="onLeft"> null 表示画在切点的左右两侧 </param>
            /// <param name="length"></param>
            /// <param name="continuPickPoint">没有成功选择一个点，只是输入了关键词，此时需要继续进行选择</param>
            /// <returns>如果没有选择到有效的点，则返回null</returns>
            private Point3d? GetOnePoint(Editor editor, ref bool? onLeft, ref double length,
                out bool continuPickPoint)
            {
                var ppo = new PromptPointOptions("\n选择曲线附近的一个点[长度(L) / 左侧(Lf) / 右侧(Rt) / 两侧(B)]:", "长度 左侧 右侧 两侧");
                ppo.Keywords.Default = "长度";
                ppo.AllowArbitraryInput = true; // 用户可以输入非关键字的字符，其可以

                // 在界面中选择一个角度
                var pdr = editor.GetPoint(ppo);

                if (pdr.Status == PromptStatus.Keyword) // 用户输入关键词或者其他任意字符
                {
                    continuPickPoint = true;
                    switch (pdr.StringResult)
                    {
                        case "长度":
                            var op = new PromptDistanceOptions("\n指定垂线的长度")
                            {
                                AllowNone = true,
                                AllowNegative = false,
                                AllowZero = false,
                                AllowArbitraryInput = false
                            };

                            var res = editor.GetDistance(op);
                            if (res.Status == PromptStatus.OK)
                            {
                                if (res.Value > 0)
                                {
                                    length = res.Value;
                                }
                            }
                            break;
                        case "左侧":
                            onLeft = true;
                            break;
                        case "右侧":
                            onLeft = false;
                            break;
                        case "两侧":
                            onLeft = null;
                            break;
                        default: // 用户输入一个任意字符
                                 // 可能直接输入一个表示长度的字符
                            var s = pdr.StringResult;
                            double len;
                            if (double.TryParse(s, out len) && len > 0)
                            {
                                length = len;
                            }
                            break;
                    }
                }
                else if (pdr.Status == PromptStatus.Cancel) // 用户按下ESC
                {
                    continuPickPoint = false;
                    return null;
                }
                if (pdr.Status == PromptStatus.OK) // 用户选择到一个点
                {
                    continuPickPoint = false;
                    return pdr.Value;
                }
                continuPickPoint = true;
                return null;
            }
        }
    }
}