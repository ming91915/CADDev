using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using eZcad.Examples;
using eZcad.Utility;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(GraphicalElementsSelector))]

namespace eZcad.Examples
{
    /// <summary> 从用户选择集或者整个文档中过滤出指定信息的对象 </summary>
    internal static class GraphicalElementsSelector
    {

        #region --- GetSelection

        private static void GetSelectionWithFilter()
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Color, 5),
                new TypedValue((int) DxfCode.Start, "CIRCLE"),
                new TypedValue((int) DxfCode.LayerName, "0")
            };
            /*
                var pso = new PromptSelectionOptions();
                pso.Keywords.Add("NoFilter", "无(N)", "无(N)"); //
            */

            /*
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Start, "DIMENSION"),
                // 将标注类型限制为转角标注与对齐标注
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue(100, "AcDbAlignedDimension"),
                new TypedValue(100, "AcDbRotatedDimension"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };
            */


            //获取当前文档编辑器
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // 请求在图形区域选择对象
            var res = acDocEd.GetSelection(new SelectionFilter(filterTypes));

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value;
                Application.ShowAlertDialog("Number of objects selected: " + acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects selected: 0");
            }
        }

        private static void GetSelectionWithKeywords()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Create our options object
            var op = new PromptSelectionOptions();

            // Add our keywords
            op.Keywords.Add("First");
            op.Keywords.Add("Second");

            // Set our prompts to include our keywords
            string kws = op.Keywords.GetDisplayString(true);
            op.MessageForAdding = "\nPlease add objects to selection or " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = "\nPlease remove objects from selection or " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            op.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    ed.WriteMessage("\nKeyword entered: {0}", e.Input);
                };

            // Finally run the selection and show any results
            var res = ed.GetSelection(op);

            if (res.Status == PromptStatus.OK)
            {
                ed.WriteMessage($"\n{res.Value.Count} object{(res.Value.Count == 1 ? "" : "s")} selected.");
            }
        }

        /// <summary> 在  GetSelection 方法中指定关键字与对应的快捷键 </summary>
        /// <returns></returns>
        private static SelectionSet GetSelectionWithKeywordsAndShortcut(DocumentModifier docMdf, ref string filteredDxfName,
            out bool continueSelect)
        {
            var ed = docMdf.acEditor;

            // Create our options object
            var pso = new PromptSelectionOptions();

            pso.Keywords.Add("NoFilter", "无(N)", "无(N)"); //

            // Set our prompts to include our keywords
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择要取交集的对象。\n当前过滤类型：{filteredDxfName} " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = pso.MessageForAdding; // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // 响应事件
            var keywordsInput = false; // 用户在命令行中输入了关键字或者非关键字

            var defDxfName = filteredDxfName;
            pso.UnknownInput += delegate (object sender, SelectionTextInputEventArgs e)
            {
                keywordsInput = true;
                switch (e.Input)
                {
                    case "N": // 表示输入了关键字 NoFilter
                        defDxfName = null;
                        break;
                    case "n": // 表示输入了关键字 NoFilter
                        defDxfName = null;
                        break;
                    default:
                        defDxfName = e.Input;
                        break;
                }
                // !!! ((char)10) 对应按下 Enter 键，这一句会立即提交到AutoCAD 命令行中以结束 ed.GetSelection 对线程的阻塞。即是可以模拟当用户输入关键字时立即按下 Escape，此时 API 会直接结束 ed.GetSelection 并往下执行，其返回的 PromptSelectionResult.Status 属性值为 Error。
                docMdf.acActiveDocument.SendStringToExecute(((char)10).ToString(), true, false, true);
            };

            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate (object sender, SelectionTextInputEventArgs e)
                {
                    keywordsInput = true;
                    switch (e.Input)
                    {
                        case "NoFilter":
                            defDxfName = null;
                            break;
                        default:
                            break;
                    }
                    // !!! ((char)10) 对应按下 Enter 键，这一句会立即提交到AutoCAD 命令行中以结束 ed.GetSelection 对线程的阻塞。即是可以模拟当用户输入关键字时立即按下 Escape，此时 API 会直接结束 ed.GetSelection 并往下执行，其返回的 PromptSelectionResult.Status 属性值为 Error。
                    docMdf.acActiveDocument.SendStringToExecute(((char)10).ToString(), true, false, true);
                };

            // Finally run the selection and show any results

            PromptSelectionResult res = null;
            if (string.IsNullOrEmpty(filteredDxfName))
            {
                res = ed.GetSelection(pso);
            }
            else
            {
                var filterType = new TypedValue[1] { new TypedValue((int)DxfCode.Start, filteredDxfName) };
                res = ed.GetSelection(pso, new SelectionFilter(filterType));
            }

            docMdf.WriteNow(res.Status);

            if (res.Status == PromptStatus.OK)
            {
                continueSelect = false;
                return res.Value;
            }
            if (keywordsInput)
            {
                filteredDxfName = defDxfName;
                continueSelect = true;
                return null;
            }
            continueSelect = false;
            return null;
        }


        /// <summary> 举例，选择多个属性定义对象 </summary>
        private static List<AttributeDefinition> SelectAttibuteDefinitions()
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "ATTDEF"),
            };

            // Create our options object
            var op = new PromptSelectionOptions();

            // Add our keywords
            //op.Keywords.Add("First");
            //op.Keywords.Add("Second");

            // Set our prompts to include our keywords
            string kws = op.Keywords.GetDisplayString(true);
            op.MessageForAdding = "\n 请选择一个或多个属性定义 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = "\nPlease remove objects from selection or " + kws;


            //获取当前文档编辑器
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // 请求在图形区域选择对象
            var res = acDocEd.GetSelection(op, new SelectionFilter(filterTypes));

            var output = new List<AttributeDefinition>();
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value.GetObjectIds();
                foreach (var id in acSSet)
                {
                    var obj = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if (obj != null)
                    {
                        output.Add(obj);
                    }
                }
            }
            return output;
        }

        #endregion

        private static void GetAngleWithKeywords()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var pao = new PromptAngleOptions(
                messageAndKeywords: "\n指定对角点或[栏选(F) / 圈围(WP) / 圈交(CP)]:",
                globalKeywords: "栏选1 圈围2 圈交3");

            // 在界面中选择一个角度
            var pdr = ed.GetAngle(pao);

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

        /// <summary> 通过点选的方式选择一条曲线 </summary>
        private static Curve GetEntity(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一条曲线 ");
            peO.SetRejectMessage("\n 请选择一个曲线对象\n");
            peO.AddAllowedClass(typeof(Curve), exactMatch: false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                return docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as Curve;
            }
            return null;
        }

        /// <summary> 从两个选项中选择一个 </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示第一个选项（默认值），false 表示第二个选项</returns>
        private static bool GetKeywordsFromTwoOptions(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n[按顶点缩放(V) / 按长度缩放(L)]:",
                globalKeywords: "顶点 长度"); // 默认值写在前面
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                if (res.StringResult == "长度")
                {
                    return false;
                }
            }
            return true; // true 表示第一个选项
        }

        /// <summary> 分段的长度 </summary>
        private static double GetDistance(DocumentModifier docMdf)
        {
            var op = new PromptDistanceOptions(message: "\n每个分段的长度")
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

        /// <summary> 在命令行中获取一个整数值 </summary>
        private static int GetInterger(DocumentModifier docMdf)
        {
            var op = new PromptIntegerOptions(message: "\n每隔多少个顶点进行分段")
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



        /// <summary> 在命令行中获取一个字符 </summary>
        /// <param name="value">成功获得的数值</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetString(Editor ed, out string value)
        {
            value = "";
            var op = new PromptStringOptions(message: "\n用来进行剪切的标高：")
            {
                AllowSpaces = true,
                DefaultValue = "默认值",
                UseDefaultValue = true
            };
            //
            var res = ed.GetString(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.StringResult;
                return true;
            }
            return false;
        }

        #region ---   公共静态函数

        /// <summary> 在命令行中获取一个小数值 </summary>
        /// <param name="value">成功获得的数值</param>
        /// <param name="allowArbitraryInput"></param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        public static bool GetDouble(Editor ed, string message, out double value, double defaultValue = 1,
            bool allowNegative = true, bool allowNone = false, bool allowZero = true, bool allowArbitraryInput = false)
        {
            value = double.MaxValue;
            var op = new PromptDoubleOptions(message: $"\n {message}")
            {
                AllowNegative = allowNegative,
                AllowNone = allowNone,
                AllowZero = allowZero,
                AllowArbitraryInput = allowArbitraryInput,
                DefaultValue = defaultValue
            };
            //
            var res = ed.GetDouble(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.Value;
                return true;
            }
            return false;
        }

        /// <summary> 在界面中选择一个点 </summary>
        /// <param name="point">成功获得的三维点</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        public static bool GetPoint(Editor ed, string message, out Point3d point)
        {
            point = default(Point3d);
            var op = new PromptPointOptions(message: $"\n {message}")
            {
            };
            //
            var res = ed.GetPoint(op);
            if (res.Status == PromptStatus.OK)
            {
                point = res.Value;
                return true;
            }
            return false;
        }

        #endregion
    }
}