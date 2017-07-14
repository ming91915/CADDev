using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace eZcad.Examples
{
    /// <summary> 根据两条样条曲线创建出位于其中间的一条新样条曲线 
    /// Finding an AutoCAD spline between two others using .NET </summary>
    /// <remarks>参考：http://www.tuicool.com/articles/BFJVju </remarks>
    public class SplineHandler
    {
        [CommandMethod("SBS")]
        public static void SplineBetweenSplines()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            // Select our splines
            var peo = new PromptEntityOptions("\nSelect first spline");
            peo.SetRejectMessage("\nMust be a spline.");
            peo.AddAllowedClass(typeof(Spline), true);
            var per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            var spId1 = per.ObjectId;
            peo.Message = "\nSelect second spline";
            per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            var spId2 = per.ObjectId;
            // Create a transaction

            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Open our splines
                var sp1 =
                    tr.GetObject(spId1, OpenMode.ForRead) as Spline;
                var sp2 =
                    tr.GetObject(spId2, OpenMode.ForRead) as Spline;

                if (sp1 != null && sp2 != null)
                {
                    try
                    {
                        // Get Ge equivalents of the two splines

                        var cur1 = sp1.GetGeCurve() as NurbCurve3d;
                        var cur2 = sp2.GetGeCurve() as NurbCurve3d;

                        if (cur1 != null && cur2 != null)
                        {
                            // Find the middle curve between the two
                            var cur3 = MiddleCurve(cur1, cur2);
                            if (cur3 != null)
                            {
                                // Create a spline from this middle curve
                                var sp = Curve.CreateFromGeCurve(cur3);
                                if (sp != null)
                                {
                                    // Add our new spline to the database
                                    var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                                    btr.AppendEntity(sp);
                                    tr.AddNewlyCreatedDBObject(sp, true);
                                }
                            }
                        }
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        ed.WriteMessage("\nException: {0}", ex.Message);
                    }
                }
            }
        }
        
        /// <summary>
        /// 根据两条样条曲线创建出位于其中间的一条新样条曲线
        /// </summary>
        /// <param name="cur1"></param>
        /// <param name="cur2"></param>
        /// <returns></returns>
        private static NurbCurve3d MiddleCurve(NurbCurve3d cur1, NurbCurve3d cur2)
        {
            // Return a NurbCurve3d that's halfway between those passed in

            // Start by getting the period of both curves

            double per1, per2;
            var ip1 = cur1.IsPeriodic(out per1);
            var ip2 = cur2.IsPeriodic(out per2);

            // Make the sure the curves have the same degree, period,
            // number of control points, knots and weights

            if (
                cur1.Degree != cur2.Degree || ip1 != ip2 || per1 != per2 ||
                cur1.NumberOfControlPoints != cur2.NumberOfControlPoints ||
                cur1.NumberOfKnots != cur2.NumberOfKnots ||
                cur1.NumWeights != cur2.NumWeights
                )
                return null;

            var degree = cur1.Degree;
            var period = ip1;

            // Get the set of averaged control points
            var numPoints = cur1.NumberOfControlPoints;
            var pts = new Point3dCollection();
            for (var i = 0; i < numPoints; i++)
            {
                var pt1 = cur1.ControlPointAt(i);
                var pt2 = cur2.ControlPointAt(i);
                pts.Add(pt1 + (pt2 - pt1) / 2);
            }

            // Get the set of averaged knots
            var numKnots = cur1.NumberOfKnots;
            var knots = new KnotCollection();

            for (var i = 0; i < numKnots; i++)
            {
                knots.Add((cur1.KnotAt(i) + cur2.KnotAt(i)) / 2);
            }
            // Get the set of averaged weights
            var numWeights = cur1.NumWeights;
            var weights = new DoubleCollection();

            for (var i = 0; i < numWeights; i++)
            {
                knots.Add((cur1.GetWeightAt(i) + cur2.GetWeightAt(i)) / 2);
            }
            // Create our new Ge curve based on all this data
            return new NurbCurve3d(degree, knots, pts, weights, period);
        }
    }
}