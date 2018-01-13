using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eZcad.Addins
{

    /// <summary>
    /// 用来进行表格多级排序的字段列集合
    /// </summary>
    public class SortColumnCollection
    {
        public List<SortColumn> SortColumns;
        private int _Index;
        private SortColumn _lastField;

        public SortColumnCollection()
        {
            SortColumns = new List<SortColumn>();
            _Index = 0;
        }


        /// <summary> 向全局的排序字段集合中添加一个字段 </summary>
        /// <param name="field"></param>
        /// <returns>所添加的列 或者是 修改的最后一列 </returns>
        public SortColumn AddSortColumn(string field)
        {
            SortColumn sc;
            const bool defaultAsc = true;
            if (SortColumns.Count == 0)
            {
                _Index += 1;
                sc = new SortColumn(_Index, field, defaultAsc);
                SortColumns.Add(sc);
            }
            else
            {
                if (field == _lastField.Field)
                {
                    // 只修改集合中最后一个字段排序的升降
                    sc = _lastField;
                    sc.Ascend = !sc.Ascend;
                }
                else
                {
                    // 看当前点击的字段列是不是已经点击过了的
                    sc = SortColumns.FirstOrDefault(r => r.Field == field);
                    if (sc == null)
                    {
                        // 说明当前点击的是一个新列
                        _Index += 1;
                        sc = new SortColumn(_Index, field, defaultAsc);
                        SortColumns.Add(sc);
                    }
                    else
                    {
                        // 说明当前点击的是一个已经点击过的列
                        _Index += 1;
                        sc.Ascend = !sc.Ascend;
                        sc.Index = _Index;
                    }
                }
            }
            // 重新对 Index 进行编号
            SortColumns.Sort(SortOrderComparerAsc);
            for (int j = 0; j < SortColumns.Count; j++)
            {
                SortColumns[j].Index = j + 1;
            }
            //
            _lastField = sc;
            return sc;
        }


        public void ClearSortColumns()
        {
            _Index = 0;
            SortColumns = new List<SortColumn>();
        }

        public string GetSortCriterior()
        {
            if (SortColumns.Count == 0)
            {
                return null;
            }
            // 排序，将小编号排到前面，说明最先选的级别最高
            SortColumns.Sort(SortOrderComparerAsc);
            var sb = new StringBuilder();
            string asc;
            SortColumn sc = SortColumns[0];
            // 第一个
            asc = sc.Ascend ? "ASC" : "DESC";
            sb.Append($"{sc.Field} {asc}");
            // 剩下的字段
            for (int i = 1; i < SortColumns.Count; i++)
            {
                sc = SortColumns[i];
                asc = sc.Ascend ? "ASC" : "DESC";
                sb.Append($", {sc.Field} {asc}");
            }
            return sb.ToString();
        }

        /// <summary> 编号大的排前面 </summary>
        private int SortOrderComparerDesc(SortColumn s1, SortColumn s2)
        {
            return s2.Index.CompareTo(s1.Index);
        }
        /// <summary> 编号小的排前面 </summary>
        private int SortOrderComparerAsc(SortColumn s1, SortColumn s2)
        {
            return s1.Index.CompareTo(s2.Index);
        }
    }


    /// <summary> 用来进行表格多级排序的字段列 </summary>
    public class SortColumn
    {
        /// <summary> 排序级别，小值对应的级别最高 </summary>
        public int Index { get; set; }

        public string Field { get; set; }

        /// <summary> true 表示升序，false表示降序 </summary>
        public bool Ascend { get; set; }

        /// <summary> 构造函数 </summary>
        public SortColumn(int index, string field, bool ascend)
        {
            Index = index;
            Field = field;
            Ascend = ascend;
        }

        public override string ToString()
        {
            return $"{Field},{Index},{Ascend}";
        }
    }
}