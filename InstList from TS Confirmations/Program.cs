/*
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
 *  
 * 2024 01 31
 *  Adding code to use 'Trade Station' selection in form
 *  Use new branch - Activate Trade Station Button
 *  Create property in Form1.cs (double click on radio button to access From1.cs)
 *  Created string initialDirectory to choose which directory to open
 *  After 'Concat lines from TS report into fullLine' need to create instList for TS App report and then
 *      call extension instList.
 *  In Ryzen - 2\WPFDemo.Read All Lines WinForms an instList can be created from subs[] -> instList = new List<Ret>()
 *  
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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static WindowsFormsApp1.Classes;
using static WindowsFormsApp1.Enums;
using static WindowsFormsApp1.Variables;
using static InstList_from_TS_Confirmations.Methods;


namespace WindowsFormsApp1
{
    public class Variables
    {
        public static string filePath = null;
        Form1 form = new Form1();
        public static FileSource fileSource = new FileSource();
        public static string initialDirectory = "";
        public static string title = "";
        public static string fileSelected = null;
        public static string fileSelectedName = "";
        public static int lineCount = 0;
        public static List<Ret> instList = new List<Ret>();
    }
    public static class Program
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
            System.Windows.Forms.Application.EnableVisualStyles();

            //  Sets the application-wide default for the UseCompatibleTextRendering property defined on certain controls.
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            //  need to create form before calling it for return values
            Form1 form = new Form1();

            //  Begins running a standard application message loop on the current thread, without a form.
            System.Windows.Forms.Application.Run(form);
            //bool fileSource = form.FileSource;
            //bool maleBtn = form.maleBtn;

            //  Get results of file source radio buttons
            //FileSource fileSource = new FileSource();
            fileSource = form.FileOrigin;
            //if (fileSource == FileSource.TSApp)
            //{
            //    MessageBox.Show("App is source");

            //}
            //else if (fileSource == FileSource.TSWebsite )
            //    {
            //    MessageBox.Show("Website is source");

            //}
            //bool tSApp = form.tSSource;
            //  File dialog to get file from either ...App Data\NinjaTrader\NinjaTrader Data\Trade Station Confirmations
            //  or ...C:\Users\Owner\IDrive-Sync\TradeManagerAnalysis
            //  Use fileSource to set InitialDirectory
            //  
            //maleB
            #endregion Start form


            //  2023 DST Sun, Mar 12, 2023 2:00 AM - Sun, Nov 5, 2023 2:00
            //DateTime startDST22 = new DateTime(2022, 03, 13);
            //DateTime endDST22 = new DateTime(2022, 11, 06);
            //DateTime startDST23 = new DateTime(2023, 03, 12);
            //DateTime endDST23 = new DateTime(2023, 11, 05);
            //DateTime startDST24 = new DateTime(2024, 03, 10);
            //DateTime endDST24 = new DateTime(2024, 11, 03);
            //  Set InitialDirectory to ...\Data from Website or ...C:\Users\Owner\IDrive-Sync\TradeManagerAnalysis
            //string initialDirectory = "";
            //string title = "";

            //List<Ret> instList = new List<Ret>();
            //int lineCount = 0;
            //string fileSelectedName = "";
            //List<LinesJoined> fullLine = new List<LinesJoined>();
            //  Used for location where file is to be written
            //  Set in each of the different secions (TSApp, TSWebsite, NTExport)
            string filePath = null;


            if (fileSource == FileSource.TSWebsite)
            {
                instList = Website();
            }
            else if (fileSource == FileSource.TSApp)
            {
                instList = TSApp();
            }
            else if (fileSource == FileSource.NTExport)
            {
                instList = NTExport();
            }

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
            if (fileSource == FileSource.TSWebsite || fileSource == FileSource.TSApp)
            {
                filePath = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\2024 02 Feb\Results\";
            }
            else if (fileSource == FileSource.NTExport)
            {
                filePath = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\NinjaTrader Exports\2024 02 Feb\Results\";
            }

            cc.Write
                (
                    columnsWithAttributes,
                    //@"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\" + fileSelectedName + " Confirmation Results" + ".csv"
                    filePath + fileSelectedName + " Confirmation Results" + ".csv"

                );

            #endregion Use LINQtoCSV on combined list to write - Not adjusted for LIFO
        }
    }
}
