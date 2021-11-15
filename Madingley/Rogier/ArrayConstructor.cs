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

    class ArrayConstructor
    {
        
        
        public double[][][][][][][] ConstructDataArray(int cellshigh, int cellswide, int nfgs, FunctionalGroupDefinitions bmdef)
        {

            //THE ARRAY CREATOR
            //Structure:
            // (Functional Group)           
            // (Size Class)               
            // (Maturity)                 Only 2 levels: mature or immature.
            // (y co-ord)                     
            // (x co-ord)                 
            // (month)                    set to 12: monthly resolution
            // (data)

            //Instance the body size calculator
            SizeBinCalc calc = new SizeBinCalc(bmdef,nfgs);
            int[] sizebins = calc.NumberBins(nfgs);


            //Instance the empty array
            double[][][][][][][] array = new double[nfgs][][][][][][];
            for (int guild = 0; guild < nfgs; guild++)
            {
                //This adds the correct size dimensions for the body size internal array for each guild.
                //Loosely this is Ceiling(log(maxbodysize,2) - log(minbodysize,2)), for details check inside the SizeBinCalc class above.

                array[guild] = new double[sizebins[guild]][][][][][];

                for (int size = 0; size < sizebins[guild]; size++)
                {
                    array[guild][size] = new double[2][][][][];

                    for (int maturity = 0; maturity < 2; maturity++)
                    {
                        array[guild][size][maturity] = new double[cellshigh][][][];

                        for (int height = 0; height< cellshigh; height++)
                        {
                            array[guild][size][maturity][height] = new double[cellswide][][];

                            for (int width = 0; width < cellswide; width++)
                            {
                                array[guild][size][maturity][height][width] = new double[12][];

                                for (int month = 0; month < 12; month++)
                                {
                                    // Guild for the Array:
                                    // All data come in pairs... sum(x) and sum(xsquared), in that order
                                    // 0,1: total abundance (extant + new) at the end of ecology but before dispersal.
                                    // 2,3,4 transition odds
                                    // 5,6,7 horizontal/vertical dispersal odds (all)
                                    // 8,9,10 diagonal disperal odds.
                                    // 11,12,13 death odds
                                    
                                    array[guild][size][maturity][height][width][month] = new double[14] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                }
                            }
                        }
                    }
                }
            }
            return array;
        }



        public double[][][][][][] ConstructAbundanceCarrierArray(int cellshigh, int cellswide, int nfgs, FunctionalGroupDefinitions bmdefs)
        {
            //Instance the body size calculator
            SizeBinCalc calc = new SizeBinCalc(bmdefs, nfgs);
            int[] sizebins = calc.NumberBins(nfgs);


            //Instance the empty array
            double[][][][][][] array = new double[nfgs][][][][][];
            for (int guild = 0; guild < nfgs; guild++)
            {
                //This adds the correct size dimensions for the body size internal array for each guild.
                //Loosely this is Ceiling(log(maxbodysize,2) - log(minbodysize,2)), for details check inside the SizeBinCalc class above.

                array[guild] = new double[sizebins[guild]][][][][];

                for (int size = 0; size < sizebins[guild]; size++)
                {
                    array[guild][size] = new double[2][][][];


                    array[guild][size] = new double[cellshigh][][][];

                    for (int height = 0; height < cellshigh; height++)
                    {
                        array[guild][size][height] = new double[cellswide][][];

                        for (int width = 0; width < cellswide; width ++)
                        {
                            array[guild][size][height][width] = new double[2][];
                            for (int mat = 0; mat < 2; mat++ )
                            {
                                
                                //All data come in pairs... 
                                // 0 abundance new for this lifestage OR moving horizontal (when dispersal carrier)
                                // 1 abundance already at this lifestage OR moving diagonal (when disperal carrier)

                                array[guild][size][height][width][mat] = new double[2] { 0, 0 };

                            }
                            
                        }
                    }

                }

            }

            return array;
        }



        //MAKE INDEX ARRAY
        public int[] ConstructIndexArray(int nfgs, FunctionalGroupDefinitions bmdefs)
        {

            //Instance the body size calculator... there is probably a better method of doing this than creating a whole new instance of the SizeBinCalc object but is only once so shouldn't be a problem.
            SizeBinCalc indexconstruct = new SizeBinCalc(bmdefs, nfgs);
            int[] sizeindex = indexconstruct.IndexArray(nfgs);

            return sizeindex;

        }
    }

}
