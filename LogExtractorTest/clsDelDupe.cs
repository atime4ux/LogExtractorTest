using System;
using System.Collections.Generic;
using System.Text;

namespace LogExtractorTest
{
    public class clsDelDupe
    {
        public void delDupe(StringBuilder stackBuilder, StringBuilder srcBuilder)
        {
            libCommon.clsUtil objUtil = new libCommon.clsUtil();

            string[] srcText;
            int i;

            srcText = objUtil.Split(srcBuilder.ToString(), "\r\n");

            for (i = 0; i < srcText.Length; i++)
            {
                if (stackBuilder.ToString().IndexOf(srcText[i]) < 0)
                {
                    stackBuilder.AppendLine(srcText[i]);
                }
            }
        }
    }
}
