using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins.Text;
using eZcad.Debug;
using eZcad.Utility;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(CentrolizedDimer))]

namespace eZcad.Addins.Text
{
    /// <summary> <seealso cref="CommandDescription"/> </summary>
    [EcDescription(CommandDescription)]
    public class CentrolizedDimer : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CentrolizedDim";
        private const string CommandText = @"集中标注";
        private const string CommandDescription = @"在钢筋标注中搜索集中标注";

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void TextRegex()
        {
            DocumentModifier.ExecuteCommand(CentrolizedDim);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new CentrolizedDimer();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.CentrolizedDim,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> <seealso cref="CommandDescription"/> </summary>
        public ExternalCmdResult CentrolizedDim(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var lines = GetCentralizedDimLine(docMdf);
            if (lines.Count == 0) { return ExternalCmdResult.Cancel; }
            //
            const double angleToler = 0.001;
            const double textDist = 100;
            // 只取水平或者竖直的直线
            lines = lines.Where(r => Math.Abs(r.Angle) < angleToler
            || Math.Abs(r.Angle - 0.5 * Math.PI) < angleToler
            || Math.Abs(r.Angle - Math.PI) < angleToler
            || Math.Abs(r.Angle - 1.5 * Math.PI) < angleToler).ToList();
            docMdf.WriteNow("匹配的直线", lines.Count);
            //
            bool? horiVert = null;
            foreach (var line in lines)
            {
                var angle = line.Angle;
                if (Math.Abs(angle) < angleToler || Math.Abs(angle - Math.PI) < angleToler || Math.Abs(angle - 2.0 * Math.PI) < angleToler)
                {
                    horiVert = true;
                }
                else if (angle - 0.5 * Math.PI < angleToler || angle - 1.5 * Math.PI < angleToler)
                {
                    horiVert = false;
                }
                if (horiVert == null) { continue; }

                //  搜索直线附近的集中标注文字
                bool upRight;
                List<DBText> dimTexts;
                var succ = SearchDimTexts(line, horiVert.Value, textDist, out dimTexts, out upRight);
                if (!succ) continue;
                docMdf.WriteNow("匹配的文本", dimTexts.Count);
                // 设置格式
                line.UpgradeOpen();
                line.Layer = "集中标注线";
                line.DowngradeOpen();
                foreach (var txt in dimTexts)
                {
                    txt.UpgradeOpen();
                    txt.Layer = "集中标注";
                    txt.DowngradeOpen();
                }
            }
            //

            //
            return ExternalCmdResult.Commit;
        }

        /// <summary>
        /// 搜索直线附近的标注文本
        /// </summary>
        /// <param name="line"></param>
        /// <param name="horiVert">直线是水平还是竖直的</param>
        /// <param name="textDist">文本到直线的容许距离值</param>
        /// <param name="upRight">集中标注是否在直线的上方或右侧</param>
        /// <returns></returns>
        private bool SearchDimTexts(Line line, bool horiVert, double textDist, out List<DBText> dimTexts, out bool upRight)
        {
            dimTexts = null;
            upRight = true;
            var ext = line.GeometricExtents;
            var min = ext.MinPoint;
            var max = ext.MaxPoint;
            ext = horiVert
                ? new Extents3d(new Point3d(min.X, min.Y - textDist, min.Z), new Point3d(max.X, max.Y + textDist, max.Z))
                : new Extents3d(new Point3d(min.X - textDist, min.Y, min.Z), new Point3d(max.X + textDist, max.Y, max.Z));
            //
            var texts = FindTexts(ext);
            if (texts == null || texts.Count == 0) return false;
            // 检查是否为集中标注
            var leftDownTexts = new List<DBText>();
            var rightUpTexts = new List<DBText>();
            var txtAngleTol = 0.001;
            if (horiVert)
            {
                foreach (var txt in texts)
                {
                    if (Math.Abs(txt.Rotation - 1.5 * Math.PI) > txtAngleTol && Math.Abs(txt.Rotation - 0.5 * Math.PI) > txtAngleTol)
                    { continue; }
                    var extText = txt.GeometricExtents;
                    if (extText.MinPoint.Y > line.StartPoint.Y)
                    {
                        rightUpTexts.Add(txt);
                    }
                    else if (extText.MaxPoint.Y < line.StartPoint.Y)
                    {
                        leftDownTexts.Add(txt);
                    }
                }
            }
            else
            {
                // 直线为竖直线
                foreach (var txt in texts)
                {
                    if (Math.Abs(txt.Rotation - 0 * Math.PI) > txtAngleTol && Math.Abs(txt.Rotation - 1 * Math.PI) > txtAngleTol)
                    { continue; }
                    var extText = txt.GeometricExtents;
                    if (extText.MinPoint.X > line.StartPoint.X)
                    {
                        rightUpTexts.Add(txt);
                    }
                    else if (extText.MaxPoint.X < line.StartPoint.X)
                    {
                        leftDownTexts.Add(txt);
                    }
                }
            }
            if (leftDownTexts.Count + rightUpTexts.Count == 0) return false;
            // 返回最多的那一侧的文本
            if (leftDownTexts.Count < rightUpTexts.Count)
            {
                dimTexts = rightUpTexts;
                upRight = true;
                return true;
            }
            else if (leftDownTexts.Count > rightUpTexts.Count)
            {
                dimTexts = leftDownTexts;
                upRight = false;
                return true;
            }
            else
            {
                // 左右两边的文字数量相等
                throw new InvalidOperationException("直线左右两侧标注文字数量相等");
            }
            return false;
        }

        /// <summary> 搜索边坡线 </summary>
        private List<DBText> FindTexts(Extents3d extSection)
        {
            SelectionFilter _filter = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "TEXT")
            });

            var res = _docMdf.acEditor.SelectCrossingWindow(
                pt1: extSection.MinPoint,
                pt2: extSection.MaxPoint,
                filter: _filter);

            if (res.Status == PromptStatus.OK)
            {
                var txts = res.Value.GetObjectIds().Select(id => id.GetObject(OpenMode.ForRead)).OfType<DBText>().ToList();
                return txts;
            }
            return null;
        }


        #region ---   界面操作


        public static List<Line> GetCentralizedDimLine(DocumentModifier docMdf)
        {

            // Create our options object
            var op = new PromptSelectionOptions();

            // Set our prompts to include our keywords
            string kws = op.Keywords.GetDisplayString(true);
            op.MessageForAdding = "\n选择集中标注线" + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = op.MessageForAdding;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。

            // Implement a callback for when keywords are entered
            var filterTypes = new TypedValue[]
         {
                new TypedValue((int) DxfCode.Start, "LINE"),
                new TypedValue((int) DxfCode.LayerName, "集中标注线")
         };
            var lines = new List<Line>();
            // Finally run the selection and show any results
            var res = docMdf.acEditor.GetSelection(op, new SelectionFilter(filterTypes));

            if (res.Status == PromptStatus.OK)
            {
                var ids = res.Value.GetObjectIds();
                foreach (var id in ids)
                {
                    var o = id.GetObject(OpenMode.ForRead) as Line;
                    if (o != null)
                    {
                        lines.Add(o);
                    }
                }
            }
            return lines;
        }

        /// <summary> 选择单行或者多行文字 </summary>
        public static ObjectId[] SelectTexts(DocumentModifier docMdf)
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Start, "TEXT"),
                new TypedValue((int) DxfCode.Start, "MTEXT"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };

            var op = new PromptSelectionOptions();
            // Set our prompts to include our keywords
            op.MessageForAdding = "\n选择单行或者多行文字以进行正则表达式查询或替换"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = op.MessageForAdding;


            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetSelection(new SelectionFilter(filterTypes));

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value;
                return acSSet.GetObjectIds();
            }
            return null;
        }

        /// <summary> 在命令行中获取一个字符 </summary>
        /// <param name="value">成功获得的数值</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetRegexPattern(Editor ed, out bool showtips, out bool ignoreCase, out string value)
        {
            value = "";
            showtips = false;
            ignoreCase = false;
            var op = new PromptKeywordOptions("正则表达式")
            {
                AllowArbitraryInput = true,
            };
            op.SetMessageAndKeywords(messageAndKeywords: "\n查询正则表达式[提示(T) / 不区分大小写(C)]:",
                    globalKeywords: "提示 不区分大小写"); // 默认值写在前面
            //
            var res = ed.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.StringResult;
                if (value == "提示")
                {
                    showtips = true;
                }
                else if (value == "不区分大小写")
                {
                    ignoreCase = true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}