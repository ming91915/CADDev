using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using AutoCADDev.ExternalCommand;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADDev.ExternalCommand
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExternalCommandHandler
    {
        #region ---   外部命令的的加载与方法的提取

        /// <summary> 将程序集文件加载到内存，并且提取出其中的 CAD 外部命令 </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static List<MethodInfo> LoadExternalCommandsFromAssembly(string assemblyPath)
        {
            //先将插件拷贝到内存缓冲。一般情况下，当加载的文件大小大于2^32 byte (即4.2 GB），就会出现OutOfMemoryException，在实际测试中的极限值为630MB。
            byte[] buff = File.ReadAllBytes(assemblyPath);

            //不能直接通过LoadFrom或者LoadFile，而必须先将插件拷贝到内存，然后再从内存中Load
            Assembly asm = Assembly.Load(buff);

            return GetExternalCommandClass(asm);
        }

        private static List<MethodInfo> GetExternalCommandClass(Assembly ass)
        {
            List<MethodInfo> ecClasses = new List<MethodInfo>();
            var classes = ass.GetTypes();
            foreach (Type cls in classes)
            {
                if (cls.GetInterfaces().Any(r => r == typeof(IExternalCommand))) // 说明这个类实现了 CAD 的命令接口
                {
                    // 寻找此类中所实现的那个 Execute 方法
                    Type[] paraTypes = new Type[2]
                    {typeof (string).MakeByRefType(), typeof (IList<ObjectId>).MakeByRefType()};
                    MethodInfo m = cls.GetMethod("Execute", paraTypes);
                    //
                    if (m != null)
                    {
                        ecClasses.Add(m);
                    }
                }
            }
            return ecClasses;
        }

        #endregion

        #region ---   外部命令的执行

        private static MethodInfo _currentExternalCommand;
        private static string _currentExternalCommandAssemblyPath;
        /// <summary> 执行当前（即上次执行过的那个）外部命令 </summary>
        public static void InvokeCurrentExternalCommand()
        {
            if (_currentExternalCommand != null && _currentExternalCommandAssemblyPath != null)
            {
                InvokeExternalCommand(_currentExternalCommandAssemblyPath, _currentExternalCommand);
            }
        }

        /// <summary> 执行 CAD 的外部命令 </summary>
        /// <param name="assemblyPath">外部命令所对应的dll的绝对路径</param>
        /// <param name="externalCommand">此命令必须是实现了 IExternalCommand.Execute </param>
        /// <remarks>出于调试的即时更新的考虑，这里在每一次调试外部命令时，都对最新的dll进行重新加载。</remarks>
        public static void InvokeExternalCommand(string assemblyPath, MethodInfo externalCommand)
        {
            //
            Type tyOld = externalCommand.DeclaringType;
            string typeName = tyOld.FullName;

            // 重新加载最新的程序集
            List<MethodInfo> methods = LoadExternalCommandsFromAssembly(assemblyPath);

            //
            var newMethods = methods.Where(r => r.DeclaringType.FullName == typeName).ToArray();
            if (newMethods.Any())
            {
                InvokeCommand(newMethods[0]);
            }

            //
            _currentExternalCommandAssemblyPath = assemblyPath;
            _currentExternalCommand = externalCommand;
        }

        /// <summary> 执行 CAD 的外部命令 </summary>
        /// <param name="externalCommand">此命令必须是实现了 IExternalCommand.Execute </param>
        private static void InvokeCommand(MethodInfo externalCommand)
        {
            // 创建一个实例对象
            Type tp = externalCommand.DeclaringType;
            object ins = tp.Assembly.CreateInstance(tp.FullName);

            //// 构造一个实例，在此构造函数中即可以设计对应的插件功能了
            //object instance = asm.CreateInstance(
            //    typeName: classFullName,
            //    ignoreCase: false,
            //    bindingAttr: BindingFlags.CreateInstance,
            //    binder: null,
            //    args: constructorArgs,
            //    culture: CultureInfo.CurrentCulture,
            //    activationAttributes: null);

            // 执行命令
            ExternalCommandResult res;
            string errorMessage = "";
            List<ObjectId> elementSet;

            //
            // 注意如果要提取 ref 或 out 类型的参数的结果，则必须将对应的参数全部放置在一个 parameters 数组中
            object[] parameters = new object[] { "", new List<ObjectId>() };
            try
            {
                // 执行操作
                res = (ExternalCommandResult)externalCommand.Invoke(obj: ins, parameters: parameters);

                // 提取 ref 类型 或者 out 类型的 参数
                errorMessage = parameters[0] as string;
                elementSet = parameters[0] as List<ObjectId>;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = ex.Message;
                }
                else
                {
                    errorMessage = errorMessage + "\n\r--------------------------------------------\n\r"
                        + ex.Message;
                }
                elementSet = new List<ObjectId>();
                res = ExternalCommandResult.Failed;
            }

            // 对命令的结果进行处理
            switch (res)
            {
                case ExternalCommandResult.Failed:
                    {
                        MessageBox.Show(errorMessage);
                        break;
                    }
                case ExternalCommandResult.Cancelled:
                    {
                        break;
                    }
                case ExternalCommandResult.Succeeded:
                    {
                        break;
                    }
            }
        }
        #endregion
    }
}