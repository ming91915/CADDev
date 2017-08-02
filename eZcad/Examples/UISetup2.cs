using System;
using System.Windows.Forms;
using AutoCAD;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

namespace eZcad.Examples
{
    public partial class UISetup2
    {
        /// <summary> 添加菜单的操作可以直接在AutoCAD运行过程中执行，而不用像Revit中一样必须要先关闭然后在程序启动时添加 </summary>
        public static void AddMenuItem()
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

        /// <summary>
        /// 从局部 CUI 文件中加载工具栏
        /// </summary>
        /// <param name="cuixFile">局部自定义文件的绝对路径</param>
        /// <param name="cuiName">局部自定义文件的名称</param>
        [CommandMethod("LoadToolbars")]
        public static void LoadToolbars(string cuiName, string cuixFile)
        {
            var app = Application.AcadApplication as AcadApplication;
            AcadMenuGroups mgroups = app.MenuGroups;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var mg = default(AcadMenuGroup);
            try
            {
                //Attempt to access our menugroup 
                mg = mgroups.Item(cuiName);
            }
            catch (System.Exception)
            {
                try
                {
                    ; //Failure simply means we need to load the CUI first   
                    mgroups.Load(cuixFile);
                    mg = mgroups.Item(cuiName);
                }
                catch (Exception)
                {

                    throw new InvalidOperationException("指定的局部自定义文件中没有对应的名称！");
                }
            }

            foreach (AcadMenuGroup g in mgroups)
            {
                ed.WriteMessage("\n" + g.MenuFileName + ",\t" + g.Name);
            }

            //Cycle through the toobars, setting them to visible  
            ed.WriteMessage("\n" + mg.Toolbars.Count.ToString());
            ed.WriteMessage("\n" + mg.Menus.Count.ToString());
            for (int i = 0; i <= mg.Toolbars.Count - 1; i++)
            {
                ed.WriteMessage("\n" + mg.Toolbars.Item(i).Name.ToString() + "\n");
                mg.Toolbars.Item(i).Visible = true;
           }
        }

        /// <summary>从局部Cui文件中加载选项卡（必须先添加对 AcCui.dll 的引用） </summary>
        /// <param name="strCuipath"></param>
        /// <remarks> 
        /// 使用cuix文件添加对应 RibbonButton  
        /// 先判断是否添加配置cuix  
        /// 将配置cuix中的配置功能项添加到acad.cuix中的功能区
        /// 刷新工作空间的功能区命令  
        /// </remarks>
        public static void AddRibbonButtonByCustomCui(string strCuipath)
        {
            string mainCuiFile = (string)Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("MENUNAME");
            mainCuiFile += ".cuix";
            CustomizationSection csLoad = new CustomizationSection(mainCuiFile);
            PartialCuiFileCollection pPartialCuiFileCollection = csLoad.PartialCuiFiles;
            if (pPartialCuiFileCollection.Contains("mycui.cuix"))
            {
                MessageBox.Show("已加载插件！");
                Autodesk.AutoCAD.ApplicationServices.Application.UnloadPartialMenu(strCuipath);
                //return;  
            }

            bool isOK = Autodesk.AutoCAD.ApplicationServices.Application.LoadPartialMenu(strCuipath);
            //加载自定义cui  
            if (!isOK)
            {
                MessageBox.Show("加载自定义配置文件失败！");
                return;
            }

            //加载CUI  
            //Application.QuitWillStart += new EventHandler(Application_BeginQuit);  
            //Application.BeginQuit += new EventHandler(Application_BeginQuit);  
            //Autodesk.Windows.ComponentManager.ApplicationMenu.Opening += new EventHandler<EventArgs>(ApplicationMenu_Opening);  

            CustomizationSection cs = new CustomizationSection(mainCuiFile);
            PartialCuiFileCollection cuiFiles = cs.PartialCuiFiles;
            //acad.cuix配置文件  
            if (cuiFiles.Contains("mycui.cuix"))
            {
                //将my.cuix文件中的配置按钮写入acad.cuix文件中去  
                string strPartialCui = cuiFiles.GetFileNameByIndex(cuiFiles.IndexOf("mycui.cuix"));
                CustomizationSection csCustom = new CustomizationSection(strPartialCui);
                var pRibbonPanelSource = csCustom.MenuGroup.RibbonRoot.FindPanel("RBNU_191_C0DED");
                //自定义panel中的ElementID  
                var pCloneRibbonPanelSource =
                    pRibbonPanelSource.Clone() as Autodesk.AutoCAD.Customization.RibbonPanelSource;
                cs.MenuGroup.RibbonRoot.RibbonPanelSources.Add(pCloneRibbonPanelSource);

                RibbonTabSource pRibbonTableSource2 = cs.MenuGroup.RibbonRoot.FindTab("RBN_00012112");
                //插件Tab的ElementID  
                RibbonPanelSourceReference pRibbonPanelSourceRefrence =
                    new RibbonPanelSourceReference(pRibbonTableSource2);
                //这一步ID一定要赋值  
                pRibbonPanelSourceRefrence.PanelId = pCloneRibbonPanelSource.ElementID;
                pRibbonTableSource2.Items.Add(pRibbonPanelSourceRefrence);

                cs.Save();
                Autodesk.AutoCAD.ApplicationServices.Application.ReloadAllMenus(); //否则不现实按钮  
            }
        }

    }
}