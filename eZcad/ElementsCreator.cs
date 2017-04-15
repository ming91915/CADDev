using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;



namespace AutoCADDev
{

    public class ElementsCreator : IDisposable
    {
        public Transaction acTransaction;
        public Document acActiveDocument;
        public Database acDataBase;
        private DocumentLock acLock;

        public ElementsCreator()
        {

            // 获得当前文档和数据库   Get the current document and database
            acActiveDocument = Application.DocumentManager.MdiActiveDocument;
            acDataBase = acActiveDocument.Database;
            //
            acLock = acActiveDocument.LockDocument();
            acTransaction = acDataBase.TransactionManager.StartTransaction();
        }

        #region IDisposable Support
        private bool valuesDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!valuesDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (acLock != null)
                    {
                        acLock.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                valuesDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ElementsCreator() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion



    }
}



