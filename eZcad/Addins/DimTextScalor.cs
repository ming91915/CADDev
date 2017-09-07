using System;
using System.Collections.Generic;
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

[assembly: CommandClass(typeof(DimTextScalor))]

namespace eZcad.Addins
{
    /// <summary> 标注尺寸的单位修改 </summary>
    public class DimTextScalor
    {
        /// <summary> 将用户手动修改过的标注进行单位缩放 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "ScaleDimText", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void EcScaleDimText()
        {
            DocumentModifier.ExecuteCommand(ScaleDimText);
        }

        /// <summary> 在新选择集中过滤出与当前选择集不相交的对象 </summary>
        public static ExternalCmdResult ScaleDimText(DocumentModifier docMdf, SelectionSet impliedSelection)
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