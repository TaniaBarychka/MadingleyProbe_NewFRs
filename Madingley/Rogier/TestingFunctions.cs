using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Madingley;

namespace Rogier
{
    class TestingFunctions
    {
        
        public void TestArray(double[][][][][][] abund)
        {
            //
            for (int fg = 0; fg < abund.Length; fg++)
            {
                for (int size = 0; size < abund[fg].Length; size++)
                {
                    for (int mat = 0; mat < abund[fg][size].Length; mat++)
                    {
                        for (int y = 0; y< abund[fg][size][mat].Length; y++)
                        {
                            for (int x= 0; x < abund[fg][size][mat][y].Length; x++)
                            {
                                for (int data = 0; data < abund[fg][size][mat][y][x].Length; data++)
                                {



                                    if (abund[fg][size][mat][y][x][data] != 0)
                                    {
                                        Console.WriteLine("ALARM! ABUNDANCE CARRIER ARRAY NOT 0'd out properly!");
                                        Console.WriteLine("Offending FG = " + fg + " and size class: " + size);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}