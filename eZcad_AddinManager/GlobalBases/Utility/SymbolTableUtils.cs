using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.Win32;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace eZcad.Utility
{
    /// <summary> 字典 Dictionary 与 符号表 SymbolTable 操作 </summary>
    /// <remarks></remarks>
    public static class SymbolTableUtils
    {
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
        /// <param name="trans">请确保事务已经打开</param>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static LayerTableRecord GetOrCreateLayer(Transaction trans, Database db, string layerName)
        {
            LayerTable layers = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
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
                trans.AddNewlyCreatedDBObject(ltr, true);
                return ltr;
            }
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
        
    }
}