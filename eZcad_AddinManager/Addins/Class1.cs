using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;

[assembly: CommandClass(typeof(NetLoadTest))]

namespace eZcad.Addins
{
    public class NetLoadTest
    {
        [CommandMethod("MNL")]
        public static void MyNetLoad()
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptStringOptions pso = new PromptStringOptions("\n输入要加载的程序集全路径: ");
            pso.AllowSpaces = true;
            PromptResult pr = ed.GetString(pso);
            if (pr.Status != PromptStatus.OK)
            {
                return;
            }
            try
            {
                //
                byte[] buff = File.ReadAllBytes(pr.StringResult);
                //先将插件拷贝到内存缓冲。一般情况下，当加载的文件大小大于2^32 byte (即4.2 GB），就会出现OutOfMemoryException，在实际测试中的极限值为630MB。
                var ass = Assembly.Load(buff);
                //  var ass = Assembly.LoadFile(pr.StringResult);

                //var ass = System.Reflection.Assembly.LoadFrom(pr.StringResult);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n无法加载程序集{0}: {1}", pr.StringResult, ex.Message);
            }
        }
    }

    [EcDescription("AddinManager 调试代码模板")]
    public class Ec_DebugTemplate123321 : ICADExCommand
    {
        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
             ref IList<ObjectId> elementSet)
        {
            return ExternalCommandResult.Succeeded;
        }
    }
}
