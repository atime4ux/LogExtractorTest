using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExtractorTest
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        public Form1()
        {
            InitializeComponent();
        }

        //불러오기
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog.RestoreDirectory = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //파일경로 표시
                fileLocation.Text = openFileDialog.FileName;
            }
        }

        //검색
        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked.Equals(true))
            {
                //Form2 객체 생성
                Form2 objForm2 = new Form2(fileLocation.Text, srchWrd.Text, "ext");
                //실행
                objForm2.Show();
            }
            else if (radioButton2.Checked.Equals(true))
            {
                //Form2 객체 생성
                Form2 objForm2 = new Form2(fileLocation.Text, srchWrd.Text, "del");
                //실행
                objForm2.Show();
            }
            else if (radioButton3.Checked.Equals(true))
            {
                //Form2 객체 생성
                Form2 objForm2 = new Form2(fileLocation.Text, srchWrd.Text, "delDupe");
                //실행
                objForm2.Show();
            }
            else
            {
                MessageBox.Show("파일과 폴더 중 검색 방법을 선택하세요.");
            }
        }
    }
}
