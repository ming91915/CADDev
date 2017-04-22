using System.Diagnostics;
using System.Windows.Forms;
using AutoCADDev.Examples;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(GraphicalElementsFilter))]

namespace AutoCADDev.Examples
{
    /// <summary> 从用户选择集或者整个文档中过滤出指定信息的对象 </summary>
    internal static class GraphicalElementsFilter
    {
        [CommandMethod("FindCirclesNoUI")]
        public static void FindCirclesNoUI()
        {
            // 获得当前文档和数据库   Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            using (var docLocker = acDoc.LockDocument())
            {
                // 创建一个 TypedValue 数组，用于定义过滤条件
                TypedValue[] acTypValAr = new TypedValue[]
                {
                    // new TypedValue((int)DxfCode.Color, 5),
                    // new TypedValue((int) DxfCode.Start, "CIRCLE"),
                    new TypedValue((int) DxfCode.LayerName, "0")
                };

                // 将过滤条件赋值给SelectionFilter对象
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                //获取当前文档编辑器
                Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

                // 不通过用户界面交互，直接在整个文档中按指定的过滤规则选择出对象
                //PromptSelectionResult quickSelect = acDocEd.SelectAll(acSelFtr);

                PromptSelectionResult quickSelect = acDocEd.SelectCrossingWindow(
                    pt1: new Point3d(0, 0, 0), pt2: new Point3d(1, 1, 0), filter: acSelFtr);

                MessageBox.Show(quickSelect.Status.ToString());

                if (quickSelect.Status == PromptStatus.OK)
                {
                    var ids = quickSelect.Value.GetObjectIds();
                    MessageBox.Show($"选择到的对象数量：{ids.Length}");
                    foreach (ObjectId id in ids)
                    {
                        Debug.Print(id.ObjectClass.Name);
                    }
                }
            }
        }
    }
}