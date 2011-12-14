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

        double emailListPosition = 0; //in pixels
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
                if (emailListPosition > 0)
                {
                    emailListPosition = 0;
                }
                if (emailListPosition < -EmailListing.EMAIL_LISTING_WIDTH * emailListStackPanel.Children.Count + 500)
                {
                    emailListPosition = oldEmailListPosition;

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
        public int CurrentEmailIndex
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
                if (currentEmailIndex < 0)
                    currentEmailIndex = 0;
                if (currentEmailIndex > emailListStackPanel.Children.Count - 1)
                    currentEmailIndex = emailListStackPanel.Children.Count - 1;
                return currentEmailIndex;
            }

        }

        DispatcherTimer PicturingTakingTimer = null;

        public bool CountingDownForPicture
        {
            get { return PicturingTakingTimer != null && PicturingTakingTimer.IsEnabled; }
        }

        DispatcherTimer NotificationTimer = null;

        bool Selected = false;

        public MainWindow mainWindow;
        Action SendEmail;

        //relative to Control Point
        Rect EmailHitRect = new Rect(new Point(2.5*CONTROL_THRESHOLD, -4 * CONTROL_THRESHOLD), new Size(100, 140));

        public UserFrame()
        {
            InitializeComponent();
            ParseEmailList();

            this.Loaded += new RoutedEventHandler(UserFrame_Loaded);

            SendEmail = new Action(SendEmailNow);

            LeftScrollArrow.SetScrollDirn(ScrollDirn.Left);
            RightScrollArrow.SetScrollDirn(ScrollDirn.Right);

            LeftScrollArrow.ActivatedWParam += new Action<double>(ScrollArrow_Scrolled);
            RightScrollArrow.ActivatedWParam += new Action<double>(ScrollArrow_Scrolled);

            //gui.EmailProgressCanvas.SetValue(Canvas.LeftProperty, EmailHitRect.Left);
            //gui.EmailProgressCanvas.SetValue(Canvas.TopProperty, EmailHitRect.Top);
            //gui.EmailProgressPoly.SetAngle(0);

            //gui.EmailBackgroundRect.Width = EmailHitRect.Width;
            //gui.EmailBackgroundRect.Height = EmailHitRect.Height;

            //gui.EmailProgressPoly.SetValue(Canvas.LeftProperty, -EmailHitRect.Width / 2);
            //gui.EmailProgressPoly.SetValue(Canvas.TopProperty, -EmailHitRect.Height / 2);
        }

       

        void UserFrame_Loaded(object sender, RoutedEventArgs e)
        {
            ColourCurrentEmail();

        }
        public void ControlPointAppear(Point ctrlPt, Point anchor, int ID)
        {
            Console.WriteLine("Appear " + ID);
            CurrentSkeletonID = ID;
            Hand.Visibility = Visibility.Visible;
            //gui.Visibility = Visibility.Visible;
            ControlPointUpdate(ctrlPt, anchor);
        }

        public const double CONTROL_THRESHOLD = 60;
        public const double CONTROL_OFFSET = 40;
        public const double SCROLL_RATE = 20;

        public const int EMAIL_PROGRESS_FRAMES_NEEDED = 35;
        int EmailProgressFrames = 0;

        EmailListing CurrentEmailListing;

        void ScrollArrow_Scrolled(double param)
        {
            EmailListPosition += SCROLL_RATE * param;
        }

        List<object> hitResultsList = new List<object>();
        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        public void ControlPointUpdate(Point ctrlPt, Point anchor)
        {
            hitResultsList.Clear();
            VisualTreeHelper.HitTest(userCanvas, null, new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(ctrlPt));
            foreach (object result in hitResultsList)
            {
                if (result is IGestureWidget)
                {
                    ((IGestureWidget)result).ControlPointHits();
                }
            }

            
            foreach(UIElement uie in userCanvas.Children)
            {
                if (uie is IGestureWidget)
                {
                    ((IGestureWidget)uie).ControlPointUpdate(ctrlPt);
                }
            }

            Hand.SetValue(Canvas.LeftProperty, ctrlPt.X - Hand.ActualWidth/2.0);
            Hand.SetValue(Canvas.TopProperty, ctrlPt.Y - Hand.ActualHeight/2.0);

        }


        int CountUntilPicture = 3;
        void TakePictureForEmail()
        {
            CountUntilPicture = 3;
            EmailNotificationTxt.Text = "Taking Picture in..." + CountUntilPicture;
            EmailNotificationTxt.Visibility = Visibility.Visible;
            //gui.Visibility = Visibility.Hidden;
            Hand.Visibility = Visibility.Hidden;
            depthImage.Visibility = Visibility.Hidden;

            PicturingTakingTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Input, TakePictureCountdown, mainWindow.Dispatcher);
        }

        void TakePictureCountdown(object o, EventArgs e)
        {
            CountUntilPicture--;
            EmailNotificationTxt.Text = "Taking Picture in..." + CountUntilPicture;

            if (CountUntilPicture <=0)
            {
                if (mainWindow.SkeletonsVisible)
                {
                    SendEmailNow();
                }
                else
                {
                    ShowNotification("Can't see anyone! Cancelled.");

                }

                //Reset Email Position
                EmailListPosition = -emailListStackPanel.Children.Count * EmailListing.EMAIL_LISTING_WIDTH / 2.0;

                PicturingTakingTimer.Stop();

                //gui.Visibility = Visibility.Visible;
                Hand.Visibility = Visibility.Visible;

                depthImage.Visibility = Visibility.Visible;
            }
        }

        void SendEmailNow()
        {
            string ImagePath = Photo.Save(mainWindow.GetCurrentImage(), 640, 480);

            int r = 0;

            r = Email.SendEmail(CurrentEmailListing.emailAddress, "DGP Doorbell", "You have someone at the door!", ImagePath);
            
            switch (r)
            {
                case 0:
                    if (!Settings.Debug)
                        ShowNotification("Email sent to: " + CurrentEmailListing.GivenName);
                    else
                        ShowNotification("(Debug) Email sent to: " + Settings.DebugEmail);
                    break;
                case 1: //already sent
                    ShowNotification("Email already sent within " + Settings.Timeout + " s ago");
                    break;
                case -1: //other error
                    ShowNotification("Error: could not send email");
                    break;
            }
        }

        void ShowNotification(string msg)
        {
            EmailNotificationTxt.Visibility = Visibility.Visible;
            EmailNotificationTxt.Text = msg;
            NotificationTimer = new DispatcherTimer(TimeSpan.FromSeconds(3), DispatcherPriority.Input, HideNotification, mainWindow.Dispatcher);
        }

        void HideNotification(object o, EventArgs e)
        {
            EmailNotificationTxt.Visibility = Visibility.Hidden;
            NotificationTimer.Stop();
        }

        void ColourCurrentEmail()
        {
            Brush SelectedBackground = Brushes.LightBlue;
            //Brush SelectedForeground = Brushes.Black;
            if (Selected)
            {
                SelectedBackground = Brushes.CadetBlue;
                //SelectedForeground = Brushes.White;
            }

            ((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]).border.Background = SelectedBackground;
            //((EmailListing)emailListStackPanel.Children[CurrentEmailIndex]).SetForeground(SelectedForeground);

        }

        public void ControlPointLose()
        {
            Hand.Visibility = Visibility.Hidden;
            //gui.Visibility = Visibility.Hidden;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Debug)
                DebugMode.Visibility = Visibility.Visible;
            else
                DebugMode.Visibility = Visibility.Hidden;

            EmailListPosition = -emailListStackPanel.Children.Count * EmailListing.EMAIL_LISTING_WIDTH / 2.0;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DebugMode.SetValue(Canvas.LeftProperty, (double)(userCanvas.Width - DebugMode.Width) - 20);
            DebugMode.SetValue(Canvas.TopProperty, (double)(userCanvas.Height - DebugMode.Height - 20));
        }

    }
}
