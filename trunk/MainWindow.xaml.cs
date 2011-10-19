/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do Kinect NUI initialization and
// processing and also to display NUI streams on screen.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
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
using Microsoft.Research.Kinect.Nui;

namespace DGPDoorbell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);

            userFrame1.mainWindow = this;
            
            //this.WindowState = System.Windows.WindowState.Maximized
            //this.Topmost = true;

            Log.FileName = "DoorBellLog.txt";            
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //return;
            //Console.WriteLine(e.NewSize.Width);

            userFrame1.Width = e.NewSize.Width - debugPanel.ActualWidth;
            userFrame1.Height = e.NewSize.Height;// userFrame1.Width * 3 / 4 + userFrame1.emailListStackPanel.ActualHeight;            

            userFrame1.Title.Width = userFrame1.Width * 0.20;
            userFrame1.Title.Height = userFrame1.Title.Width / 2.4;

            userFrame1.Doorbell.FontSize = userFrame1.Title.Height;

            userFrame1.InstructionsImg.Height = e.NewSize.Height - 1.25 * (userFrame1.emailListStackPanel.ActualHeight); ;

            userFrame1.userCanvas.Width = userFrame1.Width - userFrame1.InstructionsImg.ActualWidth;
            userFrame1.userCanvas.Height = e.NewSize.Height - 1.25 * (userFrame1.emailListStackPanel.ActualHeight + userFrame1.Title.Height);

            userFrame1.EmailNotificationTxt.Width = userFrame1.userCanvas.Width;
            userFrame1.EmailNotificationTxt.SetValue(Canvas.BottomProperty, userFrame1.EmailNotificationTxt.ActualHeight);

            userFrame1.userImage.Width = userFrame1.userCanvas.Width;
            userFrame1.userImage.Height = e.NewSize.Height - 1.25 * (userFrame1.emailListStackPanel.ActualHeight + userFrame1.Title.Height);

            userFrame1.depthImage.Width = userFrame1.userCanvas.Width;
            userFrame1.depthImage.Height = e.NewSize.Height - 1.25 * (userFrame1.emailListStackPanel.ActualHeight + userFrame1.Title.Height);
        }

        Runtime nui;
        int totalFrames = 0;
        int lastFrames = 0;
        DateTime lastTime = DateTime.MaxValue;

        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        const int ALPHA_IDX = 3;
        byte[] depthFrame32 = new byte[320 * 240 * 4];
        
        Dictionary<JointID,Brush> jointColors = new Dictionary<JointID,Brush>() { 
            {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
            {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
            {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
            {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
            {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
            {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
            {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
            {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        };

        private void Window_Loaded(object sender, EventArgs e)
        {
            nui = new Runtime();

            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }

            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            lastTime = DateTime.Now;

            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        byte[] convertDepthFrame(byte[] depthFrame16)
        {
            //modified so this doesn't exactly match the Kinect SDK sample code. - Dustin

            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16+1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                depthFrame32[i32 + RED_IDX] = 0;
                depthFrame32[i32 + GREEN_IDX] = 0;
                depthFrame32[i32 + BLUE_IDX] = 0;
                depthFrame32[i32 + ALPHA_IDX] = 255;

                
                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 2);
                        break;
                    case 1:
                        depthFrame32[i32 + RED_IDX] = intensity;
                        break;
                    case 2:
                        depthFrame32[i32 + GREEN_IDX] = intensity;
                        break;
                    case 3:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 4:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);
                        break;
                    case 5:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 6:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 7:
                        depthFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);
                        break;
                }

                //only highlight main player if a skeleton is using the system.
                if (SkeletonsVisible && player != userFrame1.CurrentSkeletonID + 1)
                {
                    depthFrame32[i32 + ALPHA_IDX] = 0;
                }
            }
            return depthFrame32;
        }

        byte[] lastDepthFrame16 = new byte[320 * 480 * 2];

        byte[] GrayScaleDepth = new byte[320 * 240];

        int[] DepthIndicesFromColour = new int[640 * 480];

        byte[] drawColourHere = new byte[640 * 480];
        const int FLAG_BEHIND_DIST_THRESHOLD = 255;
        const int FLAG_USE_DEPTH = 254;
        //0 = blank/no data.
        //+ve = player id.

        const double BLACKOUT_DISTANCE_THRESHOLD = 5;

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action<ImageFrameReadyEventArgs>(this.ProcessDepthFrame), new object[] { e });
            
        }

        void ProcessDepthFrame(ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;

            byte[] convertedDepthFrame = convertDepthFrame(Image.Bits);

            userFrame1.depthImage.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgra32, null, convertedDepthFrame, Image.Width * 4);

            if (!userFrame1.CountingDownForPicture)
            {
                if (SkeletonsVisible)
                {
                    userFrame1.depthImage.Opacity = 0.3;
                }
                else
                {
                    userFrame1.depthImage.Opacity = 1.0;
                }
            }

            Image.Bits.CopyTo(lastDepthFrame16, 0);

            //CalculateBackgroundRemoval();
        }

        void CalculateBackgroundRemoval()
        {
            drawColourHere = new byte[640 * 480];

            for (int i =0; i < DepthIndicesFromColour.Length; i++)
            {
                DepthIndicesFromColour[i] = -1;
            }

            int colourX, colourY;

            for (int y = 0; y < 240; y++)
            {
                for (int x = 0; x < 320; x++) //these x and y are the coordinates of the depthFrame.
                {
                    int d = x + y * 320;

                    //remove if behind a certain depth, or skeletal id is set to zero.
                    int player = lastDepthFrame16[d * 2] & 0x07;
                    int realDepth = (lastDepthFrame16[d * 2 + 1] << 5) | (lastDepthFrame16[d * 2] >> 3);

                    GrayScaleDepth[d] = (byte)(255 - (255 * realDepth / 0x0fff));

                    Microsoft.Research.Kinect.Nui.Vector depthVector = nui.SkeletonEngine.DepthImageToSkeleton(x / nui.DepthStream.Width,
                        y / nui.DepthStream.Height, (short)realDepth);

                    short shortDepth = (short)((short)(lastDepthFrame16[d * 2 + 1] << 8) | (short)(lastDepthFrame16[d * 2]));

                    nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, new ImageViewArea(), x, y, shortDepth, out colourX, out colourY);

                    int colourIndex = colourX + colourY * 640;

                    DepthIndicesFromColour[colourIndex] = d; //assign depth index;

                    if (depthVector.Z > BLACKOUT_DISTANCE_THRESHOLD)
                    {
                        drawColourHere[colourIndex] = 255;
                    } else {
                        drawColourHere[colourIndex] = (byte)player;
                    }
                }
            }
        }

        private void Set3x3Patch(byte[] data, int index, int width, byte value)
        {
            for (int _y = -1; _y < 2; _y++)
            {
                for (int _x = -1; _x < 2; _x++)
                {
                    int localIndex = index + width * _y + _x;
                    if (localIndex > data.Length - 1 || localIndex < 0)
                        continue;
                    data[localIndex] = 1;
                }
            }
        }

        void DoSkeletonDisplay(SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;

            int iSkeleton = 0;
            Brush[] brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeleton.Children.Clear();
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        Point jointPos = getDisplayPosition(joint);
                        Line jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeleton.Children.Add(jointLine);
                    }
                }
                iSkeleton++;
            } // for each skeleton
        }

        byte[] lastColorFrame = new byte[640 * 480 * 4];

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action<ImageFrameReadyEventArgs>(this.ProcessColourFrame), new object[] { e });
        }

        void ProcessColourFrame(ImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            PlanarImage Image = e.ImageFrame.Image;
            //video.Source = BitmapSource.Create(
            //    Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);

            ProcessSkeletalControlInput();

            Image.Bits.CopyTo(lastColorFrame, 0);

            //RemoveBackgroundAndHighlightUser();

            userFrame1.userImage.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, lastColorFrame, Image.Width * Image.BytesPerPixel);

            UpdateFPS();
        }

        void RemoveBackgroundAndHighlightUser()
        {
            //will either:
            // - black out
            // - draw depth
            // - draw primary-colour shaded depth (secondary user)
            // - draw full-colour user.

            const int IMAGE_WIDTH = 640;

            //now, erase everywhere where drawHere isn't!
            for (int i = 0; i < drawColourHere.Length; i++)
            {
                int drawSourceIndex = i;
                //do 3x3 search for closest valid depth values in drawColourHere
                // search is CCW, starting at pixel -1 in x direction.
                int searchPosition = 10; 
                while (drawColourHere[drawSourceIndex] == 0 && searchPosition < 9)
                {
                    searchPosition++;
                    switch (searchPosition)
                    {
                        case 1:
                            drawSourceIndex = i - 1;
                            break;
                        case 2:
                            drawSourceIndex = i + IMAGE_WIDTH - 1;
                            break;
                        case 3:
                            drawSourceIndex = i + IMAGE_WIDTH;
                            break;
                        case 4:
                            drawSourceIndex = i + IMAGE_WIDTH + 1;
                            break;
                        case 5:
                            drawSourceIndex = i + 1;
                            break;
                        case 6:
                            drawSourceIndex = i - IMAGE_WIDTH + 1;
                            break;
                        case 7:
                            drawSourceIndex = i - IMAGE_WIDTH;
                            break;
                        case 8:
                            drawSourceIndex = i - IMAGE_WIDTH - 1;
                            break;
                    }

                    if (drawSourceIndex < 0 || drawSourceIndex >= drawColourHere.Length)
                    {
                        //reset to source pixel. searchPosition will keep track of where we are.
                        drawSourceIndex = i;
                    }
                }

                if (searchPosition >= 9 || drawColourHere[drawSourceIndex] == FLAG_BEHIND_DIST_THRESHOLD)
                {
                    //black out.
                    lastColorFrame[4 * i + 0] = 0;
                    lastColorFrame[4 * i + 1] = 0;
                    lastColorFrame[4 * i + 2] = 0;
                    lastColorFrame[4 * i + 3] = 0;
                    return;
                }

                if (drawColourHere[drawSourceIndex] == userFrame1.CurrentSkeletonID)
                {
                    //leave as-is RGB.
                    return;
                }

                if (drawColourHere[drawSourceIndex] == FLAG_USE_DEPTH)
                {
                    //draw grayscale depth
                    lastColorFrame[4 * i + 0] = GrayScaleDepth[drawSourceIndex];
                    lastColorFrame[4 * i + 1] = GrayScaleDepth[drawSourceIndex];
                    lastColorFrame[4 * i + 2] = GrayScaleDepth[drawSourceIndex];
                    lastColorFrame[4 * i + 3] = GrayScaleDepth[drawSourceIndex];
                    return;
                }

                if (drawColourHere[drawSourceIndex] > 0)
                {
                    //draw depth-shaded colour based on player
                    switch (drawColourHere[drawSourceIndex])
                    {
                        case 1:
                            lastColorFrame[4 * i + 0] = GrayScaleDepth[drawSourceIndex]; 
                            lastColorFrame[4 * i + 1] = 0;
                            lastColorFrame[4 * i + 2] = 0;
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                        case 2:
                            lastColorFrame[4 * i + 0] = 0; 
                            lastColorFrame[4 * i + 1] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 2] = 0;
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                        case 3:
                            lastColorFrame[4 * i + 0] = 0;
                            lastColorFrame[4 * i + 1] = 0;
                            lastColorFrame[4 * i + 2] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                        case 4:
                            lastColorFrame[4 * i + 0] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 1] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 2] = 0;
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                        case 5:
                            lastColorFrame[4 * i + 0] = 0;
                            lastColorFrame[4 * i + 1] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 2] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                        case 6:
                            lastColorFrame[4 * i + 0] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 1] = 0;
                            lastColorFrame[4 * i + 2] = GrayScaleDepth[drawSourceIndex];
                            lastColorFrame[4 * i + 3] = 255;
                            break;
                    }
                    return;
                }
            }
        }

        private void Set4Pixels(byte[] data, int startIndex, byte value)
        {
            for (int c = 0; c < 4; c++)
            {
                data[4 * startIndex + c] = 0;
            }
        }

        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(userFrame1.userImage.Width * colorX / 640.0), (int)(userFrame1.userImage.Height * colorY / 480.0));
        }

        Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(getDisplayPosition(joints[ids[i]]));
            }

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        SkeletonFrame lastSkeletonFrame;

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            lastSkeletonFrame = e.SkeletonFrame;

            //DoSkeletonDisplay(e);
        }
        
        public bool SkeletonsVisible = false;

        void ProcessSkeletalControlInput()
        {
            if (lastSkeletonFrame != null)
            {
                SkeletonsVisible = false;
                for (int s = 0; s < lastSkeletonFrame.Skeletons.Length; s++)
                {
                    if (lastSkeletonFrame.Skeletons[s].TrackingState == SkeletonTrackingState.Tracked && lastSkeletonFrame.Skeletons[s].Position.Z < 2.5)
                    {
                        SkeletonsVisible = true;

                        Microsoft.Research.Kinect.Nui.Vector RightHandVector = lastSkeletonFrame.Skeletons[s].Joints[JointID.WristRight].Position;
                        Microsoft.Research.Kinect.Nui.Vector HipCentreVector = lastSkeletonFrame.Skeletons[s].Joints[JointID.HipCenter].Position;

                        Point RightHand = getDisplayPosition(lastSkeletonFrame.Skeletons[s].Joints[JointID.WristRight]);
                        Point HipCentre = getDisplayPosition(lastSkeletonFrame.Skeletons[s].Joints[JointID.HipCenter]);

                        Point RightHandDefault = HipCentre + new System.Windows.Vector(100, 0);

                        if (userFrame1.CurrentSkeletonID == s)
                        {
                            //update control point
                            userFrame1.ControlPointUpdate(RightHand, RightHandDefault);
                        }
                        else if (userFrame1.CurrentSkeletonID == -1)
                        {
                            //new control point
                            userFrame1.ControlPointAppear(RightHand, RightHandDefault, s);
                        }

                        //userFrame1.depthImage.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        if (userFrame1.CurrentSkeletonID == s)
                        {
                            //lose control point
                            userFrame1.ControlPointLose();
                        }
                    }
                }

                if (!SkeletonsVisible && !userFrame1.CountingDownForPicture)
                {
                    userFrame1.depthImage.Visibility = Visibility.Visible;

                }
            }
        }

        public byte[] GetCurrentImage()
        {
            return lastColorFrame;
        }

        void UpdateFPS()
        {
            ++totalFrames;
            DateTime cur = DateTime.Now;
            if (cur.Subtract(lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = totalFrames - lastFrames;
                lastFrames = totalFrames;
                lastTime = cur;
                frameRate.Text = frameDiff.ToString() + " fps";
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
            Environment.Exit(0);
        }

    }
}
