using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using static WindowsFormsApp1.Classes;
using static WindowsFormsApp1.Enums;
using static WindowsFormsApp1.Variables;
using System.Globalization;
using System.Windows.Forms;


namespace InstList_from_TS_Confirmations
{
    public class Methods
    {
        #region Website

        public static List<Ret> Website()
        {
            List<LinesJoined> fullLine = new List<LinesJoined>();
            List<Ret> instList = new List<Ret>();
            if (fileSource == FileSource.TSWebsite)
            {
                initialDirectory = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\2025 01 Jan\Downloads";
                title = "Select Confirmation From Website.csv";
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()

                {
                    InitialDirectory = initialDirectory,
                    Title = title
                };

                //	Show dialog
                var fileToOpen = openFileDialog.ShowDialog();
                string fileSelected = openFileDialog.FileName;

                //  Get actual file name without path

                //	Query to get lines containg '$'	
                var linesToKeep = File.ReadLines(fileSelected).Where(l => l.Contains("Principal") || l.Contains("Execution")).ToList();

                //	Trim '.csv' from string
                fileSelected = fileSelected.Replace("Confirmation From Website.csv", "");

				//  Trim file path - keep name
				//  delete characters up to start of file name '\\RYZEN-1\TradeManagerAnalysis\'
				try
				{
					fileSelectedName = fileSelected.Remove(0, 96);
					//MessageBox.Show($"{fileSelectedName}");
					fileSelectedName = fileSelectedName.Replace("Downloads", "Results"); //   2024 Jan\Downloads\2024 01 12

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
                //	Create new file name 'xxx Modified.csv'
                string filePathIDrive = fileSelected + " Modified.csv";

                //	Write query to new file
                //  WritAllLines - 'Creates a new file, writes one or more strings to the file, and then closes the file.' (string array)
                //  WriteAllText - 'File class method that is used to create a new file, writes the specified string to the file,
                //      and then closes the file. If the target file already exists, it is overwritten.' (string)

                #region Concat lines from TS report into fullLine
                //	Save strings starting with the Id (int) and add to next string
                string priorString = "";
                char stringChar;
                List<LinesJoined> fullReport = new List<LinesJoined>();
                List<LinesJoined> lastPass = new List<LinesJoined>();

                //  Confirmation format is:
                //      01/05/2024 ... Bought ... TSLA ... 238.95
                //          ... 10:02:44 ...
                //      01/05/2024 ... Sold ... TSLA ... 238.46
                //         ... 10:15:24 ...
                //  Use lineCount from 0 to end of list
                //  Increment lineCount after each line
                //  If modulus division is 0 save line
                //  If modulus division is 1 concat lines
                //  First pass creates full Bought/Sold line and second pass results in 'Bought...Sold...'
                //
                // 2024 04 27 
                //  Symbol description can have from 2 to 4 words in description
                //  '04/15/2024 04/17/2024 Bought NVIDIA CORP NVDA 1 ',
                //  '04/11/2024 04/15/2024 Sold TESLA MTRS INC TSLA 1', 
                //  '04/22/2024 04/24/2024 Sold ADVANCED MICRO DEVICES INC AMD 6 '
                //
                //  Solution:
                //      Since qty is substring following symbol, search for first sub returning True for
                //      subs[count].All(char.IsDigit) - using System.Ling;
                //      First sub[] that is 'IsDigit' will Qty.
                //      Back up one sub[] for symbol
                int lineCount = 0;
                foreach (var l in linesToKeep)
                {
                    if (lineCount % 2 == 0)
                    {
                        //	Save line for later concatenation
                        priorString = l;
                    }
                    else if (lineCount % 2 == 1)
                    {
                        fullLine.Add(new LinesJoined { LinePlusLine = priorString + " " + l });
                    }
                    lineCount++;
                }
                foreach (var l in fullLine)
                {
                    fullReport.Add(l);
                }
                fullReport = fullReport.ToList();

                //  Create list using subs from fullReport to be used in sorting
                List<Confirmation> splitFullReport = new List<Confirmation>();
                string[] subs;
                int iD = 0;
                int count = 0;
                string symbol = ""; 
                string timeOnly;
                long timeInTicks;
                DateTime dt;
                foreach (var f in fullReport)
                {
                    //  NVDA and TSLA have different number of words per line
                    //  NVDA had 20 and TSLA has 21

                    //  count to 0
                    count = 0;

                    //  Split fullReport string into words
                    subs = f.LinePlusLine.Split(' ');

                    //  Step through subs until first digit is found - value is qty of shares
                    while (!subs[count].All(char.IsDigit))
                    { 
                        count++; 
                    }

                    //  subs[count] is Qty
                    //  subs[count - 1] is Symbol
                    //  subs[count + 9] is datetime (dt)
                    //  subs[count + 1] is Price
                    //count--;
                    //symbol = subs[count];

                    //  Use count and count++ for Symbol and Qty to created new Confirmation

                    dt = DateTime.Parse(subs[0] + " " + subs[count + 9]);
                    //  Convert to MST from EST
                    dt = dt.AddMinutes(-120);
                    timeInTicks = dt.Ticks;
                    //  Get time only for entry into list
                    timeOnly = dt.ToString("HH:mm:ss");

                    splitFullReport.Add(new Confirmation
                    {
                        ID = iD,
                        Long_Short = subs[2],
                        Symbol = subs[count - 1],
                        TradeDate = subs[0],
                        TradeTime = timeOnly,
                        TimeTicks = timeInTicks,
                        DTTradeTime = dt,
                        //Price = Math.Round(Convert.ToDouble(subs[count + 1]), 2),
						Price = Convert.ToDouble(subs[count + 1]),
						Qty = Int32.Parse(subs[count])
                    });

                    //if (subs[6] == "TSLA")
                    //{ 
                    //    dt = DateTime.Parse(subs[0] + " " + subs[16]);
                    //    //  Convert to MST from EST
                    //    dt = dt.AddMinutes(-120);
                    //    timeInTicks = dt.Ticks;
                    //    //  Get time only for entry into list
                    //    timeOnly = dt.ToString("HH:mm:ss");

                    //    splitFullReport.Add(new Confirmation
                    //    {
                    //        ID = iD,
                    //        Long_Short = subs[2],
                    //        Symbol = subs[6],
                    //        TradeDate = subs[0],
                    //        TradeTime = timeOnly,
                    //        TimeTicks = timeInTicks,
                    //        DTTradeTime = dt,
                    //        Price = Math.Round(Convert.ToDouble(subs[8]), 2),
                    //        Qty = Int32.Parse(subs[7])
                    //    });
                    //}

                    //if (subs[5] == "NVDA")
                    //{
                    //    dt = DateTime.Parse(subs[0] + " " + subs[15]);
                    //    //  Convert to MST from EST
                    //    dt = dt.AddMinutes(-120);
                    //    timeInTicks = dt.Ticks;
                    //    //  Get time only for entry into list
                    //    timeOnly = dt.ToString("HH:mm:ss");

                    //    splitFullReport.Add(new Confirmation
                    //    {
                    //        ID = iD,
                    //        Long_Short = subs[2],
                    //        Symbol = subs[5],
                    //        TradeDate = subs[0],
                    //        TradeTime = timeOnly,
                    //        TimeTicks = timeInTicks,
                    //        DTTradeTime = dt,
                    //        Price = Math.Round(Convert.ToDouble(subs[7]), 2),
                    //        Qty = Int32.Parse(subs[6])
                    //    });
                    //}


                    iD++;
                }
                splitFullReport.ToList();
                //  Create list sorted by day then time
                var orderedFullReport = splitFullReport.OrderBy(l => l.DTTradeTime.Day).ThenBy(l => l.DTTradeTime.TimeOfDay).ToList();

                //  Create List<Ret> instList here from orderedFullReport
                foreach (Confirmation c in orderedFullReport)
                {
                    instList.Add(
                    new Ret
                    {
                        Account = 1,
                        Name = c.Symbol,
                        Quantity = c.Qty,
                        Price = c.Price,
                        Time = c.TimeTicks,
                        HumanTime = c.DTTradeTime.ToString("HH:mm:ss MM/dd/yyyy"),
                        Long_Short = c.Long_Short,

                    });

                }
                instList.ToList();
            }
            #endregion Concat lines from TS report 
            return instList;
        }
#endregion Website

        #region TS App
        public static List<Ret> TSApp()
        {

            #region Create FileDialog, display, read file into query, and create new class that will be useable by instList

            List<Ret> instList = new List<Ret>();

            List<LinesJoined> fullLine = new List<LinesJoined>();
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {

                InitialDirectory = @"C:\Users\Rod\Cloud-Drive\TradeManagerAnalysis",
                Title = "Browse Trade Station .csv Files"
            };

            //	Show dialog
            var fileToOpen = openFileDialog.ShowDialog();

            string fileSelected = openFileDialog.FileName;

            //  Get actual file name without path
            //	Query to get lines containg '$'	
            var linesToKeep = File.ReadLines(fileSelected).Where(l => l.Contains("$")).ToList();

            //	Trim '.csv' from string
            fileSelected = fileSelected.Replace(".csv", "");

            //  Trim file path - keep name
            //  delete characters up to start of file name '\\RYZEN-1\TradeManagerAnalysis\'
            fileSelectedName = fileSelected.Remove(0, 46);

            //  \\RYZEN-1\TradeManagerAnalysis\2023 12 05
            //	Create new file name 'xxx Modified.csv'
            string filePathIDrive = fileSelected + " Modified.csv";

            //	Write query to new file
            //  WritAllLines - 'Creates a new file, writes one or more strings to the file, and then closes the file.' (string array)
            //  WriteAllText - 'File class method that is used to create a new file, writes the specified string to the file,
            //      and then closes the file. If the target file already exists, it is overwritten.' (string)
            File.WriteAllLines(filePathIDrive, linesToKeep);
            #endregion Create FileDialog, display, read file into query, and create new class that will be useable by instList

            #region Concat lines from TS report into fullLine
            //	Save strings starting with the Id (int) and add to next string
            string priorString = "";
            char stringChar;

            //	Keep only lines starting with a digit and concat with next line
            foreach (var l in linesToKeep)
            {
                stringChar = l[0];
                if (Char.IsDigit(l[0]))
                {
                    //	Save line for later concatenation
                    priorString = l;
                }
                else
                {
                    fullLine.Add(new LinesJoined
                    {
                        LinePlusLine = priorString + l
                    });
                }
            }
            fullLine = fullLine.ToList();
            #endregion Concat lines from TS report 

            #region Create instList 
            //  Create instList to allow use of extensions
            //  fullLine format is both entry and exit so split into two lines
            DateTime dtEntry;
            DateTime dtExit;

            long entryTimeInTicks;
            long exitTimeInTicks;

            //  Use Bought and Sold for transaction type to make instList work with existing code
            string entryTradeType = "";
            string exitTradeType = "";
            //	Split lines into subs

            foreach (LinesJoined sub in fullLine)
            {
                string[] subs;  //  = s.Split(' ', '.');
                string s = sub.LinePlusLine;
                subs = s.Split(',');
                subs.ToList();
                var x = subs[7];
                dtEntry = DateTime.Parse(subs[2]);
                entryTimeInTicks = dtEntry.Ticks;
                dtExit = DateTime.Parse(subs[15]);
                exitTimeInTicks = dtExit.Ticks;

                //  Change Buy/Sell to Bought/Sold
                if (subs[1] == "Buy")
                {
                    entryTradeType = "Bought";
                }
                else if (subs[1] == "Sell")
                {
                    entryTradeType = "Sold";
                }
                else if (subs[1] == "Sell Short")
                {
                    entryTradeType = "Sold";
                }
                if (subs[14] == "Sell")
                {
                    exitTradeType = "Sold";
                }
                else if (subs[14] == "Buy")
                {
                    exitTradeType = "Bought";
                }
                else if (subs[14] == "Buy to Cover")
                {
                    exitTradeType = "Bought";
                }
                //  Add lines to instList
                instList.Add(
                    new Ret
                    {
                        Account = 1,
                        Name = subs[4],
                        Quantity = (long?)Convert.ToInt64(subs[7]),

                        Price = (double?)Decimal.Parse(subs[5],
                            NumberStyles.AllowCurrencySymbol |
                            NumberStyles.AllowDecimalPoint |
                            NumberStyles.AllowThousands,
                            new CultureInfo("en-US")),
                        Time = entryTimeInTicks,
                        HumanTime = dtEntry.ToString("HH:mm:ss MM/dd/yyyy"),
                        Long_Short = entryTradeType
                    }

                    );
                instList.Add(
                    new Ret
                    {
                        Account = 1,
                        Name = subs[17],
                        Quantity = (long?)Convert.ToInt64(subs[7]),

                        Price = (double?)Decimal.Parse(subs[18],
                            NumberStyles.AllowCurrencySymbol |
                            NumberStyles.AllowDecimalPoint |
                            NumberStyles.AllowThousands,
                            new CultureInfo("en-US")),
                        Time = exitTimeInTicks,
                        HumanTime = dtExit.ToString("HH:mm:ss MM/dd/yyyy"),
                        Long_Short = exitTradeType
                    });

            }
            #endregion Create instList 
            return instList;
        }
        #endregion TS App

        #region NTExport
        public static List<Ret> NTExport()
        {
            List<Ret> instList = new List<Ret>();

            initialDirectory = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\NinjaTrader Exports\2024 07 Jul\Exports";
            title = "Select Export from NinjaTrader";
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()

            {
                InitialDirectory = initialDirectory,
                Title = title
            };

            //	Show dialog
            var fileToOpen = openFileDialog.ShowDialog();
            string fileSelected = openFileDialog.FileName;

            //  Get actual file name without path
            //	Query to get lines
            //	First line is title line
            int fileIndex = 0;
            //  Select all lines
            var linesToKeep = File.ReadLines(fileSelected).Where(l => l == l).ToList();

            //	Remove 'NinjaTrader Grid' and '.csv'
            fileSelected = fileSelected.Replace("NinjaTrader Grid ", "");
            fileSelected = fileSelected.Replace(".csv", "");

            //  Trim file path - keep name
            //  delete characters up to start of file name '\\RYZEN-1\TradeManagerAnalysis\'
            fileSelectedName = fileSelected.Remove(0, 96);

            //  Result is '"2024-02-16 01-55 PM"'
            //  Remove after '2024-02-16'
            fileSelectedName = fileSelectedName.Remove(10);

            //  Remove '-' from date -> 2024 02 13
            fileSelectedName = fileSelectedName.Replace("-", " ");

            //  Create instList from list 'linesToKeep'
            lineCount = 0;
            //  Create instList to allow use of extensions
            //  fullLine format is both entry and exit so split into two lines
            DateTime dtEntry;
            DateTime dtExit;

            long entryTimeInTicks;
            long exitTimeInTicks;

            //  Use Bought and Sold for transaction type to make instList work with existing code
            string entryTradeType = "";
            string exitTradeType = "";
            //	Split lines into subs

            foreach (var line in linesToKeep)
            {
                //  Skip first line.
                if (lineCount == 0)
                {
                    lineCount++;
                    continue;
                }
                string[] subs;  //  = s.Split(' ', '.');
                string s = line;
                subs = s.Split(',');
                subs.ToList();
                var x = subs[7];
                dtEntry = DateTime.Parse(subs[8]);
                entryTimeInTicks = dtEntry.Ticks;
                dtExit = DateTime.Parse(subs[9]);
                exitTimeInTicks = dtExit.Ticks;

                //  Change Long / Short to Bought / Sold
                if (subs[4] == "Long")
                {
                    entryTradeType = "Bought";
                    exitTradeType = "Sold";
                }
                else if (subs[4] == "Short")
                {
                    entryTradeType = "Sold";
                    exitTradeType = "Bought";
                }

                //  Skip line if Name (subs[1]) not equal to "NQ MAR24"
                //  Bonds price format is 109'250 which causes problems with conversion to double
                //  This needs to changed to symbol desired ie. TSLA, NQ MAR24
                if (subs[1] == "TSLA")
                {
                    //  Need to make two lines for each line in NT report
                    instList.Add(
                    new Ret
                    {
                        Account = 1,
                        Name = subs[1],
                        Quantity = (long?)Convert.ToInt64(subs[5]),
                        Price = (double?)Decimal.Parse(subs[6]),
                        Time = entryTimeInTicks,
                        //HumanTime = c.DTTradeTime.ToString("HH:mm:ss MM/dd/yyyy"),
                        HumanTime = dtEntry.ToString("HH:mm:ss MM/dd/yyyy"),
                        Long_Short = entryTradeType,
                    });
                instList.Add(
                    new Ret
                    {
                        Account = 1,
                        Name = subs[1],
                        Quantity = (long?)Convert.ToInt64(subs[5]),
                        Price = (double?)Decimal.Parse(subs[7]),
                        Time = entryTimeInTicks,
                        //HumanTime = c.DTTradeTime.ToString("HH:mm:ss MM/dd/yyyy"),
                        HumanTime = dtExit.ToString("HH:mm:ss MM/dd/yyyy"),
                        Long_Short = exitTradeType,
                    });
                }
            }
            return instList;
        }
    }
    #endregion NTExport

}
