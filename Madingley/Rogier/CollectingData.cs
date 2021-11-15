using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Madingley;

namespace Madingley
{
    class CollectingData
    {

        // These function perform all of the various data collection excercises and calculations 
        
        public void CalculateCellAbundance(uint lat, uint lon,  ModelGrid modgrid, ref double[][][][][][] abundarray, int[] sizeindexarray, uint thistimestep, string t)
        {
            // Get a temporary local copy of the cohorts in the grid cell
            GridCellCohortHandler TempCohorts = modgrid.GetGridCellCohorts(lat, lon);

            // Loop over functional groups in the grid cell cohorts
            for (int j = 0; j < TempCohorts.Count; j++)
            {
                // Loop over cohorts within this functional group
                if (TempCohorts[j] != null)
                {
                    for (int c = 0; c < TempCohorts[j].Count; c++)
                    {
                        // Grab the cohort
                        Cohort cohort = TempCohorts[j][c];

                        // Get the data and indexing information you need:
                        double mass = cohort.AdultMass;
                        double abund = cohort.CohortAbundance;
                        int sizeclassindex = (int)((Math.Ceiling(Math.Log(mass, 2) - Math.Log(0.00001, 2)) - 1) - sizeindexarray[j]);
                        
                        // Tend data needs to be split into newstate or oldstate so need to modify stateindex
                        int juveindex;
                        int stateindex;
                        if (t == "tend")
                        {
                            stateindex = 0;

                            if (cohort.MaturityTimeStep == 4294967295)
                            {
                                juveindex = 0;
                            }
                            else
                            {
                                juveindex = 1;
                            }

                            if (juveindex == 0)
                            {
                                if (cohort.BirthTimeStep == thistimestep)
                                {

                                    stateindex = 1;
                                }
                            }

                            else
                            {
                                if (cohort.MaturityTimeStep == thistimestep)
                                {

                                    stateindex = 1;
                                }
                            }
                        }

                        // If not tend data then just need to select right juveindex and leave state as 0.
                        else
                        {
                            if (cohort.MaturityTimeStep == 4294967295)
                            {
                                juveindex = 0;
                            }
                            else
                            {
                                juveindex = 1;
                            }
                            stateindex = 0;
                        }
                        

                        //Now add it to the abundance array... tstart will always go into 0th index as all stuff must be 'old'.
                        //tend array the 0th entry is old stuff and the 1st entry is all the newly born/mature stuff.
                        abundarray[j][sizeclassindex][lat][lon][juveindex][stateindex] += abund;
                        

                    }
                }
            }
        }

        public void CalculateGlobalDispersalAbundance(ModelGrid modgrid, int [] sizeindexarray, ref double [][][][][][] disparray)
        {
            //0. N 
            //1. NE
            //2. E
            //3. SE
            //4. S
            //5. SW
            //6 W
            //7, NW

            uint[] diagonals = { 1, 3, 5, 7 };

             for (uint ii = 0; ii < modgrid.DeltaFunctionalGroupDispersalArray.GetLength(0); ii++)
            {
                for (uint jj = 0; jj < modgrid.DeltaFunctionalGroupDispersalArray.GetLength(1); jj++)
                {
                    //If the grid cell is not a valid one in the model then ignore it. 
                    if (modgrid.DeltaFunctionalGroupDispersalArray[ii, jj] != null)
                    {
                        // No cohorts are dispersing if there are non in deltafunctional array so skip on to the next one.
                        if (modgrid.DeltaFunctionalGroupDispersalArray[ii, jj].Count == 0)
                        {
                        }
                        else
                        {
                            for (int kk = 0; kk < modgrid.DeltaFunctionalGroupDispersalArray[ii, jj].Count; kk++)
                            {
                                
                                //Grab the cohort in question
                                int fgindex = (int)modgrid.DeltaFunctionalGroupDispersalArray[ii, jj][kk];
                                int cohortindex = (int)modgrid.DeltaCohortNumberDispersalArray[ii, jj][kk];
                                Cohort c = modgrid.GetGridCellCohorts(ii, jj)[fgindex][cohortindex];
                                
                                //Gather the data you need
                                double bodymass = c.AdultMass;
                                int sizeindex = (int)((Math.Ceiling(Math.Log(bodymass, 2) - Math.Log(0.00001, 2)) - 1) - sizeindexarray[fgindex]);

                                int maturity = 0;
                                if(c.MaturityTimeStep != 4294967295)
                                {
                                    maturity = 1;
                                }
                                double abundance = c.CohortAbundance;
                                
                                //0. N, 1. NE, 2. E, 3. SE, 4. S, 5. SW, 6 W, 7, NW
                                uint direction = modgrid.DeltaCellExitDirection[ii, jj][kk];
                                bool isDiagonal = diagonals.Contains(direction);
                                
                                //NOW Enter it into the array according to whether it is straight or diagonal dispersal.
                                if(isDiagonal)
                                {
                                   disparray[fgindex][sizeindex][ii][jj][maturity][1] += abundance;
                                }
                                else
                                {
                                    disparray[fgindex][sizeindex][ii][jj][maturity][0] += abundance;
                                }
                                
                            }
                        }
                    }

                }
            }
        }

        public void ZeroAbundArray (ref double[][][][][][] carrierarray)
        {
            for (int a = 0; a < carrierarray.Length; a++)
            {
                for (int b = 0; b < carrierarray[a].Length; b++)
                {
                    for (int c = 0; c < carrierarray[a][b].Length; c++)
                    {
                        for (int d = 0; d < carrierarray[a][b][c].Length; d++)
                        {
                            for (int e = 0; e < carrierarray[a][b][c][d].Length; e++)
                            {
                                for (int f = 0; f < carrierarray[a][b][c][d][e].Length; f++ )

                                    carrierarray[a][b][c][d][e][f] = 0;
                                
                            }
                        }
                    }
                }
            }
        }

    }
}


    

