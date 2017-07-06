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

            var xvel = ProcessStick(state.Gamepad.LeftThumbY);
            var yvel = -1*ProcessStick(state.Gamepad.LeftThumbX);
            var zvel = ProcessStick(state.Gamepad.RightThumbY);

            virtualRobot._wrapper.Velocity = new Vector3D(xvel, yvel, zvel) * scaleFactor;

            var angular = ProcessStick(state.Gamepad.RightThumbX);

            virtualRobot._wrapper.AxisVelocity = new PrincipalAxis(0, 0, angular);

            virtualRobot._robot.MotorContoller["Left"].Thrust = (sbyte)(zvel * 127);
            virtualRobot._robot.MotorContoller["Right"].Thrust = (sbyte)(zvel * 127);
        }

        private double ProcessStick(short stickValue)
        {
            const int deadZone = 5000;

            if ((stickValue > 0 && stickValue < deadZone) || (stickValue < 0 && stickValue > (-1 * deadZone)))
                return 0;

            return stickValue / (double) short.MaxValue;
        }

        private void plusVel_Click(object sender, RoutedEventArgs e)
        {
            virtualRobot._wrapper.Velocity += new Vector3D(1, 0, 0);
        }

        private void minusVel_Click(object sender, RoutedEventArgs e)
        {
            virtualRobot._wrapper.Velocity -= new Vector3D(1, 0, 0);

        }
    }
}
    