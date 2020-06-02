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
            Document doc = app.ActiveUIDocument.Document;
            if (!File.Exists(rfaPath))
            {
                message = "族不存在!";
                return Result.Failed;

            }
            Document familyDoc = doc.Application.OpenDocumentFile(rfaPath);
            FamilySymbol familySymbol=null;
            using (Transaction doctran = new Transaction(doc, "Load"))
            {
                doctran.Start();
                if (doc.LoadFamily(rfaPath, out Family family))
                {
                    doctran.Commit();
                    familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                }
                else
                {
                    familySymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Select(p => p as FamilySymbol).FirstOrDefault(p => p.Name == familyDoc.Title.Replace(".rfa", ""));
                }
                if (familySymbol != null)
                {
                    if (doctran.GetStatus()!=TransactionStatus.Started)
                    {
                        doctran.Start();
                    }
                    familySymbol.Activate();
                    RevitCommandId commandId = RevitCommandId.LookupCommandId("ID_OBJECTS_FAMSYM");
                    if (app.CanPostCommand(commandId))
                    {
                        app.PostCommand(commandId);
                    }
                    doctran.Commit();
                }
            }        
            return Result.Succeeded;
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
