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

        public decimal Threshold { get; set; } = 0.001m;//0.001m 6633 OVERVEJ OM DER SKAL RODES MED THRESHHOLD. TÆL HVOR MANGE TOPPER DER BURDE VÆRE I FORHOLD TIL HVAD DEN FINDER.

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
            //3. Analyse charge
            analyseCharge();
            Debug.WriteLine("End of file");


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
            decimal lastPotential = 10;//The potential will always be less than 10 V.
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
            decimal minimum_counter = 0;
            bool globalMinimumReached = false;
            bool IR_drop = false;

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
                if (IR_drop == true)//dette bruges efter local max er detected, således IR-drop springes over.
                {
                    et_initial = measurement.Potential;
                    IR_drop = false;
                    Decimal value2 = Decimal.Subtract(lastPotential, measurement.Potential);//faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                    Debug.WriteLine("IR-drop " + value2);
                    lastPotential = measurement.Potential;
                    lastTime = measurement.Time;
                    continue;
                }
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
                        }
                        if(measurement.Potential > lastPotential)//local minimum is detected (previous measurement was minimum)
                        {
                            //Collecting data
                            et_final = lastPotential;
                            t_final = lastTime;
                            //Debug.WriteLine("Difference " + differenceFromLastMeasurement);
                            minimum_counter++;
                            //Debug.WriteLine("Minimum detected " + minimum_counter);
                            //Preparing for search of local maximum
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            direction = Direction.Up;
                            continue;
                        }
                        if (globalMinimumReached == true)
                        {
                            Debug.WriteLine("I'm at the bottom of the ocean");
                            break;
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
                            Decimal value = Decimal.Subtract(t_final, t_initial);
                            Debug.WriteLine("Potential at local maximum " + lastPotential);
                            Debug.WriteLine("Time at local max " + lastTime);
                            //Decimal value_test = Decimal.Subtract(2m, 1m);
                            Debug.WriteLine("Time difference " + value);
                            //Debug.WriteLine("test difference" + value_test);

                            //prep for next collection of Gitt data
                            es_initial = lastPotential;
                            //disse to skal ændres til næste loop.
                            IR_drop = true; //IR_drop sættes til true.
                            //et_initial = measurement.Potential;
                            t_initial = measurement.Time; //tid starter ved no-current, men pot springes over grundet IR-drop.
                            //prep for next iteration
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            direction = Direction.Down;
                            continue;
                        }
                    }
                }
                if (differenceFromLastMeasurement < Threshold)
                {
                    lastPotential = measurement.Potential;
                    lastTime = measurement.Time;
                }
            }
        }

        //should be run after analyseDischarge()
        private void analyseCharge()
        {
            Direction direction = Direction.Up;
            //trackers
            bool firstMeasurement = true;
            decimal lastPotential = -10;//The potential will always be greater than 10 V.
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
            decimal minimum_counter = 0;
            bool globalMinimumReached = false;
            bool firstMaxReached = false;
            bool firstMinReached = false;
            bool IR_drop = false;

            //TJEK OM TALLENE PASSER ELLER OM DER SKAL LAVES EKSTRA TJEK/JUSTERINGER. HVIS ALT ER GODT, LAV LOGIK FOR GLOBALMAXIMUMREACHED = TRUE

            List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //liste med enkelt-talt til udregninger  

            //from global minimum to end
            foreach (var measurement in Measurements)
            {
                if (measurement.Potential != GlobalMinimum && globalMinimumReached == false)
                {
                    continue;
                }
                if (measurement.Potential == GlobalMinimum)
                {
                    globalMinimumReached = true;
                    
                }
                if (globalMinimumReached == true)
                {
                    decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastPotential, measurement.Potential));
                    if(IR_drop == true)
                    {
                        et_initial = measurement.Potential;
                        IR_drop = false;
                        Decimal value2 = Decimal.Subtract(lastPotential, measurement.Potential);//faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                        Debug.WriteLine("IR-drop " + value2);
                        lastPotential = measurement.Potential;
                        lastTime = measurement.Time;
                        continue;
                    }

                    if (differenceFromLastMeasurement > Threshold)
                    {
                        //searching for first local maximum after global minimum, data is discarded
                        if(direction == Direction.Up && measurement.Potential > lastPotential && firstMaxReached == false)//if current is bigger than last, first max not reached.
                        {
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                        if(direction == Direction.Up && measurement.Potential < lastPotential && firstMaxReached == false)//first max is reached, and "skipped"
                        {
                            direction = Direction.Down;
                            firstMaxReached = true;
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            Debug.WriteLine("Found local maximum ");
                            continue;
                        }
                        
                        //searching for local minimum after global minimum, data is collected
                        if (direction == Direction.Down && measurement.Potential < lastPotential && firstMaxReached == true)//if current potential is smaller than last, min not reached
                        {
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                        if (direction == Direction.Down && measurement.Potential > lastPotential && firstMaxReached == true)//if current potenial is greater than last, min reached.
                        {
                            if (firstMinReached == true)//if it is not first min.
                            {
                                es_final = lastPotential;
                                diffMeasurements.Add(new DiffMeasurement(es_initial, es_final, et_initial, et_final, t_initial, t_final));
                                object_counter++;
                                Debug.WriteLine("Just created an object 2 " + object_counter);
                                Decimal value = Decimal.Subtract(t_final, t_initial);
                                Debug.WriteLine("time differece " + value);
                            }
                            
                            //collect data
                            es_initial = lastPotential;
                            //et_initial skal først måles næste gang.
                            IR_drop = true;
                            et_initial = measurement.Potential;
                            t_initial = measurement.Time;

                            //prepare next iteration
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            direction = Direction.Up;
                            Debug.WriteLine("Found local minimum ");
                            firstMinReached = true;
                            continue;
                        }
                        if (direction == Direction.Up && measurement.Potential > lastPotential && firstMaxReached == true) //max not reached, continue.
                        {
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            continue;
                        }
                        if (direction == Direction.Up && measurement.Potential < lastPotential && firstMaxReached == true) //max reached, collect data.
                        {
                            //collect data
                            et_final = lastPotential;
                            t_final = lastTime;

                            //prepare for next iteration
                            lastPotential = measurement.Potential;
                            lastTime = measurement.Time;
                            direction = Direction.Down;
                        }

                    }
                    if (differenceFromLastMeasurement < Threshold)
                    {
                        lastPotential = measurement.Potential;
                        lastTime = measurement.Time;
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
