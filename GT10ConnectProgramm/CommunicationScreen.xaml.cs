using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GT10ConnectProgramm
{
    /// <summary>
    /// ConnectOption.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommunicationScreen : Window
    {
        string information; // Thread_DoWork 함수 내에서 어떤 동작을 할지 결정하는 Command 역할을 하는 information 변수
        private BackgroundWorker loadingThread; // BackgroundWorker 객체 생성
        int[] checkstate = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // (IP통신)인증기 체크박스10개의 상태를 담을 배열
        string[] correctIPAddress = new string[10]; //현재 연결된 IP번호를 담을 배열
        string[] correctUSBConnect = new string[10]; // 현재 연결된 COM포트 번호를 담을 배열
        string[] GT10_Name = new string[10]; // 인증기 이름을 전달받아 저장할 배열
        int[] checkUSBState = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // (USB통신)인증기 체크박스10개의 상태를 담을 배열
        int[] dataSpeed = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 문자열 크기를 지정
        string port = ""; // 포트가 정상적으로 동작하는지 결과를 저장할 문자열 변수
        string[] ipAddress_SSH = new string[10]; // 현재 연결된 인증기중 본인이 사용할 인증기 IP번호만 담고있는 배열
        string[] correctUSBstate = new string[10]; // 현재 연결된 인증기중 본인이 사용할 인증기 포트명을 담고있는 배열
        int stopState_USB = 0; // USB 통신이 올바르지 못하여 문제가 생겼을 경우 자동으로 포트 종료후 메세지 창을 출력하는것을 제어할 int형 변수
        string receivedUSBData; // 포트를 통해 받은 데이터를 저장.
        SerialPort serialPort; // 시리얼 통신 퐅트 객체 생성

        // BackGroundWorker Thread 설정 및 생성
        private void initBW()
        {
            //BackgroundWorker Thread 실행
            loadingThread = new BackgroundWorker()
            {
                WorkerReportsProgress = true, //진행 상황 보고 여부 설정 
                WorkerSupportsCancellation = true //진행중 동작 취소 지원 여부 설정
            };
            loadingThread.DoWork += Thread_DoWork; // Thread_DoWork함수를 수행함.

            loadingThread.ProgressChanged += Thread_ProgressChanged; // BackGroundWorker Thread 진행 변경시 Thread_ProgressChanged 함수 수행 (이 프로젝트에선 고려 X)

            loadingThread.RunWorkerCompleted += Thread_RunWorkerCompleted; // BackGroundWorker Thread의 Thread_DoWork함수 수행이 끝났을 때 Thread_RunWorkerCompleted함수를 실행하고 종료
        }

        private void Thread_DoWork(object sender, DoWorkEventArgs e)
        {
            //IP통신상태 체크 기능
            if (information.Equals("인증기통신"))
            {
                string[] ipaddress = new string[10];
                for (int i = 0; i < ipaddress.Length; i++)
                {
                    if (i != 9)
                    {
                        ipaddress[i] = "169.254.7." + (i + 1).ToString();
                    }
                    else
                    {
                        ipaddress[i] = "169.254.7." + (i + 2).ToString();
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (loadingThread.CancellationPending) // 취소 명령시 Thread_DoWork 종료
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (ERROR_FLAG == true) // 에러 발생시 throw 명령으로 Thread_DoWork 종료
                    {
                        throw new Exception("ErrorRaised");
                    }
                    CommandPrompt commandPrompt = new CommandPrompt(); // CommandPrompt 클래스 객체 생성(CMD명령어 수행)
                    string cmdOutput = commandPrompt.InputPingCommand(ipaddress[i]); // Ping -w 1 -n 1 ipaddress[i] 명령 수행 및 출력 결과를 저장
                    ConnectPing connectPing = new ConnectPing(); // ConnectPing 클래스 객체 생성(출력 결과값으로 정상적으로 통신이 되는지 체크)
                    string result = connectPing.Check_GetCorrectIP(ipaddress[i], cmdOutput); // 통신 상태 체크 결과를 저장 (연결 실패 or 연결 성공 or 올바른 접근 ip가 아닙니다.)
                    if (result.Equals("연결 실패"))
                    {

                    }
                    else if (result.Equals("올바른 접근 ip가 아닙니다."))
                    {

                    }
                    else
                    {
                        // 정상적으로 통신이 될때 ipaddress[i] 를 correctIPAddress[i]에 저장
                        correctIPAddress[i] = ipaddress[i];

                    }
                }
            }
            //IP통신으로 데이터를 읽어들여 저장하는 기능
            else if (information.Equals("데이터읽기"))
            {
                ReadFile classFor = new ReadFile(ipAddress_SSH, "pi", "raspberry"); // ReadFile 클래스 객체 생성(sftp 사용하여 라즈베리파이 에서 파일을 불러와 저장함.)
                if (loadingThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (ERROR_FLAG == true)
                {
                    throw new Exception("ErrorRaised");
                }
            }
            //인증기와 PC가 통신이 가능한 USB 포트를 체크
            else if (information.Equals("USB통신상태체크"))
            {
                string[] portnames = SerialPort.GetPortNames(); // 현재 PC에 할당된 포트목록을 불러옴.
                port = ""; // 포트 동작여부 초기화
                for (int i = 0; i < portnames.Length; i++)
                {
                    // 해당 포트로 시리얼 통신 설정 및 수행
                    serialPort = new SerialPort();
                    serialPort.PortName = portnames[i];
                    serialPort.BaudRate = (int)115200;
                    serialPort.DataBits = (int)8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Encoding = System.Text.Encoding.GetEncoding(65001);
                    serialPort.Open(); // 포트 Open
                    serialPort.WriteLine("test"); // test 명령어 수행
                    System.Threading.Thread.Sleep(100); // 0.1초 동안 대기
                    int bytesize = serialPort.BytesToRead; // byte 타입으로 데이터 크기를 저장
                    byte[] tt = new byte[bytesize]; // byte배열로 저장
                    serialPort.Read(tt, 0, bytesize); // 데이터 받음. byte타입 배열로 tt변수에 받은 데이터를 저장
                    receivedUSBData = Convert.ToString(System.Text.Encoding.GetEncoding(65001).GetString(tt)); // 받은 데이터 tt를 string 타입으로 변환. Encoding 타입 65001 = UTF-8 
                    if (receivedUSBData.Contains("test")) // 통신이 정상적으로 수행되어 test 라는 값이 찍혔다면
                    {
                        correctUSBstate[i] = portnames[i]; // correctUSBstate 배열에 현재 포트명을 저장
                        port += "포트명 : " + portnames[i] + " , 연결유무 : 연결됨\n"; // 포트 상태 문자열 저장
                    }
                    serialPort.Close(); // 포트 닫음.
                }
            }
            // USB로 데이터 읽어 다운하는 기능
            else if (information.Equals("데이터다운"))
            {
                // 정상적인 USB 포트 & 현재 체크된 USB 포트들만 통신
                for (int i = 0; i < correctUSBConnect.Length; i++)
                {
                    //현재 체크되지 않은 포트들은 '존재하지않음.' 이라고 명시가 되어있음
                    if (correctUSBConnect[i].Equals("존재하지않음.") != true)
                    {
                        receivedUSBData = "";
                        serialPort = new SerialPort();
                        serialPort.PortName = correctUSBConnect[i];
                        serialPort.BaudRate = (int)115200;
                        serialPort.DataBits = (int)8;
                        serialPort.Parity = Parity.None;
                        serialPort.StopBits = StopBits.One;
                        serialPort.Encoding = System.Text.Encoding.GetEncoding(65001);
                        serialPort.Open();
                        serialPort.WriteLine("test");
                        System.Threading.Thread.Sleep(1000);
                        int bytesize = serialPort.BytesToRead;
                        byte[] tt = new byte[bytesize];
                        serialPort.Read(tt, 0, bytesize);
                        receivedUSBData = Convert.ToString(System.Text.Encoding.GetEncoding(65001).GetString(tt));
                        int k1 = receivedUSBData.IndexOf("\r", 0);
                        int k2 = receivedUSBData.IndexOf("\r", k1 + 1);
                        if (receivedUSBData.Contains("GT10") != true) // 인증기 명이 포함되어 있지 않으면 바로 종료하고 MessageBox 송출
                        {
                            serialPort.Close();
                            stopState_USB = 1;
                            break;
                        }
                        string adg = receivedUSBData.Substring(receivedUSBData.IndexOf("GT10"), 6); // 인증기 이름 저장
                        GT10_Name[i] = adg;
                        receivedUSBData = receivedUSBData.Replace("\n", ""); // \n 문자열 제거
                        receivedUSBData = receivedUSBData.Replace("\r" + adg + "\r", ""); // 이름이 포함된 문자열 제거
                        string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER"; // DATAFOLDER 폴더 경로 지정, 폴더 생성.
                        DirectoryInfo di = new DirectoryInfo(folderpath);
                        if (di.Exists == false)
                        {
                            di.Create();
                        }
                        folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + adg; // DATAFOLDER 폴더 안에 인증기 이름의 폴더 생성
                        di = new DirectoryInfo(folderpath);
                        if (di.Exists == false)
                        {
                            di.Create();
                        }
                        string a = receivedUSBData.Replace(receivedUSBData.Substring(0, k2 - 1), ""); // 인증기 안에 존재하는 날짜별로 저장된 데이터들 찾기
                        string[] a_array = a.Split('\r'); // 데이터들 날짜별 배열로 저장
                        int buffer = 0;
                        string[] datainformation = Directory.GetFiles(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + adg, "*.csv"); //현자 저장된 데이터가 있는지 여부 파악
                        for (int z = 0; z < datainformation.Length; z++)
                        {
                            datainformation[z] = datainformation[z].Replace(System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + adg + "\\", ""); // 경로 문자열 제거 
                        }
                        // 만약 데이터가 있다면 기존 데이터는 받지않고 새로운 데이터만 받는다.
                        if (datainformation.Length > 0)
                        {
                            for (int x = 0; x < a_array.Length; x++)
                            {
                                if ((a_array[x] != null && a_array[x].Length > 0 && Array.IndexOf(datainformation, a_array[x]) == -1) || a_array[x].Equals(DateTime.Now.ToString("yyyy-MM-dd") + ".csv")) // 중복되는 날짜의 데이터가 아닌 것들만 데이터로 받아서 처리한다.
                                {
                                    serialPort.WriteLine("information" + a_array[x].Replace(".csv", "")); //information날짜 전송
                                    System.Threading.Thread.Sleep(100);
                                    bytesize = serialPort.BytesToRead;
                                    tt = new byte[bytesize];
                                    System.Threading.Thread.Sleep(100);
                                    serialPort.Read(tt, 0, bytesize);
                                    receivedUSBData = Convert.ToString(Encoding.GetEncoding(65001).GetString(tt));
                                    k1 = receivedUSBData.IndexOf("\n", 0);
                                    string msg = receivedUSBData.Replace(receivedUSBData.Substring(0, k1), "");
                                    msg = msg.Replace("\r", ""); // 전달받은 데이터 크기를 저장
                                    dataSpeed[x] = Convert.ToInt32(msg); // dataSpeed에 데이터 크기를 저장
                                    serialPort.WriteLine(a_array[x]);
                                    receivedUSBData = "";
                                    while (receivedUSBData.Length < dataSpeed[x]) // 전달받은 데이터의 크기가 dataSpeed에 저장된 수치보다 작을경우 계속해서 데이터를 받도록 설정 
                                    {
                                        System.Threading.Thread.Sleep(buffer); // +0.1초동안 계속 늘려가면서 스레드 중지 => 신호를 더 받기위한 대기시간 필요
                                        bytesize = serialPort.BytesToRead;
                                        tt = new byte[bytesize];
                                        serialPort.Read(tt, 0, bytesize);
                                        receivedUSBData += Convert.ToString(System.Text.Encoding.GetEncoding(65001).GetString(tt));
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        receivedUSBData = receivedUSBData.Replace("print\r", "");
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        receivedUSBData = receivedUSBData.Replace(a_array[x] + "\r", "");
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        buffer += 100;
                                    }
                                    System.IO.File.WriteAllText(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + adg + "\\" + a_array[x], receivedUSBData, Encoding.GetEncoding(65001)); // 데이터를 저장
                                }
                            }
                        }
                        else //저장된 데이터가 하나도 없을 경우
                        {
                            for (int j = 0; j < a_array.Length; j++)
                            {
                                if (a_array[j] != null && a_array[j].Length > 0)
                                {
                                    serialPort.WriteLine("information" + a_array[j].Replace(".csv", ""));
                                    System.Threading.Thread.Sleep(1000);
                                    bytesize = serialPort.BytesToRead;
                                    tt = new byte[bytesize];
                                    serialPort.Read(tt, 0, bytesize);
                                    receivedUSBData = Convert.ToString(System.Text.Encoding.GetEncoding(65001).GetString(tt));
                                    k1 = receivedUSBData.IndexOf("\n", 0);
                                    string msg = receivedUSBData.Replace(receivedUSBData.Substring(0, k1), "");
                                    msg = msg.Replace("\r", "");
                                    dataSpeed[j] = Convert.ToInt32(msg);
                                    serialPort.WriteLine(a_array[j]);
                                    receivedUSBData = "";
                                    while (receivedUSBData.Length < dataSpeed[j])
                                    {
                                        System.Threading.Thread.Sleep(buffer);
                                        bytesize = serialPort.BytesToRead;
                                        tt = new byte[bytesize];
                                        serialPort.Read(tt, 0, bytesize);
                                        receivedUSBData += Convert.ToString(System.Text.Encoding.GetEncoding(65001).GetString(tt));
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        receivedUSBData = receivedUSBData.Replace("print\r", "");
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        receivedUSBData = receivedUSBData.Replace(a_array[j] + "\r", "");
                                        receivedUSBData = receivedUSBData.Replace("\n", "");
                                        buffer += 100;
                                    }
                                    System.IO.File.WriteAllText(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + adg + "\\" + a_array[j], receivedUSBData, Encoding.GetEncoding(65001));
                                }
                            }
                        }
                        serialPort.Close(); //포트 닫음
                    }
                }
            }
        }
        LoadingScreen lw;
        bool ERROR_FLAG = false;

        // 로딩창 생성 함수
        private void InitLoadWindow()
        {
            lw = new LoadingScreen();
            lw.LoadingError += LoadingError;
            lw.LoadingCancel += LoadingCancel;
            lw.Topmost = true;
            lw.Show();
            loadingThread.RunWorkerAsync();
        }
        private void LoadingError(object sender, RoutedEventArgs e)
        {
            ERROR_FLAG = true;
        }
        private void LoadingCancel(object sender, RoutedEventArgs e)
        {
            loadingThread.CancelAsync();
        }
        private void Thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Thread_DoWork의 IP통신상태 체크 기능이 종료됬을 경우 시행
            if (information.Equals("인증기통신"))
            {
                if (e.Cancelled)
                {
                    System.Windows.MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    lw.Close();
                    string s = DateTime.Now.ToString("yyyy-MM-dd-HH시mm분ss초");
                    string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\ERROR";
                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }
                    FileInfo fileInfo = new FileInfo(folderpath + "\\errorlog.txt");
                    if (fileInfo.Exists == true)
                    {
                        StreamWriter writer = new StreamWriter(folderpath + "\\errorlog.txt", true, System.Text.Encoding.Default);
                        writer.WriteLine("\n" + s + e.Error.ToString());
                        writer.Close();
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(folderpath + "\\errorlog.txt", s + "\n" + e.Error.ToString(), System.Text.Encoding.Default);
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    if (correctIPAddress != null)
                    {
                        연결창.IsEnabled = true; // CommunicationScreen 창 활성화
                        연결창.Opacity = 1; // Opacity = 명암
                        for (int i = 0; i < correctIPAddress.Length; i++)
                        {
                            if (correctIPAddress[i] != null)
                            {
                                //TextBox에 업로드
                                통신현황.AppendText("연결된 인증기 : " + (i + 1) + "번 인증기 ㅣ " + "연결된 인증기 IP :" + correctIPAddress[i] + "\n");
                                int number = Convert.ToInt32(correctIPAddress[i].Replace("169.254.7.", ""));
                                //체크박스 활성화
                                if (number < 11)
                                {
                                    if (number == 1)
                                    {
                                        인증기1.IsEnabled = true;
                                    }
                                    if (number == 2)
                                    {
                                        인증기2.IsEnabled = true;
                                    }
                                    if (number == 3)
                                    {
                                        인증기3.IsEnabled = true;
                                    }
                                    if (number == 4)
                                    {
                                        인증기4.IsEnabled = true;
                                    }
                                    if (number == 5)
                                    {
                                        인증기5.IsEnabled = true;
                                    }
                                    if (number == 6)
                                    {
                                        인증기6.IsEnabled = true;
                                    }
                                    if (number == 7)
                                    {
                                        인증기7.IsEnabled = true;
                                    }
                                    if (number == 8)
                                    {
                                        인증기8.IsEnabled = true;
                                    }
                                    if (number == 9)
                                    {
                                        인증기9.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    if (number == 11)
                                    {
                                        인증기10.IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("인증기 통신 연결을 찾을 수 없습니다.");
                    }
                    lw.Close(); // LoadingScreen 종료
                }
            }
            //Thread_DoWork의 IP통신으로 데이터를 읽어들여 저장하는 기능이 끝났을 때
            else if (information.Equals("데이터읽기"))
            {
                연결창.IsEnabled = true;
                연결창.Opacity = 1;
                if (e.Cancelled)
                {
                    System.Windows.MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    System.Windows.MessageBox.Show("Error!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else
                {
                    lw.Close();
                    System.Windows.MessageBox.Show("데이터 읽기 성공!");
                    DataTable dataTable = new DataTable(); //DataGrid창에 해당 정보를 구현하기 위한 DataTable 객체 생성
                    dataTable.Columns.Add("번호", typeof(string)); // '번호' 열 추가
                    dataTable.Columns.Add("인증기", typeof(string)); // '인증기' 열 추가
                    for (int j = 0; j < 10; j++)
                    {
                        if ((ipAddress_SSH[j] != null) && (ipAddress_SSH[j].Equals("존재하지않음.") == false))
                        {
                            dataTable.Rows.Add(new string[] { (j + 1).ToString(), "GT10_" + (j + 1).ToString() }); // 행에 인증기 번호 및 인증기 이름 추가
                        }

                    }
                    ((LaunchScreen)System.Windows.Application.Current.MainWindow).인증기_IP통신.ItemsSource = dataTable.DefaultView; // LaunchScreen.xaml의 DataGrid 인증기_IP통신에 DataTable 정보 전달
                    for (int i = 0; i < checkstate.Length; i++)
                    {
                        checkstate[i] = 0;
                    }
                    // 체크박스 Check 상태 초기화
                    인증기1.IsChecked = false;
                    인증기2.IsChecked = false;
                    인증기3.IsChecked = false;
                    인증기4.IsChecked = false;
                    인증기5.IsChecked = false;
                    인증기6.IsChecked = false;
                    인증기7.IsChecked = false;
                    인증기8.IsChecked = false;
                    인증기9.IsChecked = false;
                    인증기10.IsChecked = false;
                    DataTable dataTable2 = new DataTable();
                    dataTable2.Columns.Add("저장된 인증기", typeof(string));
                    DirectoryInfo directoryInfo = new DirectoryInfo(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER"); // 인증기 데이터 파일 탐색
                    DirectoryInfo[] searchfiles = directoryInfo.GetDirectories();
                    if (searchfiles != null)
                    {

                        for (int d = 0; d < searchfiles.Length; d++)
                        {
                            dataTable2.Rows.Add(new string[] { searchfiles[d].ToString() });
                        }
                    }
                            ((LaunchScreen)System.Windows.Application.Current.MainWindow).저장된데이터.ItemsSource = dataTable2.DefaultView; // LaunchScreen.xaml의 DataGrid 저장된데이터에 DataTable 정보 전달
                }
            }
            //Thread_DoWork의 인증기와 PC가 통신이 가능한 USB 포트를 체크 기능 끝날시 수행
            else if (information.Equals("USB통신상태체크"))
            {
                if (e.Cancelled)
                {
                    System.Windows.MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    lw.Close();
                    string s = DateTime.Now.ToString("yyyy-MM-dd-HH시mm분ss초");
                    string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\ERROR";
                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }
                    FileInfo fileInfo = new FileInfo(folderpath + "\\errorlog.txt");
                    if (fileInfo.Exists == true)
                    {
                        StreamWriter writer = new StreamWriter(folderpath + "\\errorlog.txt", true, System.Text.Encoding.Default);
                        writer.WriteLine("\n" + s + e.Error.ToString());
                        writer.Close();
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(folderpath + "\\errorlog.txt", s + "\n" + e.Error.ToString(), System.Text.Encoding.Default);
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    연결창.IsEnabled = true;
                    연결창.Opacity = 1;
                    USB_통신현황.Text += port; // 통신현황 업로드

                    //체크박스 활성화 (통신가능한 포트수 만큼)
                    if (correctUSBstate[0] != null)
                    {
                        인증기1_USB.Content = correctUSBstate[0];
                        인증기1_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[1] != null)
                    {
                        인증기2_USB.Content = correctUSBstate[1];
                        인증기2_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[2] != null)
                    {
                        인증기3_USB.IsEnabled = true;
                        인증기3_USB.Content = correctUSBstate[2];
                    }
                    if (correctUSBstate[3] != null)
                    {
                        인증기4_USB.Content = correctUSBstate[3];
                        인증기4_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[4] != null)
                    {
                        인증기5_USB.Content = correctUSBstate[4];
                        인증기5_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[5] != null)
                    {
                        인증기6_USB.Content = correctUSBstate[5];
                        인증기6_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[6] != null)
                    {
                        인증기7_USB.Content = correctUSBstate[6];
                        인증기7_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[7] != null)
                    {
                        인증기8_USB.Content = correctUSBstate[7];
                        인증기8_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[8] != null)
                    {
                        인증기9_USB.Content = correctUSBstate[8];
                        인증기9_USB.IsEnabled = true;
                    }
                    if (correctUSBstate[9] != null)
                    {
                        인증기10_USB.Content = correctUSBstate[9];
                        인증기10_USB.IsEnabled = true;
                    }

                    lw.Close();

                }
            }
            //Thread_DoWork의 USB로 데이터 읽어 다운하는 기능 끝날시 수행
            else if (information.Equals("데이터다운"))
            {
                if (e.Error != null)
                {
                    lw.Close();
                    string s = DateTime.Now.ToString("yyyy-MM-dd-HH시mm분ss초");
                    string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\ERROR";
                    DirectoryInfo di = new DirectoryInfo(folderpath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }
                    FileInfo fileInfo = new FileInfo(folderpath + "\\errorlog.txt");
                    if (fileInfo.Exists == true)
                    {
                        StreamWriter writer = new StreamWriter(folderpath + "\\errorlog.txt", true, System.Text.Encoding.Default);
                        writer.WriteLine("\n" + s + e.Error.ToString());
                        writer.Close();
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(folderpath + "\\errorlog.txt", s + "\n" + e.Error.ToString(), System.Text.Encoding.Default);
                        MessageBox.Show(e.Error.ToString(), "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                if (stopState_USB == 0)
                {
                    연결창.IsEnabled = true;
                    연결창.Opacity = 1;
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("인증기정보", typeof(string));
                    dataTable.Columns.Add("USB포트", typeof(string));
                    for (int j = 0; j < 10; j++)
                    {
                        if ((correctUSBstate[j] != null) && (correctUSBstate[j].Equals("존재하지않음.") == false))
                        {
                            dataTable.Rows.Add(new string[] { GT10_Name[j], correctUSBstate[j] });
                        }
                    }
                ((LaunchScreen)System.Windows.Application.Current.MainWindow).인증기_USB통신.ItemsSource = dataTable.DefaultView; // LaunchScreen.xaml의 DataGrid 인증기_USB통신에 DataTable 정보 전달
                    DataTable dataTable2 = new DataTable();
                    dataTable2.Columns.Add("저장된 인증기", typeof(string));
                    DirectoryInfo directoryInfo = new DirectoryInfo(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER");
                    DirectoryInfo[] searchfiles = directoryInfo.GetDirectories();
                    if (searchfiles != null)
                    {

                        for (int d = 0; d < searchfiles.Length; d++)
                        {
                            dataTable2.Rows.Add(new string[] { searchfiles[d].ToString() });
                        }
                    }
                            ((LaunchScreen)System.Windows.Application.Current.MainWindow).저장된데이터.ItemsSource = dataTable2.DefaultView;
                    lw.Close();
                }
                else
                {
                    MessageBox.Show("데이터 통신이 원활하지 않습니다.\n재연결하거나 설정이 필요합니다.");
                    연결창.IsEnabled = true;
                    연결창.Opacity = 1;
                    DataTable dataTable = new DataTable();
                    ((LaunchScreen)System.Windows.Application.Current.MainWindow).저장된데이터.ItemsSource = dataTable.DefaultView;
                    ((LaunchScreen)System.Windows.Application.Current.MainWindow).인증기_USB통신.ItemsSource = dataTable.DefaultView;
                }
            }

        }
        // 체크박스 체크시마다 포트 및 IP할당
        void CheckBox1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 0)
                {
                    if (인증기1.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox2(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 1)
                {
                    if (인증기2.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox3(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 2)
                {
                    if (인증기3.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox4(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 3)
                {
                    if (인증기4.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox5(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 4)
                {
                    if (인증기5.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox6(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 5)
                {
                    if (인증기6.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox7(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 6)
                {
                    if (인증기7.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox8(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 7)
                {
                    if (인증기8.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox9(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 8)
                {
                    if (인증기9.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox10(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 9)
                {
                    if (인증기10.IsChecked == true)
                    {
                        checkstate[i] = 1;
                    }
                    else
                    {
                        checkstate[i] = 0;
                    }
                }
            }
        }
        void CheckBox1_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 0)
                {
                    if (인증기1_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기1_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox2_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 1)
                {
                    if (인증기2_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기1_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox3_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 2)
                {
                    if (인증기3_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기3_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox4_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 3)
                {
                    if (인증기4_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기4_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox5_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 4)
                {
                    if (인증기5_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기5_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox6_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 5)
                {
                    if (인증기6_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기6_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox7_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 6)
                {
                    if (인증기7_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기7_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox8_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 7)
                {
                    if (인증기8_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기8_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox9_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 8)
                {
                    if (인증기9_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기9_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        void CheckBox10_USB(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (i == 9)
                {
                    if (인증기10_USB.IsChecked == true)
                    {
                        checkUSBState[i] = 1;
                        correctUSBConnect[i] = 인증기10_USB.Content.ToString();
                    }
                    else
                    {
                        checkUSBState[i] = 0;
                        correctUSBConnect[i] = "";
                    }
                }
            }
        }
        private void Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        //CommunicationScreen.xaml 실행시 함수 실행
        public CommunicationScreen()
        {
            InitializeComponent();
            string[] Portnum = SerialPort.GetPortNames();
            인증기1_USB.IsEnabled = false;
            인증기2_USB.IsEnabled = false;
            인증기3_USB.IsEnabled = false;
            인증기4_USB.IsEnabled = false;
            인증기5_USB.IsEnabled = false;
            인증기6_USB.IsEnabled = false;
            인증기7_USB.IsEnabled = false;
            인증기8_USB.IsEnabled = false;
            인증기9_USB.IsEnabled = false;
            인증기10_USB.IsEnabled = false;
            인증기1.IsEnabled = false;
            인증기2.IsEnabled = false;
            인증기3.IsEnabled = false;
            인증기4.IsEnabled = false;
            인증기5.IsEnabled = false;
            인증기6.IsEnabled = false;
            인증기7.IsEnabled = false;
            인증기8.IsEnabled = false;
            인증기9.IsEnabled = false;
            인증기10.IsEnabled = false;
            인증기데이터.IsEnabled = false;
            인증기데이터.Opacity = 0.5;
            통신창.Background = new SolidColorBrush(Color.FromArgb(110, 98, 149, 255)); // 배경색깔 설정
        }
        //Thread_DoWork의 IP통신상태 체크로 이동
        private void ConnectGT10_IP(object sender, RoutedEventArgs e)
        {
            인증기1.IsEnabled = false;
            인증기2.IsEnabled = false;
            인증기3.IsEnabled = false;
            인증기4.IsEnabled = false;
            인증기5.IsEnabled = false;
            인증기6.IsEnabled = false;
            인증기7.IsEnabled = false;
            인증기8.IsEnabled = false;
            인증기9.IsEnabled = false;
            인증기10.IsEnabled = false;
            인증기데이터.IsEnabled = true;
            연결창.IsEnabled = false;
            연결창.Opacity = 0.5;
            인증기데이터.Opacity = 1;
            information = "인증기통신";
            for (int i = 0; i < 10; i++)
            {
                correctIPAddress[i] = null;
                checkstate[i] = 0;
            }
            initBW();
            InitLoadWindow();
        }
        //Thread_DoWork의 IP통신 데이터 다운로드 기능으로 이동
        void DataDownload_IP(object sender, RoutedEventArgs e)
        {
            연결창.IsEnabled = false;
            연결창.Opacity = 0.5;
            information = "데이터읽기";
            int count = 0;
            for (int i = 0; i < checkstate.Length; i++)
            {
                if (checkstate[i] != 1)
                {
                    ipAddress_SSH[i] = "존재하지않음.";
                }
                else
                {
                    ipAddress_SSH[i] = correctIPAddress[i];
                    count++;
                }
            }
            if (count == 0)
            {
                연결창.IsEnabled = true;
                연결창.Opacity = 1;
                System.Windows.MessageBox.Show("선택한 인증기가 존재하지 않습니다.");
            }
            else
            {
                initBW();
                InitLoadWindow();
            }
        }
        // 종료시 LaunchScreen 창 활성화 및 명암 조정
        private void Window_Closed(object sender, EventArgs e)
        {
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).메인창.IsEnabled = true;
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).메인창.Opacity = 1;
        }
        //Thrad_DoWork의 USB통신 데이터 다운로드 기능으로 이동
        private void DataDownload_USB(object sender, RoutedEventArgs e)
        {
            연결창.IsEnabled = false;
            연결창.Opacity = 0.5;
            information = "데이터다운";
            int count = 0;
            for (int i = 0; i < checkUSBState.Length; i++)
            {
                if (checkUSBState[i] != 1)
                {
                    correctUSBConnect[i] = "존재하지않음.";
                }
                else
                {
                    count++;
                }
            }
            if (count > 0)
            {
                initBW();
                InitLoadWindow();
            }
            else
            {
                연결창.IsEnabled = true;
                연결창.Opacity = 1;
                MessageBox.Show("선택된 인증기가 존재하지 않습니다.");
            }
        }
        //현재 xaml 종료
        private void Exit_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //Thread_DoWork의 USB통신상태 체크로 이동
        private void ConnectGT10_USB(object sender, RoutedEventArgs e)
        {
            인증기1_USB.IsChecked = false;
            인증기2_USB.IsChecked = false;
            인증기3_USB.IsChecked = false;
            인증기4_USB.IsChecked = false;
            인증기5_USB.IsChecked = false;
            인증기6_USB.IsChecked = false;
            인증기7_USB.IsChecked = false;
            인증기8_USB.IsChecked = false;
            인증기9_USB.IsChecked = false;
            인증기10_USB.IsChecked = false;
            연결창.Opacity = 0.5;
            information = "USB통신상태체크";
            initBW();
            InitLoadWindow();
        }
        //마우스 좌측버튼 누른채 마우스 커서를 옮기면 해당 창이 이동하는 기능
        private void 연결창_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
