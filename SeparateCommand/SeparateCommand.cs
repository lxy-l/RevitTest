using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SeparateCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string msg, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            string temFilePath = @"G:/temFile.rvt";
            string temFilePath2 = @"G:/temFile2.rvt";
            Document temDoc = uiapp.Application.NewProjectDocument(temFilePath);

            temDoc.SaveAs(temFilePath2);
            temDoc.Close(false);
            uiapp.OpenAndActivateDocument(temFilePath);
            temDoc = uiapp.ActiveUIDocument.Document;

            //关闭本地文件
            string docPathName = doc.PathName;
            ModelPath modelPath = doc.GetWorksharingCentralModelPath();
            doc.Close(false);

            //分离模型
            OpenOptions openOptions = new OpenOptions
            {
                
                DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets
            };
            Document detachDoc = uiapp.Application.OpenDocumentFile(modelPath, openOptions);
            detachDoc.SaveAs(@"");
            detachDoc.Close(false);

            //重新打开本地文件

            uiapp.OpenAndActivateDocument(docPathName);

            //清理临时文档
            temDoc.Close(false);

            return Result.Succeeded;
        }
    }
}
