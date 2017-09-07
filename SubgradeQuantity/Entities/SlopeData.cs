using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.SlopeProtection;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 每一侧边坡的所有数据 </summary>
    public class SlopeData
    {
        public const string AppName = "eZSubgradeQuantity";
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

        /// <summary> true 表示填方边坡，false 表示挖方边坡 </summary>
        [Category(ctg_Others), ReadOnly(true), Description("true 表示填方边坡，false 表示挖方边坡")]
        public bool FillCut { get; set; }

        /// <summary> 岩质边坡还是土质边坡 </summary>
        [Category(ctg_Others), ReadOnly(false), Description("岩质边坡还是土质边坡")]
        public SubgradeType SoilOrRock { get; set; }

        // -------------------------------------- 标高与长度

        /// <summary> 边坡线顶部标高，即使此边坡与挡墙重叠 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡线顶部标高，即使此边坡与挡墙重叠")]
        public double TopElevation { get; set; }

        /// <summary> 边坡线底部标高，即使此边坡与挡墙重叠 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("边坡线底部标高，即使此边坡与挡墙重叠")]
        public double BottomElevation { get; set; }

        /// <summary> 挡墙与自然地面交点相对于边坡线内侧点的高差。对于填方边坡，其值一般为负，且只考虑路肩墙，不考虑路堤墙或护脚墙；
        /// 对于挖方边坡，其值一般为正。如果其值为0，一般即表示此边坡中没有挡墙，或者挡墙顶（底）与边坡内边缘点在同一标高。 </summary>
        [Category(ctg_Geometry), ReadOnly(true), Description("挡墙与自然地面交点相对于路面中心点的高差。对于填方边坡，其值一般为负，且只考虑路肩墙，不考虑路堤墙或护脚墙；对于挖方边坡，其值一般为正。如果其值为0，一般即表示此边坡中没有挡墙，或者挡墙顶（底）与边坡内边缘点在同一标高。")]
        public double RetainingWallHeight { get; set; }

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

        /// <summary> 剪切边坡的水位线 </summary>
        [Category(ctg_Others), Description("剪切边坡的水位线")]
        public List<Handle> Waterlines { get; set; }

        #endregion

        #endregion

        public SlopeData(double station,bool onLeft) : this()
        {
            Station = station;
            OnLeft = onLeft;
        }
        public SlopeData()
        {
            Slopes = new List<Slope>();
            Platforms = new List<Platform>();
            Waterlines = new List<Handle>();
            //
            FillCut = true;
        }

        #region ---   数据 与 ResultBuffer 的转换

        public const string DictKey_General = "General";
        public const string DictKey_Slopes = "Slopes";
        public const string DictKey_Platforms = "Platforms";
        public const string DictKey_Waterlines = "Waterlines";

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
                data.FullyCalculated = Utils.GetExtendedDataBool(buffs[0]);
                data.CenterAxisHandle = Utils.ConvertToHandle(buffs[1].Value.ToString());
                data.FillCut = Utils.GetExtendedDataBool(buffs[2]);
                data.Station = (double)buffs[3].Value;
                data.OnLeft = Utils.GetExtendedDataBool(buffs[4]);
                data.SoilOrRock = (SubgradeType)Enum.ToObject(typeof(SubgradeType), (short)buffs[5].Value);

                //  标高与长度
                baseId = 5;
                data.TopElevation = (double)buffs[baseId + 1].Value;
                data.BottomElevation = (double)buffs[baseId + 2].Value;
                data.RetainingWallHeight = (double)buffs[baseId + 3].Value;
                //data.SlopeLengthBelowWaterLevel = (double)buffs[baseId + 3].Value;
                //data.SlopeLengthBelowFillTop = (double)buffs[baseId + 4].Value;
            }
            catch (Exception ex)
            {
                Debug.Print("数据转换出错：" + ex.Message + "\r\n" + ex.StackTrace);
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
            // 水位线

            // 一般数据
            var wl = Utils.GetDictionaryValue<Xrecord>(extensionDict, DictKey_Waterlines);
            if (wl == null || wl.Data == null) return data;
            buffs = wl.Data.AsArray();
            var waterlines = buffs.Select(r => Utils.ConvertToHandle(r.Value.ToString())).ToList();
            data.Waterlines = waterlines;
            //
            return data;
        }

        /// <summary> 保存通用数据 </summary>
        public ResultBuffer ToResBuff_General()
        {
            var generalBuff = new ResultBuffer
                (
                // new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                Utils.SetExtendedDataBool(FullyCalculated),
                new TypedValue((int)DxfCode.ExtendedDataHandle, CenterAxisHandle),
                Utils.SetExtendedDataBool(FillCut),
                new TypedValue((int)DxfCode.ExtendedDataReal, Station),
                Utils.SetExtendedDataBool(OnLeft),
                new TypedValue((int)DxfCode.ExtendedDataInteger16, (short)SoilOrRock),
                //  标高与长度
                new TypedValue((int)DxfCode.ExtendedDataReal, TopElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, BottomElevation),
                new TypedValue((int)DxfCode.ExtendedDataReal, RetainingWallHeight)
                //new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowWaterLevel),
                //new TypedValue((int)DxfCode.ExtendedDataReal, SlopeLengthBelowFillTop),
                );
            return generalBuff;
        }

        /// <summary> 保存边坡数据 </summary>
        public void ToDict_Slopes(Transaction trans, DBDictionary container)
        {
            int id = 0;
            var dbSlopes = new DBDictionary();
            Utils.OverlayDictValue(trans,container,DictKey_Slopes,dbSlopes);
            //
            foreach (var sp in Slopes)
            {
                var buff = sp.ToResultBuffer();
                var rec = new Xrecord() { Data = buff };
                // 由于 dbSlopes 是新创建的，其中还没有任何键，所以这里不用作同名键的检查，可以直接通过SetAt进行数据的添加。
                dbSlopes.SetAt(id.ToString(), rec);
                trans.AddNewlyCreatedDBObject(rec, true);
                id += 1;
            }
            //
            id = 0;
            var dbPlatforms = new DBDictionary();
            Utils.OverlayDictValue(trans, container, DictKey_Platforms, dbPlatforms);
            foreach (var sp in Platforms)
            {
                var buff = sp.ToResultBuffer();
                var rec = new Xrecord() { Data = buff };
                // 由于 dbPlatforms 是新创建的，其中还没有任何键，所以这里不用作同名键的检查，可以直接通过SetAt进行数据的添加。
                dbPlatforms.SetAt(id.ToString(), rec);
                trans.AddNewlyCreatedDBObject(rec, true);
                id += 1;
            }
        }

        /// <summary> 保存水位线数据 </summary>
        public ResultBuffer ToResBuff_Waterlines()
        {
            var typedValues = Waterlines.Select(r => new TypedValue((int)DxfCode.ExtendedDataHandle, r)).ToArray();
            var waterlineBuff = new ResultBuffer(typedValues);
            //var rec = new Xrecord();
            //rec.Data = generalBuff;
            return waterlineBuff;
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
            if (extensionDict.Contains(DictKey_Waterlines))
            {
                extensionDict.Remove(DictKey_Waterlines);
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
            var mainLevel1 = (int)s1.Index;
            var mainLevel2 = (int)s2.Index;
            if (mainLevel1 < mainLevel2)
            {
                return -1;
            }
            else if (mainLevel1 > mainLevel2)
            {
                return 1;
            }
            else // 两者的主编号相同
            {
                if (s1 is Platform)
                {
                    return 1;
                }
                else
                {
                    // 说明 s1 为边坡
                    if (s2 is Platform)
                    {
                        //  s1 为边坡，s2 为平台
                        return -1;
                    }

                    // s1 、 s2 均为边坡，且位于同一级子边坡中
                    if (s1.Index < s2.Index)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
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