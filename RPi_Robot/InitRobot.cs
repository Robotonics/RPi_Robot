using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.Foundation;
using Windows.Devices.Gpio;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;




namespace RPi_Robot
{
    /// <summary>
    /// InitRobot class:
    /// Initializes GPIO pins and provides methods for
    /// interfacing with sensors, servos & motors
    /// </summary>
    class InitRobot
    {
        

        // Initialize component pin values
        const int FR_DCMOTOR_PIN = 24;          // Front Right DC Motor     Pin 24      GPIO Pin 8/SPI CE 0
        const int FL_DCMOTOR_PIN = 19;          // Front Left DC Motor      Pin 19      GPIO Pin 10/SPI MOSI
        const int BR_DCMOTOR_PIN = 26;          // Back Right DC Motor      Pin 26      GPIO Pin 7/SPI CE 1
        const int BL_DCMOTOR_PIN = 21;          // Back Left DC Motor       Pin 21      GPIO Pin 9/SPI MISO
        const int FL_IROBJ_PIN = 7;             // Front Left IR Object     Pin 7       GPIO Pin 4/Clock
        const int FR_IROBJ_PIN = 11;            // Front Right IR Object    Pin 11      GPIO Pin 17
        const int FL_IRLINE_PIN = 12;           // Front Left IR Line       Pin 12      GPIO Pin 18/PCM CLK
        const int FR_IRLINE_PIN = 13;           // Front Right IR Line      Pin 13      GPIO Pin 27
        const int SONAR_PIN = 8;                // Ultrasonic Sensor        Pin 8       GPIO Pin 14/UART TTXD
        const int PAN_SERVO_PIN = 22;           // Left/Right Servo         Pin 22      GPIO Pin 25
        const int TILT_SERVO_PIN = 18;          // Up/Down Servo            Pin 18      GPIO Pin 24 

        private static GpioController gpioController = null;
        private static GpioPin frMotorPin = null;
        private static GpioPin flMotorPin = null;
        private static GpioPin brMotorPin = null;
        private static GpioPin blMotorPin = null;
        private static GpioPin flIRObjPin = null;
        private static GpioPin frIRObjPin = null;
        private static GpioPin flIRLinePin = null;
        private static GpioPin frIRLinePin = null;
        private static GpioPin sonarPin = null;
        private static GpioPin panServoPin = null;
        private static GpioPin tiltServoPin = null;


        private static bool GpioInitialized = false;

