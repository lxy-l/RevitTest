using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ImportRVTCommand
{
    /// <summary>
    /// 加载族外部事件
    /// </summary>
    public class LoadRfaEvent : IExternalEventHandler
    {
        //Logo族路径
        private string paramPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\swiftshader\ReadOnlyParameters.txt";
        private string Logopath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\logo.rfa";
        private string targetPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\swiftshader\logo.rfa";
        //族自定义参数
        private static List<RafModel> FamilyRfas { get; set; }
        //族文件路径
        public static string Path { get; set; }
        public LoadRfaEvent() { }
        public LoadRfaEvent(List<RafModel> rafs, string path)
        {
            FamilyRfas = rafs;
            Path = path;
        }

        public void Execute(UIApplication app)
        {
            Document uidoc = app.ActiveUIDocument.Document;
            Autodesk.Revit.ApplicationServices.Application  application = app.Application;
            Document familyDoc =null;
            try
            {
                familyDoc = uidoc.Application.OpenDocumentFile(Path);
                FamilyManager familyManager = familyDoc.FamilyManager;
                using (Transaction doctran = new Transaction(uidoc, "Load"))
                {
                    doctran.Start();
                    //载入族
                    if (uidoc.LoadFamily(Path, out Family family))
                    {
                        //加载旧logo族
                        File.Copy(targetPath, Logopath, true);
                        using (Transaction familydoctran = new Transaction(familyDoc, "Add"))
                        {

                            familydoctran.Start();
                            //族logoSymbol对象获取
                            if (!familyDoc.LoadFamilySymbol(Logopath, "logo", out FamilySymbol logoSymbol))
                            {
                                TaskDialog.Show("Revit", "Logo族文件未找到！");
                                familydoctran.RollBack();
                                doctran.RollBack();
                            }
                            else
                            {
                                //老参数列表
                                IList<string> ParamesList = new List<string>();
                                foreach (FamilyParameter item in familyManager.Parameters)
                                {
                                    ParamesList.Add(item.Definition.Name);
                                }
                                //标识参数
                                if (!ParamesList.Contains("版权所有者") && !ParamesList.Contains("声明") && !ParamesList.Contains("公司官网"))
                                {
                                    application.SharedParametersFilename =paramPath;
                                    DefinitionFile definitionFile = application.OpenSharedParameterFile();
                                    DefinitionGroup shareGroup = definitionFile.Groups.get_Item("ParamGroup");
                                    ExternalDefinition definition = shareGroup.Definitions.get_Item("版权所有者") as ExternalDefinition;
                                    ExternalDefinition definition2 = shareGroup.Definitions.get_Item("声明") as ExternalDefinition;
                                    ExternalDefinition definition3 = shareGroup.Definitions.get_Item("公司官网") as ExternalDefinition;
                                    FamilyParameter parameter = familyManager.AddParameter(definition, BuiltInParameterGroup.PG_IDENTITY_DATA, false);
                                    FamilyParameter parameter2 = familyManager.AddParameter(definition2, BuiltInParameterGroup.PG_IDENTITY_DATA, false);
                                    FamilyParameter parameter3 = familyManager.AddParameter(definition3, BuiltInParameterGroup.PG_IDENTITY_DATA, false);
                                   
                                    foreach (FamilyType familyType in familyManager.Types)
                                    {
                                        familyManager.CurrentType = familyType;
                                        familyManager.Set(parameter, "有限公司");
                                        familyManager.Set(parameter2, "仅供内部生产学习使用，违者必究");
                                        familyManager.Set (parameter3, "www..com");
                                    }
                                }
                                //自定义参数
                                if (FamilyRfas != null)
                                {
                                    foreach (RafModel item in FamilyRfas)
                                    {
                                        if (!ParamesList.Contains(item.name))
                                        {
                                            FamilyParameter parameter = familyManager.AddParameter(item.name, BuiltInParameterGroup.PG_GENERAL, (ParameterType)Enum.Parse(typeof(ParameterType), item.type.ToString()), item.familyType);
                                            foreach (FamilyType familyType in familyManager.Types)
                                            {
                                                familyManager.CurrentType = familyType;
                                                if (!string.IsNullOrEmpty(item.defaultV))
                                                {
                                                    switch ((ParameterType)Enum.Parse(typeof(ParameterType), item.type.ToString()))
                                                    {
                                                        case ParameterType.Text:
                                                            familyManager.Set(parameter, item.defaultV);
                                                            break;
                                                        case ParameterType.Integer:
                                                            familyManager.Set(parameter, int.Parse(item.defaultV));
                                                            break;
                                                        case ParameterType.Number:
                                                            familyManager.Set(parameter, double.Parse(item.defaultV));
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //激活logo对象
                                logoSymbol.Activate();
                                //载入logo实例
                                familyDoc.FamilyCreate.NewFamilyInstance(new XYZ(-1, 2, 0), logoSymbol, Autodesk.Revit.DB.Structure.StructuralType.Footing);
                                //提交
                                familydoctran.Commit();
                                doctran.Commit();
                                //重载                                
                                Family loadedFamily = familyDoc.LoadFamily(uidoc, new projectFamLoadOption());
                                TaskDialog.Show("Revit", "族文件及参数成功载入");
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Revit", "不能重复载入！");
                        //MessageBox.Show("不能重复载入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        doctran.RollBack();
                    }
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit", $"载入异常：{e.Message}");
            }
            finally
            {
                //释放资源
                familyDoc?.Close(false);
                familyDoc?.Dispose();
                //删除族文件
                File.Delete(Path);
            }
        }

        public string GetName()
        {
            return "LoadRfaEvent";
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
