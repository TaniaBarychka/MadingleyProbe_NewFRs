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
    public class Utilities
    {
        public void PrintAbundArray( double[][][][][][] abundarray, int fg)
        {
            for (int size = 0; size < abundarray[fg].Length; size++)
            {
                Console.WriteLine("SIZECLASS: {0}", size);
                for (int i = 0; i < abundarray[fg][size].Length; i++)
                {
                    for (int j = 0; j < abundarray[fg][size][i].Length; j++)
                    {
                        Console.Write(string.Format("{0} ", abundarray[fg][size][i][j][0][0]));
                    }
                    Console.Write(Environment.NewLine + Environment.NewLine);
                }
                Console.ReadLine();
            }

        }
        public void PrintFGCellOccupation(List<uint[]> _CellList, ModelGrid modgrid)
        {
            int[] nonempty = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int xyz = 0; xyz < 19; xyz++)
            {
                for (int asdf = 0; asdf < _CellList.Count; asdf++)
                {
                    GridCellCohortHandler holder = modgrid.GetGridCellCohorts(_CellList[asdf][0], _CellList[asdf][1]);
                    if (holder[xyz] != null)
                    {
                        for (int c = 0; c < holder[xyz].Count; c++)
                        {
                            if (holder[xyz][c].CohortAbundance != 0)
                            {
                                nonempty[xyz] += 1;
                                break;
                            }
                            
                        }
                    }
                }
            }
            Console.WriteLine("The FGs have the following representation: ");
            for (int j = 0; j < nonempty.Length; j++)
            {
                Console.Write(string.Format(" FG: {0} in cells: {1}, ", j, nonempty[j]));
            }
            Console.ReadLine();
            
        }

    


    public void PrintFGCellMap(double[][][][][][] abundmap, int maturity)
        {

            for (int guild= 0; guild < abundmap.Length; guild++)
            {
                Console.WriteLine("Doing Guild: {0}", guild);

                for (int size = 0; size < abundmap[guild].Length; size++)
                {
                    Console.WriteLine("Doing Size: {0}", size);

                    for (int lat = 0; lat < abundmap[guild][size].Length; lat++ )
                    {
                        for(int lon = 0; lon < abundmap[guild][size][lat].Length; lon ++)
                        {
                            int presence = 0;
                            if(abundmap[guild][size][lat][lon][maturity][0] != 0)
                            {
                                presence = 1;
                            }
                            Console.Write(string.Format("{0} ", presence));

                        }
                        Console.Write(Environment.NewLine + Environment.NewLine);
                    }

                }
            }
    
        }

    }

}
