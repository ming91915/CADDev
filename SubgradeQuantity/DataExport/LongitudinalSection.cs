using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.DataExport
{
    /// <summary> 由道路中桩设计线与其对应的自然地面线所组成的纵断面图 </summary>
    public class LongitudinalSection
    {
        #region --- Fields

        private readonly DocumentModifier _docMdf;
        public readonly CompositeCurve2d RoadCurve2d;
        public readonly CompositeCurve2d GroundCurve2d;
        /// <summary> 路线的起点桩号 </summary>
        public readonly double StartStation;
        /// <summary> 路线的终点桩号 </summary>
        public readonly double EndStation;
        public readonly CurveCurveIntersector2d Intersects;
        //private readonly Polyline _road;
        //private readonly Polyline _ground;
        /// <summary> 每个交点桩号所对应的交点坐标 </summary>
        public Dictionary<double, Point2d> IntersPoints;

        #endregion

        /// <summary> 构造函数 </summary>
        /// <param name="docMdf"></param>
        /// <param name="roadCurve"></param>
        /// <param name="groundCurve"></param>
        public LongitudinalSection(DocumentModifier docMdf, CompositeCurve3d roadCurve, CompositeCurve3d groundCurve)
        {
            _docMdf = docMdf;
            RoadCurve2d = roadCurve.Get2dLinearCurve();
            GroundCurve2d = groundCurve.Get2dLinearCurve();
            //
            StartStation = RoadCurve2d.StartPoint.X;
            EndStation = RoadCurve2d.EndPoint.X;

            // 求交点
            Intersects = new CurveCurveIntersector2d(RoadCurve2d, GroundCurve2d);
            GetIntersects();
            //_road = Curve.CreateFromGeCurve(roadCurve) as Polyline;
            //_ground = Curve.CreateFromGeCurve(groundCurve) as Polyline;
        }


        private void GetIntersects()
        {
            IntersPoints = new Dictionary<double, Point2d>();
            for (int i = 0; i < Intersects.NumberOfIntersectionPoints; i++)
            {
                var pt = Intersects.GetIntersectionPoint(i);
                IntersPoints.Add(pt.X, pt);
            }
        }

        /// <summary> 交界点是从填进行挖，还是从挖进入填 </summary>
        /// <returns></returns>
        public bool FilltoCut(PointOnCurve2d ptRoad, PointOnCurve2d ptGround)
        {
            var ratioRoad = ptRoad.GetDerivative(1);
            var ratioGround = ptGround.GetDerivative(1);
            if (Math.Tan(ratioGround.Angle) > Math.Tan(ratioRoad.Angle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 纵断面中某个桩号所对应的填方高度，如果为负值，则代表挖方高度 </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public double GetFillHeight(double station)
        {
            var intersVerticalRoad = new CurveCurveIntersector2d(RoadCurve2d,
                new Line2d(new Point2d(station, 0), new Vector2d(0, 1)));
            var intersVerticalGround = new CurveCurveIntersector2d(GroundCurve2d,
                new Line2d(new Point2d(station, 0), new Vector2d(0, 1)));
            if (intersVerticalRoad.NumberOfIntersectionPoints == 0 ||
                intersVerticalGround.NumberOfIntersectionPoints == 0)
            {
                // 这种情况一般不会出现
            }
            else
            {
                var yRoad = intersVerticalRoad.GetIntersectionPoint(0).Y;
                var yGround = intersVerticalGround.GetIntersectionPoint(0).Y;
                return yRoad - yGround;
            }
            return 0.0;
        }
    }
}