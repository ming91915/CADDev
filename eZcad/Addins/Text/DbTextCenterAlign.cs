using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Addins.Text;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(DbTextCenterAlign))]

namespace eZcad.Addins.Text
{
    /// <summary> 将多个单行文字的对齐方式进行修改，并整体进行竖向中心对齐 </summary>
    [EcDescription(CommandDescription)]
    public class DbTextCenterAlign : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"CenterAlign";
        private const string CommandText = @"文字对齐";
        private const string CommandDescription = @"将多个单行文字的对齐方式进行修改，并整体进行竖向中心对齐";

        /// <summary> 将多个单行文字的对齐方式进行修改，并整体进行竖向中心对齐 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void CenterAlign()
        {
            DocumentModifier.ExecuteCommand(CenterAlign);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new DbTextCenterAlign();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.CenterAlign,
                impliedSelection, ref errorMessage, ref elementSet);
        }


        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将多个单行文字的对齐方式进行修改，并整体进行竖向中心对齐 </summary>
        public ExternalCmdResult CenterAlign(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            var texts = GetDbTexts(docMdf);
            if (texts.Count == 0) { return ExternalCmdResult.Commit; }
            //
            Point3d basePt;
            var succ = GetPoint(docMdf.acEditor, out basePt);
            if (!succ) { return ExternalCmdResult.Cancel; }

            var baseX = basePt.X;
            foreach (var txt in texts)
            {
                txt.UpgradeOpen();
                // txt.Position = new Point3d(30,30,0);
                //  txt.SetAlignment();
                txt.SetAlignment( TextVerticalMode.TextVerticalMid, TextHorizontalMode.TextCenter);
                //txt.Justify = AttachmentPoint.MiddleCenter;
                var alignPt = txt.AlignmentPoint;
                // txt.Position = new Point3d(30,30,0);
                txt.AlignmentPoint = new Point3d(baseX, alignPt.Y, alignPt.Z);
                txt.DowngradeOpen();
            }

            return ExternalCmdResult.Commit;
        }

        #region ---   界面操作

        /// <summary> 选择多个单行文字 </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static List<DBText> GetDbTexts(DocumentModifier docMdf)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Add our keywords
            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n 选择多个单行文字" + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n 选择多个单行文字 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            var f = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "TEXT") });
            var texts = new List<DBText>();
            var res = docMdf.acEditor.GetSelection(pso, f);
            if (res.Status == PromptStatus.OK)
            {
                var ids = res.Value.GetObjectIds();
                foreach (var id in ids)
                {
                    var text = id.GetObject(OpenMode.ForRead) as DBText;
                    if (text != null)
                    {
                        texts.Add(text);
                    }
                }
            }
            return texts;
        }

        /// <summary> 在界面中选择一个点 </summary>
        /// <param name="point">成功获得的三维点</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetPoint(Editor ed, out Point3d point)
        {
            point = default(Point3d);
            var op = new PromptPointOptions(message: "\n 选择一个对齐点：")
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