using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Addins.Text
{
    public class TextRegex
    {
        private readonly Dictionary<ObjectId, string> _allTexts;

        /// <summary> 构造函数 </summary>
        /// <param name="texts">单行文字或者多行文字</param>
        public TextRegex(IEnumerable<ObjectId> texts)
        {
            _allTexts = new Dictionary<ObjectId, string>();
            object obj;
            foreach (var text in texts)
            {
                var dxf = text.ObjectClass.DxfName;
                if (dxf == "TEXT" || dxf == "MTEXT")
                {
                    obj = text.GetObject(OpenMode.ForRead);
                    if (obj is DBText)
                    {
                        _allTexts.Add(text, (obj as DBText).TextString);
                    }
                    else if (obj is MText)
                    {
                        _allTexts.Add(text, (obj as MText).Text);
                    }
                }
            }
        }

        /// <summary> 进行文字的匹配 </summary>
        public ObjectId[] Match(string pattern, bool ignoreCase = false)
        {
            Regex regex;
            regex = ignoreCase ? new Regex(pattern, RegexOptions.IgnoreCase) : new Regex(pattern);
            //
            var mmm = new List<ObjectId>();
            foreach (var t in _allTexts)
            {
                if (regex.IsMatch(t.Value))
                {
                    mmm.Add(t.Key);
                }
            }
            var matches = _allTexts.Where(r => regex.IsMatch(r.Value));
            return matches.Select(r => r.Key).ToArray();
        }
    }
}
