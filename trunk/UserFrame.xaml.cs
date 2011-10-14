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
                double oldEmailListPosition = emailListPosition;
                
                emailListPosition = value;
                if (emailListPosition > 500)
                {
                    emailListPosition = 500;
                }
                if (CurrentEmailIndex >= emailListStackPanel.Children.Count)
                {
                    emailListPosition = oldEmailListPosition;
                }

                if (this.IsLoaded)
                {
                    this.emailListStackPanel.SetValue(Canvas.LeftProperty, emailListPosition);
                    Console.WriteLine(emailListPosition);
                }
            }
        }

        int currentEmailIndex = 0;
        int CurrentEmailIndex
        {
            get
            {
                //uncolour old
                try
                {
                    ((EmailListing)emailListStackPanel.Children[currentEmailIndex]).border.Background = Brushes.LightGray;
                }
                catch { }

                //colour new elsewhere
                currentEmailIndex= (-(int)emailListPosition + (int)this.Width / 2) / EmailListing.EMAIL_LISTING_WIDTH; //current email width;
                return currentEmailIndex;
            }

        }

        bool SuppressEmailing = false;
        DispatcherTimer SuppressionTimer;

        bool Selected = false;

        public MainWindow mainWindow;
        Action SendEmail;

        int NumFlicks = 0;

        //relative to Control Point
        Rect EmailHitRect = new Rect(new Point(2*CONTROL_THRESHOLD, -5 * CONTROL_THRESHOLD), new Size(40, 40));

        public UserFrame()
        {
            InitializeComponent();
            ParseEmailList();

            this.Loaded += new RoutedEventHandler(UserFrame_Loaded);

            SendEmail = new Action(SendEmailNow);

            gui.EmailProgressCanvas.SetValue(Canvas.LeftProperty, EmailHitRect.Left);
            gui.EmailProgressCanvas.SetValue(Canvas.TopProperty, EmailHitRect.Top);
            gui.EmailProgressPoly.SetAngle(0);
        }

        void UserFrame_Loaded(object sender, RoutedEventArgs e)
        {
            ColourCurrentEmail();

        }
        public void ControlPointAppear(Point ctrlPt, Point anchor, int ID)
        {
            CurrentSkeletonID = ID;
            Hand.Visibility = Visibility.Visible;
            gui.Visibility = Visibility.Visible;
            ControlPointUpdate(ctrlPt, anchor);
        }

        public const double CONTROL_THRESHOLD = 60;
        public const double CONTROL_OFFSET = 40;
        public const double SCROLL_RATE = 0.1;

        public const int EMAIL_PROGRESS_FRAMES_NEEDED = 25;
        int EmailProgressFrames = 0;

        EmailListing CurrentEmailListing;

        public void ControlPointUpdate(Point ctrlPt, Point anchor)
        {
            Hand.SetValue(Canvas.LeftProperty, ctrlPt.X - Hand.ActualWidth/2.0);
            Hand.SetValue(Canvas.TopProperty, ctrlPt.Y - Hand.ActualHeight/2.0);

            //This is where the interface control happens.
            Vector DiffVector = ctrlPt - anchor;

            gui.SetValue(Canvas.LeftProperty, anchor.X - gui.ActualWidth / 2.0);
            gui.SetValue(Canvas.TopProperty, anchor.Y - gui.ActualHeight / 2.0);

            //Console.WriteLine(anchor + " " + ctrlPt);

            if (Selected)
            {
                gui.EmailProgressCanvas.Visibility = Visibility.Visible;
                if (EmailHitRect.Left <= DiffVector.X && EmailHitRect.Right >= DiffVector.Y &&
                    EmailHitRect.Top <= DiffVector.Y && EmailHitRect.Bottom >= DiffVector.Y)
                {
                    //start progress
                    EmailProgressFrames++;

                    gui.EmailProgressPoly.SetAngle(EmailProgressFrames / (double)EMAIL_PROGRESS_FRAMES_NEEDED * Math.PI*2);

                    if (EmailProgressFrames >= EMAIL_PROGRESS_FRAMES_NEEDED)
                    {
                        //Send Email
                        EmailNotificationTxt.Text = "Email sent to " + CurrentEmailListing.GivenName + "!";
                        SendEmail.BeginInvoke(null, null);

                        gui.EmailProgressPoly.SetAngle(0);
                        Selected = false;
                    }
                }
                else
                {
                    EmailProgressFrames = 0;
                }
            }
            else
            { //not Selected

                gui.EmailProgressCanvas.Visibility = Visibility.Hidden;


                //scrolling.
                if (DiffVector.X > CONTROL_THRESHOLD + CONTROL_OFFSET)
                {
                    EmailListPosition -= SCROLL_RATE * Math.Abs(DiffVector.X);
                    gui.Right();
                    NumFlicks = 0;

                }
                else if (DiffVector.X < -CONTROL_THRESHOLD + CONTROL_OFFSET)
                {
                    EmailListPosition += SCROLL_RATE * Math.Abs(DiffVector.X);
                    gui.Left();
                    NumFlicks = 0;

                }
                else if (DiffVector.Y < -CONTROL_THRESHOLD * 2)
                {
                    //selecting
                    if (!SuppressEmailing)
                    {
                        SuppressEmailing = true;

                        NumFlicks++;
                        CurrentEmailListing = ((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]);

                        Selected = true;

                        //Not sure if the below is useful anymore - Dustin
                        if (SuppressionTimer != null)
                            SuppressionTimer.Stop();

                        SuppressionTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Render, EndSupression, Dispatcher);

                        EmailNotificationTxt.Visibility = Visibility.Visible;
                        gui.Up();
                    }
                }
                else
                {
                    if (!SuppressEmailing)
                    {
                        gui.ResetGUI();

                    }
                }
            }
            ColourCurrentEmail();
        }

        void SendEmailNow()
        {
            string ImagePath = Photo.Save(mainWindow.GetCurrentImage(), 640, 480);

            Email.SendEmail(CurrentEmailListing.emailAddress, "DGP Doorbell", "You have someone at the door!", ImagePath);

            
        }

        private void EndSupression(object o, EventArgs e)
        {
            SuppressEmailing = false;
            gui.ResetGUI();

            EmailNotificationTxt.Visibility = Visibility.Collapsed;

        }

        void ColourCurrentEmail()
        {
            Brush SelectedBrush = Brushes.LightBlue;
            if (Selected)
                SelectedBrush = Brushes.Blue;

            ((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]).border.Background = SelectedBrush;
        }

        public void ControlPointLose()
        {
            Hand.Visibility = Visibility.Hidden;
            gui.Visibility = Visibility.Hidden;

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
