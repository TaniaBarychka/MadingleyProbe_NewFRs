using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Madingley;
using Rogier;

namespace Madingley
{

    public class RogierDataCarriers
    {
        
        // This class will do everything... it will build the array and fill them then save the data to disc. 
        // YOU NEED TO HAVE A ZEROING FUNCTION AT THE END OF EACH DATA COLLECTING PERIOD. 
        
        // These are the necessary arrays
        public double[][][][][][][] dataArray;
        public int[] sizeindexArray;
        public double[][][][][][] tstartArray;
        public double[][][][][][] tendArray;
        public double[][][][][][] dispabundArray;
        public double[][][][][][] dispcarrierArray;
        
        
        // This class will build themm
        ArrayConstructor builder;

        // This class will record them
        CollectingData collector;

        // Allows you to do stuff like printing out matrices...
        Utilities utilitor;

        // Allows you to test arrays have been zeroed out properly
        TestingFunctions testor;

        // Allows you to save the big array as a sqlite file
        CreateDatabase databasor;

        // How to burn in for
        public int burnintime;

        // Time steps recorded
        public int recordedtime;

        // Global Time steps
        public int globaln;

        public RogierDataCarriers(int cellshigh,int  cellswide, int nfgs, FunctionalGroupDefinitions bmdefs, int burnt)
        {
            builder = new ArrayConstructor();
            collector = new CollectingData();
            utilitor = new Utilities();
            testor = new TestingFunctions();
            databasor = new CreateDatabase();
            SetUpArrays(cellshigh, cellswide, nfgs, bmdefs);
            burnintime = burnt;
            globaln = 0;
        }

        public void CreateDB()
        {
            // Get name stamp for db
            string time = string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);
            string databasefilename = String.Concat(@".\MadingleyRun" + time + ".db3");

            // Make the database
            databasor.DBFilename = databasefilename;
            databasor.CreateDBWithDataTable();

        }



        // Call this to instance the arrays with the right dimensions
        public void SetUpArrays(int cellshigh,int  cellswide, int nfgs, FunctionalGroupDefinitions bmdefs)
        {
            Console.WriteLine("CONSTRUCTING ARRAYS...");
            dataArray = builder.ConstructDataArray(cellshigh, cellswide, nfgs, bmdefs);
            sizeindexArray = builder.ConstructIndexArray(nfgs, bmdefs);
            tstartArray = builder.ConstructAbundanceCarrierArray(cellshigh, cellswide, nfgs, bmdefs);
            tendArray = builder.ConstructAbundanceCarrierArray(cellshigh, cellswide, nfgs, bmdefs);
            dispabundArray = builder.ConstructAbundanceCarrierArray(cellshigh, cellswide, nfgs, bmdefs);
            dispcarrierArray = builder.ConstructAbundanceCarrierArray(cellshigh, cellswide, nfgs, bmdefs);
            Console.WriteLine("FINISHED...");
        }

        public void CollectTstartData(List<uint[]> _CellList, ModelGrid modgrid, uint hh)
        {
            Console.WriteLine("Rogier: Collecting abundance before ecology for vital rate calculations");
            // loop through all the cells grid cells.
            for (int ii = 0; ii < _CellList.Count; ii++)
            {
                // Add the grid-cell data to the tstart grid.
                collector.CalculateCellAbundance(_CellList[ii][0], _CellList[ii][1], modgrid, ref tstartArray, sizeindexArray,hh,"tstart");
            }

        }

        public void CollectTendData(List<uint[]> _CellList, ModelGrid modgrid, uint hh)
        {
            Console.WriteLine("Rogier: Collecting abundance before merging but after ecology for vital rate calculations");
            // loop through all the cells grid cells.
            for (int ii = 0; ii < _CellList.Count; ii++)
            {
                // Add the grid-cell data to the tstart grid.
                collector.CalculateCellAbundance(_CellList[ii][0], _CellList[ii][1], modgrid, ref tendArray, sizeindexArray,hh,"tend");
            }

        }
        public void CollectDispAbundData(List<uint[]> _CellList, ModelGrid modgrid, uint hh)
        {
            Console.WriteLine("Rogier: Collecting abundance after merging for dispersal rate calculation");
            // loop through all the cells grid cells.
            for (int ii = 0; ii < _CellList.Count; ii++)
            {
                // Add the grid-cell data to dispabundarray... using the 'tstart' protocol as this means it collects all juves and adults and doesn't distinguish between new and old ones
                collector.CalculateCellAbundance(_CellList[ii][0], _CellList[ii][1], modgrid, ref dispabundArray, sizeindexArray, hh, "tstart");
            }

        }

