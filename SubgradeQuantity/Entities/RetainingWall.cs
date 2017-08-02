using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.SubgradeQuantity.Entities
{
    /// <summary>
    /// 挡土墙
    /// </summary>
    public class RetainingWall
    {
        public Polyline RetainWall { get; }

        private CompositeCurve3d _wallCurve;
        public CompositeCurve3d WallCurve
        {
            get
            {
                _wallCurve = _wallCurve ?? GetGeCurve();
                return _wallCurve;
            }
        }

        private Curve3d[] _wallCurves;
        public Curve3d[] WallCurves
        {
            get
            {
                _wallCurves = _wallCurves ?? GetGeCurves();
                return _wallCurves;
            }
        }

        public RetainingWall(Polyline retainWall)
        {
            RetainWall = retainWall;
        }

        private CompositeCurve3d GetGeCurve()
        {
            return RetainWall.GetGeCurve() as CompositeCurve3d;
        }
        private Curve3d[] GetGeCurves()
        {
            return WallCurve.GetCurves();
        }

        /// <summary> 挡墙顶部在AutoCAD中的Y坐标值 </summary>
        public double GetTopY()
        {
            var top = double.MinValue;
            Point3d pt;
            foreach (var curve3D in WallCurves)
            {
                pt = curve3D.StartPoint;
                top = pt.Y > top ? pt.Y : top;
                pt = curve3D.EndPoint;
                top = pt.Y > top ? pt.Y : top;
            }
            return top;
        }
        /// <summary> 挡墙底部在AutoCAD中的Y坐标值 </summary>
        public double GetBottomY()
        {
            var bottom = double.MaxValue;
            Point3d pt;
            foreach (var curve3D in WallCurves)
            {
                pt = curve3D.StartPoint;
                bottom = pt.Y < bottom ? pt.Y : bottom;
                pt = curve3D.EndPoint;
                bottom = pt.Y < bottom ? pt.Y : bottom;
            }
            return bottom;
        }
    }
}
