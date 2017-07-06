using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Simulator.Serialization;
using Simulator.Util;

namespace Simulator.Controls
{
    /// <summary>
    /// Interaction logic for VirtualRobot.xaml
    /// </summary>
    public partial class VirtualRobot : UserControl
    {
        public Model3DWrapper _wrapper;
        private Timer _timer;
        private Dictionary<string, ArrowVisual3D> _thrustArrows;
        public Robot _robot;

        public VirtualRobot()
        {
            InitializeComponent();

            _thrustArrows = new Dictionary<string, ArrowVisual3D>();

            CompositionTarget.Rendering += this.CompositionTargetRendering;

            _timer = new Timer(0.05);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _wrapper?.Tick();
        }

        public void LoadRobot(Robot robot)
        {
            _robot = robot;

            _wrapper = new Model3DWrapper(robot.Model, this);
            _wrapper.PositionUpdated += PositionUpdated;

            PositionUpdated(_wrapper.Model.Bounds.Size, _wrapper.TransformPosition, default(Vector3D));

            viewport.Children.Clear();
            viewport.Children.Add(new DefaultLights());
            viewport.Children.Add(new GridLinesVisual3D()
            {
                Width = 30,
                Length = 30,
                Thickness = 0.1,
                MajorDistance = 1,
                MinorDistance = 1
            });
            viewport.Children.Add(new ModelVisual3D() {Content = robot.Model});

            _thrustArrows.Clear();
            overlay.Children.Clear();

            foreach (var key in robot.MotorContoller.Keys)
            {
                var m = robot.MotorContoller[key];

                var text1 = new TextBlock
                {
                    Text = key,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = Brushes.YellowGreen,
                    Background = Brushes.Gray,
                    Padding = new Thickness(4)
                };
                Overlay.SetPosition3D(text1, m.LabelLocation);
                overlay.Children.Add(text1);


                var v = new ArrowVisual3D()
                {
                    Material = new DiffuseMaterial(Brushes.Red),
                    Point1 = m.ThrustLocation,
                    Point2 = m.ThrustLocation + (4 * m.Direction),
                    Diameter = 0.5
                };
                _thrustArrows.Add(key, v);
                viewport.Children.Add(v);
            }
        }

        private void PositionUpdated(Size3D modelSize, Point3D src, Vector3D offset)
        {
            // Calculate something like the center of the model
            var properCenter = new Point3D(
                src.X + (modelSize.Y / 4.0),
                src.Y + (modelSize.Z / 4.0),
                src.Z + (modelSize.X / 4.0));

            if (viewport.CameraController == null) //Camera has not initialized yet
                return;

            // Update the camera postition
            viewport.CameraController.CameraTarget = properCenter;
            viewport.CameraController.CameraPosition += offset;

            // Update all motor labels
            foreach (TextBlock overlayChild in overlay.Children)
            {
                Overlay.SetPosition3D(overlayChild, Overlay.GetPosition3D(overlayChild) + offset);
            }

            foreach (var kvp in _thrustArrows)
            {
                var m = _robot.MotorContoller[kvp.Key];

                var arrow = kvp.Value;

                var matrix = arrow.Transform.Value;
                matrix.Translate(offset);
                arrow.Transform = new MatrixTransform3D(matrix);

                const int ArrowScale = 10;

                if (m.Thrust == 0)
                {
                    arrow.Visible = false;
                }
                else if (m.Thrust > 0)
                {
                    arrow.Visible = true;
                    arrow.Material = new DiffuseMaterial(Brushes.Green);
                    arrow.Point1 = m.ThrustLocation;
                    arrow.Point2 = m.ThrustLocation + ((Math.Abs(m.Thrust) / 127.0 * ArrowScale) * m.Direction);

                }
                else if (m.Thrust < 0)
                {
                    arrow.Visible = true;
                    arrow.Material = new DiffuseMaterial(Brushes.Blue);
                    arrow.Point2 = m.ThrustLocation;
                    arrow.Point1 = m.ThrustLocation + ((Math.Abs(m.Thrust) / 127.0 * ArrowScale) * m.Direction);

                }
            }
        }

        private void viewport_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _wrapper.Paused = true;
        }

        private void viewport_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _wrapper.Paused = false;
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            var matrix = Viewport3DHelper.GetTotalTransform(this.viewport.Viewport);
            
            foreach (FrameworkElement element in this.overlay.Children)
            {
                var position = Overlay.GetPosition3D(element);
                var position2D = matrix.Transform(position);
                Canvas.SetLeft(element, position2D.X - element.ActualWidth / 2);
                Canvas.SetTop(element, position2D.Y - element.ActualHeight / 2);
            }
        }
    }
}
