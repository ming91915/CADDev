using System;
using System.IO;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCADDev.Utility
{
    /// <summary> 对文档进行配置，以启动文档的改写模式 </summary>
    public class DocumentModifier : IDisposable
    {
        #region ---   fields

        public  Transaction acTransaction;
        /// <summary> 当前活动的AutoCAD文档 </summary>
        public readonly Document acActiveDocument;
        /// <summary> 当前活动的AutoCAD文档中的数据库 </summary>
        public readonly Database acDataBase;

        /// <summary>  </summary>
        public readonly Editor acEditor;

        private readonly DocumentLock acLock;

        //
        private readonly bool _openDebugerText;
        private readonly StringBuilder _debugerSb;

        #endregion

        #region ---   构造函数
        /// <summary> 对文档进行配置，以启动文档的改写模式 </summary>
        /// <param name="openDebugerText">是否要打开一个文本调试器</param>
        public DocumentModifier(bool openDebugerText)
        {
            _openDebugerText = openDebugerText;

            // 获得当前文档和数据库   Get the current document and database
            acActiveDocument = Application.DocumentManager.MdiActiveDocument;
            acDataBase = acActiveDocument.Database;
            acEditor = acActiveDocument.Editor;

            //
            acLock = acActiveDocument.LockDocument();
            acTransaction = acDataBase.TransactionManager.StartTransaction();
            if (openDebugerText)
            {
                _debugerSb = new StringBuilder();
            }
        }
        #endregion

        #region IDisposable Support

        private bool valuesDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!valuesDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    acTransaction.Dispose();
                    acLock.Dispose();

                    // 写入调试信息 并 关闭文本调试器
                    if (_openDebugerText && _debugerSb.Length > 0)
                    {
                        ShowDebugerInfo();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                valuesDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DocumentModifier()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region --- Debuger Info

        /// <summary> 向文本调试器中写入数据 </summary>
        /// <param name="value"></param>
        public void WriteLineIntoDebuger(params string[] value)
        {
            if (_openDebugerText)
            {
                _debugerSb.Append(value[0]);
                for (int i = 1; i < value.Length; i++)
                {
                    _debugerSb.Append($", {value[i]}");
                }

                _debugerSb.AppendLine();
            }
        }

        /// <summary> 向文本调试器中写入多行数据 </summary>
        /// <param name="lines"></param>
        public void WriteLinesIntoDebuger(params string[] lines)
        {
            if (_openDebugerText)
            {
                foreach (var s in lines)
                {
                    _debugerSb.AppendLine(s);
                }
            }
        }
        /// <summary> 实时显示调试信息 </summary>
        /// <param name="value"></param>
        public void WriteNow(params string[] value)
        {
            var sb = new StringBuilder();
            sb.Append(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                sb.Append($", {value[i]}");
            }
            sb.AppendLine();
            acEditor.WriteMessage(sb.ToString());
        }

        /// <summary> 实时显示调试信息 </summary>
        /// <param name="value"></param>
        public void WriteNow(params double[] value)
        {
            var sb = new StringBuilder();
            sb.Append(value[0]);
            for (int i = 1; i < value.Length; i++)
            {
                sb.Append($", {value[i]}");
            }
            acEditor.WriteMessage(sb.ToString());
        }

        private void ShowDebugerInfo()
        {
            if (_openDebugerText)
            {
                acEditor.WriteMessage("\n------------------------- AddinManager 调试信息 ---------------------\n");
                acEditor.WriteMessage(_debugerSb.ToString());
                //
                _debugerSb.Clear();
            }
        }

        #endregion
    }
}