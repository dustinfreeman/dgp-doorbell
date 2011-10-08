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
        public string dgpID;
        public string emailAddress;
        public string GivenName;
        public string LastName;

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

        public const int EMAIL_LISTING_WIDTH = 250;

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
                eListing.Width = EMAIL_LISTING_WIDTH;
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