        /// <summary>
        /// GpioInit initializes GPIO pins. First a component's GPIO pin is
        /// activated, and then 'SetDriveMode' is used to signal whether the
        /// component provides input or output.
        /// </summary>
        public static void GpioInit()
        {
            try
            {
                gpioController = GpioController.GetDefault();
                

                if (null != gpioController)
                {
                    frMotorPin = gpioController.OpenPin(FR_DCMOTOR_PIN);
                    frMotorPin.SetDriveMode(GpioPinDriveMode.Output);

                    flMotorPin = gpioController.OpenPin(FL_DCMOTOR_PIN);
                    flMotorPin.SetDriveMode(GpioPinDriveMode.Output);

                    blMotorPin = gpioController.OpenPin(BL_DCMOTOR_PIN);
                    blMotorPin.SetDriveMode(GpioPinDriveMode.Output);
                    
                    brMotorPin = gpioController.OpenPin(BR_DCMOTOR_PIN);
                    brMotorPin.SetDriveMode(GpioPinDriveMode.Output);

                    flIRObjPin = gpioController.OpenPin(FL_IROBJ_PIN);
                    flIRObjPin.SetDriveMode(GpioPinDriveMode.Input);

                    frIRObjPin = gpioController.OpenPin(FR_IROBJ_PIN);
                    frIRObjPin.SetDriveMode(GpioPinDriveMode.Input);

                    flIRLinePin = gpioController.OpenPin(FL_IRLINE_PIN);
                    flIRLinePin.SetDriveMode(GpioPinDriveMode.Input);

                    frIRLinePin = gpioController.OpenPin(FR_IRLINE_PIN);
                    frIRLinePin.SetDriveMode(GpioPinDriveMode.Input);

                    panServoPin = gpioController.OpenPin(PAN_SERVO_PIN);
                    panServoPin.SetDriveMode(GpioPinDriveMode.Output);

                    tiltServoPin = gpioController.OpenPin(TILT_SERVO_PIN);
                    tiltServoPin.SetDriveMode(GpioPinDriveMode.Output);

                    // set GpioInitialized to true if successful
                    GpioInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: GpioInit Failed: " + ex.Message);
            }
        } // end GpioInit

        //************************//
        //*** SETUP IR SENSORS ***//
        //************************//

        /// <summary>
        /// LeftIRObj reads the current value
        /// of the left IR object sensor.
        /// </summary>
        private static bool LeftIRObj()
        {
            GpioPinValue pinVal = flIRObjPin.Read();
            
            if (pinVal == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // end LeftIRObj

        /// <summary>
        /// RightIRObj reads the current value
        /// of the left IR object sensor.
        /// </summary>
        private static bool RightIRObj()
        {
            GpioPinValue pinVal = frIRObjPin.Read();

            if (pinVal == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // end RightIRObj

        /// <summary>
        /// LeftIRLine reads the current value
        /// of the left IR line sensor
        /// </summary>
        private static bool LeftIRLine()
        {
            GpioPinValue pinVal = flIRLinePin.Read();
            
            if (pinVal == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // end LeftIRLine

        /// <summary>
        /// RightIRLine reads the current value
        /// of the right IR line sensor
        /// </summary>
        private static bool RightIRLine()
        {
            GpioPinValue pinVal = frIRLinePin.Read();

            if (pinVal == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // end RightIRLine

        //*******************************//
        //*** SETUP ULTRASONIC SENSOR ***//
        //*******************************//

        /// <summary>
        /// getElapsedTime uses the ultrasonic sensor to
        /// send out a pulse and records the time it
        /// takes to be read back in by the sensor
        /// </summary>
        private static decimal GetElapsedTime()
        {
            decimal start = 0.00m;
            decimal stop = 0.00m;

            Stopwatch count = new Stopwatch();
            long seed = Environment.TickCount; 

            sonarPin = gpioController.OpenPin(SONAR_PIN);
            sonarPin.SetDriveMode(GpioPinDriveMode.Output);

            sonarPin.Write(GpioPinValue.Low);
            new System.Threading.ManualResetEvent(false).WaitOne(10); // pause thread for 10 microseconds
            sonarPin.Write(GpioPinValue.High);

            count.Start();

            sonarPin.SetDriveMode(GpioPinDriveMode.Input);
            GpioPinValue sonarVal = sonarPin.Read();

            while (sonarVal == 0 && count.ElapsedMilliseconds < 100)
            {
                start = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
                sonarVal = sonarPin.Read(); // update sonarVal
            }

            count.Restart();

            while (sonarVal != 0 && count.ElapsedMilliseconds < 100)
            {
                stop = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
                sonarVal = sonarPin.Read(); // update sonarVal
            }

            decimal elapsedTime = stop - start;
            decimal etSec = elapsedTime * 0.001m; // convert elapsedTime from milliseconds to seconds

            return etSec;

            /* IMPLEMENTATION NOTE:
            *  To get a distance value, multiply the return value (etSec)
            *  by the time it takes the signal to reach a target and return
            *  (e.g. etSec * 34000 = cm/s). Then divide that value by
            *  two to get the distance between the bot and a particular object
            */
        } // end GetElapsedTime

        //*****************************//
        //*** SETUP PAN/TILT SERVOS ***//
        //*****************************//

        // Initialize servo variables
        private enum ServoFunction { Pan, Tilt }; // enumeration holding the pan and the tilt servos
        public enum ServoControl { Stop = -1, Pls1 = 0, Pls2 = 2 };
        public static ServoControl panMove = ServoControl.Stop;
        public static ServoControl tiltMove = ServoControl.Stop;

        private static IAsyncAction run;
        private static ulong ticksPerMs;

        

        //public static int servoSpeed = 10000;

        ///<summary>
        /// ServoInit initializes the pan and tilt servos
        /// </summary>
        public static void ServoInit()
        {
            ticksPerMs = (ulong)(Stopwatch.Frequency) / 1000;

            run = Windows.System.Threading.ThreadPool.RunAsync(
                (source) =>
                {
                    // create a ManualResetEvent and set it to nonsignaled
                    ManualResetEvent mre = new ManualResetEvent(false);
                    mre.WaitOne(1000);
                    
                    //TODO: Setup servo controlls in the controller class

                    while (true)
                    {
                        GenPulse(ServoFunction.Pan);
                        mre.WaitOne(2);
                        GenPulse(ServoFunction.Tilt);
                        mre.WaitOne(2);
                    }
                }, WorkItemPriority.High);

        } // end ServoInit

        /// <summary>
        /// GenPulse sends a pulse to either the pan or tilt servo based
        /// on the ServoFunction parameter passed
        /// </summary>
        /// <param name="servo"></param>
        private static void GenPulse(ServoFunction servo)
        {
            ulong pulseTicks = ticksPerMs;

            if (servo == ServoFunction.Pan)
            {
                if (panMove == ServoControl.Stop)
                {
                    return;
                }
                if (panMove == ServoControl.Pls2)
                {
                    pulseTicks = ticksPerMs * 2;
                }
                // open the panServo pin
                panServoPin.Write(GpioPinValue.High);
            }
            else // tilt servo
            {
                if (tiltMove == ServoControl.Stop)
                {
                    return;
                }
                if (tiltMove == ServoControl.Pls2)
                {
                    pulseTicks = ticksPerMs * 2;
                }
                // open the tiltServo pin
                tiltServoPin.Write(GpioPinValue.High);
            }

            // Keep track of time--Don't close GPIO pins until elapsedTime
            // is greater than (20)ticksPerMs or pulseTicks
            ulong elapsedTime;
            ulong startTime = (ulong)(MainPage.stopwatch.ElapsedTicks);

            while (true)
            {
                // update elapsed time by subracting ElapsedTicks from the
                // ElapsedTicks recorded prior to entering the loop
                elapsedTime = (ulong)(MainPage.stopwatch.ElapsedTicks) - startTime;

                if (elapsedTime > (20 * ticksPerMs) || elapsedTime > pulseTicks)
                {
                    break;
                }
            }

            // Close pins
            if (servo == ServoFunction.Pan)
            {
                panServoPin.Write(GpioPinValue.Low);
            }
            else // ServoFunction.Tilt
            {
                tiltServoPin.Write(GpioPinValue.Low);
            }
        } // end GenPulse

        //***********************//
        //*** SETUP DC Motors ***//
        //***********************//
        
        public static void Stop()
        {
            frMotorPin.Write(GpioPinValue.Low);
            flMotorPin.Write(GpioPinValue.Low);
            brMotorPin.Write(GpioPinValue.Low);
            blMotorPin.Write(GpioPinValue.Low);
        }

        public static void Forward()
        {
            frMotorPin.Write(GpioPinValue.High);
            flMotorPin.Write(GpioPinValue.High);
            brMotorPin.Write(GpioPinValue.Low);
            blMotorPin.Write(GpioPinValue.Low);
        }

        public static void Reverse()
        {
            frMotorPin.Write(GpioPinValue.Low);
            flMotorPin.Write(GpioPinValue.Low);
            brMotorPin.Write(GpioPinValue.High);
            blMotorPin.Write(GpioPinValue.High);
        }
        public static void SpinLeft()
        {
            frMotorPin.Write(GpioPinValue.High);
            flMotorPin.Write(GpioPinValue.Low);
            brMotorPin.Write(GpioPinValue.Low);
            blMotorPin.Write(GpioPinValue.High);
        }

        public static void SpinRight()
        {
            frMotorPin.Write(GpioPinValue.Low);
            flMotorPin.Write(GpioPinValue.High);
            brMotorPin.Write(GpioPinValue.High);
            blMotorPin.Write(GpioPinValue.Low);
        }



    } // end Class InitRobot
}