        public void CalculateDispersalData(ModelGrid modgrid, uint timestep)
        {

            int monthstep = (int)timestep % 12;

            //Fill up the dispacarrierArray with all the data from the modgrid deltadispersal object
            collector.CalculateGlobalDispersalAbundance(modgrid, sizeindexArray, ref dispcarrierArray);

            // Now add to the data arry depending on whether the abundance array is empty or not:
            for (int guild = 0; guild < tendArray.Length; guild++)
            {

                for (int size = 0; size < tendArray[guild].Length; size++)
                {
                    for (int lat = 0; lat < tendArray[guild][size].Length; lat++)
                    {
                        for (int lon = 0; lon < tendArray[guild][size][lat].Length; lon++)
                        {
                            for (int mat = 0; mat < tendArray[guild][size][lat][lon].Length; mat++)
                            {
                                // Use the post merge abundances as the denominator
                                double totalabundance = dispabundArray[guild][size][lat][lon][mat][0];
                                
                                // If there was actually some odds to record.
                                if(totalabundance > 0)
                                {
                                    //Calculate per capita rates for diagonal and horizontal/vertical disperal
                                    double pcapitahor = dispcarrierArray[guild][size][lat][lon][mat][0] /totalabundance;
                                    double pcapitadiag = dispcarrierArray[guild][size][lat][lon][mat][1] / totalabundance;
                                    int monthind = (int)monthstep;
                                    //Now pop in to the appropriate points in the carrier array. 

                                    // Horizontal/vertical dispersal
                                    dataArray[guild][size][mat][lat][lon][monthstep][5] += pcapitahor;
                                    dataArray[guild][size][mat][lat][lon][monthstep][6] += Math.Pow(pcapitahor, 2);
                                    dataArray[guild][size][mat][lat][lon][monthstep][7] += 1;

                                    // Diagonal dispersal
                                    dataArray[guild][size][mat][lat][lon][monthstep][8] += pcapitadiag;
                                    dataArray[guild][size][mat][lat][lon][monthstep][9] += Math.Pow(pcapitadiag, 2);
                                    dataArray[guild][size][mat][lat][lon][monthstep][10] +=1 ;

                                }
                            }
                        }
                    }
                }
            }
        }

        public void CalculateVitalStatistics(List<uint[]> _CellList, uint currenttimestep)
        {
            int monthind = (int)currenttimestep % 12;

            for (int ii = 0; ii < _CellList.Count; ii++)
            {

                for (int guild = 0; guild < tstartArray.Length; guild++)
                {
                    
                

                    for (int size = 0; size < tstartArray[guild].Length; size++)
                    {
                            for (int mat = 0; mat < 2; mat++)
                            {            

                                // 3 PHASES:
                                // 1) Collect the abundance data at tend
                                // 2) Calculate and collect the death data
                                // 3) calculate and collect the transition data
                                double newstate = tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][mat][1];
                                double oldstate = tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][mat][0];

                            
                                //PHASE 1
                                // Calculate abundance data -- NB this is at the end of ecology. 
                                double totalabund = newstate + oldstate;
                                dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][0] += totalabund;
                                dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][1] += Math.Pow(totalabund, 2);

                                //PHASE 2
                                //Calculate death data.
                                //Individuals at tstart
                                double attstart = tstartArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][mat][0];
                            
