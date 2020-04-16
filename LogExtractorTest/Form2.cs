using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Collections;
using System.Threading;
using System.IO;


//대소문자 구분 방법 추가할것!!!!

namespace LogExtractorTest
{
    public partial class Form2 : Form
    {
        /* 스레드 생성과정에서 서로 다른 프로세스에 접근을 위해서 필요한 델리깃 선언*/
        // 진행바 값을 보내기 위한 델리깃
        public delegate void SetProgCallBack(int progValue);
        // 진행률을 보내기 위한 델리깃
        public delegate void SetLabelCallBack(string progStr);
        // 복사완료 후 창 종료를 위한 델리깃
        public delegate void ExitCallBack();

        // 스레드 선언
        private System.Threading.Thread t1;  

        private string src;
        private string srchWrd;

        //ext : 추출
        //del : 삭제
        private string option;

        //생성
        public Form2(string Source, string Search, string opt)
        {
            InitializeComponent();

            src = Source;
            srchWrd = Search;
            option = opt;
        }

        //실행
        private void Form2_Load(object sender, System.EventArgs e)
        {
            Form1 objForm1 = new Form1();

            // 진행바의 최대값 100으로 설정
            progressBar1.Maximum = 100;

            if (option.Equals("ext"))
            {
                // 스레드 생성
                t1 = new System.Threading.Thread(new ThreadStart(extract));
            }
            else if (option.Equals("del"))
            {
                // 스레드 생성
                t1 = new System.Threading.Thread(new ThreadStart(delete));
            }
            else if (option.Equals("delDupe"))
            {
                t1 = new System.Threading.Thread(new ThreadStart(delDupe));
            }

            //.NET환경에서 스레드 사용시 OLE와의 문제 해결
            t1.ApartmentState = ApartmentState.STA;
            
            // 스레드 시작
            t1.Start();   
        }

        // t1 스레드에서 만들어진 값(vv)을 메인스레드(폼)의 진행바컨트롤에 지정하기 위한 메소드
        private void SetProgBar(int progValue)
        {
            
            // 서로다른 프로세스에서 객체를 크로스로 접근하면 예외가 발생하므로 이를 해결하기 
            // 위해서 Invoke 메소드 사용하게 된다.
            // 진행바가 현재 Invoke가 필요한 상태인지 파악하여 필요하다면 대기상태에 있다가
            // 접근가능할 때 백그라운드 작업을 진행하고, 필요한 상태가 아니라면
            // 진행바의 해당 속성에 바로 대입한다.
            if (this.progressBar1.InvokeRequired)
            {
                // 델리깃 생성
                SetProgCallBack dele = new SetProgCallBack(SetProgBar);

                // 대기상태에 있다가 접근가능한 상태일 때 SetProgBar 간접호출
                this.Invoke(dele, progValue);
            }
            else
            {
                this.progressBar1.Value = progValue;
            }
        }

        // t1 스레드에서 만들어진 값(str)을 메인스레드(폼)의 레이블에 지정하기 위한 메소드
        private void SetLabel(string progStr)
        {
            if (this.label2.InvokeRequired)
            {
                SetLabelCallBack dele = new SetLabelCallBack(SetLabel);
                this.Invoke(dele, progStr);
            }
            else
            {
                this.label2.Text = progStr;
            }
        }

        // t1 스레드에서 메인스레드(폼)의 종료메소드(Close)를 지정하기 위한 메소드 
        private void Exit()
        {
            ExitCallBack dele = new ExitCallBack(Close);

            // 복사진행률 창 닫기
            this.Invoke(dele);
        }

        //중단버튼 눌렀을때
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult Result;

            t1.Suspend();

            Result = MessageBox.Show("작업을 중단할까요?", "정지", MessageBoxButtons.YesNo);

            if (Result == DialogResult.Yes)
            {
                this.Exit();
            }
            else
            {
                t1.Resume();
            }
        }

        //파일로 내용 저장
        private string saveFile(StringBuilder text)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            DialogResult Result;

            saveFileDialog.InitialDirectory = src;
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "텍스트 파일 (*.txt)|*.txt";

