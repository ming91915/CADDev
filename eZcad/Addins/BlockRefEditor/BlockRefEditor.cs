using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
            return eZcadAddinManagerDebuger.DebugInAddinManager(s.EditBlockRef,
                impliedSelection, ref errorMessage, ref elementSet);
        }

        #endregion

        private DocumentModifier _docMdf;

        /// <summary> 将多个块的属性值进行统一编辑 </summary>
        public ExternalCmdResult EditBlockRef(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _docMdf = docMdf;
            // 每一个块参照，以及其所在的布局空间（或模型空间）
            Dictionary<BlockReference, string> blockRefs = GetBlockRefs();
            if (blockRefs == null || blockRefs.Count == 0)
            {
                docMdf.WriteNow($"未搜索到匹配的块参照对象。");
                return ExternalCmdResult.Cancel;
            }

            // 提取块参照中的属性定义
            var attDefs = GetExtinctAttDefs(blockRefs.Keys);
            //
            // eZstd.Miscellaneous.DebugUtils.ShowEnumerableProperty(attDefs, "Tag");
            //
            var attBlocks = blockRefs.Select(r => new AttriBlock(r.Key, r.Value)).ToList();
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

        /// <summary> 从当前空间或者布局中提取块参照对象 </summary>
        private Dictionary<BlockReference, string> GetBlockRefs()
        {
            Dictionary<BlockReference, string> blockRefs;
            if (FromCurrentSpaceOrAllLayouts(_docMdf))
            {
                var succ = GetBlocksFromCurrentSpace(_docMdf, out blockRefs);
            }
            else
            {
                string blockName;
                var succ = GetString(_docMdf.acEditor, out blockName);
                if (!succ) return null;
                blockRefs = GetBlockRefsFromAllLayouts(blockName);
            }
            return blockRefs;
        }

        /// <summary> 提取所有块参照实例中，不重复的属性定义 </summary>
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

        private Dictionary<BlockReference, string> GetBlockRefsFromAllLayouts(string blockName)
        {
            var blockRefs = new Dictionary<BlockReference, string>();
            var lm = LayoutManager.Current;
            var id = _docMdf.acActiveDocument.Database.LayoutDictionaryId;
            var layouts = _docMdf.acTransaction.GetObject(id, OpenMode.ForRead) as DBDictionary;
            foreach (DBDictionaryEntry dde in layouts)
            {
                var layoutName = dde.Key;
                Layout lo = _docMdf.acTransaction.GetObject(dde.Value, OpenMode.ForRead) as Layout;
                // 不对模型空间进行处理
                if (lo.LayoutName == ACadConstants.LayoutName_Model) continue;
                //
                var btr = lo.BlockTableRecordId.GetObject(OpenMode.ForRead) as BlockTableRecord;
                // 遍历布局中的所有元素
                var entityIds = btr.Cast<ObjectId>();
                var blockRefs1 = entityIds.Where(r => r.ObjectClass.Name == "AcDbBlockReference");
                var blockRefs2 = blockRefs1.Cast<dynamic>();
                var blockRef = blockRefs2.FirstOrDefault(r => r.Name.ToUpper() == blockName.ToUpper());
                if (blockRef != null)
                {
                    blockRefs.Add(
                        key: ((ObjectId)blockRef).GetObject(OpenMode.ForRead) as BlockReference,
                        value: lo.LayoutName);
                }
            }
            return blockRefs;
        }

        #region ---   界面操作

        /// <summary> 选择多个块参照 </summary>
        /// <param name="docMdf"></param>
        /// <param name="blockRefs">选择的块参照</param>
        /// <returns></returns>
        private bool GetBlocksFromCurrentSpace(DocumentModifier docMdf, out Dictionary<BlockReference, string> blockRefs)
        {
            blockRefs = new Dictionary<BlockReference, string>();
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
                // 获取当前空间的名称
                string spaceName = "*当前空间";
                //
                var acSSet = res.Value;
                foreach (var id in acSSet.GetObjectIds())
                {
                    var e = id.GetObject(OpenMode.ForRead) as BlockReference;
                    if (e != null)
                    {
                        blockRefs.Add(e, spaceName);
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

        /// <summary> 从两个选项中选择一个 </summary>
        /// <param name="docMdf"></param>
        /// <returns>true 表示第一个选项（默认值），false 表示第二个选项</returns>
        private static bool FromCurrentSpaceOrAllLayouts(DocumentModifier docMdf)
        {
            var op = new PromptKeywordOptions(
                messageAndKeywords: "\n[当前空间(C) / 所有布局(L)]:",
                globalKeywords: "空间 布局"); // 默认值写在前面
            op.AllowArbitraryInput = false;
            op.AllowNone = true;
            var res = docMdf.acEditor.GetKeywords(op);
            if (res.Status == PromptStatus.OK)
            {
                // 非默认值
                if (res.StringResult == "布局")
                {
                    return false;
                }
            }
            return true; // true 表示第一个选项
        }

        /// <summary> 在命令行中获取一个字符 </summary>
        /// <param name="value">成功获得的数值</param>
        /// <returns>操作成功，则返回 true，操作失败或手动取消操作，则返回 false</returns>
        private static bool GetString(Editor ed, out string value)
        {
            value = "";
            var op = new PromptStringOptions(message: "\n要提取的块参照的名称")
            {
                AllowSpaces = false,
                UseDefaultValue = true
            };
            //
            var res = ed.GetString(op);
            if (res.Status == PromptStatus.OK)
            {
                value = res.StringResult;
                return true;
            }
            return false;
        }

        #endregion
    }
}