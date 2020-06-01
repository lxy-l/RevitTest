using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace ImportRVTCommand
{
    /// <summary>
    /// 清洗族文件参数
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ClearFamilyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialogResult result = TaskDialog.Show("Revit", "是否清洗载入族的公式?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No, TaskDialogResult.Yes);
            if (result == TaskDialogResult.Yes)
            {

                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> collection = collector.OfClass(typeof(FamilySymbol)).ToElements();
                FamilyManager familyManger = null;
                Document familyDoc = null;
                foreach (FamilySymbol item in from FamilySymbol item in collection
                                              from Parameter item2 in item.Parameters
                                              where item2.AsString() == "有限公司" && item2.Definition.Name == "版权所有者"
                                              select item)
                {
                    
                    familyDoc = doc.EditFamily(item.Family);
                    using (Transaction trna = new Transaction(familyDoc, "Remove"))
                    {
                        trna.Start();
                        familyManger = familyDoc.FamilyManager;
                        //不同规格族参数清理
                        foreach (FamilyType type in familyManger.Types)
                        {
                            familyManger.CurrentType = type;
                            foreach (FamilyParameter param in from FamilyParameter param in
                                                      from FamilyParameter param in familyManger.Parameters
                                                      where !string.IsNullOrEmpty(param.Formula)
                                                      select param
                                                  where param.Definition.Name != "版权所有者"
                                                  select param)
                            {
                                familyManger.SetFormula(param, null);
                            }
                        }
                        trna.Commit();
                    }
                    // 将这些修改重新载入到工程文档中(不重载参数) 
                    _ = familyDoc?.LoadFamily(doc, new projectFamLoadOption());

                }
                TaskDialog.Show("Revit", "清洗已完成");


            }
            return Result.Succeeded;

        }
        /// <summary>
        /// 重载
        /// </summary>
        private class projectFamLoadOption : IFamilyLoadOptions
        {
            bool IFamilyLoadOptions.OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                //参数重载
                overwriteParameterValues = false;
                return true;
            }
            bool IFamilyLoadOptions.OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Project;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}
