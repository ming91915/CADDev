using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eZcad.SubgradeQuantities.Entities;
using eZcad.SubgradeQuantities.Redundant;

namespace eZcad.SubgradeQuantities.DataExport
{

    /// <summary> 工程量表类型 </summary>
    public enum WorkSheetDataType
    {
        /// <summary> 数据源 </summary>
        SourceData,
        /// <summary> 所有的桩号横断面 </summary>
        AllStations,
        /// <summary> 边坡防护工程量表 </summary>
        SlopeProtection,
        /// <summary> 高填深挖工程量表 </summary>
        ThickFillCut,
    }

    /// <summary>
    /// 放入一个Excel工作表的全部数据
    /// </summary>
    public class WorkSheetData
    {
        public WorkSheetDataType Type { get; private set; }
        public Array Data { get; set; }
        public readonly string SheetName;
        public readonly ProtectionStyle ProtectionStyle;
        public readonly bool OnLeft;

        /// <summary> 构造函数 </summary>
        /// <param name="type"></param>
        /// <param name="sheetName"></param>
        /// <param name="data">一个二维数组，表示工作表中的所有数据（包括表头）</param>
        public WorkSheetData(WorkSheetDataType type, string sheetName, Array data)
        {
            Type = type;
            SheetName = sheetName;
            Data = data;
        }

        public WorkSheetData(string sheetName, Array data, ProtectionStyle protectionStyle, bool onLeft)
        {
            Type = WorkSheetDataType.SlopeProtection;
            SheetName = sheetName;
            ProtectionStyle = protectionStyle;
            OnLeft = onLeft;
            Data = data;
        }

        public override string ToString()
        {
            var onleft = OnLeft ? "左" : "右";
            return $"{SheetName},{onleft},{Data}";
        }
    }
}
