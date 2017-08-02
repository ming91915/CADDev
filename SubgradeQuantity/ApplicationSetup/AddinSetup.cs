using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using eZcad.SubgradeQuantity;
using eZcad.SubgradeQuantity.Cmds;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Orientation = System.Windows.Controls.Orientation;
using RibbonButton = Autodesk.Windows.RibbonButton;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonPanelSource = Autodesk.Windows.RibbonPanelSource;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(AddinSetup))]
// 下面关于 assembly: ExtensionApplication 在实际使用时必须添加
[assembly: ExtensionApplication(typeof(AddinSetup))]

namespace eZcad.SubgradeQuantity
{
    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class AddinSetup : IExtensionApplication
    {
        void IExtensionApplication.Initialize()
        {
            // Add one time initialization here
            // One common scenario is to setup a callback function here that 
            // unmanaged code can call. 
            // To do this:
            // 1. Export a function from unmanaged code that takes a function
            //    pointer and stores the passed in value in a global variable.
            // 2. Call this exported function in this function passing delegate.
            // 3. When unmanaged code needs the services of this managed module
            //    you simply call acrxLoadApp() and by the time acrxLoadApp 
            //    returns  global function pointer is initialized to point to
            //    the C# delegate.
            // For more info see: 
            // http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
            // http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
            // http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
            // as well as some of the existing AutoCAD managed apps.

            // Initialize your plug-in application here

            CreateRibbon();
            // ComponentManager.ItemInitialized 事件在每一次添加对象（选项卡 RibbonTab、不包括：工具栏）时都会触发。
            // ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        }

        void IExtensionApplication.Terminate()
        {
        }

        //添加自定义功能区选项卡
        private const string TabId_SubgradeQuantity = "MSDI_SubgradeQuantity";
        private const string TabName_SubgradeQuantity = "MSDI_SubgradeQuantity";
        private const string TabTitle_SubgradeQuantity = "路基工程量";

        /// <summary> 添加自定义功能区选项卡 </summary>
        [CommandMethod("SubgradeQuantityRibbon")]
        public void CreateRibbon()
        {
            if (ComponentManager.Ribbon == null)
            {
                MessageBox.Show(@"请先通过 RIBBON 命令打开选项卡，然后重复 SubgradeQuantityRibbon 命令。",
                    @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            RibbonControl ribCntrl = ComponentManager.Ribbon;
            // ribCntrl can also be RibbonServices.RibbonPaletteSet.RibbonControl, this class can be found in AcWindows.dll;  

            // 如果已经加载，则显示出来即可
            var existingRibbonTab = ribCntrl.Tabs.FirstOrDefault(r => r.Id == TabId_SubgradeQuantity
                                                                      && r.Title == TabTitle_SubgradeQuantity
                                                                      && r.Name == TabName_SubgradeQuantity);

            if (existingRibbonTab != null)
            {
                ed.Command(new object[] { "Ribbon" });
                existingRibbonTab.IsActive = true;
                return;
            }
            else
            {
                //Add the tab
                RibbonTab ribTab = new RibbonTab
                {
                    Title = TabTitle_SubgradeQuantity,
                    Id = TabId_SubgradeQuantity,
                    Name = TabName_SubgradeQuantity
                };
                ribCntrl.Tabs.Add(ribTab);
                //
                AddControls(ribTab);
                //set as active tab
                ribTab.IsActive = true;
            }
            ed.Command(new object[] { "Ribbon" });
        }

        #region --- 在选项卡中添加工具

        /// <summary> 在选项卡中添加工具 </summary>
        /// <param name="ribTab"></param>
        private static void AddControls(RibbonTab ribTab)
        {
            //create and add both panels

            // ----------------------------- 路基横断面系统 ------------------
            var pnl_SubgSystem = AddPanel(ribTab, "路基系统");
            AddButton(pnl_SubgSystem, method: typeof(SectionsConstructor).GetMethod(SectionsConstructor.CommandName, new Type[0]));
            AddButton(pnl_SubgSystem, method: typeof(StationNavigator).GetMethod(StationNavigator.CommandName, new Type[0]));
            AddButton(pnl_SubgSystem, method: typeof(LongitudinalSectionDrawer).GetMethod(LongitudinalSectionDrawer.CommandName, new Type[0]));

            // ----------------------------- 边坡防护的构造 ------------------
            var pnl_Slope = AddPanel(ribTab, "边坡防护");
            AddButton(pnl_Slope, method: typeof(SlopeConstructor).GetMethod(SlopeConstructor.CommandName, new Type[0]));
            AddButton(pnl_Slope, method: typeof(ProtectionPlacer).GetMethod(ProtectionPlacer.CommandName, new Type[0]));
            AddButton(pnl_Slope, method: typeof(ProtectionFlusher).GetMethod(ProtectionFlusher.CommandName, new Type[0]));
            
            // ----------------------------- 工程量的提取 ------------------
            var pnl_Quantity = AddPanel(ribTab, "工程量提取");
            AddButton(pnl_Quantity, method: typeof(InfosGetter_Slope).GetMethod(InfosGetter_Slope.CommandName, new Type[0]));
            AddButton(pnl_Quantity, method: typeof(InfosGetter_ThinFill).GetMethod(InfosGetter_ThinFill.CommandName, new Type[0]));


            // ----------------------------- 选项设置 ------------------
            var pnl_Settings = AddPanel(ribTab, "设置");
            AddButton(pnl_Settings, method: typeof(OptionsSetter).GetMethod(OptionsSetter.CommandName, new Type[0]));
        }

        private static RibbonPanel AddPanel(RibbonTab sourceTab, string panelTitle)
        {
            //create the panel source
            RibbonPanelSource ribPanelSource = new RibbonPanelSource();
            ribPanelSource.Title = panelTitle;

            //create the panel
            RibbonPanel ribPanel = new RibbonPanel();
            ribPanel.Source = ribPanelSource;
            sourceTab.Panels.Add(ribPanel);
            return ribPanel;
        }

        #endregion

        #region --- 添加按钮

        private static void AddButton(RibbonPanel panel, MethodInfo method)
        {
            if (!method.IsPublic) return;

            // 命令
            var commandMethod =
                method.GetCustomAttributes(typeof(CommandMethodAttribute)).First() as CommandMethodAttribute;
            if (commandMethod == null)
            {
                return;
            }
            var cmd = commandMethod.GroupName + "." + commandMethod.GlobalName;

            var ri = method.GetCustomAttributes(typeof(RibbonItemAttribute)).FirstOrDefault() as RibbonItemAttribute;
            BitmapImage largeImage = null;

            if (File.Exists(ri.LargeImagePath))
            {
                largeImage = new BitmapImage(new Uri(ri.LargeImagePath));
            }
            AddButton(panel, cmd, ri.Text, ri.Description, RibbonItemSize.Large, largeImage);
        }

        /// <summary> 在选项面板中添加一个按钮 </summary>
        /// <param name="panel">按钮所在的选项面板</param>
        /// <param name="commandName">按钮所对应的命令名，命令后不能加空格</param>
        /// <param name="buttonText">按钮的名称</param>
        /// <param name="description">按钮的功能描述</param>
        /// <param name="size">图片显示为大图像还是小图像 </param>
        /// <param name="largeImage"> 按钮所对应的图像，其像素大小为 32*32 </param>
        /// <param name="smallImage">按钮所对应的图像，其像素大小为 16*16 </param>
        private static void AddButton(RibbonPanel panel, string commandName, string buttonText,
            string description = null,
            RibbonItemSize size = RibbonItemSize.Large, BitmapImage largeImage = null, BitmapImage smallImage = null)
        {
            //create button1
            RibbonButton ribButton = new RibbonButton
            {
                Text = buttonText,
                Description = description,
                ShowText = true,
                Orientation = Orientation.Vertical, // 竖向则文字显示在图片正文，水平则文字显示在图片右边
                //
                Image = smallImage,
                LargeImage = largeImage,
                Size = size, // 按钮图片是显示为大图标还是正常小图标。
                ShowImage = true,
                //
                AllowInStatusBar = true,
                AllowInToolBar = true,

                // HelpTopic = "帮助",
                // HelpSource = new Uri("www.baidu.com"),

                //pay attention to the SPACE(or line feed) after the command name
                CommandParameter = commandName + "\n", // "DrawCircle ",
                CommandHandler = new AdskCommandHandler()
            };
            //
            panel.Source.Items.Add(ribButton);
        }

        #endregion

        /// <summary> 一个通用的类，用来响应各种 RibbonButton 按钮的事件 </summary>
        public class AdskCommandHandler : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                //is from Ribbon Button
                RibbonButton ribBtn = parameter as RibbonButton;
                if (ribBtn != null)
                {
                    //execute the command 
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute((string)ribBtn.CommandParameter,
                        true, false, true);
                }
            }
        }
    }
}