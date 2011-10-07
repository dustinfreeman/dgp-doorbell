using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGPDoorbell
{
    /// <summary>
    /// Interaction logic for EmailListing.xaml
    /// </summary>
    public partial class EmailListing : UserControl, IComparable<EmailListing>
    {
        string dgpID;
        string emailAddress;
        string GivenName;
        string LastName;

        public EmailListing()
        {
            InitializeComponent();
        }

        public void Update()
        {
            LastNameTxt.Text = LastName;
            GivenNameTxt.Text = GivenName;
            EmailTxt.Text = emailAddress;
        }

        public static EmailListing EmailListingFactory(string line)
        {
            try
            {
                string[] lineSplit = line.Split(',');

                if (lineSplit[4] == "0")
                {   //account we don't want to use.
                    return null;
                }

                EmailListing eListing = new EmailListing();

                eListing.dgpID = lineSplit[0];
                eListing.emailAddress = lineSplit[1];
                eListing.GivenName = lineSplit[2];
                eListing.LastName = lineSplit[3];

                eListing.Update();

                return eListing;
            }
            catch
            {
                return null;
            }
        }

        public int CompareTo(EmailListing other)
        {
            return String.Compare(this.LastName, other.LastName);

        }
    }
}
