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
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(Polyface_SubDMesh_Convertor))]

namespace eZcad.Addins.Geometry
{
    /// <summary> 将多面网格转换为细分网格 </summary>
    [EcDescription(CommandDescription)]
    public class Polyface_SubDMesh_Convertor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ConvertPolyfaceToSubDMesh";
        private const string CommandText = @"网格转换";
        private const string CommandDescription = @"将多面网格转换为细分网格";

        /// <summary> 将多面网格转换为细分网格 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void ConvertPolyfaceToSubDMesh()
        {
            DocumentModifier.ExecuteCommand(ConvertPolyfaceToSubDMesh);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new Polyface_SubDMesh_Convertor();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.ConvertPolyfaceToSubDMesh,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将多面网络转换为细分网格 </summary>
        private ExternalCmdResult ConvertPolyfaceToSubDMesh(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var mtd = ChooseMethod(docMdf.acEditor);

            var deleteOriginal = DeleteOriginal(docMdf.acEditor);

            if (mtd == ConvertMethod.PolyfaceMeshToSubDMesh)
            {
                ConvertPolyfaceToSubDmesh(docMdf, deleteOriginal);
            }
            else
            {
                ConvertSubDmeshToPolyfaceMesh(docMdf, deleteOriginal);

            }
            return ExternalCmdResult.Commit;
        }

        private static void ConvertPolyfaceToSubDmesh(DocumentModifier docMdf, bool deletePolyfaceMesh)
        {
            var pMeshes = SelectPolyfacemeshes(docMdf);
            if (pMeshes != null && pMeshes.Length > 0)
            {
                var blkTb =
                    docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr =
                    docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                foreach (var id in pMeshes)
                {
                    var pMesh = id.GetObject(OpenMode.ForRead) as PolyFaceMesh;
                    if (pMesh != null)
                    {
                        var subDMesh = pMesh.ConvertToSubDMesh();
                        if (subDMesh != null)
                        {
                            btr.AppendEntity(subDMesh);
                            docMdf.acTransaction.AddNewlyCreatedDBObject(subDMesh, true);
                            // 删除选择的多面网格
                            if (deletePolyfaceMesh)
                            {
                                pMesh.UpgradeOpen();
                                pMesh.Erase(true);
                            }
                        }
                    }
                }
            }
        }
        private static void ConvertSubDmeshToPolyfaceMesh(DocumentModifier docMdf, bool deleteSubDmesh)
        {
            var subDMeshes = SelectSubDMeshes(docMdf);
            if (subDMeshes != null && subDMeshes.Length > 0)
            {
                var blkTb =
                    docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr =
                    docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                foreach (var id in subDMeshes)
                {
                    var subDMesh = id.GetObject(OpenMode.ForRead) as SubDMesh;
                    if (subDMesh != null)
                    {
                        var pMesh = subDMesh.ConvertToPolyFaceMesh(btr, docMdf.acTransaction);

                        // 删除选择的多面网格
                        if (deleteSubDmesh)
                        {
                            subDMesh.UpgradeOpen();
                            subDMesh.Erase(true);
                        }

                    }
                }
            }
        }

        #region ---   界面交互

        private static ConvertMethod ChooseMethod(Editor ed)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n网格转换 [细分网格到多面网格(P) / 多面网格到细分网格(S)]:",
                globalKeywords: "多面网格 细分网格");
            op.AllowNone = true;
            op.AllowArbitraryInput = false;
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                if (res.StringResult == "细分网格")
                {
                    return ConvertMethod.PolyfaceMeshToSubDMesh;
                }
            }
            // 默认项
            return ConvertMethod.SubDMeshToPolyfaceMesh;
        }
        private enum ConvertMethod
        {
            PolyfaceMeshToSubDMesh,
            SubDMeshToPolyfaceMesh,
        }

        private static bool DeleteOriginal(Editor ed)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n删除原对象? [Yes(Y) / No(N)]:",
                globalKeywords: "Yes No");
            op.AllowNone = true;
            op.AllowArbitraryInput = false;
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                if (res.StringResult == "No")
                {
                    return false;
                }
            }
            // 默认项
            return true;
        }

        /// <summary>选择多个网格 </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static ObjectId[] SelectSubDMeshes(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "MESH"),
                new TypedValue((int) 100, "AcDbSubDMesh")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择网格"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            var res = ed.GetSelection(pso, filter);

            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            return null;
        }


        /// <summary>选择多个多面网格 </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static ObjectId[] SelectPolyfacemeshes(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "POLYLINE"),
                new TypedValue((int) 100, "AcDbPolyFaceMesh")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择多面网格"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            var res = ed.GetSelection(pso, filter);

            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            return null;
        }

        #endregion
    }
}