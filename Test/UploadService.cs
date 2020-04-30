using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public sealed class UploadService
    {
        /// <summary>
        /// 项目文件上传
        /// </summary>
        /// <returns></returns>
        public static bool UploadRVT(string path)
        {

            try
            {
                string newFile = $@"C:\Users\lxysa\Desktop\RevitSaveTest\TestApplication\bin\Debug\temp\{Guid.NewGuid()}.rvt";
                File.Copy(path, newFile,true);
                RestClient client2 = new RestClient("http://localhost:64416");
                RestRequest request = new RestRequest("Home/UploadRVT", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddFile("file", newFile);
                IRestResponse res = client2.Execute(request);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    if (res.Content == "OK")
                    {
                        return true;
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
    }
}
