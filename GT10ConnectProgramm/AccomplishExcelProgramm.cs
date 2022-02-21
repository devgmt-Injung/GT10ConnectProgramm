using Microsoft.Office.Interop.Excel;
using System;

namespace GT10ConnectProgramm
{
    internal class AccomplishExcelProgramm //Excel 실행 및 Excel 쓰기, 저장, 지우기 기능 담당하는 클래스
    {
        Microsoft.Office.Interop.Excel.Application application; // Excel 실행 객체
        Workbook workbook; // Excel 특정 파일 실행
        Worksheet worksheet1;  // 특정 Excel 파일의 Worksheet 불러오는 객체
        public AccomplishExcelProgramm()  // Excel_Test.xls 파일 실행하는 생성자(단, 화면을 띄우지 않음.)
        {
            string filepath = System.IO.Directory.GetCurrentDirectory() + "\\Excel_Test.xls"; // 프로그램 설치 경로에 저장되어있는 Excel_Test.xls 파일의 경로를 저장
            application = new Microsoft.Office.Interop.Excel.Application(); // Excel 프로세스 실행
            workbook = application.Workbooks.Open(Filename: @filepath);  // Excel_Test.xls 파일 실행
        }

        public AccomplishExcelProgramm(string filepath) // Excel_Test.xls 파일 실행하는 생성자(화면도 동시에 띄워짐.)
        {
            application = new Microsoft.Office.Interop.Excel.Application();
            workbook = application.Workbooks.Open(Filename: @filepath);
            application.Visible = true; // Excel을 실행할 때, 화면에 나타나게 할건지 정하는 변수 Visible. (true일 경우 화면에 나타남.)
        }
        public void SetWorkSheet(int a) // Worksheet 설정
        {
            /*Excel_Test.xls 파일을 실행할때 Visible 속성을 true값이 할당되어 있을때, Excel_Test.xls창이 나타나게 되지만, 
            인위적으로 Excel_Test.xls창을 닫게될 경우 application 자체가 종료되어버리기 때문에, Worksheet를 불러올 수 없는 에러가 발생함. 
            이에따라 두가지 경우로 나뉘어 try 문으로 실행하였음.*/
            try
            {
                if (application.Visible == true)
                {
                    Quit();
                    string filepath = System.IO.Directory.GetCurrentDirectory() + "\\Excel_Test.xls";
                    application = new Microsoft.Office.Interop.Excel.Application();
                    workbook = application.Workbooks.Open(Filename: @filepath);
                    //기존의 application이 모종의 이유로 종료되었을 경우 catch문으로 넘어감.
                    worksheet1 = workbook.Worksheets.get_Item("인증기" + a); // Worksheet를 결정
                    application.Visible = false; // Excel창을 화면에 나타나게 하지 않음.
                }
                else
                {
                    worksheet1 = workbook.Worksheets.get_Item("인증기" + a); // Worksheet를 결정
                    application.Visible = false;
                }
            }
            catch
            {
                // ============================== AccomplishExcelProgramm() 생성자의 기능을 다시 선언. Excel 프로세스를 재실행 함. ===================================
                string filepath = System.IO.Directory.GetCurrentDirectory() + "\\Excel_Test.xls";
                application = new Microsoft.Office.Interop.Excel.Application();
                workbook = application.Workbooks.Open(Filename: @filepath);
                // =====================================================================================================================================================
                worksheet1 = workbook.Worksheets.get_Item("인증기" + a); // Worksheet 결정
                application.Visible = false; // Excel창을 화면에 나타나게 하지 않음.
            }
        }

        // 특정 데이터(문자열 배열)을 전달받아 Excel에 쓰는 함수
        public void WriteDataToExcel(int words_line, string[] words, string 결재1, string 결재2, string 결재3, string 결재1_1, string 결재2_1, string 결재3_1)
        {
            for (int k = 0; k < words_line; k++)
            {
                Range rg1 = worksheet1.Range[worksheet1.Cells[k + 10, 3], worksheet1.Cells[k + 10, 5]]; // 발급문서명 영역
                rg1.Merge(); // 영역 병합
                Range rg2 = worksheet1.Range[worksheet1.Cells[k + 10, 6], worksheet1.Cells[k + 10, 7]]; // 수수료 영역
                rg2.Merge();
                Range rg3 = worksheet1.Range[worksheet1.Cells[k + 10, 9], worksheet1.Cells[k + 10, 10]]; // 발행소계 영역
                rg3.Merge();
                Range rg4 = worksheet1.Range[worksheet1.Cells[k + 10, 11], worksheet1.Cells[k + 10, 12]]; // 결손매수 영역
                rg4.Merge();
                Range rg5 = worksheet1.Range[worksheet1.Cells[k + 10, 13], worksheet1.Cells[k + 10, 14]]; // 결손소계 영역
                rg5.Merge();
                Range rg6 = worksheet1.Range[worksheet1.Cells[k + 10, 15], worksheet1.Cells[k + 10, 16]]; // 정산합계 영역
                rg6.Merge();
                worksheet1.Cells[k + 10, 2] = words[(k * 9) + 9 - 8]; // 번호
                rg1.Value = words[(k * 9) + 9 - 7];
                rg2.Value = words[(k * 9) + 9 - 6];
                worksheet1.Cells[k + 10, 8] = words[(k * 9) + 9 - 5]; // 발행매수
                rg3.Value = words[(k * 9) + 9 - 4];
                rg4.Value = words[(k * 9) + 9 - 3];
                rg5.Value = words[(k * 9) + 9 - 2];
                rg6.Value = words[(k * 9) + 9 - 1];
            }
            Range rg7 = worksheet1.Range[worksheet1.Cells[5, 2], worksheet1.Cells[6, 2]]; // 월,일(날짜) 영역
            rg7.Merge();
            rg7.Value = words[0];
            //=========== 결재칸 ===========
            worksheet1.Cells[4, 14] = 결재1;
            worksheet1.Cells[4, 15] = 결재2;
            worksheet1.Cells[4, 16] = 결재3;
            worksheet1.Cells[5, 14] = 결재1_1;
            worksheet1.Cells[5, 15] = 결재2_1;
            worksheet1.Cells[5, 16] = 결재3_1;
            //==============================
        }

