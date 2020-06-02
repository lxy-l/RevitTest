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
using System.Windows.Forms;

namespace PlaceRfaCommand
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        private static string rfaPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\爱奥尼柱.rfa";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            //if (!File.Exists(rfaPath))
            //{
            //    message = "族不存在!";
            //    return Result.Failed;

            //}
            Document familyDoc = null;
            FamilySymbol familySymbol = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择文件";
            dialog.Filter = "族文件(*.rfa)|*.rfa";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                rfaPath = dialog.FileName;
                familyDoc = doc.Application.OpenDocumentFile(dialog.FileName);
                using (Transaction doctran = new Transaction(doc, "Load"))
                {
                    doctran.Start();
                    if (doc.LoadFamily(rfaPath, out Family family))
                    {
                        doctran.Commit();
                        var ids = family.GetTypeId();
                        familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                    }
                    else
                    {
                        familySymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                            .Select(p => p as FamilySymbol)
                            .FirstOrDefault(p => p.Name == familyDoc.Title.Replace(".rfa", ""));
                    }
                    //&&familySymbol.Category.CategoryType!= CategoryType.Annotation
                    if (familySymbol != null)
                    {
                        if (doctran.GetStatus() != TransactionStatus.Started)
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
