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
    }
}
