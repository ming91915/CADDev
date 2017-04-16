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

        [CommandMethod("FilterBlueCircleOnLayer0")]
        public static void FilterBlueCircleOnLayer0()
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            TypedValue[] acTypValAr = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Color, 5),
                new TypedValue((int) DxfCode.Start, "CIRCLE"),
                new TypedValue((int) DxfCode.LayerName, "0")
            };

            // 将过滤条件赋值给SelectionFilter对象
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            //获取当前文档编辑器
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // 请求在图形区域选择对象
            PromptSelectionResult acSSPrompt = acDocEd.GetSelection(acSelFtr);

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

        [CommandMethod("GetSelectionWithKeywords")]
        public static void GetSelectionWithKeywords()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Create our options object
            PromptSelectionOptions pso = new PromptSelectionOptions();

            // Add our keywords
            pso.Keywords.Add("First");
            pso.Keywords.Add("Second");

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\nPlease add objects to selection or " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\nPlease remove objects from selection or " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    ed.WriteMessage("\nKeyword entered: {0}", e.Input);
                };

            // Finally run the selection and show any results
            PromptSelectionResult psr = ed.GetSelection(pso);

            if (psr.Status == PromptStatus.OK)
            {
                ed.WriteMessage($"\n{psr.Value.Count} object{(psr.Value.Count == 1 ? "" : "s")} selected.");
            }
        }


        [CommandMethod("GetAngleWithKeywords")]
        public static void GetAngleWithKeywords()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            var pao = new PromptAngleOptions(
                messageAndKeywords: "\n指定对角点或[栏选(F) / 圈围(WP) / 圈交(CP)]:",
                globalKeywords: "栏选1 圈围2 圈交3");

            // 在界面中选择一个角度
            PromptDoubleResult pdr = ed.GetAngle(pao);

            MessageBox.Show(pdr.Status.ToString());
            if (pdr.Status == PromptStatus.Keyword)
            {
                MessageBox.Show($"用户输入的关键字为“{pdr.StringResult}”"); // 用户在命令行中输入的关键字

                //　根据用户在命令行中输入的不同的关键字，进行对应的处理
                switch (pdr.StringResult)
                {
                    case "栏选1":
                        break;
                    case "圈围2":

                        break;
                    case "圈交3":

                        break;
                    default: // 如果用户不输入任何关键字，则StringResult属性的值为null

                        break;
                }
            }
            else if (pdr.Status == PromptStatus.OK) // 用户选择结束
            {
                MessageBox.Show(pdr.Value.ToString()); // 用户在界面中选择的角度值
            }
        }
    }
}