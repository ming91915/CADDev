using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace eZcad.SubgradeQuantity.Utility
{

    /// <summary> 用来记录一些静态成员的实例类（只类只有一个全局唯一的实例对象） </summary>
    [Serializable()]
    [XmlInclude(typeof(Criterion_ThinFillShallowCut))]
    public abstract class StaticCriterion
    {
        public const string ctg_Judge = "判断";
        public const string ctg_Calculate = "计量";
        
        /// <summary> 显示在属性编辑窗口中的标题 </summary>
        public abstract string FormTitle { get; }
        public abstract string FileExtension { get; }

    }

    /// <summary> 判断标准——低填浅挖 </summary>
    public class Criterion_ThinFillShallowCut : StaticCriterion
    {
        [Browsable(false)]
        public override string FormTitle { get { return "低填浅挖相关指标"; } }
        [Browsable(false)]
        public override string FileExtension { get { return "低填浅挖(*.tfsc)| *.tfsc"; } }

        #region ---   判断标准——低填浅挖

        /// <summary> 低填浅挖中，判断低填路堤中的中心填方高度（路面与自然地面）的最大值，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断低填路堤中的中心填方高度（路面与自然地面）的最大值，单位为米")]
        public double ThinFill_MaxDepth { get; set; }
        
        /// <summary> 低填浅挖中，判断低填路堤时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断低填路堤时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n")]
        public double 低填射线坡比 { get; set; }
        // public double ThinFill_SlopeCriterion_upper { get; set; }


        /// <summary> 低填浅挖中，判断低填路堤时，从中心线与自然地面交点向下进行倾角放射，射线角度为1:n </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断低填路堤时，从中心线与自然地面交点向下进行倾角放射，射线角度为1: n")]
        public double ThinFill_SlopeCriterion_lower { get; set; }


        /// <summary> 低填浅挖中，路槽中点（或道路中点）以下要保证0.8m的加固区，当路槽中点与自然地面的高度小于0.8m时，
        /// 需要在自然地面以下进行地基加固处理。此变量对应为0.8m的加固区，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，路槽中点（或道路中点）以下要保证0.8m的加固区，当路槽中点与自然地面的高度小于0.8m时， 需要在自然地面以下进行地基加固处理。此变量对应为0.8m的加固区，单位为米")]
        public double ThinFill_TreatedDepth { get; set; }
        
        /// <summary> 低填浅挖中，判断浅挖路堑中的中心挖方高度（路面与自然地面）的最大值，单位为米 </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断浅挖路堑中的中心挖方高度（路面与自然地面）的最大值，单位为米")]
        public double ShallowCut_MaxDepth { get; set; }
        
        /// <summary> 低填浅挖中，判断浅挖路堑时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n </summary>
        [Browsable(true), Category(ctg_Judge), Description("低填浅挖中，判断浅挖路堑时，从中心线与自然地面交点向上进行倾角放射，射线角度为1:n")]
        public double ShallowCut_SlopeCriterion_upper { get; set; }

        #endregion

        #region ---   构造全局唯一的实例对象

        private static Criterion_ThinFillShallowCut _uniqueInstance;
        /// <summary> 全局唯一的实例对象：低填浅挖的判断标准 </summary>
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
            ThinFill_MaxDepth = 1.5;
            低填射线坡比 = 5;
            ThinFill_SlopeCriterion_lower = 5;
            ThinFill_TreatedDepth = 0.8;
            //
            ShallowCut_MaxDepth = 1.5;
            ShallowCut_SlopeCriterion_upper = 5;

            // 这一句必须保留，因为在序列化时会直接进行此处的 public 构造函数，而不会从 public static DefinitionCollection GetUniqueInstance() 进入。
            // 此时必须通过这一句保证 _uniqueInstance 与本全局对象的同步。
            _uniqueInstance = this;
            //
        }
        #endregion

    }
}
