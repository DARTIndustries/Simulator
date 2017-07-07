using System;
using System.IO;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Ionic.Zip;
using Newtonsoft.Json;
using Simulator.Serialization;

namespace Simulator.Control3D
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
}
