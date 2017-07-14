using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary> 边坡分段，比如每一级斜坡或者平台 </summary>
    public interface ISlopeSeg
    {
        string Type { get; }
        int Index { get; set; }
        double Length { get; }
        string ProtectionMethod { get; set; }
        double ProtectionLength { get; set; }
        double SlopeRatio { get; }
    }

    /// <summary> 每一级边坡的信息 </summary>
    public class Slope : ISlopeSeg
    {
        public string Type { get { return "边坡"; } }

        #region   ---   XData Fields
        /// <summary> 第几级坡，第一级边坡的下标为 1</summary>
        public int Index { get; set; }

        public Point3d TopPoint { get; }
        public Point3d BottomPoint { get; }
        /// <summary> 边坡的斜坡长度 </summary>
        public double Length { get; }
        /// <summary> 按 坡高:坡宽 = 1:n 的模式，计算出来的坡比的绝对值，比如某边坡坡率为 1:-0.75，则返回 0.75 </summary>
        public double SlopeRatio { get; }
        //
        public string ProtectionMethod { get; set; }
        /// <summary> 边坡的防护长度，一般情况下，其值是与<seealso cref="Length"/>值相同的，但是在一些特殊的情况下，
        /// 比如水下填方边坡防护时，水位线以上的部分可能不用防护，所以就会出现某一级边坡的防护长度小于实际边坡长度的情况 </summary>
        public double ProtectionLength { get; set; }
        //
        /// <summary> 从坡顶指向坡底的方向向量 </summary>
        public Vector3d SlopeVector { get; private set; }
        public double SlopeHeight { get; private set; }

        #endregion

        public Slope(int index, Point3d topPt, Point3d bottomPt)
        {
            Index = index;
            TopPoint = topPt;
            BottomPoint = bottomPt;
            //
            Length = topPt.DistanceTo(bottomPt);
            ProtectionLength = Length;
            SlopeVector = new Vector3d(topPt.X - bottomPt.X, topPt.Y - bottomPt.Y, 0);
            //
            SlopeHeight = topPt.Y - bottomPt.Y;
            SlopeRatio = Math.Abs(SlopeVector.X) / SlopeVector.Y; // 边坡坡率为 1:dir
        }

        #region   ---   数据 与 ResultBuffer 的转换

        public static Slope FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            try
            {
                var index = (int)buffs[0].Value;
                var topPt = (Point3d)buffs[1].Value;
                var bottomPt = (Point3d)buffs[2].Value;
                var pf = new Slope(index, topPt, bottomPt)
                {
                    ProtectionMethod = (string)buffs[3].Value,
                    ProtectionLength = (double)buffs[4].Value
                };
                var pt = (Point3d)buffs[5].Value;
                pf.SlopeVector = new Vector3d(pt.X, pt.Y, pt.Z); // 在XData中，无法记录 Vector3d 类型的数据，只能用 Point3d 进行转换
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
                     new TypedValue((int)DxfCode.ExtendedDataXCoordinate, TopPoint),
                     new TypedValue((int)DxfCode.ExtendedDataXCoordinate, BottomPoint),
                     new TypedValue((int)DxfCode.ExtendedDataAsciiString, ProtectionMethod),
                     //
                     new TypedValue((int)DxfCode.ExtendedDataReal, ProtectionLength),
                     // 在XData中，无法记录 Vector3d 类型的数据，只能用 Point3d 进行转换
                     new TypedValue((int)DxfCode.ExtendedDataWorldXDir, new Point3d(SlopeVector.X, SlopeVector.Y, SlopeVector.Z))
                     );
            return data;
        }

        #endregion

        public override string ToString()
        {
            return $"第 {Index} 级边坡";
        }
    }

    /// <summary> 边坡平台 </summary>
    public class Platform : ISlopeSeg
    {
        public string Type { get { return "平台"; } }

        #region   ---   XData Fields

        /// <summary> 平台位于第 Index 与 Index+1 级边坡之间，0 表示路面位置的平台，比如挖方边坡的碎落台 </summary>
        public int Index { get; set; }

        public Point3d Middle { get; set; }
        public double Length { get; }
        /// <summary> 平台的防护方式 </summary>
        public string ProtectionMethod { get; set; }
        /// <summary> 平台的防护长度，一般情况下，其值是与<seealso cref="Length"/>值相同的，但是在一些特殊的情况下，防护长度可能会小于实际的平台长度 </summary>
        public double ProtectionLength { get; set; }
        /// <summary> 坡率，对于平台而言，其值总为 0  </summary>
        public double SlopeRatio { get; }
        #endregion

        public Platform(int index, Point3d middle, double length)
        {
            Index = index;
            Middle = middle;
            Length = length;
            ProtectionLength = length;
            //
            SlopeRatio = 0;
        }

        #region   ---   数据 与 ResultBuffer 的转换

        public static Platform FromResultBuffer(ResultBuffer buff)
        {
            var buffs = buff.AsArray();
            try
            {
                var index = (int)buffs[0].Value;
                var middle = (Point3d)buffs[1].Value;
                var length = (double)buffs[2].Value;
                var pf = new Platform(index, middle, length)
                {
                    ProtectionMethod = (string)buffs[3].Value,
                    ProtectionLength = (double)buffs[4].Value
                };
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
                new TypedValue((int)DxfCode.ExtendedDataXCoordinate, Middle),
                new TypedValue((int)DxfCode.ExtendedDataReal, Length),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, ProtectionMethod ?? ""),
                new TypedValue((int)DxfCode.ExtendedDataReal, ProtectionLength)
                );
            return data;
        }
        #endregion

        public override string ToString()
        {
            return $"第 {Index} 级平台";
        }
    }
}