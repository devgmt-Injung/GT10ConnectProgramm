namespace GT10ConnectProgramm
{
    internal class ConnectPing //IP통신 상태 체크를 위한 클래스
    {
        public ConnectPing()
        {

        }

        public string Check_GetCorrectIP(string address, string output) //받은 결과를 토대로 IP통신상태 체크
        {
            string result;
            if (address.Contains("169.254") == true) // IP주소가 169.254일때만 체크
            {
                if (output.Contains("만료") == true || output.Contains("전송하지 못했습니다.") == true || output.Contains("일반오류") == true) // 해당 문자열이 있을때 연결 실패
                {
                    result = "연결 실패";
                }
                else if (output.Contains("연결할 수 없습니다.") == true || output.Contains("요청 시간이 만료되었습니다.") == true || output.Contains("호스트를 찾을 수 없습니다.") == true)
                {
                    result = "연결 실패";
                }
                else
                {
                    result = "연결 성공 : " + address;
                }
            }
            else
            {
                result = "올바른 접근 ip가 아닙니다.";
            }
            return result; // 체크 결과 반환
        }
    }
}
