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
 *  2024 02 22
 *      When trade is a bond the price will cause an error - 'Wrong format'
 *      Short term fix is to exclude lines in 'Add' in Methods.NTExport to exclude any contract that is not NQ Mar24. Ln 402
 *      Bond price format causes a with parse - Price = (double?)Decimal.Parse(subs[6]),
 *
 * 2024 02 26
 *  Adding code to Methods.TSApp to calculate Win/Loss ratio
 *  Try adding more variables to CSV (source.Csv is list that is modified by extensions)
 *      Added Win as decimal.  Others are double? and I think decimal caused a problem.
 *  Problem when trying to add nullable int (int?) to nTDrawlLine in extension. Error: Need to assign a vale to nullable parameter.
 *      Solved problem by add steps in 'FillWinLossColumn' that assigned null to all variables.  Solved problem and 0 was not displayed in view grid.
 *      
 *  2024 05 02
 *      Updated extension 'FillDailyWinLossColumn' to use Linq to calculate daily summary results for each symbol.
 *      Need to add that info to '2024 04 26  Confirmation Results.csv'
 *      
 *  2024 05 03
 *      Need to reverse list without changing list
 *      https://stackoverflow.com/questions/19102021/how-to-reverse-a-generic-list-without-changing-the-same-list
 *      'foreach (var t1 in Enumerable.Reverse(t.List))'
 *      
 * 2024 05 04  
 *  Extensions:
 *       public static List<NTDrawLine> CreateNTDrawline(this Source source)
 *          Creates 'nTDrawLine' which is used to fill output file
 *          
 *       public static Source Fill(this Source source)
 *          Enters data into source.Csv file
 *          
 *       public static Source FillDailyPercentColumn(this Source source)
 *          Fills in DailyPercentColumn only
 *          
 *       public static Source FillDailyTotalColumn(this Source source)
 *          Fills in DailyDollarTotal and TotalTrades only
 *          
 *       public static Source FillLongShortColumnInTradesList(this Source source)
 *          Determines whether entry is a long or short position and fills in Long_Short column for entries
 *          
 *       public static Source FillProfitLossColumnInTradesList(this Source source)
 *          Fills in P/L column
 *          
 *       public static Source FillPercentColumn (this Source source)
 *          Fills in PercentReturn column
 *          
 *       public static Source FillDailyWinLossColumn(this Source source)
 *          Extract P/L from source.Csv.P_L and place in Win/Loss/Zero columns
 *          Changes source.Csv 
 *          Fills in last line with sums from CSV.Win and CSV.Loss columns
 *          If multiple symbols each different symbols has summary values calculated
 *          Creates lastRow list - summary figures that will be overwritten
 *          
 *       public static Source FillWinLossSummary(this Source source)
 *          Fills in far right columns for page summary
 *          Will compile stats for multiple days if available
 *          
 *       public static Source GetActiveEntry(this Source source)  
 *          On first pass ActiveEntry numbers have been set in Main()
 *          Get starting entry price row and values
 *          Start at first entry above exit and search for row that has entry = true and matched = false
 *          Record starting Id
 *          
 *       public static Source MatchAndAddToCsv(this Source source)  
 *          Called from Fill
 *          Sets Matched to true for Exit and Entry in source.Trades
 *          Adds row to csv
 *          csv will not be in correct order to sort before exit
 *          
 *       public static Source UpdateActiveEntry(this Source source)
 *          Subtracts qty of exits from first entry that is not filled
 *          To get next open entry after source.ActiveEntryRemaining == 0 work down from top to find first .Matched == false
 *          All exits will be matched
 *          
 *  2024 12 25
 *      Error that Classes.Source does not contain a definition for 'CreateNTDrawline'
 *      The extension is missing from #region sttements.
 *      Assuming that at some point I copied it and hit cut instead.
 *      Extension is intact in NinjaTrader.Custom.sln in Ryzen-2 and ZBook
 *      Creating new branch and adding the extension from NinjaTrader.Custom.sln
 *      Copied CreateNTDrawline() from NinjaTrader.Custom.AddOns.SqLiteExecutionsExtension to SqLiteExecutionsExtension
 *	    Problem solved.
 *	    
 */

