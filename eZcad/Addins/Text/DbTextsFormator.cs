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

[assembly: CommandClass(typeof(DbTextsFormator))]

namespace eZcad.Addins.Text
{
    /// <summary> 将多个单行文字按其定位进行组合 </summary>
    [EcDescription(CommandDescription)]
    public class DbTextsFormator : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"FormatDbTexts";
        private const string CommandText = @"文字拆分";
        private const string CommandDescription = @"将多个单行文字按其定位进行拆分或组合";

        /// <summary> 将多个单行文字按其定位进行拆分或组合 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void FormatDbTexts()
        {
            DocumentModifier.ExecuteCommand(FormatDbTexts);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new DbTextsFormator();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.FormatDbTexts,
                impliedSelection, ref errorMessage, ref elementSet);
        }


        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将多个单行文字按其定位进行拆分或组合 </summary>
        public ExternalCmdResult FormatDbTexts(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
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
            return ExternalCmdResult.Commit;
        }

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
            SetTextHeight();
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
            // if (txtStr.Length == 1) return;
            // 单行文字中至少有一个字符

            bool lasteIsEng = (int)txtStr[0] < 127;
            TextLanguage curTextLanguage = new TextLanguage(isEng: lasteIsEng);
            var sepTexts = new List<TextLanguage> { curTextLanguage }; // 分割好的文字
            for (int i = 0; i < txtStr.Length; i++)
            {
                char c = txtStr[i];
                // 如果是空格，则保持其前面的中英文类型
                if (char.IsWhiteSpace(c))
                {
                    curTextLanguage.Append(c);
                }
                // 1、用ASCII码判断：在 ASCII码表中，英文的范围是0 - 127，而汉字则是大于127。
                else if ((int)c > 127) // 说明是中文
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
            double selfMoveY;
            double nextMoveX;
            var firstT = sepTexts[0];
            if (sepTexts.Count == 1)
            {
                // 只修改字高，不修改定位
                txt.UpgradeOpen();
                txt.Height = GetStringHeight(firstT._isEng, out nextMoveX, out selfMoveY);
                txt.DowngradeOpen();
            }
            else if (sepTexts.Count > 1)
            {
                DBText leftTxt = txt;
                DBText rightTxt;
                Point3d oriLeftBottom = leftTxt.Position; // 全部文本的最左下角点

                // 先处理第一批字符
                leftTxt.UpgradeOpen();
                leftTxt.TextString = firstT.Text;

                // 将对齐方式转换为默认的左对齐
                leftTxt.SetAlignment();
                //
                leftTxt.Height = GetStringHeight(firstT._isEng, out nextMoveX, out selfMoveY);
                leftTxt.Position = new Point3d(oriLeftBottom.X, oriLeftBottom.Y + selfMoveY, oriLeftBottom.Z);

                // GetStringWidth(firstT._isEng, firstT.Text.Length, originalHeight, widthFactor);
                leftTxt.DowngradeOpen();
                //
                var leftWidth = 0.0; // GetTextWidth(leftTxt); // 左侧文本的左下角点到全部文本最左下角点的距离
                var rota = leftTxt.Rotation;
                var thisMoveX = nextMoveX;
                Point3d startPosition; ; // 字符末端的坐标
                // 处理后面的字符串
                for (int i = 1; i < sepTexts.Count; i++)
                {
                    var tl = sepTexts[i];
                    rightTxt = leftTxt.GetTransformedCopy(Matrix3d.Identity) as DBText;
                    rightTxt.TextString = tl.Text;
                    rightTxt.Height = GetStringHeight(tl._isEng, out nextMoveX, out selfMoveY);
                    startPosition = GetStartPosition(oriLeftBottom, ref leftWidth, leftTxt, rota, thisMoveX, selfMoveY);
                    rightTxt.Position = startPosition;
                    //
                    thisMoveX = nextMoveX;
                    leftTxt = rightTxt;
                    // GetStringWidth(tl._isEng, tl.Text.Length, originalHeight, widthFactor);
                    // 添加到数据库中
                    btr.AppendEntity(rightTxt);
                    docMdf.acTransaction.AddNewlyCreatedDBObject(rightTxt, true);
                }
            }
        }


        /// <summary> 右侧单行文本的起始端（左下角）坐标 </summary>
        /// <param name="oriLeftBottom">全部文本的最左下角点</param>
        /// <param name="leftWidth">左侧文本的左下角点到全部文本最左下角点的距离</param>
        /// <param name="leftTxt">左侧文本</param>
        /// <param name="rota">文本的旋转角度，单位为弧度</param>
        /// <param name="y0">右侧文本要在左侧基础上在Y方向上移动的距离</param>
        /// <returns></returns>
        private Point3d GetStartPosition(Point3d oriLeftBottom, ref double leftWidth, DBText leftTxt, double rota, double moveX, double y0)
        {
            var x0 = leftWidth + leftTxt.GetTextWidth() + moveX;
            //
            var r = Math.Sqrt(x0 * x0 + y0 * y0);
            var a0 = eZstd.Mathematics.MathUtils.GetAngleR(x0, y0);
            //
            var dx = r * Math.Cos(a0 + rota);
            var dy = r * Math.Sin(a0 + rota);
            //
            leftWidth = x0;
            return new Point3d(oriLeftBottom.X + dx, oriLeftBottom.Y + dy, oriLeftBottom.Z);
        }

        /// <summary> 中文与英文的字高，以逗号（中文逗号或英文逗号）分隔。比如“3.5,2.5”，表示中文字高3.5，英文字高2.5 </summary>
        private static string CharHeight;
        private static double ChiHeight = 350;
        private static double EngHeight = 250;

        /// <summary> 根据字条串是中文还是英文来设计不同的x与y方向上的偏移 </summary>
        /// <param name="isEng"></param>
        /// <param name="nextMoveX">下一个文字在X方向上移动的距离</param>
        /// <param name="selfMoveY">文字在Y方向上移动的距离</param>
        /// <returns></returns>
        private double GetStringHeight(bool isEng, out double nextMoveX, out double selfMoveY)
        {
            if (isEng)
            {
                nextMoveX = 1.2 * (EngHeight / 2.5); //  以英文的字高为 2.5 mm 作为基准
                selfMoveY = 0.5 * (EngHeight / 2.5);  //  以英文的字高为 2.5 mm 作为基准
                return EngHeight; // 英文的字高为 2.5 mm
            }
            else
            {
                nextMoveX = 0.5 * (ChiHeight / 3.5);  //  以中文的字高为 3.5 mm 作为基准
                selfMoveY = 0;   //  以中文的字高为 3.5 mm 作为基准
                return ChiHeight; // 中文的字高为 3.5 mm
            }
        }


        /// <summary> 通过命令行交互，设置中文与英文的字高，以逗号（中文逗号或英文逗号）分隔。比如“2.5,2.5”，表示中文字高3.5，英文字高2.5 </summary>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private bool SetTextHeight()
        {

            var defaultValue = ChiHeight.ToString("0.00") + "," + EngHeight.ToString("0.00");
            //
            var op = new PromptStringOptions(message: "\n设置中英文字高：")
            {
                AllowSpaces = false,
                DefaultValue = defaultValue,
                UseDefaultValue = true
            };
            //
            var res = _docMdf.acEditor.GetString(op);
            if (res.Status == PromptStatus.OK)
            {
                var v = res.StringResult;
                var s = v.Split(',');
                double num;
                var isNum = double.TryParse(s[0], out num);
                if (isNum)
                {
                    ChiHeight = num;
                }
                if (s.Length > 1)
                {
                    isNum = double.TryParse(s[1], out num);
                    if (isNum)
                    {
                        EngHeight = num;
                    }
                }
                return true;
            }
            return false;
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
                delegate (object sender, SelectionTextInputEventArgs e)
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
            var f = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "TEXT") });
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