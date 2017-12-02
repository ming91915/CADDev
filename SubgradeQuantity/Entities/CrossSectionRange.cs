using System;
using System.Collections.Generic;
using System.Linq;
using eZcad.SubgradeQuantity.Utility;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 某一个横断面桩号所对应的某种工程量数据 </summary>
    public class CrossSectionRange<T> where T : HalfValue
    {
        #region --- Fields

        /// <summary> 实际在计量时应该不考虑此断面，比如此断面位于桥梁或者隧道内部 </summary>
        public bool IsNull { get;  set; }
        
        /// <summary> true 表示此区间是某一个单独断面的区间； false 表示此区间是多个断面合并后的区间，此时对应的<seealso cref="StationInbetween"/>属性不再具有其实际的意义 </summary>
        public bool Merged { get; private set; }

        /// <summary> 区间中间的桩号，此桩号表示前后区间 <seealso cref="BackValue"/> 与 <seealso cref="FrontValue"/> 的分割桩号，但它不一定是前后桩号的中点值。 </summary>
        public double StationInbetween { get; private set; }
        /// <summary> 中间桩号与前桩号（较小桩号）之间的半个区间对应的工程值 </summary>
        public T BackValue { get; set; }
        /// <summary> 中间桩号与后桩号（较大桩号）之间的半个区间对应的工程值 </summary>
        public T FrontValue { get; set; }

        #endregion

        public CrossSectionRange(double station, T backValue, T frontValue)
        {
            StationInbetween = station;
            BackValue = backValue;
            FrontValue = frontValue;
            //
            Merged = false;
            IsNull = false;
        }

        #region --- 多区间的合并、剪切等操作

        /// <summary> 将本桩号区间与其紧紧相连的下一个区间进行合并 </summary>
        /// <param name="frontRange">比此区间桩号更大的区间</param>
        /// <returns>true表示可以合并，false表示合并不了</returns>
        public virtual bool TryMerge(CrossSectionRange<T> frontRange)
        {
            // 桩号的包含
            if ((frontRange.BackValue.EdgeStation - this.FrontValue.EdgeStation > ProtectionConstants.RangeMergeTolerance)
                || (this.BackValue.EdgeStation - frontRange.FrontValue.EdgeStation > ProtectionConstants.RangeMergeTolerance))
            { return false; }

            if (FrontValue.IsMergeable(frontRange.BackValue))
            {
                // 说明这两段桩号区间在几何上是相接的
                FrontValue.Merge(frontRange.BackValue);
                FrontValue.Merge(frontRange.FrontValue);
                //
                FrontValue.EdgeStation = frontRange.FrontValue.EdgeStation;
                Merged = true;
                return true;
            }
            return false;
        }

        /// <summary> 用桥梁或隧道的结构物来剪切对应的区间值 </summary>
        /// <param name="blockStation">桥梁或者隧道等结构物的起始（末端）桩号</param>
        public virtual void Cut(double blockStation)
        {
            if (FrontValue.EdgeStation < blockStation)
            {
                FrontValue.CutByBlock(blockStation);
                FrontValue.EdgeStation = blockStation;
            }
            else if (BackValue.EdgeStation > blockStation)
            {
                BackValue.CutByBlock(blockStation);
                BackValue.EdgeStation = blockStation;
            }
            else
            {
                throw new InvalidOperationException("用来剪切的桥梁或隧道等结构物桩号位于区间之外");
            }
        }

        /// <summary> 返回此区间的前半段与后半段合并后的数据。此方法改变本实例的<seealso cref="BackValue"/>属性，
        /// 此后，<seealso cref="FrontValue"/> 中除了其 <seealso cref="HalfValue.EdgeStation"/> 属性外，其他的属性均无效。 </summary>
        /// <returns></returns>
        public HalfValue UnionBackFront()
        {
            BackValue.Merge(FrontValue);
            return BackValue;
        }
        #endregion

        /// <summary>
        /// 将 边坡横断面集合转换为二维数组，以用来写入 Excel
        /// </summary>
        /// <param name="slopes"></param>
        /// <returns></returns>
        public static object[,] ConvertToArr(IList<CrossSectionRange<T>> slopes)
        {
            var res = new object[slopes.Count(), 3];

            return res;
        }


        #region ---   一般方法

        public override string ToString()
        {
            return $"{BackValue.EdgeStation.ToString("0.###")}~{FrontValue.EdgeStation.ToString("0.###")}";
        }

        /// <summary> 用新的数据替换对象中的原数据 </summary>
        public void Override(CrossSectionRange<T> newSection)
        {
            StationInbetween = newSection.StationInbetween;
            BackValue = newSection.BackValue;
            FrontValue = newSection.FrontValue;
        }
        #endregion

    }

    /// <summary> 某一区间的前/后半段的工程量值 </summary>
    public abstract class HalfValue : IMergeable
    {
        /// <summary> 半区间的方向，true 表示向大桩号方向，即<seealso cref="EdgeStation"/> 大于 <seealso cref="ParentStation"/>；
        ///  false 表示向小桩号方向，即<seealso cref="EdgeStation"/> 小于 <seealso cref="ParentStation"/>； </summary>
        public bool Direction { get; set; }

        /// <summary> 此半区间所属的横断面桩号。此参数只能通过 <seealso cref="SetParentStation"/> 进行赋值，而且一旦被赋值，便不会再被修改。 </summary>
        public double ParentStation { get; private set; }
        /// <summary> 此半区间所对应的非横断面的另一个边界桩号 </summary>
        public double EdgeStation { get; set; }

        private bool _parentStationNotSet = true;
        /// <summary> 设置此区间所属的横断面桩号 </summary>
        public void SetParentStation(double parentStation)
        {
            if (_parentStationNotSet)
            {
                ParentStation = parentStation;
                _parentStationNotSet = false;
            }
        }
        
        /// <summary> 将相连的两个半区间的值进行合并 </summary>
        /// <param name="connectedHalf">与本半区间相连的另一个半区间。可以是“桩号1.前 连接 桩号2.后”，也可以是“桩号2.后 连 桩号2.前”</param>
        /// <returns></returns>
        public abstract void Merge(IMergeable connectedHalf);

        /// <summary> 两个相邻区间是否可以合并到同一行 </summary>
        /// <param name="next">与本区间紧密相连的下一个区间</param>
        public abstract bool IsMergeable(IMergeable next);

        /// <summary> 用桥梁或隧道的结构物来剪切对应的区间值 </summary>
        /// <param name="blockStation"></param>
        public abstract void CutByBlock(double blockStation);

    }
}
