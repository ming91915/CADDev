using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

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
    }
}
