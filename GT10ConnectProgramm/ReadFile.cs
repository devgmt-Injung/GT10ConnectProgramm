using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GT10ConnectProgramm
{
    internal class ReadFile
    {
        public ReadFile(string[] ipaddress, string hostname, string pwd)
        {
            int count = 0;
            for (int i = 0; i < ipaddress.Length; i++)
            {
                if (!ipaddress[i].Equals("존재하지않음."))
                {
                    count++;
                    string[] file_inform = Directory.GetFiles(@System.IO.Directory.GetCurrentDirectory(), ipaddress[i]);
                    Console.WriteLine(file_inform.Length);
                    if (file_inform.Length > 0)
                    {
                        File.Delete(System.IO.Directory.GetCurrentDirectory() + "\\" + ipaddress[i]);
                    }

                    var meth = new PasswordAuthenticationMethod(hostname, pwd);
                    ConnectionInfo myConnectioninfo = new ConnectionInfo(ipaddress[i], 22, hostname, meth);
                    myConnectioninfo.Encoding = System.Text.Encoding.Default;
                    string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER";
                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }
                    folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\GT10_" + (i + 1).ToString();
                    di = new DirectoryInfo(folderpath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }
                    using (var client = new SftpClient(myConnectioninfo))
                    {
                        client.Connect();
                        if (client.IsConnected == false) { return; }
                        var file = client.ListDirectory(@"/home/pi").FirstOrDefault(f => f.Name == "datainformation.txt");
                        if (file != null)
                        {
                            using (Stream fs = File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\DATAFOLDER\\GT10_" + (i + 1).ToString() + "\\", "datainformation.txt")))
                            {
                                client.DownloadFile(file.FullName, fs);
                            }
                        }
                        client.Disconnect();
                    }
                    string path = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\GT10_" + (i + 1).ToString() + "\\datainformation.txt";
                    string[] textValue = System.IO.File.ReadAllLines(path);
                    File.Delete(System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\GT10_" + (i + 1).ToString() + "\\datainformation.txt");
                    string[] filepath = Directory.GetFiles(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\GT10_" + (i + 1).ToString(), "*.csv");
                    for(int j = 0; j < filepath.Length; j++)
                    {
                        filepath[j] = filepath[j].Replace(System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\GT10_" + (i + 1).ToString()+"\\", "");
                    }
                    using (var client = new SftpClient(myConnectioninfo))
                    {
                        client.Connect();
                        if (client.IsConnected == false) { return; }
                        for (int j = 0; j < textValue.Length; j++)
                        {
                            if(Array.IndexOf(filepath, textValue[j]) == -1 || textValue[j].Equals(DateTime.Now.ToString("yyyy-MM-dd") + ".csv")) { 
                            var file = client.ListDirectory(@"/home/pi").FirstOrDefault(f => f.Name == textValue[j]);
                            if (file != null)
                            {
                                using (Stream fs = File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\DATAFOLDER\\GT10_" + (i + 1).ToString() + "\\", textValue[j])))
                                {
                                    client.DownloadFile(file.FullName, fs);
                                }
                            }
                            }

                        }
                        client.Disconnect();
                    }
                }
            }
        }

        public ReadFile()
        {

        }

        public string[] GetData(string name)
        {
            StreamReader sr = new StreamReader(name, System.Text.Encoding.GetEncoding(65001));
            string[] vs = { };
            var list = new List<string>();
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();
                string[] a = str.Split(',');
                list.Add(Path.GetFileNameWithoutExtension(name));
                list.AddRange(a);
            }
            vs = list.ToArray();
            return vs;
        }

        public string[] GetData_Line(string name)
        {
            StreamReader sr = new StreamReader(name, System.Text.Encoding.GetEncoding(65001));
            string[] vs = { };
            var list = new List<string>();
            while (!sr.EndOfStream)
            {
                string str = Path.GetFileNameWithoutExtension(name) + "," + sr.ReadLine();
                list.Add(str);
            }
            vs = list.ToArray();
            return vs;
        }
    }
}