            while (saveFileDialog.FileName.Trim().Length == 0)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter srWriter = new StreamWriter(saveFileDialog.FileName);
                    //null문자 제거 후 저장
                    srWriter.Write(text.ToString().Replace("\0", ""));
                    srWriter.Close();
                }
                
                if (saveFileDialog.FileName.Trim().Length == 0)
                {
                    Result = MessageBox.Show("저장을 취소하겠습니까?", "Caution", MessageBoxButtons.YesNo);
                    if (Result == DialogResult.Yes)
                    {
                        return "CANCEL";
                    }
                }
            }
            return "SAVE";
        }

        //삭제
        public void delete()
        {
            // clsParser 클래스 생성
            clsParser objParser = new clsParser();

            //스트림리더
            StreamReader sr;

            StringBuilder strBuilder = new StringBuilder();
            StringBuilder strBuilder2 = new StringBuilder();

            string Result;

            long fileSize = 0;
            long cSize = 0;

            int defBuffer = 2048000;
            int progValue = 0;
            int blockLength;
            int i;

            char[] Temp; ;

            if (src.Length > 0)
            {
                sr = new StreamReader(src, Encoding.Default);
                fileSize = sr.BaseStream.Length;

                //기본 버퍼크기 2048000
                //파일사이즈가 버퍼보다 작으면 파일사이즈로 버퍼 생성
                if (fileSize < defBuffer)
                {
                    blockLength = (int)fileSize;
                }
                else
                {
                    blockLength = defBuffer;
                }
                Temp = new char[blockLength];



                while (sr.Peek() >= 0)
                {
                    //블럭단위로 읽기
                    if ((sr.BaseStream.Length - cSize) < blockLength)
                    {
                        blockLength = (int)(sr.BaseStream.Length - cSize);
                    }

                    //읽어서 스트링으로 저장
                    sr.ReadBlock(Temp, 0, blockLength);
                    for (i = 0; i < blockLength; i++)
                    {
                        strBuilder2.Append(Temp[i]);
                    }

                    //바이트 크기
                    cSize += Encoding.Default.GetBytes(strBuilder2.ToString()).Length;

                    //파서 실행
                    Result = objParser.deleteLog(srchWrd, strBuilder2.ToString());
                    if (Result.Length > 0)
                    {
                        strBuilder.AppendLine(Result);
                    }

                    //스트링 삭제
                    strBuilder2.Remove(0, strBuilder2.Length);

                    //진생상황 표시
                    progValue = (int)((cSize * 100) / fileSize);
                    //블럭 사용때문에 더 커질 수 있음
                    if (progValue > 100)
                    {
                        progValue = 100;
                    }

                    SetProgBar(progValue);
                    SetLabel(progValue + "%");
                }

                //진행상황 닫기
                this.Exit();
            }
            else
            {
                MessageBox.Show("파일을 선택하세요.");
            }

            //파일 저장과정
            if (strBuilder.Length > 0)
            {
                try
                {
                    //취소를 누르면 취소
                    //저장을 누르면 저장
                    if (saveFile(strBuilder).Equals("SAVE"))
                    {
                        MessageBox.Show("저장했습니다.");
                    }
                    else
                    {
                        MessageBox.Show("취소되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("저장할 내용이 없습니다.");
            }
        }
        
        //추출
        public void extract()
        {
            // clsParser 클래스 생성
            clsParser objParser = new clsParser();
            
            //스트림리더
            StreamReader sr;
            
            StringBuilder strBuilder = new StringBuilder();
            StringBuilder strBuilder2 = new StringBuilder();

            string Result;

            long fileSize = 0;
            long cSize = 0;

            int defBuffer = 2048000;
            int progValue = 0;
            int blockLength;
            int i;

            char[] Temp; ;

            if (src.Length > 0)
            {
                sr = new StreamReader(src, Encoding.Default);
                fileSize = sr.BaseStream.Length;

                //기본 버퍼크기 2048000
                //파일사이즈가 버퍼보다 작으면 파일사이즈로 버퍼 생성
                if (fileSize < defBuffer)
                {
                    blockLength = (int)fileSize;
                }
                else
                {
                    blockLength = defBuffer;
                }
                Temp = new char[blockLength];
                


                while (sr.Peek() >= 0)
                {
                    //블럭단위로 읽기
                    if ((sr.BaseStream.Length - cSize) < blockLength)
                    {
                        blockLength = (int)(sr.BaseStream.Length - cSize);
                    }

                    //읽어서 스트링으로 저장
                    sr.ReadBlock(Temp, 0, blockLength);
                    for (i = 0; i < blockLength; i++)
                    {
                        strBuilder2.Append(Temp[i]);
                    }

                    //바이트 크기
                    cSize += Encoding.Default.GetBytes(strBuilder2.ToString()).Length;

                    //파서 실행
                    Result = objParser.ParseLog(srchWrd, strBuilder2.ToString());
                    if (Result.Length > 0)
                    {
                        strBuilder.AppendLine(Result);
                    }

                    //스트링 삭제
                    strBuilder2.Remove(0, strBuilder2.Length);

                    //진생상황 표시
                    progValue = (int)((cSize * 100) / fileSize);
                    //블럭 사용때문에 더 커질 수 있음
                    if (progValue > 100)
                    {
                        progValue = 100;
                    }

                    SetProgBar(progValue);
                    SetLabel(progValue + "%");
                }
                
                //진행상황 닫기
                this.Exit();
            }
            else
            {
                MessageBox.Show("파일을 선택하세요.");
            }
            
            //파일 저장과정
            if (strBuilder.Length > 0)
            {
                try
                {
                    //취소를 누르면 취소
                    //저장을 누르면 저장
                    if (saveFile(strBuilder).Equals("SAVE"))
                    {
                        MessageBox.Show("저장했습니다.");
                    }
                    else
                    {
                        MessageBox.Show("취소되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("저장할 내용이 없습니다.");
            }
        }

        //중복 제거
        public void delDupe()
        {
            //클래스 생성
            clsDelDupe objDelDupe = new clsDelDupe();

            //스트림리더
            StreamReader sr;

            StringBuilder strBuilder = new StringBuilder();//결과값 저장
            StringBuilder strBuilder2 = new StringBuilder();//블럭단위로 읽어서 저장

            long fileSize = 0;
            long cSize = 0;

            int defBuffer = 2048000;
            int progValue = 0;
            int blockLength;
            int i;

            char[] Temp; ;

            if (src.Length > 0)
            {
                sr = new StreamReader(src, Encoding.Default);
                fileSize = sr.BaseStream.Length;

                //기본 버퍼크기 2048000
                //파일사이즈가 버퍼보다 작으면 파일사이즈로 버퍼 생성
                if (fileSize < defBuffer)
                {
                    blockLength = (int)fileSize;
                }
                else
                {
                    blockLength = defBuffer;
                }
                Temp = new char[blockLength];



                while (sr.Peek() >= 0)
                {
                    //블럭단위 설정
                    if ((sr.BaseStream.Length - cSize) < blockLength)
                    {
                        blockLength = (int)(sr.BaseStream.Length - cSize);
                    }

                    //블럭 단위로 읽어서 스트링으로 저장
                    sr.ReadBlock(Temp, 0, blockLength);
                    for (i = 0; i < blockLength; i++)
                    {
                        strBuilder2.Append(Temp[i]);
                    }

                    //바이트 크기
                    cSize += Encoding.Default.GetBytes(strBuilder2.ToString()).Length;

                    //실행
                    objDelDupe.delDupe(strBuilder, strBuilder2);

                    //스트링 삭제
                    strBuilder2.Remove(0, strBuilder2.Length);
                    
                    //진생상황 표시
                    progValue = (int)((cSize * 100) / fileSize);

                    //블럭 사용때문에 더 커질 수 있음
                    if (progValue > 100)
                    {
                        progValue = 100;
                    }

                    SetProgBar(progValue);
                    SetLabel(progValue + "%");
                }

                //진행상황 닫기
                this.Exit();
            }
            else
            {
                MessageBox.Show("파일을 선택하세요.");
            }

            //파일 저장과정
            if (strBuilder.Length > 0)
            {
                try
                {
                    //취소를 누르면 취소
                    //저장을 누르면 저장
                    if (saveFile(strBuilder).Equals("SAVE"))
                    {
                        MessageBox.Show("저장했습니다.");
                    }
                    else
                    {
                        MessageBox.Show("취소되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("저장할 내용이 없습니다.");
            }
        }
    }
}