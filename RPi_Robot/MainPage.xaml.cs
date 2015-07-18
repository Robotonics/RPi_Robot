using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPi_Robot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static Stopwatch stopwatch;
        public MainPage()
        {
            this.InitializeComponent();
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // initialize GPIO components
            InitRobot.GpioInit();

            // activate the IR sensor display
            ObjectDetection();

            // begin returning sonar data
            GetDistance();


        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlCmds(Commands.Stop);
        }

        private void fwdBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlCmds(Commands.Forward);
        }

        private void rightBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlCmds(Commands.Right);
        }

        private void leftBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlCmds(Commands.Left);
        }

        private void revBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlCmds(Commands.Reverse);
        }


        /// <summary>
        /// Key down and key up functions passes user input to the VKeyRobotInput function.
        /// Key down passes allong the acctual command provided by the user. After the user
        /// stops giving a particular command, CmdKeyUp gets called which passes along
        /// a stop command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmdKeyDown(object sender, KeyRoutedEventArgs e)
        {
            VKeyRobotInput(e.Key);
        }

        private void CmdKeyUp(object sender, KeyRoutedEventArgs e)
        {
            VKeyRobotInput(Windows.System.VirtualKey.Space);
        }

        static void VKeyRobotInput(Windows.System.VirtualKey vkey)
        {
            // Keyboard input for robot commands are:
            // Fwd:         W
            // Rev:         S
            // Left:        A
            // Right:       D
            // Stop:        SPACE
            // Pan Servo:   LEFT/RIGHT (arrow keys)
            // Tilt Servo:  UP/DOWN (arrow keys)
            switch(vkey)
            {
                case Windows.System.VirtualKey.W:
                    ControlCmds(Commands.Forward);
                    break;
                case Windows.System.VirtualKey.S:
                    ControlCmds(Commands.Reverse);
                    break;
                case Windows.System.VirtualKey.A:
                    ControlCmds(Commands.Left);
                    break;
                case Windows.System.VirtualKey.D:
                    ControlCmds(Commands.Right);
                    break;
                default:
                case Windows.System.VirtualKey.Space:
                    ControlCmds(Commands.Stop);
                    break;
            }
        }

        public enum Commands { Stop, Forward, Left, Right, Reverse };

        public static void ControlCmds(Commands cmd)
        {
            switch(cmd)
            {
                case Commands.Forward:
                    InitRobot.Forward();
                    break;
                case Commands.Reverse:
                    InitRobot.Reverse();
                    break;
                case Commands.Left:
                    InitRobot.SpinLeft();
                    break;
                case Commands.Right:
                    InitRobot.SpinRight();
                    break;
                default:
                case Commands.Stop:
                    InitRobot.Stop();
                    break;
            }
        }

        public static void ObjectDetection()
        {
            MainPage objDetect = new MainPage();
            SolidColorBrush detectedRed = new SolidColorBrush();
            SolidColorBrush noObjWhite = new SolidColorBrush();
            detectedRed.Color = Color.FromArgb(255, 0, 0, 1);
            noObjWhite.Color = Color.FromArgb(244, 244, 245, 1);

            bool lastL = InitRobot.LeftIRObj();
            bool lastR = InitRobot.RightIRObj();

            while (true)
            {
                bool newL = InitRobot.LeftIRObj();
                bool newR = InitRobot.RightIRObj();

                if (newL != lastL || newR != lastR)
                {
                    if (newL == true)
                    {
                        objDetect.leftObjIndicator.Fill = detectedRed;
                    }
                    else
                    {
                        objDetect.leftObjIndicator.Fill = noObjWhite;
                    }
                    if (newR == true)
                    {
                        objDetect.rightObjIndicator.Fill = detectedRed;
                    }
                    else
                    {
                        objDetect.rightObjIndicator.Fill = noObjWhite;
                    }
                }

                lastL = newL;
                lastR = newR;
                new System.Threading.ManualResetEvent(false).WaitOne(100); // pause thread for 0.1 seconds
            }
        }

        public void GetDistance()
        {
            decimal eTime = InitRobot.GetElapsedTime();
            decimal dist = eTime * 13503.937m; // elapsed time * the speed of sound in inches
            dist = dist / 2;

            while (true)
            {
                decimal newTime = InitRobot.GetElapsedTime();
                decimal newDist = newTime * 13503.937m;
                newDist = newDist / 2;

                if (newDist != dist)
                {
                    var ftAndIn = PrepareOutput(newDist);
                    int feet = ftAndIn.Key;
                    decimal inches = ftAndIn.Value;
                    DistanceText = feet + "ft. " + inches + "in.";
                }

                dist = newDist;
                new System.Threading.ManualResetEvent(false).WaitOne(100);
            }
        }

        public string DistanceText { get; set; }

        static KeyValuePair<int,decimal> PrepareOutput(decimal inches)
        {
            return new KeyValuePair<int, decimal>((int)inches / 12, inches % 12);
        }

    }
}
