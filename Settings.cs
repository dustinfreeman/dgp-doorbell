using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace DGPDoorbell
{
    class Settings
    {
        static private bool m_Debug;
        static private bool m_LogNotSent;
        static private int m_Timeout;        

        static public void LoadSettings()
        {            
            try
            {
                using (StreamReader sr = new StreamReader("Settings.txt"))
                {
                    string line;
                    string[] parts;

                    while ((line = sr.ReadLine()) != null)
                    {
                        parts = line.Split('=');

                        switch(parts[0].Trim())
                        {
                            case "Debug":
                                if (Int32.Parse(parts[1]) == 1)
                                    m_Debug = true;
                                else
                                    m_Debug = false;
                                break;
                            case "Timeout":
                                m_Timeout = Int32.Parse(parts[1]);
                                break;
                            case "LogNotSent":
                                if (Int32.Parse(parts[1]) == 1)
                                    m_LogNotSent = true;
                                else
                                    m_LogNotSent = false;

                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
            }
        }

        #region PROPERTIES
        /// <summary>
        /// flag indicating debug mode
        /// </summary>
        static public bool Debug
        {
            get { return m_Debug; }
            set { m_Debug = value; }
        }

        /// <summary>
        /// number of seconds to timeout before next send
        /// </summary>
        static public int Timeout
        {
            get { return m_Timeout; }
            set { m_Timeout = value; }
        }

        static public bool LogNotSent
        {
            get { return m_LogNotSent; }
            set { m_LogNotSent = value; }
        }
        #endregion
    }
}
