using System;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;
using eZstd.Mathematics;

namespace eZcad.SubgradeQuantities.SlopeProtection
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
        double Length { get; }
        Point3d MiddlePoint { get; }

        // ------------------ 用户设定的数据
        string ProtectionMethod { get; set; }
        double ProtectionLength { get; set; }
        Handle ProtectionMethodText { get; set; }

        ///// <summary> 按 坡高:坡宽 = 1:n 的模式，计算出来的坡比的绝对值，比如某边坡坡率为 1:-0.75，则返回 0.75 </summary>
        //double SlopeRatio { get; }

        /// <summary> 根据二维矢量返回其相对于正X轴沿逆时针的角度值，其值的范围为[0, 360度) </summary>
    }

    /// <summary> 每一级边坡的信息 </summary>
    public class Slope : ISlopeSeg
    {
        public SlopeSegType Type
        {
            get { return SlopeSegType.边坡; }
        }

        #region   ---   XData Fields

        // ------------------ 必须存储的几何数据 

        /// <summary> 第几级坡，第一级边坡的下标为 1（也是最小值），当第一级边坡中有多级子边坡（比如一级边坡分3种防护形式）时，
        /// 从内向外的三级子边坡的下标分别为1.1、1.2、1.3，所以本系统在某一级边坡中最多设置9级子边坡 </summary>
        [Browsable(true),ReadOnly(true)]
        public double Index { get; set; }

        /// <summary> 靠近路面的点 </summary>
        public Point3d InnerPoint { get; }

        /// <summary> 远离路面的点 </summary>
        public Point3d OuterPoint { get; }

        // ------------------ 计算得到的几何数据 

        public Point3d TopPoint { get; }
        public Point3d BottomPoint { get; }
        public Point3d MiddlePoint { get; }

        /// <summary> 边坡的斜坡长度 </summary>
        public double Length { get; }

        /// <summary> 按 坡高:坡宽 = 1:n 的模式，计算出来的坡比的绝对值，比如某边坡坡率为 1:-0.75，则返回 0.75 </summary>
        public double SlopeRatio { get; }

        /// <summary> 根据二维矢量返回其相对于正X轴沿逆时针的角度值，其值的范围为[0, 360度) </summary>
        public double Degree { get; }

        public double SlopeHeight { get; private set; }

        // ------------------ 用户设定的数据

        /// <summary> 边坡防护方式 </summary>
        public string ProtectionMethod { get; set; }

        /// <summary> 边坡的防护长度，一般情况下，其值是与<seealso cref="Length"/>值相同的，但是在一些特殊的情况下，
        /// 比如水下填方边坡防护时，水位线以上的部分可能不用防护，所以就会出现某一级边坡的防护长度小于实际边坡长度的情况 </summary>
        public double ProtectionLength { get; set; }

        public Handle ProtectionMethodText { get; set; }

        #endregion

        /// <summary> 构造函数 </summary>
        public Slope(double index, Point3d innerPt, Point3d outerPt)
        {
            Index = index;
            InnerPoint = innerPt;
            OuterPoint = outerPt;
            //
            if (innerPt.Y > outerPt.Y)
            {
                TopPoint = innerPt;
                BottomPoint = outerPt;
            }
            else
            {
                BottomPoint = innerPt;
                TopPoint = outerPt;
            }
            //
            Length = innerPt.DistanceTo(outerPt);
            MiddlePoint = new Point3d((innerPt.X + outerPt.X) / 2, (innerPt.Y + outerPt.Y) / 2, (innerPt.Z + outerPt.Z) / 2);
            Degree = MathUtils.GetAngleD(outerPt.X - innerPt.X, outerPt.Y - innerPt.Y);
            SlopeHeight = TopPoint.Y - BottomPoint.Y;
            var slopeVector = outerPt - innerPt;
            SlopeRatio = Math.Abs(slopeVector.X) / slopeVector.Y; // 边坡坡率为 1:dir
            //
            ProtectionLength = Length;
        }

        #region   ---   数据 与 ResultBuffer 的转换

        public static Slope FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            try
            {
                var index = (int)buffs[0].Value;
                var innerPt = (Point3d)buffs[1].Value;
                var outerPt = (Point3d)buffs[2].Value;
                var pf = new Slope(index, innerPt, outerPt);

                // 用户设置的数据
                pf.ProtectionMethod = (string)buffs[3].Value;
                pf.ProtectionLength = (double)buffs[4].Value;
                pf.ProtectionMethodText = Utils.ConvertToHandle(buffs[5].Value.ToString());

                // 在XData中，无法记录 Vector3d 类型的数据，只能用 Point3d 进行转换
                return pf;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public ResultBuffer ToResultBuffer()
        {
            ResultBuffer data = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataInteger32, Index),
                new TypedValue((int)DxfCode.ExtendedDataXCoordinate, InnerPoint),
                new TypedValue((int)DxfCode.ExtendedDataXCoordinate, OuterPoint),

                // 用户设置的数据
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, ProtectionMethod),
                new TypedValue((int)DxfCode.ExtendedDataReal, ProtectionLength),
                // 在XData中，无法记录 Vector3d 类型的数据，只能用 Point3d 进行转换
                //new TypedValue((int) DxfCode.ExtendedDataWorldXDir,
                //    new Point3d(SlopeVector.X, SlopeVector.Y, SlopeVector.Z))
                new TypedValue((int)DxfCode.ExtendedDataHandle, ProtectionMethodText)
                );
            return data;
        }

        #endregion

        public override string ToString()
        {
            return $"第 {Index.ToString("0.#")} 级边坡，{ProtectionMethod}";
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary> 边坡平台 </summary>
    public class Platform : ISlopeSeg
    {
        public SlopeSegType Type
        {
            get { return SlopeSegType.平台; }
        }

        #region   ---   XData Fields

        /// <summary> 平台位于第 Slope.Index 与 Slope.Index + 1 级边坡之间，0 表示路面位置的平台（也是最小值），比如挖方边坡的碎落台 </summary>
        [Browsable(true),ReadOnly(true)]
        public double Index { get; set; }

        // ------------------ 必须存储的几何数据 

        /// <summary> 靠近路面的点 </summary>
        public Point3d InnerPoint { get; }

        /// <summary> 远离路面的点 </summary>
        public Point3d OuterPoint { get; }

        // ------------------ 计算得到的几何数据 
        public double Length { get; }

        /// <summary> 根据二维矢量返回其相对于正X轴沿逆时针的角度值，其值的范围为[0, 360度)。
        /// 对于平台而言，其值总为 0 或 180 </summary>
        public double Degree { get; }
        /// <summary> 按 坡高:坡宽 = 1:n 的模式，计算出来的坡比的绝对值，比如某边坡坡率为 1:-0.75，则返回 0.75 </summary>
        public double SlopeRatio { get; }

        public Point3d MiddlePoint { get; }

        // ------------------ 用户输入的数据 
        /// <summary> 平台的防护方式 </summary>
        public string ProtectionMethod { get; set; }

        /// <summary> 平台的防护长度，一般情况下，其值是与<seealso cref="Length"/>值相同的，但是在一些特殊的情况下，防护长度可能会小于实际的平台长度 </summary>
        public double ProtectionLength { get; set; }

        public Handle ProtectionMethodText { get; set; }

        #endregion

        public Platform(double index, Point3d innerPt, Point3d outerPt)
        {
            Index = index;
            InnerPoint = innerPt;
            OuterPoint = outerPt;
            //
            Length = OuterPoint.DistanceTo(innerPt);
            Degree = MathUtils.GetAngleD(outerPt.X - innerPt.X, outerPt.Y - innerPt.Y);
            MiddlePoint = new Point3d((innerPt.X + outerPt.X) / 2, (innerPt.Y + outerPt.Y) / 2, (innerPt.Z + outerPt.Z) / 2);
            //
            ProtectionLength = Length;
        }

        #region   ---   数据 与 ResultBuffer 的转换

        public static Platform FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            try
            {
                var index = (int)buffs[0].Value;
                var innerPt = (Point3d)buffs[1].Value;
                var outerPt = (Point3d)buffs[2].Value;
                var pf = new Platform(index, innerPt, outerPt);
                // 用户设置的数据

                pf.ProtectionMethod = (string)buffs[3].Value;
                pf.ProtectionLength = (double)buffs[4].Value;
                pf.ProtectionMethodText = Utils.ConvertToHandle(buffs[5].Value.ToString());

                return pf;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public ResultBuffer ToResultBuffer()
        {
            ResultBuffer data = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataInteger32, Index),
                new TypedValue((int)DxfCode.ExtendedDataXCoordinate, InnerPoint),
                new TypedValue((int)DxfCode.ExtendedDataXCoordinate, OuterPoint),
                //
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, ProtectionMethod ?? ""),
                new TypedValue((int)DxfCode.ExtendedDataReal, ProtectionLength),
                new TypedValue((int)DxfCode.ExtendedDataHandle, ProtectionMethodText)
                );
            return data;
        }

        #endregion

        public override string ToString()
        {
            return $"第 {Index.ToString("0.#")} 级平台，{ProtectionMethod}";
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}