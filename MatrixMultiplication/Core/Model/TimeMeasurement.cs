using System.Collections.Generic;

namespace MatrixMul.Core.Model
{
    public class TimeMeasurement
    {
        public List<Measurement> Measurements { get; set; }

        public TimeMeasurement()
        {
            Measurements = new List<Measurement>();
        }

        public void AddMeasurement(string name, long start, long end = -1)
        {
            if (end == -1)
            {
                end = Util.GetUnixTimestamp();
            }

            Measurements.Add(new Measurement
            {
                Name = name,
                End = end,
                Start = start,
            });
        }
    }

    public class Measurement
    {
        public string Name { get; set; }
        public long Start { get; set; }
        public long End { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Measurement measurement)
            {
                return measurement.Name == this.Name;
            }

            return false;
        }
    }
}