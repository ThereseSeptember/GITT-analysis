using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GITT_Analysis
{
    class AnalyticalData
    {
        List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //liste med enkelt-talt til udregninger
        List<List<Measurement>> measurements = new List<List<Measurement>>(); //Liste til potential graftegning

        public static List<Measurement> fromFile(string path)
        {
            List<Measurement> measurements = new List<Measurement>();
            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                String[] parts = line.Replace(",", ".").Split("\t");
                if (parts[0] == "mode")
                {
                    continue;
                }
                decimal potential = decimal.Parse(parts[10], NumberStyles.Float, CultureInfo.InvariantCulture);
                decimal time = decimal.Parse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture);
                decimal lithium = decimal.Parse(parts[17], NumberStyles.Float, CultureInfo.InvariantCulture);
                measurements.Add(new Measurement(potential, time, lithium));
            }
            return measurements;
        }
    }
}
