using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Ionic.Zip;
using Newtonsoft.Json;
using Simulator.HelixOnly;

namespace Simulator.Serialization
{
    public class Robot
    {
        public Model3DGroup Model { get; set; }

        public string Name { get; set; }

        public MotorContoller MotorContoller { get; set; }

        public Point3D CenterOfMass { get; set; }

        public int Mass { get; set; }

        public static Robot LoadFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Unable to find Robot", path);

            RobotConfig cfg;
            Model3DGroup model;
            var importer = new ModelImporter();

            if (path.EndsWith(".zip"))
            {
                string temp;
                using (var zip = ZipFile.Read(path))
                using (var reader = new StreamReader(zip["robot.json"].OpenReader()))
                {
                    cfg = JsonConvert.DeserializeObject<RobotConfig>(reader.ReadToEnd());

                    temp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
                    zip[cfg.ModelFile].Extract(temp);
                }

                model = importer.Load(Path.Combine(temp, cfg.ModelFile));
            }
            else if (path.EndsWith(".json"))
            {
                cfg = JsonConvert.DeserializeObject<RobotConfig>(File.ReadAllText(path));

                model = importer.Load(Path.Combine(Path.GetDirectoryName(path), cfg.ModelFile));
            }
            else
                throw new FormatException();


            var matrix = model.Transform.Value;

            matrix.Scale(new Vector3D(cfg.ScalingFactor, cfg.ScalingFactor, cfg.ScalingFactor));

            foreach (var cfgRotation in cfg.Rotations)
            {
                matrix.Rotate(new Quaternion(cfgRotation.Vector, cfgRotation.Angle));
            }

            model.Transform = new MatrixTransform3D(matrix);

            var controller = new MotorContoller();

            foreach (var motor in cfg.Motors)
            {
                controller.Register(motor.Key, new Motor(motor.Vector)
                {
                    LabelLocation = motor.LabelLocation,
                    ThrustLocation = motor.ThrustLocation
                });
            }

            return new Robot()
            {
                Model = model,
                Name = cfg.Name,
                MotorContoller = controller,
                CenterOfMass = cfg.CenterOfMass,
                Mass = cfg.Mass
            };
        }

    }

    public class RobotConfig
    {
        public string Name { get; set; }

        public string ModelFile { get; set; }

        public List<Rotation> Rotations { get; set; }

        public List<MotorConfiguration> Motors { get; set; }

        public double ScalingFactor { get; set; }

        public Point3D CenterOfMass { get; set; }

        public int Mass { get; set; }
    }

    public struct Rotation
    {
        public Vector3D Vector { get; set; }
        public double Angle { get; set; }
    }

    public struct MotorConfiguration
    {
        public string Key { get; set; }

        public Vector3D Vector { get; set; }

        public Point3D LabelLocation { get; set; }

        public Point3D ThrustLocation { get; set; }
    }
}