using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        public static List<CSV> lastRow = new List<CSV>();
        public static List<MultipleSymbols> multipleSymbols = new List<MultipleSymbols>();
    }
    public static class Program
    {
        [STAThread]
        static void Main()
        {
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

            //  Get results of file source radio buttons
            fileSource = form.FileOrigin;
            #endregion Start form

            #region Select Method
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
            #endregion Select Method

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
			instList = instList.OrderBy(j => j.Name).ThenBy(j => j.Time).ToList();
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
                //	Create 'workingTrades' list																		
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
            //  if (t.Id == 0 && t.IsExit == true)
            //  Need to change from Console.WriteLine() to NT Print
            try
            {
                if (trades[0].Position != 0)

                {
                    Console.WriteLine(@"Postion on - not flat");                                                  
                    Console.WriteLine(string.Format("Trades position = {0}", (trades[0].Position)));        
                    //System.Environment.Exit(-1);                                                               															
                }
            }
            catch
            {
            }
            //	Top row is now first trade in selected list - Position != 0
            trades.Reverse();
			workingTrades = trades.ToList();
			//workingTrades = trades.OrderBy(j => j.Name).ThenBy(j => j.Time).ToList();

			trades.Clear();

            #endregion Create List<Trade> workingTrades

            #region Code from 'Fill finList Prices Return List and Csv from Extension'							

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

            #region Fill in Id	
            //	Add Id to workingTrades
            int i = 0;
            foreach (var t in workingTrades) 
            {
                t.Id = i;
                i++;  
            }
            #endregion Fill in Id

            #region foreach() through source.Trades
            foreach (var t in source.Trades) 
            {
                //	Record size of first entry and Id
                //	Need to keep record of how many entries are matched on split exits
                //	Updated in UpdateActiveEntery()
                if (t.Id == 0 && t.IsEntry == true)
                {
                    source.ActiveEntryId = t.Id;   
                    source.ActiveEntryRemaining = t.Qty;     
                    source.ActiveEntryPrice = t.Price;   
                }

                //	Is trade a normal exit?
                //	If previous trade was reversal the source.Trades.IsRev is == true
                //if (t.Entry == false && t.Exit == true && source.Trades[source.rowInTrades - 1].IsRev == false) 
                if (t.IsEntry == false && t.IsExit == true) 
                {
                    source.Fill();
                }

                //	Set reversal flags row numbers
                if (t.IsEntry == true && t.IsExit == true) 
                {
                    //	Set source.IsReversal = true - used to break out of Fill()
                    source.IsReversal = true; 
                    source.RowOfReverse = source.rowInTrades;
                    source.RowInTrades = source.rowInTrades; 
                    source.rowInTrades = source.rowInTrades; 
                    source.Fill(); 	
                }

                source.rowInTrades++; 
                //	Increase source.rowInTrades it was cycled through in the Fill extension
                source.RowInTrades++; 
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
            #endregion Fill in P_L coulmn in source.csv

            #region Fill in Percent Column
            //  Divide P_L by quantity, divide entry price by 4, divide adjusted P/L by adjusted entry price
            source.FillPercentColumn();
            #endregion Fill in Percent Column

            #region Fill in Win/Loss Columns

            //  Copy P/L numbers into Win, Loss, or Zero columns
            source.FillDailyWinLossColumn();

            #endregion Fill in Win/Loss Columns

            #region FillWinLossSummary
            //  Fill page summary in far right columns
            source.FillWinLossSummary();
            #endregion FillWinLossSummary

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
            var x = source.NTDrawLine[0].WinLossRatio;
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
                                            TotalTrades = l.TotalTrades,
                                            Blank = "",
                                            Win = l.Win,
                                            Loss = l.Loss,
                                            Zero = l.Zero,
                                            WinTot = l.WinTot,  
                                            LossTot = l.LossTot,
                                            WinCount = l.WinCount,
                                            LossCount = l.LossCount,
                                            ZeroCount = l.ZeroCount,
                                            Count = l.Count,
                                            WinLossPercent = l.WinLossPercent,
                                            AvgWin = l.AvgWin,
                                            AvgLoss = l.AvgLoss,
                                            WinLossRatio = l.WinLossRatio,
                                            PBlank = l.PBlank,
                                            PwinTot = l.PwinTot,
                                            PlossTot = l.PlossTot,
                                            PwinCount = l.PwinCount,
                                            PlossCount = l.PlossCount,
                                            PzeroCount = l.PzeroCount,
                                            Pcount = l.Pcount,
                                            PwinLossPercent = l.PwinLossPercent,
                                            PavgWin = l.PavgWin,
                                            PavgLoss = l.PavgLoss,
                                            PwinLossRatio = l.PwinLossRatio
                                        };
            columnsWithAttributes.ToList();

            
            //  write to csvNTDrawline if not in Playback mode
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
                filePath = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\Data from Website\2024 11 Nov\Results\";
            }
            else if (fileSource == FileSource.NTExport)
            {
                filePath = @"C:\Users\Rod\AppData\Local\NinjaTrader\NinjaTrader Data\NinjaTrader Exports\2024 05 May\Results\";
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
