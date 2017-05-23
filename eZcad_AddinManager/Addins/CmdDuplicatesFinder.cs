using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

[assembly: CommandClass(typeof(CmdDuplicatesFinder))]
// [assembly: PerDocumentClass(typeof(CmdDuplicatesFinder))]

namespace eZcad.Addins
{
    class CmdDuplicatesFinder
    {
        [CommandMethod("FindCmdDuplicates")]
        public void FindCmdDuplicatesCmd()
        {
            string asmPath = SelectAssembly();

            if (asmPath == null)
                return;

            FindCmdDuplicates(asmPath);
        }

        private string SelectAssembly()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "Load Assembly File";

            dlg.InitialDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop);

            dlg.Filter = ".Net Assembly (*.dll)|*.dll";
            dlg.FilterIndex = 1;
            dlg.RestoreDirectory = true;

            while (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    AssemblyName asmName =
                        AssemblyName.GetAssemblyName(dlg.FileName);

                    return dlg.FileName;
                }
                catch (BadImageFormatException ex)
                {
                    MessageBox.Show(
                        "Sorry the file is not a valid .Net assembly...",
                        "Invalid Assembly",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            return null;
        }

        public void FindCmdDuplicates(string asmPath)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Assembly asm = Assembly.LoadFile(asmPath);

            Type[] expTypes;
            var errTypes = new Exception[0];
            try
            {
                expTypes = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                errTypes = ex.LoaderExceptions;
                expTypes = ex.Types;
            }

            var map = new Dictionary<string, List<MethodInfo>>();

            try
            {
                //
                foreach (Type type in expTypes)
                {
                    if (type == null){continue;}

                    MethodInfo[] methods = type.GetMethods();

                    foreach (MethodInfo method in methods)
                    {
                        CommandMethodAttribute attribute =
                            GetCommandMethodAttribute(method);

                        if (attribute == null)
                            continue;

                        if (!map.ContainsKey(attribute.GlobalName))
                        {
                            var methodInfo = new List<MethodInfo>();

                            map.Add(attribute.GlobalName, methodInfo);
                        }

                        map[attribute.GlobalName].Add(method);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }

            // 查看加载出错的类
            foreach (Exception ex in errTypes)
            {
                ed.WriteMessage($"\n{ex.Message}");
            }
            // 查看重新的类
            foreach (var keyValuePair in map)
            {
                if (keyValuePair.Value.Count > 1)
                {
                    ed.WriteMessage(
                        "\nDuplicate Attribute: " + keyValuePair.Key);

                    foreach (var method in keyValuePair.Value)
                    {
                        ed.WriteMessage(
                            "\n – Method: " + method.Name);
                    }
                }
            }
        }

        public CommandMethodAttribute GetCommandMethodAttribute(MethodInfo method)
        {
            object[] attributes = method.GetCustomAttributes(true);

            foreach (object attribute in attributes)
            {
                if (attribute is CommandMethodAttribute)
                {
                    return attribute as CommandMethodAttribute;
                }
            }

            return null;
        }
    }
}