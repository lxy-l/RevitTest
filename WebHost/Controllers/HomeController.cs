using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WebHost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadRVT(HttpPostedFileBase file,string fileName)
        {
            string guid = "";
            try
            {
                string path =$@"{Server.MapPath("/Upload/")}";
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
               guid = Guid.NewGuid().ToString();
                file.SaveAs($"{path}{guid}.rvt");

            }
            catch (Exception)
            {
                return Content("");
            }
           
            return Content(guid);
        }

        [HttpPost]
        public string DownloadRvt(string rvtId)
        {
            string path = $@"{Server.MapPath("/Upload/")}{rvtId}.rvt";
            FileStream fsr = new FileStream(path, FileMode.Open);
            byte[] readBytes = new byte[fsr.Length];
            fsr.Read(readBytes, 0, readBytes.Length);     
            fsr.Close();
            JavaScriptSerializer json = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };
            return json.Serialize(new RvtFileBytes{ProjectName="Name", Buffer=readBytes });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}