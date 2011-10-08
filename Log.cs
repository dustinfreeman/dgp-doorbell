using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DGPDoorbell
{
    class LogInfo
    {
        private string m_Time;
        private string m_Message;

        public LogInfo(string time, string message)
        {
            Time = time;
            Message = message;
        }

        public string Time
        { 
            set { m_Time = value; }
            get { return m_Time; }
        }
        public string Message
        {
            set { m_Message = value; }
            get { return m_Message; }
        }
    }

    class Log
    {
        #region VARIABLES

        static string m_stLogFile = "Log.txt";
        static bool m_bBuffer = false;
        static ArrayList m_LogEntries = new ArrayList();

        #endregion

        static public void WriteDate()
        {            
            using (StreamWriter sw = File.AppendText(m_stLogFile))
            {
                sw.WriteLine("[Date: " + DateTime.Now.ToString("d") + "]");

                sw.Close();
            }         
        }

        static public void Write(string Info)
        {
            if (Buffer)
            {
                m_LogEntries.Add(new LogInfo(DateTime.Now.ToString("HH:mm:ss.fff"), Info));
            }
            else
            {
                using (StreamWriter sw = File.AppendText(m_stLogFile))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " > " + Info);

                    sw.Close();
                }
            }
        }

        static public void Commit()
        {
            if (Buffer)
            {
                using (StreamWriter sw = File.AppendText(m_stLogFile))
                {
                    foreach (LogInfo i in m_LogEntries)
                    {
                        sw.WriteLine(i.Time + " > " + i.Message);
                    } 

                    sw.Close();
                }

                m_LogEntries.Clear();
            }
        }


        #region PROPERTIES

        static public string FileName
        {
            set
            {
                m_stLogFile = value;
            }
            get
            {
                return m_stLogFile;
            }
        }

        static public bool Buffer
        {
            set { m_bBuffer = value; }
            get { return m_bBuffer; }
        }

        #endregion
    }
}
