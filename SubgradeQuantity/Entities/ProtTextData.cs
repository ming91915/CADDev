using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 边坡防护文字对象的数据 </summary>
    public class ProtTextData
    {
        private const string ctg_General = "通用";
        
        public const string RegAppName_SlopeText = @"Ss_SlopeText";
        // public const string RegAppName_Platform = @"Ss_Platform";

        #region --- XData Fields

        [Category(ctg_General), ReadOnly(true), Description("桩号")]
        public double Station { get; set; }

        /// <summary> 位于路基左侧还是右侧 </summary>
        [Category(ctg_General), ReadOnly(false), Description("位于路基左侧还是右侧")]
        public bool Left { get; set; }


        /// <summary> 对应边坡还是平台 </summary>
        [Category(ctg_General), ReadOnly(false), Description("对应边坡还是平台")]
        public bool SlopePlatform { get; set; }

        /// <summary> 子边坡或者子平台的等级编号 </summary>
        [Category(ctg_General), ReadOnly(false), Description("子边坡或者子平台的等级编号")]
        public double Index { get; set; }

        #endregion

        /// <summary> 构造函数 </summary>
        public ProtTextData(double station, bool left, bool slopePlatform, double index)
        {
            Station = station;
            Left = left;
            SlopePlatform = slopePlatform;
            Index = index;
        }

        #region ---   数据 与 ResultBuffer 的转换

        /// <summary>
        /// 利用<seealso cref="buff"/>中的数据对<seealso cref="ProtTextData"/>对象进行数据填充
        /// </summary>
        /// <param name="buff"></param>
        public static ProtTextData FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            if (buffs.Length == 0) return null;
            var appName = buffs[0].Value.ToString();
            if (appName == RegAppName_SlopeText)
            {
                try
                {
                    var station = (double)buffs[1].Value;
                    var left = Utils.GetExtendedDataBool(buffs[2]);
                    var slopePlatform = Utils.GetExtendedDataBool(buffs[3]);
                    var index = (double)buffs[4].Value;
                    //
                    return new ProtTextData(station, left, slopePlatform, index);
                }
                catch (Exception ex)
                {
                    Debug.Print("从 ResultBuffer 中提取出 ProtTextData 信息时出错" + ex.AppendMessage());
                }
            }
            return null;
        }

        public ResultBuffer ToResultBuffer()
        {
            var buff = new ResultBuffer(
                  new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName_SlopeText),
                  new TypedValue((int)DxfCode.ExtendedDataReal, Station),
                  Utils.SetExtendedDataBool(Left),
                  Utils.SetExtendedDataBool(SlopePlatform),
                  new TypedValue((int)DxfCode.ExtendedDataReal, Index)
                  );
            return buff;
        }

        /// <summary> 清除 XData 中的数据 </summary>
        /// <param name="clearAll">如果其值为 true，则<paramref name="type"/>值不起作用</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ResultBuffer ClearValue()
        {
            ResultBuffer buff = null;
            buff =
                new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName_SlopeText));
            // 此时，Entity中的XData集合里，对应AppName下的所有数据，连同AppName这一项本身，都在实体中删除了。
            // 但是此AppName在 RegAppTable 中对应的 RegAppTableRecord 定义还是存在的。
            return buff;
        }

        #endregion

        #region --- 信息输出

        public override string ToString()
        {
            var slpPlatform = SlopePlatform ? "边坡" : "平台";
            return $"桩号: {Station}，第 {Index} 级 {slpPlatform}";
        }

        #endregion
    }
}