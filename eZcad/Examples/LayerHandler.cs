using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Examples
{
    internal class LayerHandler
    {
        // 根据图层名获取图层  
        public static LayerTableRecord GetLayer(string name, Database db)
        {
            LayerTableRecord layer = null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTableRecord layerTemp = new LayerTableRecord();
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt != null)
                {
                    if (lt.Has(name))
                    {
                        layerTemp = tr.GetObject(lt[name], OpenMode.ForRead) as LayerTableRecord;
                        if (null != layerTemp && !layerTemp.IsErased)
                        {
                            layer = layerTemp;
                        }
                    }
                }
                tr.Commit();
            }
            return layer;
        }

        // 新建一个图层 (置为当前图层)
        public static ObjectId CreatLayer(string layername, Database db, bool setAsCurrent = false)
        {
            ObjectId id = ObjectId.Null;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable table = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                if (null != table)
                {
                    if (!table.Has(layername)) // 存在就不再创建  
                    {
                        LayerTableRecord layer = new LayerTableRecord();
                        layer.Name = layername;

                        id = table.Add(layer);
                        tr.AddNewlyCreatedDBObject(layer, true);

                        if (setAsCurrent)
                        {
                            // 设置为当前图层
                            db.Clayer = layer.ObjectId;
                        }
                    }
                }

                tr.Commit();
            }

            return id;
        }

        // 删除图层  
        public static void DeleteLayer(string delLayer, Database db)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable table = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                if (null != table)
                {
                    LayerTableRecord curLayer = tr.GetObject(db.Clayer, OpenMode.ForRead) as LayerTableRecord;
                    if (curLayer.Name.ToLower() != delLayer.ToLower()) // 当前图层不可删除  
                    {
                        if (table.Has(delLayer)) // 不存在就不用删除
                        {
                            var layer = tr.GetObject(table[delLayer], OpenMode.ForWrite) as LayerTableRecord;
                            if (layer != null && !layer.IsErased)
                            {
                                ObjectIdCollection idArr = new ObjectIdCollection();
                                idArr.Add(layer.ObjectId);
                                db.Purge(idArr); // 删除图层id  

                                if (idArr.Count != 0) // 判断是否删除失败  
                                {
                                    layer.Erase();
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
