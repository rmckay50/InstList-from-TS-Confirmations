using System;
using System.Collections.Generic;
using System.Linq;
using static WindowsFormsApp1.Classes;
using MoreLinq;
using static WindowsFormsApp1.Enums;
using static WindowsFormsApp1.Variables;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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
                            Ptotal = csv.Ptotal,
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
        //	Called from 'Check for normal exit (Entry == false - Exit == true)' and reverses (involves an exit)
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
            source.Remaining = source.Trades[source.RowInTrades].Qty;

            //	Save exit price - it is used for all entry matches
            source.StartingExitPrice = source.Trades[source.RowInTrades].Price;

            while (source.Remaining > 0)
            {

                source.UpdateActiveEntry();
                source.MatchAndAddToCsv();

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
                source.RowInTrades--;
            }
            //Console.WriteLine("\nFill() Returned to " + memberName + "() at line " + LineNumber + " / " + LN());

            return source;
        }
        #endregion Fill        

        #region FillDailyPercentColumn

        public static Source FillDailyPercentColumn(this Source source)
        {
            //  Fills in DailyPercentColumn only
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
                //  Last line will be mostly null for multiple symbols
                //  Check for null and go to last section for data entry code
                if (c.EndTime == null)
                {
                    break;
                }
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
            }

                //  if ID  == list.count - at end of list - enter last total
                //if (iD == source.Csv.Count)
                //{
                    source.Csv[iD - 1].DailyPercentTotal = runningTotal;

                    //  enter number of trades in TotalTrades
                    source.Csv[iD - 1].TotalTrades = TotalTrades;

                //}

            return source;
        }


        #endregion FillDailyPercentColumn

        #region FillDailyTotalColumn
        //  for source
        public static Source FillDailyTotalColumn(this Source source)
        {
            //  Fills in DailyDollarTotal and TotalTrades only
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

                //  Last line will be mostly null for multiple symbols
                //  Check for null and go to last section for data entry code
                if (c.EndTime == null)
                {
                    break;
                }

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
            }
                    source.Csv[iD - 1].DailyDollarTotal = runningTotal;

                    //  enter number of trades in TotalTrades
                    source.Csv[iD - 1].TotalTrades = TotalTrades;

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
            //  Source.Trades is file being changed
            //	Fill() Called by Main () at line 449 / 522
            //  Console.WriteLine("\nFillLongShortColumnInTradesList() Called by " + memberName + " () at line " + LineNumber + " / " + LN());
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

            return source;                                                                                  //	Fill
        }
        #endregion FillLongShortColumnInTradesList

        #region Fill in workingTrades P/L column using Source
        //  Fills in P/L column
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
                            //pl.P_L = (double)pl.Qty * Math.Round((Double)pl.P_L, 2);
							pl.P_L = (double)pl.Qty * (Double)pl.P_L;

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
							//pl.P_L = (double)pl.Qty * Math.Round((Double)pl.P_L, 2);
							pl.P_L = (double)pl.Qty * (Double)pl.P_L;

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
        //  Fills in PercentReturn column
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
                    if (source.Name == "ZN")
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
        //  Changes source.Csv 
        //  Fills in last line with sums from CSV.Win and CSV.Loss columns
        //  If multiple symbols each different symbols has summary values calculated
        //  Creates lastRow list - summary figures that will be overwritten

        public static Source FillDailyWinLossColumn(this Source source)
        {
            #region Local variables
            //  Uses sourse.Csv
            //  File has Id, FilledBy, StartTimeTicks, StartTime, Entry, Endtime.Ticks,
            //      Exit, Long_Short, P_L, Qty, P_LDivdedByQty, PercentReturn filled in
            //  This extension fills in Win, Loss WinTot, LossTot WinCount, LossCount, ZeroCount, Count,
            //      WinlossPercent, AvgWin, AvgLoss, WinLossRatio
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
            //  initialize with first value because starting point for foreach is line 2
            double runningTotal = (double)source.Csv[0].PercentReturn;

            //  need to keep track of line number in list
            int iD = -1;
            #endregion Local variables

            //  Order list.  If not done now the summary line may no be on last line
            source.Csv = source.Csv.OrderBy(i => i.Name).ThenBy(i => i.StartTimeTicks).ToList();
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
                    if ( winCount != 0 )
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
                    //  If file is from TS website and has multiple symbols
                    //      place summary on line below last entry (ID++)
                    //if (Variables.fileSource == FileSource.TSWebsite && nameCount.Count() > 1)
                    //{
                    //    iD++;
                    //}
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
                        source.Csv[iD].AvgLoss = avgLoss;
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
            //  create list with Name, Count (number of trades/symbol)
            //groupName = workingCsv.GroupBy(i => i.Name)
            //    .Select(j => new
            //    {
            //        Name = j.Key,
            //        Count = j.Count()
            //    })
            //    .OrderBy(i => i.Name).ToList();

            #region Use Linq to Fill In Values - Find Number of Symbols and Trades for Each Symbol
            //  GroupBy list is created in an if() statement and contenets need to be copied to a 
            //      non-local list 'nameCount' List<NameCount> which is a class with Name and Count of names
            //      Located in 'WindowsFormsApp1.Classes.NameCount'
            List<nameCountP_L> nameCountP_L = new List<nameCountP_L>();

            //  Make a copy of source.Csv to work with
            List<CSV> workingCsv = new List<CSV>();

            //  Make copy of source.Csv
            if (Variables.fileSource == FileSource.TSWebsite)
            {
                //  Make copy of source.Csv. source.Csv no has Win / Loss in columns and day summary at bottom
                foreach (var v in source.Csv)
                {
                    workingCsv.Add(v);
                }
                workingCsv = workingCsv.OrderBy(I => I.Name).ThenBy (i => i.StartTimeTicks).ToList();
                //  Order workingCsv by symbol Name and fill in Win / Loss totals and calculated results
                //var workingCsvOrdered = workingCsv.GroupBy(p => p.Name)
                //    .Select(e => new
                //    {
                //        Name = e.Key,
                //        WinTotal = e.Sum(k => k.Win),
                //        LossTotal = e.Sum(k => k.Loss),
                //        WinCount = (decimal?)e.Where(p => p.Win != null).Select(p => p.Win).Count(),
                //        LossCount = (decimal?)e.Where(p => p.Loss != null).Select(p => p.Loss).Count(),
                //        TotalCount = e.Where(p => p.Win != null).Select(p => p.Win).Count() + e.Where(p => p.Loss != null).Select(p => p.Loss).Count(),
                //        WinLossPercent = (decimal)e.Where(p => p.Win != null).Select(p => p.Win).Count()
                //            / (e.Where(p => p.Loss != null).Select(p => p.Loss).Count() + e.Where(p => p.Win != null).Select(p => p.Win).Count()),
                //        AvgWin = e.Sum(k => k.Win) / e.Where(p => p.Win != 0).Select(p => p.Win).Count(),
                //        AvgLoss = e.Sum(k => k.Loss) / e.Where(p => p.Loss != 0).Select(p => p.Loss).Count(),
                //    })
                //    .OrderBy(i => i.Name)
                //    .ToList(); //.OrderBy(i => i.Name).ThenBy(i => i.StartTimeTicks);
                multipleSymbols = workingCsv.GroupBy(i => i.Name)
                .Select(e =>
                new MultipleSymbols
                {
                    Name = e.Key,
                    WinTotal = (decimal?)e.Sum(k => k.Win),
                    LossTotal = (decimal?)e.Sum(k => k.Loss),
                    WinCount = e.Where(p => p.Win != null).Select(p => p.Win).Count(),
                    LossCount = e.Where(p => p.Loss != null).Select(p => p.Loss).Count(),
                    TotalCount = e.Where(p => p.Win != null).Select(p => p.Win).Count() + e.Where(p => p.Loss != null).Select(p => p.Loss).Count(),
                    WinLossPercent = (decimal)e.Where(p => p.Win != null).Select(p => p.Win).Count()
                    / (e.Where(p => p.Loss != null).Select(p => p.Loss).Count() + e.Where(p => p.Win != null).Select(p => p.Win).Count()),
                })
                .OrderBy(e => e.Name)
                .ToList();

                //  Caculate win / loss count and win / loss ration
                foreach (var x in multipleSymbols)
                {
                    if (x.WinCount != 0)
                    {
                        x.AvgWin = x.WinTotal / x.WinCount;
                    }
                    else if (x.WinCount == 0)
                    {
                        x.AvgWin = 0;
                    }


                    if (x.LossCount != 0)
                    {
                        x.AvgLoss = (decimal?)Math.Round((double)(x.LossTotal / x.LossCount), 2);
                    }
                    else if (x.LossCount == 0)
                    {
                        x.AvgLoss = 0;
                    }

                    //if ( x.WinCount)

                    if (x.AvgLoss != 0)
                    {
                        x.WinLossRatio = x.AvgWin / -x.AvgLoss;
                    }
                    else if (x.AvgLoss == 0)
                    {
                        x.WinLossRatio = 100;
                    }
                }

                // Check how many symbols.  If more than one need to fill in summary for each different symbol
                // Step through lists from bottom to get result on last line of symbols
                if (multipleSymbols.Count() > 1)
                {
                    var rows = workingCsv.Count() - 1;
                    lastRow.Add(
                        new CSV
                        {
                            WinTot = workingCsv[rows].WinTot,
                            LossTot = workingCsv[rows].LossTot,
                            WinCount = workingCsv[rows].WinCount,
                            LossCount = workingCsv[rows].LossCount,
                            ZeroCount = workingCsv[rows].ZeroCount,
                            Count = workingCsv[rows].Count,
                            WinLossPercent = workingCsv[rows].WinLossPercent,
                            AvgWin = workingCsv[rows].AvgWin,
                            AvgLoss = workingCsv[rows].AvgLoss,
                            WinLossRatio = workingCsv[rows].WinLossRatio,
                        });

                    //PwinTot = workingCsv[rows].PwinTot,
                    //PlossTot = workingCsv[rows].PlossTot,
                    //Ptotal = workingCsv[rows].Ptotal,
                    //PwinCount = workingCsv[rows].PwinCount,
                    //PlossCount = workingCsv[rows].PlossCount,
                    //PzeroCount = workingCsv[rows].PzeroCount,
                    //Pcount = workingCsv[rows].Pcount,
                    //PwinLossPercent = workingCsv[rows].PwinLossPercent,
                    //PavgWin = workingCsv[rows].PavgWin,
                    //PavgLoss = workingCsv[rows].PavgLoss,
                    //PwinLossRatio = workingCsv[rows].PwinLossRatio,


                    multipleSymbols.Reverse();
                    foreach ( var x in multipleSymbols)
                    {
                        foreach ( var y in Enumerable.Reverse(workingCsv))
                        {
                            if (x.Name == y.Name)
                            { 
                                y.WinTot = (double?)x.WinTotal;
                                y.LossTot = (double?)x.LossTotal;
                                y.WinCount = x.WinCount;
                                y.LossCount = x.LossCount;
                                y.ZeroCount = x.ZeroCount;
                                y.Count = x.TotalCount;
                                y.WinLossPercent = (double?)x.WinLossPercent;
                                y.AvgWin = (double?)x.AvgWin;
                                y.AvgLoss = (double?)x.AvgLoss;
                                y.WinLossRatio = (double?)x.WinLossRatio;
                                break;
                            }
                        }

                    }
                    workingCsv.ToList();

                    source.Csv = workingCsv;
                    source.Csv.Add(new CSV
                    {
                        WinTot = lastRow[0].WinTot,
                        LossTot = lastRow[0].LossTot,
                        WinCount = lastRow[0].WinCount,
                        LossCount = lastRow[0].LossCount,
                        ZeroCount = lastRow[0].ZeroCount,
                        Count = lastRow[0].Count,
                        WinLossPercent = lastRow[0].WinLossPercent,
                        AvgWin = lastRow[0].AvgWin,
                        AvgLoss = lastRow[0].AvgLoss,
                        WinLossRatio = lastRow[0].WinLossRatio,

                    });
                }
                //  create list with Name, Count (number of trades/symbol)
                var groupName = workingCsv.GroupBy(i => i.Name)
                    .Select(j => new
                    {
                        Name = j.Key,
                        Count = j.Count(),
                        P_LSum = j.Sum(i => i.P_L)
                    })
                    .OrderBy(i => i.Name).ToList();

                //  Need to copy groupName into another List<T> because it is local to the if statement
                //foreach (var v in groupName)
                //{
                //    nameCountP_L.Add(new nameCountP_L() { Name = v.Name, Count = v.Count, P_LSum = v.P_LSum });
                //}
            }
            #endregion Use Linq to Fill In Values - Find Number of Symbols and Trades for Each Symbol
            //foreach (var v in  nameCount)
            //{
            //    var n = v.Name;
            //    var c = v.Count;
            //}
            //  Number of rows in nameCount is found with nameCount.Count()
            //var x = nameCount.Count();


            return source;
        }
        #endregion FillWinLossColumn

        #region FillWinLossSummary
        //  Fills in far right columns for page summary
        //  Will compile stats for multiple days if available
        public static Source FillWinLossSummary(this Source source)
        {
            #region Variables

            //  Keep track of position in .Csv list
            //  At end fill in total values
            double? pAvgWin = 0;
            double? pAvgLoss = 0;
            int? pCount = 0;
            double? pLossTot = 0;
            int? pLossCount = 0;
            double? pTotal = 0;
            double? pWinLossPercent = 0;
            double? pWinLossRatio = 0;
            int? pWinCount = 0;
            double? pWinTot = 0;
            int? pZeroCount = 0;

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
                if ( winLoss.WinCount != null )
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
            source.Csv[iD - 1].Ptotal = Math.Round((double)(pWinTot + pLossTot), 2);
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

            //  Fill in remaining columns in Variables.lastRow
            //  If more than one symbol in trades data

            if (multipleSymbols.Count() > 1)
            {

                var rows = source.Csv.Count() - 1;

                lastRow[0].PwinTot = source.Csv[rows].PwinTot;
                lastRow[0].PlossTot = source.Csv[rows].PlossTot;
                lastRow[0].Ptotal = source.Csv[rows].Ptotal;
                lastRow[0].PwinCount = source.Csv[rows].PwinCount;
                lastRow[0].PlossCount = source.Csv[rows].PlossCount;
                lastRow[0].PzeroCount = source.Csv[rows].PzeroCount;
                lastRow[0].Pcount = source.Csv[rows].Pcount;
                lastRow[0].PwinLossPercent = source.Csv[rows].PwinLossPercent;
                lastRow[0].PavgWin = source.Csv[rows].PavgWin;
                lastRow[0].PavgLoss = source.Csv[rows].PavgLoss;
                lastRow[0].PwinLossRatio = source.Csv[rows].PwinLossRatio;
            }
            return source;
        }
        #endregion FillWinLossSummary

        #region GetActiveEntry - Finds applicable entry in Trades 
        //	On first pass ActiveEntry numbers have been set in Main()
        //	Get starting entry price row and values
        //	Start at first entry above exit and search for row that has entry = true and matched = false
        //	Record starting Id
        public static Source GetActiveEntry(this Source source)                                           //	GetActiveEntry
        {
            var s = source.Trades[0].Id;
            //	start is first row above exit row
            //  will throw exception if trade start is proir to selected date in indidator setup
            int start = source.Trades[source.rowInTrades - 1].Id;
            for (int i = source.Trades[source.rowInTrades - 1].Id; i >= 0; i--)
            {

                int filler = 0;
                //  Find entry that has not been filled
                //  May be a number of unmatched entries above.
                //  Use first found for LIFO
                if (source.Trades[i].IsEntry == true && source.Trades[i].Matched == false)
                {
                    source.ActiveEntryId = source.Trades[i].Id;
                    source.ActiveEntryPrice = source.Trades[i].Price;
                    source.ActiveEntryRemaining = Math.Abs((long)source.Trades[i].Position);
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

    }
}
