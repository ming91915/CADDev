using System.Collections.Generic;
using System.Windows.Forms;
using AutoCADDev.ExternalCommand;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADDev
{
    public class cmd_Debug1 : IExternalCommand
    {
        public ExternalCommandResult Execute(ref string errorMessage, ref IList<ObjectId> elementSet)
        {
            MessageBox.Show(@"动态调试 Execute");
            errorMessage = "测试一下出错的情况";
            return ExternalCommandResult.Failed;
        }
    }

    public class cmd_Debug2 : IExternalCommand
    {
        public ExternalCommandResult Execute(ref string errorMessage, ref IList<ObjectId> elementSet)
        {
            MessageBox.Show(@"动态调试 AABB");
            return ExternalCommandResult.Succeeded;
        }
    }
}