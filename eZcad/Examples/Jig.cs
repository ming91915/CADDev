using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using eZcad;
using eZcad.Examples;
using Exception = System.Exception;

[assembly: CommandClass(typeof(eZcad.Examples.Jig))]

namespace eZcad.Examples
{
    class Jig
    {
        [CommandMethod("circleJig")]
        public static void CircleJig()
        {
            // Create a new instance of a circle we want to form with the jig 
            Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);

            // Create a new jig. 
            MyCircleJig jig = new MyCircleJig(circle);

            // Now loop for the inputs. 
            for (int i = 0; i <= 1; i++)
            {
                // Set the current input to the loop counter. ) 
                jig.CurrentInput = i;

                // Get the editor object. 
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                // Invoke the jig. 
                PromptResult promptResult = ed.Drag(jig);

                // Make sure the Status property of the PromptResult variable is ok.
                if (promptResult.Status == PromptStatus.Cancel | promptResult.Status == PromptStatus.Error)
                {
                    // some problem occured. Return 
                    return;
                }
            }

            // once we are finished with the jig, time to add the newly formed circle to the database 
            // get the working database 
            Database dwg = Application.DocumentManager.MdiActiveDocument.Database;
            // now start a transaction 
            Transaction trans = dwg.TransactionManager.StartTransaction();
            try
            {
                // open the current space for write 
                BlockTableRecord currentSpace =
                    (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);
                // add it to the current space 
                currentSpace.AppendEntity(circle);
                // tell the transaction manager about it 
                trans.AddNewlyCreatedDBObject(circle, true);

                // all ok, commit it 

                trans.Commit();
            }
            catch (Exception ex)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("Error CircleJig: " + ex.Message);
            }
            finally
            {
                // whatever happens we must dispose the transaction 

                trans.Dispose();
            }
        }
    }

    //一个Jig的例子
    class MyCircleJig : EntityJig
    {
        // We need two inputs for a circle, the center and the radius. 
        private Point3d centerPoint;
        private double radius;

        // Because we are going to have 2 inputs, a center point and a radius we need 
        // to keep track of the input number. 
        private int currentInputValue;

        // We will use a Property to get and set the variable created in step 3. 
        public int CurrentInput
        {
            get { return currentInputValue; }
            set { currentInputValue = value; }
        }


        // Create the default constructor. Pass in an Entity variable named ent. 
        // Derive from the base class and also pass in the ent passed into the constructor. 
        public MyCircleJig(Entity ent)
            : base(ent)
        {
        }

        // Override the Sampler function.
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            // Create a switch statement. 
            switch (currentInputValue)
            {
                // se 0 (zero) for the case. (getting center for the circle) 
                case 0:

                    Point3d oldPnt = centerPoint;

                    PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick center point : ");

                    // Check the status of the PromptPointResult 
                    if (jigPromptResult.Status == PromptStatus.OK)
                    {
                        // Make the centerPoint member variable equal to the Value 
                        // property of the PromptPointResult 
                        centerPoint = jigPromptResult.Value;

                        // Check to see if the cursor has moved. 
                        if ((oldPnt.DistanceTo(centerPoint) < 0.001))
                        {
                            // If we get here then there has not been any change to the location 
                            // return SamplerStatus.NoChange 
                            return SamplerStatus.NoChange;
                        }
                    }


                    // If the code gets here than there has been a change in the location so 
                    // return SamplerStatus.OK 
                    return SamplerStatus.OK;

                // Use 1 for the case. (getting radius for the circle) 
                case 1:

                    double oldRadius = radius;
                    JigPromptDistanceOptions jigPromptDistanceOpts = new JigPromptDistanceOptions("Pick radius : ");

                    jigPromptDistanceOpts.UseBasePoint = true;

                    jigPromptDistanceOpts.BasePoint = centerPoint;

                    // Now we ready to get input. 
                    PromptDoubleResult jigPromptDblResult = prompts.AcquireDistance(jigPromptDistanceOpts);


                    //  Check the status of the PromptDoubleResult 
                    if ((jigPromptDblResult.Status == PromptStatus.OK))
                    {
                        radius = jigPromptDblResult.Value;

                        // Check to see if the radius is too small  
                        if (Math.Abs(radius) < 0.1)
                        {
                            // Make the Member variable radius = to 1. This is 
                            // just an arbitrary value to keep the circle from being too small 
                            radius = 1;
                        }

                        // Check to see if the cursor has moved. 
                        if ((Math.Abs(oldRadius - radius) < 0.001))
                        {
                            // If we get here then there has not been any change to the location 
                            // Return SamplerStatus.NoChange 
                            return SamplerStatus.NoChange;
                        }
                    }

                    // If we get here the cursor has moved. return SamplerStatus.OK 
                    return SamplerStatus.OK;
            }
            // Return SamplerSataus.NoChange. This will not ever be hit as we are returning
            // in the switch statement. (just avoiding the compile error)
            return SamplerStatus.NoChange;
        }

        // Override the Update function. 
        protected override bool Update()
        {
            // In this function (Update) for every input, we need to update the entity 
            switch (currentInputValue)
            {
                // Use 0 (zero) for the case. (Updating center for the circle) 
                case 0:

                    // The jig stores the circle as an Entity type. 
                    ((Circle)Entity).Center = centerPoint;

                    // break out of the switch statement
                    break;

                // Use 1 for the case. (Updating radius for the circle) 
                case 1:

                    // The jig stores the circle as an Entity type. 
                    ((Circle)Entity).Radius = radius;

                    // break out of the switch statement
                    break;
            }
            // Return true. 
            return true;
        }
    }
}