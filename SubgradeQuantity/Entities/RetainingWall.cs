using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Options;

namespace eZcad.SubgradeQuantity.Entities
{
    public enum RetainingWallType:short
    {
        /// <summary> 无挡土墙 </summary>
        无 = 0,
        路肩墙,
        /// <summary> 包括护脚墙，即挡墙上方还有边坡 </summary>
        路堤墙,
        /// <summary> 所有挖方的挡墙类型。 </summary>
        路堑墙,
        /// <summary> 有挡土墙，但是不明确它的类型 </summary>
        其他,
    }

    /// <summary> 挡土墙 </summary>
    public class RetainingWall
    {
        #region --- Fields
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

        private static readonly SelectionFilter _filter = new SelectionFilter(new[]{

                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RetainingWall_Left),
                new TypedValue((int) DxfCode.LayerName, Options_LayerNames.LayerName_RetainingWall_Right),
                new TypedValue((int) DxfCode.Operator, "OR>")
        });

        /// <summary> 从 AutoCAD 界面中过滤出边坡对象的过滤规则 </summary>
        public static SelectionFilter Filter
        {
            get
            {
                return _filter;
            }
        }

        #endregion

        public RetainingWall(Polyline retainWall)
        {
            RetainWall = retainWall;
        }

        #region --- 私有方法

        private CompositeCurve3d GetGeCurve()
        {
            return RetainWall.GetGeCurve() as CompositeCurve3d;
        }
        private Curve3d[] GetGeCurves()
        {
            return WallCurve.GetCurves();
        }

        #endregion

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
