using System;
using System.Collections.Generic;
using System.Text;

namespace GITT_Analysis
{
    class DiffMeasurement
    {
        public decimal Es_initial { get; set; }
        public decimal Es_final { get; set; }
        public decimal Et_initial { get; set; }
        public decimal Et_final { get; set; }
        public decimal Time_initial { get; set; }
        public decimal Time_final { get; set; }
        public decimal Lithium_initial { get; set; }
        public decimal Lithium_final { get; set; }
        //constructor
        public DiffMeasurement(decimal Es_ini, decimal Es_fin, decimal Et_ini, decimal Et_fin, decimal Time_ini, decimal Time_fin, decimal Lithium_ini, decimal Litihum_fin)
        {
            this.Es_initial = Es_ini;
            this.Es_final = Es_fin;
            this.Et_initial = Et_ini;
            this.Et_final = Et_fin;
            this.Time_initial = Time_ini;
            this.Time_final = Time_fin;
            this.Lithium_initial = Lithium_ini;
            this.Lithium_final = Litihum_fin;
        }

        /// <summary>
        /// Create a new diff measurement from another diff measurement
        /// </summary>
        /// <param name="diff"></param>
        public DiffMeasurement(DiffMeasurement diff)
        {
            Es_initial = diff.Es_initial;
            Es_final = diff.Es_final;
            this.Et_initial = diff.Et_initial;
            this.Et_final = diff.Et_final;
            this.Time_initial = diff.Time_initial;
            this.Time_final = diff.Time_final;
            this.Lithium_initial = diff.Lithium_initial;
            this.Lithium_final = diff.Lithium_final;
        }

        public DiffMeasurement()
        {

        }

        public DiffMeasurement Clone()
        {
            return new DiffMeasurement(Es_initial, Es_final, Et_initial, Et_final, Time_initial, Time_final, Lithium_initial, Lithium_final);
        }
    }
}
