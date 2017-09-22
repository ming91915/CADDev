using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.DataExport;
using eZcad.SubgradeQuantity.Entities;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    public partial class PF_ExportFillCutInters : ModalPForm
    {

        #region ---   Fields

        private SelectionSet _impliedSelection;

        private Curve _roadCenter;
        private Curve _ground;

        /// <summary> 将某点左乘此矩阵时，表示此点原来位于地理坐标系，其左乘结果为转换为AutoCAD几何坐标系中的坐标值 </summary>
        private Matrix3d _matGC = Matrix3d.Identity;

        /// <summary> 将某点左乘此矩阵时，表示此点原来位于AutoCAD几何坐标系，其左乘结果为转换为地理坐标系中的坐标值 </summary>
        private Matrix3d _matCG = Matrix3d.Identity;

        #endregion

        #region ---   窗口的构造、打开与关闭

        private static PF_ExportFillCutInters _uniqueInstance;

        public static PF_ExportFillCutInters GetUniqueInstance(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _uniqueInstance = _uniqueInstance ?? new PF_ExportFillCutInters();
            _uniqueInstance._docMdf = docMdf;
            _uniqueInstance._impliedSelection = impliedSelection;
            //
            return _uniqueInstance;
        }

        private PF_ExportFillCutInters() : base()
        {
            InitializeComponent();
            //
        }

        #endregion

        #region ---   界面操作

        private void label_roadCenter_Click(object sender, EventArgs e)
        {
            Utils.FocusOnMainUIWindow();
            _roadCenter = GetPolyline(_docMdf.acEditor, "选择道路中桩设计线");
            if (_roadCenter != null)
            {
                label_roadCenter.Text = _roadCenter.Handle.ToString();
                //
                _matCG = Get2dTransform(new Point2d(_roadCenter.StartPoint.X, _roadCenter.StartPoint.Y),
                    new Point2d(_baseStation, _baseElev), 1 / _xScale, 1 / _yScale);
                _matGC = Get2dTransform(new Point2d(_baseStation, _baseElev),
                    new Point2d(_roadCenter.StartPoint.X, _roadCenter.StartPoint.Y), _xScale, _yScale);
                //
                panel_Transform.Enabled = true;
            }
            else
            {
                label_roadCenter.Text = @"***";
                //
                panel_Transform.Enabled = false;
            }
        }

        private void label_ground_Click(object sender, EventArgs e)
        {
            Utils.FocusOnMainUIWindow();
            _ground = GetPolyline(_docMdf.acEditor, "选择地面线");
            label_ground.Text = _ground != null ? _ground.Handle.ToString() : @"***";
        }

        #endregion


        private static Curve GetPolyline(Editor ed, string message)
        {
            // 点选
            var peO = new PromptEntityOptions("\n" + message);
            peO.SetRejectMessage("\n" + message);
            peO.AddAllowedClass(typeof(Polyline), exactMatch: false);
            peO.AddAllowedClass(typeof(Polyline2d), exactMatch: false);

            // 请求在图形区域选择对象
            var res = ed.GetEntity(peO);

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId.GetObject(OpenMode.ForRead) as Curve;
            }
            return null;
        }

        private static Curve[] SelectPolylines(Editor ed, string message)
        {
            var filterType = new[]
            {
                new TypedValue((int) DxfCode.Operator, "<OR"),
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.Start, "POLYLINE"),
                new TypedValue((int) DxfCode.Operator, "OR>"),
            };

            // 点选
            var peO = new PromptSelectionOptions();
            peO.MessageForAdding = "\n" + message; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            peO.MessageForRemoval = "\n" + message;

            // 请求在图形区域选择对象
            var res = ed.GetSelection(peO, new SelectionFilter(filterType));

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds().Select(r => r.GetObject(OpenMode.ForRead)).OfType<Curve>().ToArray();
            }
            return null;
        }

        private double[][] MergeRanges(double[][] rawRanges)
        {

            return rawRanges;
        }

        /// <summary>
        /// 将一个n行2列的矩阵所表示的n个区间进行并集组合，形成新的分段，每个分段之间互不相交。
        /// </summary>
        /// <param name="rawRanges">表示一个n行2列的矩阵</param>
        /// <param name="smallToLarge">true 表示输入数组中每一行中的两个元素都是小的在前；false 表示每一行中的两个元素都是大的在前；
        /// null 表示每一行中的两个元素的大小排序并不确定</param>
        /// <returns></returns>
        public static double[][] MergeRanges(double[][] rawRanges, bool? smallToLarge = null)
        {
            if (rawRanges.Length <= 1) return rawRanges;

            // 1. 对每一行中的两个元素的大小进行排序，使其较小值在前
            if (smallToLarge == null)
            {
                for (int i = 0; i < rawRanges.Length; i++)
                {
                    rawRanges[i] = new double[] { Math.Min(rawRanges[i][0], rawRanges[i][1]), Math.Max(rawRanges[i][0], rawRanges[i][1]) };
                }
            }
            else if (!smallToLarge.Value)
            {
                double largerValue;
                foreach (var rr in rawRanges)
                {
                    largerValue = rr[0];
                    rr[0] = rr[1];
                    rr[1] = largerValue;
                }
            }
            // 2. 对多个行之间进行排序，使第一个元素（较小值）在前的那一行排在前面
            Array.Sort(rawRanges, SortRowsToMerge);

            // 3. 进行多个区间的组合
            var mergedRanges = new List<double[]>();
            double lastSmallerValue = rawRanges[0][0];
            double lastLargerValue = rawRanges[0][1];
            for (int i = 1; i < rawRanges.Length; i++)
            {
                if (rawRanges[i][0] <= lastLargerValue) // 说明这两行可以融合到一起
                {
                    lastLargerValue = rawRanges[i][1];

                    // 将两个区间的某些属性进行融合

                }
                else
                {
                    // 说明此时应该创建一个新的分段
                    mergedRanges.Add(new double[] { lastSmallerValue, lastLargerValue });
                    //
                    lastSmallerValue = rawRanges[i][0];
                    lastLargerValue = rawRanges[i][1];
                }
            }
            // 最后一个分段可能还未添加到集合中
            if (mergedRanges.Count == 0
                || lastLargerValue != mergedRanges[mergedRanges.Count - 1][1])
            {
                mergedRanges.Add(new double[] { lastSmallerValue, lastLargerValue });
            }
            return mergedRanges.ToArray();
        }

        private static int SortRowsToMerge(double[] row1, double[] row2)
        {
            return row1[0].CompareTo(row2[0]);
        }

        #region ---   坐标变换

        private double _baseStation = 0;
        private double _baseElev = 0;
        private double _xScale = 1;
        private double _yScale = 1;

        private void textBoxNum_Transform_TextChanged(object sender, EventArgs e)
        {
            if (_roadCenter != null)
            {
                var tol = 0.000000001;
                _baseStation = textBoxNum_StartStation.ValueNumber;
                _baseElev = textBoxNum_StartElevation.ValueNumber;
                _xScale = textBoxNum_xScale.ValueNumber;
                _yScale = textBoxNum_yScale.ValueNumber;
                if (Math.Abs(_xScale) < tol || Math.Abs(_yScale) < tol)
                {
                    return;
                }
                //
                _matCG = Get2dTransform(new Point2d(_roadCenter.StartPoint.X, _roadCenter.StartPoint.Y),
                    new Point2d(_baseStation, _baseElev), 1 / _xScale, 1 / _yScale);
                _matGC = Get2dTransform(new Point2d(_baseStation, _baseElev),
                    new Point2d(_roadCenter.StartPoint.X, _roadCenter.StartPoint.Y), _xScale, _yScale);
                // dataGridView_Excludes.Refresh();
            }
            else
            {
                MessageBox.Show(@"请先选择道路中桩设计线");
                return;
            }
        }

        /// <summary> 从某点P从 坐标系 <paramref name="pFrom"/> 到 <paramref name="pTo"/> 点的变换矩阵 </summary>
        /// <param name="pFrom"></param>
        /// <param name="pTo"></param>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        /// <returns></returns>
        private static Matrix3d Get2dTransform(Point2d pFrom, Point2d pTo, double xScale, double yScale)
        {
            var dx = pTo.X - pFrom.X * xScale;
            var dy = pTo.Y - pFrom.Y * yScale;

            double[] data3d = new double[]
            {
                xScale, 0, 0, dx,
                0, yScale, 0, dy,
                0, 0, 1, 0,
                0, 0, 0, 1,
            };
            var m3 = new Matrix3d(data3d);

            //double[] data2d = new double[]
            //{
            //    xScale, 0, dx,
            //    0, yScale, dy,
            //    0, 0, 1,
            //};
            var m2 = new Matrix3d(data3d);
            return m2;
        }

        private double[] GetStationsFromCurve(Curve c)
        {
            var s = GetGFromC(new Point3d(c.StartPoint.X, c.StartPoint.Y, 0)).X;
            var e = GetGFromC(new Point3d(c.EndPoint.X, c.EndPoint.Y, 0)).X;
            return new double[] { s, e };
        }

        private Point3d GetGFromC(Point3d p)
        {
            return p.TransformBy(_matCG);
        }

        #endregion

        protected override void OnCmdRun(bool closeWindow)
        {
            if (_roadCenter == null || _ground == null)
            {
                MessageBox.Show("请先选择道路中桩设计线与地面线");
                return;
            }
            //
            base.OnCmdRun(closeWindowWhenFinished: true);
            //
            if (_roadCenter.StartPoint.X > _roadCenter.EndPoint.X)
            {
                _roadCenter.UpgradeOpen();
                _roadCenter.ReverseCurve();
                _roadCenter.DowngradeOpen();
            }
            if (_ground.StartPoint.X > _ground.EndPoint.X)
            {
                _ground.UpgradeOpen();
                _ground.ReverseCurve();
                _ground.DowngradeOpen();
            }
            var roadCurve = _roadCenter.GetGeCurve() as CompositeCurve3d;
            roadCurve.TransformBy(_matCG);
            var groundCurve = _ground.GetGeCurve() as CompositeCurve3d;
            groundCurve.TransformBy(_matCG);
            //
            var ls = new LongitudinalSection(_docMdf, roadCurve, groundCurve);
            if (ls.Intersects.NumberOfIntersectionPoints == 0)
            {
                _docMdf.WriteNow("没有填挖交界交点");
                return;
            }
            else
            {
                if (radioButton_FillCutInters.Checked)
                {
                    var exp = new Exporter_FillCutInters(_docMdf, ls);
                    exp.ExportFillCutInters();
                }
                else if (radioButton_LongitudinalStairExcav.Checked)
                {
                    var exp = new Exporter_StairsExcavLong(_docMdf, ls);
                    exp.ExportLongitudinalStairExcav();
                }
            }
        }
    }
}