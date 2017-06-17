using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

[assembly: CommandClass(typeof (DbTextsFormator))]

namespace eZcad.Addins
{
    /// <summary> 将多个单行文字按其定位进行组合 </summary>
    public class DbTextsFormator
    {
        #region --- 命令设计

        /// <summary> 将多个单行文字按其定位进行组合 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "FormatDbTexts", CommandFlags.Modal | CommandFlags.UsePickSet
            )]
        public void EcFormatDbTexts()
        {
            DocumentModifier.ExecuteCommand(FormatDbTexts);
        }

        /// <summary> 将多个单行文字按其定位进行组合 </summary>
        public void FormatDbTexts(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            bool cont = true;
            ObjectId[] texts = null;
            bool sepOrCmb = true; // true 表示将文字进行分割
            while (cont)
            {
                texts = GetDbTexts(docMdf, ref cont, ref sepOrCmb);
            }
            if (texts != null)
            {
                if (sepOrCmb)
                {
                    SeperateDbTexts(docMdf, texts);
                }
                else
                {
                    var txts = texts.Select(txtid => txtid.GetObject(OpenMode.ForRead) as DBText);
                    CombineDbTexts(docMdf, txts);
                }
            }
        }

        #endregion

        #region --- ComineTexts

        /// <summary> 将多个单行文字按其定位进行组合 </summary>
        /// <param name="texts"></param>
        public void CombineDbTexts(DocumentModifier docMdf, IEnumerable<DBText> texts)
        {
            var arr2D = new EntityArray2D<DBText>(texts);
            var textsArr2D = arr2D.Arrange2D();
            //
            for (int r = 0; r < textsArr2D.GetLength(0); r++)
            {
                // 将一行中的所有文字转换到一个单行文字中
                var sb = new StringBuilder();
                DBText baseText = null;
                for (int c = 0; c < textsArr2D.GetLength(1); c++)
                {
                    var cellTexts = textsArr2D[r, c];
                    if (cellTexts.Count > 0)
                    {
                        if (baseText == null)
                        {
                            baseText = cellTexts.First();
                        }
                        foreach (var t in cellTexts)
                        {
                            sb.Append(t.TextString);
                            if (!t.Equals(baseText))
                            {
                                t.UpgradeOpen();
                                t.Erase(true);
                            }
                        }
                    }

                    if (baseText != null)
                    {
                        baseText.UpgradeOpen();
                        baseText.TextString = sb.ToString();
                    }
                }
            }
        }

        #endregion

        #region ---   SeperateDbText

        /// <summary> 将单行文字中的字符按中英文进行分割，并设置不同的字号 </summary>
        /// <param name="docMdf"></param>
        /// <param name="texts"></param>
        private void SeperateDbTexts(DocumentModifier docMdf, ObjectId[] texts)
        {
            var blkTb =
                docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr =
                docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                    BlockTableRecord;
            foreach (var tId in texts)
            {
                var txt = tId.GetObject(OpenMode.ForRead) as DBText;
                if (txt != null)
                {
                    SeperateOneDbText(docMdf, txt, btr);
                }
            }
        }

        private void SeperateOneDbText(DocumentModifier docMdf, DBText txt, BlockTableRecord btr)
        {
            var txtStr = txt.TextString;
            if (txtStr.Length == 0) return;
            if (txtStr.Length == 1) return;
            // 单行文字中至少有两个字符

            bool lasteIsEng = (int) txtStr[0] < 127;
            TextLanguage curTextLanguage = new TextLanguage(isEng: lasteIsEng);
            var sepTexts = new List<TextLanguage> {curTextLanguage}; // 分割好的文字
            for (int i = 0; i < txtStr.Length; i++)
            {
                char c = txtStr[i];
                // 1、用ASCII码判断：在 ASCII码表中，英文的范围是0 - 127，而汉字则是大于127。
                if ((int) c > 127) // 说明是中文
                {
                    if (lasteIsEng)
                    {
                        curTextLanguage = new TextLanguage(false);
                        sepTexts.Add(curTextLanguage);
                        //
                    }
                    curTextLanguage.Append(c);
                    lasteIsEng = false;
                }
                else // 说明是英文
                {
                    if (!lasteIsEng)
                    {
                        curTextLanguage = new TextLanguage(true);
                        sepTexts.Add(curTextLanguage);
                        //
                    }
                    curTextLanguage.Append(c);
                    lasteIsEng = true;
                }
            }
            // ------------- 对分割好的文字进行处理
            double moveY;
            var firstT = sepTexts[0];
            if (sepTexts.Count == 1)
            {
                // 只修改字高，不修改定位
                txt.UpgradeOpen();
                txt.Height = GetStringHeight(firstT._isEng, out moveY);
                txt.DowngradeOpen();
            }
            else if (sepTexts.Count > 1)
            {
                var width = 1.0d;
                var originalPosition = txt.Position;

                var baseY = originalPosition.Y;
                // 先处理第一批字符
                txt.UpgradeOpen();
                txt.TextString = firstT.Text;
                txt.Height = GetStringHeight(firstT._isEng, out moveY);
                txt.Position = new Point3d(originalPosition.X, originalPosition.Y + moveY, originalPosition.Z);

                width = GetTextWidth(txt);
                    // GetStringWidth(firstT._isEng, firstT.Text.Length, originalHeight, widthFactor);
                txt.DowngradeOpen();

                // 处理后面的字符串
                for (int i = 1; i < sepTexts.Count; i++)
                {
                    var tl = sepTexts[i];
                    txt = txt.GetTransformedCopy(Matrix3d.Displacement(new Vector3d(width, 0, 0))) as DBText;
                    txt.TextString = tl.Text;
                    txt.Height = GetStringHeight(tl._isEng, out moveY);
                    txt.Position = new Point3d(txt.Position.X, baseY + moveY, txt.Position.Z);
                    //
                    width = GetTextWidth(txt);
                        // GetStringWidth(tl._isEng, tl.Text.Length, originalHeight, widthFactor);
                    // 添加到数据库中
                    btr.AppendEntity(txt);
                    docMdf.acTransaction.AddNewlyCreatedDBObject(txt, true);
                }
            }
        }

        private static double GetTextWidth(DBText txt)
        {
            var b = txt.Bounds.Value;
            return b.MaxPoint.X - b.MinPoint.X + 1; // 0.5mm 为两种语言之间的间隔
        }

        private static double GetStringWidth(bool isEng, int length, double height, double widthfactor)
        {
            var baseWidth = height*widthfactor*length;
            if (isEng)
            {
                return baseWidth*1.2;
            }
            else
            {
                return baseWidth*1.5;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isEng"></param>
        /// <param name="moveY">文字在Y方向上移动的距离</param>
        /// <returns></returns>
        private static double GetStringHeight(bool isEng, out double moveY)
        {
            if (isEng)
            {
                moveY = 0.5;
                return 2.5; // 英文的字高为 2.5 mm
            }
            else
            {
                moveY = 0;
                return 3.5; // 中文的字高为 3.5 mm
            }
        }

        private class TextLanguage
        {
            public readonly bool _isEng;
            private StringBuilder _text;

            public string Text
            {
                get { return _text.ToString(); }
            }

            public TextLanguage(bool isEng)
            {
                _isEng = isEng;
                _text = new StringBuilder();
            }

            public void Append(string s)
            {
                _text.Append(s);
            }

            public void Append(char s)
            {
                _text.Append(s);
            }
        }

        #endregion

        #region ---   界面操作

        /// <summary> 选择多个单行文字 </summary>
        /// <param name="docMdf"></param>
        /// <param name="ContinueSelect"></param>
        /// <param name="sepOrCmb"></param>
        /// <returns></returns>
        private static ObjectId[] GetDbTexts(DocumentModifier docMdf, ref bool ContinueSelect, ref bool sepOrCmb)
        {
            // Create our options object
            var pso = new PromptSelectionOptions();

            // Add our keywords
            pso.Keywords.Add("Seperate");
            pso.Keywords.Add("COmbine");

            // Set our prompts to include our keywords
            string kws = pso.Keywords.GetDisplayString(true);
            pso.MessageForAdding = "\n选择多个单行文字" + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            pso.MessageForRemoval = "\n选择多个单行文字 " + kws;
            // 当用户在命令行中输入Re（或Remove）时，命令行出现的提示字符。
            bool sep = sepOrCmb;
            // Implement a callback for when keywords are entered
            // 当用户在命令行中输入关键字时进行对应操作。
            pso.KeywordInput +=
                delegate(object sender, SelectionTextInputEventArgs e)
                {
                    if (e.Input.Equals("Seperate", StringComparison.CurrentCultureIgnoreCase))
                    {
                        docMdf.WriteNow("\n将选择的多行文字按中英文进行分割");
                        pso.MessageForAdding = "将选asdf择的多行文字按中英文进行分割";
                        sep = true;
                    }
                    else if (e.Input.Equals("COmbine", StringComparison.CurrentCultureIgnoreCase))
                    {
                        docMdf.WriteNow("\n将选择的多行文字按定位进行组合");
                        sep = false;
                    }
                };
            // Finally run the selection and show any results
            var f = new SelectionFilter(new TypedValue[] {new TypedValue((int) DxfCode.Start, "TEXT")});
            var psr = docMdf.acEditor.GetSelection(pso, f);
            sepOrCmb = sep;
            if (psr.Status == PromptStatus.OK)
            {
                ContinueSelect = false;
                return psr.Value.GetObjectIds();
            }
            else
            {
                ContinueSelect = false;
            }
            return null;
        }

        #endregion
    }
}