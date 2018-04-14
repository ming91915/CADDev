using System;
using System.Collections.Generic;
using eZcad.SubgradeQuantity.Utility;

namespace eZcad.SubgradeQuantity.DataExport
{
    public partial class Exporter_SlopeProtection
    {
        /// <summary> 边坡防护形式的信息，包含有指定防护方式的子边坡集合。
        /// 即最后导出到Excel 表中的每一行数据 </summary>
        private class DataRow
        {
            public double StartStation { get; private set; }
            public double EndStation { get; private set; }
            public ProtectionRange Range { get; }

            /// <summary> 此断面边坡的左右两边所占据的总面积 </summary>
            public double Area { get; private set; }

            /// <summary> 一般来说，一种防护类型中，这一名称属性值都是一样的，
            /// 但是，对于特殊的情况，比如锚杆网格梁可以细分为不同的规格，如6m锚杆、8m锚杆，而这些锚杆都是计在锚杆网格梁的表格中的。 </summary>
            public string ProtectionName { get; set; }

            private double[] MatchedSlopes { get; }
            private double[] MatchedPlatforms { get; }

            /// <summary> 构造函数 </summary>
            public DataRow(double startStation, double endStation, double area, string protectionName,
                ProtectionRange range, double[] matchedSlopes, double[] matchedPlatforms)
            {
                StartStation = startStation;
                EndStation = endStation;
                if (startStation > endStation)
                {
                    throw new ArgumentException($"后方桩号 {startStation.ToString("0.###")} 的值必须小于前方桩号 {EndStation.ToString("0.###")}");
                }
                Area = area;
                ProtectionName = protectionName;
                Range = range;
                //
                MatchedSlopes = matchedSlopes;
                MatchedPlatforms = matchedPlatforms;
            }

            /// <summary> 将本断面边坡（桩号较小）与后面的某断面边坡（桩号较大）进行合并 </summary>
            /// <param name="nextProtMtdInfo">参数边坡的桩号比本对象边坡的桩号大</param>
            /// <returns>如果两者可以合并，则返回 true，并对本对象进行扩展；如果不能合并，则返回false</returns>
            public bool Merge(DataRow nextProtMtdInfo)
            {
                // 桩号的包含
                if ((nextProtMtdInfo.StartStation > this.EndStation) || (nextProtMtdInfo.EndStation < this.StartStation))
                    return false;

                // 防护名称是否相同
                if (String.Compare(ProtectionName, nextProtMtdInfo.ProtectionName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                // 说明这两个断面是相连的
                if (nextProtMtdInfo.Range == Range)
                {
                    // 说明这两个断面的子边坡防护区域是相同的
                    //switch (Range)
                    //{
                    //    case ProtectionRange.PartialSlopess:
                    //        {
                    //            // 此时要判断两个边坡的子边坡级别是否相同
                    //            if ((MatchedSlopes.Length == nextProtMtdInfo.MatchedSlopes.Length)
                    //                && (MatchedPlatforms.Length == nextProtMtdInfo.MatchedPlatforms.Length))
                    //            {
                    //                bool allMatched = true;
                    //                for (int i = 0; i < MatchedSlopes.Length; i++)
                    //                {
                    //                    if (MatchedSlopes[i] != nextProtMtdInfo.MatchedSlopes[i])
                    //                    {
                    //                        allMatched = false;
                    //                        break;
                    //                    }
                    //                }
                    //                for (int i = 0; i < MatchedPlatforms.Length; i++)
                    //                {
                    //                    if (MatchedPlatforms[i] != nextProtMtdInfo.MatchedPlatforms[i])
                    //                    {
                    //                        allMatched = false;
                    //                        break;
                    //                    }
                    //                }
                    //                if (allMatched)
                    //                {
                    //                    // !!!! 终于完全匹配上啦
                    //                    StartStation = Math.Min(this.StartStation, nextProtMtdInfo.StartStation);
                    //                    EndStation = Math.Max(this.EndStation, nextProtMtdInfo.EndStation);
                    //                    Area += nextProtMtdInfo.Area;
                    //                    return true;
                    //                }
                    //            }
                    //            break;
                    //        }
                    //    default:
                    //        {
                    //            StartStation = Math.Min(this.StartStation, nextProtMtdInfo.StartStation);
                    //            EndStation = Math.Max(this.EndStation, nextProtMtdInfo.EndStation);
                    //            Area += nextProtMtdInfo.Area;
                    //            return true;
                    //        }
                }
                StartStation = Math.Min(this.StartStation, nextProtMtdInfo.StartStation);
                EndStation = Math.Max(this.EndStation, nextProtMtdInfo.EndStation);
                Area += nextProtMtdInfo.Area;
                return true;
            }

            #region --- 信息输出

            /// <summary> 备注项，比如第n级边坡 </summary>
            public string GetDescription()
            {
                string desc = null;
                switch (Range)
                {
                    case ProtectionRange.AllSection:
                        desc = "全断面";
                        break;
                    case ProtectionRange.AllSlopes:
                        desc = "所有边坡";
                        break;
                    case ProtectionRange.AllPlatforms:
                        desc = "所有平台";
                        break;
                    case ProtectionRange.PartialSlopes:
                        desc = "部分边坡";
                        break;
                    case ProtectionRange.PartialPlatforms:
                        desc = "部分平台";
                        break;
                    case ProtectionRange.AllSlopes | ProtectionRange.PartialPlatforms:
                        desc = "所有边坡+部分平台";

                        //foreach (var id in MatchedSlopes)
                        //{
                        //    desc += "边坡" + id + ", ";
                        //}
                        //foreach (var id in MatchedPlatforms)
                        //{
                        //    desc += "平台" + id + ", ";
                        //}
                        break;
                    case ProtectionRange.PartialSlopes | ProtectionRange.AllPlatforms:
                        desc = "部分边坡+所有平台";
                        break;
                    case ProtectionRange.PartialSlopes | ProtectionRange.PartialPlatforms:
                        desc = "部分平台+部分边坡";
                        break;
                }
                return desc;
            }

            public static string[] GetTableHeader()
            {
                return new string[] { "起始桩号", "结尾桩号", "桩号区间", "长度", "面积", "防护方式", "备注" };
            }

            private object[] GetRow()
            {
                return new object[]
                {StartStation, EndStation,SQUtils.GetStationString(StartStation, EndStation, maxDigits: 0),
                    EndStation - StartStation, Area, ProtectionName, GetDescription()};
            }

            public static object[,] ConvertToArray(List<DataRow> protMthinfos)
            {
                if (protMthinfos.Count > 0)
                {
                    var col = DataRow.GetTableHeader().Length;
                    var res = new object[protMthinfos.Count, col];
                    for (int i = 0; i < protMthinfos.Count; i++)
                    {
                        var r = protMthinfos[i].GetRow();
                        for (int j = 0; j < col; j++)
                        {
                            res[i, j] = r[j];
                        }
                    }
                    return res;
                }
                else
                {
                    return new object[0, 0];
                }
            }

            public override string ToString()
            {
                return $"{StartStation}~{EndStation},{ProtectionName}, Area:{Area.ToString("0.##")}";
            }

            #endregion
        }
    }
}