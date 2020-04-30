using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    public sealed class HandleRvtService
    {
        /// <summary>
        /// 项目文件上传
        /// </summary>
        /// <returns></returns>
        public static bool UploadRVT(string path, string title)
        {
            string newFile = $@"{Path.GetDirectoryName(path)}{title}_key.rvt";
            try
            {
               
                File.Copy(path, newFile, true);
                RestClient client = new RestClient("http://localhost:64416");
                RestRequest request = new RestRequest("Home/UploadRVT", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddFile("file", newFile);
                request.AddParameter("fileName", title);
                IRestResponse res = client.Execute(request);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(res.Content))
                    {
                        return true;
                    }
                   
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
            }
        }

        /// <summary>
        /// 下载项目文件
        /// </summary>
        /// <param name="rvtId"></param>
        public static string DownloadRvt(string rvtId)
        {
            try
            {
                RestClient client = new RestClient("http://localhost:64416");
                RestRequest request = new RestRequest("Home/DownloadRvt", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddParameter("rvtId", rvtId);
                IRestResponse res = client.Execute(request);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    RvtFileBytes retValue = JsonConvert.DeserializeObject<RvtFileBytes>(res.Content);
                    byte[] buffer = retValue.Buffer;
                    if (buffer != null)
                    {
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
                        path = $@"{path}\{retValue.ProjectName}.rvt";
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            fs.Write(buffer, 0, buffer.Length);
                            fs.Flush();
                        }
                        FileInfo fi = new FileInfo(path);
                        if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        {
                            File.SetAttributes(path, fi.Attributes | FileAttributes.Hidden);
                        }
                        return path;
                    }

                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
