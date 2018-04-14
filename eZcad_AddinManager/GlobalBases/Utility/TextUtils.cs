using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Utility
{
    public static class TextUtils
    {
        #region ---   特殊符号的转换 ConvertDbTextSpecialSymbols

        /// <summary> 将单行文字中的特殊字符转换为对应的数据，以填充到多行文字中，使其显示效果相同 </summary>
        /// <param name="dbText">以 Hztxt 作为字体的单行文字对象中的文本</param>
        /// <returns></returns>
        public static string ConvertDbTextSpecialSymbols(this DBText dbText)
        {
            //dbText = @"输入两个个钢筋符号《%%130和%%134》";
            //var res = @"输入两个个钢筋符号《和》";

            var replacedText = SpecialSymbolConvertor.Replace(dbText.TextString, evaluator: SpecSybReplacor);
            return replacedText;
        }

        /// <summary> 将单行文字中的特殊字符转换为对应的数据，以填充到多行文字中，使其显示效果相同 </summary>
        /// <param name="dbText">以 Hztxt 作为字体的单行文字对象中的文本</param>
        /// <returns></returns>
        public static string ConvertDbTextSpecialSymbols(string dbText)
        {
            //dbText = @"输入两个个钢筋符号《%%130和%%134》";
            //var res = @"输入两个个钢筋符号《和》";

            var replacedText = SpecialSymbolConvertor.Replace(dbText, evaluator: SpecSybReplacor);
            return replacedText;
        }

        /// <summary>
        /// HZTXT 字体中的特殊符号与对应的编码
        /// </summary>
        private static Dictionary<string, string> HztxtSpecialSymbols = new Dictionary<string, string>()
        {
            {"%%129", SsDiameter},
            {"%%130", SsHPB300},
            {"%%131", SsHRB335},
            {"%%132", SsUpper2}, // 上标 2
            {"%%133", ssUpper3}, // 上标 3
            {"%%134", SsHRB400},
        };

        /// <summary> 上标 2 </summary>
        private const string SsUpper2 = @"";

        /// <summary> 上标 3 </summary>
        private const string ssUpper3 = @"{\H0.7x;\S3^;}"; // @"
                                                           // "; // @"{\H0.5x;\S3^;}"}, // 上标 3

        /// <summary> 直径符号(圆+斜竖线) </summary>
        private const string SsDiameter = @"";
        /// <summary> HPB300 的钢筋符号 </summary>
        private const string SsHPB300 = @"";
        /// <summary> HRB335 的钢筋符号（不再生产 20170510） </summary>
        private const string SsHRB335 = @"";
        /// <summary> HRB400 的钢筋符号 </summary>
        private const string SsHRB400 = @"";

        private static Regex SpecialSymbolConvertor = new Regex(pattern: @"(%%\d{3})");

        private static string SpecSybReplacor(Match m)
        {
            var v = m.Value;
            if (HztxtSpecialSymbols.ContainsKey(v))
            {
                return HztxtSpecialSymbols[v];
            }
            else
            {
                return v;
            }
        }

        #endregion

    }
}
