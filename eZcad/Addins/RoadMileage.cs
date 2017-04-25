using System;
using AutoCADDev.Utility;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AutoCADDev.Addins
{
    /// <summary> 获取指定的曲线上某点所对应的里程 </summary>
    public class RoadMileage
    {
        /// <summary> 标志点在曲线中所对应的 parameter 值 </summary>
        private double _basePara;

        /// <summary> 标志点在曲线中所对应的里程桩号，比如 K5+560.123，即为5560.123 </summary>
        private double _baseMile;

        DocumentModifier _docMdf;

        /// <summary> 构造函数 </summary>
        /// <param name="basePara">标志点在曲线中所对应的 parameter 值</param>
        /// <param name="baseMile">标志点在曲线中所对应的里程桩号，比如 K5+560.123，即为5560.123</param>
        public RoadMileage(double basePara, double baseMile)
        {
            _basePara = basePara;
            _baseMile = baseMile;
        }

        #region ---   在界面中获取对象

        #endregion

        /// <summary> 为指定的曲线添加垂线 </summary>
        /// <param name="docMdf"></param>
        /// <param name="impliedSelection"> 用户在执行方法之前已经选择好的对象。</param>
        [CommandMethod("eZcad", "GetRoadMileage", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void GetRoadMileage(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            Curve baseCurve = null;
            if (impliedSelection != null)
            {
                foreach (var id in impliedSelection.GetObjectIds())
                {
                    baseCurve = docMdf.acTransaction.GetObject(id, OpenMode.ForRead) as Curve;
                    if (baseCurve != null)
                    {
                        break;
                    }
                }
            }
            if (baseCurve == null)
            {
                baseCurve = PickOneCurve(docMdf);
            }
            if (baseCurve == null) return;

            // 在界面中选择一个点，并生成对应位置处曲线的垂线
            Point3d? pt = PickPoint(docMdf.acEditor, baseCurve);
            while (pt != null)
            {
                //
                WriteMileage(docMdf.acEditor, baseCurve, pt.Value);
                //
                pt = PickPoint(docMdf.acEditor, baseCurve);
            }
        }

        private Curve PickOneCurve(DocumentModifier docMdf)
        {
            // 点选
            var peO = new PromptEntityOptions("\n 选择一条曲线 ");
            peO.SetRejectMessage("\n 请选择一个曲线对象\n");
            peO.AddAllowedClass(typeof(Curve), exactMatch: false);

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetEntity(peO);

            Curve curve = null;
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                curve = docMdf.acTransaction.GetObject(res.ObjectId, OpenMode.ForRead) as Curve;
            }
            return curve;
        }

        /// <summary> 在曲线附近选一个点，并绘制其垂线 </summary>
        /// <param name="editor"></param>
        /// <param name="baseCurve"></param>
        /// <returns></returns>
        private Point3d? PickPoint(Editor editor, Curve baseCurve)
        {
            bool continuPickPoint = false;
            Point3d? pt;
            do
            {
                pt = GetOnePoint(editor, baseCurve, out continuPickPoint);
            } while (continuPickPoint);

            return pt;
        }

        /// <summary> 在界面中选择一个点，用来在其附近创建曲线的垂线 </summary>
        /// <param name="onLeft"> null 表示画在切点的左右两侧 </param>
        /// <param name="length"></param>
        /// <param name="continuPickPoint">没有成功选择一个点，只是输入了关键词，此时需要继续进行选择</param>
        /// <returns>如果没有选择到有效的点，则返回null</returns>
        private Point3d? GetOnePoint(Editor editor, Curve c, out bool continuPickPoint)
        {
            var ppo = new PromptPointOptions(messageAndKeywords: "\n选择曲线附近的一个点[参考里程(R)]:",
                globalKeywords: "参考里程")
            {
                //  ppo.Keywords.Default = "长度";
                AllowArbitraryInput = false, // 用户可以输入非关键字的字符，其可以
                AllowNone = true,
            };

            // 在界面中选择一个角度
            var res = editor.GetPoint(ppo);

            if (res.Status == PromptStatus.Keyword) // 用户输入关键词或者其他任意字符
            {
                continuPickPoint = true;
                switch (res.StringResult)
                {
                    case "参考里程":

                        // 设置一个参考点与对应的里程
                        PickBaseMileage(c);
                        break;
                }
            }
            else if ((res.Status == PromptStatus.None) || (res.Status == PromptStatus.Cancel)) // 用户按下 Enter 或 ESC
            {
                continuPickPoint = false;
                return null;
            }
            if (res.Status == PromptStatus.OK) // 用户选择到一个点
            {
                continuPickPoint = false;
                return res.Value;
            }
            continuPickPoint = true;
            return null;
        }

        // 设置一个基准里程点
        private void PickBaseMileage(Curve c)
        {
            Point3d? basePt = null;
            // 选择一个点作为基准里程点
            var op = new PromptPointOptions("\n选择一个点作为基准里程点")
            {
                AllowNone = true,
                AllowArbitraryInput = false
            };

            var res = _docMdf.acEditor.GetPoint(op);
            if (res.Status == PromptStatus.Keyword)
            {
                basePt = Utils.GetPointFromString(res.StringResult);
            }
            else if (res.Status == PromptStatus.OK)
            {
                basePt = res.Value;
            }
            // 
            if (basePt != null) // 成功选择到了里程基准点
            {
                basePt = c.GetClosestPointTo(basePt.Value, extend: false);
                _basePara = c.GetParameterAtPoint(basePt.Value);

                // 指定基准点对应的里程
                var op1 = new PromptDoubleOptions("\n指定基准点所对应的里程值")
                {
                    AllowNone = true,
                    AllowNegative = false,
                    AllowZero = true,
                };

                var res1 = _docMdf.acEditor.GetDouble(op1);
                if (res1.Status == PromptStatus.OK)
                {
                    _baseMile = res1.Value;
                }
            }
        }

        private void WriteMileage(Editor editor, Curve c, Point3d pt)
        {
            var closestPt = c.GetClosestPointTo(pt, extend: false);
            var newPara = c.GetParameterAtPoint(closestPt);
            var oldDis = c.GetDistanceAtParameter(_basePara);
            var newDis = c.GetDistanceAtParameter(newPara);

            // 计算新增加的里程
            var newMileage = _baseMile + newDis - oldDis;
            var kComp = (int)Math.Floor(newMileage / 1000); // 千米的分量
            var mComp = newMileage - kComp * 1000; // 米的分量

            string msg =
                $"坐标{closestPt.ToString()};\t距起点距离：{newDis};\t距参考点距离：{newDis - oldDis};\t参考里程：K{kComp}+{mComp.ToString("0.000")}";
            editor.WriteMessage(msg);
        }
    }
}