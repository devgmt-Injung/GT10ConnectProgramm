namespace GT10ConnectProgramm
{
    internal class CommandPrompt  // 명령프롬포트(CMD)에 ping 명령여를 입력하기 위한 클래스
    {
        public CommandPrompt() // 생성자
        {

        }

        // ping 명령어를 입력하여 결과를 문자열 타입으로 반환하는 함수
        public string InputPingCommand(string address)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process(); // 명령프롬포트 프로세스 객체 생성
            pProcess.StartInfo.FileName = "ping"; // 기본 명령어 Ping
            pProcess.StartInfo.Arguments = "-w 1 -n 1 " + address; // Ping 뒤에 올 부가 설정 (address와 PC간의 통신상태를 1번 1초만 체크하라는 의미)
            pProcess.StartInfo.UseShellExecute = false; // 셸 사용 X (독립적인 프로세스로 사용)
            pProcess.StartInfo.RedirectStandardOutput = true; // 출력 결과 O
            pProcess.StartInfo.CreateNoWindow = true; // 새창 띄우기 X
            pProcess.Start(); //명령프롬포트 실행
            string cmdoutput = pProcess.StandardOutput.ReadToEnd(); // 명령어에 따른 출력된 결과를 cmdoutput에 저장
            pProcess.Close(); //명령프롬포트 종료
            return cmdoutput; //출력 결과 반환
        }
    }
}
