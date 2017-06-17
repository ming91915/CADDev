using AutoCAD;
using Autodesk.AutoCAD.ApplicationServices;

namespace eZcad
{
    public class Example_AddMenuItem
    {
        /// <summary> 添加菜单的操作可以直接在AutoCAD运行过程中执行，而不用像Revit中一样必须要先关闭然后在程序启动时添加 </summary>
        private static void AddMenuItem()
        {
            // This example creates a new menu called TestMenu and inserts a menu item
            // into it. The menu is then displayed on the menu bar.
            // To remove the menu after execution of this macro, use the Customize Menu
            // option from the Tools menu.

            var app = Application.AcadApplication as AcadApplication;

            // AcadMenuGroup 表示AutoCAD中加载的一个.cuix文件中的菜单集合。  
            // MenuGroups.Item(0) 与 Menubar 中的菜单集合 并不是同一个集合，它的 MenuFileName 属性对应的文件为：
            // C:\Users\zengfy\appdata\roaming\autodesk\autocad 2014\r19.1\chs\support\acad.cuix
            AcadMenuGroup currMenuGroup = app.MenuGroups.Item(0);

            //Create the new menu
            AcadPopupMenu topPpMenu = currMenuGroup.Menus.Add("MenuHandler");

            // Assign the macro string the VB equivalent of "ESC ESC _open "
            string openMacro = "ESC ESC _open "; // VBA 中的写法：openMacro = Chr(3) & Chr(3) & Chr(95) & "open" & Chr(32)

            //Add a menu item to the new menu, Macro 即对应要在命令行中输入的字符
            AcadPopupMenuItem childMenuItem = topPpMenu.AddMenuItem(Index: topPpMenu.Count + 1, Label: "Open",
                Macro: openMacro);

            // 添加可以包含子项的菜单
            AcadPopupMenu menuItemContainer = topPpMenu.AddSubMenu(Index: topPpMenu.Count + 1, Label: "block");
            AcadPopupMenuItem childMenuItem2 = menuItemContainer.AddMenuItem(Index: menuItemContainer.Count + 1,
                Label: "画圆", Macro: "Circle ");

            // Display the menu on the menu bar

            // If index is a string, the name of the popup menu must contain any underscore that appears in the menu. 
            topPpMenu.InsertInMenuBar(Index: "工具(&T)");  // Index: app.MenuBar.Count + 1
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