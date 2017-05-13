using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(DBTextsToMText))]

namespace eZcad.Addins
{
    /// <summary> 将单行文字转换为多行文字 </summary>
    public class DBTextsToMText
    {
        /// <summary> 将单行文字转换为多行文字 </summary>
        [CommandMethod("eZcad", "DBTextsToMText", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void EcConvertDBTextsToMText()
        {
            DocumentModifier.ExecuteCommand(ConvertDBTextsToMText);
        }

        /// <summary> 将单行文字转换为多行文字 </summary>
        public static void ConvertDBTextsToMText(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            if (ManualMode(docMdf))
            {
                ConvertInManualMode(docMdf);
            }
            else
            {
                ConvertInAutoMode(docMdf);
            }
        }

        private static void ConvertInAutoMode(DocumentModifier docMdf)
        {
            var texts = SelectTexts(docMdf);
            if (texts == null || texts.Length == 0) return;

            // 将选择的文字按Y坐标排序
            var sortedTexts = new SortedDictionary<double, Entity>();
            double maxWidth = 0;

            foreach (var txtId in texts)
            {
                double width = 0;
                var txt = txtId.GetObject(OpenMode.ForRead) as Entity;
                if (txt is DBText)
                {
                    var dt = txt as DBText;
                    if (!sortedTexts.ContainsKey(dt.Position.Y))
                    {
                        //
                        width = dt.TextString.Length * dt.Height * dt.WidthFactor * 1.05; // 1.1 为放大系数
                        maxWidth = Math.Max(maxWidth, width);
                        sortedTexts.Add(dt.Position.Y, dt);
                    }
                }
                else if (txt is MText)
                {
                    var mt = txt as MText;
                    if (!sortedTexts.ContainsKey(mt.Location.Y))
                    {
                        width = mt.ActualWidth;
                        maxWidth = Math.Max(maxWidth, width);
                        sortedTexts.Add(mt.Location.Y, mt);
                    }
                }
            }

            var sb = new StringBuilder();
            var textsUd = sortedTexts.Reverse().ToArray(); // 第一个元素的Y坐标值最大，表示在最上方

            foreach (var v in textsUd)
            {
                var txt = v.Value;
                if (txt is DBText)
                {
                    sb.Append(TextUtils.ConvertDbTextSpecialSymbols((txt as DBText).TextString) + @"\P");
                }
                else if (txt is MText)
                {
                    sb.Append((txt as MText).Contents + @"\P");
                }
            }
            //
            var txtHeight = 0.0;
            var location = new Point3d();
            Entity topText = textsUd[0].Value;
            if (topText is DBText)
            {
                var dt = (topText as DBText);
                txtHeight = dt.Height;
                location = new Point3d(dt.Position.X, dt.Position.Y + dt.Height, dt.Position.Z);
            }
            else if (topText is MText)
            {
                txtHeight = (topText as MText).TextHeight;
                location = (topText as MText).Location;
            }
            // 以只读方式打开块表   Open the Block table for read
            var acBlkTbl = docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;

            // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
            var btr =
                docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                    BlockTableRecord;

            var mTxt = new MText()
            {
                Location = location,
                Width = maxWidth,
                TextHeight = txtHeight,
                LineSpacingFactor = 0.85,
                Contents = sb.ToString(),
            };
            // 刷格式
            mTxt.SetPropertiesFrom(topText);
            //
            btr.AppendEntity(mTxt);
            docMdf.acTransaction.AddNewlyCreatedDBObject(mTxt, true);

            // 删除原来的文字
            foreach (var ent in sortedTexts.Values)
            {
                ent.UpgradeOpen();
                ent.Erase(true);
            }
        }

        private static void ConvertInManualMode(DocumentModifier docMdf)
        {
            MText mTxt = null;
            DBText txt = null;
            txt = PickDbText(docMdf);
            double mTextWidth = 0;
            if (txt != null)
            {
                txt.Highlight(); // 让此文字显示为被选中的状态
                mTextWidth = txt.TextString.Length * txt.Height * txt.WidthFactor;
                // 以只读方式打开块表   Open the Block table for read
                var acBlkTbl = docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;

                // 以写方式打开模型空间块表记录   Open the Block table record Model space for write
                var acBlkTblRec =
                    docMdf.acTransaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;

                mTxt = new MText()
                {
                    Contents = txt.TextString,
                    Location = new Point3d(txt.Position.X, txt.Position.Y + txt.Height, txt.Position.Z),
                    Width = mTextWidth,
                    TextHeight = txt.Height,
                };
                // 刷格式
                mTxt.SetPropertiesFrom(txt);

                acBlkTblRec.AppendEntity(mTxt);
                docMdf.acTransaction.AddNewlyCreatedDBObject(mTxt, true);

                txt.UpgradeOpen();
                txt.Erase(true);
            }
            //
            if (mTxt != null)
            {
                //
                txt = PickDbText(docMdf);
                while (txt != null)
                {
                    txt.Highlight(); // 让此文字显示为被选中的状态

                    double dbTxtWidth = txt.TextString.Length * txt.Height * txt.WidthFactor;

                    if (dbTxtWidth > mTextWidth)
                    {
                        mTextWidth = dbTxtWidth;
                        mTxt.Width = mTextWidth;
                    }
                    mTxt.Contents += "\\P" + txt.TextString;  // “\P”为 MText 中专门用来表示换行的符号
                    // mTxt.Draw();
                    //
                    txt.UpgradeOpen();
                    txt.Erase(true);
                    docMdf.acEditor.UpdateScreen();
                    //
                    txt = PickDbText(docMdf);
                }
            }
        }

        #region ---   界面交互

        private static bool ManualMode(DocumentModifier docMdf)
        {

            Dictionary<string, string> ss = new Dictionary<string, string> { { "e", "te" } };

            var option = new PromptKeywordOptions(
                messageAndKeywords: "\n将单行文字转换为多行文字\n[手动(M) / 自动(A)]:",
                globalKeywords: "手动 自动");
            option.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(option);
            if (res.Status == PromptStatus.OK && res.StringResult == "手动")
            {
                return true;
            }
            return false;
        }

        private static DBText PickDbText(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一个单行文字 ");
            peO.SetRejectMessage("\n 请选择一个单行文字\n");
            peO.AddAllowedClass(typeof(DBText), exactMatch: false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            DBText curve = null;
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                curve = docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as DBText;
            }
            return curve;
        }

        /// <summary>选择多个单行或者多行文字 </summary>
        /// <param name="docMdf"></param>
        /// <returns></returns>
        private static ObjectId[] SelectTexts(DocumentModifier docMdf)
        {
            var ed = docMdf.acEditor;

            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Start, "TEXT"),
                new TypedValue((int) DxfCode.Start, "MTEXT"),
                new TypedValue((int) DxfCode.Operator, "OR>")
            };
            var filter = new SelectionFilter(filterType);

            // Create our options object
            var pso = new PromptSelectionOptions();
            var kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = $"\n选择单行或者多行文字"; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
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