using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity.Options
{
    public static class Options_General
    {
        /// <summary> 横断面信息的块参照中，标识里程信息的属性定义的名称 </summary>
        public static string StationFieldDef = "0006";

        /// <summary> 道路大致宽度，单位为 m </summary>
        public static double RoadWidth = 30;


        /// <summary> 水位标高，单位为m </summary>
        public static double WaterLevel = 1736.8;

        /// <summary> 填方防护考虑水位 </summary>
        public static bool ConsiderWaterLevel = true;

        /// <summary> 填方边坡防护的最高标高，其值一般是相对于水位标高而言的，比如位于水位标高之上1.0m </summary>
        public static double FillUpperEdge = 1738;
    }
}