using DieselEngineFormats;
using DieselEngineFormats.Bundle;
using DieselEngineFormats.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MassunitMaker
{
    class MassunitJsonUnit
    {
        public string path { get; set; }
        public float[][] positions { get; set; }
        public float[][] rotations { get; set; }
    }

    class MassunitJson
    {
        public string path { get; set; }
        public MassunitJsonUnit[] units { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*
             * {
             *      "path": "path/to/massunit.massunit",
             *      "units" : [
             *          {
             *              "unit": "5118480363ec24df",
             *              "positions": [[1, 2, 3], [4, 5, 6]],
             *              "rotations": [[0, 0, 0, 0.1], [0.1, 0.2, 0, 0]]
             *          }
             *      ]
             * }
             */
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp/massunit.json");
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("No massunit JSON file found. " + jsonPath);
                Console.ReadLine();
                return;
            }

            string str = File.ReadAllText(jsonPath);

            var jsonMassunit = JsonConvert.DeserializeObject<MassunitJson>(str);
            var headers = new List<MassUnitHeader>();

            foreach (var unit in jsonMassunit.units)
            {
                var positions = new List<Vector3>();
                var roations = new List<Quaternion>();

                foreach (float[] pos in unit.positions)
                {
                    positions.Add(new Vector3(pos[0], pos[1], pos[2]));
                }
                foreach (float[] rot in unit.rotations)
                {
                    roations.Add(new Quaternion(rot[0], rot[1], rot[2], rot[3]));
                }

                headers.Add(new MassUnitHeader(General.SwapEndianness(Convert.ToUInt64(unit.path, 16)), positions, roations));
            }

            var massunit = new MassUnit(headers);
            massunit.WriteFile(jsonMassunit.path);
            File.Delete(jsonPath);
        }

    }
}
