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
                if (emailListPosition < -EmailVisual.EMAIL_WIDTH * emailListStackPanel.Children.Count + 500)
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
                //TODO get rid of re-colouring.
                //uncolour old
                try
                {
                    ((EmailVisual)emailListStackPanel.Children[currentEmailIndex]).border.Background = Brushes.LightGray;
                }
                catch { }

                //colour new elsewhere
                currentEmailIndex = (-(int)emailListPosition + (int)this.Width / 2) / EmailVisual.EMAIL_WIDTH; //current email width;
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

        UIState? _state = null;
        UIState State
        {
            get { return (UIState)_state; }
            set {
                if (_state == value) { return; } //only NEW states

                //old state.
                switch (_state)
                {
                    case UIState.Standby:
                        userImage.Visibility = Visibility.Visible;
                        break;

                    case UIState.NameScrolling:
                        LeftScrollArrow.Visibility = Visibility.Hidden;
                        RightScrollArrow.Visibility = Visibility.Hidden;
                        emailListStackPanel.Visibility = Visibility.Hidden;
                        //Reset Email Position
                        EmailListPosition = -emailListStackPanel.Children.Count * EmailVisual.EMAIL_WIDTH / 2.0;

                        break;
                    case UIState.PictureCountdown:
                        depthImage.Visibility = Visibility.Visible;
                        break;
                }

                switch (value)
                {
                    case UIState.Standby:
                        userImage.Visibility = Visibility.Hidden;

                        break;
                    case UIState.NameScrolling:
                        LeftScrollArrow.Visibility = Visibility.Visible;
                        RightScrollArrow.Visibility = Visibility.Visible;
                        emailListStackPanel.Visibility = Visibility.Visible;
                        break;
                    case UIState.PictureCountdown:
                        depthImage.Visibility = Visibility.Hidden;
                        TakePictureForEmail();
                        break;
                    case UIState.PictureOptions:
                        //send, retake
                        //TODO PictureOptions
                        break;
                }

                _state = value;
            }
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

            SendEmail = new Action(SendEmailNow);

            LeftScrollArrow.SetScrollDirn(ScrollDirn.Left);
            RightScrollArrow.SetScrollDirn(ScrollDirn.Right);

            LeftScrollArrow.ActivatedWParam += new Action<double>(ScrollArrow_Scrolled);
            RightScrollArrow.ActivatedWParam += new Action<double>(ScrollArrow_Scrolled);

            State = UIState.Standby;
        }

        public void ControlPointAppear(Point ctrlPt, Point anchor, int ID)
        {
            Console.WriteLine("Appear " + ID);
            CurrentSkeletonID = ID;
            State = UIState.NameScrolling;

            ControlPointUpdate(ctrlPt, anchor);
        }

        public const double CONTROL_THRESHOLD = 60;
        public const double CONTROL_OFFSET = 40;
        public const double SCROLL_RATE = 20;

        public const int EMAIL_PROGRESS_FRAMES_NEEDED = 35;
        int EmailProgressFrames = 0;

        EmailVisual CurrentEmailVisual;

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
                if (result is GestureWidget)
                {
                    ((GestureWidget)result).ControlPointHits();
                }
            }

            
            foreach(UIElement uie in userCanvas.Children)
            {
                if (uie is GestureWidget)
                {
                    ((GestureWidget)uie).ControlPointUpdate(ctrlPt);
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

                

                PicturingTakingTimer.Stop();

                this.State = UIState.PictureOptions;

                
            }
        }

        void SendEmailNow()
        {
            string ImagePath = Photo.Save(mainWindow.GetCurrentImage(), 640, 480);

            int r = 0;

            r = Email.SendEmail(CurrentEmailVisual.emailAddress, "DGP Doorbell", "You have someone at the door!", ImagePath);
            
            switch (r)
            {
                case 0:
                    if (!Settings.Debug)
                        ShowNotification("Email sent to: " + CurrentEmailVisual.GivenName);
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

        public void ControlPointLose()
        {
            State = UIState.Standby;
            CurrentSkeletonID = -1;
        }

        void ParseEmailList()
        {
            StreamReader reader = new StreamReader("emailList.csv");

            List<EmailVisual> EmailVisuals = new List<EmailVisual>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                EmailVisual eVisual = EmailVisual.EmailVisualFactory(line);
                if (eVisual != null)
                {
                    EmailVisuals.Add(eVisual);
                }
            }

            EmailVisuals.Sort();
            foreach (EmailVisual eVisual in EmailVisuals)
            {
                emailListStackPanel.Children.Add(eVisual);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Debug)
                DebugMode.Visibility = Visibility.Visible;
            else
                DebugMode.Visibility = Visibility.Hidden;

            EmailListPosition = -emailListStackPanel.Children.Count * EmailVisual.EMAIL_WIDTH / 2.0;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DebugMode.SetValue(Canvas.LeftProperty, (double)(userCanvas.Width - DebugMode.Width) - 20);
            DebugMode.SetValue(Canvas.TopProperty, (double)(userCanvas.Height - DebugMode.Height - 20));
        }

    }

    public enum UIState
    {
        Standby,
        NameScrolling,
        PictureCountdown,
        PictureOptions,
    }
}
