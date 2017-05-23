using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;

// This line is not mandatory, but improves loading performances
// 测试中，如果不使用下面这条，则在AutoCAD中对应的 External Command 不能正常加载。

[assembly: CommandClass(typeof(EcLoadTemplate1))]

namespace eZcad.Addins
{
    /// <summary> 自动切换输入法 </summary>
    internal class EcLoadTemplate1
    {
        #region --- Fields

        #endregion

        #region --- 构造函数

        /// <summary> 如果此类中有作为实例方法的 EcInstanceMethod， 则此类必须有一个无参数的构造函数，否则，当用户在 AutoCAD中调用此实例方法时，会出现报错“没有为该对象定义无参数的构造函数” </summary>
        public EcLoadTemplate1()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\n------- 无参数 ---------\n 构造了一个实例 EcLoad。");
        }

        public EcLoadTemplate1(string name) : this()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\n-------- 有参数 --------\n 构造了一个实例 EcLoad。");
        }

        #endregion

        #region --- 外部命令 - 实例 - 1

        [CommandMethod("eZcad", "EcInstanceMethod10", CommandFlags.Modal)]
        public void EcInstanceMethod1() // This method can have any name
        {
            // 当用户第一次在 AutoCAD 界面中通过命令行调用此方法时，AutoCAD会通过此类的无参数构造函数创建出一个对应的实例A
            //（即使当此程序集在加载时在 IExtensionApplication.Initialize() 方法内部创建过此类的实例B），然后调用对应的 实例A.EcInstanceMethod1()。
            DocumentModifier.ExecuteCommand(InstanceMethod1);
        }

        public void InstanceMethod1(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            if (impliedSelection != null && impliedSelection.Count > 0)
            {
            }
        }

        #endregion

        #region --- 外部命令 - 实例 - 2

        [CommandMethod("eZcad", "EcInstanceMethod20", CommandFlags.Modal)]
        public void EcInstanceMethod2() // This method can have any name
        {
            // 当用户已经在 AutoCAD 界面中通过命令行调用过此类中的某实例 EcInstanceMethod1 后，再调用此 EcInstanceMethod2 时，
            // 不会再创建出此类的另一个实例对象C，而是会直接调用 实例对象A.EcInstanceMethod2()。
            DocumentModifier.ExecuteCommand(InstanceMethod2);
        }

        public void InstanceMethod2(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            if (impliedSelection != null && impliedSelection.Count > 0)
            {
            }
        }

        #endregion

        #region --- 外部命令 - 静态 - 1

        [CommandMethod("eZcad", "EcStaticeMethod10", CommandFlags.Modal)]
        public static void EcStaticeMethod1() // This method can have any name
        {
            // 当用户在 AutoCAD 界面中通过命令行调用过此类中的某静态 EcStaticeMethod 时，不会创建出此类的任何实例（也不需要），而是直接调用静态方法 EcLoad.EcStaticeMethod()。
            DocumentModifier.ExecuteCommand(StaticeMethod1);
        }

        public static void StaticeMethod1(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            if (impliedSelection != null && impliedSelection.Count > 0)
            {
            }
        }

        #endregion

        #region --- 私有方法

        #endregion
    }
}