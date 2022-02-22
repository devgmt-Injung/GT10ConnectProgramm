using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GT10ConnectProgramm
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LaunchScreen
    {
        SerialPort serialPort; // 시리얼 포트 통신을 위한 SerialPort객체 생성
        private BackgroundWorker loadingThread; // 로딩중 화면을 구현하기 위한 BackGround스레드
        string command; // 스레드 내부에 어떤 명령을 줄건지 설정하는 String 타입 변수
        string[] GT10_Data; // 데이터를 받을 배열
        string[] GT10_DataLine; // 데이터를 줄 단위로 받을 배열
        string current_GT10Address; // 선택한 인증기 주소(USB,IP 정 보등)를 담을 String 타입 변수
        int dbState = 0; // 저장한 데이터를 조회 할지 말지 결정할 int타입 변수, 1이면 저장한 데이터 조회, 0이면 조회 하지 않음.
        int usbState = 0; // USB통신 데이터를 조회할지 말지 결정할 int타입 변수, 1이면 USB데이터 조회, 0이면 조회 하지 않음.
        string current_GT10USB_Port; // 선택한 USB포트 번호를 담을 String타입 변수

        // 스레드 부분 - 스레드 형성 및 실행
        private void initBW()
        {
            loadingThread = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            loadingThread.DoWork += Thread_DoWork;

            loadingThread.ProgressChanged += Thread_ProgressChanged;

            loadingThread.RunWorkerCompleted += Thread_RunWorkerCompleted;
        }

        //스레드 동작 내용
        private void Thread_DoWork(object sender, DoWorkEventArgs e)
        {
            //데이터 조회 기능(IP통신 기기 선택시)
            if (command.Equals("데이터조회") && dbState == 0 && usbState == 0)
            {
                if (loadingThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (ERROR_FLAG == true)
                {
                    throw new Exception("ErrorRaised");
                }
                if (current_GT10Address != null)
                {
                    string[] directory = Directory.GetFiles(current_GT10Address + "\\", "*.csv"); // 현재경로에서 전달받은 인증기 명 폴더 안에 .csv확장자로 된 파일을 찾는다.
                    ReadFile file = new ReadFile(); // 특정 경로의 파일을 읽는 ReadFile 클래스의 객체 생성
                    var list = new List<string>();
                    var list_line = new List<string>();
                    for (int i = 0; i < directory.Length; i++)
                    {
                        GT10_Data = file.GetData(directory[i]); // 데이터를 불러온다.
                        list.AddRange(GT10_Data);
                        GT10_DataLine = file.GetData_Line(directory[i]); // 데이터를 줄단위로 불러온다.
                        list_line.AddRange(GT10_DataLine);
                    }
                    GT10_Data = list.ToArray();
                    GT10_DataLine = list_line.ToArray();
                }
            }
            //데이터 조회 기능(저장된 통신 기기 선택시)
            else if (command.Equals("데이터조회") && dbState == 1 && usbState == 0)
            {
                if (loadingThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (ERROR_FLAG == true)
                {
                    throw new Exception("ErrorRaised");
                }
                if (current_GT10Address != null)
                {
                    string[] directory = Directory.GetFiles(current_GT10Address + "\\", "*.csv");
                    ReadFile file = new ReadFile();
                    var list = new List<string>();
                    var list_line = new List<string>();
                    for (int i = 0; i < directory.Length; i++)
                    {
                        GT10_Data = file.GetData(directory[i]);
                        list.AddRange(GT10_Data);
                        GT10_DataLine = file.GetData_Line(directory[i]);
                        list_line.AddRange(GT10_DataLine);
                    }
                    GT10_Data = list.ToArray();
                    GT10_DataLine = list_line.ToArray();
                }
            }
            //데이터 조회 기능(USB 통신 기기 선택시)
            else if (command.Equals("데이터조회") && dbState == 0 && usbState == 1)
            {
                if (loadingThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (ERROR_FLAG == true)
                {
                    throw new Exception("ErrorRaised");
                }
                if (current_GT10USB_Port != null)
                {
                    string[] directory = Directory.GetFiles(current_GT10USB_Port + "\\", "*.csv");
                    ReadFile file = new ReadFile();
                    var list = new List<string>();
                    var list_line = new List<string>();
                    for (int i = 0; i < directory.Length; i++)
                    {
                        GT10_Data = file.GetData(directory[i]);
                        list.AddRange(GT10_Data);
                        GT10_DataLine = file.GetData_Line(directory[i]);
                        list_line.AddRange(GT10_DataLine);
                    }
                    GT10_Data = list.ToArray();
                    GT10_DataLine = list_line.ToArray();
                }
            }
        }

        //Serial통신에 사용되는 함수

        //로딩창을 선언해줄 Window.xaml의 객체 lw
        LoadingScreen lw;
        //스레드 실행도중 에러가 발생되면 처리하기 위해 생성한 bool타입 변수 ERROR_FLAG
        bool ERROR_FLAG = false;

        //Window1 창 생성
        private void InitLoadWindow()
        {
            lw = new LoadingScreen();
            lw.LoadingError += LoadingError;
            lw.LoadingCancel += LoadingCancel;
            lw.Show();
            loadingThread.RunWorkerAsync();
        }
        // 에러 발생시키는 함수
        private void LoadingError(object sender, RoutedEventArgs e)
        {
            ERROR_FLAG = true;
        }
        // 취소 발생시키는 함수
        private void LoadingCancel(object sender, RoutedEventArgs e)
        {
            loadingThread.CancelAsync();
        }

        // Thread_DoWork 함수가 실행을 끝마쳤을때 실행되는 함수
        private void Thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Thread_DoWork의 데이터 조회 기능(IP 통신 기기 선택시) 끝날시에 수행
            if (command.Equals("데이터조회") && dbState == 0 && usbState == 0)
            {
                if (e.Cancelled)
                {
                    MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    MessageBox.Show("Error!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else
                {
                    if (current_GT10Address != null)
                    {
                        메인창.IsEnabled = true;
                        메인창.Opacity = 1;
                        ResDatePicker1.IsEnabled = true;
                        전체날짜.IsEnabled = true;
                        내용검색.IsEnabled = true;
                        검색.IsEnabled = true;
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("날짜", typeof(string));
                        dataTable.Columns.Add("번호", typeof(string));
                        dataTable.Columns.Add("발급문서명", typeof(string));
                        dataTable.Columns.Add("수수료", typeof(string));
                        dataTable.Columns.Add("발행매수", typeof(string));
                        dataTable.Columns.Add("발행소계", typeof(string));
                        dataTable.Columns.Add("결손매수", typeof(string));
                        dataTable.Columns.Add("결손소계", typeof(string));
                        dataTable.Columns.Add("정산합계", typeof(string));
                        for (int i = 0; i < (int)(GT10_Data.Length / 9); i++)
                        {
                            dataTable.Rows.Add(new string[] { GT10_Data[(i * 9)].ToString(), GT10_Data[(i * 9) + 1].ToString(), GT10_Data[(i * 9) + 2].ToString(), GT10_Data[(i * 9) + 3].ToString(),
                                    GT10_Data[(i * 9) + 4].ToString(), GT10_Data[(i * 9) + 5].ToString(), GT10_Data[(i * 9) + 6].ToString(), GT10_Data[(i * 9) + 7].ToString(), GT10_Data[(i * 9) + 8].ToString() });
                        }
                        조회데이터.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        메인창.IsEnabled = true;
                        메인창.Opacity = 1;
                        MessageBox.Show("선택한 인증기 데이터가 없습니다.", "인증기를 선택해 주세요.");
                    }

                    lw.Close();
                }
            }
            //Thread_DoWork의 데이터 조회 기능(저장된 통신 기기 선택시) 끝날시에 수행
            else if (command.Equals("데이터조회") && dbState == 1 && usbState == 0)
            {
                if (e.Cancelled)
                {
                    MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    MessageBox.Show("Error!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else
                {
                    if (current_GT10Address != null)
                    {
                        메인창.IsEnabled = true;
                        메인창.Opacity = 1;
                        ResDatePicker1.IsEnabled = true;
                        전체날짜.IsEnabled = true;
                        내용검색.IsEnabled = true;
                        검색.IsEnabled = true;
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("날짜", typeof(string));
                        dataTable.Columns.Add("번호", typeof(string));
                        dataTable.Columns.Add("발급문서명", typeof(string));
                        dataTable.Columns.Add("수수료", typeof(string));
                        dataTable.Columns.Add("발행매수", typeof(string));
                        dataTable.Columns.Add("발행소계", typeof(string));
                        dataTable.Columns.Add("결손매수", typeof(string));
                        dataTable.Columns.Add("결손소계", typeof(string));
                        dataTable.Columns.Add("정산합계", typeof(string));
                        for (int i = 0; i < (int)(GT10_Data.Length / 9); i++)
                        {
                            dataTable.Rows.Add(new string[] { GT10_Data[(i * 9)].ToString(), GT10_Data[(i * 9) + 1].ToString(), GT10_Data[(i * 9) + 2].ToString(), GT10_Data[(i * 9) + 3].ToString(),
                                    GT10_Data[(i * 9) + 4].ToString(), GT10_Data[(i * 9) + 5].ToString(), GT10_Data[(i * 9) + 6].ToString(), GT10_Data[(i * 9) + 7].ToString(), GT10_Data[(i * 9) + 8].ToString() });
                        }

                        조회데이터.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        메인창.IsEnabled = true;
                        메인창.Opacity = 1;
                        MessageBox.Show("선택한 포트가 없습니다.", "포트를 선택해 주세요.");
                    }
                    lw.Close();
                }
            }
            //Thread_DoWork의 데이터 조회 기능(USB 통신 기기 선택시) 끝날시에 수행
            else if (command.Equals("데이터조회") && dbState == 0 && usbState == 1)
            {
                if (e.Cancelled)
                {
                    MessageBox.Show("Canceled!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else if (e.Error != null)
                {
                    MessageBox.Show("Error!", "Loading Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    lw.Close();
                    Environment.Exit(-1);
                }
                else
                {
                    if (current_GT10USB_Port != null)
                    {
                        메인창.IsEnabled = true;
                        메인창.Opacity = 1;
                        ResDatePicker1.IsEnabled = true;
                        전체날짜.IsEnabled = true;
                        내용검색.IsEnabled = true;
                        검색.IsEnabled = true;
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("날짜", typeof(string));
                        dataTable.Columns.Add("번호", typeof(string));
                        dataTable.Columns.Add("발급문서명", typeof(string));
                        dataTable.Columns.Add("수수료", typeof(string));
                        dataTable.Columns.Add("발행매수", typeof(string));
                        dataTable.Columns.Add("발행소계", typeof(string));
                        dataTable.Columns.Add("결손매수", typeof(string));
                        dataTable.Columns.Add("결손소계", typeof(string));
                        dataTable.Columns.Add("정산합계", typeof(string));
                        for (int i = 0; i < (int)(GT10_Data.Length / 9); i++)
                        {
                            dataTable.Rows.Add(new string[] { GT10_Data[(i * 9)].ToString(), GT10_Data[(i * 9) + 1].ToString(), GT10_Data[(i * 9) + 2].ToString(), GT10_Data[(i * 9) + 3].ToString(),
                                    GT10_Data[(i * 9) + 4].ToString(), GT10_Data[(i * 9) + 5].ToString(), GT10_Data[(i * 9) + 6].ToString(), GT10_Data[(i * 9) + 7].ToString(), GT10_Data[(i * 9) + 8].ToString() });
                        }
                        조회데이터.ItemsSource = dataTable.DefaultView;
                        lw.Close();
                    }
                    else
                    {
                        MessageBox.Show("선택된 인증기가 없습니다.");
                        lw.Close();
                    }
                }
            }

        }

        private void Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        // MainWindow.xaml 실행시 첫번째로 선언.
        public LaunchScreen()
        {
            InitializeComponent();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("저장된 인증기", typeof(string));
            string folderpath = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER";
            DirectoryInfo di = new DirectoryInfo(folderpath);
            if (di.Exists == false)
            {
                di.Create();
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER"); //DATAFOLDER아래에 저장된 폴더 명 반환
            DirectoryInfo[] searchfiles = directoryInfo.GetDirectories();
            if (searchfiles != null)
            {

                for (int i = 0; i < searchfiles.Length; i++)
                {
                    dataTable.Rows.Add(new string[] { searchfiles[i].ToString() });
                }
            }
            저장된데이터.ItemsSource = dataTable.DefaultView;
            전체날짜.IsEnabled = false;
            ResDatePicker1.IsEnabled = false;
            내용검색.IsEnabled = false;
            검색.IsEnabled = false;
            엑셀카운트.Text = "0";
            엑셀.Text = "비활성화";
            내용검색.Foreground = Brushes.Gray;  //텍스트 블록의 전경색 설정
            Grid_2.Background = new SolidColorBrush(Color.FromArgb(110, 98, 149, 255)); //배경색깔

        }

        // 마우스 좌측버튼 클릭한 상태로 움직였을시 창을 움직이게 하는 함수
        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
        // MainWindows.xaml의 프로그램 종료 버튼 클릭시 실행하는 함수. 프로그램 종료 기능 담당
        void Exit_Window(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            this.Close();
        }

        // MainWindow.xaml의 연결하기 버튼을 클릭할 때 마다 실행.
        void Show_CommunicationScreen(object sender, RoutedEventArgs e)
        {
            메인창.IsEnabled = false; // MainWindow.xaml 비활성화
            메인창.Opacity = 0.5;
            CommunicationScreen communicationScreen = new CommunicationScreen();
            communicationScreen.Topmost = true;
            communicationScreen.Show();
        }

        // MainWindow.xaml의 데이터 설정 버튼을 클릭할 때 마다 실행.
        void Show_SaveFileScreen(object sender, RoutedEventArgs e)
        {
            메인창.IsEnabled = false;
            메인창.Opacity = 0.5;
            SaveFileScreen saveFileScreen = new SaveFileScreen();
            saveFileScreen.Topmost = true;
            saveFileScreen.Show();
        }

        // MainWindow.xaml의 데이터 조회 버튼을 클릭할 때 마다 실행. 
        void ShowData(object sender, RoutedEventArgs e)
        {

            command = "데이터조회";
            메인창.IsEnabled = false;
            메인창.Opacity = 0.5;
            initBW();
            InitLoadWindow();
        }

        // MainWindow.xaml의 검색 버튼을 클릭할 때 마다 실행.
        void TextBox_Changed(object sender, RoutedEventArgs e)
        {
            if (GT10_DataLine != null)
            {
                if (ResDatePicker1.Text.Equals(""))
                {
                    if (내용검색.Text != null && 내용검색.Text.Equals("") == false && 내용검색.Text.Equals("내용검색") == false)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("날짜", typeof(string));
                        dataTable.Columns.Add("번호", typeof(string));
                        dataTable.Columns.Add("발급문서명", typeof(string));
                        dataTable.Columns.Add("수수료", typeof(string));
                        dataTable.Columns.Add("발행매수", typeof(string));
                        dataTable.Columns.Add("발행소계", typeof(string));
                        dataTable.Columns.Add("결손매수", typeof(string));
                        dataTable.Columns.Add("결손소계", typeof(string));
                        dataTable.Columns.Add("정산합계", typeof(string));
                        for (int j = 0; j < GT10_DataLine.Length; j++)
                        {
                            if (GT10_DataLine[j].Contains(내용검색.Text) && 내용검색.Text != "")
                            {

                                string[] data = GT10_DataLine[j].Split(',');
                                for (int i = 0; i < (int)(data.Length / 9); i++)
                                {
                                    dataTable.Rows.Add(new string[] { data[(i * 9)].ToString(), data[(i * 9) + 1].ToString(), data[(i * 9) + 2].ToString(), data[(i * 9) + 3].ToString(),
                                            data[(i * 9) + 4].ToString(), data[(i * 9) + 5].ToString(), data[(i * 9) + 6].ToString(), data[(i * 9) + 7].ToString(), data[(i * 9) + 8].ToString() });
                                }

                            }
                        }
                        조회데이터.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("검색어를 입력해주세요.", "");
                    }
                }
                else
                {
                    if (내용검색.Text != null && 내용검색.Text.Equals("") == false && 내용검색.Text.Equals("내용검색") == false)
                    {
                        DateTime dtDate = new DateTime();
                        dtDate = DateTime.ParseExact(ResDatePicker1.Text, "yyyy/mm/dd", null);
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("날짜", typeof(string));
                        dataTable.Columns.Add("번호", typeof(string));
                        dataTable.Columns.Add("발급문서명", typeof(string));
                        dataTable.Columns.Add("수수료", typeof(string));
                        dataTable.Columns.Add("발행매수", typeof(string));
                        dataTable.Columns.Add("발행소계", typeof(string));
                        dataTable.Columns.Add("결손매수", typeof(string));
                        dataTable.Columns.Add("결손소계", typeof(string));
                        dataTable.Columns.Add("정산합계", typeof(string));
                        for (int j = 0; j < GT10_DataLine.Length; j++)
                        {
                            if (GT10_DataLine[j].Contains(내용검색.Text) && 내용검색.Text != "" && GT10_DataLine[j].Contains(dtDate.ToString("yyyy/mm/dd")))
                            {
                                string[] data = GT10_DataLine[j].Split(',');
                                for (int i = 0; i < (int)(data.Length / 9); i++)
                                {
                                    dataTable.Rows.Add(new string[] { data[(i * 9)].ToString(), data[(i * 9) + 1].ToString(), data[(i * 9) + 2].ToString(), data[(i * 9) + 3].ToString(),
                                            data[(i * 9) + 4].ToString(), data[(i * 9) + 5].ToString(), data[(i * 9) + 6].ToString(), data[(i * 9) + 7].ToString(), data[(i * 9) + 8].ToString() });
                                }

                            }
                        }
                        조회데이터.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("검색어를 입력해주세요.", "");
                    }
                }
            }
            else
            {
                MessageBox.Show("데이터가 존재하지 않습니다.", "에러");
            }

        }

        // MainWindow.xaml의 내용검색 텍스트 박스를 클릭할 때 마다 실행.
        void ClickThe_TextBox(object sender, RoutedEventArgs e)
        {
            내용검색.Text = ""; // 클릭시마다 텍스트 박스 내용 초기화.
        }

        // MainWindow.xaml의 데이터저장 버튼 클릭할 때 마다 실행.
        void Show_SaveExcelScreen(object sender, RoutedEventArgs e)
        {

            메인창.IsEnabled = false;
            command = "엑셀조회";
            메인창.Opacity = 0.5;
            SaveExcelScreen saveExcelScreen = new SaveExcelScreen();
            saveExcelScreen.Show();
        }

        // MainWindow.xaml의 DataGrid1에 형성된 버튼을 클릭할 때 마다 실행
        void ChooseGT10_IP(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    usbState = 0;
                    dbState = 0;
                    DataRowView myrow = (DataRowView)인증기_IP통신.CurrentCell.Item;
                    MessageBox.Show(myrow.Row.ItemArray[1] + "와 연결됨.", "");
                    current_GT10Address = myrow.Row.ItemArray[1].ToString();
                    current_GT10Address = current_GT10Address.Replace("_IP", "");
                    current_GT10Address = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + current_GT10Address; //선택한 인증기의 경로 저장
                    break;
                }
            }
        }

        // MainWindow.xaml의 USBDATA에 형성된 버튼을 클릭할 때 마다 실행
        void ChooseGT10_USB(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    if (serialPort != null)
                    {
                        if (serialPort.IsOpen == true)
                        {
                            serialPort.Close();
                        }
                    }
                    dbState = 0;
                    usbState = 1;
                    DataRowView myrow = (DataRowView)인증기_USB통신.CurrentCell.Item;
                    current_GT10USB_Port = myrow.Row.ItemArray[0].ToString();
                    current_GT10USB_Port = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + current_GT10USB_Port; //선택한 인증기의 경로 저장
                    MessageBox.Show(myrow.Row.ItemArray[0].ToString() + "와 연결됨.");
                    break;
                }
            }
        }

        // MainWindow.xaml의 전체날짜 버튼을 클릭할 때 마다 실행
        void ShowData_AllDate(object sender, RoutedEventArgs e)
        {
            ResDatePicker1.Text = "";
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("날짜", typeof(string));
            dataTable.Columns.Add("번호", typeof(string));
            dataTable.Columns.Add("발급문서명", typeof(string));
            dataTable.Columns.Add("수수료", typeof(string));
            dataTable.Columns.Add("발행매수", typeof(string));
            dataTable.Columns.Add("발행소계", typeof(string));
            dataTable.Columns.Add("결손매수", typeof(string));
            dataTable.Columns.Add("결손소계", typeof(string));
            dataTable.Columns.Add("정산합계", typeof(string));
            for (int i = 0; i < (int)(GT10_Data.Length / 9); i++)
            {
                dataTable.Rows.Add(new string[] { GT10_Data[(i * 9)].ToString(), GT10_Data[(i * 9) + 1].ToString(), GT10_Data[(i * 9) + 2].ToString(), GT10_Data[(i * 9) + 3].ToString(),
                                    GT10_Data[(i * 9) + 4].ToString(), GT10_Data[(i * 9) + 5].ToString(), GT10_Data[(i * 9) + 6].ToString(), GT10_Data[(i * 9) + 7].ToString(), GT10_Data[(i * 9) + 8].ToString() });
            }

            조회데이터.ItemsSource = dataTable.DefaultView;
        }

        // MainWindow.xaml의 ResDatePicker1에서 날짜 변경될 때 마다 실행
        void ShowData_DateChanged(object sender, RoutedEventArgs e)
        {
            if (!ResDatePicker1.Text.Equals(""))
            {
                DateTime dtDate = new DateTime();
                dtDate = DateTime.ParseExact(ResDatePicker1.Text, "yyyy/mm/dd", null);
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("날짜", typeof(string));
                dataTable.Columns.Add("번호", typeof(string));
                dataTable.Columns.Add("발급문서명", typeof(string));
                dataTable.Columns.Add("수수료", typeof(string));
                dataTable.Columns.Add("발행매수", typeof(string));
                dataTable.Columns.Add("발행소계", typeof(string));
                dataTable.Columns.Add("결손매수", typeof(string));
                dataTable.Columns.Add("결손소계", typeof(string));
                dataTable.Columns.Add("정산합계", typeof(string));
                for (int j = 0; j < GT10_DataLine.Length; j++)
                {
                    if (GT10_DataLine[j].Contains(dtDate.ToString("yyyy/mm/dd")))
                    {
                        string[] data = GT10_DataLine[j].Split(',');
                        for (int i = 0; i < (int)(data.Length / 9); i++)
                        {
                            dataTable.Rows.Add(new string[] { data[(i * 9)].ToString(), data[(i * 9) + 1].ToString(), data[(i * 9) + 2].ToString(), data[(i * 9) + 3].ToString(),
                                            data[(i * 9) + 4].ToString(), data[(i * 9) + 5].ToString(), data[(i * 9) + 6].ToString(), data[(i * 9) + 7].ToString(), data[(i * 9) + 8].ToString() });
                        }

                    }
                }
                조회데이터.ItemsSource = dataTable.DefaultView;
            }
        }

        // MainWindow.xaml의 DB목록 DataGrid에 형성된 버튼을 클릭할 때 마다 실행
        void ChooseGT10_File(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    DataRowView myrow = (DataRowView)저장된데이터.CurrentCell.Item;
                    usbState = 0;
                    current_GT10Address = myrow.Row.ItemArray[0].ToString();
                    MessageBox.Show(current_GT10Address + "선택됨.");
                    current_GT10Address = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + current_GT10Address;
                    dbState = 1;
                    break;
                }
            }
        }
    }

}