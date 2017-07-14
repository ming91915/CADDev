using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
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

        // 块定义，插入模型空间  
        public static ObjectId BlkInDb(BlockTableRecord block, Point3d pt, Database db)
        {
            BlockReference blkRef = null;

            ObjectId id = new ObjectId();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开模型空间  
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modeSpce = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                //  

                // 新建一个块引用  
                blkRef = new BlockReference(pt, block.ObjectId);
                id = modeSpce.AppendEntity(blkRef);
                tr.AddNewlyCreatedDBObject(blkRef, true);

                // 遍历块记录中的所有实体，加入块引用中  
                foreach (ObjectId idTemp in block)
                {
                    // 判断该实体是否是块定义 ，也可以以块定义方式打开，但是会产生事物嵌套  
                    if (idTemp.ObjectClass.Equals(Autodesk.AutoCAD.Runtime.RXObject.GetClass(typeof(AttributeDefinition))))
                    {
                        AttributeDefinition adDef = tr.GetObject(idTemp, OpenMode.ForRead) as AttributeDefinition;
                        if (adDef != null)
                        {
                            AttributeReference ar = new AttributeReference(adDef.Position, adDef.TextString, adDef.Tag, new ObjectId());
                            blkRef.AttributeCollection.AppendAttribute(ar);
                        }
                    }
                }

                tr.Commit();
            }

            return id;
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

        #region ---   字典操作

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

    }
}
