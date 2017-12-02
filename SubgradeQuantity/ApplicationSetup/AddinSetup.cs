using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using eZcad.Addins;
using eZcad.SubgradeQuantity;
using eZcad.SubgradeQuantity.Cmds;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
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
            InitializeComponent();

            // 
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"{TabId_SubgradeQuantity}程序加载成功\n");
        }

        void IExtensionApplication.Terminate()
        {
        }

        //添加自定义功能区选项卡
        private const string TabId_SubgradeQuantity = "MSDI_SubgradeQuantity";
        private const string TabName_SubgradeQuantity = "MSDI_SubgradeQuantity";
        private const string TabTitle_SubgradeQuantity = "路基工程量";
        private static string IconDir = "";


        private void InitializeComponent()
        {
            var assPath = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName;
            IconDir = Directory.GetCurrentDirectory();
            IconDir =new FileInfo(assPath).Directory.FullName;
            // CreateRibbon();

            // ComponentManager.ItemInitialized 事件在每一次添加对象（选项卡 RibbonTab、不包括：工具栏）时都会触发。
            // ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        }

        private const string CmdStartRibbon = @"SQRibbon";
        /// <summary> 添加自定义功能区选项卡 </summary>
        [CommandMethod(ProtectionConstants.eZGroupCommnad, CmdStartRibbon, ProtectionConstants.ModelState)]
        public void CreateRibbon()
        {
            if (ComponentManager.Ribbon == null)
            {
                MessageBox.Show($"请先通过 RIBBON 命令打开选项卡，然后重复 {CmdStartRibbon} 命令。",
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
                try
                {
                    AddControls(ribTab);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "选项卡工具加载出错\r\n" + ex.StackTrace, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //set as active tab
                ribTab.IsActive = true;
            }
            ed.Command(new object[] { "Ribbon" });
        }

        /// <summary> 在选项卡中添加工具 </summary>
        /// <param name="ribTab"></param>
        private static void AddControls(RibbonTab ribTab)
        {
            // ----------------------------- 项目信息 ----------------------------------------
            var pnl_Project = CreatePanel(ribTab, "项目信息");
            AddButton(pnl_Project, method: typeof(ProjectInfos).GetMethod(ProjectInfos.CommandName, new Type[0]), size: RibbonItemSize.Large);

            // ----------------------------- 路基横断面系统 ----------------------------------------
            var pnl_SubgSystem = CreatePanel(ribTab, "路基系统");
            AddButton(pnl_SubgSystem, method: typeof(SectionsConstructor).GetMethod(SectionsConstructor.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_SubgSystem, method: typeof(StationNavigator).GetMethod(StationNavigator.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_SubgSystem, method: typeof(LongitudinalSectionDrawer).GetMethod(LongitudinalSectionDrawer.CommandName, new Type[0]), size: RibbonItemSize.Large);

            var spltBtn3 = CreateSplitButton(pnl_SubgSystem, "横断面信息");
            AddButton(spltBtn3, method: typeof(SectionWalker).GetMethod(SectionWalker.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn3, method: typeof(SectionInfosPlayer).GetMethod(SectionInfosPlayer.CommandName, new Type[0]), size: RibbonItemSize.Large);

            // ----------------------------- 边坡防护的构造 ----------------------------------------
            var pnl_Slope = CreatePanel(ribTab, "边坡防护");
            AddButton(pnl_Slope, method: typeof(SlopeConstructor).GetMethod(SlopeConstructor.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Slope, method: typeof(SlopeEraser).GetMethod(SlopeEraser.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Slope, method: typeof(SlopeWalker).GetMethod(SlopeWalker.CommandName, new Type[0]), size: RibbonItemSize.Large);
            //
            var spltBtn1 = CreateSplitButton(pnl_Slope, "边坡修剪");
            AddButton(spltBtn1, method: typeof(SlopeCutter).GetMethod(SlopeCutter.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn1, method: typeof(SlopeSegMerge).GetMethod(SlopeSegMerge.CommandName, new Type[0]), size: RibbonItemSize.Large);
            //
            AddButton(pnl_Slope, method: typeof(ProtectionPlacer).GetMethod(ProtectionPlacer.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Slope, method: typeof(SlopeProtTextModifier).GetMethod(SlopeProtTextModifier.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Slope, method: typeof(ProtectionFlusher).GetMethod(ProtectionFlusher.CommandName, new Type[0]), size: RibbonItemSize.Large);

            // ----------------------------- 工程量的提取 ----------------------------------------
            var pnl_Quantity = CreatePanel(ribTab, "工程量提取");
            AddButton(pnl_Quantity, method: typeof(SubgradeEnvir).GetMethod(SubgradeEnvir.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Quantity, method: typeof(CriterionEditor).GetMethod(CriterionEditor.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(pnl_Quantity, method: typeof(InfosGetter_Slope).GetMethod(InfosGetter_Slope.CommandName, new Type[0]), size: RibbonItemSize.Large);

            var spltBtn2 = CreateSplitButton(pnl_Quantity, "一般工程量");
            AddButton(spltBtn2, method: typeof(InfosGetter_ThinFill).GetMethod(InfosGetter_ThinFill.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn2, method: typeof(InfosGetter_HighFill).GetMethod(InfosGetter_HighFill.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn2, method: typeof(InfosGetter_SteepSlope).GetMethod(InfosGetter_SteepSlope.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn2, method: typeof(InfosGetter_StairExcav).GetMethod(InfosGetter_StairExcav.CommandName, new Type[0]), size: RibbonItemSize.Large);
            AddButton(spltBtn2, method: typeof(InfosGetter_RoadSurface).GetMethod(InfosGetter_RoadSurface.CommandName, new Type[0]), size: RibbonItemSize.Large);

            AddButton(pnl_Quantity, method: typeof(InfosGetter_FillCutInters).GetMethod(InfosGetter_FillCutInters.CommandName, new Type[0]), size: RibbonItemSize.Large);

            // ----------------------------- 其他工具 ----------------------------------------
            var pnl_Tools = CreatePanel(ribTab, "工具");
            AddButton(pnl_Tools, method: typeof(ClosedAreaSumup).GetMethod(ClosedAreaSumup.CommandName, new Type[0]), size: RibbonItemSize.Large);


            // ----------------------------- 选项设置 ----------------------------------------
            // var pnl_Settings = CreatePanel(ribTab, "设置");
        }

        private static RibbonPanel CreatePanel(RibbonTab sourceTab, string panelTitle)
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

        #region --- 添加按钮

        private static void AddButton(RibbonPanel panel, MethodInfo method, RibbonItemSize size)
        {
            string commandName;
            string buttonText;
            string description;
            BitmapImage largeImage;
            BitmapImage smallImage;
            GetMethodElements(method, out commandName, out buttonText, out description, out largeImage, out smallImage);
            //
            var ribButton = CreateButton(commandName, buttonText, description, size, largeImage);
            panel.Source.Items.Add(ribButton);
        }

        private static void AddButton(RibbonSplitButton splitButton, MethodInfo method, RibbonItemSize size)
        {
            string commandName;
            string buttonText;
            string description;
            BitmapImage largeImage;
            BitmapImage smallImage;
            GetMethodElements(method, out commandName, out buttonText, out description, out largeImage, out smallImage);
            //
            var ribButton = CreateButton(commandName, buttonText, description, size, largeImage);
            splitButton.Items.Add(ribButton);
        }

        /// <summary> 在选项面板中添加一个按钮 </summary>
        /// <param name="commandName">按钮所对应的命令名，命令后不能加空格</param>
        /// <param name="buttonText">按钮的名称</param>
        /// <param name="description">按钮的功能描述</param>
        /// <param name="size">图片显示为大图像还是小图像 </param>
        /// <param name="largeImage"> 按钮所对应的图像，其像素大小为 32*32 </param>
        /// <param name="smallImage">按钮所对应的图像，其像素大小为 16*16 </param>
        private static RibbonButton CreateButton(string commandName, string buttonText,
            string description = null,
            RibbonItemSize size = RibbonItemSize.Large, BitmapImage largeImage = null, BitmapImage smallImage = null)
        {
            //create button1
            var ribButton = new RibbonButton
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
                CommandParameter = commandName + "\n", // "Circle ",
                CommandHandler = new AdskCommandHandler()
            };
            //
            return ribButton;
        }

        #endregion



        private static RibbonSplitButton CreateSplitButton(RibbonPanel panel, string buttonText)
        {
            var sb = new RibbonSplitButton()
            {
                Text = buttonText,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ListStyle = RibbonSplitButtonListStyle.List,
            };
            panel.Source.Items.Add(sb);
            return sb;
        }

        /// <summary> 从方法的Attribute中提取界面所需要的元素 </summary>
        private static void GetMethodElements(MethodInfo method, out string commandName, out string buttonText,
           out string description, out BitmapImage largeImage, out BitmapImage smallImage)
        {
            commandName = null;
            buttonText = null;
            description = null;
            largeImage = null;
            smallImage = null;
            // 命令
            var commandMethod =
                method.GetCustomAttributes(typeof(CommandMethodAttribute)).First() as CommandMethodAttribute;
            if (commandMethod == null)
            {
                return;
            }
            commandName = commandMethod.GroupName + "." + commandMethod.GlobalName;

            var ri = method.GetCustomAttributes(typeof(RibbonItemAttribute)).FirstOrDefault() as RibbonItemAttribute;
            if (ri != null)
            {
                buttonText = ri.Text;
                description = ri.Description;
                //
                if (!string.IsNullOrEmpty(ri.LargeImagePath))
                {
                    var fp = Path.Combine(IconDir, ri.LargeImagePath);
                    if (File.Exists(fp))
                    {
                        largeImage = new BitmapImage(new Uri(fp));
                    }
                }
                if (!string.IsNullOrEmpty(ri.SmallImagePath))
                {
                    var fp = Path.Combine(IconDir, ri.LargeImagePath);
                    // var fp = Path.GetFullPath(ri.SmallImagePath);
                    if (File.Exists(fp))
                    {
                        smallImage = new BitmapImage(new Uri(fp));
                    }
                }
            }
        }

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