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

namespace Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Teapot _teapot;
        private Model3DWrapper _wrapper;

        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _teapot = new Teapot();
            viewport.Children.Add(_teapot);

            _wrapper = new Model3DWrapper(_teapot, this);

            _wrapper.UpdateCamera += UpdateCamera;

            Loaded += (sender, args) => UpdateCamera(_wrapper.TransformPosition);

            _timer = new Timer(0.05);
            _timer.Elapsed += (sender, args) => _wrapper.Tick();
            _timer.Start();
        }

        private void moveForward_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button) sender;

            var add = b.Name.StartsWith("plus");


            if (b.Name.EndsWith("X"))
            {
                _wrapper.Move(new Vector3D(add ? 0.1:-0.1, 0, 0));
            }
            else if (b.Name.EndsWith("Y"))
            {
                _wrapper.Move(new Vector3D(0, add ? 0.1:-0.1, 0));
            }
            else if (b.Name.EndsWith("Z"))
            {
                _wrapper.Move(new Vector3D(0, 0, add ? 0.1:-0.1));
            }
            else if (b.Name.EndsWith("Roll"))
            {
                _wrapper.Rotate(new PrincipalAxis(0, add ? 10 : -10, 0));
            }
            else if (b.Name.EndsWith("Vel"))
            {
                //_wrapper.Velocity = _wrapper.Velocity + new Vector3D(add ? 0.1 : -0.1, 0, 0);
                _wrapper.AxisVelocity = _wrapper.AxisVelocity + new PrincipalAxis(0, add ? 1:-1, 0);
            }
        }

        private void UpdateCamera(Point3D src)
        {
            viewport.CameraController.CameraTarget = src;
            viewport.CameraController.CameraPosition = new Point3D(src.X - 12.10, src.Y, src.Z + 3.5);
            viewport.CameraController.CameraLookDirection = new Vector3D(1, 0, -0.2);
        }
    }
}
