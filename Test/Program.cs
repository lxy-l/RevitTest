using System;
using System.IO;
using System.Security.Cryptography;

namespace Test
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //加密文件输出路径
            //string outpath = "";
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Title = "请选择文件";
            //dialog.Filter = "项目文件(*.rvt)|*.rvt";
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
                UploadService.UploadRVT(@"C:\Users\lxysa\Desktop\RevitSaveTest\TestApplication\bin\Debug\temp\1.rvt");
            //}


        }
    }
}
