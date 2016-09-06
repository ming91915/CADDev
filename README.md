# CADDev
对AutoCAD进行开发

## AutoCAD AddinManager 快速调试插件
操作方法：
1. 在VS中新建一个 Test.dll 项目，并添加对 AddinManager.dll 的引用
2. 定义任何一个类，让其实现 AutoCADDev.ExternalCommand.IExternalCommand 接口
3. 打开 AutoCAD，并通过 NETLOAD 添加 AddinManager.dll
4. 在命令行中输入 LoadAddinManager ，以打开 AddinManager 界面；
5. 通过 AddinManager 界面中的 Load 加载  Test.dll ，程序会自动提取程序集中实现了 IExternalCommand 接口的类；
6. 双击任意一个方法，即可以开始进行  Test.dll  中的代码测试。当发现代码需要优化时，直接在VS中将代码进行修改并重新编译，然后在 AddinManager 继续点击对应的方法，即可以看到更新后的代码的测试结果；
7. 如果要进行断点调试，请先通过 VS 中的 Attatch to Process 将  Test.dll 与 正在运行的 AutoCAD 程序进行关联，然后通过双击 AddinManager 中的对应方法，即可以实现断点调试。

