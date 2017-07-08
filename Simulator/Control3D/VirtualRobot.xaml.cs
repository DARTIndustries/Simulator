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
using Simulator.Control3D.Physics;
using Simulator.Util;

namespace Simulator.Control3D
{
    /// <summary>
    /// Interaction logic for VirtualRobot.xaml
    /// </summary>
    public partial class VirtualRobot : UserControl
    {
        private Dictionary<string, ArrowVisual3D> _thrustArrows;
        public Robot _robot;
        public BulletPhysicsSimulator _physics;

        public VirtualRobot()
        {
            InitializeComponent();

            _thrustArrows = new Dictionary<string, ArrowVisual3D>();

            CompositionTarget.Rendering += this.CompositionTargetRendering;

            ShowMotorLabels = true;
        }
        
        public void Tick(TimeSpan delta)
        {
            foreach (var key in _robot.MotorContoller.Keys)
            {
                var m = _robot.MotorContoller[key];
                var arrow = _thrustArrows[key];

                _physics.ApplyForce(m.ThrustLocation, m.ThrustVector());

                const int ArrowScale = 5;

                if (m.Thrust == 0)
                {
                    arrow.Visible = true;
                }
                else if (m.Thrust > 0)
                {
                    arrow.Visible = true;
                    arrow.Material = new DiffuseMaterial(Brushes.Green);
                    arrow.Point1 = m.ThrustLocation;
                    arrow.Point2 = m.ThrustLocation + ((Math.Abs((int)m.Thrust) / 127.0 * ArrowScale) * m.Direction);
                }
                else if (m.Thrust < 0)
                {
                    arrow.Visible = true;
                    arrow.Material = new DiffuseMaterial(Brushes.Blue);
                    arrow.Point2 = m.ThrustLocation;
                    arrow.Point1 = m.ThrustLocation + ((Math.Abs((int)m.Thrust) / 127.0 * ArrowScale) * m.Direction);
                }
            }

            viewport.CameraController.CameraTarget = _robot.Model.Transform.Value.Transform(_robot.CenterOfMass);

            _physics.Tick(delta);
        }

        public void LoadRobot(Robot robot)
        {
            _robot = robot;
            
            // Setup the basic viewport
            viewport.Children.Clear();
            viewport.Children.Add(new DefaultLights());
            viewport.Children.Add(new GridLinesVisual3D()
            {
                Width = 50,
                Length = 50,
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

            _physics = new BulletPhysicsSimulator();

            _physics.SetPrimaryBody(_robot.Model, (float)_robot.Mass, _robot.CenterOfMass, new Vector3D(0,0,20));

            foreach (var arr in _thrustArrows.Values)
            {
                _physics.AddLinkedBody(arr.Model);
            }
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
