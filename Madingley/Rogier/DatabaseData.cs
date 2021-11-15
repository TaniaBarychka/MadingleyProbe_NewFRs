using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Madingley;
using System.Data;
using System.Data.SQLite;
using Rogier;
using System.Diagnostics;

namespace Madingley
{
    class CreateDatabase
    {
        private string dbfilename;

        public string DBFilename
        {
            get
            {
                return dbfilename;
            }
            set
            {
                dbfilename = value;
            }
        }


        public void CreateDBWithDataTable()
        {
            string createTableQuery = "create table alldata (ID INTEGER PRIMARY KEY autoincrement, ycoord int, xcoord int, FG int, sizeclass int, maturity VARCHAR(10), month VARCHAR(20), abundance double, abundancesqr double, transition double, transitionsqr double, transitionn int, horizontaldispersal double, horizontaldispersalsqr double, horizontaldispersaln int, diagdispersal double, diagdispersalsqr double, diagdispersaln int, death double, deathsqr double, deathn int)";

            System.Data.SQLite.SQLiteConnection.CreateFile(dbfilename);        // Create the file which will be hosting our database

            string datalink = String.Concat("data source=" + dbfilename);


            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(datalink))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    com.CommandText = createTableQuery;     // Set CommandText to our query that will create the table
                    com.ExecuteNonQuery();                  // Execute the query

