using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using eZcad.Examples;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using MenuItem = Autodesk.AutoCAD.Windows.MenuItem;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(Example_UISetup))]
// 下面关于 assembly: ExtensionApplication 在实际使用时必须添加
// [assembly: ExtensionApplication(typeof (Example_UISetup))]

namespace eZcad.Examples
{
    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class Example_UISetup : IExtensionApplication
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

            // 添加AutoCAD界面中的右键菜单
            AddContextMenu();

            //  ComponentManager.ItemInitialized 事件在每一次添加对象（选项卡 RibbonTab、不包括：工具栏）时都会触发。
            // 如果不考虑使用功能选项卡，则可不用绑定此事件。实际情况下，不绑定此事件也可以正常使用功能选项卡，只是可能要手动通过Ribbon命令打开一次 选项栏 界面。
            ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;

            // 添加功能面板
            palette();
        }

        void IExtensionApplication.Terminate()
        {
            // Do plug-in application clean up here
            RemoveContextMenu();
            if (myPaletteSet != null)
            {
                myPaletteSet.Dispose();
            }
        }
        
        #region --- 添加自定义功能区选项卡
        
        /// <summary>
        /// ComponentManager.ItemInitialized 事件在每一次添加对象（选项卡 RibbonTab、不包括：工具栏）时都会触发。
        /// </summary>
        /// <remarks>如果不考虑使用功能选项卡，则可不用绑定此事件。
        /// 实际情况下，不绑定此事件也可以正常使用功能选项卡，只是可能要手动通过Ribbon命令打开一次 选项栏 界面。</remarks>
        void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
        {
            // ComponentManager.Ribbon 只有当 AutoCAD 自启动以来都没有打开过 Ribbon 的情况下才会为 null！
            // 一旦用户打开过选项卡（比如通过 Ribbon）命令，即使后面通过 RibbonClose 命令将选项卡栏关闭，ComponentManager.Ribbon 的值也不再为 null。
            if (ComponentManager.Ribbon != null)
            {
                createRibbon();
                ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            }
        }

        //添加自定义功能区选项卡
        private const string MY_TAB_ID = "MY_TAB_ID";

        [CommandMethod("AddMyRibbon")]
        public void createRibbon()
        {
            if (ComponentManager.Ribbon == null)
            {
                MessageBox.Show(@"请先通过 RIBBON 命令打开选项卡，然后重复 AddMyRibbon 命令。", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
                //如果Ribbon没有打开，直接操作命令行打开 
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("_.Ribbon\r", true, false, false);
                // Application.DocumentManager.MdiActiveDocument.Editor.Command(new object[] { "Ribbon" });
            }

            RibbonControl ribCntrl = Autodesk.Windows.ComponentManager.Ribbon;
            // ribCntrl can also be RibbonServices.RibbonPaletteSet.RibbonControl, this class can be found in AcWindows.dll;     

            //add the tab
            RibbonTab ribTab = new RibbonTab();
            ribTab.Title = "自定义Ribbon";
            ribTab.Id = MY_TAB_ID;
            ribCntrl.Tabs.Add(ribTab);

            //create and add both panels
            addPanel1(ribTab);
            addPanel2(ribTab);

            //set as active tab
            ribTab.IsActive = true;
        }

        private void addPanel2(RibbonTab ribTab)
        {
            //create the panel source
            RibbonPanelSource ribPanelSource = new RibbonPanelSource();
            ribPanelSource.Title = "Edit Registry";

            //create the panel
            RibbonPanel ribPanel = new RibbonPanel();
            ribPanel.Source = ribPanelSource;
            ribTab.Panels.Add(ribPanel);

            //create button1
            RibbonButton ribButtonDrawCircle = new RibbonButton
            {
                Text = "My Draw Circle",
                ShowText = true,
                // LargeImage =  LoadImage(null), // 参数值不能为 null

                //pay attention to the SPACE after the command name
                CommandParameter = "DrawCircle ",
                CommandHandler = new AdskCommandHandler()
            };
            //
            ribPanelSource.Items.Add(ribButtonDrawCircle);
        }

        private void addPanel1(RibbonTab ribTab)
        {
            //throw new NotImplementedException();
        }

        //--配置图片  
        private BitmapImage LoadImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            BitmapImage bitImage = new BitmapImage();
            bitImage.BeginInit();
            bitImage.StreamSource = ms;
            bitImage.EndInit();
            return bitImage;
        }

        //用来响应按钮
        class AdskCommandHandler : ICommand
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
                        true,
                        false, true);
                }
            }
        }

        /// <summary>
        /// CommandMethod("DrawCircle") 用来作为选项卡中 RibbonButton.CommandParameter；
        /// 而函数名 DrawCircleFunction 用来作为 AutoCAD 右键菜单中的 MenuItem.Click 事件的响应操作
        /// </summary>
        [CommandMethod("DrawCircle")]
        public void DrawCircleFunction()
        {
            //画个圆
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions getPointOptions = new PromptPointOptions("Pick Center Point : ");
            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);
            if ((getPointResult.Status == PromptStatus.OK))
            {
                PromptDistanceOptions getRadiusOptions = new PromptDistanceOptions("Pick Radius : ");
                getRadiusOptions.BasePoint = getPointResult.Value;
                getRadiusOptions.UseBasePoint = true;
                PromptDoubleResult getRadiusResult = ed.GetDistance(getRadiusOptions);
                if ((getRadiusResult.Status == PromptStatus.OK))
                {
                    Database dwg = ed.Document.Database;
                    Transaction trans = dwg.TransactionManager.StartTransaction();
                    try
                    {
                        Circle circle = new Circle(getPointResult.Value, Vector3d.ZAxis, getRadiusResult.Value);
                        BlockTableRecord btr = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);
                        btr.AppendEntity(circle);
                        trans.AddNewlyCreatedDBObject(circle, true);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        ed.WriteMessage("problem due to " + ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
        }

        #endregion

        #region --- 添加功能控制面板

        //添加控制面板
        public PaletteSet myPaletteSet;
        public UserControl1 myPalette;

        [CommandMethod("palette")]
        public void palette()
        {
            // check to see if it is valid
            if (myPaletteSet == null)
            {
                // create a new palette set, with a unique guid
                myPaletteSet = new PaletteSet("My Palette", new Guid("D61D0875-A507-4b73-8B5F-9266BEACD596"));

                // now create a palette inside, this has our tree control
                myPalette = new UserControl1();

                // now add the palette to the paletteset
                myPaletteSet.Add("Palette1", myPalette);
            }

            // now display the paletteset
            myPaletteSet.Visible = true;
        }

        [CommandMethod("palettedel")]
        public void palettedel()
        {
            if (myPaletteSet != null)
            {
                myPaletteSet.Close();
            }
        }


        /// <summary>
        /// 此自定义控件用来作为 功能面板中的界面
        /// </summary>
        public class UserControl1 : UserControl
        {
            public UserControl1()
            {
                InitializeComponent();
            }

            private Button button1;

            #region Component Designer generated code

            /// <summary> 
            /// Required method for Designer support - do not modify 
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
                this.button1 = new System.Windows.Forms.Button();
                this.SuspendLayout();
                // 
                // button1
                // 
                this.button1.Location = new System.Drawing.Point(27, 45);
                this.button1.Name = "button1";
                this.button1.Size = new System.Drawing.Size(75, 23);
                this.button1.TabIndex = 0;
                this.button1.Text = "button1";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Click += new System.EventHandler(this.button1_Click);
                // 
                // UserControl1
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.Controls.Add(this.button1);
                this.Name = "UserControl1";
                this.Size = new System.Drawing.Size(214, 229);
                this.ResumeLayout(false);
            }

            #endregion

            private void button1_Click(object sender, EventArgs e)
            {
                MessageBox.Show("hello");
            }
        }

        #endregion

        #region --- 在界面中添加右键菜单项

        //添加右键
        ContextMenuExtension myContextMenu;

        // 3. Create a private void procedure named AddContextMenu. 
        // Note: Put the Closing Curley brace after step 12. 
        private void AddContextMenu()
        {
            // 4. Declare an editor variable. Instantiate it by making it equal to the 
            // Editor property of the Application.DocumentManager.MdiActiveDocument.Editor 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // 5. Add a try catch block. 
            // Note: put the closing curley brace for the try below step 11.
            // Put the catch below this. The curley brace for the catch is 
            // after step 12.
            try
            {
                // 6. Make the ContextMenuExtension declared in step 2 equal 
                // to a new ContextMenuExtension 
                myContextMenu = new ContextMenuExtension();

                // 7. Make the Title property of the ContextMenuExtension 
                // instantiated in step 7 equal to "Circle Jig". (it is going 
                // to run the command completed in Lab3). 
                myContextMenu.Title = "Circle Jig";

                // 8. Declare a MenuItem variable named mi. Instantiate by 
                // making it eaual to a new MenuItem. For the string parameter 
                // use a string like "Run Circle Jig". 
                MenuItem mi = new MenuItem("Run Circle Jig");

                // 9. The way the Context menu works is that for each menu entry, we specify a 
                // specific member function to be called handling the menu click event. 
                // Use the MenuItem Click event (mi.Click += )to specify that we want the 
                // click event to be handled by a function named CallbackOnClick. You will 
                // create this function in step 20 - 22.
                mi.Click += CallbackOnClick;

                // 10. Use the Add method of the MenuItems collection of the 
                // ContextMenuExtension instantiated in step 6. Pass in the 
                // MenuItem created in step 8. 
                myContextMenu.MenuItems.Add(mi);

                // 11. Use the AddDefaultContextMenuExtension of the Application. 
                // Pass in the ContextMenuExtension 
                Autodesk.AutoCAD.ApplicationServices.Application.AddDefaultContextMenuExtension(myContextMenu);
            }
            catch (Exception ex)
            {
                // 12. Use the editor variable created in step 4 and write a message. 
                // to the command line Something like" 
                // "Error Adding Context Menu: " + ex.Message 
                ed.WriteMessage("Error Adding Context Menu: " + ex.Message);
            }
        }

        // 13. Create a procedure named RemoveContextMenu. 
        // Note: Put the closing curley brace after step 19. 
        public void RemoveContextMenu()
        {
            // 14. Declare an document variable. Instantiate it by making it 
            // equal the MdiActiveDocument 
            Document activeDoc = Application.DocumentManager.MdiActiveDocument;

            // 15. Add a try catch block. 
            // Note: put the try closing curley brace below step 18.
            // put the catch after this. put the catch closing curley brace after
            // step 19.
            try
            {
                // 16. Use an "if" statement and test if the ContextMenuExtension 
                // declared in step 2 is not null (!=)
                // Note: put the closing curley brace below step 18 
                if ((myContextMenu != null))
                {
                    // 17. In the if statement, use RemoveDefaultContextMenuExtension 
                    // of the Application. Pass in the ContextMenuExtension declared 
                    // in step 2 
                    Autodesk.AutoCAD.ApplicationServices.Application.RemoveDefaultContextMenuExtension(myContextMenu);

                    // 18. Make the ContextMenuExtension declared in step 2 equal to null 
                    myContextMenu = null;
                }
            }
            catch (Exception ex)
            {
                // Use an "If" statement and test if the activeDoc
                // declared in step 14 is null  before using it.
                // In AutoCAD 2013, If AutoCAD is being closed, we may not have access to the active document here.
                if (activeDoc != null)
                {
                    // 19. Use the editor property of the active document and write a message. Something like 
                    // "Error Removing Context Menu: " + ex.Message 
                    activeDoc.Editor.WriteMessage("Error Removing Context Menu: " + ex.Message);
                }
            }
        }

        #region --- 右键菜单项所对应的操作

        [DllImport("accore.dll", CharSet = CharSet.Unicode, EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
        public static extern bool acedPostCommand32(string cmd);

        [DllImport("accore.dll", CharSet = CharSet.Unicode, EntryPoint = "?acedPostCommand@@YAHPEB_W@Z")]
        public static extern bool acedPostCommand64(string cmd);

        //check the OS (32 or 64 bits)
        public static bool is64bits
        {
            get { return (Application.GetSystemVariable("PLATFORM").ToString().IndexOf("64") > 0); }
        }

        private void CallbackOnClick(object Sender, EventArgs e)
        {
            // 21. Use the using statement and create a variable as a DcoumentLock. 
            // Instantiate it by making it equal to the .MdiActiveDocument.LockDocument 
            // method. (Because this event is running outside of the Document context we 
            // need to lock the document). By design, AutoCAD’s data is stored in documents, 
            // where commands that access entities within them have rights to make 
            // modifications. When we run our code in response to a context-menu click, 
            // we are accessing the document from outside the command structure. 
            // In order to unlock the document we simply dispose DocumentLock object 
            // returned on the original lock request. 
            // Note: put the using closing curley brace after step 22. 
            using (DocumentLock docLock = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                // 22. Call the CircleJig() function. 
                // Example_Jig.CircleJig();
                DrawCircleFunction();
                // Added from step 25 
                if (is64bits)
                {
                    acedPostCommand64("CANCELCMD");
                }
                else
                {
                    acedPostCommand32("CANCELCMD");
                }
            }
        }

        #endregion

        #endregion
    }
}