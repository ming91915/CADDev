using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.AssemblyLoader;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace eZcad.AddinManager
{
    /// <summary> 将指定程序集中的 IExternalCommand 类提取出来 </summary>
    public static class ExCommandFinder
    {
        /// <summary> 将程序集文件加载到内存，并且提取出其中的 CAD 外部命令 </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static List<ICADExCommand> RetriveExternalCommandsFromAssembly(string assemblyPath)
        {
            var assemLoader = new eZAssemblyLoader(assemblyPath);
            Assembly asm = null;
            Type[] classes = null;
            try
            {
                assemLoader.HookAssemblyResolve();
                {
                    // 方法一：zengfy 设计，在测试中通过。其关键在于不能直接将源程序集加载到进程中，
                    // 因为如果这样的话，在Visual Studio中修改此程序集的代码后不能重新编译，或者即使可以重新编译，在AddinManager加载的过程中也不会将其刷新。
                    byte[] buff = File.ReadAllBytes(assemblyPath);
                    //先将插件拷贝到内存缓冲。一般情况下，当加载的文件大小大于2^32 byte (即4.2 GB），就会出现OutOfMemoryException，在实际测试中的极限值为630MB。
                    asm = Assembly.Load(buff); //不能直接通过LoadFrom或者LoadFile，而必须先将插件拷贝到内存，然后再从内存中Load

                    // 方法二：通过LoadFile加载，在测试中发现如果这样做，则在 eZcad_AddinManager 调试过程中，如果在Visual Studio中修改了代码，则不能重新进行编译。
                    // asm = Assembly.LoadFile(assemblyPath);  // LoadFile方法不会加载此程序集引用的其他程序集，也就是不会加载程序的依赖项。
                }

                // 加载程序集中的类型，以寻找对应的 Execute 方法。

                classes = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 有可能会出现找不到文件或程序集 eZstd 或其依赖项的报错。
                classes = ex.Types;
            }
            catch (Exception ex)
            {
                // ignored
                // MessageBox.Show(ex.Message, "加载程序集时出错了", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                assemLoader.UnhookAssemblyResolve();
            }
            DocumentModifier.LineFeedInCommandLine();
            return GetExternalCommandClass(asm, classes);
        }

        private static List<ICADExCommand> GetExternalCommandClass(Assembly ass, Type[] classes)
        {

            if (ass != null && classes != null && classes.Length > 0)
            {
                List<ICADExCommand> ecClasses = new List<ICADExCommand>();
                foreach (Type cls in classes)
                {
                    if ((cls != null) && cls.GetInterfaces().Any(r => r == typeof(ICADExCommand)))
                    // 说明这个类实现了 CAD 的命令接口
                    {
                        MethodInfo m = FindExCommandMethod(cls);
                        //
                        if (m != null)
                        {
                            // 生成一个实例并转换为接口
                            var ins = ass.CreateInstance(cls.FullName);
                            ICADExCommand exC = ins as ICADExCommand;

                            if (exC != null)
                            {
                                ecClasses.Add(exC);
                            }
                        }
                    }
                }
                return ecClasses;
            }
            return new List<ICADExCommand>();
        }

        /// <summary>
        /// 从一个 实现了 CAD 的命令接口 的类中搜索到对应的 外部命令
        /// </summary>
        /// <param name="implimentedType">此类必须 实现了 CAD 的命令接口 </param>
        /// <returns></returns>
        public static MethodInfo FindExCommandMethod(Type implimentedType)
        {
            // 寻找此类中所实现的那个 Execute 方法
            Type[] paraTypes = new Type[]
            {
                                typeof (SelectionSet), typeof (string).MakeByRefType(),
                                typeof (IList<ObjectId>).MakeByRefType()
            };
            //
            MethodInfo m = implimentedType.GetMethod("Execute", paraTypes);
            //
            if (m != null && m.IsPublic)
            {
                return m;
            }
            return null;
        }
    }
}
