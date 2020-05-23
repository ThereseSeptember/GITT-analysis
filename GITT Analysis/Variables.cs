using System;
using System.Collections.Generic;
using System.Text;

namespace GITT_Analysis
{
    class Variables
    {
        //bool dataReached = false;
        public decimal tracker { get; set; }
        public decimal Potential_tracker { get; set; }
        public decimal Time_tracker { get; set; }
        public decimal difference { get; set; }

        //Variables used for measuring collection (6 needed variables per measurement)
        public decimal t_initial { get; set; }
        public decimal t_final { get; set; }
        public decimal E_t_initial { get; set; }
        public decimal E_t_final { get; set; }
        public decimal E_s_initial { get; set; }
        public decimal E_s_final { get; set; }

        public bool direction_up { get; set; } = true;//checks direction of data analysis
        public bool first_measurement { get; set; } = true;//Checks for first measurement

        //constructor
        public Variables()
        {
        }
    }
}
