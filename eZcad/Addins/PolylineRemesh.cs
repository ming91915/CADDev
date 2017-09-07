using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoCAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(PolylineRemesh))]

namespace eZcad.Addins
{
    /// <summary> 对多段线的疏密进行重新设置 </summary>
    public class PolylineRemesh
    {
        /// <summary> 对多段线的疏密进行重新设置 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "RemeshPolyline", CommandFlags.Modal | CommandFlags.UsePickSet)
        , DisplayName(@"多段线疏密"), Description("对多段线的疏密进行重新设置")]
        public void EcRemeshPolyline()
        {
            DocumentModifier.ExecuteCommand(RemeshPolyline);
        }

        /// <summary> 对多段线的疏密进行重新设置 </summary>
        public ExternalCmdResult RemeshPolyline(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var app = Application.AcadApplication as AcadApplication;
            // 获得当前文档和数据库   Get the current document and database
            var acActiveDocument = docMdf.acActiveDocument;
            var acDataBase = docMdf.acDataBase;
            var tran = docMdf.acTransaction;

            var pls = SelectPolylines(docMdf);
            if (pls == null || pls.Length == 0) return ExternalCmdResult.Cancel;
            //
            var blkTb =
                docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr =
                docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                    BlockTableRecord;
            //
            var changedPolylines = new Dictionary<Polyline, CompositeCurve3d>();
            var byVertice = ByVerticeOrLength(docMdf);
            if (byVertice)
            {

                // 按顶点缩放
                var seg = GetSegVertices(docMdf);
                if (seg > 0)
                {
                    var aa = PreserveAllArc(docMdf);
                    foreach (var pl in pls)
                    {
                        var cs = pl.GetGeCurve() as CompositeCurve3d;
                        var newCs = Utils.GetThinedPolyline(cs.GetCurves(), seg, aa);

                        if (newCs != null)
                        {
                            changedPolylines.Add(pl, newCs);
                        }
                    }
                }
            }
            else
            {
                // 按长度缩放
                var segLength = GetDistance(docMdf);
                if (segLength > 0)
                {
                    foreach (var pl in pls)
                    {
                        var cs = pl.GetGeCurve() as CompositeCurve3d;
                        var newCs = Utils.GetThinedPolyline(cs, segLength);
                        if (newCs != null)
                        {
                            changedPolylines.Add(pl, newCs);
                        }
                    }
                }
            }
            
            // 创建对应的新的多段线
            foreach (var curve in changedPolylines.Values)
            {
                var newPolyline = Curve.CreateFromGeCurve(curve);
                // newPolyline.Color = Color.FromColor(System.Drawing.Color.Green);
                // 将新对象添加到块表记录和事务
                btr.AppendEntity(newPolyline);
                docMdf.acTransaction.AddNewlyCreatedDBObject(newPolyline, true);
            }

            // 是否删除原多段线
            if (changedPolylines.Count > 0)
            {
                if (DeleteOrigional(docMdf))
                {
                    foreach (var pl in changedPolylines.Keys)
                    {
                        pl.UpgradeOpen();
                        pl.Erase();
                        pl.DowngradeOpen();
                    }
                }
            }
            return ExternalCmdResult.Commit;
        }

        #region ---   命令行交互

        /// <summary> 选择多段线以修改其疏密 </summary>
        private static Polyline[] SelectPolylines(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择多段线以修改其疏密"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。


            var res = ed.GetSelection(pso, filter);

            if (res.Status == PromptStatus.OK)
            {
                return
                    res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<Polyline>().ToArray();
            }
            return null;
        }

        /// <summary> </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示按顶点缩放（默认），false 表示按长度缩放</returns>
        private static bool ByVerticeOrLength(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions("\n[按顶点缩放(V) / 按长度缩放(L)]:", "顶点 长度");
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                if (res.StringResult == "长度")
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary> 是否删除原多段线 </summary>
        private static bool DeleteOrigional(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions("\n删除原多段线[否(N) / 是(Y)]:", "No Yes");
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                if (res.StringResult == "Yes")
                {
                    return true;
                }
            }
            return false;
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


        /// <summary> 隔多少个顶点进行分段 </summary>
        private static int GetSegVertices(DocumentModifier docMdf)
        {
            var op = new PromptIntegerOptions("\n每隔多少个顶点进行分段")
            {
                LowerLimit = 1,
                UpperLimit = (int)1e6,
                //
                AllowNegative = false,
                AllowNone = false,
                AllowZero = false,
                AllowArbitraryInput = false
            };

            //
            var res = docMdf.acEditor.GetInteger(op);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            return 0;
        }

        /// <summary> 保留多段线中所有的曲线段 </summary>
        private static bool PreserveAllArc(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions("\n 保留所有的曲线段[是(Y)/否(N)]:", "Yes No");
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                if (res.StringResult == "No")
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}