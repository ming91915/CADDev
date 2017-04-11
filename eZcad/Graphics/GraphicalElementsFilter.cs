using AutoCADDev;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof (MyCommands))]

namespace AutoCADDev.Graphics
{
    /// <summary> 从用户选择集或者整个文档中过滤出指定信息的对象 </summary>
    internal static class GraphicalElementsFilter
    {
        /// <summary> 从用户选择集或者整个文档中过滤出指定信息的对象 </summary>
        /// <remarks></remarks>
        [CommandMethod("FindElements", CommandFlags.UsePickSet)]
        public static void FindElements()
        {
            LayerFilter f = new LayerFilter();
        
        }


        [CommandMethod("FilterBlueCircleOnLayer0")]
        public static void FilterBlueCircleOnLayer0()
        {
            //获取当前文档编辑器
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr = new TypedValue[3];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Color, 5), 0);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, "CIRCLE"), 1);
            acTypValAr.SetValue(new TypedValue((int) DxfCode.LayerName, "0"), 2);

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // 请求在图形区域选择对象
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acDocEd.GetSelection(acSelFtr);

            // 如果提示状态OK，表示对象已选
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                SelectionSet acSSet = acSSPrompt.Value;
                Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects selected: 0");
            }
        }
    }
}