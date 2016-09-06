using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADDev.ExternalCommand
{
    public enum ExternalCommandResult
    {
        Cancelled = 0,
        Succeeded = 1,
        Failed = 2,
    }

    /// <summary> 用来进行AddinManager快速调试的接口。实现此接口的类必须有一个无参数的构造函数 </summary>
    public interface IExternalCommand
    {
        ExternalCommandResult Execute(ref string errorMessage, ref IList<ObjectId> elementSet);
    }
}
