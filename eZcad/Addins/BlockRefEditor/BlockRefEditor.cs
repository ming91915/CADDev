using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins;
using eZcad.Debug;
using eZcad.Utility;

[assembly: CommandClass(typeof(BlockRefEditor))]

namespace eZcad.Addins
{
    /// <summary> 将多个块的属性值进行统一编辑 </summary>
    [EcDescription(CommandDescription)]
    public class BlockRefEditor : ICADExCommand
    {
        #region --- 命令设计

        /// <summary> 命令行命令名称，同时亦作为命令语句所对应的C#代码中的函数的名称 </summary>
        public const string CommandName = @"EditBlockRef";
        private const string CommandText = @"块属性";
        private const string CommandDescription = @"将多个块的属性值进行统一编辑";

        /// <summary> 将多个块的属性值进行统一编辑 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, CommandName,
            CommandFlags.Interruptible | CommandFlags.UsePickSet | CommandFlags.NoBlockEditor)
        , DisplayName(CommandText), Description(CommandDescription)
        , RibbonItem(CommandText, CommandDescription, eZConstants.ImageDirectory + "HighFill_32.png")]
        public void EditBlockRef()
        {
            DocumentModifier.ExecuteCommand(EditBlockRef);
        }

        public ExternalCommandResult Execute(SelectionSet impliedSelection, ref string errorMessage,
            ref IList<ObjectId> elementSet)
        {
            var s = new BlockRefEditor();
            return AddinManagerDebuger.DebugInAddinManager(s.EditBlockRef,
                impliedSelection, ref errorMessage, ref elementSet);
        }


        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将多个块的属性值进行统一编辑 </summary>
        public ExternalCmdResult EditBlockRef(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            List<BlockReference> blockRefs;
            var succ = GetBlocks(docMdf, out blockRefs);
            if (!succ) return ExternalCmdResult.Cancel;

            // 提取块参照中的属性定义
            var attDefs = GetExtinctAttDefs(blockRefs);
            //
            // eZstd.Miscellaneous.DebugUtils.ShowEnumerableProperty(attDefs, "Tag");
            //
            var attBlocks = blockRefs.Select(r => new AttriBlock(r)).ToList();
            var f = new BlockRefEditorForm(attBlocks, attDefs);
            var res = f.ShowDialog();
            if (res == DialogResult.OK)
            {
                // 修改块参照中属性定义的值
                return ExternalCmdResult.Commit;

            }
            else
            {
                return ExternalCmdResult.Cancel;
            }
        }

        /// <summary>
        /// 提取所有块参照实例中，不重复的属性定义
        /// </summary>
        /// <param name="blockRefs"></param>
        /// <returns></returns>
        private List<string> GetExtinctAttDefs(IEnumerable<BlockReference> blockRefs)
        {
            // 提取块参照中的属性定义
            var btrs = new List<BlockTableRecord>(); // 已经解析过的块参照定义
            var attDefs = new List<string>();
            foreach (var br in blockRefs)
            {
                var btr = br.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                if (btrs.Contains(btr))
                {
                    continue;
                }
                var adf = btr.GetAttributeDefinitions();
                // 
                foreach (var attDef in adf)
                {
                    if (!attDefs.Contains(attDef.Tag))
                    {
                        attDefs.Add(attDef.Tag);
                    }
                }
                //
                btrs.Add(btr);
            }
            return attDefs;
        }

        #region ---   界面操作

        /// <summary> 选择多个块参照 </summary>
        /// <param name="docMdf"></param>
        /// <param name="blockRefs">选择的块参照</param>
        /// <returns></returns>
        private bool GetBlocks(DocumentModifier docMdf, out List<BlockReference> blockRefs)
        {
            blockRefs = new List<BlockReference>();
            // 创建一个 TypedValue 数组，用于定义过滤条件
            var filterTypes = new TypedValue[]
            {
               new TypedValue((int) DxfCode.Start, "INSERT")
            };
            var pso = new PromptSelectionOptions();
            pso.MessageForAdding = "选择块参照";
            pso.Keywords.Add("NoFilter", "无(N)", "无(N)"); //

            // 请求在图形区域选择对象
            var res = docMdf.acEditor.GetSelection(new SelectionFilter(filterTypes));

            // 如果提示状态OK，表示对象已选
            if (res.Status == PromptStatus.OK)
            {
                var acSSet = res.Value;
                foreach (var id in acSSet.GetObjectIds())
                {
                    var e = id.GetObject(OpenMode.ForRead) as BlockReference;
                    if (e != null)
                    {
                        blockRefs.Add(e);
                    }
                }
                //
                return blockRefs.Count > 0;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}