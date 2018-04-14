using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Debug;
using eZcad.Utility;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace eZcad.OnCode
{
    /// <summary> 对块参照对应的块定义中的属性定义的样式进行修改 </summary>
    [EcDescription(CommandDescription)]
    public class BlockAttStyleEditor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"EditBlockAttStyles";
        private const string CommandText = @"修改块属性样式";
        private const string CommandDescription = @"对块参照对应的块定义中的属性定义的样式进行修改";

        /// <summary> 对块参照对应的块定义中的属性定义的样式进行修改 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet) //  | CommandFlags.NoBlockEditor
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void EditBlockAttStyles()
        {
            DocumentModifier.ExecuteCommand(EditBlockAttStyles);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new BlockAttStyleEditor();
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.EditBlockAttStyles,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 对块参照对应的块定义中的属性定义的样式进行修改 </summary>
        public ExternalCmdResult EditBlockAttStyles(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            var attDefs = SelectAttibuteDefinitions();
            foreach (var attDef in attDefs)
            {
                var ByLayerColor = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByLayer, 256);
                var ByBlockColor = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByBlock, 0);
                attDef.UpgradeOpen();
                attDef.Color = ByBlockColor;
                attDef.DowngradeOpen();
                docMdf.WriteNow(attDef.Color);
            }
            return ExternalCmdResult.Commit;
        }

        /// <summary> 举例，选择多个属性定义对象 </summary>
        public static List<AttributeDefinition> SelectAttibuteDefinitions()
        {
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
                new TypedValue((int) DxfCode.Start, "ATTDEF"),
            };

            // Create our options object
            var op = new PromptSelectionOptions();

            // Add our keywords
            //op.Keywords.Add("First");
            //op.Keywords.Add("Second");

            // Set our prompts to include our keywords
            string kws = op.Keywords.GetDisplayString(true);
            op.MessageForAdding = "\n 请选择一个或多个属性定义 " + kws; // 当用户在命令行中输入A（或Add）时，命令行出现的提示字符。
            op.MessageForRemoval = "\nPlease remove objects from selection or " + kws;


            //获取当前文档编辑器
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // 请求在图形区域选择对象
            var res = acDocEd.GetSelection(op, new SelectionFilter(filterTypes));

            var output = new List<AttributeDefinition>();
            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value.GetObjectIds();
                foreach (var id in acSSet)
                {
                    var obj = id.GetObject(OpenMode.ForRead) as AttributeDefinition;
                    if (obj != null)
                    {
                        output.Add(obj);
                    }
                }
            }
            return output;
        }

    }
}