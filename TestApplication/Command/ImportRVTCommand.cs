using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ImportRVTCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择文件";
            dialog.Filter = "加密项目文件(*.rt)|*.rt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                string path = HandleRvtService.DownloadRvt("1");
                //Document detachDoc = uiapp.Application.OpenDocumentFile(path);
                uiapp.OpenAndActivateDocument(path);
            }
            return Result.Succeeded;
        }
    }
}
