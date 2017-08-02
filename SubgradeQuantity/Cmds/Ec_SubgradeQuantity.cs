using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity.RedundantCmds;

namespace eZcad.SubgradeQuantity.Cmds
{
    #region ---   横断面系统

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

    [EcDescription("提取整个道路所有横断面的信息")]
    public class Ec_FindAllSections : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(StationsFinder.AllSectionsInfo,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("在 AutoCAD 界面中快速导航到指定的桩号")]
    public class Ec_NavigateStation : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new StationNavigator();
            return AddinManagerDebuger.DebugInAddinManager(s.NavigateStation,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("沿着道路纵向绘制边坡线")]
    public class Ec_LongitudinalSection : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new LongitudinalSectionDrawer();
            return AddinManagerDebuger.DebugInAddinManager(s.LongitudinalSection,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    #endregion

    #region ---   边坡防护的设置

    [EcDescription("根据 AutoCAD 中的几何图形构造出完整的路基横断面信息系统")]
    public class Ec_ConstructSlopes : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new SlopeConstructor();
            return AddinManagerDebuger.DebugInAddinManager(s.ConstructSlopes,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }


    [EcDescription("从边坡线所绑定的防护方式的文字对象来设置防护")]
    public class Ec_GetProtectionMethodFromText : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ProtectionFlusher();
            return AddinManagerDebuger.DebugInAddinManager(s.FlushProtection,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    [EcDescription("放置边坡防护的文字")]
    public class Ec_PlaceText : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new ProtectionPlacer();
            return AddinManagerDebuger.DebugInAddinManager(s.PlaceProtection,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    #endregion

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
            return AddinManagerDebuger.DebugInAddinManager(sp.ThinFillShallowCut,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    #endregion

    #region ---   设置

    [EcDescription("路基防护选项设置")]
    public class Ec_SetSlopeOptions : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            return AddinManagerDebuger.DebugInAddinManager(OptionsSetter.SubgradeOptions,
                impliedSelection, ref errorMessage, ref elementSet);
        }
    }

    #endregion

    #region ---   Redundant

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

    #endregion
}