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
using Simulator.HelixOnly;
using Simulator.Serialization;

namespace Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Controller _gamepad;

        public MainWindow()
        {
            InitializeComponent();

            var robot = Robot.LoadFromFile(@"C:\Users\adam8\Git\DART\Simulator\Simulator\Robots\DartV1\robot.json");

            virtualRobot.LoadRobot(robot);

            _gamepad = new Controller(UserIndex.One);

            Timer t = new Timer(0.1);
            t.Elapsed += TOnElapsed;
            t.Start();
        }

        private void TOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var state = _gamepad.GetState();

            const int scaleFactor = 1;

            Dispatcher.Invoke(() =>
            {
                virtualRobot.Tick();


                virtualRobot._robot.MotorContoller["FrontLeft"].Thrust = (sbyte)(127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                virtualRobot._robot.MotorContoller["FrontRight"].Thrust = (sbyte)(127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                virtualRobot._robot.MotorContoller["BackLeft"].Thrust = (sbyte)(127.0 * ProcessStick(state.Gamepad.LeftThumbY));
                virtualRobot._robot.MotorContoller["BackRight"].Thrust = (sbyte)(127.0 * ProcessStick(state.Gamepad.LeftThumbY));


                var flipLeft = (((state.Gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0) ? -1 : 1);
                var flipRight = (((state.Gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0) ? -1 : 1);

                virtualRobot._robot.MotorContoller["Left"].Thrust =
                    (sbyte) (127.0 * ProcessTrigger(state.Gamepad.LeftTrigger) * flipLeft);

                virtualRobot._robot.MotorContoller["Right"].Thrust =
                    (sbyte) (127.0 * ProcessTrigger(state.Gamepad.RightTrigger) * flipRight);

            });
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
    }
}
    