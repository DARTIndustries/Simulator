using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using IronPython.Modules;
using Microsoft.Scripting.Utils;
using SharpDX.XInput;
using Simulator.Control3D;
using Simulator.Serialization;

namespace Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Controller _gamepad;
        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            var robot = Robot.LoadFromFile(@"..\..\Robots\DartV1\robot.json");

            virtualRobot.LoadRobot(robot);

            _gamepad = new Controller(UserIndex.One);

            _timer = new Timer(1);
            _timer.Elapsed += TOnElapsed;
        }


        private DateTime _lastTime = DateTime.Now;
        private void TOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var controller = virtualRobot._robot.MotorContoller;

            TimeSpan diff;
            if (elapsedEventArgs != null)
            {
                diff = elapsedEventArgs.SignalTime - _lastTime;
                _lastTime = elapsedEventArgs.SignalTime;
            }
            else
            {
                diff = new TimeSpan(0, 0, 0, 0, (int)_timer.Interval);
            }

            if (_gamepad.IsConnected)
            {
                var state = _gamepad.GetState();

                const int scaleFactor = 1;

                Dispatcher.Invoke(() =>
                {
                    virtualRobot.Tick(diff);

                    controller["FrontLeft"].Thrust =
                        (sbyte) (127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                    controller["FrontRight"].Thrust =
                        (sbyte) (127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                    controller["BackLeft"].Thrust =
                        (sbyte) (127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                    controller["BackRight"].Thrust =
                        (sbyte) (127.0 * ProcessStick(state.Gamepad.LeftThumbY));


                    var flipLeft = (((state.Gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0) ? -1 : 1);
                    var flipRight = (((state.Gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0) ? -1 : 1);

                    controller["Left"].Thrust =
                        (sbyte) (127.0 * ProcessTrigger(state.Gamepad.LeftTrigger) * flipLeft);

                    controller["Right"].Thrust =
                        (sbyte) (127.0 * ProcessTrigger(state.Gamepad.RightTrigger) * flipRight);

                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    if (Keyboard.IsKeyDown(Key.Q))
                        controller["Left"].Thrust = SByte.MaxValue;
                    else if (Keyboard.IsKeyDown(Key.Z))
                        controller["Left"].Thrust = SByte.MinValue;
                    else
                        controller["Left"].Thrust = 0;

                    if (Keyboard.IsKeyDown(Key.E))
                        controller["Right"].Thrust = SByte.MaxValue;
                    else if (Keyboard.IsKeyDown(Key.C))
                        controller["Right"].Thrust = SByte.MinValue;
                    else
                        controller["Right"].Thrust = 0;

                    virtualRobot.Tick(diff);
                });
            }
        }

        private double ProcessTrigger(byte triggerValue)
        {
            const int deadZone = 10;

            if (triggerValue < deadZone)
                return 0;

            return triggerValue / (double)byte.MaxValue;
        }

        private double ProcessStick(short stickValue)
        {
            const int deadZone = 5000;

            if ((stickValue > 0 && stickValue < deadZone) || (stickValue < 0 && stickValue > (-1 * deadZone)))
                return 0;

            return stickValue / (double) short.MaxValue;
        }

        private void StartTick(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void SingleTick(object sender, RoutedEventArgs e)
        {
            TOnElapsed(null, null);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_timer != null)
                _timer.Interval = e.NewValue;
        }
    }
}
    