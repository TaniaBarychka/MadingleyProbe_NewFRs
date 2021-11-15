using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Madingley;

namespace Rogier
{
    class SizeBinCalc
    {
        //TO CHANGE. THIS SHOULD BE READ IN FROM THE FUNCTIONAL GROUP DEFINITIONS! FINE FOR NOW...
        private double[][] bodyminmax; 

        //Constructor that loads in the BodySizeDimensionsdata
        public SizeBinCalc(FunctionalGroupDefinitions bmdefs, int nfgs)
        {
            
            bodyminmax = new double[nfgs][];

            for (int fg = 0; fg < nfgs; fg++ )
            {
                double min = bmdefs.GetBiologicalPropertyOneFunctionalGroup("minimum mass", fg);
                double max = bmdefs.GetBiologicalPropertyOneFunctionalGroup("maximum mass", fg); ;
                bodyminmax[fg] = new double[2] { min, max};
            }
            
        }

        public int[] IndexArray(int nfgs)
        {
            //Need to 'flatten' the min/max jagged array in order to find the maximum value and therefore work out number of bins are needed to encompass all possible body sizes.
            double[] findmax = bodyminmax.SelectMany(x => x).ToArray();
            double max = findmax.Max();
            double min = findmax.Min();
            int totalbins = (int)(Math.Ceiling(Math.Log(max, 2)) - Math.Ceiling(Math.Log(min, 2)));

            //Indexing array
            int[] startindex = new int[nfgs];

            for (int fg = 0; fg < bodyminmax.Length; fg++)
            {
                //This is the index of the 'real' broader 0.00001 to 15000000 array that the indexing within an fg should start on.
                //minus one because of 0 indexing.
                if(bodyminmax[fg][0] == 0.00001)
                {
                    startindex[fg] = 0;
                }
                else
                {
                    startindex[fg] = (int)(Math.Ceiling(Math.Log(bodyminmax[fg][0], 2)- Math.Log(0.00001, 2)) - 1);
                }
                
            }
           
            return startindex;
        }

        public int[] NumberBins(int nfgs)
        {
            //Now working out the size of size dimension for each FG.
            int[] numbins = new int[nfgs];

            for (int s = 0; s < numbins.Length; s++)
            {
                numbins[s] = (int)(Math.Ceiling(Math.Log(bodyminmax[s][1], 2) - Math.Log(0.00001, 2)) - Math.Floor(Math.Log(bodyminmax[s][0], 2) - Math.Log(0.00001, 2)));
                
            }
            
            return numbins;
        }
    }
}
