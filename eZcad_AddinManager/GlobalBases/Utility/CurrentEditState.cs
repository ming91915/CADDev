using System;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows.Data;

namespace eZcad.Utility
{
    /// <summary> 用来获取AutoCAD界面中的编辑状态 </summary>
    public class EditStateIdentifier
    {
        public static CurrentEditState GetCurrentEditState(DocumentModifier docMdf)
        {
            var blkTb = docMdf.acTransaction.GetObject(docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            EditState state; ;
            if (docMdf.acDataBase.TileMode)
            {
                // Model布局是活动的，且你正工作在ModelSpace（可能是文档模型空间，也可能是块编辑器或块的在位编辑空间）

                state = EditState.InModel;
                var btr = docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                // 判断是否在块编辑器中编辑某一个对象
                var be = new BlockEditor();
                bool inBlockEditorMode = be.IsInBlockEditor; // 当前界面是否位于块编辑模式
                if (inBlockEditorMode)
                {
                    var btrName = be.BlockName; // 当前正在编辑的块定义的名称
                    state = EditState.InBlockEditor;
                }
                return new CurrentEditState(blkTb, btr, state);
            }
            else
            {
                var nCurVport = (Int16)(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CVPORT"));
                if (nCurVport == 2)
                {
                    // Model之外的布局是活动的，且你正工作在浮动视口。
                    state = EditState.InViewport;
                    var btr = docMdf.acTransaction.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    return new CurrentEditState(blkTb, btr, state);
                }
                else
                {
                    // Model之外的布局是活动的，且你正工作在图纸空间。
                    state = EditState.InLayout;
                    var layouts = docMdf.acDataBase.LayoutDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;
                    var lo = docMdf.acTransaction.GetObject(layouts.GetAt(LayoutManager.Current.CurrentLayout), OpenMode.ForRead) as Layout;
                    var btr = docMdf.acTransaction.GetObject(lo.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    return new CurrentEditState(blkTb, btr, state);
                }
            }
        }

        /// <summary> 用户当前的编辑状态 </summary>
        public class CurrentEditState
        {
            /// <summary> 所有<seealso cref="BlockTableRecord"/>对象的集合 </summary>
            public readonly BlockTable BlockTable;

            /// <summary> 用户当前正在编辑状态 </summary>
            public readonly EditState EditState;
            /// <summary> 用户当前正在编辑的块定义 </summary>
            public readonly BlockTableRecord CurrentBTR;

            /// <summary> 构造函数 </summary>
            public CurrentEditState(BlockTable bt, BlockTableRecord currentBtr, EditState editState)
            {
                CurrentBTR = currentBtr;
                EditState = editState;
                //
                BlockTable = bt;
            }

            /// <summary> 退出块编辑模式 </summary>
            /// <param name="doc"></param>
            public void ExitBlockEditor(Document doc)
            {
                if (EditState == EditState.InBlockEditor)
                {
                    doc.SendStringToExecute("BCLOSE\n", true, false, false);
                }
            }
        }

        /// <summary> AutoCAD界面中的编辑状态 </summary>
        public enum EditState
        {
            /// <summary> 正在模型空间进行操作，同时包括视口编辑模式、块定义的编辑器模式，以及块定义的在位编辑模式 </summary>
            InModel = 1,
            /// <summary> 正在布局空间进行操作 </summary>
            InLayout = 2,
            /// <summary> 正在块编辑器中对某个块定义进行操作，此时的坐标原点为块定义的基点 </summary>
            InBlockEditor = InModel + 4,
            /// <summary> 正在块的在位编辑状态中对某个块定义进行操作，此时的坐标原点为文档模型的原点 </summary>
            InRefEditor = InModel + 8,
            /// <summary> 正在模型空间或者布局空间的视口中进行操作 </summary>
            InViewport = InModel + 16,
        }

    }
}