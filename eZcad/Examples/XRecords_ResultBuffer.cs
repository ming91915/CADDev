using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Examples;

[assembly: CommandClass(typeof (XRecords_ResultBuffer))]

namespace eZcad.Examples
{
    public class XRecords_ResultBuffer
    {
        /// <summary> 向 Entity 中添加数据 </summary>
        public static void AddDataToExtensionDictionary()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // pick entity to add data to! 
            PromptEntityResult getEntityResult = ed.GetEntity("Pick an entity to add an Extension Dictionary to : ");
            // if all was ok 
            if ((getEntityResult.Status == PromptStatus.OK))
            {
                // now start a transaction 
                Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
                try
                {
                    // Start of Lab5 
                    // Here we will add XData to a selected entity. 

                    // 1. Declare an Entity variable named ent. Instantiate it using GetOBject of the Transaction created above. 
                    Entity ent = (Entity) trans.GetObject(getEntityResult.ObjectId, OpenMode.ForRead);

                    // 2. Use an "if" statement and test the IsNull property of the  ExtensionDictionary of the ent.
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        // 3. Upgrade the open of the entity. Because it does not have an extenstion dictionary and we want to add it the ent needs to be open for write. 
                        ent.UpgradeOpen();

                        // 4. Create the ExtensionDictionary by calling  CreateExtensionDictionary of the entity. 
                        ent.CreateExtensionDictionary();
                    }

                    // 5. Declare a variable as DBDictionary. Instantiate it by using the GetObject method of the Transaction created above. 
                    DBDictionary extensionDict =
                        (DBDictionary) trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    // 6. Check to see if the entry we are going to add to the dictionary is already there. Use the Contains property of the dictionary in an "if else statement.
                    if (extensionDict.Contains("MyData"))
                    {
                        // 7. Declare an ObjectId variable named entryId and instantiate it using the GetAt method of the ExtenstionDictionary from step 5. Use "Mydata" for the entryName
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        // 8. If this line gets hit then data is already added.
                        ed.WriteMessage("\nThis entity already has data...");

                        // 9. Now extract the Xrecord. Declare an Xrecord variable. 
                        Xrecord myXrecord = default(Xrecord);

                        // 10. Instantiate the Xrecord variable using the  GetObject method of the Transaction created above. 
                        myXrecord = (Xrecord) trans.GetObject(entryId, OpenMode.ForRead);

                        // 11. Here print out the values in the Xrecord to the command line. 
                        ResultBuffer resBuff = myXrecord.Data;

                        foreach (TypedValue value in resBuff)
                        {
                            // 12. Use the WriteMessage method of the Editor created above. (ed). 
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());
                        }
                    }
                    else
                    {
                        // 13. If the code gets to here then the data entry does not exist, upgrade the ExtensionDictionary created in step 5 to write by calling the UpgradeOpen() method 
                        extensionDict.UpgradeOpen();

                        // 14. Create a new XRecord. Declare an Xrecord variable as a New Xrecord 
                        Xrecord myXrecord = new Xrecord();

                        // 15. Create the resbuf list. Declare a ResultBuffer variable. Instantiate it by creating a New ResultBuffer.
                        ResultBuffer data = new ResultBuffer(new TypedValue((int) DxfCode.Int16, 1),
                            new TypedValue((int) DxfCode.Text, "MyStockData"),
                            new TypedValue((int) DxfCode.Real, 51.9),
                            new TypedValue((int) DxfCode.Real, 100.0),
                            new TypedValue((int) DxfCode.Real, 320.6));

                        // 16. Add the ResultBuffer to the Xrecord using the Data 
                        // property of the Xrecord. (make it equal the ResultBuffer 
                        // from step 15) 
                        myXrecord.Data = data;

                        // 17. Create the entry in the ExtensionDictionary. Use the SetAt  method of the ExtensionDictionary from step 5. 
                        extensionDict.SetAt(searchKey: "MyData", newValue: myXrecord);

                        // 18. Tell the transaction about the newly created Xrecord using the AddNewlyCreatedDBObject of the Transaction (trans) 
                        trans.AddNewlyCreatedDBObject(myXrecord, true);
                    }
                    // all ok, commit it 
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    // whatever happens we must dispose the transaction 
                    trans.Dispose();
                }
            }
        }

        /// <summary> 向数据库中添加数据 </summary>
        public static void AddDataToNOD()
        {
            // get the editor object 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // pick entity to add data to! 
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            try
            {
                // 26. Here we will add our data to the Named Objects Dictionary.(NOD). Declare a variable as a DBDictionary. (name it nod).
                DBDictionary nod =
                    (DBDictionary) trans.GetObject(ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

                // 27. Check to see if the entry we are going to add to the NOD is  already there. 
                if (nod.Contains("MyData"))
                {
                    // 28. Declare an ObjectId variable named entryId. Instantiate it by making it equal to the return of the GetAt method of the NOD (DBDictionary) 
                    ObjectId entryId = nod.GetAt("MyData");

                    // 29. If we are here, then the Name Object Dictionary already has our data.
                    ed.WriteMessage("\n" + "This entity already has data...");

                    // 30. Get the the Xrecord from the NOD. Declare a variable as a new Xrecord 
                    Xrecord myXrecord = null;

                    // 31. USe the Transaction (trans) and use the GetObject method to get the the Xrecord from the NOD.
                    myXrecord = (Xrecord) trans.GetObject(entryId, OpenMode.ForRead);

                    // 32. Print out the values of the Xrecord to the command line.
                    foreach (TypedValue value in myXrecord.Data)
                    {
                        // 33. Use the WriteMessage method of the editor. 
                        ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());
                    }
                }
                else
                {
                    // 34. Our data is not in the Named Objects Dictionary so need to add it 
                    // upgrade the status of the NOD variable from step 26 to write status 
                    nod.UpgradeOpen();

                    // 35. Declare a varable as a new Xrecord. 
                    Xrecord myXrecord = new Xrecord();

                    // 36. Create the resbuf list. Declare a ResultBuffer variable. Instantiate it  by creating a New ResultBuffer.
                    ResultBuffer data = new ResultBuffer(new TypedValue((int) DxfCode.Int16, 1),
                        new TypedValue((int) DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int) DxfCode.Real, 51.9),
                        new TypedValue((int) DxfCode.Real, 100.0),
                        new TypedValue((int) DxfCode.Real, 320.6));

                    // 37. Add the ResultBuffer to the Xrecord using the Data property of the Xrecord.
                    myXrecord.Data = data;

                    // 38. Create the entry in the ExtensionDictionary. Use the SetAt method of the Named Objects Dictionary from step 26.
                    nod.SetAt(searchKey: "MyData", newValue: myXrecord);

                    // 39. Tell the transaction about the newly created Xrecord. 
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }

                // End of LAB 5

                // all ok, commit it 
                trans.Commit();
            }
            catch (Exception ex)
            {
                ed.WriteMessage("a problem occurred because " + ex.Message);
            }
            finally
            {
                // whatever happens we must dispose the transaction 
                trans.Dispose();
            }
        }
        
        public static void WriteXData(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var apptable = docMdf.acDataBase.RegAppTableId.GetObject(OpenMode.ForWrite) as RegAppTable;

            // RegAppTableRecord 的创建
            var strApp1 = "app1";
            if (!apptable.Has(strApp1))
            {
                var app1 = new RegAppTableRecord() { Name = strApp1, };
                apptable.Add(app1);
                docMdf.acTransaction.AddNewlyCreatedDBObject(app1, true);
            }
            var strApp2 = "app2";
            if (!apptable.Has(strApp2))
            {
                var app1 = new RegAppTableRecord() { Name = strApp2, };
                apptable.Add(app1);
                docMdf.acTransaction.AddNewlyCreatedDBObject(app1, true);
            }

            // 在界面中选择实体，用来写入XData
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var id = ed.GetEntity("\n选择一个要写入XData的实体：").ObjectId;
            var obj = id.GetObject(OpenMode.ForRead);
            if (obj != null)
            {
                obj.UpgradeOpen();
                ResultBuffer data2 = new ResultBuffer(
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, strApp1),
                        new TypedValue((int)DxfCode.ExtendedDataAsciiString, "任意字符"),
                        new TypedValue((int)DxfCode.ExtendedDataDist, 1.3),
                        new TypedValue((int)DxfCode.ExtendedDataReal, 2),

                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, strApp2),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, 88),
                        new TypedValue((int)DxfCode.ExtendedDataReal, 4.5),
                        new TypedValue((int)DxfCode.ExtendedDataScale, 6)
                        );
                obj.XData = data2;

                // 提取不同 RegAppName 的数据
                var res1 = obj.GetXDataForApplication(strApp1); // 返回的集合中包含 strApp1 在内的4个元素。
                var res2 = obj.GetXDataForApplication(strApp2); // 返回的集合中包含 strApp2 在内的4个元素。
                var res3 = obj.GetXDataForApplication("app3"); // 返回 null
            }
        }
        
        public static ResultBuffer ClearXData()
        {
            ResultBuffer buff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, "某AppName"));
            // 此时，Entity中的XData集合里，对应AppName下的所有数据，连同AppName这一项本身，都在实体中删除了。
            // 但是此AppName在 RegAppTable 中对应的 RegAppTableRecord 定义还是存在的。
            return buff;
        }
    }
}