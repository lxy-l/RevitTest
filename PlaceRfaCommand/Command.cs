using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaceRfaCommand
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        private readonly static string rfaPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\爱奥尼柱.rfa";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Document uidoc = app.ActiveUIDocument.Document;
            if (!File.Exists(rfaPath))
            {
                message = "族不存在!";
                return Result.Failed;

            }
            Document familyDoc = uidoc.Application.OpenDocumentFile(rfaPath);
            using (Transaction doctran = new Transaction(uidoc, "Load"))
            {
                doctran.Start();
                if (uidoc.LoadFamily(rfaPath, out Family family))
                {
                    FamilySymbol familySymbol = uidoc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                    familySymbol.Activate();
                    try
                    {
                        RevitCommandId commandId = RevitCommandId.LookupCommandId("ID_OBJECTS_FAMSYM");
                        if (app.CanPostCommand(commandId))
                        {
                            app.PostCommand(commandId);
                        }
                        doctran.Commit();
                        return Result.Succeeded;
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        message = "已取消放置";
                        return Result.Cancelled;
                    }
                }
                else
                {
                    FamilySymbol familySymbol = new FilteredElementCollector(uidoc).OfClass(typeof(FamilySymbol)).Select(p => p as FamilySymbol).FirstOrDefault(p => p.Name == familyDoc.Title.Replace(".rfa",""));
                    if (familySymbol==null)
                    {
                        message = "族没有实例！";
                        return Result.Failed;
                    }

                    familySymbol.Activate();
                    try
                    {
                        RevitCommandId commandId = RevitCommandId.LookupCommandId("ID_OBJECTS_FAMSYM");
                        if (app.CanPostCommand(commandId))
                        {
                            app.PostCommand(commandId);
                        }
                        doctran.Commit();
                        return Result.Succeeded;
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        message = "已取消放置";
                        return Result.Cancelled;
                    }

                }

            }
        }

        private static bool PlaceRfa()
        {

        }

        private class projectFamLoadOption : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Project;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}
