using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GT10ConnectProgramm
{
    /// <summary>
    /// Window2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SaveExcelScreen : Window //데이터를 Excel로 출력하거나 저장하는 기능
    {
        public string[] ipaddress;
        AccomplishExcelProgramm ExcelInstance;
        DataTable ExcelTable = new DataTable();
        string[] GT10_DataLine;
        string[] currentData;
        int excelcount = 0;
        string currentDate;
        int excelstate = 0;
        int currentGT10_num;
        int savestate = 0;
        LoadingScreen lw;
        bool ERROR_FLAG = false;
        private BackgroundWorker loadingThread;
        List<string> list2 = new List<string>();  // 업로드된 데이터 현황에서 삭제할 배열
        int allcheckstate = 0; // 전체 선택 체크박스 상태
        int a = 0; // 전체 선택 체크박스가 활성화 됬을때 한번만 체크박스 함수가 실행되도록 하는 int형 변수
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
        private void Thread_DoWork(object sender, DoWorkEventArgs e)
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
            else
            {
                ExcelInstance = new AccomplishExcelProgramm();
            }
        }
        private void InitLoadWindow()
        {
            lw = new LoadingScreen();
            lw.LoadingError += LoadingError;
            lw.LoadingCancel += LoadingCancel;
            lw.Topmost = true;
            lw.Show();
            loadingThread.RunWorkerAsync();
        }
        private void Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void Thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                lw.Close();
                메인.Topmost = true;
                메인.IsEnabled = true;
                메인.Opacity = 1;
            }
        }
        private void LoadingError(object sender, RoutedEventArgs e)
        {
            ERROR_FLAG = true;
        }
        private void LoadingCancel(object sender, RoutedEventArgs e)
        {
            loadingThread.CancelAsync();
        }
        public SaveExcelScreen()
        {
            InitializeComponent();
            MessageBox.Show(System.IO.Directory.GetCurrentDirectory() + "\\Excel_Test.xls");
            initBW();
            InitLoadWindow();
            ExcelTable.Columns.Add("인증기", typeof(string));
            ExcelTable.Columns.Add("날짜", typeof(string));
            메인.IsEnabled = false;
            메인.Opacity = 0.5;
            콤보박스.Items.Clear();
            아이피주소.Text = "I 인증기 정보ㅣ 인증기를 선택해 주세요";
            수신창.Background = new SolidColorBrush(Color.FromArgb(110, 98, 149, 255));
            DirectoryInfo directoryInfo = new DirectoryInfo(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER");
            DirectoryInfo[] searchfiles2 = directoryInfo.GetDirectories();
            for (int i = 0; i < searchfiles2.Length; i++)
            {
                콤보박스.Items.Add(searchfiles2[i].ToString());
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            ExcelInstance.Quit_NotSave();
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).IsEnabled = true;
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).Opacity = 1;
        }
        void WriteDataToExcel(object sender, EventArgs e)
        {
            if (excelstate != 0 && currentData != null)
            {
                ExcelInstance.SetWorkSheet(currentGT10_num);
                int word_line = currentData.Length / 9;
                ExcelInstance.WriteDataToExcel(word_line, currentData, 결재1.Text, 결재2.Text, 결재3.Text, 결재1_1.Text, 결재2_1.Text, 결재3_1.Text);
                excelcount++;
                MessageBox.Show("인증기 데이터가 업로드 되었습니다.");
                int a = 0;
                for (int i = 0; i < ExcelTable.Select().Length; i++)
                {
                    if (ExcelTable.Select()[i].ItemArray[0].Equals(콤보박스.SelectedItem.ToString().Replace("GT10_", "") + "번 인증기"))
                    {
                        ExcelTable.Rows.Remove(ExcelTable.Select()[i]);
                    }
                }
                if (a == 0)
                {
                    ExcelTable.Rows.Add(new string[] { 콤보박스.SelectedItem.ToString().Replace("GT10_", "") + "번 인증기", currentDate });
                }
                엑셀데이터.ItemsSource = ExcelTable.DefaultView;
                list2.Clear();
            }
            else
            {
                MessageBox.Show("엑셀에 올라갈 상태가 아닙니다.\n인증기를 선택하고 해당 날짜를 지정해 주세요.");
            }
        }

        private void AllDataSave(object sender, RoutedEventArgs e)
        {
            if (excelcount <= 0)
            {
                System.Windows.MessageBox.Show("Excel에 등록된 인증기 데이터가 존재하지 않습니다.", "");
            }
            else
            {
                string path;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel file (*.xls)|*.xls";
                if (saveFileDialog.ShowDialog() == true)
                {
                    path = saveFileDialog.FileName;
                    if (path.Equals(System.IO.Directory.GetCurrentDirectory() + "\\Excel_Test.xls"))
                    {
                        MessageBox.Show("기본출력용 Excel파일에 저장할 수 없습니다.\n다른경로를 이용해주세요.");
                        return;
                    }
                    ExcelInstance.SaveAndQuit(path);
                    excelcount = 0;
                    MessageBox.Show("성공적으로 저장하였습니다.");
                    MessageBoxResult result = MessageBox.Show("저장한 엑셀 데이터를 여시겠습니까?", "선택창", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            AccomplishExcelProgramm excel = new AccomplishExcelProgramm(path);
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                    savestate++;
                }
                else
                {
                    MessageBox.Show("취소되었습니다.");
                }

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("날짜", typeof(string));
            string path = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + 콤보박스.SelectedItem.ToString();
            string[] directory = Directory.GetFiles(path + "\\", "*.csv");
            string a = 콤보박스.SelectedItem.ToString().Replace("GT10_", "");
            currentGT10_num = Convert.ToInt32(a);
            currentDate = "";
            currentData = null;
            선택날짜.Text = "";
            for (int i = 0; i < directory.Length; i++)
            {
                directory[i] = directory[i].Replace(path + "\\", "");
            }
            for (int i = 0; i < directory.Length; i++)
            {
                dataTable.Rows.Add(new string[] { directory[i].ToString().Replace(".csv", "") });
            }
            데이터.ItemsSource = dataTable.DefaultView;
            아이피주소.Text = "I 인증기 정보ㅣ " + 콤보박스.SelectedItem.ToString().Replace("GT10_", "인증기");
        }
        void ShowData_AllDate(object sender, RoutedEventArgs e)
        {
            if (콤보박스.SelectedItem != null)
            {
                DataTable dataTable = new DataTable();
                string path = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + 콤보박스.SelectedItem.ToString();
                string[] directory = Directory.GetFiles(path + "\\", "*.csv");
                dataTable.Columns.Add("날짜", typeof(string));
                for (int i = 0; i < directory.Length; i++)
                {
                    directory[i] = directory[i].Replace(path + "\\", "");
                    directory[i] = directory[i].Replace(".csv", "");
                    dataTable.Rows.Add(new string[] { directory[i] });
                }
                데이터.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                MessageBox.Show("인증기를 먼저 선택해주세요.");
            }
        }
        void ShowData_DateChanged(object sender, RoutedEventArgs e)
        {
            if (!ResDatePicker1.Text.Equals(""))
            {
                if (콤보박스.SelectedItem != null)
                {
                    DataTable dataTable = new DataTable();
                    string path = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + 콤보박스.SelectedItem.ToString();
                    string[] directory = Directory.GetFiles(path + "\\", ResDatePicker1.Text + ".csv");
                    dataTable.Columns.Add("날짜", typeof(string));
                    for (int i = 0; i < directory.Length; i++)
                    {
                        directory[i] = directory[i].Replace(path + "\\", "");
                        directory[i] = directory[i].Replace(".csv", "");
                        dataTable.Rows.Add(new string[] { directory[i] });
                    }
                    데이터.ItemsSource = dataTable.DefaultView;
                }
                else
                {
                    MessageBox.Show("인증기를 먼저 선택해주세요.");
                }
            }
        }
        void ChooseGT10Data(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    excelstate = 1;
                    var row = (DataGridRow)vis;
                    DataRowView myrow = (DataRowView)데이터.CurrentCell.Item;
                    currentDate = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + 콤보박스.SelectedItem + "\\" + myrow.Row.ItemArray[0].ToString() + ".csv";
                    ReadFile readFile = new ReadFile();
                    GT10_DataLine = readFile.GetData_Line(currentDate);
                    currentDate = currentDate.Replace(System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + 콤보박스.SelectedItem + "\\", "");
                    currentDate = currentDate.Replace(".csv", "");
                    선택날짜.Text = currentDate;
                    var list = new List<string>();
                    for (int i = 0; i < GT10_DataLine.Length; i++)
                    {
                        string[] args = GT10_DataLine[i].Split(',');
                        list.AddRange(args);
                    }
                    currentData = list.ToArray();
                    break;
                }
            }
        }
        //체크 박스가 활성화 되었을때 해당열의 정보를 list2에 추가
        private void ChooseExcelData(object sender, RoutedEventArgs e)
        {
            if (allcheckstate == 0)
            {
                try
                {
                    DataRowView myrow = (DataRowView)엑셀데이터.CurrentCell.Item;
                    list2.Add(myrow.Row.ItemArray[0].ToString());
                    list2.Add(myrow.Row.ItemArray[1].ToString());
                }
                catch
                {
                    return;
                }
            }
            else
            {
                if (a == 0)
                {
                    for (int i = 0; i < ExcelTable.Select().Length; i++)
                    {
                        list2.Add(ExcelTable.Select()[i].ItemArray[0].ToString());
                        list2.Add(ExcelTable.Select()[i].ItemArray[1].ToString());
                    }
                    a++;
                }
            }
        }
        //체크박스가 비활성화 되었을때 해당열의 정보가 list2 배열에 있다면 그 정보 삭제
        private void UnChooseExcelData(object sender, RoutedEventArgs e)
        {
            if (allcheckstate == 0)
            {
                try
                {
                    DataRowView myrow = (DataRowView)엑셀데이터.CurrentCell.Item;
                    list2.Remove(myrow.Row.ItemArray[0].ToString());
                    list2.Remove(myrow.Row.ItemArray[1].ToString());
                }
                catch
                {
                    return;
                }
            }
            else
            {
                if (a == 0)
                {
                    for (int i = 0; i < ExcelTable.Select().Length; i++)
                    {
                        list2.Remove(ExcelTable.Select()[i].ItemArray[0].ToString());
                        list2.Remove(ExcelTable.Select()[i].ItemArray[1].ToString());
                    }
                    a++;
                }
            }
        }

        //전체 체크박스 체크상태 true 하는 함수
        private void AllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            allcheckstate = 1;
            a = 0;
            foreach (CheckBox tb in FindVisualChildren<CheckBox>(엑셀데이터))
            {
                tb.IsChecked = true;
            }
            a = 0;
            allcheckstate = 0;
        }

        //전체 체크박스 체크상태 false로 하는 함수
        private void AllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            allcheckstate = 1;
            a = 0;
            foreach (CheckBox tb in FindVisualChildren<CheckBox>(엑셀데이터))
            {
                tb.IsChecked = false;
            }
            a = 0;
            allcheckstate = 0;
        }

        //전달받은 객체로부터 특정 요소가 있는 검사 및 해당 요소를 반환하는 역할.
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        //선택한 데이터 업로드된 데이터에서 삭제 및 Excel파일 초기화
        private void DeleteChooseExcelData(object sender, RoutedEventArgs e)
        {
            if (list2.Count > 0)
            {
                for (int j = 0; j < list2.Count; j += 2)
                {
                    for (int i = 0; i < ExcelTable.Select().Length; i++)
                    {
                        if (ExcelTable.Select()[i].ItemArray[0].Equals(list2[j]) && ExcelTable.Select()[i].ItemArray[1].Equals(list2[j + 1]))
                        {
                            ExcelTable.Rows.Remove(ExcelTable.Select()[i]);
                        }
                    }
                }
                엑셀데이터.ItemsSource = ExcelTable.DefaultView;
                for (int i = 0; i < list2.Count; i += 2)
                {
                    int number = Convert.ToInt32(list2[i].Replace("번 인증기", ""));
                    ExcelInstance.SetWorkSheet(number);
                    ReadFile readFile = new ReadFile();
                    string path = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + "\\GT10_" + number.ToString() + "\\" + list2[i + 1] + ".csv";
                    string[] data = readFile.GetData(path);
                    int data_length = data.Length / 9;
                    ExcelInstance.ClearWorkSheet(data_length);
                }
                if (ExcelTable.Select().Length <= 0)
                {
                    excelcount = 0;
                }
            }
            else
            {
                MessageBox.Show("삭제할 데이터를 선택해주세요.");
            }
        }

        private void Exit_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //현재 작업한 Excel창 띄우기. 해당 기능을 실행하게 되면 지금까지의 Excel 상태가 초기화 됌.
        private void PrintDataToExcel(object sender, RoutedEventArgs e)
        {
            if (excelcount >= 1)
            {
                MessageBoxResult result = MessageBox.Show("데이터를 출력하게 되면 현재까지 업로드한 Excel데이터는 사라집니다.\n계속하시겠습니까?", "선택창", MessageBoxButton.YesNo);
                list2.Clear();
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ExcelTable = new DataTable();
                        ExcelTable.Columns.Add("인증기", typeof(string));
                        ExcelTable.Columns.Add("날짜", typeof(string));
                        엑셀데이터.ItemsSource = ExcelTable.DefaultView;
                        ExcelInstance.SetWorkSheet_Visible(currentGT10_num);
                        excelcount = 0;
                        excelstate = 0;
                        currentDate = null;
                        선택날짜.Text = "";
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Excel에 등록된 인증기 데이터가 존재하지 않습니다.");
            }
        }

        private void 메인_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
