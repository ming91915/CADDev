using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace eZcad.SubgradeQuantity.Options
{
    /// <summary> 用来记录一些静态成员的实例类（只类只有一个全局唯一的实例对象） </summary>
    [Serializable()]
    [XmlInclude(typeof(Criterion_ThinFillShallowCut))]
    [XmlInclude(typeof(Criterion_HighFillDeepCut))]
    [XmlInclude(typeof(Criterion_SteepFill))]
    [XmlInclude(typeof(Criterion_StairExcav))]
    [XmlInclude(typeof(Criterion_FillCutIntersect))]
    public abstract class StaticCriterion
    {
        public const string ctg_Judge = "判断";
        public const string ctg_Calculate = "计量";

        /// <summary> 显示在属性编辑窗口中的标题 </summary>
        public abstract string FormTitle { get; }
    }

    /// <summary> 各类工程量的判断与计量标准 </summary>
    [Serializable()]
    [XmlType(typeName: "各类工程量的判断与计量标准")]
    public class StaticCriterions
    {
        /// <summary> 将判断与计量标准导出到文件的后缀名，比如 “ "低填浅挖(*.tfsc)| *.tfsc" ”</summary>
        public const string FileExtensionFilter = "工程量计量准则(*.sqc)| *.sqc";

        [XmlArray(elementName: "计量准则")]
        public StaticCriterion[] Criterions { get; set; }

        #region ---   构造全局唯一的实例对象

        private static StaticCriterions _uniqueInstance;

        /// <summary> 全局唯一的实例对象 </summary>
        public static StaticCriterions UniqueInstance
        {
            get
            {
                _uniqueInstance = _uniqueInstance ?? new StaticCriterions();
                return _uniqueInstance;
            }
        }

        /// <summary> 私有的构造函数 </summary>
        private StaticCriterions()
        {
            Criterions = new StaticCriterion[]
            {
                Criterion_ThinFillShallowCut.UniqueInstance,
                Criterion_SteepFill.UniqueInstance,
                Criterion_StairExcav.UniqueInstance,
                Criterion_HighFillDeepCut.UniqueInstance,
            };

            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }

        #endregion
    }
}