        // 현재까지 작업한 Excel_Test를 다른이름으로 저장.
        public void SaveAndQuit(string path)
        {
            workbook.Saved = true; // Saved 속성 true시 Excel를 종료할 때 저장하시겠습니까? MessageBox 뜨지 않음. 이럴 경우 SaveAs와 같은 명령어로 저장을 해주어야 저장이 됨.
            try
            {
                workbook.SaveAs(path); // 반환된 특정 경로로 저장
            }
            catch
            {
            }
            /*            workbook.Close(); // Workbook 닫음.
                        DeleteObject(worksheet1); // Worksheet 종료
                        DeleteObject(workbook);  // Workbook(Excel) 종료
                        application.Quit(); // application 객체 종료
                        DeleteObject(application);  // application 종료*/
        }

        //Excel 종료 함수
        public void Quit()
        {
            DeleteObject(worksheet1);
            DeleteObject(workbook);
            application.Quit();
            DeleteObject(application);
        }

        //Excel 종료 함수. 단, 저장을 하지 않음.
        public void Quit_NotSave()
        {
            try
            {
                workbook.Saved = true;
                workbook.Close();
                DeleteObject(workbook);
                application.Quit();
                DeleteObject(application);
            }
            catch
            {
                application.Quit();
                DeleteObject(application);
            }
        }

        // 특정 Worksheet를 초기화 하는 기능
        public void ClearWorkSheet(int words_line)
        {
            for (int k = 0; k < words_line; k++)
            {
                Range rg1 = worksheet1.Range[worksheet1.Cells[k + 10, 3], worksheet1.Cells[k + 10, 5]];
                rg1.Merge();
                Range rg2 = worksheet1.Range[worksheet1.Cells[k + 10, 6], worksheet1.Cells[k + 10, 7]];
                rg2.Merge();
                Range rg3 = worksheet1.Range[worksheet1.Cells[k + 10, 9], worksheet1.Cells[k + 10, 10]];
                rg3.Merge();
                Range rg4 = worksheet1.Range[worksheet1.Cells[k + 10, 11], worksheet1.Cells[k + 10, 12]];
                rg4.Merge();
                Range rg5 = worksheet1.Range[worksheet1.Cells[k + 10, 13], worksheet1.Cells[k + 10, 14]];
                rg5.Merge();
                Range rg6 = worksheet1.Range[worksheet1.Cells[k + 10, 15], worksheet1.Cells[k + 10, 16]];
                rg6.Merge();
                worksheet1.Cells[k + 10, 2] = null;
                rg1.Value = null;
                rg2.Value = null;
                worksheet1.Cells[k + 10, 8] = null;
                rg3.Value = null;
                rg4.Value = null;
                rg5.Value = null;
                rg6.Value = null;
            }
            Range rg7 = worksheet1.Range[worksheet1.Cells[5, 2], worksheet1.Cells[6, 2]];
            rg7.Merge();
            rg7.Value = null;
            worksheet1.Cells[4, 14] = null;
            worksheet1.Cells[4, 15] = null;
            worksheet1.Cells[4, 16] = null;
            worksheet1.Cells[5, 14] = null;
            worksheet1.Cells[5, 15] = null;
            worksheet1.Cells[5, 16] = null;
        }

        // 매개변수에 해당하는 객체 프로세스를 종료하는 함수
        private void DeleteObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                string error = "메모리 할당을 해제하는 중 문제가 발생하였습니다." + ex.ToString() + "경고!";
            }
            finally
            {
                GC.Collect(); // 가비지 컬렉터 실행.
            }
        }

        // 현재 진행중인 Excel창을 화면에 나타나게 함.
        public void SetWorkSheet_Visible(int a)
        {
            workbook.Saved = true; // 종료될 때 저장은 하지 않는다.
            application.Visible = true;
        }

    }
}
