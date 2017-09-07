using System;
using System.Text;
using System.Xml.Serialization;
using eZstd.Enumerable;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    public enum Index
    {
        填挖,
        坡比,
        坡高,
        坡级,
        坡长
    }

    public enum Operator_Bool
    {
        任意 = 0,
        是,
        否
    }

    public enum Operator_Num
    {
        任意 = 0,
        等于,
        不等于,
        大于,
        大于等于,
        小于,
        小于等于,
        /// <summary>
        /// 这只是一个定义，在具体的数据中并不会保存这个值，而是保存为<seealso cref="大于等于"/>和<seealso cref="小于等于"/>的交集。
        /// </summary>
        闭区间,
    }

    //public class SlopeRangeList : XmlList<SlopeRange>
    //{
    //    public override string ToString()
    //    {
    //        var sb = new StringBuilder();
    //        foreach (var rg in this)
    //        {
    //            sb.Append(rg + ", ");
    //        }
    //        return sb.ToString();
    //    }
    //}

    public class CriterionRangeList : ICloneable
    {
        /// <summary> 集合中的多个元素之前为 And 的逻辑关系 </summary>
        public XmlList<CriterionRange> AndRange;

        public CriterionRangeList()
        {
            AndRange = new XmlList<CriterionRange>();
        }

        public override string ToString()
        {
            if (AndRange.Count == 0)
            {
                return "任意";
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var rg in AndRange)
                {
                    sb.Append(rg + ", ");
                }
                return sb.ToString();
            }
        }

        public object Clone()
        {
            var cloneObj = new CriterionRangeList();
            cloneObj.AndRange = AndRange.Clone() as XmlList<CriterionRange>;
            return cloneObj;
        }
    }


    /// <summary> 数值型范围 </summary>
    [Serializable]
    public class CriterionRange : ICloneable
    {
        #region --- Fields

        [XmlAttribute]
        public Operator_Num Operator { get; set; }

        [XmlAttribute]
        public double Value { get; set; }

        #endregion

        #region --- 值的比较

        /// <summary> 指定的值是否满足区间规则 </summary>
        /// <param name="comparedValue"></param>
        /// <returns></returns>
        public bool InRange(double comparedValue)
        {
            switch (Operator)
            {
                case Operator_Num.任意: { return true; }
                case Operator_Num.等于: { return comparedValue == Value; }
                case Operator_Num.不等于: { return comparedValue != Value; }
                case Operator_Num.大于: { return comparedValue > Value; }
                case Operator_Num.大于等于: { return comparedValue >= Value; }
                case Operator_Num.小于: { return comparedValue < Value; }
                case Operator_Num.小于等于: { return comparedValue <= Value; }

            }
            return false;
        }
        #endregion

        public override string ToString()
        {
            if (Operator == Operator_Num.任意)
            {
                return "任意";
            }
            string op = null;
            switch (Operator)
            {
                case Operator_Num.等于:
                    op = "=";
                    break;
                case Operator_Num.不等于:
                    op = "<>";
                    break;
                case Operator_Num.大于:
                    op = ">";
                    break;
                case Operator_Num.大于等于:
                    op = ">=";
                    break;
                case Operator_Num.小于:
                    op = "<";
                    break;
                case Operator_Num.小于等于:
                    op = "<=";
                    break;
            }
            return op + Value;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }


    }

    /// <summary> 用来确定边坡防护形式的准则 </summary>
    public class SpCriterion : ICloneable
    {
        #region --- Fields

        /// <summary> 边坡是为填方坡还是挖方坡 </summary>
        [XmlAttribute]
        public Operator_Bool Fill { get; set; }

        /// <summary> 横断面的某一侧边坡中，第一级子边坡的坡比。集合中的多个元素之前为 And 的逻辑关系 </summary>
        public CriterionRangeList FirstSlopeRatio { get; set; }

        /// <summary> 横断面某一侧边坡的总坡高，集合中的多个元素之前为 And 的逻辑关系  </summary>
        public CriterionRangeList SlopeHeight { get; set; }

        /// <summary> 横断面某一侧边坡的总坡级，比如最顶层的边坡为第1级边坡。集合中的多个元素之前为 And 的逻辑关系  </summary>
        public CriterionRangeList SlopeLevel { get; set; }

        /// <summary> 满足对应条件之后，确定的防护方式  </summary>
        public string ProtectionMethod { get; set; }

        #endregion

        #region --- 构造函数

        public SpCriterion()
        {
            FirstSlopeRatio = new CriterionRangeList();
            SlopeHeight = new CriterionRangeList();
            SlopeLevel = new CriterionRangeList();
        }

        #endregion

        public object Clone()
        {
            var cloned = MemberwiseClone() as SpCriterion;
            // 
            cloned.FirstSlopeRatio = CloneRange(FirstSlopeRatio);
            cloned.SlopeHeight = CloneRange(SlopeHeight);
            cloned.SlopeLevel = CloneRange(SlopeLevel);
            //
            return cloned;
        }

        private CriterionRangeList CloneRange(CriterionRangeList srcList)
        {
            var c = new CriterionRangeList();
            var srcRange = srcList.AndRange;
            var r = new XmlList<CriterionRange>();
            foreach (var v in srcRange)
            {
                r.Add(v.Clone() as CriterionRange);
            }
            c.AndRange = r;
            return c;
        }
    }


    ///// <summary> 用来确定边坡防护形式的准则 </summary>
    //public class PlatformCriterion : ICloneable
    //{
    //    #region --- Fields

    //    /// <summary> 边坡是为填方坡还是挖方坡 </summary>
    //    [XmlAttribute]
    //    public Operator_Bool Fill { get; set; }

    //    /// <summary> 边坡的坡级，比如最顶层的边坡为第1级边坡。集合中的多个元素之前为 And 的逻辑关系  </summary>
    //    public CriterionRangeList PlatformLevel { get; set; }

    //    /// <summary> 平台的水平长度，集合中的多个元素之前为 And 的逻辑关系  </summary>
    //    public CriterionRangeList PlatformLength { get; set; }

    //    /// <summary> 满足对应条件之后，确定的防护方式  </summary>
    //    public string ProtectionMethod { get; set; }

    //    #region --- 构造函数

    //    public PlatformCriterion()
    //    {
    //        PlatformLevel = new CriterionRangeList();
    //        PlatformLength = new CriterionRangeList();
    //    }

    //    #endregion

    //    public object Clone()
    //    {
    //        var cloned = MemberwiseClone() as PlatformCriterion;
    //        cloned.PlatformLevel = PlatformLevel.Clone() as CriterionRangeList;
    //        cloned.PlatformLength = PlatformLength.Clone() as CriterionRangeList;

    //        return cloned;
    //    }

    //    #endregion
    //}


    /// <summary> 完整的边坡自动防护信息  </summary>
    [Serializable]
    public class AutoProtectionCriterions
    {
        public const string FileExtension = "自动防护(*.autoP)| *.autoP";

        #region --- Properties

        [XmlAttribute]
        public string Name { get; set; }

        public XmlList<SpCriterion> SlopeCriterions { get; set; }
        // public XmlList<PlatformCriterion> PlatformCriterions { get; set; }

        #endregion

        #region --- 构造函数

        public AutoProtectionCriterions()
        {
            Name = "自动防护";
            //
            SlopeCriterions = new XmlList<SpCriterion>();
            // PlatformCriterions = new XmlList<PlatformCriterion>();
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}