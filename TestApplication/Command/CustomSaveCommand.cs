using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestApplication
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CustomSaveCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
           
            if (doc.IsFamilyDocument)
            {
                message = "仅针对于项目文件进行保存！";
                return Result.Failed;
            }
            else
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> collection = collector.OfClass(typeof(FamilySymbol)).ToElements();
                bool IsTrue = false;
                foreach (FamilySymbol item in from FamilySymbol item in collection
                                              from Parameter item2 in item.Parameters
                                              where item2.AsString() == "有限公司" && item2.Definition.Name == "版权所有者"
                                              select item)
                {
                    IsTrue = true;
                    break;
                }
                if (IsTrue)
                {
                    string oldPath = doc.PathName;
                    string path = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\temp\";
                    
                    DirectoryInfo dir = new DirectoryInfo(path);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                    if (!dir.Attributes.Equals(FileAttributes.Hidden | FileAttributes.Directory))
                    {
                        dir.LastWriteTime = DateTime.Now;
                        dir.LastAccessTime = DateTime.Now;
                        File.SetAttributes(path, dir.Attributes | FileAttributes.Hidden);
                    }
                    
                    path = $"{path}{doc.Title}.rvt";
                    Application.IsSave = true;

                    doc.SaveAs(path, new SaveAsOptions
                    {
                        Compact = true,
                        OverwriteExistingFile = true,
                        MaximumBackups = 1
                    });

                    FileInfo fi = new FileInfo(path);
                    if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        File.SetAttributes(path, fi.Attributes | FileAttributes.Hidden);
                    }
                   
                   
                    if (HandleRvtService.UploadRVT(path,doc.Title))
                    {
                        if (!oldPath.Equals(path))
                        {
                            File.Delete(oldPath);
                        }
                    }
                    else
                    {
                        message="项目加密保存出错，请检查网络！";
                        return Result.Failed;
                    }
                    return Result.Succeeded;
                }
                else
                {
                    message = "此项目非族库保护项目！";
                    return Result.Failed;
                }

            }
        }
    }
}
