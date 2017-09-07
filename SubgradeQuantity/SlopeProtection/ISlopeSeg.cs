using System;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;
using eZstd.Mathematics;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    public enum SlopeSegType
    {
        边坡,
        平台,
    }

    /// <summary> 边坡分段，比如每一级斜坡或者平台 </summary>
    public interface ISlopeSeg : ICloneable
    {
        SlopeSegType Type { get; }

        double Index { get; set; }

        /// <summary> 靠近路面的点 </summary>
        Point3d InnerPoint { get; }

        /// <summary> 远离路面的点 </summary>
        Point3d OuterPoint { get; }

        // ------------------ 计算得到的几何数据 
        double Degree { get; }
        double SlopeRatio { get; }
        /// <summary> 子边坡或子平台的斜坡几何长度 </summary>
        double Length { get; }
        /// <summary> 子边坡或子平台的高度 </summary>
        double SegHeight { get; }
        Point3d MiddlePoint { get; }

        // ------------------ 用户设定的数据
        /// <summary> 子边坡或子平台的防护长度，它与<seealso cref="Length"/>并不一定相同，比如子边坡长12m，而只有5米设置了防护 </summary>
        string ProtectionMethod { get; set; }
        double ProtectionLength { get; set; }
        Handle ProtectionMethodText { get; set; }

        ///// <summary> 按 坡高:坡宽 = 1:n 的模式，计算出来的坡比的绝对值，比如某边坡坡率为 1:-0.75，则返回 0.75 </summary>
        //double SlopeRatio { get; }

    }
}