using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AutoCAD;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Addins.Utilities;

[assembly: CommandClass(typeof(MenuItemsLoader))]

namespace eZcad.Addins
{
    /// <summary> 加载程序集并添加对应菜单 </summary>
    internal class MenuItemsLoader
    {
        #region --- 命令设计

        /// <summary> 将多个单行文字按其定位进行组合 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "LoadMenuItems", CommandFlags.Modal)]
        public void EcLoadMenuItems()
        {
            DocumentModifier.ExecuteCommand(LoadMenuItems);
        }

        #endregion

        /// <summary> 将多个单行文字按其定位进行组合 </summary>
        public void LoadMenuItems(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            string assPath = @"D:\ProgrammingCases\GitHubProjects\CADDev\bin\eZcad - 副本.dll";
            string[] assPaths = Utils.ChooseOpenFile("选择要加载菜单的程序集", "程序集(*.dll; *.exe)| *.dll; *.exe",
                multiselect: false);
            if (assPaths == null)
            {
                return;
            }
            assPath = assPaths[0];

            Assembly ass = null;
            Type[] types;
            try
            {
                ass = Assembly.LoadFrom(assPath);
                types = ass.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }
            //
            if (ass != null && types != null)
            {
                var atts = ass.GetCustomAttributes(typeof(CommandClassAttribute));
                var mtds = GetExternalCommands(atts as CommandClassAttribute[]);
                if (mtds != null && mtds.Count > 0)
                {
                    var app = Application.AcadApplication as AcadApplication;
                    var menuName = GetMenuName(docMdf.acEditor);
                    AddMenus(app, menuName + "", mtds);
                }
            }
        }

        private static List<MethodInfo> GetExternalCommands(CommandClassAttribute[] classes)
        {
            var ecClasses = new List<MethodInfo>();
            if (classes != null && classes.Length > 0)
            {
                foreach (CommandClassAttribute clsatt in classes)
                {
                    var cls = clsatt.Type;
                    var mthds = cls.GetMethods();
                    foreach (var mtd in mthds)
                    {
                        var att = mtd.GetCustomAttributes(typeof(CommandMethodAttribute));
                        if (att.Any())
                        {
                            ecClasses.Add(mtd);
                        }
                    }
                }
            }
            return ecClasses;
        }

        private string GetMenuName(Editor ed)
        {
            var op = new PromptStringOptions("菜单名称")
            {
                AllowSpaces = false,
                DefaultValue = eZConstants.eZGroupCommnad,
                UseDefaultValue = true,
            };
            var res = ed.GetString(op);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }
            return eZConstants.eZGroupCommnad;
        }

        private void AddMenus(AcadApplication app, string menuName, List<MethodInfo> methods)
        {
            // This example creates a new menu called TestMenu and inserts a menu item
            // into it. The menu is then displayed on the menu bar.
            // To remove the menu after execution of this macro, use the Customize Menu
            // option from the Tools menu.

            // AcadMenuGroup 表示AutoCAD中加载的一个.cuix文件中的菜单集合。  
            // MenuGroups.Item(0) 与 Menubar 中的菜单集合 并不是同一个集合，它的 MenuFileName 属性对应的文件为：
            // C:\Users\zengfy\appdata\roaming\autodesk\autocad 2014\r19.1\chs\support\acad.cuix
            AcadMenuGroup currMenuGroup = app.MenuGroups.Item(0);

            //Create the new menu
            foreach (AcadPopupMenu m in currMenuGroup.Menus)
            {
                if (m.Name == menuName)
                {
                    return;
                    m.RemoveFromMenuBar();
                    break;
                }
            }
            AcadPopupMenu topPpMenu = currMenuGroup.Menus.Add(menuName);
            foreach (var mtd in methods)
            {
                var att = mtd.GetCustomAttributes(typeof(CommandMethodAttribute)).First() as CommandMethodAttribute;

                // Assign the macro string the VB equivalent of "ESC ESC _open "
                //string openMacro = "ESC ESC _open "; // VBA 中的写法：openMacro = Chr(3) & Chr(3) & Chr(95) & "open" & Chr(32)

                //Add a menu item to the new menu, Macro 即对应要在命令行中输入的字符
                string label;
                // label = string.IsNullOrEmpty(att.LocalizedNameId) ? att.GlobalName : att.LocalizedNameId;
                var des =
                    mtd.GetCustomAttributes(typeof(DisplayNameAttribute)).FirstOrDefault() as DisplayNameAttribute;
                label = des != null && !string.IsNullOrEmpty(des.DisplayName) ? des.DisplayName : att.GlobalName;

                AcadPopupMenuItem childMenuItem = topPpMenu.AddMenuItem(Index: topPpMenu.Count + 1, Label: label,
                    Macro: att.GlobalName + "\n");
                
                //// 添加可以包含子项的菜单
                //AcadPopupMenu menuItemContainer = topPpMenu.AddSubMenu(Index: topPpMenu.Count + 1, Label: "block");
                //AcadPopupMenuItem childMenuItem2 = menuItemContainer.AddMenuItem(Index: menuItemContainer.Count + 1,
                //    Label: "画圆", Macro: "Circle ");
            }

            // Display the menu on the menu bar
            // If index is a string, the name of the popup menu must contain any underscore that appears in the menu. 
            topPpMenu.InsertInMenuBar(Index: app.MenuBar.Count + 1); // Index: "工具(&T)"
            return;
            // 以下为功能测试项 --------------------------------------------------------

            // The index must be either an integer or a string. If an integer, the index must be between 0 and N-1, where N is the number of objects in the menu bar. The new item will be added immediately before the specified index location. 
            topPpMenu.InsertInMenuBar(Index: app.MenuBar.Count + 1);

            // 从 MenuBar 提取一个菜单项。
            // 注意：MenuBar 只能用来读取其中的 AcadPopupMenu 集合，而不能操纵菜单项的添加或移除。
            AcadMenuBar mb = app.MenuBar;
            // 下面这个Item(0)对应的是AutoCAD界面中菜单栏中的第一个菜单项，通常这个菜单项为“文件(F)”
            AcadPopupMenu ppm1 = mb.Item(0);

            // 将菜单项从 MenuBar 中移除
            topPpMenu.RemoveFromMenuBar();
        }
    }
}