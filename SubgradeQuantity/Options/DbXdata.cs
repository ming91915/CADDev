using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.Options
{
    public class DbXdata
    {
        #region ---   DatabaseXdataType
        [Flags]
        public enum DatabaseXdataType
        {
            None = 0,
            General = 1,
            LayerNames = 2,
            RangeBlocks = 4,
            SoilRockRange = 8,
            AllSortedStations = 16,
            //
        }

        public static DatabaseXdataType GetAllXdataTypes()
        {
            var res = DatabaseXdataType.None;
            var values = Enum.GetValues(typeof(DatabaseXdataType));
            foreach (DatabaseXdataType v in values)
            {
                res = res | v;
            }
            return res;
        }

        #endregion

        private const string DatabaseDictKey = "MSDI_SubgradeQuantity";

        #region ---   数据的提取与保存

        /// <summary> 刷新所有的全局选项到内存中 </summary> 
        /// <param name="docMdf"></param>
        public static void LoadAllOptionsFromDbToMemory(DocumentModifier docMdf)
        {
            var allXdataTypes = DbXdata.GetAllXdataTypes();
            DbXdata.RefreshOptionsFromDb(docMdf, allXdataTypes);
        }

        /// <summary> 将文档数据库中的数据刷新到静态的Option类中 </summary>
        /// <param name="xdataType"> 要刷新的数据类型 ，可以将多种类型进行叠加 </param>
        public static void RefreshOptionsFromDb(DocumentModifier docMdf, DatabaseXdataType xdataType)
        {
            var baseDict = GetBaseDict(docMdf);
            if ((xdataType & DatabaseXdataType.LayerNames) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.LayerNames);
                var rec = SymbolTableUtils.GetDictionaryValue<Xrecord>(baseDict, dictKey);
                if (rec != null)
                {
                    Options_LayerNames.FromXrecord(rec);
                }
            }
            if ((xdataType & DatabaseXdataType.RangeBlocks) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.RangeBlocks);
                var rec = SymbolTableUtils.GetDictionaryValue<Xrecord>(baseDict, dictKey);
                Options_Collections.FromXrecord_Blocks(rec);
            }
            if ((xdataType & DatabaseXdataType.SoilRockRange) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.SoilRockRange);
                var rec = SymbolTableUtils.GetDictionaryValue<Xrecord>(baseDict, dictKey);
                Options_Collections.FromXrecord_SoilRockRanges(rec);

            }
            if ((xdataType & DatabaseXdataType.AllSortedStations) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.AllSortedStations);
                var rec = SymbolTableUtils.GetDictionaryValue<Xrecord>(baseDict, dictKey);
                Options_Collections.FromXrecord_SortedStations(rec);
            }
        }

        /// <summary> 将静态的Option类中的数据保存到文档数据库中 </summary>
        public static void FlushXData(DocumentModifier docMdf, DatabaseXdataType xdataType)
        {
            var baseDict = GetBaseDict(docMdf);
            baseDict.UpgradeOpen();
            if ((xdataType & DatabaseXdataType.LayerNames) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.LayerNames);
                var xBuff = Options_LayerNames.ToResultBuffer();
                SymbolTableUtils.ModifyDictXrecord(docMdf.acTransaction, baseDict, dictKey, xBuff);
                //baseDict.SetAt(dictKey, xBuff);
                //docMdf.acTransaction.AddNewlyCreatedDBObject(xBuff, true);
            }
            if ((xdataType & DatabaseXdataType.RangeBlocks) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.RangeBlocks);
                var xBuff = Options_Collections.ToResultBuffer_Blocks();
                SymbolTableUtils.ModifyDictXrecord(docMdf.acTransaction, baseDict, dictKey, xBuff);
                //baseDict.SetAt(dictKey, xrec);
                //docMdf.acTransaction.AddNewlyCreatedDBObject(xrec, true);
            }
            if ((xdataType & DatabaseXdataType.SoilRockRange) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.SoilRockRange);
                var xBuff = Options_Collections.ToResultBuffer_SoilRockRanges();
                SymbolTableUtils.ModifyDictXrecord(docMdf.acTransaction, baseDict, dictKey, xBuff);
                //baseDict.SetAt(dictKey, xrec);
                //docMdf.acTransaction.AddNewlyCreatedDBObject(xrec, true);
            }
            if ((xdataType & DatabaseXdataType.AllSortedStations) > 0)
            {
                var dictKey = Enum.GetName(typeof(DatabaseXdataType), DatabaseXdataType.AllSortedStations);
                var xBuff = Options_Collections.ToResultBuffer_SortedStations();
                SymbolTableUtils.ModifyDictXrecord(docMdf.acTransaction, baseDict, dictKey, xBuff);
                //baseDict.SetAt(dictKey, xrec);
                //docMdf.acTransaction.AddNewlyCreatedDBObject(xrec, true);
            }

            baseDict.DowngradeOpen();
        }

        public static void ClearXData(DocumentModifier docMdf, DatabaseXdataType xdataType)
        {
        }

        #endregion

        private static DBDictionary GetBaseDict(DocumentModifier docMdf)
        {
            var nod = docMdf.acDataBase.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
            var baseDict = SymbolTableUtils.GetDictionaryValue<DBDictionary>(nod, DatabaseDictKey);
            if (baseDict == null)
            {
                // DBDictionary 中插入 DBDictionary
                nod.UpgradeOpen();
                baseDict = new DBDictionary();

                nod.SetAt(DatabaseDictKey, baseDict);
                // 如果在将字典对象添加到其容器字典 extensionDict 之前，就用 AddNewlyCreatedDBObject ，则会出现报错：eNotInDatabase
                docMdf.acTransaction.AddNewlyCreatedDBObject(baseDict, true);
                nod.DowngradeOpen();
            }
            return baseDict;
        }
    }
}
