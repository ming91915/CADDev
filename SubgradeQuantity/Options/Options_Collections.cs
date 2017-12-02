using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.Entities;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Options
{
    public static class Options_Collections
    {

        #region ---   AllSortedStations 整条道路中所有的横断面（桩号从小到大排列）


        /// <summary>  整条道路中所有的横断面（桩号从小到大排列） </summary>
        public static double[] AllSortedStations = new double[0];

        /// <summary> 将静态类中的数据保存到<seealso cref="Xrecord"/>对象中 </summary>
        /// <returns></returns>
        public static ResultBuffer ToResultBuffer_SortedStations()
        {
            var generalBuff = new ResultBuffer();
            var count = AllSortedStations.Length;
            generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, count));
            foreach (var s in AllSortedStations)
            {
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s));
            }
            //
            //var rec = new Xrecord();
            //rec.Data = generalBuff;
            return generalBuff;
        }

        /// <summary> 将<seealso cref="Xrecord"/>对象中的数据刷新到内存中的静态类中 </summary>
        /// <param name="xrec">其值可以为 null，表示集合中一个数据都没有 </param>
        public static void FromXrecord_SortedStations(Xrecord xrec)
        {
            AllSortedStations = new double[0];
            if (xrec != null)
            {
                //
                var buffs = xrec.Data.AsArray();
                if (buffs == null || buffs.Length == 0)
                {
                    return;
                }
                //
                try
                {
                    var itemsCount = (int)buffs[0].Value;
                    AllSortedStations = new double[itemsCount];
                    const int baseIndex = 1;
                    for (int i = 0; i < itemsCount; i++)
                    {
                        var s = (double)buffs[i + baseIndex].Value;
                        //
                        AllSortedStations[i] = s;
                        // AllSortedStations.Add(s);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("提取整条道路中所有的横断面（桩号从小到大排列）信息出错" + ex.AppendMessage());
                    //MessageBox.Show($"刷新选项数据“{fields[index].Name}”出错。\r\n{ex.StackTrace}");
                }
            }

        }

        #endregion

        #region ---   SoilRockRanges

        /// <summary> 记录道路中岩质边坡与土质边坡的分类与所属区间（不在此区间内的边坡都认为是岩质边坡） </summary>
        public static List<SoilRockRange> SoilRockRanges = new List<SoilRockRange>();

        /// <summary> 将静态类中的数据保存到<seealso cref="Xrecord"/>对象中 </summary>
        /// <returns></returns>
        public static ResultBuffer ToResultBuffer_SoilRockRanges()
        {
            var generalBuff = new ResultBuffer();
            var count = SoilRockRanges.Count;
            generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, count));
            foreach (var s in SoilRockRanges)
            {
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.StartStation));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.EndStation));
                generalBuff.Add(Utils.SetExtendedDataEnum(s.SideDistribution));
                generalBuff.Add(Utils.SetExtendedDataEnum(s.Type));
            }
            //
            //var rec = new Xrecord();
            //rec.Data = generalBuff;
            return generalBuff;
        }

        /// <summary> 将<seealso cref="Xrecord"/>对象中的数据刷新到内存中的静态类中 </summary>
        /// <param name="xrec">其值可以为 null，表示集合中一个数据都没有 </param>
        public static void FromXrecord_SoilRockRanges(Xrecord xrec)
        {
            SoilRockRanges = new List<SoilRockRange>();
            if (xrec != null)
            {
                var buffs = xrec.Data.AsArray();
                if (buffs == null || buffs.Length == 0)
                {
                    return;
                }
                //
                try
                {
                    var itemsCount = (int)buffs[0].Value;
                    var fieldsCount = 4;
                    var baseIndex = 0;
                    for (int i = 0; i < itemsCount; i++)
                    {
                        var s = new SoilRockRange(
                            startStation: (double)buffs[baseIndex + 1].Value,
                            endStation: (double)buffs[baseIndex + 2].Value,
                            distribution: Utils.GetExtendedDataEnum<SoilRockRange.Distribution>(buffs[baseIndex + 3]),
                            type: Utils.GetExtendedDataEnum<SubgradeType>(buffs[baseIndex + 4]));

                        //
                        SoilRockRanges.Add(s);
                        baseIndex += fieldsCount;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("提取整条道路中岩土分区信息出错" + ex.AppendMessage());
                }
            }
            //
        }

        #endregion

        #region ---   RangeBlocks

        /// <summary> 整个道路有所有的桥梁隧道等结构物 </summary>
        public static List<RangeBlock> RangeBlocks = new List<RangeBlock>();

        /// <summary> 将静态类中的数据保存到<seealso cref="Xrecord"/>对象中 </summary>
        /// <returns></returns>
        public static ResultBuffer ToResultBuffer_Blocks()
        {
            var generalBuff = new ResultBuffer();
            var count = RangeBlocks.Count;
            generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, count));
            foreach (var s in RangeBlocks)
            {
                generalBuff.Add(Utils.SetExtendedDataEnum(s.Type));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.StartStation));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.EndStation));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.ConnectedBackStaion));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataReal, s.ConnectedFrontStaion));
                generalBuff.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, s.Description));
            }
            //
            //var rec = new Xrecord();
            //rec.Data = generalBuff;
            return generalBuff;
        }

        /// <summary> 将<seealso cref="Xrecord"/>对象中的数据刷新到内存中的静态类中 </summary>
        /// <param name="xrec">其值可以为 null，表示集合中一个数据都没有 </param>
        public static void FromXrecord_Blocks(Xrecord xrec)
        {
            RangeBlocks = new List<RangeBlock>();
            if (xrec != null)
            {
                var buffs = xrec.Data.AsArray();
                if (buffs == null || buffs.Length == 0)
                {
                    return;
                }
                //
                try
                {
                    var itemsCount = (int)buffs[0].Value;
                    var fieldsCount = 6;
                    var baseIndex = 1;
                    for (int i = 0; i < itemsCount; i++)
                    {
                        var s = new RangeBlock(
                            type: Utils.GetExtendedDataEnum<BlockType>(buffs[baseIndex]),
                            startStation: (double)buffs[baseIndex + 1].Value,
                            endStation: (double)buffs[baseIndex + 2].Value)
                        {
                            ConnectedBackStaion = (double)buffs[baseIndex + 3].Value,
                            ConnectedFrontStaion = (double)buffs[baseIndex + 4].Value,
                            Description = (string)buffs[baseIndex + 5].Value,
                    };
                        //
                        RangeBlocks.Add(s);
                        baseIndex += fieldsCount;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"刷新选项数据“{fields[index].Name}”出错。\r\n{ex.StackTrace}");
                }
            }
            //
        }

        #endregion

        #region ---   边坡防护

        /// <summary> 常用的填方边坡防护形式 </summary>
        public static readonly string[] CommonFillProtections = {
            "填方植草", "三维网植草", "格梁植草", "拱形骨架植草", "人字骨架植草",
            "钢筋石笼", "雷诺护垫", "干砌块石", "混凝土预制块" };

        /// <summary> 常用的挖方边坡防护形式 </summary>
        public static readonly string[] CommonCutProtections = {
            "挖方植草", "三维网植草", "拱形骨架植草", "人字骨架植草", "喷混植生",
            "锚杆格梁", "锚杆网格梁", "锚索框架梁",  "挂网喷锚",
            "锚索护面墙",
            "主动防护网", "被动防护网" };

        #endregion
    }
}