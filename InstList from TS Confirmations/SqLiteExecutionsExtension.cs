using System;
using System.Collections.Generic;
using System.Linq;
using static WindowsFormsApp1.Classes;
using MoreLinq;
//using NinjaTrader.NinjaScript;


namespace WindowsFormsApp1
{
    public static class MySQLiteExtensions
    {
        #region Create NTDrawline for save to .csv

        public static List<NTDrawLine> CreateNTDrawline(this Source source)
        {
            // NTDrawLine -> Id, EntryPrice, EntryTime(ticks), ExitPrice, ExitTime(ticks)
            List<NTDrawLine> nTDrawLine = new List<NTDrawLine>();
            // Counter for nTDrawLine lines

            foreach (var csv in source.Csv)
            {
                try
                {
                    var t = DateTime.Parse(csv.StartTime).Ticks;

                    nTDrawLine.Add(
                        new NTDrawLine
                        {
                            Id = 0,
                            Playback = false,
                            Symbol = csv.Name,
                            Long_Short = csv.Long_Short,
                            StartTimeTicks = (long)csv.StartTimeTicks,
                            StartTime = DateTime.Parse(csv.StartTime).ToString("HH:mm:ss  MM/dd/yyy"),
                            StartY = (double)csv.Entry,
                            EndTimeTicks = (long)csv.EndTimeTicks,
                            EndTime = DateTime.Parse(csv.EndTime).ToString("HH:mm:ss  MM/dd/yyy"),
                            EndY = (double)csv.Exit,
                            P_L = (double)csv.P_L,
                            Qty = (int)csv.Qty,
                            P_LDividedByQty = csv.P_LDividedByQty,
                            Percent = (double?)csv.PercentReturn,
                            DailyPercentTotal = (double?)csv.DailyPercentTotal,
                            DailyDollarTotal = (double?)csv.DailyDollarTotal,
                            TotalTrades = (int?)csv.TotalTrades,
                            Win = csv.Win,
                            Loss = csv.Loss,
                            Zero = (double?)csv.Zero,
                            WinTot = csv.WinTot,
                            LossTot = csv.LossTot,
                            WinCount = csv.WinCount,
                            LossCount = csv.LossCount,
                            ZeroCount = csv.ZeroCount,
                            Count = csv.Count,
                            WinLossPercent = csv.WinLossPercent,
                            AvgWin = csv.AvgWin,
                            AvgLoss = csv.AvgLoss,
                            WinLossRatio = csv.WinLossRatio,
                            PBlank = csv.PBlank,
                            PwinTot = csv.PwinTot,
                            PlossTot = csv.PlossTot,
                            PwinCount = csv.PwinCount,
                            PlossCount = csv.PlossCount,
                            PzeroCount = csv.PzeroCount,
                            Pcount = csv.Pcount,
                            PwinLossPercent = csv.PwinLossPercent,
                            PavgWin = csv.PavgWin,
                            PavgLoss = csv.PavgLoss,
                            PwinLossRatio = csv.PwinLossRatio
                        }
                    );
                }
                catch ( Exception ex)
                {
                    Console.WriteLine( ex );
                }
            }
            int nTDrawLineId = 0;
            foreach (var e in nTDrawLine)
            {
                e.Id = nTDrawLineId;
                nTDrawLineId++;
            }

            return nTDrawLine;
        }

        #endregion Create NTDrawline for save to .csv

        #region Fill
        // 	Extenstion to fill Exit info
        //	Called from 'Check for normal exit (Entry == false - Exit == true)'
        //	Finds entry and exit prices
        //	All source.ActiveEntry... vars are found in 'UpdateActiveEntry' - allows Fill() and PartialFill() to use same code
        public static Source Fill(this Source source)
        {


                //	Fill() Called by Main () at line 449 / 522
                //Console.WriteLine("\nFill() Called by " + memberName + " () at line " + LineNumber + " / " + LN());

                //	Get Qty of Exit 
                //	Set source.ExitQty and source.Remaining to quantity in Exit row
                //	These numbers should be updated on each match with an Entry for following progress
                //	Example 
                //	ExitQty		Remaining
                //		3			3
                //		2			1
                //		1			0		prece
                //	On partial fill this doesn't work 
                //	source.remaining should be number of unmatched entries from preceeding entry - not necessarily the most
                //		recent entry.  Adjust only if source.ActiveEntryRemaining = 0

                //	Get starting entry info
                //	Looks for first entry above (matched == false) 
                //  If partial fill need to go higher to check all entries

                //	2022 09 18 1000
                //	RowInTrades not set on 2nd exit 
                //	Try setting on entry 
                source.RowInTrades = source.rowInTrades;
                source.GetActiveEntry();

                //	Number of exits that have not been matched
                source.Remaining = source.Trades[source.RowInTrades].Qty;                                       //	Fill			

                //	Save exit price - it is used for all entry matches
                source.StartingExitPrice = source.Trades[source.RowInTrades].Price;                             //	Fill

                while (source.Remaining > 0)                                                                        //	Fill
                {

                    source.UpdateActiveEntry();                                                             //	Fill
                    source.MatchAndAddToCsv();                                                              //	Fill

                    //	Break on source.IsReversal = true;
                    //	Only need one pass

                    if (source.IsReversal == true)
                    {
                        source.IsReversal = false;
                        return source;
                    }
                    //	 Fill

                    //	Back up through trades to match all fills
                    //	rowInTades keeps track of the row in Trades as each line is processed
                    //	while source.RowInTrades is used to back up through Trades list to find matched entries
                    source.RowInTrades--;                                                                       //	Fill
                }
                //Console.WriteLine("\nFill() Returned to " + memberName + "() at line " + LineNumber + " / " + LN());

                return source;                                                                                  //	Fill
            }
        #endregion Fill        

