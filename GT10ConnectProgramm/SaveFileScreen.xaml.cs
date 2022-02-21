using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace GT10ConnectProgramm
{
    /// <summary>
    /// DataBase.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SaveFileScreen : Window //외부 경로로 파일 저장하는 기능 담당
    {

        string this_GT10 = ""; // 선택할때마다 인증기 명이 저장되는 문자열 변수입니다.
        DataTable dtTable; //전역으로 실행되는 DataTable 객체입니다. 
        int allcheckstate = 0; //DataGrid 데이터목록의 Header에 위치한 CheckBox를 클릭한 상태인지 판별하기 위한 int형 변수 
        int a = 0; // DataGrid 데이터목록의 Header에 위치한 CheckBox를 클릭했을때 한번만 실행되게 해주는 int형 변수
        //SaveFileScreen.xaml 파일이 실행될때 바로 한번 실행합니다.
        List<string> list2 = new List<string>(); //업로드할 파일을 결정할 배열 list2
        public SaveFileScreen()
        {
            InitializeComponent();
            dtTable = new DataTable();
            dtTable.Columns.Add("인증기", typeof(string)); // 열 추가
            dtTable.Columns.Add("데이터날짜", typeof(string));
            저장창.Background = new SolidColorBrush(Color.FromArgb(110, 98, 149, 255)); // 배경색 설정
            DataTable dataTable = new DataTable(); // 인증기목록에 새로운 행,열 요소를 업데이트할 DataTable 형성
            dataTable.Columns.Add("인증기정보", typeof(string));
            DirectoryInfo directoryInfo = new DirectoryInfo(@System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER");
            DirectoryInfo[] searchfiles = directoryInfo.GetDirectories(); // directoryinfo 내의 폴더를 찾습니다.
            for (int i = 0; i < searchfiles.Length; i++)
            {
                dataTable.Rows.Add(new string[] { searchfiles[i].ToString() }); // 찾은 폴더들을 전부 dataTable 객체에 생성해줍니다.
            }
            저장창.Background = new SolidColorBrush(Color.FromArgb(110, 98, 149, 255));
            인증기목록.ItemsSource = dataTable.DefaultView; // 인증기 목록 업데이트
        }
        //인증기 목록의 버튼을 클릭할때마다 데이터목록에 추가되는 함수
        void ShowData(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    list2.Clear();
                    DataRowView myrow = (DataRowView)인증기목록.CurrentCell.Item; // 현재 선택된 인증기목록 행을 대입.
                    string select_GT10 = myrow.Row.ItemArray[0].ToString(); // 인증기명 대입.
                    this_GT10 = select_GT10;
                    select_GT10 = System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + select_GT10;
                    string[] directory = Directory.GetFiles(select_GT10 + "\\", "*.csv"); // 현재 인증기명 폴더 안에 저장된 .csv확장자 파일 검색
                    ReadFile file = new ReadFile();//특정 파일의 데이터를 읽기위한 ReadFile 클래스의 객체 생성
                    var list = new List<string>();
                    var list_date = new List<string>();
                    string[] get_data;
                    for (int i = 0; i < directory.Length; i++)
                    {
                        get_data = file.GetData(directory[i]);
                        list.AddRange(get_data);
                    }
                    get_data = list.ToArray(); // 데이터 저장
                    for (int i = 0; i < (int)(get_data.Length / 9); i++) //get_data중 날짜만 골라 list_date에 대입
                    {
                        list_date.Add(get_data[i * 9]);
                    }
                    get_data = list_date.Distinct().ToArray(); //중복값 삭제한 배열 대입
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("날짜", typeof(string));
                    for (int i = 0; i < get_data.Length; i++)
                    {
                        dataTable.Rows.Add(new string[] { get_data[i] });
                    }
                    dtTable = dataTable;
                    데이터목록.ItemsSource = dataTable.DefaultView; // 데이터 목록에 날짜행열 업로드
                    foreach (CheckBox tb in FindVisualChildren<CheckBox>(데이터목록))
                    {
                        tb.IsChecked = false; // CheckBox_Checked로 이동
                    }
                    break;
                }
            }
        }
        // 현재창 닫기
        private void Exit_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // 현재창을 닫았을때 LaunchScreen.xaml창 활성화 및 명암조정
        private void Window_Closed(object sender, EventArgs e)
        {
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).메인창.IsEnabled = true;
            ((LaunchScreen)System.Windows.Application.Current.MainWindow).메인창.Opacity = 1;
        }

        //마우스 좌측 버튼 누르고 커서 이동시 창 이동
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        //업로드된 데이터를 다른 경로에 저장하는 함수
        private void SaveAsData(object sender, RoutedEventArgs e)
        {
            string[] vs = { };
            if (list2.Count > 0) // 현재 dtTable에 저장된 값이 있다면 통과
            {
                var list = new List<string>(); //데이터를 저장할 list
                string path = this_GT10; //인증기명
                list.Add("\n====================" + path + "====================\n"); //구분할 수 있게 인증기명 추가
                for (int i = 0; i < list2.Count; i++)
                {
                    string filename = list2[i] + ".csv"; //날짜명.csv
                    string final = @System.IO.Directory.GetCurrentDirectory() + "\\DATAFOLDER\\" + path + "\\" + filename; // 최종적인 파일의 경로
                    ReadFile classForFile = new ReadFile(); // 특정 경로의 파일을 읽기위한 ReadFile클래스의 객체 생성
                    list.AddRange(classForFile.GetData_Line(final)); // 줄별로 데이터를 읽어와 list에 저장
                }
                vs = list.ToArray();
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog(); //파일 경로 지정창을 나타나게함.
                if (saveFileDialog.ShowDialog() == true) // 만약 파일 경로가 지정되었다면 통과
                {
                    string filepath = saveFileDialog.FileName;
                    string line_content = "";
                    for (int i = 0; i < vs.Length; i++)
                    {
                        line_content += vs[i] + "\n"; // 배열요소마다 \n 추가해서 줄띄움.
                    }
                    System.IO.File.WriteAllText(filepath, line_content, System.Text.Encoding.Default); //해당 경로로 파일 저장
                    list2.Clear();
                    System.Windows.MessageBox.Show("성공적으로 파일을 저장했습니다.");
                }
                else //아니라면 취소되었다는 메세지 출력
                {
                    System.Windows.MessageBox.Show("취소되었습니다.");
                }
            }
            else //데이터를 선택해달라는 메세지 출력
            {
                System.Windows.MessageBox.Show("업로드할 데이터를 선택해주세요.");
            }
        }

        //DataGrid의 데이터목록, Header에 위치한 CheckBox를 클릭했을 경우 실행됨.
        //창(xaml)에 존재하는 모든 체크박스 체크 상태를 true로 설정.
        private void CheckBox_AllChecked(object sender, RoutedEventArgs e)
        {
            allcheckstate = 1;
            a = 0;
            foreach (CheckBox tb in FindVisualChildren<CheckBox>(데이터목록))
            {
                tb.IsChecked = true; // CheckBox_Checked로 이동
            }
            a = 0;
            allcheckstate = 0;
        }

        //전달받은 객체로 부터 특정 항목이 있는지 검사 및 반환하는 함수
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
        //CheckBox가 체크됬을때 실행되는 함수.
        //현재 활성화된 체크박스 행의 정보를 list2에 저장
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (allcheckstate == 0)
            {
                DataRowView myrow = (DataRowView)데이터목록.CurrentCell.Item;
                list2.Add(myrow.Row.ItemArray[0].ToString());
            }
            else
            {
                if (a == 0)
                {
                    for (int i = 0; i < dtTable.Select().Length; i++)
                    {
                        list2.Add(dtTable.Select()[i].ItemArray[0].ToString());
                    }
                    a++;
                }
            }
        }

        //CheckBox가 체크가 해제됬을때 실행되는 함수.
        //현재 활성화된 체크박스 행의 정보를 list2 배열이 가지고 있다면 해당 값을 삭제함.
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (allcheckstate == 0)
            {
                DataRowView myrow = (DataRowView)데이터목록.CurrentCell.Item;
                list2.IndexOf(myrow.Row.ItemArray[0].ToString());
                list2.Remove(myrow.Row.ItemArray[0].ToString());
            }
            else
            {
                if (a == 0)
                {
                    for (int i = 0; i < dtTable.Select().Length; i++)
                    {
                        list2.Remove(dtTable.Select()[i].ItemArray[0].ToString());
                    }
                    a++;
                }
            }
        }

        //DataGrid 데이터 목록, Header에 위치한 CheckBox의 체크항목이 해제되었을때 실행
        //모든 체크박스의 체크상태를 해제하는 기능을 가짐.
        private void CheckBox_AllUnchecked(object sender, RoutedEventArgs e)
        {
            allcheckstate = 1;
            a = 0;
            foreach (CheckBox tb in FindVisualChildren<CheckBox>(데이터목록))
            {
                tb.IsChecked = false; // CheckBox_UnChecked로 이동
            }
            a = 0;
            allcheckstate = 0;
        }
    }
}
