using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.Utility;
using eZstd.Miscellaneous;

namespace eZcad.Addins
{
    /// <summary> 一个块参照实例，及其对应的所有的块属性定义实例 </summary>
    public class AttriBlock
    {
        public double X { get; }
        public double Y { get; }
        public Handle Handle { get; }

        [Browsable(false)]
        public BlockReference BlockRef { get; }
        public string Space { get; }

        [Browsable(false)]
        public List<AttributeReference> AttRefs { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="blockRef"></param>
        public AttriBlock(BlockReference blockRef, string space)
        {
            //
            BlockRef = blockRef;
            Space = space;
            Handle = blockRef.Handle;
            X = blockRef.Position.X;
            Y = blockRef.Position.Y;
            AttRefs = blockRef.GetAttributeReferences();
        }

        /// <summary> 刷新块参照实例中的块属性定义实例的值 </summary>
        /// <param name="attDef_Value">所有可能的块属性定义及其值</param>
        public void RefreshAttValue(Dictionary<string, string> attDef_Value)
        {
            var defs = attDef_Value.Keys.ToArray();
            var values = attDef_Value.Values.ToArray();
            foreach (var af in AttRefs)
            {
                var index = defs.IndexOf(af.Tag);
                if (index >= 0 && values[index] != af.TextString)
                {
                    // 有对应项而且其字符值发生了修改
                    af.UpgradeOpen();
                    af.TextString = values[index];
                    af.DowngradeOpen();
                }
            }
        }

    }
}