                    con.Close();        // Close the connection to the database
                }
            }
        }

        public void AddParameterTable(int timesteps, int res, int miny, int maxy, int minx, int maxx)
        {
            string createTableQuery = "create table scenariometadata (ID INTEGER PRIMARY KEY autoincrement, globaln int, resolution int, minlat int, minlon int, maxlat int, maxlon int)";

            string populatetable = "INSERT INTO scenariometadata (globaln, resolution, minlat, minlon, maxlat, maxlon) VALUES(@globaln, @resolution, @minlat, @minlon, @maxlat, @maxlon)";

            string datalink = String.Concat("data source=" + DBFilename);

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(datalink))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    com.CommandText = createTableQuery;     // Set CommandText to our query that will create the table

                    com.ExecuteNonQuery();                  // Execute the command

                    com.CommandText = populatetable;        //Set up command to fill in the necessary data

                    com.Parameters.AddWithValue("@globaln", (timesteps));  //Give the command the right parameters             
                    com.Parameters.AddWithValue("@resolution", res);
                    com.Parameters.AddWithValue("@minlat", miny);
                    com.Parameters.AddWithValue("@minlon", minx);
                    com.Parameters.AddWithValue("@maxlat", maxy);
                    com.Parameters.AddWithValue("@maxlon", maxx);

                    com.ExecuteNonQuery();                  // Execute the command

                    con.Close();        // Close the connection to the database
                }
            }
        }

        public void AddSimTime(int timestepsrecorded, int totaltimesteps)
        {

            string recordingtime = "UPDATE scenariometadata SET timestepsrecorded = @timestepsrecorded WHERE globaln = @globaln";
            string createcolumn = "ALTER TABLE scenariometadata ADD COLUMN timestepsrecorded int;";
            string datalink = String.Concat("data source=" + DBFilename);


            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(datalink))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database
                    // Open the connection to the database
                    com.CommandText = createcolumn;
                    com.ExecuteNonQuery();

                    com.CommandText = recordingtime;        //Set up command to fill in the necessary data

                    com.Parameters.AddWithValue("@timestepsrecorded", timestepsrecorded);  //Give the command the right parameters    
                    com.Parameters.AddWithValue("@globaln", totaltimesteps);

                    com.ExecuteNonQuery();                  // Execute the command

                    con.Close();        // Close the connection to the database     // Close the connection to the database

                }
            }

        }


        public void AddJaggedToDB(double[][][][][][][] data, int[] totaltocount)
        {
            //Instantiate this class to get month/maturity array/dictionary constructed
            AddDataRow add = new AddDataRow();
            add.DBFolder = DBFilename;
            Console.WriteLine("Adding data points... ");
            int datano = 0;


            //wrap in 'using' so that you can submit all as a transaction.

            string datalink = String.Concat("data source=" + add.DBFolder);

            //A wee timer
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(datalink))
            {
                con.Open();

                using (System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand(con))
                {

                    using (var transaction = con.BeginTransaction())
                    {

                        //Now loop through all the data and see if you need to add it to the transaction...
                        for (int fg = 0; fg < data.Length; fg++)
                        {
                            Console.WriteLine("Adding FG: " + fg);
                            int fgcount = 0;

                            for (int size = 0; size < data[fg].Length; size++)
                            {

                                for (int mat = 0; mat < data[fg][size].Length; mat++)
                                {
                                    for (int y = 0; y < data[fg][size][mat].Length; y++)
                                    {
                                        for (int x = 0; x < data[fg][size][mat][y].Length; x++)
                                        {
                                            for (int month = 0; month < data[fg][size][mat][y][x].Length; month++)
                                            {

                                                //The if statement below only adds datapoint to the database transaction if there is a non-0 value              there. Otherwise it moves on to the next one
                                                if (data[fg][size][mat][y][x][month][0] != 0)
                                                {
                                                    double abund = data[fg][size][mat][y][x][month][0];
                                                    double abundsqr = data[fg][size][mat][y][x][month][1];
                                                    double trans = data[fg][size][mat][y][x][month][2];
                                                    double transsqr = data[fg][size][mat][y][x][month][3];
                                                    double transn = data[fg][size][mat][y][x][month][4];
                                                    double hdisp = data[fg][size][mat][y][x][month][5];
                                                    double hdispsqr = data[fg][size][mat][y][x][month][6];
                                                    double hdispn = data[fg][size][mat][y][x][month][7];
                                                    double ddisp = data[fg][size][mat][y][x][month][8];
                                                    double ddispsqr = data[fg][size][mat][y][x][month][9];
                                                    double ddispn = data[fg][size][mat][y][x][month][10];
                                                    double death = data[fg][size][mat][y][x][month][11];
                                                    double deathsqr = data[fg][size][mat][y][x][month][12];
                                                    double deathn = data[fg][size][mat][y][x][month][13];


                                                    add.AddDataPoint(fg,size,mat,y,x,month,abund,abundsqr,trans,transsqr,transn,hdisp,hdispsqr,hdispn,ddisp,ddispsqr,ddispn,death,deathsqr,deathn, cmd);

                                                    datano++;
                                                    fgcount++;

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            Console.WriteLine("FG number: " + fg + " had a total of " + fgcount + " datapoints");
                        }
                        Console.WriteLine("Finished!");
                        Console.WriteLine(datano + " datapoints in total");
                        Console.WriteLine("Adding to database...");
                        transaction.Commit();


                        Console.WriteLine("Data adding took: " + stopwatch.Elapsed.TotalSeconds + " seconds.");



                    }
                }
            }
        }
    }

    

    class AddDataRow
    {
        private string[] months = new string[12];
        private string[] maturity = new string[2];


        public AddDataRow()
        {
            months[0] = "January";
            months[1] = "February";
            months[2] = "March";
            months[3] = "April";
            months[4] = "May";
            months[5] = "June";
            months[6] = "July";
            months[7] = "August";
            months[8] = "September";
            months[9] = "October";
            months[10] = "November";
            months[11] = "December";

            maturity[0] = "Immature";
            maturity[1] = "Mature";
        }

        private string dbfolder;
        public string DBFolder
        {
            get
            {
                return dbfolder;
            }
            set
            {
                dbfolder = value;
            }
        }

       
        public void AddDataPoint(int fg, int sizeind, int mat, int y, int x, int monthind, double abund, double abundsqr, double trans, double transsqr, double transn, double hdisp, double hdispsqr, double hdispn, double ddisp, double ddispsqr, double ddispn, double death, double deathsqr,double deathn, System.Data.SQLite.SQLiteCommand cmd)
        {
            //ID INTEGER PRIMARY KEY autoincrement, ycoord int, xcoord int, FG int, sizeclass int, maturity VARCHAR(10), month VARCHAR(20), abundance double, abunancesqr double, transition double, transitionsqr double, transitionn double, horizontaldispersal double, horizontaldispersalsqr double, horizontaldispersaln double, diagdispersal double, diagdispersalsqr int, diagdispersaln double, death double, deathsqr double, deathn double
            cmd.CommandText = "INSERT INTO alldata (xcoord,ycoord,FG, sizeclass, maturity, month,abundance, abundancesqr, transition, transitionsqr, transitionn, horizontaldispersal, horizontaldispersalsqr, horizontaldispersaln, diagdispersal, diagdispersalsqr, diagdispersaln, death, deathsqr, deathn) VALUES(@xcoord, @ycoord, @FG, @sizeclass, @maturity, @month, @abundance, @abundancesqr, @transition, @transitionsqr, @transitionn, @horizontaldispersal, @horizontaldispersalsqr, @horizontaldispersaln, @diagdispersal, @diagdispersalsqr, @diagdispersaln, @death, @deathsqr, @deathn)";

            
            cmd.Parameters.AddWithValue("@xcoord", x);
            cmd.Parameters.AddWithValue("@ycoord", y);
            cmd.Parameters.AddWithValue("@FG", fg);
            cmd.Parameters.AddWithValue("@sizeclass", sizeind);
            cmd.Parameters.AddWithValue("@maturity", maturity[mat]);
            cmd.Parameters.AddWithValue("@month", months[monthind]);
            cmd.Parameters.AddWithValue("@abundance", abund);
            cmd.Parameters.AddWithValue("@abundancesqr", abundsqr);
            cmd.Parameters.AddWithValue("@transition", trans);
            cmd.Parameters.AddWithValue("@transitionsqr", transsqr);
            cmd.Parameters.AddWithValue("@transitionn", (int)transn);
            cmd.Parameters.AddWithValue("@horizontaldispersal", hdisp);
            cmd.Parameters.AddWithValue("@horizontaldispersalsqr", hdispsqr);
            cmd.Parameters.AddWithValue("@horizontaldispersaln", (int)hdispn);
            cmd.Parameters.AddWithValue("@diagdispersal", ddisp);
            cmd.Parameters.AddWithValue("@diagdispersalsqr", ddispsqr);
            cmd.Parameters.AddWithValue("@diagdispersaln", (int)ddispn);
            cmd.Parameters.AddWithValue("@death", death);
            cmd.Parameters.AddWithValue("@deathsqr", deathsqr);
            cmd.Parameters.AddWithValue("@deathn", (int)deathn);
        
            cmd.ExecuteNonQuery();
        }

    }
}
