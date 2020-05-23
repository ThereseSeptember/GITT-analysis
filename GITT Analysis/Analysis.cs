using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GITT_Analysis
{
    class Analysis
    {
        public Direction Direction { get; set; } = Direction.Down;

        public decimal GlobalMaximum { get; set; }

        public decimal Threshold { get; set; } = 0.05m;//0.001m 6633 OVERVEJ OM DER SKAL RODES MED THRESHHOLD. TÆL HVOR MANGE TOPPER DER BURDE VÆRE I FORHOLD TIL HVAD DEN FINDER.

        public decimal HighestLocalMinimaAfterReverseSecondHalf { get; set; }

        public List<Measurement> Measurements { get; set; }

        public Analysis(List<Measurement>  measurements)
        {
            Measurements = measurements;
        }

        public decimal GlobalMinimum { get; set; }

        public void Analyze()
        {
            //1. Find global minimum
            findGlobalMinimum();
            //2. Analyse data untill global minimum
            analyseDischarge();


            //Tidligere
            // 2. find global maximum
            //findGlobalMaximum();
            // 3. find lokal maximum
            //findLocalMaximum();
            //count local minima from second half
            //countLocalPeaksAfterReverse();
        }

        //Finds Global minimum
        private void findGlobalMinimum()
        {
            decimal minGlobalValue = 10; //It will always be less then 10 V.
            foreach (var measurement in Measurements)
            {
                if (measurement.Potential < minGlobalValue)
                {
                    minGlobalValue = measurement.Potential;
                }
            }
            GlobalMinimum = minGlobalValue;
            Debug.WriteLine("Global minimum " + GlobalMinimum);
        }

        private void analyseDischarge()
        {
            Direction direction = DetectDirection();
            //trackers
            bool firstMeasurement = true;
            decimal lastPotential = 10;//The potential will always be less then 10 V.
            decimal lastTime = 0;
            decimal localMaximum = 0;
            decimal localMinimum = 0;

            //data needed for object
            decimal t_initial = 0;
            decimal t_final = 0;
            decimal es_initial = 0;
            decimal es_final = 0;
            decimal et_initial = 0;
            decimal et_final = 0;

            decimal line_counter = 0;
            decimal object_counter = 0;
            bool globalMinimumReached = false;

            //TJEK OM TALLENE PASSER ELLER OM DER SKAL LAVES EKSTRA TJEK/JUSTERINGER. HVIS ALT ER GODT, LAV LOGIK FOR GLOBALMAXIMUMREACHED = TRUE

            List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //liste med enkelt-talt til udregninger

            //from start to globalminimum
            foreach (var measurement in Measurements)
            {
                line_counter++;
                if (measurement.Potential == GlobalMinimum)
                {
                    globalMinimumReached = true;
                }

                decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastPotential, measurement.Potential));

                if (differenceFromLastMeasurement > Threshold)
                {
                    if (direction == Direction.Down && firstMeasurement == true)
                    {
                        //make some logic that works for the first measurement.
                        firstMeasurement = false;
                        direction = Direction.Down;
                        //Dummy numbers
                        es_initial = 3.4042m;
                        et_initial = 4.4015m;
                        t_initial = 8.85m;
                        continue;
                    }
                    //detect local minimum
                    if (direction == Direction.Down && firstMeasurement == false)
                    {
                        if (measurement.Potential < lastPotential)
                        {
                            localMinimum = measurement.Potential;
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                        if(measurement.Potential > lastPotential)//local minimum is detected (previous measurement was minimum)
                        {
                            //Collecting data
                            et_final = lastPotential;
                            t_final = lastTime;
                            //Preparing for search of local maximum
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            direction = Direction.Up;
                            continue;
                        }
                    }
                    //detect local maximum
                    if (direction == Direction.Up && firstMeasurement == false)
                    {
                        if (measurement.Potential > lastPotential)//current is larger than last, keep searching.
                        {
                            //current is larger than last.
                            localMaximum = measurement.Potential;
                            //prep. for next iteration
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                        if (measurement.Potential < lastPotential)//Local maximum is detected (previous measurement was max)
                        {
                            localMaximum = lastPotential;
                            es_final = lastPotential;
                            //extract gitt data
                            diffMeasurements.Add(new DiffMeasurement(es_initial, es_final, et_initial, et_final, t_initial, t_final));
                            object_counter++;
                            Debug.WriteLine("Just created an object " + object_counter);

                            //prep for next collection of Gitt data
                            es_initial = lastPotential;
                            et_initial = measurement.Potential;
                            t_initial = measurement.Potential;
                            //prep for next iteration
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                    }
                }
            }
        }


        //oprindelig, starter med "up", men opsamler forkert data.
        private void findLocalMaximum()
        {
            Direction direction = DetectDirection();
            //trackers
            bool firstMeasurement = true;
            decimal lastPotential = 0;
            decimal lastTime = 0;
            decimal localMaximum = 0;
            decimal localMinimum = 0;

            //data needed for object
            decimal t_initial = 0;
            decimal t_final = 0;
            decimal es_initial = 0;
            decimal es_final = 0;
            decimal et_initial = 0;
            decimal et_final = 0;

            decimal object_counter = 0;
            bool globalMaximumReached = false;

            //TJEK OM TALLENE PASSER ELLER OM DER SKAL LAVES EKSTRA TJEK/JUSTERINGER. HVIS ALT ER GODT, LAV LOGIK FOR GLOBALMAXIMUMREACHED = TRUE

            List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //liste med enkelt-talt til udregninger

            //from start to globalmaximum
            foreach (var measurement in Measurements)
            {
                if (measurement.Potential == GlobalMaximum)
                {
                    globalMaximumReached = true;
                }
                decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastPotential, measurement.Potential));

                if (differenceFromLastMeasurement > Threshold)
                {
                    if (direction == Direction.Up && measurement.Potential < lastPotential)  // oh shit we're going down
                    {
                        localMaximum = lastPotential;
                        direction = Direction.Down;
                        if(firstMeasurement == true)
                        {
                            //Saving data
                            es_initial = lastPotential;
                            et_initial = measurement.Potential; //current potential
                            t_initial = measurement.Time; //current time

                            firstMeasurement = false;
                            continue;
                        }
                        if(firstMeasurement == false)
                        {
                            es_final = lastPotential;
                            //Make object from the good stuf!!!!!!!!!!!!!!
                            diffMeasurements.Add(new DiffMeasurement(es_initial, es_final, et_initial, et_final, t_initial, t_final));
                            object_counter++;
                            Debug.WriteLine("Just created an object " + object_counter);
                            Debug.WriteLine("time initial " + t_initial);
                            //Saving vars for next object
                            es_initial = lastPotential;
                            et_initial = measurement.Potential; //current potential
                            t_initial = measurement.Time; //current time

                            if (globalMaximumReached == true)//The Globalmaximum is reached and the last object has been made.
                            {
                                Measurements.Reverse(); //reverses list of measurements
                                globalMaximumReached = false;
                                break;
                            }

                            continue;
                        }
                    }
                    else if (direction == Direction.Down && measurement.Potential > lastPotential)
                    {
                        localMinimum = lastPotential; //tror ikke jeg bruger localmin/localmax var
                        direction = Direction.Up;

                        et_final = lastPotential;
                        t_final = lastTime;
                    }
                }

               

                lastPotential = measurement.Potential;
                lastTime = measurement.Time;
            }
        }

        //tæller minima, ved ned. Antaget at findLocalMax reverser alle data til slut.
        private void countLocalPeaksAfterReverse() //counting local minima of second half, starting backwords. HMM, måske ikke nødvendig. Tage det bagfra, skip det første, når maxGlobal nås tages højde for det.
        {
            decimal minima_counter = 0;
            decimal lastPotential = 100; //Potential is ALWAYS less than 100.
            //Count minima up till Global max. When the last minima is reach, do some logic by identifing the IR-drop (longest distance between measurements if not counting those next to global maxima.)
            foreach(var measurement in Measurements)
            {
                if (measurement.Potential == GlobalMaximum) //hvis GlobalMaximum nås, træd ud af løkke.
                {
                    break;
                }
                if (measurement.Potential < lastPotential)
                {
                    lastPotential = measurement.Potential;
                    continue;
                }
                if (measurement.Potential > lastPotential)
                {
                    minima_counter++;
                    Debug.WriteLine("Local minima " + minima_counter);
                }
            }
        }

        private void findGlobalMaximum()
        {
            decimal maxGlobalValue = 0;
            foreach(var measurement in Measurements)
            {
                if (measurement.Potential > maxGlobalValue)
                {
                    maxGlobalValue = measurement.Potential;
                }
            }
            GlobalMaximum = maxGlobalValue;
        }

        /// <summary>
        /// Detects the direction the graph is going.
        /// </summary>
        private Direction DetectDirection()
        {
            //todo: actually analyze the graph to find the correct direction
            return Direction.Down;
        }
    }
}
