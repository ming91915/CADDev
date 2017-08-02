using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.Utility
{
    public class EntityArray2D<T> where T : Entity
    {
        #region   ---   Fields

        private readonly IEnumerable<T> _entities;

        #endregion

        public EntityArray2D(IEnumerable<T> entities)
        {
            _entities = entities;
        }

        /// <summary>
        /// 将集合中的元素在二维平面中进行分格填空，以形成一个按二维几何排布的数组
        /// 每一个方格中可能有多个实体，比如多个文字挤在一堆。第(0,0)个元素代表位于几何坐标系中的靠左上角的元素们。
        /// </summary>
        /// <returns>考虑到AutoCAD几何坐标系中，Y坐标较小者位于集合前面，而在计算机的二维数组表示方法中，
        /// 下标(0,0)一般表示位于左上角的元素，所以，这里在将几何元素填充到二维数组中时，将Y值的排序进行反转。</returns>
        public List<T>[,] Arrange2D()
        {
            var entBounds = new List<EntBound>();
            foreach (var e in _entities)
            {
                if (e.Bounds.HasValue)
                {
                    entBounds.Add(new EntBound(e, new Rectangle2D(e.Bounds.Value.MinPoint, e.Bounds.Value.MaxPoint)));
                }
            }
            // 求出每一个实体在最终的二维方格中的定位下标，第一个方格的下标为 0
            int rowsCount, colsCount;
            var rowEnts = SortToOneVector(true, entBounds, out colsCount);  // 将所有元素压缩到一行，字典中的值对应二维数组中的列号
            var colEnts = SortToOneVector(false, entBounds, out rowsCount); // 将所有元素压缩到一列，字典中的值对应二维数组中的行号

            // 每一个方格中可能有多个实体，比如多个文字挤在一堆
            var res = new List<T>[rowsCount, colsCount];
            for (int r = 0; r < rowsCount; r++)  // 将二维集合中所有方格都初始化一个集合
            {
                for (int c = 0; c < colsCount; c++)
                {
                    res[r, c] = new List<T>();
                }
            }

            int rowIndex, colIndex;
            foreach (var ent in entBounds)
            {
                // 从压缩到的一整行中提取每一个元素的列号
                colIndex = rowEnts[ent];
                // 从压缩到的一整列中提取每一个元素的行号，同时，考虑到AutoCAD几何坐标系中，Y坐标较小者位于集合前面，而在计算机的二维数组表示方法中，下标(0,0)一般表示位于左上角的元素，所以，这里在将几何元素填充到二维数组中时，将Y值的排序进行反转。
                rowIndex = rowsCount - 1 - colEnts[ent];
                // 为二维方格中添加实体
                res[rowIndex, colIndex].Add(ent.Entity);
            }
            return res;
        }

        #region   ---   排列多个矩形

        /// <summary> 将集合中的元素排列到一行或一列中（紧密算法，即相交实体的认为在同一行或同一列） </summary>
        /// <param name="inRow">true 表示将集合中的元素排列到一行中，false 表示将集合中的实体排列到一列中 </param>
        /// <param name="entities"></param>
        /// <param name="count">排列完成后，共有多少行或者多少列</param>
        /// <returns>返回的字典中，键表示每一个实体对象，值代表此对象在排列好的行中的下标，第一个值的下标为0。多个实体可能对应同一个下标</returns>
        private static Dictionary<EntBound, int> SortToOneVector(bool inRow, List<EntBound> entities, out int count)
        {
            if (entities.Count == 0)
            {
                count = 0;
                return new Dictionary<EntBound, int>();
            }
            else if (entities.Count == 1)
            {
                count = 1;
                return new Dictionary<EntBound, int> { { entities.First(), 0 } };
            }
            else // 集合中至少有两个元素
            {
                var firstEnt = entities.First();
                // 相互分隔的矩形区域，前面的元素位置靠下/左
                var sepratedSortedBounds = new SortedDictionary<double, Rectangle2D> { { 0, firstEnt.Bound } };
                var sortedEntIndex = new Dictionary<EntBound, double> { { firstEnt, 0 } };

                // 从第二个元素开始添加
                for (int i = 1; i < entities.Count; i++)
                {
                    var ent = entities[i];
                    InsertEntityToOneVec(inRow, sepratedSortedBounds, sortedEntIndex, ent);
                }
                // 将表示定位的 double 值转换为 int 值
                var indices = sortedEntIndex.Values.Distinct().ToArray();
                Array.Sort(indices);
                var res = new Dictionary<EntBound, int>();
                foreach (var ent in sortedEntIndex)
                {
                    var ind = Array.IndexOf(indices, ent.Value);
                    res.Add(ent.Key, ind);
                }
                count = indices.Length;
                return res;
            }
        }

        /// <summary> 将新的实体在原集合中按是否在同一列进行插值，即将所有的元素压缩到同一行中 </summary>
        /// <param name="inRow"> true 表示将新的实体在原集合中按是否在同一列进行插值，即将所有的元素压缩到同一行中；
        /// false 表示 将新的实体在原集合中按是否在同一行进行插值，即将所有的元素压缩到同一列中 </param>
        /// <param name="sepratedSortedBounds"></param>
        /// <param name="sortedEntIndex"></param>
        /// <param name="newEnt"></param>
        private static void InsertEntityToOneVec(bool inRow, SortedDictionary<double, Rectangle2D> sepratedSortedBounds,
            Dictionary<EntBound, double> sortedEntIndex, EntBound newEnt)
        {
            IntersectState s;
            double lastIndex = sepratedSortedBounds.First().Key;

            var i = 0;
            foreach (var index in sepratedSortedBounds.Keys) // 从左到右一个一个给出来与新实体进行比较
            {
                var rec = sepratedSortedBounds[index];
                // --------------------------------------------------------
                if (inRow)
                {
                    s = rec.ColumnIntersectWith(newEnt.Bound);
                }
                else
                {
                    s = rec.RowIntersectWith(newEnt.Bound);
                }

                // --------------------------------------------------------
                if ((s & IntersectState.Intersect) > 0) // 在相交的情况下，直接将其放到同一列中
                {
                    //index = sepratedSortedBounds[bd];
                    sortedEntIndex.Add(newEnt, index);
                    return;
                }
                else if (s == IntersectState.OBottom) // 由于比较对象是从下往上给出来的，所以如果新实体位于集合中某矩形的下方，则找到了其定位
                {
                    double newId = 0;
                    if (i == 0)
                    {
                        newId = index - 1;  // 集合中 第一个进行比较的位于最左边，其编号最小，此时将新index取为直接减1。
                    }
                    else
                    {
                        newId = (lastIndex + index) / 2; // 用二分法进行定位的插值
                    }
                    sepratedSortedBounds.Add(newId, newEnt.Bound);
                    sortedEntIndex.Add(newEnt, newId);
                    return;
                }
                else // 不相交，而且新实体在集合元素的上方（右边）
                {
                }
                i += 1;
                lastIndex = index;
            }
            // 如果执行到这里，说明新实体在集合中所有实体的右边（上方）
            var newIndex = lastIndex + 1;
            sepratedSortedBounds.Add(newIndex, newEnt.Bound);
            sortedEntIndex.Add(newEnt, newIndex);
        }

        #endregion

        #region   ---   Types: EntBound

        private class EntBound
        {
            public T Entity { get; set; }
            public Rectangle2D Bound { get; set; }

            public EntBound(T entity, Rectangle2D bound)
            {
                Entity = entity;
                Bound = bound;
            }

            public override string ToString()
            {
                if (Entity is DBText)
                {
                    return (Entity as DBText).TextString;
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        #endregion
    }

    #region   ---   Types: Rectangle2D

    /// <summary>
    /// 二维矩形方框区域
    /// </summary>
    public class Rectangle2D
    {
        #region   ---   Fields

        public readonly double Left;
        public readonly double Top;
        public readonly double Bottom;
        public readonly double Right;
        public readonly double Height;
        public readonly double Width;

        #endregion

        public Rectangle2D(Point2d pt1, Point2d pt2)
        {
            Left = Math.Min(pt1.X, pt2.X);
            Right = Math.Max(pt1.X, pt2.X);
            Top = Math.Max(pt1.Y, pt2.Y);
            Bottom = Math.Min(pt1.Y, pt2.Y);
            Height = Top - Bottom;
            Width = Right - Left;
        }

        public Rectangle2D(Point3d pt1, Point3d pt2)
        {
            Left = Math.Min(pt1.X, pt2.X);
            Right = Math.Max(pt1.X, pt2.X);
            Top = Math.Max(pt1.Y, pt2.Y);
            Bottom = Math.Min(pt1.Y, pt2.Y);
            Height = Top - Bottom;
            Width = Right - Left;
        }

        #region   ---   矩形区域的相交关系

        /// <summary> 比较 Y 值，以确定是否在同一行 </summary>
        /// <param name="recB"></param>
        /// <returns></returns>
        public IntersectState RowIntersectWith(Rectangle2D recB)
        {
            return IntersectWith(Bottom, Top, recB.Bottom, recB.Top);
        }

        /// <summary> 比较 X 值，以确定是否在同一列 </summary>
        public IntersectState ColumnIntersectWith(Rectangle2D recB)
        {
            return IntersectWith(Left, Right, recB.Left, recB.Right);
        }

        private IntersectState IntersectWith(double bottom1, double top1, double bottom2, double top2)
        {
            const double tol = 1e-8;
            if (bottom2 >= top1) return IntersectState.OTop;
            else if (top2 <= bottom1) return IntersectState.OBottom;
            else if (top2 <= bottom1) return IntersectState.OBottom;
            else if ((Math.Abs(top1 - top2) < tol) && (Math.Abs(bottom1 - bottom2) < tol))
                return IntersectState.ICoinside;
            else if (top2 >= top1 && bottom2 >= bottom1) return IntersectState.ITop;
            else if (top2 <= top1 && bottom2 <= bottom1) return IntersectState.IBottom;
            else if (top2 <= top1 && bottom2 >= bottom1) return IntersectState.IInside;
            else if (top2 >= top1 && bottom2 <= bottom1) return IntersectState.IContain;
            else return IntersectState.Others;
        }

        #endregion

    }

    #endregion

    #region   ---   Types: IntersectState

    /// <summary>
    /// 两个矩形之间的相交关系。
    /// 对于左右的比较，左即对应于下，右即对应于上。
    /// </summary>
    [Flags]
    public enum IntersectState : short
    {
        Others = 0,

        /// <summary> B 与 A 相交面积不为0 </summary>
        Intersect = 1,

        /// <summary> B 与 A 相交，而且B的最上边比A的最上边高 </summary>
        ITop = Intersect | 2,

        /// <summary> B 与 A 相交，而且B的最下边比A的最下边低 </summary>
        IBottom = Intersect | 4,

        /// <summary> A 完全将 B 包含 </summary>
        IInside = Intersect | 8,

        /// <summary> B 完全将 A 包含 </summary>
        IContain = Intersect | 16,

        /// <summary> B 与 A 完全重合 </summary>
        ICoinside = Intersect | 32,

        /// <summary> B 与 A 相交面积为 0  </summary>
        Apart = 64,

        /// <summary> B 在 A 的上方，不相交或者只有边界相交 </summary>
        OTop = Apart | 128,

        /// <summary> B 在 A 的下方，不相交或者只有边界相交 </summary>
        OBottom = Apart | 256,
    }

    #endregion

}