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
                if (this.IsLoaded)
                {
                    this.emailListStackPanel.SetValue(Canvas.LeftProperty, emailListPosition);
                }
            }
        }

        public UserFrame()
        {
            InitializeComponent();
            ParseEmailList();

            this.Loaded += new RoutedEventHandler(UserFrame_Loaded);

        }

        void UserFrame_Loaded(object sender, RoutedEventArgs e)
        {

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

            }
            if (DiffVector.X < -CONTROL_THRESHOLD + CONTROL_OFFSET)
            {
                EmailListPosition += SCROLL_RATE;
            }
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

            //TODO sort in list

            foreach (EmailListing eListing in EmailListings)
            {
                emailListStackPanel.Children.Add(eListing);
            }
        }

    }
}