        #region FillDailyPercentColumn

        public static Source FillDailyPercentColumn(this Source source)
        {
            //  get date ("MM/dd/yyyy") portion of end date
            //  compare on each pass with starting date
            //  when date changes (string compare) enter new total into DailyTotal column
            var startingDate = source.Csv[0].EndTime.Substring(11);

            //  use to get trade end date to be used for comparison
            var currentTradeDate = "";

            //  use as register to total trade P/L values
            //  initialize with first value because starting poing for foreach is line 2
            double runningTotal = (double)source.Csv[0].PercentReturn;

            //  use as register to count number of trades in the day
            int TotalTrades = 1;

            //  need to keep track of line number in list
            int iD = 0;

            //  cycle through trades - compare trade end date with previous - record total on change
            //   zero accumulator
            foreach (var c in source.Csv)
            {
                //  get date of trade ("/MM/dd/yyy")
                currentTradeDate = c.EndTime.Substring(11);

                //  has date changed - value less than zero is change
                if (currentTradeDate.CompareTo(startingDate) == 0 && iD != 0)
                {
                    //  add curent line P/L to accumulator
                    runningTotal = runningTotal + (double)c.PercentReturn;

                    //  add to number of days trades
                    TotalTrades++;
                }

                //  date has changed
                else if (iD != 0)
                {
                    //  insert total in DailyTotal column 1 line up
                    source.Csv[iD - 1].DailyPercentTotal = runningTotal;

                    //  insert total in TotalTrades column 1 line up
                    source.Csv[iD - 1].TotalTrades = TotalTrades;


                    //  zero accumulator - this if is hit when dates are unequal so running total 
                    //      needs to be set to rows P/L - zero is not needed
                    runningTotal = 0;

                    //  zero TotalTrades
                    TotalTrades = 1;

                    //  add curent line P/L to accumulator
                    runningTotal = runningTotal + (double)c.PercentReturn;

                    //  update trade end date
                    startingDate = currentTradeDate;
                };

                //  update line ID
                iD++;

                //  if ID  == list.count - at end of list - enter last total
                if (iD == source.Csv.Count)
                {
                    source.Csv[iD - 1].DailyPercentTotal = runningTotal;

                    //  enter number of trades in TotalTrades
                    source.Csv[iD - 1].TotalTrades = TotalTrades;

                }
            }

            return source;
        }

        #endregion FillDailyPercentColumn

        #region FillDailyTotalColumn
        //  for source
        public static Source FillDailyTotalColumn(this Source source)
        {

            //  get date ("MM/dd/yyyy") portion of end date
            //  compare on each pass with starting date
            //  when date changes (string compare) enter new total into DailyTotal column
            var startingDate = source.Csv[0].EndTime.Substring(11);

            //  use to get trade end date to be used for comparison
            var currentTradeDate = "";

            //  use as register to total trade P/L values
            //  initialize with first value because starting poing for foreach is line 2
            double runningTotal = source.Csv[0].P_L;

            //  use as register to count number of trades in the day
            int TotalTrades = 1;

            //  need to keep track of line number in list
            int iD = 0;

            //  cycle through trades - compare trade end date with previous - record total on change
            //   zero accumulator
            foreach (var c in source.Csv)
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
                    source.Csv[iD - 1].DailyDollarTotal = runningTotal;

                    //  insert total in TotalTrades column 1 line up
                    source.Csv[iD - 1].TotalTrades = TotalTrades;


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
                if (iD == source.Csv.Count)
                {
                    source.Csv[iD - 1].DailyDollarTotal = runningTotal;

                    //  enter number of trades in TotalTrades
                    source.Csv[iD - 1].TotalTrades = TotalTrades;

                }
            }

            return source;
        }

        //  for List<NTDrawline>
        public static List<NTDrawLine> FillDailyTotalColumn
            (this List<NTDrawLine> source)

        {

            //  get date ("MM/dd/yyyy") portion of end date
            //  compare on each pass with starting date
            //  when date changes (string compare) enter new total into DailyTotal column
            var startingDate = source[0].EndTime.Substring(11);

            //  use to get trade end date to be used for comparison
            var currentTradeDate = "";

            //  use as register to total trade P/L values
            //  initialize with first value because starting poing for foreach is line 2
            double runningTotal = source[0].P_L;

            //  use as register to count number of trades in the day
            int TotalTrades = 1;

            //  need to keep track of line number in list
            int iD = 0;

            // remove duplicates
            var sourceDistinct = source.DistinctBy(c => c.StartTimeTicks);



            //returnedClassPlayBackTrades = from l in returnedClass
            //                              where l.Playback == true


            //  order list with MoreLinq
            //  DistinctBy returns IENumerable which is what OrderBy wants do not change DistinctBy to list - dowsn't work
            var sourceOrderBy = sourceDistinct.OrderBy(e => e.StartTimeTicks).ToList();

            var sourceDistinctToList = (from l in sourceOrderBy
                                        select new NTDrawLine
                                        {
                                            Id = l.Id,
                                            Playback = l.Playback,
                                            Symbol = l.Symbol,
                                            Long_Short = l.Long_Short,
                                            StartTimeTicks = l.StartTimeTicks,
                                            StartTime = l.StartTime,
                                            StartY = l.StartY,
                                            EndTimeTicks = l.EndTimeTicks,
                                            EndTime = l.EndTime,
                                            EndY = l.EndY,
                                            P_L = l.P_L,
                                            DailyDollarTotal = null,
                                            TotalTrades = null
                                        }).ToList();


            //  delete contents of DailyTotal and TotalTrades and fill in Id column
            int id = 0;
            foreach ( var s in sourceOrderBy)
            {
                s.Id = id;
                id++;

                s.DailyDollarTotal = true ? (double?)null : null;
                s.TotalTrades = (int?)null;
            }

            //  cycle through trades - compare trade end date with previous - record total on change
            //   zero accumulator
            //foreach (var c in source)
            foreach (var c in sourceOrderBy)

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
                    sourceOrderBy[iD - 1].DailyDollarTotal = runningTotal;

                    //  insert total in TotalTrades column 1 line up
                    source[iD - 1].TotalTrades = TotalTrades;


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
                if (iD == sourceOrderBy.Count)
                {
                    sourceOrderBy[iD - 1].DailyDollarTotal = runningTotal;

                    //  enter number of trades in TotalTrades
                    sourceOrderBy[iD - 1].TotalTrades = TotalTrades;

                }
            }


