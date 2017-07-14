using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Cmds
{


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

    [EcDescription("根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统")]
    public class Ec_ConstructSections : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SectionsConstructor();
            return AddinManagerDebuger.DebugInAddinManager(s.ConstructSections,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统")]
    public class Ec_ConstructSlopes : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            // var s = new SlopeConstructor();
            return AddinManagerDebuger.DebugInAddinManager(SlopeConstructor.ConstructSlopes,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }



    [EcDescription("在界面中选择边坡线以进行设置")]
    public class Ec_SetProtectionStyle : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(SlpInfosSetter.SetSlopeProtection,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("提取所有的横断面块参照的信息")]
    public class Ec_FindAllSections : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(MileagesFinder.FindAllSections,
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

    #region ---   数据提取与导出


    [EcDescription("防护信息的提取")]
    public class Ec_ExportSlopeInfos : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var sp = new InfosGetter_Slope();
            return AddinManagerDebuger.DebugInAddinManager(sp.ExportSlopeInfos,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("导出低填浅挖数据")]
    public class Ec_ExportThinFill : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var sp = new InfosGetter_ThinFill();
            return AddinManagerDebuger.DebugInAddinManager(sp.ExportThinFill,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    #endregion

}