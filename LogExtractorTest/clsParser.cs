using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace LogExtractorTest
{
    public class clsParser
    {
        //log파일 읽고 string으로 값 반환
        public string OpenLog(string file_name)
        {
            //file이 존재하지 않으면 file_name 리턴
            if (!File.Exists(file_name))
            {
                return file_name;
            }
            //streamReader에서 file 읽기 사용 선언
            using (StreamReader sr = File.OpenText(file_name))

                //읽어온 파일을 한 덩어리로 리턴
                return sr.ReadToEnd();


        }

        //결과값 파일 생성
        public void SaveLog(string result, string file_name)
        {
            //파일 생성 선언
            using (StreamWriter outfile = new StreamWriter(file_name))
            {
                //string을 파일로 저장
                outfile.Write(result);
            }
        }

        //텍스트 줄단위 가공
        public System.Collections.ArrayList LineSplit(string src)
        {
            // 어레이 리스트 생성
            System.Collections.ArrayList arrLines = new System.Collections.ArrayList();

            // arrLines를 줄단위로 자름
            arrLines.AddRange(src.Split('\n'));

            // 줄단위로 잘라진 결과값 리턴
            return arrLines;

        }

        //2차 가공 - 앞단 자르기
        public string CutOff(string src, string srchStr, int cutType)
        {
            //찾을 문자열의 위치를 알려주는 변수 선언
            int posX;

            //결과값 저장할 변수
            string result = "";

            //원본 문자열에서 찾을 문자열이 있는 위치를 posX에 전달
            posX = src.IndexOf(srchStr);

            //찾을 문자열이 있으면 If실행
            if (posX > -1)
            {

                if (cutType == 1)
                {
                    //cutType = 1 이면 찾을 문자열 앞을 자른다
                    //무궁화꽃이피었습니다 => 꽃 => 꽃이피었습니다
                    result = src.Substring(posX, src.Length - posX);

                }
                else if (cutType == 2)
                {
                    //cutType = 2 이면 문자열 남기고 뒤를 자른다.
                    //무궁화꽃이피었습니다 => 꽃 => 무궁화꽃
                    result = src.Substring(0, posX + srchStr.Length);
                }
                else if (cutType == 3)
                {
                    //cutType = 3 이면 찾을 문자열까지 자른다.
                    //무궁화꽃이피었습니다 => 꽃 => 이피었습니다
                    result = src.Substring(posX + srchStr.Length, src.Length - posX - srchStr.Length);

                }
                else if (cutType == 4)
                {
                    //cutType = 4 이면 찾을 문자열까지 자른다.
                    //무궁화꽃이피었습니다 => 꽃 => 무궁화
                    result = src.Substring(0, posX);

                }
                else
                {
                    // cutType이 지정된 값 이외의 경우엔 원본 문자열 그대로 반환
                    result = src;
                }

            }
            else
            {
                //찾을 문자열이 없으면 원본 문자열 그대로 반환
                result = src;
            }

            // 찾을 문자열을 result에 저장후 반환
            return result;

        }


        //해당 문자열 찾기
        public string FindStr(string src, string srchStr)
        {
            //결과값을 저장할 변수 선언
            string result = "";

            //찾을 문자열이 있으면 result에 원본 문자열 저장 후 반환
            if (src.IndexOf(srchStr) > -1)
            {
                result = src;
            }

            // 찾을 문자열이 없으면 ""값을 가지는 result 반환
            return result;
        }


        // form에서 로그데이터를 받아서 정의된 함수를 이용해서 parsing하는 함수
        public string ParseLog(string srchStr, string LogStr)
        {
            //결과값을 저장할 변수
            string result = "";

            //임시로 라인값을 저장할 변수
            string tempLine = "";

            //for 루프 변수
            int i;

            // 연산값을 저장할 어레이 리스트 arrLines를 생성 ** 왜 생성자를 사용하지 않았는지는 의문
            System.Collections.ArrayList arrLines;

            // 결과값을 저장할 어레이리스트
            System.Collections.ArrayList arrResult = new System.Collections.ArrayList();

            // 연산된 값을 돌려받을 스트링빌더형의 변수 선언
            System.Text.StringBuilder strBuilder = new StringBuilder();

            // 문자열을 입력받아서 줄단위로 나눈 후 arrLines에 저장
            arrLines = this.LineSplit(LogStr);

            // "Info : " 앞 부분을 자르고 찾은 문자열을 arrResult에 축적
            for (i = 0; i < arrLines.Count; i++)
            {
                // 1. CutOff(arrLines[i].ToString(), "Info : ", 3)로 "info : "를 포함한 앞부분을 잘라낸다
                // 2. FindStr((~~~~~~~, 3), srchStr);로 찾을 문자열이 있는 라인을 검색 후 tempLine에 저장한다.
                // 3. 찾을 문자열이 없으면 tempLine에 ""이 저장된다
                tempLine = FindStr(CutOff(arrLines[i].ToString(), "Info : ", 3), srchStr);

                //tempLine에 문자열이 있으면 실행
                if (tempLine.Trim().Length > 0)
                {
                    //arrResult에 tempLine에 있는 찾은 문자열 축적
                    arrResult.Add(tempLine);
                }

            }

            // arrResult에서 각 행의 뒤에 붙어있는 "\r" 삭제하고 각 행을 strBuilder에 축적
            for (i = 0; i < arrResult.Count; i++)
            {
                strBuilder.AppendLine(CutOff(arrResult[i].ToString(), "\r", 4));

            }

            // 스트링빌더형 strBuilder를 스트링으로 변환후 result에 저장
            result = strBuilder.ToString();

            // 결과값 result 반환
            return result;
        }

        // 포함된 문자열 삭제
        public string deleteLog(string srchStr, string LogStr)
        {
            //결과값을 저장할 변수
            string result = "";

            //임시로 라인값을 저장할 변수
            string tempLine = "";

            //for 루프 변수
            int i;

            // 연산값을 저장할 어레이 리스트 arrLines를 생성 ** 왜 생성자를 사용하지 않았는지는 의문
            System.Collections.ArrayList arrLines;

            // 결과값을 저장할 어레이리스트
            System.Collections.ArrayList arrResult = new System.Collections.ArrayList();

            // 연산된 값을 돌려받을 스트링빌더형의 변수 선언
            System.Text.StringBuilder strBuilder = new StringBuilder();

            // 문자열을 입력받아서 줄단위로 나눈 후 arrLines에 저장
            arrLines = this.LineSplit(LogStr);

            // "Info : " 앞 부분을 자르고 찾은 문자열을 arrResult에 축적
            for (i = 0; i < arrLines.Count; i++)
            {
                // 1. CutOff(arrLines[i].ToString(), "Info : ", 3)로 "info : "를 포함한 앞부분을 잘라낸다
                // 2. FindStr((~~~~~~~, 3), srchStr);로 찾을 문자열이 있는 라인을 검색 후 tempLine에 저장한다.
                // 3. 찾을 문자열이 없으면 tempLine에 ""이 저장된다
                tempLine = FindStr(CutOff(arrLines[i].ToString(), "Info : ", 3), srchStr);

                //찾을 문자열이 없으면
                if (tempLine.Trim().Length == 0)
                {
                    //arrResult에 arrLines에 있는 문자열 축적
                    arrResult.Add(arrLines[i].ToString());
                }

            }

            // arrResult에서 각 행의 뒤에 붙어있는 "\r" 삭제하고 각 행을 strBuilder에 축적
            for (i = 0; i < arrResult.Count; i++)
            {
                strBuilder.AppendLine(CutOff(arrResult[i].ToString(), "\r", 4));

            }

            // 스트링빌더형 strBuilder를 스트링으로 변환후 result에 저장
            result = strBuilder.ToString();

            // 결과값 result 반환
            return result;
        }
    }
}
