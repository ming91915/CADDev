using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Addins.SlopeProtection
{
    public class SlopeData
    {
        public static string AppName = "eZSlopeProtection";
        private const string ctg_Geometry = "几何";
        private const string ctg_Others = "其他";

        #region ---   Mileage

        /// <summary> 表示从来没对里程字符进行过解析，也不知道是否能正常解析 </summary>
        public const double MileageNotCal = -2;

        /// <summary> 表示无法从对应的字符中获取有效的里程值 </summary>
        public const double MileageCanNotCal = -1;

        /// <summary> 里程桩号 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡所对应的桩号")]
        public string MileageStr { get; set; }

        private bool _mileageCalculated;
        private double _mileage = MileageNotCal;

        [Category(ctg_Geometry), ReadOnly(true), Description("边坡所对应的桩号")]
        public double Mileage
        {
            get
            {
                if (!_mileageCalculated)
                {
                    var m = ProtectionUtils.GetMileageFromString(MileageStr);
                    if (m.HasValue)
                    {
                        _mileage = m.Value;
                    }
                    else
                    {
                        _mileage = MileageCanNotCal;
                    }
                    _mileageCalculated = true;
                }
                return _mileage;
            }
        }

        #endregion

        #region ---   Fields

        /// <summary> 是否在道路左边 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("是否在道路左边")]
        public bool OnLeft { get; set; }

        /// <summary> 填方边坡还是挖方边坡 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("填方边坡还是挖方边坡")]
        public bool FillExcav { get; set; }

        /// <summary> 边坡防护类型 </summary>
        [Category(ctg_Others), Description("边坡防护类型")]
        public ProtectionStyle Style { get; set; }

        /// <summary> 边坡斜面长度(不包括马道) </summary>
        [Category(ctg_Geometry), Description("边坡斜面长度(不包括马道)")]
        public double SlopeLength { get; set; }

        /// <summary> 道路中心路面标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("道路中心路面标高")]
        public double CenterElevation { get; set; }

        /// <summary> 边坡面顶部标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡面顶部标高")]
        public double TopElevation { get; set; }

        /// <summary> 边坡面底部标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡面顶部标高")]
        public double BottomElevation { get; set; }

        /// <summary> 水位线以下的边坡斜边长度 </summary>
        [Category(ctg_Geometry), Description("水位线以下的边坡斜边长度")]
        public double SlopeLengthBelowWaterLevel { get; set; }

        /// <summary> 道路中心线所对应的原自然地面的标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("道路中心线所对应的原自然地面的标高")]
        public double NaturalSurfElevation { get; set; }

        /// <summary> 横断面信息所对应的块参照的名柄值，默认值为无效的0 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("横断面信息所对应的块参照的名柄值")]
        public Handle InfoBlockHandle { get; set; }

        /// <summary> 道路中心线所对应的名柄值，默认值为无效的0 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("道路中心线所对应的名柄值")]
        public Handle CenterAxisHandle { get; set; }

        /// <summary> 填方边坡防护顶部标高之下的边坡长度 </summary>
        [Category(ctg_Geometry), Description("填方边坡防护顶部标高之下的边坡长度")]
        public double SlopeLengthBelowFillTop { get; set; }

        #endregion

        #region ---   构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="style">防护形式</param>
        /// <param name="fillExcav">true表示填方，false 表示挖方</param>
        public SlopeData()
        {
            Style = ProtectionStyle.挂网喷锚6;
            FillExcav = false;
            MileageStr = "K0+000";
            OnLeft = false;
        }

        #endregion

        #region ---   数据 与 ResultBuffer 的转换

        public static SlopeData FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            if (buffs.Length > 0 && buffs[0].Value.ToString() == AppName)
            {
                var sl = new SlopeData();
                try
                {
                    sl.FillExcav = (Int16)buffs[1].Value == 1;
                    sl.Style = (ProtectionStyle)Enum.ToObject(typeof(ProtectionStyle), (short)buffs[2].Value);
                    sl.MileageStr = (string)buffs[3].Value;
                    sl.OnLeft = (Int16)buffs[4].Value == 1;
                    sl.SlopeLength = (double)buffs[5].Value;
                    sl.CenterElevation = (double)buffs[6].Value;
                    sl.TopElevation = (double)buffs[7].Value;
                    sl.BottomElevation = (double)buffs[8].Value;
                    sl.SlopeLengthBelowWaterLevel = (double)buffs[9].Value;
                    sl.NaturalSurfElevation = (double)buffs[10].Value;
                    sl.InfoBlockHandle = new Handle(Convert.ToInt64(buffs[11].Value.ToString(), 16));
                    sl.CenterAxisHandle = new Handle(Convert.ToInt64(buffs[12].Value.ToString(), 16));
                    sl.SlopeLengthBelowFillTop = (double)buffs[13].Value;
                }
                catch (Exception ex)
                {
                }

                return sl;
            }
            return null;
        }

        public ResultBuffer ToResultBuffer()
        {
            ResultBuffer buff = new ResultBuffer
                (
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, FillExcav),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, (short)Style),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, MileageStr),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, OnLeft),
                new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLength),
                new TypedValue((int)DxfCode.ExtendedDataReal, CenterElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, TopElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, BottomElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowWaterLevel),
                new TypedValue((int)DxfCode.ExtendedDataReal, NaturalSurfElevation),
                new TypedValue((int)DxfCode.ExtendedDataHandle, InfoBlockHandle),
                new TypedValue((int)DxfCode.ExtendedDataHandle, CenterAxisHandle),
                new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowFillTop)
                );
            return buff;
        }

        public static ResultBuffer ClearValue()
        {
            ResultBuffer buff =
                new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName));
            // 此时，Entity中的XData集合里，对应AppName下的所有数据，连同AppName这一项本身，都在实体中删除了。
            // 但是此AppName在 RegAppTable 中对应的 RegAppTableRecord 定义还是存在的。
            return buff;
        }

        #endregion

        #region --- 信息输出

        public const string InfoHeader = "桩号\t标识\t边坡长度\t边坡高度\t位置\t填挖\t防护" +
                                         "\t坡顶标高\t坡底标高" +
                                         "\t水下坡长\t填方顶以下坡长\t中心路面标高\t中心自然标高\t中心填方高度";

        /// <summary>
        /// 将
        /// </summary>
        /// <returns></returns>
        public string GetInfo()
        {
            var v = GetInfoVector();
            var sb = new StringBuilder();
            foreach (var s in v)
            {
                sb.Append(s.ToString() + ',');
            }
            return sb.ToString();
        }

        public object[] GetInfoVector()
        {
            var left = OnLeft ? "L" : "R";
            var fill = FillExcav ? "填方" : "挖方";

            return new object[]
            {
                Mileage, "测量", SlopeLength, TopElevation - BottomElevation, left, fill, Style.ToString(),
                TopElevation, BottomElevation,
                SlopeLengthBelowWaterLevel, SlopeLengthBelowFillTop, CenterElevation, NaturalSurfElevation,CenterElevation - NaturalSurfElevation
            };
        }

        public static object[,] GetAllInfo(IList<SlopeData> slopeDatas)
        {
            var colCount = InfoHeader.Split('\t').Length;
            var rowCount = slopeDatas.Count;
            var res = new object[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                var sd = slopeDatas[r];
                var rowData = sd.GetInfoVector();
                for (int c = 0; c < colCount; c++)
                {
                    res[r, c] = rowData[c];
                }
            }
            return res;
        }

        #endregion
    }
}