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

        /*Threshold: When looking into the data the potential often vary a little, without being an actual local maximum or mininmum. If the difference in potential is larger than the threshold
         * the vertex will be considered a local max/min, otherwise it will be considered as insignificant variance in potential*/
        public decimal Threshold { get; set; } = 0.001m;

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
            decimal localMaximum = 0;
            decimal localMinimum = 0;
            Measurement lastMeasurement = new Measurement(10m, 0m, 0m);//The potential will always be less than 10 V.

            //data needed for object
            DiffMeasurement tempDiffMeasurement = new DiffMeasurement();

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

                decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastMeasurement.Potential, measurement.Potential));
                if (IR_drop == true)//dette bruges efter local max er detected, således IR-drop springes over.
                {
                    tempDiffMeasurement.Et_initial = measurement.Potential;
                    tempDiffMeasurement.Lithium_initial = measurement.Lithium;//collect info about lithium
                    IR_drop = false;
                    Decimal value2 = Decimal.Subtract(lastMeasurement.Potential, measurement.Potential);//faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                    Debug.WriteLine("IR-drop " + value2);
                    lastMeasurement = measurement;

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
                        tempDiffMeasurement.Es_initial = 3.4042m;
                        tempDiffMeasurement.Et_initial = 4.4015m;
                        tempDiffMeasurement.Time_initial = 8.85m;
                        tempDiffMeasurement.Lithium_initial = 0;
                        continue;
                    }
                    //detect local minimum
                    if (direction == Direction.Down && firstMeasurement == false)
                    {
                        if (measurement.Potential < lastMeasurement.Potential)
                        {
                            localMinimum = measurement.Potential;
                            lastMeasurement = measurement;
                        }
                        if(measurement.Potential > lastMeasurement.Potential)//local minimum is detected (previous measurement was minimum)
                        {
                            //Collecting data
                            tempDiffMeasurement.Et_final = lastMeasurement.Potential;
                            tempDiffMeasurement.Lithium_final = lastMeasurement.Lithium;
                            tempDiffMeasurement.Time_final = lastMeasurement.Time;
                            //Debug.WriteLine("Difference " + differenceFromLastMeasurement);
                            minimum_counter++;
                            //Debug.WriteLine("Minimum detected " + minimum_counter);
                            //Preparing for search of local maximum
                            lastMeasurement = measurement;
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
                        if (measurement.Potential > lastMeasurement.Potential)//current is larger than last, keep searching.
                        {
                            //current is larger than last.
                            localMaximum = measurement.Potential;
                            //prep. for next iteration
                            lastMeasurement = measurement;
                            continue;
                        }
                        if (measurement.Potential < lastMeasurement.Potential)//Local maximum is detected (previous measurement was max)
                        {
                            localMaximum = lastMeasurement.Potential;
                            tempDiffMeasurement.Es_final = lastMeasurement.Potential;
                            //extract gitt data
                            diffMeasurements.Add(new DiffMeasurement(tempDiffMeasurement));
                            object_counter++;
                            Debug.WriteLine("Just created an object " + object_counter);
                            Decimal value = Decimal.Subtract(tempDiffMeasurement.Time_final, tempDiffMeasurement.Time_initial);
                            Debug.WriteLine("Potential at local maximum " + lastMeasurement.Potential);
                            Debug.WriteLine("Time at local max " + lastMeasurement.Time);
                            Debug.WriteLine("Lithium initial " + tempDiffMeasurement.Lithium_initial);
                            //Decimal value_test = Decimal.Subtract(2m, 1m);
                            Debug.WriteLine("Time difference " + value);
                            //Debug.WriteLine("test difference" + value_test);

                            //prep for next collection of Gitt data
                            tempDiffMeasurement.Es_initial = lastMeasurement.Potential;
                            //disse to skal ændres til næste loop.
                            IR_drop = true; //IR_drop sættes til true.
                            //et_initial = measurement.Potential;
                            tempDiffMeasurement.Time_initial = measurement.Time; //tid starter ved no-current, men pot springes over grundet IR-drop.
                            //prep for next iteration
                            lastMeasurement = measurement;
                            direction = Direction.Down;
                            continue;
                        }
                    }
                }
                if (differenceFromLastMeasurement < Threshold)
                {
                    lastMeasurement = measurement;
                }
            }
        }

        //should be run after analyseDischarge()
        private void analyseCharge()
        {
            Direction direction = Direction.Up;
            //trackers
            bool firstMeasurement = true;
            Measurement lastMeasurement = new Measurement(-10m, 0m, 0m); //The potential will always be greater than 10 V.
            decimal localMaximum = 0;
            decimal localMinimum = 0;

            //data needed for object
            DiffMeasurement tempDiffMeasurement = new DiffMeasurement();
            
            decimal object_counter = 0;
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
                    decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastMeasurement.Potential, measurement.Potential));
                    if(IR_drop == true)
                    {
                        tempDiffMeasurement.Et_initial = measurement.Potential;
                        tempDiffMeasurement.Lithium_initial = measurement.Lithium;
                        IR_drop = false;
                        Decimal value2 = Decimal.Subtract(lastMeasurement.Potential, measurement.Potential);//faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                        Debug.WriteLine("IR-drop " + value2);
                        lastMeasurement = measurement;
                        continue;
                    }

                    if (differenceFromLastMeasurement > Threshold)
                    {
                        //searching for first local maximum after global minimum, data is discarded
                        if(direction == Direction.Up && measurement.Potential > lastMeasurement.Potential && firstMaxReached == false)//if current is bigger than last, first max not reached.
                        {
                            lastMeasurement = measurement;
                            continue;
                        }
                        if(direction == Direction.Up && measurement.Potential < lastMeasurement.Potential && firstMaxReached == false)//first max is reached, and "skipped"
                        {
                            direction = Direction.Down;
                            firstMaxReached = true;
                            lastMeasurement = measurement;
                            Debug.WriteLine("Found local maximum ");
                            continue;
                        }
                        
                        //searching for local minimum after global minimum, data is collected
                        if (direction == Direction.Down && measurement.Potential < lastMeasurement.Potential && firstMaxReached == true)//if current potential is smaller than last, min not reached
                        {
                            lastMeasurement = measurement;
                            continue;
                        }
                        if (direction == Direction.Down && measurement.Potential > lastMeasurement.Potential && firstMaxReached == true)//if current potenial is greater than last, min reached.
                        {
                            if (firstMinReached == true)//if it is not first min.
                            {
                                tempDiffMeasurement.Es_final = lastMeasurement.Potential;
                                diffMeasurements.Add(new DiffMeasurement(tempDiffMeasurement));
                                object_counter++;
                                Debug.WriteLine("Just created an object 2 " + object_counter);
                                Decimal value = Decimal.Subtract(tempDiffMeasurement.Time_final, tempDiffMeasurement.Time_initial);
                                Debug.WriteLine("time differece " + value);
                                Debug.WriteLine("Lithium initial " + tempDiffMeasurement.Lithium_initial);
                            }
                            
                            //collect data
                            tempDiffMeasurement.Es_initial = lastMeasurement.Potential;
                            //et_initial skal først måles næste gang.
                            IR_drop = true;
                            tempDiffMeasurement.Et_initial = measurement.Potential;
                            tempDiffMeasurement.Time_initial = measurement.Time;
                            tempDiffMeasurement.Lithium_initial = measurement.Lithium;

                            //prepare next iteration
                            lastMeasurement = measurement;
                            direction = Direction.Up;
                            Debug.WriteLine("Found local minimum ");
                            firstMinReached = true;
                            continue;
                        }
                        if (direction == Direction.Up && measurement.Potential > lastMeasurement.Potential && firstMaxReached == true) //max not reached, continue.
                        {
                            lastMeasurement = measurement;
                            continue;
                        }
                        if (direction == Direction.Up && measurement.Potential < lastMeasurement.Potential && firstMaxReached == true) //max reached, collect data.
                        {
                            //collect data
                            tempDiffMeasurement.Et_final = lastMeasurement.Potential;
                            tempDiffMeasurement.Time_final = lastMeasurement.Time;
                            tempDiffMeasurement.Lithium_final = lastMeasurement.Lithium;

                            //prepare for next iteration
                            lastMeasurement = measurement;
                            direction = Direction.Down;
                        }

                    }
                    if (differenceFromLastMeasurement < Threshold)
                    {
                        lastMeasurement = measurement;
                    }
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
