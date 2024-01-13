﻿/*
 * Summary of code in different sections
 * 
 * 1 - 103      Read in data from '2024 01 08 Confirmation From Website.csv' (linesToKeep)
 * 106 - 121    Get file name from filepath - 2024 01 02
 * 122 - 167    Create list (fullReport) - each line is a string and a "Bought" or "Sold" from .csv file
 *                  Time is parsed into DateTime format so that 120 minutes can be added because of EST format
 *                  Time.Ticks is used to create ticks number for 'splitFullReport'
 * 168 - 200    Split the strings in 'fullReport' so that they can be entered into List<Confirmation> splitFullReport
 * 201 - 202    Use Linq to sort 'fullReport' by Day and then timed into List<Confirmation> 'splitFullReport'
 * 203 - 224    Create variables that will be filled from the individual lines of 'orderedFullReport' List<Confirmation>
 * 220          Order by time
 * 225 - 277    Create List<NTDrawline> 'listFromTwoLines' by selecting needed data from Bought/Sold lines
 * 284 - 307    Change "Bought/Sold" to "Long/Short" which is used by 'RecordAndDisplayTradesWithButtons'
 * 310 - 339    Create List<NTDrawLine> nTDrawLine from 'listFromTwoLines'  Fill in empty columns 
 * 342 - 393    Create List<NTDrawLineForLINQtoCSV>'columnsWithAttributes' from nTDrawLine and save to file'
 *                  "@"C:\Users\Owner\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\" + fileSelectedName + " Confirmation Results" + ".csv"
 * 
 * 2024 01 10
 *  Everything works in creating csvNTDrawLine.csv except printing to file
 */


using InstList_from_TS_Confirmations.Properties;
using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static WindowsFormsApp1.Classes;
using static WindowsFormsApp1.Enums;


namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //List<NTDrawLine> nTDrawLines = new List<NTDrawLine>();
            #region Start form
            //  This method enables visual styles for the application.
            //  Visual styles are the colors, fonts, and other visual elements that form an operating system theme.
            Application.EnableVisualStyles();

            //  Sets the application-wide default for the UseCompatibleTextRendering property defined on certain controls.
            Application.SetCompatibleTextRenderingDefault(false);

            //  need to create form before calling it for return values
            Form1 form = new Form1();

            //  Begins running a standard application message loop on the current thread, without a form.
            Application.Run(form);
            //bool fileSource = form.FileSource;
            bool maleBtn = form.maleBtn;

            //  Get results of file source radio buttons
            FileSource fileSource = new FileSource();
            fileSource = form.FileOrigin;
            //if (fileSource == FileSource.TSApp)
            //{
            //    MessageBox.Show("App is source");

            //}
            //else if (fileSource == FileSource.TSWebsite )
            //    {
            //    MessageBox.Show("Website is source");

            //}
            bool tSApp = form.tSSource;
            //  File dialog to get file from either ...App Data\NinjaTrader\NinjaTrader Data\Trade Station Confirmations
            //  
            //maleB
            #endregion Start form

            #region Create FileDialog, display, read file into query, and create new class that will be useable by instList

            //  2023 DST Sun, Mar 12, 2023 2:00 AM - Sun, Nov 5, 2023 2:00
            //DateTime startDST22 = new DateTime(2022, 03, 13);
            //DateTime endDST22 = new DateTime(2022, 11, 06);
            //DateTime startDST23 = new DateTime(2023, 03, 12);
            //DateTime endDST23 = new DateTime(2023, 11, 05);
            //DateTime startDST24 = new DateTime(2024, 03, 10);
            //DateTime endDST24 = new DateTime(2024, 11, 03);

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()

            {
                //InitialDirectory = @"C:\Users\Rod\Cloud - Drive\TradeManagerAnalysis",
                //Title = "Browse Trade Station .csv Files"
                InitialDirectory = @"C:\Users\Owner\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website",
                Title = "Browse Trade Station .csv Files"

                //C:\Users\Owner\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website
            };

            //	Show dialog
            var fileToOpen = openFileDialog.ShowDialog();
            //string fileSelected = openFileDialog.FileName.Dump();
            string fileSelected = openFileDialog.FileName;

            //  Get actual file name without path
            string fileSelectedName;

            //	Query to get lines containg '$'	
            var linesToKeep = File.ReadLines(fileSelected).Where(l => l.Contains("Principal") || l.Contains("Execution")).ToList();

            //	Trim '.csv' from string
            fileSelected = fileSelected.Replace("Confirmation From Website.csv", "");

            //  Trim file path - keep name
            //  delete characters up to start of file name '\\RYZEN-1\TradeManagerAnalysis\'
            fileSelectedName = fileSelected.Remove(0, 76);

            //  \\RYZEN-1\TradeManagerAnalysis\2023 12 05
            //	Create new file name 'xxx Modified.csv'
            string filePathIDrive = fileSelected + " Modified.csv";

            //	Write query to new file
            //  WritAllLines - 'Creates a new file, writes one or more strings to the file, and then closes the file.' (string array)
            //  WriteAllText - 'File class method that is used to create a new file, writes the specified string to the file,
            //      and then closes the file. If the target file already exists, it is overwritten.' (string)
            //File.WriteAllLines(filePathIDrive, linesToKeep);
            #endregion Create FileDialog, display, read file into query, and create new class that will be useable by instList

            #region Concat lines from TS report into fullLine
            //	Save strings starting with the Id (int) and add to next string
            string priorString = "";
            char stringChar;
            List<LinesJoined> fullLine = new List<LinesJoined>();
            List<LinesJoined> fullReport = new List<LinesJoined>();
            List<LinesJoined> lastPass = new List<LinesJoined>();
            int lineCount = 0;

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
            foreach (var l in linesToKeep)
            {
                //string myString = "dummy";
                //stringChar = l[0];
                //if (Char.IsDigit(l[0]))
                if (lineCount % 2 == 0)
                    {
                        //	Save line for 
                        priorString = l;
                    //Console.WriteLine(@$" First char is digit {l[0]}");
                }
                else if (lineCount % 2 == 1 )
                {
                    //Console.WriteLine(@$" Second char is char {l[0]}");
                    fullLine.Add(new LinesJoined { LinePlusLine = priorString + " " + l });
                }
                lineCount++;
            }
            foreach ( var l in  fullLine)
            {
                fullReport.Add(l);
            }
            fullReport = fullReport.ToList();

            //  Create list using subs from fullReport to be used in sorting
            List<Confirmation> splitFullReport = new List<Confirmation>();
            string[] subs;
            int iD = 0;
            string timeOnly;
            long priceInTicks;
            foreach (var f in fullReport)
            {

                subs = f.LinePlusLine.Split(' ');
                var dt = DateTime.Parse(subs[0] + " " + subs[16]);
                //  Convert to MST from EST
                dt = dt.AddMinutes(-120);
                priceInTicks = dt.Ticks;
                //  Get time only for entry into list
                timeOnly = dt.ToString("HH:mm:ss");

                splitFullReport.Add(new Confirmation
                {
                    ID = iD,
                    Long_Short = subs[2],
                    Symbol = subs[6],
                    TradeDate = subs[0],
                    TradeTime = timeOnly,
                    TimeTicks = priceInTicks,
                    DTTradeTime = dt,
                    Price = Math.Round(Convert.ToDouble(subs[8]), 2),
                    Qty = Int32.Parse(subs[7])
                });
                iD++;
                //list[0].TradeTime;
                //splitFullReport.Dump();
            }
                splitFullReport.ToList();
            //var sortedByTime = list.OrderBy(l => l.TradeTime);
            var orderedFullReport = splitFullReport.OrderBy(l => l.DTTradeTime.Day).ThenBy(l => l.DTTradeTime.TimeOfDay).ToList();

            /******************************************************************
             * 
             *  Create List<Ret> instList here from orderedFullReport
             *  Need to include code for creating Position column
             * 
             * 
            /******************************************************************/
            List<Ret> instList = new List<Ret>();
            //foreach (Confirmation c in splitFullReport)
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

                }
                );
            }
            instList.ToList();
            #endregion Concat lines from TS report 

            #region Fill Position column
            //	Foreach through 'byTime' and fill in Position
            //	Need to know where trading starts
            lineCount = 0;
            int currentPosition = 0;
            foreach (var bT in instList)
            {
                //	Check for first pass
                if (lineCount == 0)
                {
                    //	First trade is buy 
                    //	Set position and IsEntry
                    if (bT.Long_Short == "Bought")
                    {
                        bT.Position = (long)bT.Quantity;
                        //	Need to know position on next row
                        currentPosition = (Int32)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }
                    //	First trade is sell 
                    //	Set position and IsEntry

                    else if (bT.Long_Short == "Sold")
                    {
                        bT.Position = -(long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }
                    lineCount++;
                }

                //	Not first entry
                else if (bT.Long_Short == "Bought")
                {
                    //	Addon or new position
                    if (currentPosition == 0)
                    {
                        bT.Position = (long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }
                    //	Addon
                    else if (currentPosition > 0)
                    {
                        //	Add to position size
                        bT.Position = currentPosition + (long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }
                    else if (currentPosition < 0)
                    {
                        //	Add to position size
                        bT.Position = currentPosition + (long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsExit = true;
                        bT.IsEntry = false;
                    }
                    lineCount++;
                }
                else if (bT.Long_Short == "Sold")
                {
                    if (currentPosition == 0)
                    {
                        bT.Position = -(long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }
                    //	Addon
                    else if (currentPosition > 0)
                    {
                        //	Add to position size
                        bT.Position = currentPosition - (long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsExit = true;
                        bT.IsEntry = false;
                    }
                    else if (currentPosition < 0)
                    {
                        //	Add to position size
                        bT.Position = currentPosition - (long)bT.Quantity;
                        currentPosition = (int)bT.Position;
                        bT.IsEntry = true;
                        bT.IsExit = false;
                    }

                    lineCount++;
                }
            }
            instList.Reverse();
            #endregion Fill Position column

            #region Write and read instList.json
            // 	Write List to .json file
            string fileName = @"C:\Data\InstList.json";
            //  set Serialize options
            var options = new JsonSerializerOptions { WriteIndented = true };

            //  Create Json string
            string jsonString = JsonSerializer.Serialize(instList, options);

            //  Write to C:\data\ArrowLines.json
            //  Write arrowList to file
            File.WriteAllText(fileName, jsonString);

            //  Read file into 'jsonStringReturned'
            var jsonStringReturned = File.ReadAllText(fileName);

            //  Clear original list and deserialize into it - works
            instList.Clear();
            instList = System.Text.Json.JsonSerializer.Deserialize<List<Ret>>(jsonStringReturned);

            //  Read file contents into Json format 'arrowLinesList'
            var myDeserializedClass = System.Text.Json.JsonSerializer.Deserialize<List<Ret>>(jsonStringReturned);

            #endregion Write and read instList.json

            #region Create List<Trade> workingTrades
            List<CSV> CSv = new List<CSV>();
            List<NTDrawLine> nTDrawline = new List<NTDrawLine>();
            Source source = new Source();
            List<Trade> trades = new List<Trade>();
            List<Trade> workingTrades = new List<Trade>();


            //  NT runs through this section more than once
            //      Allow only one pass
            if (trades.Count == 0)
            {
                //	Create 'workingTrades' list																		//	Main
                //	Slimmed down instList that is added to source list to make transfer to extension easier
                // 	foreach through instList and add to trades list

                foreach (var inst in instList)
                {
                    trades.Add(new Trade(inst.ExecId, inst.Position, inst.Name, inst.Quantity, inst.IsEntry, inst.IsExit, inst.Price,
                        inst.Time, inst.HumanTime, inst.Instrument, inst.Expiry, inst.P_L, inst.Long_Short));
                }
            }
            //	Top row in Trades is last trade.  Position should be zero.  If not db error or trade was exited 
            //		next day
            //	Check that position is flat
            //if (t.Id == 0 && t.IsExit == true)
            //  Need to change from Console.WriteLine() to NT Print
            try
            {
                if (trades[0].Position != 0)

                {
                    Console.WriteLine(@"Postion on - not flat");                                                  //	Main
                    Console.WriteLine(string.Format("Trades position = {0}", (trades[0].Position)));        //System.Environment.Exit(-1);                                                                //	Main																
                }
            }
            catch
            {
            }
            //	Top row is now first trade in selected list - Position != 0
            trades.Reverse();
            workingTrades = trades.ToList();
            trades.Clear();

            #endregion Create List<Trade> workingTrades

            #region Code from 'Fill finList Prices Return List and Csv from Extension'							//	Main

            #region Create Lists
            //  Lists added to source which is used in extensions
            source.Trades = workingTrades;
            source.Csv = CSv;                                                                                   //	Main
            source.NTDrawLine = nTDrawline;
            #endregion Create Lists													

            #region Initialize flags and variables in source
            //	'rowInTrades' is increased on each pass of foreach(var t in workingTrades)
            //	It is number of the row in trades that is either an Entry, Exit, or Reversal
            source.rowInTrades = 0;

            //	isReversal is flag for reversal
            source.IsReversal = false;                                                                          //	Main
            #endregion Initialize flags and variables in source

            #region Fill in Id	Not used on Ryzen-2																				//	Main
            //	Add Id to workingTrades
            int i = 0;                                                                                          //	Main
                                                                                                                //  var workingTrades2 = workingTrades.ToList();

            foreach (var t in workingTrades)                                                                        //	Main
            {
                t.Id = i;                                                                                       //	Main
                i++;                                                                                            //	Main
            }
            #endregion Fill in Id																					//	Main

            #region foreach() through source.Trades
            foreach (var t in source.Trades)                                                                    //	Main
            {
                //	Record size of first entry and Id
                //	Need to keep record of how many entries are matched on split exits
                //	Updated in UpdateActiveEntery()
                if (t.Id == 0 && t.IsEntry == true)
                {
                    source.ActiveEntryId = t.Id;                                                                //	Main
                    source.ActiveEntryRemaining = t.Qty;                                                        //	Main
                    source.ActiveEntryPrice = t.Price;                                                          //	Main
                }

                //	Is trade a normal exit?
                //	If previous trade was reversal the source.Trades.IsRev is == true
                //if (t.Entry == false && t.Exit == true && source.Trades[source.rowInTrades - 1].IsRev == false) //	Main
                if (t.IsEntry == false && t.IsExit == true) //	Main
                {
                    source.Fill();
                }

                //	Set reversal flags row numbers
                if (t.IsEntry == true && t.IsExit == true)                                                          //	Main
                {
                    //	Set source.IsReversal = true - used to break out of Fill()
                    source.IsReversal = true;                                                                   //	Main
                    source.RowOfReverse = source.rowInTrades;                                                   //	Main
                    source.RowInTrades = source.rowInTrades;                                                    //	Main
                    source.rowInTrades = source.rowInTrades;                                                    //	Main
                    source.Fill();                                                                              //	Main		
                }

                source.rowInTrades++;  // = rowInTrades;														//	Main
                                       //	Increase source.rowInTrades it was cycled through in the Fill extension
                source.RowInTrades++;                                                                           //	Main
            }

            #endregion foreach() through source.Trades

            #endregion Code from 'Fill finList Prices Return List and Csv from Extension'										

            #region FillLongShortColumnInTradesList		

            //	Call extenstion 'FillLongShortColumnInTradesList()' to fill in Long_Short column in workingTrades 
            source.FillLongShortColumnInTradesList();

            #endregion                 

            #region foreach through .csv and add StartTimeTicks StartTime ExitTimeTicks ExitTime Long_Short

            foreach (var csv in source.Csv) 
            {
                //	fill in blank spaces from workingTrades with time and tick//

                csv.Name = workingTrades[csv.EntryId].Name;
                csv.StartTimeTicks = workingTrades[csv.EntryId].Time;
                csv.StartTime = workingTrades[csv.EntryId].HumanTime;
                csv.EndTimeTicks = workingTrades[csv.FilledBy].Time;
                csv.EndTime = workingTrades[csv.FilledBy].HumanTime;
                csv.Long_Short = workingTrades[csv.EntryId].Long_Short;
            }

            #endregion foreach through .csv and add StarTimeTicks StartTime ExitTimeTicks ExitTime

            #region Fill in P_L column in source.csv
            //	Call 'FillProfitLossColumnInTradesList' to fill in csv P_L column
            source.FillProfitLossColumnInTradesList();
            //source.
            #endregion Fill in P_L coulmn in source.csv

            #region Fill in Percent Column
            //  Divide P_L by quantity, divide entry price by 4, divide adjusted P/L by adjusted entry price
            source.FillPercentColumn();
            #endregion Fill in Percent Column

            #region Fill in Daily Percent Column

            source.FillDailyPercentColumn();
            #endregion Fill in Daily Percent Column

            #region Fill in Daily Total Column

            //	Call 'FillDailyTotalColumn' to fill in csv Daily Total column
            source.FillDailyTotalColumn();

            #endregion Fill in Daily Total Column

            #region Create NTDrawLine list for use in saving to file and later in NT

            source.NTDrawLine = source.CreateNTDrawline();

            #endregion Create NTDrawLine list for use in saving to file and later in NT

            #region Use LINQtoCSV on combined list to write - Not adjusted for LIFO
            //  foreach through source.NTDrawLine to create list with correct order for cc.write
            //  uses 'NTDrawLineForLINQtoCSV' which has column attributes
            var columnsWithAttributes = from l in source.NTDrawLine
                                        select new NTDrawLineForLINQtoCSV
                                        {
                                            Id = l.Id,
                                            Playback = false,
                                            Symbol = l.Symbol,
                                            Long_Short = l.Long_Short,
                                            StartTimeTicks = l.StartTimeTicks,
                                            StartTime = l.StartTime,
                                            StartY = l.StartY,
                                            EndTimeTicks = l.EndTimeTicks,
                                            EndTime = l.EndTime,
                                            EndY = l.EndY,
                                            P_L = l.P_L,
                                            Qty = l.Qty,
                                            P_LDividedByQty = l.P_LDividedByQty,
                                            Percent = l.Percent,
                                            DailyPercentTotal = l.DailyPercentTotal,
                                            DailyDollarTotal = l.DailyDollarTotal,
                                            TotalTrades = l.TotalTrades
                                        };
            columnsWithAttributes.ToList();


            ////  write to csvNTDrawline if not in Playback mode
                CsvFileDescription scvDescript = new CsvFileDescription();
                CsvContext cc = new CsvContext();
                //  write to parameters.OutputPath - normally cscNTDrawline
                cc.Write
                (
                columnsWithAttributes,
                @"C:\Data\InstList.csv"
                );

            #endregion Use LINQtoCSV on combined list to write - Not adjusted for LIFO

            /**************************************************************************
             * 
             *                  Second Section
             * 
             * 
             * ************************************************************************/

            //	Concat lines 
            lineCount = 0;
            iD = 0;
            bool playback = false;
            string symbol = "";
            string long_Short = "";
            long startTimeTicks = 0;
            string startTime = "";
            double startY = 0;
            long endTimeTicks = 0;
            string endTime = "";
            double endY = 0;
            double p_L = 0;
            int qty = 0;
            double? p_LDividedByQty = 0;
            double? percent = 0;
            int? TotalTrades = 0;
            int entryQty = 0;
            int exitQty = 0;
            List<NTDrawLine> listFromTwoLines = new List<NTDrawLine>();
            //  Foreach through orderedFullReport and fill in rows for NTDrawLine
            //  This will be used to access extensions
            ////  Reset lineCount for 2nd pass
            foreach (var l in orderedFullReport)
            {
                if (lineCount % 2 == 0)
                {
                    //	Save needed variable from entry
                    iD = l.ID;
                    symbol = l.Symbol;
                    long_Short = l.Long_Short;
                    startTimeTicks = l.TimeTicks;
                    startTime = l.DTTradeTime.ToString("HH:mm:ss  MM/dd/yyy");
                    startY = l.Price;
                    qty = l.Qty;
                    entryQty = l.Qty;

                }
                else if (lineCount % 2 == 1)
                {
                    endTimeTicks = l.TimeTicks;
                    endTime = l.DTTradeTime.ToString("HH:mm:ss  MM/dd/yyy");
                    endY = l.Price;
                    exitQty = l.Qty;
                    var compareQuantities = entryQty - exitQty;
                    if (compareQuantities != 0)
                    {
                        Console.WriteLine(" Check Quantities match");
                        MessageBox.Show("Check Quantities Match");

                    }
                    listFromTwoLines.Add(
                    new NTDrawLine
                    {
                        Id = iD,
                        Playback = false,
                        Symbol = symbol,
                        Long_Short = long_Short,
                        StartTimeTicks = startTimeTicks,
                        StartTime = startTime,
                        StartY = startY,
                        EndTimeTicks = endTimeTicks,
                        EndTime = endTime,
                        EndY = endY,
                        P_L = p_L,
                        Qty = qty

                    }
                    );
                }
                lineCount++;
            }


            #region List<NTDrawLIne> listFromTwoLines - Change 'Bought/Sold' to 'Long/Short'
            // Foreach through list and change 'Bought/Sold' to 'Long/Short'
            try
            {
                foreach (NTDrawLine n in listFromTwoLines)
                {
                    if (n.Long_Short == "Bought")
                    {
                        n.Long_Short = "Long";
                    }
                    if (n.Long_Short == "Sold")
                    {
                        n.Long_Short = "Short";
                    }


                }
            }
            catch
            {

            }

            #endregion Change 'Bought/Sold' to Long/Short

            #region Fill in P_L column and create nTDrawline

            listFromTwoLines.FillProfitLossColumnInTradesList();
            //  Switch to nTDrawline to enable use of written Extensions
            List<NTDrawLine> nTDrawLine = new List<NTDrawLine>();
            nTDrawLine = listFromTwoLines.ToList();

            #endregion Fill in P_L column and create nTDrawline

            #region Fill in Percent Column and P_LDividedByQty

            foreach (NTDrawLine n in nTDrawLine)
            {
                n.P_LDividedByQty = (double)n.P_L / n.Qty;
                n.Percent = (double)n.P_L / (n.Qty) / (double)(n.StartY / 4);
                n.Percent = (double?)Math.Round((double)n.Percent * 100, 2);
                //Math.Round((double)((percent.P_LDividedByQty / entryDividedByFour) * 100), 2);
            }

            #endregion Fill in Percent Column and P_LDividedByQty

            #region Fill in DailyDollarTotal

            Extensions.FillDailyTotalColumn(nTDrawLine);

            #endregion Fill in DailyDollarTotal

            #region Fill in DailyPercentTotal

            Extensions.FillDailyPercentColumn(nTDrawLine);

            #endregion Fill in DailyPercentTotal

            #region Use LINQtoCSV on combined list to write
            //  foreach through source.NTDrawLine to create list with correct order for cc.write
            //  uses 'NTDrawLineForLINQtoCSV' which has column attributes
            columnsWithAttributes = from l in nTDrawLine
                                        select new NTDrawLineForLINQtoCSV
                                        {
                                            Id = l.Id,
                                            Playback = false,
                                            Symbol = l.Symbol,
                                            Long_Short = l.Long_Short,
                                            StartTimeTicks = l.StartTimeTicks,
                                            StartTime = l.StartTime,
                                            StartY = l.StartY,
                                            EndTimeTicks = l.EndTimeTicks,
                                            EndTime = l.EndTime,
                                            EndY = l.EndY,
                                            P_L = l.P_L,
                                            Qty = l.Qty,
                                            P_LDividedByQty = l.P_LDividedByQty,
                                            Percent = l.Percent,
                                            DailyPercentTotal = l.DailyPercentTotal,
                                            DailyDollarTotal = l.DailyDollarTotal,
                                            TotalTrades = l.TotalTrades
                                        };
            columnsWithAttributes.ToList();
            #endregion Use LINQtoCSV on combined list to write

            #region Write to C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website" + fileSelectedName + " TradeStation Results " + ".csv"

            //CsvFileDescription scvDescript = new CsvFileDescription();
            //CsvContext cc = new CsvContext();

            //cc.Write
            //(
            //    nTDrawLine,
            //    @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\csvNTDrawline.csv"
            //);

            //  Write results to ...\2023 12 05 TradeStation Results.cs
            //cc.Write
            //(
            //    nTDrawLine,
            //    @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\" + fileSelectedName + " TradeStation Results " + ".csv"
            //);
            cc.Write
            (
                columnsWithAttributes,
                @"C:\Users\Owner\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\" + fileSelectedName + " Confirmation Results" + ".csv"
            );


            #endregion Write to C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\" + fileSelectedName + " TradeStation Results " + ".csv"


        }
    }
}
//#region enums
//public enum TimeAdjust
//{
//    PlusOne,
//    None,
//    SubtractOne,
//    Unassigned
//}
//public enum FileSource
//{
//    TSApp,
//    TSWebsite
//}

//#endregion enums
//public class Confirmation
//{
//    public int ID { get; set; }
//    public string Long_Short { get; set; }
//    public string Symbol { get; set; }
//    public string TradeDate { get; set; }
//    public string TradeTime { get; set; }
//    public long TimeTicks { get; set; }
//    public DateTime DTTradeTime { get; set; }
//    public double Price { get; set; }
//    public int Qty { get; set; }

//}

public static class Extensions
{
    public static List<NTDrawLine> FillProfitLossColumnInTradesList (this List<NTDrawLine> nTDrawLine)
    {
        foreach (var pl in nTDrawLine)
        {
            // 	Check for null - condition when prices have not been filled in yet in finList
            if (pl.StartY != 0 && pl.EndY != 0)
            {
                // long
                if (pl.Long_Short == "Long")
                {
                    try
                    {
                        // Exception for ExitPrice is null -- caused by making calculation on partial fill before price columns are filled in
                        //ln = LineNumber();
                        pl.P_L = (double)pl.EndY - (double)pl.StartY;

                        //  Multiply PL by qty to get correct value
                        pl.P_L = (double)pl.Qty * Math.Round((Double)pl.P_L, 2);
                    }
                    catch
                    {
                        //ln = LineNumber();
                        //ln.Dump("long catch block");
                        //pl.Exit.Dump("catch ExitPrice");
                        //pl.Entry.Dump("EntryPrice");
                    }
                }

                // short
                if (pl.Long_Short == "Short")
                {
                    try
                    {
                        pl.P_L = (double)pl.StartY - (double)pl.EndY;

                        //  Multiply PL by qty to get correct value
                        pl.P_L = (double)pl.Qty * Math.Round((Double)pl.P_L, 2);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        return nTDrawLine;

    }

    public static List<NTDrawLine> FillDailyTotalColumn(List<NTDrawLine> nTDrawLine)
    {
        //var x = nTDrawLine[0].EndTime;
        //  get date ("MM/dd/yyyy") portion of end date
        //  compare on each pass with starting date
        //  when date changes (string compare) enter new total into DailyTotal column
        var startingDate = nTDrawLine[0].EndTime.Substring(11);

        //  use to get trade end date to be used for comparison
        var currentTradeDate = "";

        //  use as register to total trade P/L values
        //  initialize with first value because starting poing for foreach is line 2
        double runningTotal = nTDrawLine[0].P_L;

        //  use as register to count number of trades in the day
        int TotalTrades = 1;

        //  need to keep track of line number in list
        int iD = 0;

        //  cycle through trades - compare trade end date with previous - record total on change
        //   zero accumulator
        foreach (var c in nTDrawLine)
        {
            //  get date of trade ("/MM/dd/yyy")
            currentTradeDate = c.EndTime.Substring(11);

            //  has date changed - value less than zero is change
            if (currentTradeDate.CompareTo(startingDate) == 0 && iD != 0)
            {
                //  add curent line P/L to accumulator
                runningTotal = runningTotal + c.P_L;

                //  add to number of days trades
                TotalTrades++;
            }

            //  date has changed
            else if (iD != 0)
            {
                //  insert total in DailyTotal column 1 line up
                nTDrawLine[iD - 1].DailyDollarTotal = runningTotal;

                //  insert total in TotalTrades column 1 line up
                nTDrawLine[iD - 1].TotalTrades = TotalTrades;


                //  zero accumulator - this if is hit when dates are unequal so running total 
                //      needs to be set to rows P/L - zero is not needed
                runningTotal = 0;

                //  zero TotalTrades
                TotalTrades = 1;

                //  add curent line P/L to accumulator
                runningTotal = runningTotal + c.P_L;

                //  update trade end date
                startingDate = currentTradeDate;
            };

            //  update line ID
            iD++;

            //  if ID  == list.count - at end of list - enter last total
            if (iD == nTDrawLine.Count)
            {
                nTDrawLine[iD - 1].DailyDollarTotal = runningTotal;

                //  enter number of trades in TotalTrades
                nTDrawLine[iD - 1].TotalTrades = TotalTrades;

            }
        }

        return nTDrawLine;
    }

    public static List<NTDrawLine> FillDailyPercentColumn(this List<NTDrawLine> nTDrawLine)
    {
        //  get date ("MM/dd/yyyy") portion of end date
        //  compare on each pass with starting date
        //  when date changes (string compare) enter new total into DailyTotal column
        var startingDate = nTDrawLine[0].EndTime.Substring(11);

        //  use to get trade end date to be used for comparison
        var currentTradeDate = "";

        //  use as register to total trade P/L values
        //  initialize with first value because starting poing for foreach is line 2
        double runningTotal = (double)nTDrawLine[0].Percent;

        //  use as register to count number of trades in the day
        int TotalTrades = 1;

        //  need to keep track of line number in list
        int iD = 0;

        //  cycle through trades - compare trade end date with previous - record total on change
        //   zero accumulator
        foreach (var c in nTDrawLine)
        {
            //  get date of trade ("/MM/dd/yyy")
            currentTradeDate = c.EndTime.Substring(11);

            //  has date changed - value less than zero is change
            if (currentTradeDate.CompareTo(startingDate) == 0 && iD != 0)
            {
                //  add curent line P/L to accumulator
                runningTotal = runningTotal + (double)c.Percent;

                //  add to number of days trades
                TotalTrades++;
            }

            //  date has changed
            else if (iD != 0)
            {
                //  insert total in DailyTotal column 1 line up
                //nTDrawLine[iD - 1].DailyPercentTotal = runningTotal;
                nTDrawLine[iD - 1].DailyPercentTotal = Math.Round((runningTotal * 100), 1);

                //  insert total in TotalTrades column 1 line up
                //nTDrawLine[iD - 1].TotalTrades = TotalTrades;


                //  zero accumulator - this if is hit when dates are unequal so running total 
                //      needs to be set to rows P/L - zero is not needed
                runningTotal = 0;

                //  zero TotalTrades
                TotalTrades = 1;

                //  add curent line P/L to accumulator
                runningTotal = runningTotal + (double)c.Percent;

                //  update trade end date
                startingDate = currentTradeDate;
            };

            //  update line ID
            iD++;

            //  if ID  == list.count - at end of list - enter last total
            if (iD == nTDrawLine.Count)
            {
                //Math.Round((double)((percent.P_LDividedByQty / marginToPoints) * 100), 1);
                //nTDrawLine[iD - 1].DailyPercentTotal = runningTotal;
                //nTDrawLine[iD - 1].DailyPercentTotal = Math.Round((runningTotal * 100), 1);
                nTDrawLine[iD - 1].DailyPercentTotal = runningTotal;


                //  enter number of trades in TotalTrades
                //nTDrawLine[iD - 1].TotalTrades = TotalTrades;

            }
        }

        return nTDrawLine;
    }


}