                                //Individuals of that survived until tend but are not new... 
                                double attend = tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][mat][0];

                            
                                
                                // Individuals can also disappear if they mature... only applicable to juveniles (mat = 0)
                                double ascended = 0;
                                if (mat == 0)
                                {
                                    // NEWLY MATURED...
                                    ascended += tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][1][1];
                                }

                                //Number who died
                                double dead = attstart - attend - ascended;

                                

                                //Make it per capita
                                double pcapitadeath = dead / attstart;

                                //Put it in the dataArray if there was any one there at the start of the timestep
                                if(attstart > 0 )
                                {
                                dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][11] += pcapitadeath;
                                dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][12] += Math.Pow(pcapitadeath, 2);
                                dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][13] += 1;
                                }
                            
                                
                            
                            //PHASE 3
                            //Calculate transition data 
                            //The calculation will differ for different lifestages
                            double pcapitatrans = 0;
                            double possibletransitions = 0;
                            double nextstage = 0;

                            // Transition is considered transitioning away from current lifestage. 
                            // i.e. maturation to adult for juveniles and birth rate for adults
                            if (mat == 0)
                            {
                                // Maturation rate per juvenile
                                // How many NEW adults
                                nextstage = tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][1][1];
                                
                                // How many juveniles at the start of the time step
                                possibletransitions = tstartArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][0][0];
                                
                                // The rate... if 0 / 0 will give inf as answer but will not be stored due to log check below
                                pcapitatrans = nextstage/ possibletransitions;
                            }
                            
                            else
                            {
                                // Expected births per individual
                                // How many adults are there at the end of the lifestage?
                                

                                
                                possibletransitions = newstate+ oldstate + dead;
                                
                                // How many new juveniles are there at the end of ecology?
                                nextstage = tendArray[guild][size][_CellList[ii][0]][_CellList[ii][1]][0][1];
                                
                                // The rate.
                                pcapitatrans = nextstage / possibletransitions; 
                            }




                            //Put it in the dataArray if there was any available to give birth/mature. 
                            if (possibletransitions > 0.0)
                                {
                                    dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][2] += pcapitatrans;
                                    dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][3] += Math.Pow(pcapitatrans, 2);
                                    dataArray[guild][size][mat][_CellList[ii][0]][_CellList[ii][1]][monthind][4] += 1;
                                }



                            }
                        
                    }
                    
                
                
                }
            
            
        }
        }




        public void ZeroArrays()
        {
            //Needs to be done at the end of every timestep...
            //IS THIS SUBSTANTIALLY SLOWER THAN JUST GARBAGING THE ARRAY AND REINSTANCING?!?!?!

            collector.ZeroAbundArray(ref tendArray);
            collector.ZeroAbundArray(ref tstartArray);
            collector.ZeroAbundArray(ref dispcarrierArray);
            collector.ZeroAbundArray(ref dispabundArray);

        }

        public void PrintAbund(string typeofarray, int fg)
        {
            if(typeofarray == "tstart")
            {
                utilitor.PrintAbundArray(tstartArray,fg);
            }
            else
            {
                utilitor.PrintAbundArray(tstartArray, fg);
            }
            
        }
        public void PrintPresence(int maturity)
        {
            utilitor.PrintFGCellMap(tendArray, maturity);
        }
        public void TestZeroedAbund()
        {
            //Check all the carriers are empty
            testor.TestArray(dispcarrierArray);
            testor.TestArray(tendArray);
            testor.TestArray(tstartArray);
            testor.TestArray(dispabundArray);
            
        }

        public void AddDataToDatabase(int[] countdata, int resolution, int minx, int miny, int maxx, int maxy)
        {
            
            // Give the databsor the data
            databasor.AddJaggedToDB(dataArray, countdata);

            // Add the simulation metadata
            databasor.AddParameterTable(globaln, resolution, miny, maxy, minx, maxx);
            databasor.AddSimTime(recordedtime, globaln);
            //Done!
        }

        public int[] CountDataset()
        {
            int numberpoints = 0;
            int totalnumber = 0;
            int[] pointtotal = new int[2] { 0, 0 };

            for (int fg = 0; fg < dataArray.Length; fg++)
            {

                for (int size = 0; size < dataArray[fg].Length; size++)
                {

                    for (int mat = 0; mat < dataArray[fg][size].Length; mat++)
                    {
                        for (int lat= 0; lat < dataArray[fg][size][mat].Length; lat++)
                        {
                            for (int lon = 0; lon < dataArray[fg][size][mat][lat].Length; lon++)
                            {
                                for (int month = 0; month < dataArray[fg][size][mat][lat][lon].Length; month++)
                                {
                                    totalnumber++;
                                    if (dataArray[fg][size][mat][lat][lon][month][0] != 0)
                                    {
                                        numberpoints++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("The total number of points to be added is: " + numberpoints + " out of a total of " + totalnumber + " possible points");
            pointtotal[0] = numberpoints;
            pointtotal[1] = totalnumber;
            return pointtotal;
        }

    }
}




