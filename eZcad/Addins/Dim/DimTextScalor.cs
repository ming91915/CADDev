using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoCAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Addins.Dim;
using eZcad.Debug;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(DimTextScalor))]

namespace eZcad.Addins.Dim
{
    /// <summary> 标注尺寸的单位修改 </summary>
    [EcDescription(CommandDescription)]
    public class DimTextScalor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"ScaleDimText";
        private const string CommandText = @"缩放标注值";
        private const string CommandDescription = @"将用户手动修改过的标注进行单位缩放";

        /// <summary> 计算选择的所有曲线的面积与长度之和 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void ScaleDimText()
        {
            DocumentModifier.ExecuteCommand(ScaleDimText);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new DimTextScalor();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.ScaleDimText,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        private ExternalCmdResult ScaleDimText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {

            var app = Application.AcadApplication as AcadApplication;
            // 获得当前文档和数据库   Get the current document and database
            var acActiveDocument = docMdf.acActiveDocument;
            var acDataBase = docMdf.acDataBase;
            var tran = docMdf.acTransaction;

            var dims = SelectDims((docMdf));
            if (dims == null || dims.Length == 0) return ExternalCmdResult.Cancel;
            //
            var res = docMdf.acEditor.GetDouble("设置标注单位的缩放比例:");
            double scaleRatio = 0;
            if (res.Status == PromptStatus.OK)
            {
                scaleRatio = res.Value;
            }
            else
            {
                return ExternalCmdResult.Cancel;
            }
            // 进行缩放
            foreach (var dimId in dims)
            {
                var dim = dimId.GetObject(OpenMode.ForRead) as Dimension;
                // 只对用户手动修改过标注尺寸值的标注 对象进行修改，如果标注对象的值是实际测量值，则保持原来的值
                if (dim is RotatedDimension)
                {
                    var rotDim = dim as RotatedDimension;
                    if (!string.IsNullOrEmpty(rotDim.DimensionText))
                    {
                        double oldValue;
                        if (double.TryParse(rotDim.DimensionText, out oldValue))
                        {
                            rotDim.UpgradeOpen();
                            rotDim.DimensionText = (oldValue * scaleRatio).ToString();
                            rotDim.DowngradeOpen();
                        }
                    }
                }
                else if (dim is AlignedDimension)
                {
                    var alignDim = dim as AlignedDimension;
                    if (!string.IsNullOrEmpty(alignDim.DimensionText))
                    {
                        double oldValue;
                        if (double.TryParse(alignDim.DimensionText, out oldValue))
                        {
                            alignDim.UpgradeOpen();
                            alignDim.DimensionText = (oldValue * scaleRatio).ToString();
                            alignDim.DowngradeOpen();
                        }
                    }
                }
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary>
        /// </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static ObjectId[] SelectDims(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "DIMENSION"),
                // 将标注类型限制为转角标注与对齐标注
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue(100, "AcDbAlignedDimension)"),
                new TypedValue(100, "AcDbRotatedDimension))"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择标注对象"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。


            var res = ed.GetSelection(pso, filter);

            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            return null;
        }
    }
}