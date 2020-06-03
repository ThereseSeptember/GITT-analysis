using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GITT_Analysis
{
    class Analysis
    {
        //I don't think I'm using this.
        public Direction Direction { get; set; } = Direction.Down;

        public decimal GlobalMaximum { get; set; }

        /*Threshold: When looking into the data the potential often vary a little, without being an actual local maximum or mininmum. If the difference in potential is larger than the threshold
         * the vertex will be considered a local max/min, otherwise it will be considered as insignificant variance in potential*/
        public decimal Threshold { get; set; } = 0.001m;

        public decimal HighestLocalMinimaAfterReverseSecondHalf { get; set; }

        /*Creating a list of measurements. Each measurement will contain all the data needed for calculating the diffusion coefficient. This includes all initial and final values of
         steady-state potential, E_s, the change in potential during constant current, E_t, time and the amount of lithium in the cathode material, Li_xV_2O_5.*/
        public List<Measurement> Measurements { get; set; }

        public List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>();

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
            //calculate diff.coeff.
            calculateDiffusionCoefficient();
        }

        /// <summary>
        /// Finds global minimum, which is used to determine when the "analyseCharge" method should start collecting data.
        /// </summary>
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

        /// <summary>
        /// This is a method used to analyse the discharge of the Li_xV_2_O_5 battery. It will first search for the global minimum, which is an indicator of the discharge being completed.
        /// The method will detect local minima and maxima, extracting data from these. A maxima will be detected when the potential drops (exceeding the threshold), which means the actual maximum
        /// will be the previous measurement (datapoint). To account for this, the previous measurement will always be saved in an object (variable-collection) called "lastMeasurement", which is
        /// used to acces the data from the maximum. It is a similar process when detecting local minima. From manually analyzing the data, it was found that the first measurement after a local maximum
        /// is a part of the IR-drop, which is accounted for using a boolean to indicate the IR-drop is taking place.
        /// </summary>
        private void analyseDischarge()
        {
            Direction direction = DetectDirection();
            //trackers
            bool firstMeasurement = true; //this boolean is needed to check for the first measurement, as this is plotted in manually.
            decimal localMinimum = 0;
            Measurement lastMeasurement = new Measurement(10m, 0m, 0m);//lastMeasurement will contain potential, time and lithium. The initial potential will always be less than 10 V.

            //data needed for object. tempDiffMeasurement will collect the data which will be used to make a "final" collection when all values are obtained.
            DiffMeasurement tempDiffMeasurement = new DiffMeasurement();

            //Debugging counters (I think)
            decimal line_counter = 0;
            decimal object_counter = 0;
            decimal minimum_counter = 0;
            //bool used to determine if the global minimum is reached
            bool globalMinimumReached = false;
            //bool used to detect if the current measurement is within an IR-drop.
            bool IR_drop = false;

            //List with data which will be used to calculate the diff. coefficient.
            //List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>();//maybe return this?AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa

            //from start to globalminimum
            foreach (var measurement in Measurements)
            {
                line_counter++;
                if (measurement.Potential == GlobalMinimum)
                {
                    globalMinimumReached = true;
                }

                decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastMeasurement.Potential, measurement.Potential));//used to compare to threshold.
                if (IR_drop == true)//This will be accessed after local max is detected. If IR_drop = true, the previous measurement was in the IR-drop, but not this one. Data can be collected.
                {
                    tempDiffMeasurement.Et_initial = measurement.Potential;//Collecting E_t initial.
                    tempDiffMeasurement.Lithium_initial = measurement.Lithium;//Collecting lithium initial.
                    IR_drop = false; //setting the IR_drop to false, such that "regular" datacollecting can continue.
                    Decimal value2 = Decimal.Subtract(lastMeasurement.Potential, measurement.Potential);//Debugging, faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                    Debug.WriteLine("IR-drop " + value2);//debugging
                    lastMeasurement = measurement;//opdating lastMeasurement before continuing to the next measurement.

                    continue;
                }
                if (differenceFromLastMeasurement > Threshold)
                {
                    if (direction == Direction.Down && firstMeasurement == true)//This will only be accesed upon the very first measurement. This is used to set the first data manually as no logic was made for detecting those.
                    {
                        //Potentially: make some logic that works for the first measurement.
                        firstMeasurement = false;
                        direction = Direction.Down; //setting the direction to down, but it already is?
                        //The first measurement/local max is set manually, as the potential varies a lot within the first few seconds making it difficult to make logic, as it is not a local max. Maybe threshold can help.
                        //Dummy numbers SHOULD BE CORRECTED!!!!!
                        tempDiffMeasurement.Es_initial = 3.4042m;
                        tempDiffMeasurement.Et_initial = 4.4015m;
                        tempDiffMeasurement.Time_initial = 8.85m;
                        tempDiffMeasurement.Lithium_initial = 0;
                        continue;
                    }
                    //Detecting local minimum. Direction is down and it is no longer the first measurement.
                    if (direction == Direction.Down && firstMeasurement == false)
                    {
                        if (measurement.Potential < lastMeasurement.Potential)//Local minimum not found - the current measurement has lower potential than last measurement.
                        {
                            localMinimum = measurement.Potential;//setting localminimum to current potential, as it is, currently, the lowest potential detected.
                            lastMeasurement = measurement;//updating lastMeasurement before analysing the next measurement.            
                        }
                        if(measurement.Potential > lastMeasurement.Potential)//Local minimum is found - previous potential is smaller than current -> the previous measurement was minimum.
                        {
                            //Collecting data from the local minimum
                            tempDiffMeasurement.Et_final = lastMeasurement.Potential;
                            tempDiffMeasurement.Lithium_final = lastMeasurement.Lithium;
                            tempDiffMeasurement.Time_final = lastMeasurement.Time;
                            //Debug.WriteLine("Difference " + differenceFromLastMeasurement);
                            //minimum_counter used for debugging.
                            minimum_counter++;
                            //Debug.WriteLine("Minimum detected " + minimum_counter);
                            //Preparing for search of local maximum
                            lastMeasurement = measurement;//updating lastMeasurement before analysing the next measurement.
                            direction = Direction.Up;//changing direction to "up", which means it is now searching for a local maximum.
                            continue;                            
                        }
                        if (globalMinimumReached == true)
                        {
                            Debug.WriteLine("I'm at the bottom of the ocean");
                            break;
                        }
                    }
                    //Detect local maximum, this is done when local minimum is reached and direction is changed.
                    if (direction == Direction.Up && firstMeasurement == false)
                    {
                        if (measurement.Potential > lastMeasurement.Potential)//Local max is not reached, current potential is bigger than the previous measurement.
                        {
                            //updating lastMeasurement before analysing the next measurement.
                            lastMeasurement = measurement;
                            continue;
                        }
                        if (measurement.Potential < lastMeasurement.Potential)//Local maximum is detected - previous measurement was max
                        {
                            //Collect data
                            tempDiffMeasurement.Es_final = lastMeasurement.Potential;
                            //Extract GITT data
                            diffMeasurements.Add(new DiffMeasurement(tempDiffMeasurement));
                            //Debugging purposes
                            object_counter++;
                            Debug.WriteLine("Just created an object " + object_counter);
                            Decimal value = Decimal.Subtract(tempDiffMeasurement.Time_final, tempDiffMeasurement.Time_initial);
                            Debug.WriteLine("Potential at local maximum " + lastMeasurement.Potential);
                            Debug.WriteLine("Time at local max " + lastMeasurement.Time);
                            Debug.WriteLine("Lithium initial " + tempDiffMeasurement.Lithium_initial);
                            //Decimal value_test = Decimal.Subtract(2m, 1m);
                            Debug.WriteLine("Time difference " + value);
                            //Debug.WriteLine("test difference" + value_test);
                            //Debugging purposes end!!

                            //Preperation for next collection of GITT data
                            tempDiffMeasurement.Es_initial = lastMeasurement.Potential;
                            //We are currently in an IR-drop (this is known from manual analysis)
                            IR_drop = true; //IR_drop sættes til true.
                            tempDiffMeasurement.Time_initial = measurement.Time; //Time for no-current is starting now, potential is collected at next iteration due to IR-drop.
                            //prep for next iteration
                            lastMeasurement = measurement;
                            //It will now search for a local minimum.
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
            Measurement lastMeasurement = new Measurement(-10m, 0m, 0m); //Keeping track of previous measurement. The potential will always be greater than -10 V.
           
            //data needed for object
            DiffMeasurement tempDiffMeasurement = new DiffMeasurement();
            
            //debugging
            decimal object_counter = 0;

            //Booleans used to keep track of data
            bool globalMinimumReached = false;
            bool firstMaxReached = false;
            bool firstMinReached = false;
            bool IR_drop = false;

            //List<DiffMeasurement> diffMeasurements = new List<DiffMeasurement>(); //List with variables for calculating diff. coefficient. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa

            //This will be analysing data from the global minimum till the end.
            foreach (var measurement in Measurements)
            {
                if (measurement.Potential != GlobalMinimum && globalMinimumReached == false)//The global minimum has not been reached - continue to next measurement.
                {
                    continue;
                }
                if (measurement.Potential == GlobalMinimum)//The global minimum has been reached - let the fun begin!
                {
                    globalMinimumReached = true;
                    
                }
                if (globalMinimumReached == true)//If the global minimum has been reached, the data-collection can start!
                {
                    decimal differenceFromLastMeasurement = Math.Abs(decimal.Subtract(lastMeasurement.Potential, measurement.Potential));//Used when comparing to the threshold.
                    if(IR_drop == true)//Inside IR-drop.
                    {
                        //collecting data
                        tempDiffMeasurement.Et_initial = measurement.Potential;
                        tempDiffMeasurement.Lithium_initial = measurement.Lithium;
                        IR_drop = false;
                        //debugging purposes
                        Decimal value2 = Decimal.Subtract(lastMeasurement.Potential, measurement.Potential);//faktisk ikke det reelle IR-drop, men fra punkt to til tre.
                        Debug.WriteLine("IR-drop " + value2);
                        //debugging end
                        lastMeasurement = measurement;//updating lastMeasurement before analysing the next measurement.
                        continue;
                    }

                    if (differenceFromLastMeasurement > Threshold)
                    {
                        //Searching for first local maximum after global minimum, data is discarded due to irregular measurement conditions.
                        if(direction == Direction.Up && measurement.Potential > lastMeasurement.Potential && firstMaxReached == false)//If current is bigger than last, first max not reached.
                        {
                            lastMeasurement = measurement;//updating lastMeasurement before analysing the next measurement.
                            continue;
                        }
                        if(direction == Direction.Up && measurement.Potential < lastMeasurement.Potential && firstMaxReached == false)//first max is reached, and "skipped"
                        {
                            direction = Direction.Down;
                            firstMaxReached = true;
                            lastMeasurement = measurement;
                            Debug.WriteLine("Found local maximum SKIP ");
                            continue;
                        }
                        
                        //Searching for local minimum after global minimum, data is collected
                        if (direction == Direction.Down && measurement.Potential < lastMeasurement.Potential && firstMaxReached == true)//if current potential is smaller than last, min not reached
                        {
                            lastMeasurement = measurement;
                            continue;
                        }
                        if (direction == Direction.Down && measurement.Potential > lastMeasurement.Potential && firstMaxReached == true)//if current potenial is greater than last, min reached - the previous measurement was the minimum.
                        {
                            if (firstMinReached == true)//If it is not first min, data can be extracted.
                            {
                                tempDiffMeasurement.Es_final = lastMeasurement.Potential;
                                //Extracting data used to calculate the diff. coef.
                                diffMeasurements.Add(new DiffMeasurement(tempDiffMeasurement));
                                //Debugging purposes
                                object_counter++;
                                Debug.WriteLine("Just created an object 2 " + object_counter);
                                Decimal value = Decimal.Subtract(tempDiffMeasurement.Time_final, tempDiffMeasurement.Time_initial);
                                Debug.WriteLine("time differece " + value);
                                Debug.WriteLine("Lithium initial " + tempDiffMeasurement.Lithium_initial);
                                //Debugging puposes end
                            }
                            
                            //collect data (This is done for ALL minimums also the first.)
                            tempDiffMeasurement.Es_initial = lastMeasurement.Potential;
                            IR_drop = true;
                            tempDiffMeasurement.Time_initial = measurement.Time;//Time for no-current is starting now, potential is collected at next iteration due to IR-drop.
                            
                            //prepare next iteration
                            lastMeasurement = measurement;
                            direction = Direction.Up;
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


        private void calculateDiffusionCoefficient()
        {
            Debug.WriteLine("Let's calculate stuff");
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

        //I don't think I'm using this
        private Direction DetectDirection()
        {
            //todo: actually analyze the graph to find the correct direction
            return Direction.Down;
        }
    }
}
