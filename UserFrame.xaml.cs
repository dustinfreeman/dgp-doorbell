﻿using System;
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

using System.Windows.Threading;

using System.IO;

namespace DGPDoorbell
{
    /// <summary>
    /// Interaction logic for UserFrame.xaml
    /// </summary>
    public partial class UserFrame : UserControl
    {
        //presents an interface to a single user.

        public int CurrentSkeletonID = -1;

        double emailListPosition = 0;
        double EmailListPosition
        {
            get
            {
                return emailListPosition;
            }
            set
            {
                emailListPosition = value;
                if (emailListPosition > 500)
                {
                    emailListPosition = 500;
                }
                if (this.IsLoaded)
                {
                    this.emailListStackPanel.SetValue(Canvas.LeftProperty, emailListPosition);
                    Console.WriteLine(emailListPosition);
                }
            }
        }

        const int EMAIL_LISTING_WIDTH = 200;

        int currentEmailIndex = 0;
        int CurrentEmailIndex
        {
            get
            {
                //uncolour old
                ((EmailListing)emailListStackPanel.Children[currentEmailIndex]).border.Background = Brushes.LightGray;

                //colour new.
                currentEmailIndex= (-(int)emailListPosition + (int)this.Width / 2) / EMAIL_LISTING_WIDTH; //current email width;
                return currentEmailIndex;
            }

        }

        bool SuppressEmailing = false;
        DispatcherTimer SuppressionTimer;

        public UserFrame()
        {
            InitializeComponent();
            ParseEmailList();

            this.Loaded += new RoutedEventHandler(UserFrame_Loaded);

        }

        void UserFrame_Loaded(object sender, RoutedEventArgs e)
        {
            ColourCurrentEmail();

        }

        public void ControlPointAppear(Point ctrlPt, Point anchor, int ID)
        {
            CurrentSkeletonID = ID;
            HandEllipse.Visibility = Visibility.Visible;
            ControlPointUpdate(ctrlPt, anchor);
        }

        public const double CONTROL_THRESHOLD = 60;
        public const double CONTROL_OFFSET = 20;
        public const double SCROLL_RATE = 10;

        public void ControlPointUpdate(Point ctrlPt, Point anchor)
        {
            HandEllipse.SetValue(Canvas.LeftProperty, ctrlPt.X - HandEllipse.Width);
            HandEllipse.SetValue(Canvas.TopProperty, ctrlPt.Y - HandEllipse.Height);

            //This is where the interface control happens.
            Vector DiffVector = ctrlPt - anchor;

            if (DiffVector.X > CONTROL_THRESHOLD + CONTROL_OFFSET)
            {
                EmailListPosition -= SCROLL_RATE;
                gui.Right();

            } 
            else if (DiffVector.X < -CONTROL_THRESHOLD + CONTROL_OFFSET)
            {
                EmailListPosition += SCROLL_RATE;
                gui.Left();

            } 
            else if (DiffVector.Y < -CONTROL_THRESHOLD)
            {
                EmailListing CurrentEmailListing = ((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]);

                Email.SendEmail(CurrentEmailListing.emailAddress, "DGP Doorbell", "You have someone at the door!");
                SuppressEmailing = true;

                gui.Up();

                if (SuppressionTimer != null)
                    SuppressionTimer.Stop();

                SuppressionTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Render, EndSupression, Dispatcher);
            }
            else
            {
                if (!SuppressEmailing)
                {
                    gui.ResetGUI();
                }

            }

            ColourCurrentEmail();
        }

        private void EndSupression(object o, EventArgs e)
        {
            SuppressEmailing = false;
            gui.ResetGUI();
        }

        void ColourCurrentEmail()
        {
            ((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]).border.Background = Brushes.LightBlue;
        }

        public void ControlPointLose()
        {
            HandEllipse.Visibility = Visibility.Hidden;
            CurrentSkeletonID = -1;
        }

        void ParseEmailList()
        {
            StreamReader reader = new StreamReader("emailList.csv");

            List<EmailListing> EmailListings = new List<EmailListing>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                EmailListing eListing = EmailListing.EmailListingFactory(line);
                if (eListing != null)
                {
                    EmailListings.Add(eListing);
                }
            }

            EmailListings.Sort();

            foreach (EmailListing eListing in EmailListings)
            {
                emailListStackPanel.Children.Add(eListing);
            }
        }

    }
}