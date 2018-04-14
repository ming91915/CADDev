using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AddinManager;
using eZcad.SubgradeQuantity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace eZcad.SQcmds
{
    [EcDescription("AddinManager 调试代码模板")]
    public class DebugTemplate : ICADExCommand
    {
        private DocumentModifier _docMdf;

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            TestDictOnAbortGetAtDict();
            return ExternalCommandResult.Succeeded;

            var s = new DebugTemplate();
            return SQAddinManagerDebuger.DebugInAddinManager(s.DoSomething,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        // 开始具体的调试操作
        private ExternalCmdResult DoSomething(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            return ExternalCmdResult.Cancel;
        }

        /// <summary> 案例测试： 修改 DBDictionary 中的键值后，撤消时键被删除 </summary>
        /// <remarks>当执行“Dictionary 中的键 K 有值v1 -> 打开事务 -> 通过 Dictionary.SetAt(K, v2); 修改 K 的值为 v2  -> 提交事务  -> Ctrl+Z 撤消 ”之后， Dictionary 中的键 K 就不存在了，而不是 K 的值撤消为 v1。</remarks>
        private static void TestDictOnAbortGetAtDict1()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var acDataBase = doc.Database;
            var ed = doc.Editor;

            //
            var acLock = doc.LockDocument();
            var trans = acDataBase.TransactionManager.StartTransaction();
            try
            {
                var res = ed.GetEntity("在界面中选择一个对象");
                if (res.Status == PromptStatus.OK)
                {
                    var ent = res.ObjectId.GetObject(OpenMode.ForWrite);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        MessageBox.Show($"初始化字典");
                        ent.CreateExtensionDictionary();
                    }

                    // 总的字典
                    var extDict = ent.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
                    //
                    const string keyName = "测试Key";

                    // SetAt 对已经存在的键K的值V1进行修改（修改为V2），可以正常执行，但是修改完成后再执行撤销操作，则整个键值对K-V2会被删除，而不是返回为原键值对K-V1。
                    // SetAt用在第一次赋值，而GetAt用在取值并修改。
                    // 所以，如果要修改键K的值，可以先将K键值对Remove掉（此时对应的V1也会在事务中删除），然后再通过SetAt重新添加同名键的键值对，
                    // 也可以先通过GetAt以ForWrite的方式提取出V1（DBObject对象），然后对V1进行修改，最后执行V1.DownGradeOpen()。

                    // 若V1为XRecord对象，则可以直接修改其Data属性；
                    // 而若V1本身为DBDictionary对象，则要按同样的思路进行考虑：1.如果新键K2不存在，则直接SetAt添加，2.如果新键K3已经存在，则可能先将其Remove掉，再SetAt添加；也可以先提取出原有键K3的值V3，再对V3的属性进行修改。
                    if (extDict.Contains(keyName))
                    {
                        var dt = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                        foreach (var e in dt)
                        {
                            dt.Remove(e.Key);
                        }
                        dt.DowngradeOpen();
                    }
                    // 填充值
                    int baseid = DateTime.Now.Second;
                    if (extDict.Contains(keyName))
                    {
                        var subdt = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                        CreateSubDict(trans, subdt, baseid);
                        subdt.DowngradeOpen();

                    }
                    else
                    {
                        var subDict = new DBDictionary();
                        var id = extDict.SetAt(keyName, subDict);
                        trans.AddNewlyCreatedDBObject(subDict, true);
                        MessageBox.Show(id.GetObject(OpenMode.ForRead).GetType().FullName + "\r\n" + (id == subDict.Id).ToString());

                        CreateSubDict(trans, subDict, baseid);
                    }



                    //
                    extDict.DowngradeOpen();
                    ent.DowngradeOpen();

                    var dt1 = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                    var s = new StringBuilder();
                    foreach (var de in dt1)
                    {
                        s.AppendLine(de.Key);
                    }

                    MessageBox.Show($"键“{keyName}”的值为：“{s.ToString()}个”");
                }
                //
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Commit();
                MessageBox.Show("出错，但还是提交事务\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                trans.Dispose();
                acLock.Dispose();
            }
        }


        /// <summary> 案例测试： 修改 DBDictionary 中的键值后，撤消时键被删除 </summary>
        /// <remarks>当执行“Dictionary 中的键 K 有值v1 -> 打开事务 -> 通过 Dictionary.SetAt(K, v2); 修改 K 的值为 v2  -> 提交事务  -> Ctrl+Z 撤消 ”之后， Dictionary 中的键 K 就不存在了，而不是 K 的值撤消为 v1。</remarks>
        private static void TestDictOnAbortGetAtDict()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var acDataBase = doc.Database;
            var ed = doc.Editor;

            //
            var acLock = doc.LockDocument();
            var trans = acDataBase.TransactionManager.StartTransaction();
            try
            {
                var res = ed.GetEntity("在界面中选择一个对象");
                if (res.Status == PromptStatus.OK)
                {
                    var ent = res.ObjectId.GetObject(OpenMode.ForWrite);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        MessageBox.Show($"初始化字典");
                        ent.CreateExtensionDictionary();
                    }

                    // 总的字典
                    var extDict = ent.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
                    //
                    const string keyName = "测试Key";

                    // SetAt 对已经存在的键K的值V1进行修改（修改为V2），可以正常执行，但是修改完成后再执行撤销操作，则整个键值对K-V2会被删除，而不是返回为原键值对K-V1。
                    // SetAt用在第一次赋值，而GetAt用在取值并修改。
                    // 所以，如果要修改键K的值，可以先将K键值对Remove掉（此时对应的V1也会在事务中删除），然后再通过SetAt重新添加同名键的键值对，
                    // 也可以先通过GetAt以ForWrite的方式提取出V1（DBObject对象），然后对V1进行修改，最后执行V1.DownGradeOpen()。

                    // 若V1为XRecord对象，则可以直接修改其Data属性；
                    // 而若V1本身为DBDictionary对象，则要按同样的思路进行考虑：1.如果新键K2不存在，则直接SetAt添加，2.如果新键K3已经存在，则可能先将其Remove掉，再SetAt添加；也可以先提取出原有键K3的值V3，再对V3的属性进行修改。
                    if (extDict.Contains(keyName))
                    {
                        var dt = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                        foreach (var e in dt)
                        {
                            dt.Remove(e.Key);
                        }
                        dt.DowngradeOpen();
                    }
                    // 填充值
                    int baseid = DateTime.Now.Second;
                    if (extDict.Contains(keyName))
                    {
                        var subdt = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                        CreateSubDict(trans, subdt, baseid);
                        subdt.DowngradeOpen();

                    }
                    else
                    {
                        var subDict = new DBDictionary();
                        var id = extDict.SetAt(keyName, subDict);
                        trans.AddNewlyCreatedDBObject(subDict, true);
                        MessageBox.Show(id.GetObject(OpenMode.ForRead).GetType().FullName + "\r\n" + (id == subDict.Id).ToString());

                        CreateSubDict(trans, subDict, baseid);
                    }



                    //
                    extDict.DowngradeOpen();
                    ent.DowngradeOpen();

                    var dt1 = extDict.GetAt(keyName).GetObject(OpenMode.ForWrite) as DBDictionary;
                    var s = new StringBuilder();
                    foreach (var de in dt1)
                    {
                        s.AppendLine(de.Key);
                    }

                    MessageBox.Show($"键“{keyName}”的值为：“{s.ToString()}个”");
                }
                //
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Commit();
                MessageBox.Show("出错，但还是提交事务\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                trans.Dispose();
                acLock.Dispose();
            }
        }


        private static void CreateSubDict(Transaction trans, DBDictionary containerDict, int baseid)
        {
            var iKey = 0;
            for (int i = baseid; i < baseid + 3; i++)
            {
                iKey += 1;
                ResultBuffer generalBuff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataInteger32, i));
                var rec = new Xrecord() { Data = generalBuff };
                containerDict.SetAt(iKey.ToString(), rec);
                trans.AddNewlyCreatedDBObject(rec, true);
            }
        }

        /// <summary> 案例测试： 修改 DBDictionary 中的键值后，撤消时键被删除 </summary>
        /// <remarks>当执行“Dictionary 中的键 K 有值v1 -> 打开事务 -> 通过 Dictionary.SetAt(K, v2); 修改 K 的值为 v2  -> 提交事务  -> Ctrl+Z 撤消 ”之后， Dictionary 中的键 K 就不存在了，而不是 K 的值撤消为 v1。</remarks>
        private static void TestDictOnAbortGetAtXrecord()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var acDataBase = doc.Database;
            var ed = doc.Editor;

            //
            var acLock = doc.LockDocument();
            var trans = acDataBase.TransactionManager.StartTransaction();
            try
            {
                var res = ed.GetEntity("在界面中选择一个对象");
                if (res.Status == PromptStatus.OK)
                {
                    var ent = res.ObjectId.GetObject(OpenMode.ForWrite);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        MessageBox.Show($"初始化字典");
                        ent.CreateExtensionDictionary();
                    }

                    // 总的字典
                    var extDict = ent.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
                    //
                    const string keyName = "测试Key";
                    string value = "初值";
                    if (extDict.Contains(keyName))
                    {
                        var oldvalue =
                            ((Xrecord)extDict.GetAt(keyName).GetObject(OpenMode.ForRead)).Data.AsArray()[0].Value
                                .ToString();
                        value = oldvalue + oldvalue.Length.ToString();
                        // value = oldvalue;
                    }

                    //
                    ResultBuffer generalBuff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataAsciiString, value));
                    if (extDict.Contains(keyName))
                    {
                        var dt = extDict.GetAt(keyName);
                        var recd = dt.GetObject(OpenMode.ForWrite) as Xrecord;
                        recd.Data = generalBuff;
                        recd.DowngradeOpen();
                    }
                    else
                    {
                        var recd = new Xrecord() { Data = generalBuff };
                        extDict.SetAt(keyName, recd);
                        trans.AddNewlyCreatedDBObject(recd, true);
                    }
                    //
                    extDict.DowngradeOpen();
                    ent.DowngradeOpen();
                    MessageBox.Show($"键“{keyName}”的值为：“{value}”");
                }
                //
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Commit();
                MessageBox.Show("出错，但还是提交事务\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                trans.Dispose();
                acLock.Dispose();
            }
        }

        /// <summary> 案例测试： 修改 DBDictionary 中的键值后，撤消时键被删除 </summary>
        /// <remarks>当执行“Dictionary 中的键 K 有值v1 -> 打开事务 -> 通过 Dictionary.SetAt(K, v2); 修改 K 的值为 v2  -> 提交事务  -> Ctrl+Z 撤消 ”之后， Dictionary 中的键 K 就不存在了，而不是 K 的值撤消为 v1。</remarks>
        public static void TestDictOnAbortSetAt()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var acDataBase = doc.Database;
            var ed = doc.Editor;

            //
            var acLock = doc.LockDocument();
            var trans = acDataBase.TransactionManager.StartTransaction();
            try
            {
                var res = ed.GetEntity("在界面中选择一个对象");
                if (res.Status == PromptStatus.OK)
                {
                    var ent = res.ObjectId.GetObject(OpenMode.ForWrite);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        MessageBox.Show($"初始化字典");
                        ent.CreateExtensionDictionary();
                    }

                    // 总的字典
                    var extDict = ent.ExtensionDictionary.GetObject(OpenMode.ForWrite) as DBDictionary;
                    //
                    const string keyName = "测试Key";
                    string value = "初值";
                    if (extDict.Contains(keyName))
                    {
                        var oldvalue =
                            ((Xrecord)extDict.GetAt(keyName).GetObject(OpenMode.ForRead)).Data.AsArray()[0].Value
                                .ToString();
                        value = oldvalue + oldvalue.Length.ToString();
                        // value = oldvalue;
                    }

                    //
                    var generalBuff = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataAsciiString, value));
                    var rec = new Xrecord { Data = generalBuff };

                    // 下面两句应该是出错的关键因素
                    extDict.SetAt(keyName, rec);
                    trans.AddNewlyCreatedDBObject(rec, true);

                    //
                    extDict.DowngradeOpen();
                    ent.DowngradeOpen();
                    MessageBox.Show($"键“{keyName}”的值为：“{value}”");
                }
                //
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Commit();
                MessageBox.Show("出错，但还是提交事务\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                trans.Dispose();
                acLock.Dispose();
            }
        }

    }
}