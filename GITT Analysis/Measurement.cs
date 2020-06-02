using System;
using System.Collections.Generic;
using System.Text;

namespace GITT_Analysis
{
    class Measurement
    {
        public decimal Potential { get; set; }
        public decimal Time { get; set; }

        public decimal Lithium { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="potential">The potential from a given gitt measurement</param>
        /// <param name="time"></param>
        public Measurement(decimal potential, decimal time, decimal lithium)
        {
            this.Potential = potential;
            this.Time = time;
            this.Lithium = lithium;
        }
        private void print(List<DiffMeasurement> diffMeasurements)
        {
            throw new NotImplementedException();
        }


    }

    
}
