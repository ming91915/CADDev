using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Utility;
using Microsoft.Win32;

namespace eZcad.Utility
{
    public static class Utils
    {
        /// <summary> 从字符中解析出坐标点，比如“1.2, 2.3, 5” </summary>
        public static Point3d? GetPointFromString(string coord)
        {
            var s = coord.Split(',');
            var xyz = new List<double>();
            double c = 0;
            foreach (var v in s)
            {
                if (double.TryParse(v, out c))
                {
                    xyz.Add(c);
                }
            }
            //
            switch (xyz.Count)
            {
                case 2: return new Point3d(xyz[0], xyz[1], 0);
                case 3: return new Point3d(xyz[0], xyz[1], xyz[2]);
                default:
                    return null;
            }

        }

        /// <summary> 将插件程序注册到注册表中 </summary>
        /// <returns></returns>
        public static bool WriteRegistryKey()
        {
            // AutoCad 2014
            /* 
Windows Registry Editor Version 5.00
[HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\AutoCAD\R19.1\ACAD-D001:804\Applications\AddinManager]
"DESCRIPTION"="AddinManager"
"LOADCTRLS"=dword:00000002
"LOADER"="E:\\zengfy data\\GithubProjects\\CADDev\\eZcad_AddinManager\\bin\\Debug\\eZcad_AddinManager.dll"
"MANAGED"=dword:00000001
             */
            try
            {
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey SOFTWARE = localMachine.OpenSubKey("SOFTWARE", true);
                RegistryKey Autodesk = SOFTWARE.OpenSubKey("Autodesk", true);
                RegistryKey AutoCAD = Autodesk.OpenSubKey("AutoCAD", true);
                RegistryKey R16_2 = AutoCAD.OpenSubKey("R16.2", true);
                RegistryKey ACAD = R16_2.OpenSubKey("ACAD-4001:804", true);
                RegistryKey Applications = ACAD.OpenSubKey("Applications", true);

                RegistryKey MXCAD = Applications.CreateSubKey("MXCAD");
                MXCAD.SetValue("LOADCTRLS", 0x02);
                MXCAD.SetValue("LOADER", "总目录" + @"bin\Debug\MXCAD.dll");
                MXCAD.SetValue("MANAGED", 0x01);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region ---   ObjectId 集合的排序

        /// <summary>
        ///     Sorts an array of ObjectIds based on a string property and order.
        /// </summary>
        /// <param name="ids">The array of IDs to sort.</param>
        /// <param name="propertySelector">A function selecting the string property.</param>
        /// <param name="orderSelector">A function to specify the selection order.</param>
        /// <returns>An ordered enumerable of key-value pairs.</returns>
        /// <remarks>
        /// 举例1：Sort ObjectIds based on the layer name
        ///     var sorted = psr.Value.GetObjectIds().Sort(propertySelector: id => id.Layer, orderSelector: kv => kv.Value);
        /// </remarks>
        public static List<KeyValuePair<ObjectId, string>> Sort(this ObjectId[] ids,
            Func<dynamic, string> propertySelector, Func<KeyValuePair<ObjectId, string>, string> orderSelector)
        {
            return Sort<string>(ids, propertySelector, orderSelector);
        }

        /// <summary>
        ///     Sorts an array of ObjectIds based on a string property and order.
        /// </summary>
        /// <param name="ids">The array of IDs to sort.</param>
        /// <param name="propertySelector">A function selecting the string property.</param>
        /// <param name="orderSelector">A function to specify the selection order.</param>
        /// <returns>An ordered enumerable of key-value pairs.</returns>
        /// <remarks>举例：Sort ObjectIds based on the layer color
        /// var sorted = SortObjectsExtensions.Sort<int>(psr.Value.GetObjectIds(), id => id.LayerId.Color.ColorIndex, kv => -kv.Value);
        /// </remarks>
        public static List<KeyValuePair<ObjectId, T>> Sort<T>(ObjectId[] ids,
            Func<dynamic, T> propertySelector, Func<KeyValuePair<ObjectId, T>, T> orderSelector)
        {
            var map = new Dictionary<ObjectId, T>();

            foreach (dynamic id in ids)
            {
                map.Add(id, propertySelector(id));
            }
            return map.OrderBy(orderSelector).ToList();
        }
        #endregion

        /// <summary> 将表示句柄值的字符转换为句柄 </summary>
        /// <param name="handle">表示句柄的字符，即16进制的数值，比如“409E”。最小的句柄值为1。</param>
        public static Handle ConvertToHandle(string handle)
        {
            return new Handle(Convert.ToInt64(handle, 16));
        }

        #region ---   字典 Dictionary 与 符号表 SymbolTable 操作

        /// <summary> 提取字典中的键所对应的值 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static T GetDictionaryValue<T>(DBDictionary src, string Key) where T : DBObject
        {
            if (src.Contains(Key))
            {
                var id = src.GetAt(Key);
                return id.GetObject(OpenMode.ForRead) as T;
            }
            return null;
        }

        /// <summary> 直接修改字典中 Xrecord 类型数据的属性 </summary>
        /// <param name="dict"> 用户必须自行确保此时 dict 已经打开写入权限 </param>
        /// <param name="buffer"> 要添加或者修改的键中的新值 </param>
        /// <remarks>也可以不通过修改属性的方法，而参考<seealso cref="OverlayDictValue"/>函数直接将同名键删除然后新建的方式来进行同名键值的刷新。</remarks>
        public static void ModifyDictXrecord(Transaction trans, DBDictionary dict, string key, ResultBuffer buffer)
        {
            if (dict.Contains(key))
            {
                var rec = dict.GetAt(key).GetObject(OpenMode.ForWrite) as Xrecord;
                rec.Data = buffer;
                rec.DowngradeOpen();
            }
            else
            {
                var rec = new Xrecord() { Data = buffer };
                dict.SetAt(key, rec);
                trans.AddNewlyCreatedDBObject(rec, true);
            }
        }

        /// <summary> 先移除同名键，然后再通过SetAt添加新的同名键值对 </summary>
        /// <param name="trans"></param>
        /// <param name="dict"> 用户必须自行确保此时 dict 已经打开写入权限 </param>
        /// <param name="key">要进行添加或者修改的键</param>
        /// <param name="value">要添加或者刷新的新值，可以是 <seealso cref="Xrecord"/>、<seealso cref="DBDictionary"/> 等类型 </param>
        /// <remarks>对于<seealso cref="DBDictionary"/>的键值对的修改，一般是要求先将键值对添加到<seealso cref="DBDictionary"/>中，然后再对<param name="value"></param>值的属性进行设置</remarks>
        public static void OverlayDictValue(Transaction trans, DBDictionary dict, string key, DBObject value)
        {
            if (dict.Contains(key))
            {
                // 对于同名键，如果直接SetAt，而不先将其从字典容器中移除的话，可以正常地进行数据的修改，但是在执行Undo操作时，此键值对会直接被删除，而不会还原到其修改前的值。
                dict.Remove(key);
            }
            dict.SetAt(key, value);
            // 如果在将字典对象添加到其容器字典之前，就用 AddNewlyCreatedDBObject ，则会出现报错：eNotInDatabase
            trans.AddNewlyCreatedDBObject(value, true);
        }

        /// <summary> 如果数据库中有此图层，则直接返回，如果没有，则创建对应图层 </summary>
        /// <param name="docMdf"></param>
        /// <param name="layerName"></param>
        public static LayerTableRecord GetOrCreateLayer(DocumentModifier docMdf, string layerName)
        {
            LayerTable layers =
                docMdf.acTransaction.GetObject(docMdf.acDataBase.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (layers.Has(layerName))
            {
                return layers[layerName].GetObject(OpenMode.ForRead) as LayerTableRecord;
            }
            else
            {
                var ltr = new LayerTableRecord();
                ltr.Name = layerName;
                //
                layers.UpgradeOpen();
                layers.Add(ltr);
                layers.DowngradeOpen();
                docMdf.acTransaction.AddNewlyCreatedDBObject(ltr, true);
                return ltr;
            }
        }

        /// <summary> 索引水位线图层 </summary>
        /// <returns></returns>
        public static LayerTableRecord GetOrCreateLayer_WaterLine(DocumentModifier docMdf)
        {
            var l = GetOrCreateLayer(docMdf, ProtectionConstants.LayerName_WaterLine);
            l.UpgradeOpen();
            l.Color = Color.FromColor(System.Drawing.Color.Aqua);
            l.LineWeight = LineWeight.LineWeight070;
            l.DowngradeOpen();
            return l;
        }

        /// <summary>
        /// 从数据库中按名称搜索或者创建出<seealso cref="RegAppTableRecord"/>对象
        /// </summary>
        /// <returns></returns>
        public static ObjectId GetOrCreateAppName(Database db, Transaction startedTrans, string appName)
        {
            var apptable = db.RegAppTableId.GetObject(OpenMode.ForWrite) as RegAppTable;

            // RegAppTableRecord 的创建
            if (!apptable.Has(appName))
            {
                var app1 = new RegAppTableRecord() { Name = appName, };
                apptable.Add(app1);
                startedTrans.AddNewlyCreatedDBObject(app1, true);
                return app1.ObjectId;
            }
            else
            {
                return apptable[appName];
            }
        }

        #endregion

        #region ---   多段线变稀

        /// <summary> 将给定的线性多段线的段数变稀 </summary>
        /// <param name="cs">用来变稀的那条比较密的多段线几何，集合中的曲线必须首尾相连</param>
        /// <param name="segPoints">每隔多少个点取用一个，比如2表示只取源多段线中的第1、3、5、7 ... 个点</param>
        /// <param name="includeAllNonlinear"> true 表示保留所有的曲线段，只将直线段的顶点变疏；false 表示不管是曲线段还是直线段，最终都按顶点坐标转换为直线段 </param>
        /// <returns></returns>
        public static CompositeCurve3d GetThinedPolyline(Curve3d[] cs, int segPoints, bool includeAllNonlinear)
        {
            Point3d startPt = cs[0].StartPoint;
            Point3d endPt;
            var curves = new List<Curve3d>();
            Curve3d c = null;
            var n = cs.Length;
            var id = 1;
            if (includeAllNonlinear)
            {
                // 保留所有的曲线段，只将直线段的顶点变疏
                for (var i = 0; i < n; i++)
                {
                    c = cs[i];
                    if (c is LineSegment3d)
                    {
                        if (id % segPoints == 0)
                        {
                            // 说明到了关键点
                            endPt = c.EndPoint;
                            curves.Add(new LineSegment3d(startPt, endPt));
                            startPt = endPt;
                        }
                        id += 1;
                    }
                    else
                    {

                        if (id > 1)
                        {
                            // 说明中间有直线段
                            endPt = c.StartPoint;
                            curves.Add(new LineSegment3d(startPt, endPt));
                        }
                        else
                        {
                            // 说明前一段也是曲线
                        }
                        // 强制性添加上这一段曲线
                        curves.Add(c);
                        startPt = c.EndPoint;
                        id = 1;
                    }
                }
            }
            else
            {
                // 不管是曲线段还是直线段，最终都按顶点坐标转换为直线段
                for (var i = 0; i < n; i++)
                {
                    c = cs[i];
                    if (id % segPoints == 0)
                    {
                        // 说明到了关键点
                        endPt = c.EndPoint;
                        curves.Add(new LineSegment3d(startPt, endPt));
                        startPt = endPt;
                    }
                    id += 1;
                }
            }

            // 强制补上最后一个可能漏掉的直线段
            if (c != null && startPt != c.EndPoint)
            {
                curves.Add(new LineSegment3d(startPt, c.EndPoint));
            }

            return new CompositeCurve3d(curves.ToArray());
        }

        /// <summary>
        /// 通过限定分段长度来对多段线变稀或者变密（保留首尾两个点）
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="segLength">每一分段的长度</param>
        /// <returns></returns>
        public static CompositeCurve3d GetThinedPolyline(CompositeCurve3d cs, double segLength)
        {
            var startPara = cs.GetParameterOf(cs.StartPoint);
            var endPara = cs.GetParameterOf(cs.EndPoint);
            var startPt = cs.StartPoint;
            var endPt = startPt;
            var para = startPara;
            //
            var segCount = (int)Math.Ceiling((endPara - startPara) / segLength);
            var lines = new Curve3d[segCount];

            // 最后一段的间距不由 segLength 控制
            for (int i = 0; i < segCount - 1; i++)
            {
                para += segLength;
                endPt = cs.EvaluatePoint(para);
                //
                lines[i] = new LineSegment3d(startPt, endPt);
                //
                startPt = endPt;
            }
            // 处理最后一段曲线
            lines[segCount - 1] = new LineSegment3d(startPt, cs.EndPoint);
            //
            return new CompositeCurve3d(lines);
        }

        #endregion

        #region ---   ViewTableRecord

        /// <summary> 在AutoCAD界面中显示出指定的二维矩形范围 </summary>
        public static void ShowExtentsInView(Editor ed, Extents3d ext)
        {
            // 获取当前视图
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y; // 界面中显示的测量高度（不是屏幕分辨率的像素高度）
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.CenterPoint = new Point2d((ext.MinPoint.X + ext.MaxPoint.X) / 2, (ext.MinPoint.Y + ext.MaxPoint.Y) / 2);
                // 最后AutoCAD会对指定的矩形框进行缩放，以确保指定的矩形框完全正中地显示在屏幕中。
                ed.SetCurrentView(view);
            }
        }

        #endregion

        #region ---   XData XRecord 相关操作

        /// <summary> 将一个布尔值转换为对应的 ExtendedData </summary>
        public static TypedValue SetExtendedDataBool(bool value)
        {
            return new TypedValue((int)DxfCode.ExtendedDataInteger16, value);
        }

        /// <summary> 从 ExtendedData 值中提取出对应的 布尔值  </summary>
        public static bool GetExtendedDataBool(TypedValue buff)
        {
            return (Int16)buff.Value == 1;
        }

        #endregion

        /// <summary> 将焦点从操作的<seealso cref="System.Windows.Forms.Form"/>转移到 AutoCAD 主界面窗口。此操作在对 无模态窗口 操作时非常有用。 </summary>
        public static void FocusOnMainUIWindow() { Application.MainWindow.Focus(); }
    }
}
