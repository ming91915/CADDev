using System;
using AutoCADDev.Examples;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace AutoCADDev.Examples
{
    internal class CustomEntity : Entity
    {
        public CustomEntity(IntPtr unmanagedObjPtr, bool autoDelete) : base(unmanagedObjPtr, autoDelete)
        {
            var tr = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
            IntPtr pt = tr.UnmanagedObject;

        }

    }

    internal class CustomCurve : Curve
    {
        public CustomCurve(IntPtr unmanagedObjPtr, bool autoDelete) : base(unmanagedObjPtr, autoDelete)
        {
        }
    }
}
