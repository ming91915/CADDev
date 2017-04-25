using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace eZcad.AddinManager
{
    /// <summary>
    /// 用来作为实现 ExternalCommand 的类的描述
    /// </summary>
    public class EcDescriptionAttribute : Attribute
    {
        /// <summary> 具体的描述 </summary>
        public readonly string Description;

        public EcDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    public enum ExternalCommandResult
    {
        Cancelled = 0,
        Succeeded = 1,
        Failed = 2,
    }

    /// <summary> 用来进行AddinManager快速调试的接口。实现此接口的类必须有一个无参数的构造函数。
    /// 另外，推荐实现此接口的类所在程序集中也设计一个实现 IDllActivator_std 接口的类。 </summary>
    public interface ICADExCommand
    {
        /// <summary> CAD AddinManger 快速调试插件 </summary>
        /// <param name="impliedSelection"> 用户在执行方法之前已经选择好的对象。</param>
        /// <param name="errorMessage">当返回值为<see cref="ExternalCommandResult.Failed"/>时，这个属性代表给出的报错信息。</param>
        /// <param name="elementSet">当返回值为<see cref="ExternalCommandResult.Failed"/>时，这个属性代表与出错内容相关的任何对象。</param>
        /// <returns></returns>
        ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage, ref IList<ObjectId> elementSet);
    }
}
