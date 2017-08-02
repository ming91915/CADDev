using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 每一侧边坡的所有数据 </summary>
    public class SlopeData
    {
        public static string AppName = "eZSubgradeQuantity";
        private const string ctg_Geometry = "几何";
        private const string ctg_Others = "其他";

        #region ---   XData Fields

        #region --- General

        /// <summary> 是否进行过一次完整的计算 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("计算值")]
        public bool FullyCalculated { get; set; }

        /// <summary> 道路中心线所对应的名柄值，默认值为无效的0 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("道路中心线所对应的名柄值")]
        public Handle CenterAxisHandle { get; set; }

        /// <summary> 边坡所在桩号 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("边坡所在桩号")]
        public double Station { get; set; }

        /// <summary> 是否在道路左边 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("是否在道路左边")]
        public bool OnLeft { get; set; }

        /// <summary> 填方边坡还是挖方边坡 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("填方边坡还是挖方边坡")]
        public bool FillCut { get; set; }

        // -------------------------------------- 标高与长度

        /// <summary> 边坡线顶部标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡线顶部标高")]
        public double TopElevation { get; set; }

        /// <summary> 边坡线底部标高 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡线底部标高")]
        public double BottomElevation { get; set; }

        ///// <summary> 水位线以下的边坡斜边长度 </summary>
        //[Category(ctg_Geometry), Description("水位线以下的边坡斜边长度")]
        //public double SlopeLengthBelowWaterLevel { get; set; }

        ///// <summary> 填方边坡防护顶部标高之下的边坡长度 </summary>
        //[Category(ctg_Geometry), Description("填方边坡防护顶部标高之下的边坡长度")]
        //public double SlopeLengthBelowFillTop { get; set; }

        #endregion

        #region ---   多级边坡分别防护

        /// <summary> 每一级斜坡或者平坡，靠近道路中线的边坡对象位于位于集合的前面 </summary>
        [Category(ctg_Others), Description("边坡")]
        public List<Slope> Slopes { get; set; }

        /// <summary> 每一级平台，靠近道路中线的平台对象位于位于集合的前面 </summary>
        [Category(ctg_Others), Description("边坡平台")]
        public List<Platform> Platforms { get; set; }

        #endregion

        #endregion

        public SlopeData(double station) : this()
        {
            Station = station;
        }
        public SlopeData()
        {
            Slopes = new List<Slope>();
            Platforms = new List<Platform>();
        }

        #region ---   数据 与 ResultBuffer 的转换

        public const string DictKey_General = "General";
        public const string DictKey_Slopes = "Slopes";
        public const string DictKey_Platforms = "Platforms";

        public static SlopeData FromEntity(Entity ent)
        {
            var data = new SlopeData() { FullyCalculated = false };
            if (ent.ExtensionDictionary.IsNull)
            {
                return data;
            }
            // 总的字典
            var extensionDict = ent.ExtensionDictionary.GetObject(OpenMode.ForRead) as DBDictionary;
            int baseId;

            // 一般数据
            var rec = Utils.GetDictionaryValue<Xrecord>(extensionDict, DictKey_General);
            if (rec == null) return data;

            var buffs = rec.Data.AsArray();
            try
            {
                data.FullyCalculated = (Int16)buffs[0].Value == 1;
                data.CenterAxisHandle = Utils.ConvertToHandle(buffs[1].Value.ToString());
                data.FillCut = (Int16)buffs[2].Value == 1;
                data.Station = (double)buffs[3].Value;
                data.OnLeft = (Int16)buffs[4].Value == 1;

                //  标高与长度
                baseId = 4;
                data.TopElevation = (double)buffs[baseId + 1].Value;
                data.BottomElevation = (double)buffs[baseId + 2].Value;
                //data.SlopeLengthBelowWaterLevel = (double)buffs[baseId + 3].Value;
                //data.SlopeLengthBelowFillTop = (double)buffs[baseId + 4].Value;
            }
            catch (Exception ex)
            {
            }
            // 边坡
            var dbSlopes = Utils.GetDictionaryValue<DBDictionary>(extensionDict, DictKey_Slopes);
            if (dbSlopes != null)
            {
                for (int i = 0; i < dbSlopes.Count; i++)
                {
                    var v = Utils.GetDictionaryValue<Xrecord>(dbSlopes, i.ToString());
                    var slp = Slope.FromResultBuffer(v.Data);
                    if (slp != null)
                    {
                        data.Slopes.Add(slp);
                    }
                }
            }
            // 平台
            var dbPlatforms = Utils.GetDictionaryValue<DBDictionary>(extensionDict, DictKey_Platforms);
            if (dbPlatforms != null)
            {
                for (int i = 0; i < dbPlatforms.Count; i++)
                {
                    var v = Utils.GetDictionaryValue<Xrecord>(dbPlatforms, i.ToString());
                    var slp = Platform.FromResultBuffer(v.Data);
                    if (slp != null)
                    {
                        data.Platforms.Add(slp);
                    }
                }
            }
            return data;
        }

        /// <summary> 保存通用数据 </summary>
        public Xrecord ToXrecord_General()
        {
            ResultBuffer generalBuff = new ResultBuffer
                (
                // new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, FullyCalculated),
                new TypedValue((int)DxfCode.ExtendedDataHandle, CenterAxisHandle),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, FillCut),
                new TypedValue((int)DxfCode.ExtendedDataReal, Station),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, OnLeft),
                //  标高与长度
                new TypedValue((int)DxfCode.ExtendedDataReal, TopElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, BottomElevation)
                //new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowWaterLevel),
                //new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowFillTop),
                );
            var rec = new Xrecord();
            rec.Data = generalBuff;
            return rec;
        }

        /// <summary> 保存边坡数据 </summary>
        public void ToDict_Slopes(Transaction trans, DBDictionary container)
        {
            int id = 0;
            var dbSlopes = new DBDictionary();
            container.SetAt(DictKey_Slopes, dbSlopes);
            trans.AddNewlyCreatedDBObject(dbSlopes, true);
            // 如果在将字典对象添加到其容器字典之前，就用 AddNewlyCreatedDBObject ，则会出现报错：eNotInDatabase
            //
            foreach (var sp in Slopes)
            {
                var buff = sp.ToResultBuffer();
                var rec = new Xrecord() { Data = buff };
                dbSlopes.SetAt(id.ToString(), rec);
                trans.AddNewlyCreatedDBObject(rec, true);
                id += 1;
            }
            id = 0;
            var dbPlatforms = new DBDictionary();
            container.SetAt(DictKey_Platforms, dbPlatforms);
            trans.AddNewlyCreatedDBObject(dbPlatforms, true);
            foreach (var sp in Platforms)
            {
                var buff = sp.ToResultBuffer();
                var rec = new Xrecord() { Data = buff };
                dbPlatforms.SetAt(id.ToString(), rec);
                trans.AddNewlyCreatedDBObject(rec, true);
                id += 1;
            }
        }

        public void ClearValue(Entity ent)
        {
            if (ent.ExtensionDictionary.IsNull)
            {
                return;
            }
            // 总的字典
            var extensionDict = ent.ExtensionDictionary.GetObject(OpenMode.ForRead) as DBDictionary;
            if (extensionDict.Contains(DictKey_General))
            {
                extensionDict.Remove(DictKey_General);
            }
            if (extensionDict.Contains(DictKey_Slopes))
            {
                extensionDict.Remove(DictKey_Slopes);
            }
            if (extensionDict.Contains(DictKey_Platforms))
            {
                extensionDict.Remove(DictKey_Platforms);
            }
        }

        #endregion

        /// <summary> 将边坡与平台放到同一个集合中并进行排序。 </summary>
        /// <param name="slopes"><seealso cref="Slope.Index"/>的含义，请参考其属性注释</param>
        /// <param name="platforms"><seealso cref="Platform.Index"/>的含义，请参考其属性注释</param>
        /// <param name="sort">对集合中的元素进行排序，使位于集合中靠前位置的对象，表示此对象更接近路面</param>
        /// <remarks>边坡所对应的几何线段，每一个线段都代表了某一级边坡（包括马道平台）。
        /// 同时，位于集合中靠前位置的对象，表示此对象更接近路面</remarks>
        public static List<ISlopeSeg> Combine(IList<Slope> slopes, IList<Platform> platforms, bool sort)
        {
            var ss = new List<ISlopeSeg>();
            ss.AddRange(slopes);
            ss.AddRange(platforms);
            if (sort)
            {
                ss.Sort(SlopeSegComparison);
            }
            return ss;
        }

        private static int SlopeSegComparison(ISlopeSeg s1, ISlopeSeg s2)
        {
            if (s1.Index < s2.Index)
            {
                return -1;
            }
            else if (s1.Index > s2.Index)
            {
                return 1;
            }
            else // 两者的编号相同
            {
                // 平台在前
                if (s1 is Slope)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        #region --- 信息输出

        public const string InfoHeader = "桩号\t标识\t边坡高度\t位置\t填挖" +
                                         "\t坡顶标高\t坡底标高";

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
            var left = OnLeft ? "左" : "右";
            var fill = FillCut ? "填" : "挖";

            return new object[]
            {
                Station, "测量", TopElevation - BottomElevation, left, fill,
                TopElevation, BottomElevation,
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

        public override string ToString()
        {
            var left = OnLeft ? "左" : "右";
            var fill = FillCut ? "填" : "挖";
            return $"{Station},{left},{fill}";
        }

        #endregion
    }
}