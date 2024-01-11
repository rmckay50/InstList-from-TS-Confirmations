using LINQtoCSV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Classes
    {
        #region Classes 
        public class Confirmation
        {
            public int ID { get; set; }
            public string Long_Short { get; set; }
            public string Symbol { get; set; }
            public string TradeDate { get; set; }
            public string TradeTime { get; set; }
            public long TimeTicks { get; set; }
            public DateTime DTTradeTime { get; set; }
            public double Price { get; set; }
            public int Qty { get; set; }

        }
        public class CSV
        {
            //public int EntryId { get; set; }
            public int EntryId { get; set; }                                                                //	class CSV
            public string Name { get; set; }
            public int FilledBy { get; set; }                                                               //	class CSV
            public long? RemainingExits { get; set; }                                                         //	class CSV
            public long? StartTimeTicks { get; set; }
            public string StartTime { get; set; }
            public double? Entry { get; set; }                                                               //	class CSV
            public long? EndTimeTicks { get; set; }
            public string EndTime { get; set; }
            public double? Exit { get; set; }                                                                //	class CSV
            public string Long_Short { get; set; }
            public double P_L { get; set; }
            public long? Qty { get; set; }                                                                    //	class CSV
            public double? P_LDividedByQty { get; set; }
            public double? PercentReturn { get; set; }
            public double? DailyPercentTotal { get; set; }
            public double? DailyDollarTotal { get; set; }
            public int? TotalTrades { get; set; }


            public IEnumerator GetEnumerator()                                                              //	class CSV
            {
                return (IEnumerator)this;                                                                       //	class CSV
            }
        }

        public class LinesJoined
        {
            //	Concatenate even lines to prior odd
            public string LinePlusLine { get; set; }
            public LinesJoined() { }
            public LinesJoined(string linePlusLine)
            {
                LinePlusLine = linePlusLine;
            }
        }
        public class NTDrawLine
        {
            //DateTime startTime, double startY, DateTime endTime, double endY
            public int Id { get; set; }
            public bool Playback { get; set; }
            public string Symbol { get; set; }
            public string Long_Short { get; set; }
            public long StartTimeTicks { get; set; }
            public string StartTime { get; set; }
            public double StartY { get; set; }
            public long EndTimeTicks { get; set; }
            public string EndTime { get; set; }
            public double EndY { get; set; }
            public double P_L { get; set; }
            public int Qty { get; set; }
            public double? P_LDividedByQty { get; set; }
            public double? Percent { get; set; }
            public double? DailyPercentTotal { get; set; }
            public double? DailyDollarTotal { get; set; }
            public int? TotalTrades { get; set; }

            public NTDrawLine() { }

            public NTDrawLine(int id, bool playback, string symbol, string long_Short, int qty, long startTimeTicks, string startTime, double startY, long endTimeTicks, string endTime, double endY,
                double p_L, double? p_LDividedByQty, double? percent, double? dailyPercentTotal, double? dailyTotal, int? totalTrades)
            {
                Id = id;
                Playback = playback;
                Symbol = symbol;
                Long_Short = long_Short;
                StartTimeTicks = startTimeTicks;
                StartTime = startTime;
                StartY = startY;
                EndTimeTicks = endTimeTicks;
                EndTime = endTime;
                EndY = endY;
                P_L = p_L;
                Qty = qty;
                P_LDividedByQty = p_LDividedByQty;
                Percent = percent;
                DailyPercentTotal = dailyPercentTotal;
                DailyDollarTotal = dailyTotal;
                TotalTrades = totalTrades;
            }

            public IEnumerator GetEnumerator()                                                              //	class CSV
            {
                return (IEnumerator)this;                                                                       //	class CSV
            }

        }
        public class NTDrawLineForLINQtoCSV
        {
            [CsvColumn(FieldIndex = 1)]
            public int Id { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public bool Playback { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Symbol { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public string Long_Short { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public long StartTimeTicks { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public string StartTime { get; set; }
            [CsvColumn(FieldIndex = 7)]
            public double StartY { get; set; }
            [CsvColumn(FieldIndex = 8)]
            public long EndTimeTicks { get; set; }
            [CsvColumn(FieldIndex = 9)]
            public string EndTime { get; set; }
            [CsvColumn(FieldIndex = 10)]
            public double EndY { get; set; }
            [CsvColumn(FieldIndex = 11)]
            public double P_L { get; set; }
            [CsvColumn(FieldIndex = 12)]
            public int Qty { get; set; }
            [CsvColumn(FieldIndex = 13)]
            public double? P_LDividedByQty { get; set; }
            [CsvColumn(FieldIndex = 14)]
            public double? Percent { get; set; }
            [CsvColumn(FieldIndex = 15)]
            public double? DailyPercentTotal { get; set; }
            [CsvColumn(FieldIndex = 16)]
            public double? DailyDollarTotal { get; set; }
            [CsvColumn(FieldIndex = 17)]
            public int? TotalTrades { get; set; }
            public IEnumerator GetEnumerator()
            {
                return (IEnumerator)this;
            }
        }
        public class Ret
        {
            //  InstId is an Id for this class and is filled in later
            public long? InstId { get; set; }         // Created when instList is made
            public long ExecId { get; set; }
            public long Account { get; set; }
            public string Name { get; set; }
            public long? Position { get; set; }
            public long? Quantity { get; set; }
            public bool? IsEntry { get; set; }
            public bool? IsExit { get; set; }
            public double? Price { get; set; }
            public long? Time { get; set; }
            public string HumanTime { get; set; }
            public long Instrument { get; set; }
            public string Expiry { get; set; }
            public double? P_L { get; set; }
            public string Long_Short { get; set; }

            public Ret() { }

            public Ret(long instId, long execId, long account, string name, long? position, long? quantity, bool? isEntryL, bool? isExit, double? price, long? time, string humanTime, long instrument, string expiry, double? p_L, string long_Short)
            {
                InstId = instId;
                ExecId = execId;
                Name = name;
                Account = account;
                Position = position;
                Quantity = quantity;
                IsEntry = isEntryL;
                IsExit = isExit;
                Price = price;
                Time = time;
                HumanTime = humanTime;
                Instrument = instrument;
                Expiry = expiry;
                P_L = p_L;
                Long_Short = long_Short;
            }
        }
        public class Source
        {
            public int ActiveEntryId { get; set; }                                                          //	class source
            public long? ActiveEntryRemaining { get; set; }                                                   //	class source
            public double? ActiveEntryPrice { get; set; }                                                    //	class source
            public bool IsReversal { get; set; }
            public int Margin { get; set; }
            public string InstrumentType { get; set; }
            public string Name { get; set; }
            public long PositionAfterReverse { get; set; }                                                   //	class source
            public long RowOfReverse { get; set; }                                                           //	class source
            public long Position { get; set; }                                                               //	class source
            public double? StartingExitPrice { get; set; }                                                   //	class source
            public int rowInTrades { get; set; }                                                            //	class source
            public int RowInTrades { get; set; }                                                            //	class source
            public long? ExitQty { get; set; }                                                                //	class source
            public long? Remaining { get; set; }                                                              //	class source
            public List<Trade> Trades { get; set; }                                                         //	class source
            public List<CSV> Csv { get; set; }                                                              //	class source
            public List<NTDrawLine> NTDrawLine { get; set; }

        }
        public class Trade
        {
            public int Id { get; set; }
            public long ExecId { get; set; }                                                                     // 	class Trade
            public string Name { get; set; }
            public long? Position { get; set; }                                                                    // 	class Trade
            public long? Qty { get; set; }                                                                    // 	class Trade
            public bool? IsEntry { get; set; }                                                                 // 	class Trade
            public bool? IsExit { get; set; }                                                                  // 	class Trade
            public bool IsRev { get; set; }                                                                 // 	class Trade
            public bool Matched { get; set; }                                                               // 	class Trade
            public double? Price { get; set; }                                                               // 	class Trade
            public long? Time { get; set; }
            public string HumanTime { get; set; }
            public long Instrument { get; set; }
            public string Expiry { get; set; }
            public double? P_L { get; set; }
            public string Long_Short { get; set; }
            public int TradeNo { get; set; }

            public IEnumerator GetEnumerator()
            {
                return (IEnumerator)this;
            }

            public Trade() { }

            //  Without int execId (second cstr, code runs
            //  public Ret(int instId, int execId, string name, int? position, int? quantity, bool? isEntry, bool? isExit, double? price, long? time,
            //	string humanTime, long instrument, string expiry, double? p_L, string long_Short)
            public Trade(long execId, long? position, string name, long? qty, bool? isEntry, bool? isExit, double? price, long? time,
                string humanTime, long instrument, string expiry, double? p_L, string long_Short)
            {
                //InstId = instId;
                ExecId = execId;
                Position = position;
                Name = name;
                Qty = qty;
                IsEntry = isEntry;
                IsExit = isExit;
                Price = price;
                Time = time;
                HumanTime = humanTime;
                Instrument = instrument;
                Expiry = expiry;
                P_L = p_L;
                Long_Short = long_Short;
            }

            public Trade(int id)
            {
                Id = id;
            }
        }
        public class TradeStationCsv
        {
            public int Id { get; set; }
            public string Long_Short { get; set; }
            public string Symbol { get; set; }
            public string Share_Contracts { get; set; }
            public DateTime EntryTime { get; set; }      // 	May work with DateTime + Time + AM/PM
            public decimal EntryPrice { get; set; }
            public DateTime ExitTime { get; set; }
            public decimal ExitPrice { get; set; }
            public decimal P_L { get; set; }
            public decimal Cum_NetProfit { get; set; }
        }
        #endregion Classes 

    }
}
