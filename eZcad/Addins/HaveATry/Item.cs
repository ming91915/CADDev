using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins.HaveATry;
using eZstd.Miscellaneous;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(Item))]

namespace eZcad.Addins.HaveATry
{
    internal enum Subject
    {
        /// <summary> 设计通知单 </summary>
        Inform,

        /// <summary> 变更申请单 </summary>
        Request
    }

    internal class Item
    {
        #region ---   Fields

        public string PaperId { get; set; }
        public Subject Subject { get; set; }
        public bool LeftSide { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public string Category { get; set; }

        #endregion

        public const double BarHeight = 10;
        public const double TextHeight = 5;

        public ObjectIdCollection Draw(Transaction trans, BlockTableRecord btr, string[] categories)
        {
            var ids = new ObjectIdCollection();
            double top, bottom, left, right;
            GetRec(categories, out top, out bottom, out left, out right);
            var middleH = (left + right) / 2;
            var middleV = (top + bottom) / 2;
            //
     
            // 创建一条多段线
            var acPoly = new Polyline();
            acPoly.SetDatabaseDefaults();
            acPoly.AddVertexAt(0, new Point2d(left, bottom), 0, 0, 0);
            acPoly.AddVertexAt(0, new Point2d(left, top), 0, 0, 0);
            acPoly.AddVertexAt(0, new Point2d(right, top), 0, 0, 0);
            acPoly.AddVertexAt(0, new Point2d(right, bottom), 0, 0, 0);
            acPoly.Closed = true;
            // 添加新对象到块表记录和事务中
            btr.AppendEntity(acPoly);
            trans.AddNewlyCreatedDBObject(acPoly, true);
            ids.Add(acPoly.Id);

            // 对设计变通知项进行填充
            if (Subject == Subject.Inform)
            {
                // 创建Hatch对象并添加到块表记录
                var acHatch = new Hatch();
                btr.AppendEntity(acHatch);
                trans.AddNewlyCreatedDBObject(acHatch, true);
                // 设置填充对象的属性
                // 关联属性必须在将填充对象添加到块表记录之后，执行AppendLoop之前设置

                acHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                acHatch.Associative = true;
                acHatch.AppendLoop(HatchLoopTypes.Outermost, ids);
                acHatch.EvaluateHatch(true);
                //
                ids.Add(acHatch.Id);
            }

            // 添加文字与标注
            var txtStart = new DBText
            {
                TextString = Start.ToString(),
                Height = TextHeight,

                HorizontalMode = TextHorizontalMode.TextLeft,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                AlignmentPoint = new Point3d(left, middleV, 0),
            };
            btr.AppendEntity(txtStart);
            trans.AddNewlyCreatedDBObject(txtStart, true);
            ids.Add(txtStart.Id);

            var txtEnd = new DBText
            {
                TextString = End.ToString(),
                Height = TextHeight,

                HorizontalMode = TextHorizontalMode.TextRight,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                AlignmentPoint = new Point3d(right, middleV, 0),
            };
            btr.AppendEntity(txtEnd);
            trans.AddNewlyCreatedDBObject(txtEnd, true);
            ids.Add(txtEnd.Id);

            var txtPaper = new DBText
            {
                TextString = PaperId,
                Height = TextHeight,

                HorizontalMode = TextHorizontalMode.TextCenter,
                VerticalMode = TextVerticalMode.TextVerticalMid,
                AlignmentPoint = new Point3d(middleH, middleV, 0), // 如果要设置对齐，则一定要先设置 HorizontalMode 与 VerticalMode，最后设置 AlignmentPoint。设置了对齐后，Position属性被屏蔽，不论设置为什么值都不起作用。
               
            };
            btr.AppendEntity(txtPaper);
            trans.AddNewlyCreatedDBObject(txtPaper, true);
            ids.Add(txtPaper.Id);

            return ids;
        }

        /// <summary>
        ///     得到每一个矩形条的位置
        /// </summary>
        /// <returns></returns>
        private void GetRec(string[] categories, out double top, out double bottom,
            out double left, out double right)
        {
            var middleV = GetMiddleV(categories, Category, LeftSide);
            top = middleV + 0.5 * BarHeight;
            bottom = middleV - 0.5 * BarHeight;

            //
            left = Start;
            right = End;
        }

        public static double GetMiddleV(string[] categories, string category, bool isLeftSide)
        {

            var ind = categories.IndexOf(category);
            var v = (ind + 1.5) * BarHeight;
            if (!isLeftSide) // 道路右侧 绘制在X轴下方
            {
                v = -v;
            }
            
            return v ; 
        }
    }
}