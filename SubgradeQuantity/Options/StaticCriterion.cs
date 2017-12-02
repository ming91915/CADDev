using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace eZcad.SubgradeQuantity.Options
{
    /// <summary> 判断标准——低填浅挖 </summary>
    [XmlType(typeName: "低填浅挖")]
    public class Criterion_ThinFillShallowCut : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "低填浅挖";

        #region ---   判断标准——低填浅挖

        /// <summary> 低填浅挖中，判断低填路堤中的中心填方高度（路面与自然地面）的最大值，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断低填路堤中的中心填方高度（路面与自然地面）的最大值，单位为米")]
        public double 低填最大高度 { get; set; }

        /// <summary> 低填浅挖中，判断低填路堤时，从中心线与自然地面交点向上（或向下）进行倾角放射，射线角度为1:n </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断低填路堤时，从中心线与自然地面交点向上（或向下）进行倾角放射，射线角度为1:n")]
        public double 低填射线坡比 { get; set; }

        /// <summary> 低填浅挖中，路面顶部中点以下要保证1.5m处理深度，当路面中点与自然地面的高度小于1.5m时， 需要在自然地面以下进行超挖回填处理。 </summary>
        [Browsable(true), Category(ctg_Calculate),
         Description("低填浅挖中，路面顶部中点以下要保证1.5m处理深度，当路面中点与自然地面的高度小于1.5m时， 需要在自然地面以下进行超挖回填处理。")]
        public double 低填处理高度 { get; set; }


        /// <summary> 判断浅挖路堑中的中心挖方高度（路面与自然地面）的最大值，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("判断浅挖路堑中的中心挖方高度（路面与自然地面）的最大值，单位为米 ")]
        public double 浅挖最大深度 { get; set; }

        /// <summary> 低填浅挖中，判断浅挖路堑时，从中心线与自然地面交点向上（或向下）进行倾角放射，射线角度为1:n </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断浅挖路堑时，从中心线与自然地面交点向上（或向下）进行倾角放射，射线角度为1:n")]
        public double 浅挖射线坡比 { get; set; }

        /// <summary> 浅挖路堑中，路槽底部中点以下 0.8m 处理深度，当路槽底与自然地面的高度小于0.8m 时， 需要在路槽底以下进行翻挖压实处理。 </summary>
        [Browsable(true), Category(ctg_Calculate),
         Description("浅挖路堑中，路槽底部中点以下 0.8m 处理深度，当路槽底与自然地面的高度小于0.8m 时， 需要在路槽底以下进行翻挖压实处理。")]
        public double 浅挖处理高度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_ThinFillShallowCut _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_ThinFillShallowCut UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_ThinFillShallowCut();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_ThinFillShallowCut() : base()
        {
            低填最大高度 = 1.5;
            低填射线坡比 = 5;
            低填处理高度 = 1.5;
            //
            浅挖最大深度 = 0.5;
            浅挖射线坡比 = 5;
            浅挖处理高度 = 0.8;

            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断标准——高填深挖 </summary>
    [XmlType(typeName: "高填深挖")]
    public class Criterion_HighFillDeepCut : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "高填深挖";

        #region ---   判断标准——低填浅挖

        /// <summary> 当填方边坡高度大于n米时，即判断为高填路堤，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方边坡高度大于n米时，即判断为高填路堤，单位为米")]
        public double 填方最低高度 { get; set; }

        /// <summary> 当土质挖方边坡高度大于n米时，即判断为深挖路堤，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当土质挖方边坡高度大于n米时，即判断为深挖路堑，单位为米")]
        public double 土质挖方最低高度 { get; set; }

        /// <summary> 当岩质挖方边坡高度大于n米时，即判断为深挖路堤，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当岩质挖方边坡高度大于n米时，即判断为深挖路堑，单位为米")]
        public double 岩质挖方最低高度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_HighFillDeepCut _uniqueInstance;

        /// <summary> 全局唯一的实例对象：低填浅挖的判断标准 </summary>
        public static Criterion_HighFillDeepCut UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_HighFillDeepCut();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_HighFillDeepCut() : base()
        {
            填方最低高度 = 20;
            //
            土质挖方最低高度 = 20;
            岩质挖方最低高度 = 30;

            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断标准——陡坡路堤 </summary>
    [XmlType(typeName: "陡坡路堤")]
    public class Criterion_SteepFill : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "陡坡路堤";

        #region ---   判断标准——低填浅挖

        /// <summary> 在判断陡坡路堤时，每隔多少米进行一次判断，判断其自然地面的坡比是否陡于1:2.5 </summary>
        [Browsable(true), Category(ctg_Judge), Description("在判断陡坡路堤时，每隔多少米进行一次判断，判断其自然地面的坡比是否陡于1:2.5")]
        public double 最小迭代宽度 { get; set; }

        /// <summary> 当填方边坡的坡比陡于1:2.5时，即认为是陡坡路堤断面 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方边坡的坡比陡于1:2.5时，即认为是陡坡路堤断面")]
        public double 陡坡坡比 { get; set; }

        /// <summary> 当填方段高度大于5m的陡坡路堤，在路床位置铺设三层土工格栅。 </summary>
        [Browsable(true), Category(ctg_Judge), Description("填方段高度大于5m的陡坡路堤，在路床位置铺设三层土工格栅。")]
        public double 加筋体对应填方段最小高度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_SteepFill _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_SteepFill UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_SteepFill();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_SteepFill() : base()
        {
            最小迭代宽度 = 4;
            陡坡坡比 = 2.5;
            加筋体对应填方段最小高度 = 5;
            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断标准——挖台阶（除陡坡路堤） </summary>
    [XmlType(typeName: "挖台阶")]
    public class Criterion_StairExcav : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "挖台阶";

        #region ---   判断标准——挖台阶

        /// <summary> 在判断陡坡路堤时，每隔多少米进行一次判断，判断其自然地面的坡比是否陡于1:2.5 </summary>
        [Browsable(true), Category(ctg_Judge), Description("在判断陡坡路堤时，每隔多少米进行一次判断，判断其自然地面的坡比是否陡于1:2.5")]
        public double 最小迭代宽度 { get; set; }

        /// <summary> 当填方边坡的坡比陡于 1:2.5 时，即认为是陡坡路堤断面 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方边坡的坡比陡于 1:2.5 时，即认为是陡坡路堤断面")]
        public double 陡坡坡比 { get; set; }

        /// <summary> 当填方边坡的坡比陡于 1:2.5 时，即认为是陡坡路堤断面 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方边坡的坡比陡于 1:2.5 时，即认为是陡坡路堤断面")]
        public double 填方坡比上限 { get; set; }

        /// <summary> 当填方边坡的坡比缓于 1:5 时，即认为是属性低填浅挖部分，不需要进行挖台阶处理 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方边坡的坡比缓于 1:5 时，即认为是属性低填浅挖部分，不需要进行挖台阶处理")]
        public double 填方坡比下限 { get; set; }


        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_StairExcav _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_StairExcav UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_StairExcav();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_StairExcav() : base()
        {
            最小迭代宽度 = 4;
            陡坡坡比 = 2.5;
            填方坡比上限 = 2.5;
            填方坡比下限 = 5;
            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断与计量标准——纵向填挖交界 </summary>
    [XmlType(typeName: "纵向填挖交界")]
    public class Criterion_FillCutIntersect : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "纵向填挖交界";

        #region ---   判断标准——挖台阶

        /// <summary> 在纵向填挖交界点，取填方区多少米宽度范围进行处理 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("在纵向填挖交界点，取填方区多少米宽度范围进行处理")]
        public double 填方区处理宽度 { get; set; }

        /// <summary> 在纵向填挖交界点，取挖方区多少米宽度范围进行处理 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("在纵向填挖交界点，取挖方区多少米宽度范围进行处理")]
        public double 挖方区处理宽度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_FillCutIntersect _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_FillCutIntersect UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_FillCutIntersect();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_FillCutIntersect() : base()
        {
            填方区处理宽度 = 3;
            挖方区处理宽度 = 10;
            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断与计量标准——纵向挖台阶 </summary>
    [XmlType(typeName: "纵向挖台阶")]
    public class Criterion_StairExcavLong : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "纵向挖台阶";

        #region ---   判断标准——挖台阶

        /// <summary> 挖台阶量的最小分段宽度 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("挖台阶量的最小分段宽度")]
        public double 最小区间宽度 { get; set; }

        /// <summary> 要进行挖台阶处理的最小纵坡。在纵断面中，当原地面纵坡大于12% 时，应按设计要求挖台阶，或设置坡度向内并大于4%、宽度大于2m的台阶。 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("要进行挖台阶处理的最小纵坡。在纵断面中，当原地面纵坡大于12% 时，应按设计要求挖台阶，或设置坡度向内并大于4%、宽度大于2m的台阶。")]
        public double 临界纵坡 { get; set; }

        /// <summary> 每一个台阶的宽度。在纵断面中，当原地面纵坡大于12% 时，应按设计要求挖台阶，或设置坡度向内并大于4%、宽度大于2m的台阶。 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("每一个台阶的宽度。在纵断面中，当原地面纵坡大于12% 时，应按设计要求挖台阶，或设置坡度向内并大于4%、宽度大于2m的台阶。")]
        public double 台阶宽度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_StairExcavLong _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_StairExcavLong UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_StairExcavLong();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_StairExcavLong() : base()
        {
            最小区间宽度 = 10;
            台阶宽度 = 2;
            临界纵坡 = 0.12;
            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }

    /// <summary> 判断与计量标准——路面工程量 </summary>
    [XmlType(typeName: "路面工程量")]
    public class Criterion_RoadSurface : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle => "路面工程量";

        #region ---   判断标准——挖台阶

        /// <summary> 填方有挡墙路段的每延米路肩体积 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("填方有挡墙路段的每延米路肩体积")]
        public double 路肩面积_挡墙 { get; set; }

        /// <summary> 填方无挡墙而且设有护栏路段的每延米路肩体积 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("填方无挡墙而且设有护栏路段的每延米路肩体积")]
        public double 路肩面积_护栏 { get; set; }

        /// <summary> 填方无挡墙而且没有设护栏路段的每延米路肩体积 </summary>
        [Browsable(true), Category(ctg_Calculate), Description("填方无挡墙而且没有设护栏路段的每延米路肩体积")]
        public double 路肩面积_无护栏 { get; set; }

        /// <summary> 当填方无挡墙时，如果填方边坡的高度大于3.0m时，则认为需要在路肩上需要设置护栏 </summary>
        [Browsable(true), Category(ctg_Judge), Description("当填方无挡墙时，如果填方边坡的高度大于3.0m时，则认为需要在路肩上设置护栏")]
        public double 设护栏段的填方高度 { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_RoadSurface _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static Criterion_RoadSurface UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new Criterion_RoadSurface();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private Criterion_RoadSurface() : base()
        {
            路肩面积_挡墙 = 0.1125;
            路肩面积_护栏 = 路肩面积_无护栏 + 0.225;
            路肩面积_无护栏 = 0.2643750; // 0.258417;

            设护栏段的填方高度 = 3.0;
            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }
}