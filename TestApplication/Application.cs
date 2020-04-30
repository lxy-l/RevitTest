using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Application : IExternalApplication
    {

      

        public static bool IsSave { get; set; }
        public static PushButton TryButton { get; set; }//自定义保存
        public static PushButton ImportButton { get; set; }//导入

        private static readonly string AddInPath = typeof(Application).Assembly.Location;

        private static AddInCommandBinding SaveAsProjectEvent { get; set; }
        private static AddInCommandBinding SaveingEvent { get; set; }


        public Result OnShutdown(UIControlledApplication application)
        {

            SaveAsProjectEvent.BeforeExecuted -= Binding_BeforeExecuted;
            SaveingEvent.BeforeExecuted -= Binding_BeforeExecuted;
            application.ControlledApplication.DocumentSavingAs -= ControlledApplication_DocumentSavingAs;
            application.ControlledApplication.DocumentClosing -= ControlledApplication_DocumentClosing;
            application.ControlledApplication.DocumentClosed -=ControlledApplication_DocumentClosed;
            CleanTemp();
            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
           
            application.CreateRibbonTab("Test");
            RibbonPanel tryPanel = application.CreateRibbonPanel("Test", "Try");
            PushButtonData saveBtnData = new PushButtonData("Save", "自定义保存", AddInPath, "TestApplication.CustomSaveCommand");
            PushButtonData importBtnData = new PushButtonData("Import", "导入", AddInPath, "TestApplication.ImportRVTCommand");
            TryButton = tryPanel.AddItem(saveBtnData) as PushButton;
            ImportButton = tryPanel.AddItem(importBtnData) as PushButton;
            //注册事件
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsProject));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsLibraryFamily));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsLibraryGroup));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsLibraryView));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveSelection));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveAsProjectEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsTemplate));
            SaveAsProjectEvent.BeforeExecuted += Binding_BeforeExecuted;
            SaveingEvent = application.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.Save));
            SaveingEvent.BeforeExecuted += Binding_BeforeExecuted;

            application.ControlledApplication.DocumentSavingAs += ControlledApplication_DocumentSavingAs;
            application.ControlledApplication.DocumentClosing += ControlledApplication_DocumentClosing;
            application.ControlledApplication.DocumentClosed += ControlledApplication_DocumentClosed;
            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            Document doc = e.Document;
            if (!doc.IsFamilyDocument)
            {
                if (CheckFamily(doc, out string msg))
                {
                    if (!HandleRvtService.UploadRVT(doc.PathName, doc.Title))
                    {
                        e.Cancel();
                        TaskDialog.Show("Revit", "项目未保存，请检查！");
                    }

                }
            }   
        }

        private static void CleanTemp()
        {

            string path = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\temp\";
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                try
                {
                    dir.Delete(true);
                }
                catch (Exception) { }

            }
        }
        private void ControlledApplication_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e) => CleanTemp();

        private void ControlledApplication_DocumentSavingAs(object sender, Autodesk.Revit.DB.Events.DocumentSavingAsEventArgs e)
        {
            if (IsSave)
            {
                IsSave = false;
            }
            else
            {
                if (CheckFamily(e.Document,out string msg))
                {
                    e.Cancel();
                    TaskDialog.Show("Revit", msg);
                }
            }
        }

        private static bool CheckFamily(Document doc,out string msg)
        {

            if (doc.IsFamilyDocument)
            {
                FamilyManager familyManager = doc.FamilyManager;
                var Copy = familyManager.get_Parameter("版权所有者");
                var Declare = familyManager.get_Parameter("声明");
                var Company = familyManager.get_Parameter("公司官网");
                msg = "非法保存！";
                if (Copy != null && Declare != null && Company != null)
                    return true;
                return false;
            }

            else
            {
                msg = "项目文件受保护，请到族库自定义保存！";
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> collection = collector.OfClass(typeof(FamilySymbol)).ToElements();
                foreach (FamilySymbol item in from FamilySymbol item in collection
                                              from Parameter item2 in item.Parameters
                                              where item2.AsString() == "天津安捷物联科技股份有限公司" && item2.Definition.Name == "版权所有者"
                                              select item)
                {
                    return true;
                }
                return false;
            }

        }

        private void Binding_BeforeExecuted(object sender, Autodesk.Revit.UI.Events.BeforeExecutedEventArgs e)
        {
            if (IsSave)
            {
                IsSave = false;
            }
            else
            {
                if (CheckFamily(e.ActiveDocument,out string msg))
                {
                    e.Cancel = true;
                    TaskDialog.Show("Revit", msg);
                }
            }
        }
    }
}
