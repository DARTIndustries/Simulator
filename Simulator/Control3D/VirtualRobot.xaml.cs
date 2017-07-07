using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Scripting.Utils;
using Simulator.Control3D;
using Simulator.Serialization;
using Simulator.Util;

namespace Simulator.HelixOnly
{
    /// <summary>
    /// Interaction logic for VirtualRobot.xaml
    /// </summary>
    public partial class VirtualRobot : UserControl
    {
        private Timer _timer;
        private Dictionary<string, ArrowVisual3D> _thrustArrows;
        public Robot _robot;
        private PhysicsModelGroup _modelGroup;
        public PhysicsSimulator _physics;

        public VirtualRobot()
        {
            InitializeComponent();

            _thrustArrows = new Dictionary<string, ArrowVisual3D>();

            CompositionTarget.Rendering += this.CompositionTargetRendering;

            ShowMotorLabels = true;
        }

        #region Internal Timer

        public void BeginInternalTick()
        {
            _timer = new Timer(0.05);
            _timer.Elapsed += (sender, args) => Tick();
            _timer.Start();
        }

        public void EndInternalTick()
        {
            _timer.Stop();
            _timer = null;
        }

        private void viewport_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _timer?.Stop();
        }

        private void viewport_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer?.Start();
        }

        #endregion

        public void Tick()
        {
            foreach (var key in _robot.MotorContoller.Keys)
            {
                var m = _robot.MotorContoller[key];
                var arrow = _thrustArrows[key];

                _physics.ApplyForce(m.ThrustLocation, m.ThrustVector());

                const int ArrowScale = 5;

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

            viewport.CameraController.CameraTarget = _robot.Model.Transform.Value.Transform(_robot.CenterOfMass);

            _physics.Tick();
        }

        public void LoadRobot(Robot robot)
        {
            _robot = robot;
            
            // Setup the basic viewport
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

            // Add the model 
            viewport.Children.Add(new ModelVisual3D() {Content = robot.Model});

            // Clear thrust arrows, as we're (re)creating them
            _thrustArrows.Clear();

            // Clear the overlay.
            overlay.Children.Clear();

            // Lets make some motors
            foreach (var key in robot.MotorContoller.Keys)
            {
                // Get the given motor
                var m = robot.MotorContoller[key];

                // Create its force arrow
                var v = new ArrowVisual3D()
                {
                    Material = new DiffuseMaterial(Brushes.Red),
                    Point1 = m.ThrustLocation,
                    Point2 = m.ThrustLocation + (4 * m.Direction),
                    Diameter = 0.5
                };
                
                // Add to the collection for easy access later
                _thrustArrows.Add(key, v);
                viewport.Children.Add(v);
            }


            _modelGroup = new PhysicsModelGroup(_robot.Mass, _robot.CenterOfMass, _robot.Model.Bounds);

            _modelGroup.Add(_robot.Model);
            _modelGroup.AddRange(_thrustArrows.Values.Select(x => x.Model));

            _physics = new PhysicsSimulator(_modelGroup);

            //viewport.CameraController.CameraTarget = _robot.CenterOfMass;
        }

        private void PositionUpdated(Size3D modelSize, Point3D src, Vector3D offset)
        {
            if (viewport.CameraController == null) //Camera has not initialized yet
                return;

            // Update the camera postition
            viewport.CameraController.CameraPosition += offset;

            // Update all motor labels
            foreach (TextBlock overlayChild in overlay.Children)
            {
                Overlay.SetPosition3D(overlayChild, Overlay.GetPosition3D(overlayChild) + offset);
            }


        }

        #region Labels
        public bool ShowMotorLabels { get; set; }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (ShowMotorLabels)
            {
                if (overlay.Visibility != Visibility.Visible)
                    overlay.Visibility = Visibility.Visible;

                var matrix = Viewport3DHelper.GetTotalTransform(this.viewport.Viewport);

                foreach (FrameworkElement element in this.overlay.Children)
                {
                    var position = Overlay.GetPosition3D(element);
                    var position2D = matrix.Transform(position);
                    Canvas.SetLeft(element, position2D.X - element.ActualWidth / 2);
                    Canvas.SetTop(element, position2D.Y - element.ActualHeight / 2);
                }
            }
            else
            {
                if (overlay.Visibility != Visibility.Hidden)
                    overlay.Visibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
