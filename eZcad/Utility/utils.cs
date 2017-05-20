using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
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
                    if (idTemp.ObjectClass.Equals(RXObject.GetClass(typeof(AttributeDefinition))))
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
    }
}
