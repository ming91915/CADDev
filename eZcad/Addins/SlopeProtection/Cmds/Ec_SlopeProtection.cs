using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.Addins.SlopeProtection;
using eZcad.Utility;

namespace eZcad.Addins.SlopeProtection
{
    [EcDescription("在界面中选择边坡线以进行设置")]
    public class Ec_SetProtectionStyle : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(SpInfosSetter.SetSlopeProtection,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("提取所有的横断面块参照的信息")]
    public class Ec_FindAllSections : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(SectionsFinder.FindAllSections,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("防护信息的提取")]
    public class Ec_ExportSlopeInfos : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var sp = new SpInfosGetter();
            return AddinManagerDebuger.DebugInAddinManager(sp.ExportSlopeInfos,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("边坡防护选项设置")]
    public class Ec_SetSlopeOptions : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(OptionsSetter.SetSlopeOptions,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    

    [EcDescription("边坡信息漫游")]
    public class Ec_SlopeWalk : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeWalker();
            return AddinManagerDebuger.DebugInAddinManager(s.SlopeWalk,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }
}