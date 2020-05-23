using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace GITT_Analysis
{
    public partial class Form1 : Form
    {
        private string loadedFile = null;
        //bool dataReached = false;
        //decimal tracker = 0;
        //decimal Potential_tracker = 0;
        //decimal Time_tracker = 0;
        //decimal difference = 0;

        //Variables used for measuring collection (6 needed variables per measurement)
        //decimal t_initial = 0;
        //decimal t_final = 0;
        //decimal E_t_initial = 0;
        //decimal E_t_final = 0;
        //decimal E_s_initial = 0;
        //decimal E_s_final = 0;

        //bool direction_up = true; //checks direction of data analysis
        //bool first_measurement = true; //Checks for first measurement

        Variables variables = new Variables();

        //Evt løb alt i gennem og tjek for højeste potential!

        decimal object_counter = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gittFileDialog.Filter = "GITT File (*.txt)|*.txt";
            if (gittFileDialog.ShowDialog() != DialogResult.OK)
                return;
            loadedFile = gittFileDialog.FileName;
            lbl_file_loaded.Text = $"File loaded: {Path.GetFileName(gittFileDialog.FileName)}";
            btn_analyze.Enabled = true;
        }
         
        private void btn_analyze_Click(object sender, EventArgs e)
        {

            MessageBox.Show("hello");
            List<Measurement> measurementst = AnalyticalData.fromFile(loadedFile);
            Analysis analysis = new Analysis(measurementst);
            analysis.Analyze();
            
            
         /*   
            List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //liste med enkelt-talt til udregninger
            List<List<Measurement>> measurements = new List<List<Measurement>>(); //Liste til potential graftegning
            List<Measurement> cycles = new List<Measurement>();
            string[] lines = File.ReadAllLines(loadedFile);
            foreach (var line in lines)
            {
                String[] parts = line.Replace(",",".").Split("\t");
                if (parts[0] == "mode")
                {
                    continue;
                }
                decimal potential = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //sets "potential" to current potential
                if (variables.direction_up == true)
                {
                    if (variables.tracker == 0 || variables.tracker < potential || variables.tracker == potential)//first meassurement or previous measurement (V) is smaller than current measurement, haven't reach max.
                    {
                        cycles.Add(new Measurement(potential, decimal.Parse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture)));
                        variables.tracker = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //set tracker equal to current measurement (V), keeping track of highest potential
                        variables.Potential_tracker = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //potential_tracker = highest potential
                        variables.Time_tracker = decimal.Parse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture); //time_tracker = time for highest potential
                        continue;
                    }
                    if (variables.tracker > decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture)) //if previous measurement (V) is larger than current measurement (might have reached local maxima)
                    {
                        variables.difference = Math.Abs(variables.tracker - potential); //difference between highest potential (tracker) and current potential (potential)
                        if (Decimal.Compare(variables.difference, 0.001m) == -1) //difference er smaller than 0.001 (caused by small variances in the measurements, change in direction should be ignored)  
                        {
                            variables.Potential_tracker = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //Maybe delete
                            variables.Time_tracker = decimal.Parse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture); //maybe delete
                            //maybe tracker updater here
                            continue;
                        }
                        //difference is greater than 0.001 (change in direction detected).
                        if (variables.first_measurement == true) // t_initial and E_s_initial are found.
                        {
                            variables.t_initial = variables.Time_tracker; //t_iitial = prevous time (from the highest potential)
                            variables.E_s_initial = variables.Potential_tracker; //E_s_initial = prevous potential (highest potential)
                            variables.E_t_initial = potential; //setting the current potentiel (which is the first after the max) to E_t_initial
                            variables.first_measurement = false;
                            variables.direction_up = false;
                            Debug.WriteLine("changing direction to down");
                            continue;
                        }
                        if (variables.first_measurement == false) //E_s_initial and E_t_initial for preveous data-collection are found,  t_initial and E_s_initial for current data-collection are found.
                        {
                            //currently: E_t_initial = prevous max, t_initial = previous max, E_s_final = Potential_tracker (from prevous "cycle")
                            diffMeasurements.Add(new DiffMeasurement(variables.E_s_initial, variables.Potential_tracker, variables.E_t_initial, variables.E_t_final, variables.t_initial, variables.t_final)); //assuming all data is found
                            object_counter++;
                            Debug.WriteLine("Just created an object "+ object_counter);
                            variables.t_initial = variables.Time_tracker; //for the "next" data-collection
                            variables.E_t_initial = variables.Potential_tracker; //for the "next" data-collection
                            variables.direction_up = false;
                            Debug.WriteLine("changing direction to down");
                            continue;
                            //THROW SOME KIND OF EXEPTION PLEASE
                        }
                        //diffMeasurements.Add(new DiffMeasurement(Potential_tracker, Potential_tracker, Potential_tracker, Potential_tracker, Time_tracker, Time_tracker));
                        //Debug.WriteLine("First measurement completed");
                    }
                }
                if (variables.direction_up == false)
                {
                    if (variables.tracker == 0 || variables.tracker > potential || variables.tracker == potential)//first meassurement/previous measurement (V) is greater than current measurement, haven't reach min.
                    {
                        cycles.Add(new Measurement(potential, decimal.Parse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture)));
                        variables.tracker = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //set tracker equal to current measurement (V), keeping track of previous potential
                        variables.Potential_tracker = decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture); //potential_tracker = current potential
                        variables.Time_tracker = decimal.Parse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture); //time_tracker = time for current potential
                        continue;
                    }
                    if (variables.tracker < decimal.Parse(parts[8], NumberStyles.Float, CultureInfo.InvariantCulture)) //if previous measurement (V) is smaller than current measurement (might have reached local minimum)
                    {
                        //no check for minor differences
                        //E_t_final and t_final is found.
                        variables.E_t_final = variables.Potential_tracker; //sets E_t_final to prevous potental (minimum)
                        variables.t_final = variables.Time_tracker; //sets t_final to prevous minimum
                        variables.direction_up = true;
                        Debug.WriteLine("changing direction to up");
                        continue;
                    }//TJEK OM DET ER DE RIGTIG TING DER GEMMES. e_T_INI ER VEL FØRSTE TIL EFTER MAX, SÅ DET KAN NEMT TILGÅS, LIGE INDEN BOOL DIRECTION ÆNDRES.
                }
                //if (true)
                //    measurements.Add(cycles);
                //Debug.WriteLine(line);
            }
        */
        }

        
    }
}