            return sourceOrderBy;
        }

        #endregion FillDailyTotalColumn       

        #region FillLongShortColumnInTradesList
        // 	Determines whether entry is a long or short position and fills in Long_Short column for entries
        //	Exits are left as null
        //	Called from Main()
        // 	Extenstion to fill Exit info
        //	Called from 'Check for normal exit (Entry == false - Exit == true)'
        //	Finds entry and exit prices
        //	All source.ActiveEntry... vars are found in 'UpdateActiveEntry' - allows Fill() and PartialFill() to use same code

        public static Source FillLongShortColumnInTradesList(this Source source)
        {
            //	Fill() Called by Main () at line 449 / 522
            //Console.WriteLine("\nFillLongShortColumnInTradesList() Called by " + memberName + " () at line " + LineNumber + " / " + LN());
            //	Order is set to top entry being the last trade
            //	Position value will be zero
            //	foreach through list and compare previous position to current position
            //	Increase in position after entry == long while decrease after entry == short		
            //source.Trades.Reverse();
            //	Need to renumber Id column
            int tradesId = 0;
            long? lastPosition = 0;

            foreach (var id in source.Trades)
            {
                id.Id = tradesId;
                tradesId++;
            }
            //	longShort = ""	is set to null on entry
            //	will only be null on start of foreach
            string longShort = "";
            foreach (var ls in source.Trades)
            {
                ////	Exit if first trade is not an exit
                //if (ls.Id == 0 && ls.IsExit == false)
                //{
                //    Console.WriteLine(@"First trade is exit");
                //                                              
                //    System.Environment.Exit(-1);              
                //}
                #region First trade fill in 'Long' or 'short'
                // Fill in Long_Short column
                // Is entry buy or sell first trade (LongShort is initialized to "")
                if (longShort == "")
                {
                    if (ls.Position <= -1)
                    {
                        //position = Position.Short; 
                        longShort = "Short";
                        //lastPosition = -1;
                        //break;
                    }
                    else if (ls.Position >= 1)
                    {
                        //position = Position.Long;
                        longShort = "Long";
                        //lastPosition = 1;
                        //break;
                    }
                }
                #endregion

                #region Fill in 'Long' or 'Short' column for remaining entries in trade
                //	Fill in Trades Long_Short column with "null" if trade is an exit
                //  Sets Long_Short to null on reversal - change to check for both entry and exit == true and then fill in Long_Short column
                if (ls.IsExit == true)
                {
                    //  both true means reverse
                    if (ls.IsExit == true && ls.IsEntry == true)
                    {
                        //  position positive is long trade
                        if (ls.Position > 0)
                        {
                            //position = Position.Long;
                            longShort = "Long";
                            lastPosition = ls.Position;
                            ls.Long_Short = longShort;
                        }

                        //  position negative is short trade
                        else if (ls.Position < 0)
                        {
                            //position = Position.Short;
                            longShort = "Short";
                            lastPosition = ls.Position;
                            ls.Long_Short = longShort;
                        }

                        //  skip setting Long_Short to null
                        goto done;
                    }

                    longShort = null;
                    lastPosition = ls.Position;
                    ls.Long_Short = null;

                //  branches to here
                done:;

                    //  will go to next item and skip the check for entry
                    continue;
                }

                // If position size increases (positive) trade was a long
                //	Fill in posList 'Long_Short' with "Long"
                if (ls.IsEntry == true && ls.Position > lastPosition)
                {
                    longShort = "Long";
                    lastPosition = ls.Position;
                    ls.Long_Short = longShort;
                }

                // If position size increases (negative) trade was a short
                //	Fill in posList 'Long_Short' with "Short"
                else if (ls.IsEntry == true && ls.Position < lastPosition)
                {
                    longShort = "Short";
                    lastPosition = ls.Position;
                    ls.Long_Short = longShort;
                }

                #endregion

            }



            //Console.WriteLine("\nFill() Returned to " + memberName + "() at line " + LineNumber + " / " + LN());

            return source;                                                                                  //	Fill
        }
        #endregion FillLongShortColumnInTradesList

        #region Fill in workingTrades P/L column using Source

        public static Source FillProfitLossColumnInTradesList(this Source source)
        {
            foreach (var pl in source.Csv)
            {
                // 	Check for null - condition when prices have not been filled in yet in finList
                if (pl.Exit.HasValue && pl.Entry.HasValue)
                {
                    // long
                    if (pl.Long_Short == "Long")
                    {
                        try
                        {
                            // Exception for ExitPrice is null -- caused by making calculation on partial fill before price columns are filled in
                            //ln = LineNumber();
                            pl.P_L = (double)pl.Exit - (double)pl.Entry;

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
                            pl.P_L = (double)pl.Entry - (double)pl.Exit;

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
            return source;
        }

        #endregion Fill in workingTrades P/L column using Source

        #region FillPercentColumn

        public static Source FillPercentColumn (this Source source)
        {

            //  assign divisor using margin requirement or 4 for stocks  
            double? divisor = null;
            foreach (var percent in source.Csv)
            {
                //  Divide P/L by quantity to get return / share
                percent.P_LDividedByQty = percent.P_L / percent.Qty;
                //  percent return = P/L / (Entry Price / 4)
                //  divide P/L by qty
                //  Check whether instrument is Future  or Stock
                if (source.InstrumentType == "Future")
                {
                    //  source.Margin is a property set for indicator
                    if (source.Name == "NQ")
                    {
                        //  Futures use margin
                        //  Convert dollars to NQ points
                        var marginToPoints = source.Margin / 20;
                        //  Return is P/L divided by margin in points
                        percent.PercentReturn = Math.Round((double)((percent.P_LDividedByQty / marginToPoints) * 100), 1);
                    }
                    if ( source.Name == "ZN")
                    {
                        //  Futures use margin
                        //  Convert dollars to NQ points
                        var marginToPoints = source.Margin / 1000;
                        //  Return is P/L divided by margin in points
                        percent.PercentReturn = Math.Round((double)((percent.P_LDividedByQty / marginToPoints) * 100), 1);
                    }
                }
                //  set source.InstrumentType to 'Stock' for working with TS confirmation reports
                source.InstrumentType = "Stock";
                if (source.InstrumentType == "Stock")
                {
                        //  Stocks use 4 to 1 leverage
                        var entryDividedByFour = (double)percent.Entry / 4;
                        percent.PercentReturn = Math.Round((double)((percent.P_LDividedByQty / entryDividedByFour) * 100), 2);

                }
            }
            return source;
        }
        #endregion FillPercentColumn

        #region FillDailyWinLossColumn
        //  Extract P/L from source.Csv.P_L and place in Win/Loss/Zero columns
        public static Source FillDailyWinLossColumn(this Source source)
        {
            #region Local variables
            //  Keep track of position in .Csv list
            //  At end fill in total values
            double? avgWin = 0;
            double? avgLoss = 0;
            double? lossTotal = 0;
            int? lossCount = 0;
            int? winCount = 0;
            double? winLossPercent = 0;
            double? winLossRatio = 0;
            double? winTotal = 0;
            int? zeroCount = 0;

            //  get date ("MM/dd/yyyy") portion of end date
            //  compare on each pass with starting date
            //  when date changes (string compare) enter new total into DailyTotal column
            var startingDate = source.Csv[0].EndTime.Substring(11);

            //  use to get trade end date to be used for comparison
            var currentTradeDate = "";

            //  use as register to total trade P/L values
            //  initialize with first value because starting poing for foreach is line 2
            double runningTotal = (double)source.Csv[0].PercentReturn;

            //  need to keep track of line number in list
            int iD = -1;
            #endregion Local variables

            //  Need to assign values to all fields to keep compiler from complaning about need an assignment for nullable variable 
            foreach (var winLoss in source.Csv)
            {
                //  Keep track of line in list
                iD++;
                //  get date of trade ("/MM/dd/yyy")
                currentTradeDate = winLoss.EndTime.Substring(11);

                //  has date changed - value less than zero is change
                //  Date doesn't change - fill sourve.Csv values
                //  Date changes - jump to else if and 
                if (currentTradeDate.CompareTo(startingDate) != 0)
                {
                    //  Fill in summary region for days trades
                    source.Csv[iD - 1].WinTot = winTotal;
                    source.Csv[iD - 1].LossTot = lossTotal;
                    source.Csv[iD - 1].WinCount = winCount;
                    source.Csv[iD - 1].LossCount = lossCount;
                    source.Csv[iD - 1].ZeroCount = zeroCount;
                    source.Csv[iD - 1].Count = winCount + lossCount + zeroCount;
                    winLossPercent = (double)winCount / (winCount + lossCount);
                    winLossPercent = Math.Round((double)winLossPercent, 4);
                    source.Csv[iD - 1].WinLossPercent = winLossPercent * 100;

                    //  Calculate average win
                    //  Consider no wins
                    if (winCount != 0)
                    {
                        winLoss.AvgWin = null;
                        avgWin = Math.Round((Double)(winTotal / winCount), 2);
                        source.Csv[iD - 1].AvgWin = avgWin;
                    }
                    else
                    {
                        winLoss.AvgWin = null;
                        avgWin = 0;
                        source.Csv[iD - 1].AvgWin = avgWin;
                    }

                    //  Calculate average loss
                    //  Consider no losses
                    if (lossCount != 0)
                    {
                        winLoss.AvgLoss = null;
                        avgLoss = Math.Round((Double)(lossTotal / lossCount), 2);
                        source.Csv[iD - 1].AvgLoss = avgLoss;
                    }
                    else
                    {
                        winLoss.AvgLoss = null;
                        avgLoss = 0;
                        source.Csv[iD - 1].AvgWin = avgLoss;
                    }

                    //  Calculate Win/Loss ratio
                    //  Consider no Losses
                    if (avgLoss != 0)
                    {
                        winLoss.WinLossRatio = null;
                        winLossRatio = Math.Round((Double)(avgWin / -avgLoss), 2);
                        source.Csv[iD - 1].WinLossRatio = winLossRatio;
                    }
                    else
                    {
                        winLoss.WinLossRatio = null;
                        winLossRatio = 100;
                        source.Csv[iD - 1].WinLossRatio = winLossRatio;
                    }

                    //  Zero registers
                    winTotal = 0;
                    winCount = 0;
                    lossTotal = 0;
                    lossCount = 0;
                    zeroCount = 0;
                    winLossPercent = 0;

                    //  Replace starting date with new date
                    startingDate = currentTradeDate;
                }

                //  winLoss.xxx is actual slot in .Csv file 
                //  need to fill in the row above the date change (source.Csv[iD-1].xxx)
                //      and null the slots until the next date change
                //  date has changed    
                if (winLoss.P_L > 0)
                {
                    winLoss.Win = winLoss.P_L;
                    winTotal += winLoss.P_L;
                    winLoss.Zero = winLoss.Zero;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;
                    winLoss.Count = null;
                    winLoss.WinLossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    winCount += 1;
                }
                else if (winLoss.P_L < 0)
                {
                    winLoss.Loss = winLoss.P_L;
                    lossTotal += winLoss.P_L;
                    winLoss.Zero = winLoss.Zero;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;
                    winLoss.Count = null;
                    winLoss.WinLossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    lossCount += 1;

                }
                else if (winLoss.P_L == 0)
                {
                    winLoss.Zero = 0;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;
                    winLoss.Count = null;
                    winLoss.WinLossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    zeroCount += 1;
                }

                //  if at end of list - enter last total
                if (iD + 1 == source.Csv.Count)
                {
                    //source.Csv[iD - 1].WinTot = winTotal;
                    source.Csv[iD].WinTot = winTotal;
                    source.Csv[iD].LossTot = lossTotal;
                    source.Csv[iD].WinCount = winCount;
                    source.Csv[iD].LossCount = lossCount;
                    source.Csv[iD].ZeroCount = zeroCount;
                    source.Csv[iD].Count = winCount + lossCount + zeroCount;
                    winLossPercent = (double)winCount / (winCount + lossCount);
                    winLossPercent = Math.Round((double)winLossPercent, 4);
                    source.Csv[iD].WinLossPercent = winLossPercent * 100;

                    //  Consider no wins
                    if (winCount != 0)
                    {
                        winLoss.AvgWin = null;
                        avgWin = Math.Round((Double)(winTotal / winCount), 2);
                        source.Csv[iD].AvgWin = avgWin;
                    }
                    else
                    {
                        winLoss.AvgWin = null;
                        avgWin = 0;
                        source.Csv[iD].AvgWin = avgWin;
                    }

                    //  Consider no losses
                    if (lossCount != 0)
                    {
                        winLoss.AvgLoss = null;
                        avgLoss = Math.Round((Double)(lossTotal / lossCount), 2);
                        source.Csv[iD].AvgLoss = avgLoss;
                    }
                    else
                    {
                        winLoss.AvgLoss = null;
                        avgLoss = 0;
                        source.Csv[iD].AvgWin = avgLoss;
                    }

                    //  Consider no Losses
                    if (avgLoss != 0)
                    {
                        winLoss.WinLossRatio = null;
                        winLossRatio = Math.Round((Double)(avgWin / -avgLoss), 2);
                        source.Csv[iD].WinLossRatio = winLossRatio;
                    }
                    else
                    {
                        winLoss.WinLossRatio = null;
                        winLossRatio = 100;
                        source.Csv[iD].WinLossRatio = winLossRatio;
                    }

                }

            }
            return source;
        }
        #endregion FillWinLossColumn

        #region FillWinLossSummary
        public static Source FillWinLossSummary(this Source source)
        {
            #region Variables

            //  Keep track of position in .Csv list
            //  At end fill in total values
            double? pAvgWin = 0;
            double? pAvgLoss = 0;
            double? pLossTot = 0;
            int? pLossCount = 0;
            int? pWinCount = 0;
            double? pWinLossPercent = 0;
            double? pWinLossRatio = 0;
            double? pWinTot = 0;
            int? pZeroCount = 0;
            int? pCount = 0;

            //  need to keep track of line number in list
            int iD = 0;

            #endregion Variables

            #region Sum wins/Losses

            foreach (var winLoss in source.Csv)
            {
                //  Keep track of line in list
                iD++;

                //  Need to check for null because null will not add/subtract
                //  Sum wins
                if (winLoss.Win != null)
                {
                    pWinTot += winLoss.Win;
                }
                //  Sum losses
                if (winLoss.Loss != null)
                {
                    pLossTot += winLoss.Loss;
                }
                //  Sum win count
                if (winLoss.WinCount != null)
                {
                    pWinCount += winLoss.WinCount;
                }
                //  Sum loss count
                if (winLoss.LossCount != null)
                {
                    pLossCount += winLoss.LossCount;
                }
                //  Sum zero count
                if (winLoss.ZeroCount != null)
                {
                    pZeroCount += winLoss.ZeroCount;
                }
                //  Sum total count
                if (winLoss.Count != null)
                {
                    pCount += winLoss.Count;
                }
            }

            //  Fill in course.Csv slots with calculated values
            source.Csv[iD - 1].PwinTot = pWinTot;
            source.Csv[iD - 1].PlossTot = pLossTot;
            source.Csv[iD - 1].PwinCount = pWinCount;
            source.Csv[iD - 1].PwinCount = pWinCount;
            source.Csv[iD - 1].PlossCount = pLossCount;
            source.Csv[iD - 1].PzeroCount = pZeroCount;
            source.Csv[iD - 1].Pcount = pCount;
            pWinLossPercent = (double)pWinCount / (pWinCount + pLossCount);
            pWinLossPercent = Math.Round((double)pWinLossPercent, 4);
            source.Csv[iD - 1].PwinLossPercent = pWinLossPercent;

            //  Calculate average win
            //  Consider no wins
            if (pWinCount != 0)
            {
                pAvgWin = Math.Round((Double)(pWinTot / pWinCount), 2);
                source.Csv[iD - 1].PavgWin = pAvgWin;
            }
            else
            {
                pAvgWin = 0;
                source.Csv[iD - 1].PavgWin = pAvgWin;
            }

            //  Calculate average loss
            //  Consider no losses
            if (pLossCount != 0)
            {
                pAvgLoss = Math.Round((Double)(pLossTot / pLossCount), 2);
                source.Csv[iD - 1].PavgLoss = pAvgLoss;
            }
            else
            {
                pAvgLoss = 0;
                source.Csv[iD - 1].PavgLoss = pAvgLoss;
            }

            //  Calculate Win/Loss ratio
            //  Consider no Losses
            if (pAvgLoss != 0)
            {
                pWinLossRatio = Math.Round((Double)(pAvgWin / -pAvgLoss), 2);
                source.Csv[iD - 1].PwinLossRatio = pWinLossRatio;
            }
            else
            {
                pWinLossRatio = 100;
                source.Csv[iD - 1].PwinLossRatio = pWinLossRatio;
            }
            #endregion Sum wins/Losses

            return source;
        }
        #endregion FillWinLossSummary

        #region FillWinLossColumn
        //  Extract P/L from source.Csv.P_L and place in Win/Loss/Zero columns
        public static Source FillWinLossColumn(this Source source)
        {
            //  Keep track of position in .Csv list
            //  At end fill in total values
            int lineCount = 0;
            double? winTotal = 0;
            int? winCount = 0;
            double? lossTotal = 0;
            int? lossCount = 0;
            int? zeroCount = 0;
            double? a = 0;

            //  Need to assign values to all fields to keep compiler from  complaning about need an assignment for nullable variable 
            foreach (var winLoss in source.Csv)
            {
                if (winLoss.P_L > 0)
                {
                    winLoss.Win = winLoss.P_L;
                    winTotal += winLoss.P_L;
                    winLoss.Zero = winLoss.Zero;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;   
                    winLoss.Count = null;
                    winLoss.Win_LossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    winCount += 1;
                }
                else if (winLoss.P_L < 0)
                {  
                    winLoss.Loss = winLoss.P_L;
                    lossTotal += winLoss.P_L;
                    winLoss.Zero = winLoss.Zero;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;
                    winLoss.Count = null;
                    winLoss.Win_LossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    lossCount += 1;

                }
                else if (winLoss.P_L == 0)
                {
                    winLoss.Zero = 0;
                    winLoss.WinTot = null;
                    winLoss.LossTot = null;
                    winLoss.WinCount = null;
                    winLoss.LossCount = null;
                    winLoss.ZeroCount = null;
                    winLoss.Count = null;
                    winLoss.Win_LossPercent = null;
                    winLoss.AvgWin = null;
                    winLoss.AvgLoss = null;
                    winLoss.WinLossRatio = null;
                    zeroCount += 1;
                }
                lineCount++;

                //  At last line in .Csv?
                if (lineCount == source.Csv.Count)
                {
                    winLoss.WinTot = winTotal;
                    winLoss.WinCount = winCount;
                    winLoss.LossTot = lossTotal;
                    winLoss.LossCount = lossCount;
                    winLoss.ZeroCount = zeroCount;
                    winLoss.Count = winCount + lossCount + zeroCount;
                    a = (double)winCount / (winCount + lossCount);
                    a = Math.Round((double)a, 4);
                    winLoss.Win_LossPercent = a * 100;
                    
                    //  Consider no wins
                    if (winLoss.WinCount != 0)
                    {
                        winLoss.AvgWin = Math.Round((Double)(winTotal / winCount), 2);
                    }
                    else
                    {
                        winLoss.AvgWin = 0;
                    }

                    //  Consider no losses
                    if (winLoss.LossCount != 0)
                    {
                        winLoss.AvgLoss = Math.Round((Double)(lossTotal / lossCount), 2);
                    }
                    else
                    {
                        winLoss.AvgLoss = 0;
                    }
                    
                    //  Consider no Losses
                    if ( winLoss.AvgLoss != 0)
                    {
                        winLoss.WinLossRatio = Math.Round((Double)(winLoss.AvgWin / -winLoss.AvgLoss), 2);
                    }
                    else
                    {
                        winLoss.WinLossRatio = 100;
                    }
                    //winLoss.AvgLoss = Math.Round((Double)(lossTotal / lossCount),2);
                    //winLoss.WinLossRatio = Math.Round((Double)(winLoss.AvgWin / -winLoss.AvgLoss),2);

                    //  Math.Round((Double)pl.P_L, 2);
                }
            }
            return source;
        } 
        #endregion FillWinLossColumn

        #region GetActiveEntry - Finds applicable entry in Trades 
        //	On first pass ActiveEntry numbers have been set in Main()
        //	Get starting entry price row and values
        //	Start at first entry above exit and search for row that has entry = true and matched = false
        //	Record starting Id
        public static Source GetActiveEntry(this Source source)                                           //	GetActiveEntry
        {
            //Console.WriteLine("\nGetActiveEntry() Called by " + memberName + " () at line " + LineNumber + " / " + LN());
            //if (source.ActiveEntryRemaining == 0)
            //{
            var s = source.Trades[0].Id;
            //	start is first row above exit row
            //  will throw exception if trade start is proir to selected date in indidator setup
            int start = source.Trades[source.rowInTrades - 1].Id;
            for (int i = source.Trades[source.rowInTrades - 1].Id; i >= 0; i--)
            {

                int filler = 0;
                if (source.Trades[i].IsEntry == true && source.Trades[i].Matched == false)
                {
                    source.ActiveEntryId = source.Trades[i].Id;                                         //	GetActiveEntry
                    source.ActiveEntryPrice = source.Trades[i].Price;                                   //	GetActiveEntry	
                    source.ActiveEntryRemaining = source.Trades[i].Qty;                                   //	GetActiveEntry	
                                                                                                          //	2022 09 18  Problems here - on first pass from Main() source.ActiveEntryRemaining has been set to t.Qty
                                                                                                          //	Line above has been added but will probably not work on subsequent passes!
                                                                                                          //if (source.ActiveEntryRemaining == 0)
                                                                                                          //{
                                                                                                          //	source.ActiveEntryRemaining = source.Trades[i].Qty;                                 //	GetActiveEntry
                                                                                                          //}

                    break;
                    //return source;
                }
                //}
            }

            //Console.WriteLine("\nGetActiveEntry() Returned to " + memberName + "() at line " + LineNumber + " / " + LN());

            return source;
        }
        #endregion GetActiveEntry - Finds applicable entry in Trades 

        #region MatchAndAddToCsv
        //	Called from Fill
        //	Sets Matched to true for Exit and Entry in source.Trades
        //	Adds row to csv
        //	csv will not be in correct order to sort before exit
        public static Source MatchAndAddToCsv(this Source source)                                           //	MatchndAddToCsv
        {
            //Console.WriteLine("\nMatchAndAddToCsv() Called by " + memberName + " () at line " + LineNumber + " / " + LN());


            //	Do not set Matched on exit row until remaining == 0
            //	2022 09 03 0800 
            //	When ActiveEntryId changes that entry has been filled
            // 	 
            //if (source.PriorActiveEntryId != source.ActiveEntryId)											//	MatchndAddToCsv
            //{
            //	source.Trades[source.PriorActiveEntryId].Matched = true;									//	MatchndAddToCsv
            //}

            if (source.ActiveEntryRemaining == 0)                                                                       //	MatchndAddToCsv
            {
                source.Trades[source.ActiveEntryId].Matched = true;                                         //	MatchndAddToCsv
            }

            //// Set last row .Matched to true on last pass
            //if ( source.Trades.Count() - 1 == source.rowInTrades )											//	MatchndAddToCsv
            //{
            //	source.Trades[source.rowInTrades].Matched = true;											//	MatchndAddToCsv
            //}

            // Set last row .Matched to true on last pass
            if (source.Remaining == 0)                                            //	MatchndAddToCsv
            {
                //foreach (var t in source.Trades) 
                //{
                //	t.Matched = true;
                //}
                source.Trades[source.rowInTrades].Matched = true;                                           //	MatchndAddToCsv
            }

            // 	When Position == 0 all trades are matched
            //	When Position == 0 and source.Remaining == 0 set all .Matched to true
            if (source.Trades[source.rowInTrades].Position == 0 && source.Remaining == 0)                                            //	MatchndAddToCsv
                                                                                                                                     //	MatchndAddToCsv
            {
                for (int i = source.rowInTrades; i >= 0; i--)
                {
                    source.Trades[i].Matched = true;
                }

            }



            //ln.Dump("In MatchndAddToCsv()   " + LineNumber);                                                //	MatchndAddToCsv
            //Console.WriteLine($"\nsource.ExitQty = {source.ExitQty}");                                    //	MatchndAddToCsv

            //	Add line to csv
            CSV csv = new CSV()                                                                             //	MatchndAddToCsv
            {
                EntryId = source.ActiveEntryId,                                                             //	MatchndAddToCsv
                FilledBy = source.rowInTrades,                                                              //	MatchndAddToCsv
                Entry = source.ActiveEntryPrice,                                                            //	MatchAndAddToCsv
                Qty = source.ExitQty,                                                                       //	MatchndAddToCsv
                RemainingExits = source.Remaining,                                                          //	MatchndAddToCsv
                Exit = source.StartingExitPrice,

            };
            source.Csv.Add(csv);                                                                            //	MatchndAddToCsv
                                                                                                            //	Update RowInCsv
            /// 2022 11 25 1325 Commented out to check for need																										//	Record row in CSV
            //source.RowInCsv++;                                                                              //	MatchndAddToCsv
            //	source.ExitQty is updated remainng quantity to match is quantity matched 
            ///	Try commenting this out
            //source.ExitQty = source.ExitQty - source.Remaining;

            //Console.WriteLine("\nMatchndAddToCsv() Returned to " + memberName + "() at line " + LineNumber + " / " + LN());

            return source;                                                                                  //	MatchndAddToCsv
        }
        #endregion MatchAndAddToCsv

        #region UpdateActiveEntry
        //	Subtracts qty of exits from first entry that is not filled
        //	To get next open entry after source.ActiveEntryRemaining == 0 work down from top to find first .Matched == false
        //	All exits will be matched
        public static Source UpdateActiveEntry(this Source source)                                            //	UpdateAtiveEntry
        {
            //Console.WriteLine("\nUpdateActiveEntry() Called by " + memberName + " () at line " + LineNumber + " / " + LN());
            //Console.WriteLine($"\nsource.Remaining = {source.Remaining}" + " at line " + LN());                                //	UpdateAtiveEntry
            //Console.WriteLine($"\nsource.ExitQty = {source.ExitQty}" + " at line " + LN());                                	//	UpdateAtiveEntry

            //	Subtract qty of exits from source.ActiveEntryRemaining
            //	When source.ActiiveEntryRemaining == 0 find next open entry
            //	Initialized in Main() on first foreach()
            //		source.ActiveEntryRemaining = t.Qty
            if (source.ActiveEntryRemaining == 0)                                                           //	UpdateAtiveEntry		
            {
                //source.PriorActiveEntryId = source.ActiveEntryId;											//	UpdateAtiveEntry
                //for (int i = 0; i < source.rowInTrades; i++)
                for (int i = source.rowInTrades - 1; i >= 0; i--)                                       //	UpdateAtiveEntry

                {
                    // 	Start at top of trades and work down to first unmatched row
                    //	Doesn't work for FIFO if multiple entries and then close - will get first not last
                    if (source.Trades[i].Matched == false)
                    {
                        source.ActiveEntryId = i;                                                           //	UpdateAtiveEntry
                        source.ActiveEntryRemaining = source.Trades[i].Qty;                                 //	UpdateAtiveEntry
                        source.ActiveEntryPrice = source.Trades[i].Price;                                   //	UpdateActiveEntry
                        break;                                                                              //	UpdateAtiveEntry
                    }
                }
            }
            //else
            //{
            //	source.PriorActiveEntryId = source.ActiveEntryId;											//	UpdateAtiveEntry
            //}

            //	Moved from Fill()
            #region Get ActiveEntry... calculation to 'UpdateActiveEntry'
            //	Moved to GetActiveEntry
            //	Get starting entry price row and values
            //	Start at first entry above exit and search for row that has entry = true and matched = false
            //	Record starting Id
            //int start = source.Trades[source.rowInTrades - 1].Id;
            //for (int i = source.Trades[source.rowInTrades - 1].Id; i >= 0; i--)                         //	UpdateActiveEntry
            //{
            //	
            //	int filler = 0;                                                                         //	UpdateActiveEntry								
            //	if (source.Trades[i].Entry == true && source.Trades[i].Matched == false)                //	UpdateActiveEntry
            //	{
            //		source.ActiveEntryId = source.Trades[i].Id;                                         //	UpdateActiveEntry
            //		source.ActiveEntryPrice = source.Trades[i].Price;                                 	//	UpdateActiveEntry	
            //		source.ActiveEntryRemaining = source.Trades[i].Qty;                                 //	UpdateActiveEntry
            //		break;
            //	}
            //}
            #endregion Get ActiveEntry... calculation to 'UpdateActiveEntry'

            #region Set source.ExitQty
            //	Set source.ExitQty and pass to MatchAndAddToCsv()
            //	This should be after next block because source.ActiveEntryRemaining will be changed
            if (source.Remaining >= source.ActiveEntryRemaining)

            {
                source.ExitQty = source.ActiveEntryRemaining;                                               //	UpdateActiveEntry
            }
            else
            {
                source.ExitQty = source.Remaining;                                                          //	UpdateActiveEntry
            }
            #endregion Set source.ExitQty

            source.ActiveEntryRemaining = source.ActiveEntryRemaining - source.ExitQty;                     //	UpdateAtiveEntry		


            //Console.WriteLine($"source.ActiveEntryId = {source.ActiveEntryId}");              				//	UpdateAtiveEntry
            //Console.WriteLine($"source.ActiveEntryRemaining = {source.ActiveEntryRemaining}");              //	UpdateAtiveEntry
            //Console.WriteLine($"source.rowInTrades = {source.rowInTrades}");                                //	UpdateAtiveEntry
            source.Remaining = source.Remaining - source.ExitQty;                                           // 	UpdateAtiveEntry

            //Console.WriteLine($"\nsource.Remaining = {source.Remaining}" + " at line " + LN());                                    //	UpdateAtiveEntry
            //Console.WriteLine($"\nsource.ExitQty = {source.ExitQty}" + " at line " + LN());                                        //	UpdateAtiveEntry

            //source.Csv.Dump("Csv");
            //Console.WriteLine("Csv");
            //Console.WriteLine("\nUpdateAtiveEntry() Returned to " + memberName + " () at line " + LineNumber + " / " + LN());

            return source;                                                                                  //	UpdateAtiveEntry

        }
        #endregion UpdateActiveEntry

        //public static void Print(this Dummy dummy)
        //{
        //    //        Console.WriteLine("Hello from Extension");
        //    NinjaTrader.Code.Output.Reset(PrintTo.OutputTab1);
        //    NinjaTrader.Code.Output.Process("Hello from Printer", PrintTo.OutputTab1);

        //}
    }
}
