#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    [Gui.CategoryOrder("Setup", 0)]
    [Gui.CategoryOrder("Strategies", 1)]
    [Gui.CategoryOrder("MT4/5 Settings", 2)]
    [Gui.CategoryOrder("EMA", 3)]
    [Gui.CategoryOrder("Trend Trader Strategy", 4)]
    [Gui.CategoryOrder("Volume Based Coloured Bars", 5)]
    [Gui.CategoryOrder("Parameters", 6)]
    public class FXMATHEUSstrategyv3 : Strategy
    {
        private string longalert;
        private string shortalert;
        private string closelong;
        private string closeshort;

        private EMA ema;    // EMA 200
        private EMA ema2;   // EMA 800
        private FVGv1 fvg;

        private FXMATHEUSindicator ret;
        private double pos, posPrev = 0;

        public bool vold1, vold2, vold3;
        public bool volu1, volu2, volu3;

        private double buy1_sl = 0;
        private double buy2_sl = 0;
        private double buy3_sl = 0;
        private double buy4_sl = 0;
        private double buy5_sl = 0;

        private double sell1_sl = 0;
        private double sell2_sl = 0;
        private double sell3_sl = 0;
        private double sell4_sl = 0;
        private double sell5_sl = 0;

        double longStopPrice = 0.0;
        double longStopPrice2 = 0.0;
        double longStopPrice3 = 0.0;
        double longStopPrice4 = 0.0;
        double longStopPrice5 = 0.0;

        double longStopPrice_prev = 0.0;
        double longStopPrice_prev2 = 0.0;
        double longStopPrice_prev3 = 0.0;
        double longStopPrice_prev4 = 0.0;
        double longStopPrice_prev5 = 0.0;

        double shortStopPrice = 99999999;
        double shortStopPrice2 = 99999999;
        double shortStopPrice3 = 99999999;
        double shortStopPrice4 = 99999999;
        double shortStopPrice5 = 99999999;

        double shortStopPrice_prev = 99999999;
        double shortStopPrice_prev2 = 99999999;
        double shortStopPrice_prev3 = 99999999;
        double shortStopPrice_prev4 = 99999999;
        double shortStopPrice_prev5 = 99999999;

        private int ema200_break_up = 0;
        private int ema200_break_down = 0;

        private bool flagEma200 = false;
        private bool flagEma800 = false;

        private int onStrategy = 0;
        private int position = 0;

        private bool strategy_1;
        private bool strategy_2;
        private bool strategy_3;
        private bool strategy_4;
        private bool strategy_5;

        NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 12);


        /// <FVG>
        private int bar;

        int n = 0;
        int m = 0;
        bool bullmit = false;
        bool bearmit = false;
        private double bullhigh;
        private double bearlow;
        private double prevnearbull;
        private double prevnearbear;

        private List<string> bullhighlinearr = new List<string>();
        private List<string> bulllowlinearr = new List<string>();
        private List<string> bearhighlinearr = new List<string>();
        private List<string> bearlowlinearr = new List<string>();

        private List<DateTime> bullfvghight = new List<DateTime>();
        private List<DateTime> bullfvglowt = new List<DateTime>();
        private List<DateTime> bearfvghight = new List<DateTime>();
        private List<DateTime> bearfvglowt = new List<DateTime>();

        private List<DateTime> bullfvghighttemp = new List<DateTime>();
        private List<DateTime> bullfvglowttemp = new List<DateTime>();
        private List<DateTime> bearfvghighttemp = new List<DateTime>();
        private List<DateTime> bearfvglowttemp = new List<DateTime>();

        private List<double> bullfvghigh = new List<double>();
        private List<double> bullfvglow = new List<double>();
        private List<double> bearfvghigh = new List<double>();
        private List<double> bearfvglow = new List<double>();

        private List<double> bullfvghightemp = new List<double>();
        private List<double> bullfvglowtemp = new List<double>();
        private List<double> bearfvghightemp = new List<double>();
        private List<double> bearfvglowtemp = new List<double>();

        string mit = "no mitigation";
		int PrevBar = -1;
        /// </FVG>
        protected int ResolutionToSec(string p_res = "")
        {
            string res = p_res;
            int multi = 1;
            switch (BarsPeriod.BarsPeriodType)
            {
                case BarsPeriodType.Second:
                    multi = 1;
                    break;

                case BarsPeriodType.Minute:
                    multi = 60;
                    break;

                case BarsPeriodType.Day:
                    multi = 86400;
                    break;

                case BarsPeriodType.Week:
                    multi = 604800;
                    break;

                default:
                    multi = 1;
                    break;
            }

            int mins;
            mins = res == "1" ? 1 :
            res == "2" ? 2 :
            res == "3" ? 3 :
            res == "4" ? 4 :
            res == "5" ? 5 :
            res == "10" ? 10 :
            res == "15" ? 15 :
            res == "30" ? 30 :
            res == "60" ? 60 :
            res == "" ? BarsPeriod.Value : 1;

            int ms = multi * mins * 1000;
            return ms;
        }
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "FXMATHEUSstrategyv3";
                Calculate = Calculate.OnPriceChange;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = false;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 30;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                // Parameters for MT4/5 Settings
                pc_risk = 2;        // default value : 2 , step : 0.1 , min value : 0
                pc_id = "";         // defval : "" , 
                pc_prefix = "";     // defval : ""

                // Parameters for EMA
                ema_len = 200;
                ema2_len = 800;

                // Parameters for Trend Trader
                Length = 21;
                Multiplier = 3;

                // Volume Based Colored Bars
                smaLength = 21;

                // Strategies
                strategy_1 = true;
                strategy_2 = true;
                strategy_3 = true;
                strategy_4 = true;
                strategy_5 = true;

                // Parameters
                longTrailPerc = 1;
                quantitySize = 2;

                // FVG
                Bullfvg = Brushes.Teal;
                Bearfvg = Brushes.Maroon;
                Lookback = 50;
                Bprs = true;


                // Parameters for Alert
                double usef = pc_risk;
                var symbol = pc_prefix;
                longalert = pc_id + ",buy," + symbol + ",risk=" + usef.ToString("N2") + "";
                shortalert = pc_id + ",sell," + symbol + ",risk=" + usef.ToString("N2") + "";
                closelong = pc_id + ",closelong," + symbol + "";
                closeshort = pc_id + ",closeshort," + symbol + "";
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                // EMA 200
                ema = EMA(Close, ema_len);
                AddChartIndicator(ema);
                ChartIndicators[0].Plots[0].Brush = Brushes.Purple;
                ChartIndicators[0].Plots[0].Width = 3;

                // EMA 800
                ema2 = EMA(Close, ema2_len);
                AddChartIndicator(ema2);
                ChartIndicators[1].Plots[0].Brush = Brushes.Orange;
                ChartIndicators[1].Plots[0].Width = 3;

                // RET
                ret = FXMATHEUSindicator(Length, Multiplier);
                AddChartIndicator(ret);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;
            //Add your custom strategy logic here.
            TrendTraderStrategy();
            VolumeBasedColoredBars();
            if (strategy_1 & (onStrategy == 0 | onStrategy == 1)) Strategy_1();
            if (strategy_2 & (onStrategy == 0 | onStrategy == 2)) Strategy_2();
            if (strategy_3 & (onStrategy == 0 | onStrategy == 3)) Strategy_3();
            if (strategy_4 & (onStrategy == 0 | onStrategy == 4)) Strategy_4();
            if (strategy_5 & (onStrategy == 0 | onStrategy == 5)) Strategy_5();

            // FVG
            if(CurrentBar != PrevBar)
            {
                bar = ResolutionToSec();
                BullFVGCheck();
                BearFVGCheck();
                DrawBullFVG();
                DrawBearFVG();
                PrevBar = CurrentBar;
            }
            // End of FVG
        }
        protected void TrendTraderStrategy()
        {
            pos = Close[0] > ret[0] ? 1 : Close[0] < ret[0] ? -1 : 0;

            if (pos != posPrev & pos == 1)
            {
                Alert("buyAlert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            if (pos != posPrev & pos == -1)
            {
                Alert("sellAlert", Priority.High, "Color changed - Sell", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            posPrev = pos;
        }
        protected void VolumeBasedColoredBars()
        {
            double volume = VOL()[0];
            double avrg = SMA(VOL(), smaLength)[0];

            vold1 = volume > avrg * 1.5 & Close[0] < Open[0];
            vold2 = volume >= avrg * 0.5 & volume <= avrg * 1.5 & Close[0] < Open[0];
            vold3 = volume < avrg * 0.5 & Close[0] < Open[0];

            volu1 = volume > avrg * 1.5 & Close[0] > Open[0];
            volu2 = volume >= avrg * 0.5 & volume <= avrg * 1.5 & Close[0] > Open[0];
            volu3 = volume < avrg * 0.5 & Close[0] > Open[0];

            Brush cold1 = new SolidColorBrush(Color.FromRgb(128, 0, 0));
            Brush cold2 = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            Brush cold3 = Brushes.Orange;
            Brush colu1 = new SolidColorBrush(Color.FromRgb(0, 100, 0));
            Brush colu2 = Brushes.Lime;
            Brush colu3 = new SolidColorBrush(Color.FromRgb(127, 255, 212));
            Brush color_1 = vold1 ? cold1 : vold2 ? cold2 : vold3 ? cold3 : volu1 ? colu1 : volu2 ? colu2 : volu3 ? colu3 : Brushes.White;

            BarBrush = color_1;

        }
        protected void Strategy_1()
        {
            bool buy1 = volu1 & CrossAbove(Close, ret, 1) & Close[0] > ema[0] & Close[0] > ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            bool sell1 = vold1 & CrossBelow(Close, ret, 1) & Close[0] < ema[0] & Close[0] < ema2[0] & Position.MarketPosition == MarketPosition.Flat;

            if (buy1 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
				Print("Buy1");
                EnterLong("Buy1");
                buy1_sl = Low[1];
                longStopPrice_prev = buy1_sl;
                onStrategy = 1;
                position = 1;

                Draw.Text(this, "Buy1" + CurrentBar, false, "BUY-1", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
            }

            if (sell1 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
				Print("Sell1");
                EnterShort("Sell1");
                sell1_sl = High[1];
                shortStopPrice_prev = sell1_sl;
                onStrategy = 1;
                position = -1;

                Draw.Text(this, "Sell1" + CurrentBar, false, "SELL-1", 0, High[0], +30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
            }

            double shortTrailPerc = longTrailPerc; // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

            // Determine trail stop loss prices
            double stopValue;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                stopValue = Close[0] * (1 - longTrailPerc / 100);
                longStopPrice = Math.Max(stopValue, Math.Max(longStopPrice_prev, buy1_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, longStopPrice_prev, 0, longStopPrice, Brushes.LimeGreen, DashStyleHelper.Solid, 2);
                longStopPrice_prev = longStopPrice;
            }


            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                stopValue = Close[0] * (1 + shortTrailPerc / 100);
                shortStopPrice = Math.Min(stopValue, Math.Min(shortStopPrice_prev, sell1_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, shortStopPrice_prev, 0, shortStopPrice, Brushes.Red, DashStyleHelper.Solid, 2);
                shortStopPrice_prev = shortStopPrice;
            }

            double long_sl1 = longStopPrice;
            double short_sl1 = shortStopPrice;

            if (Position.MarketPosition == MarketPosition.Long & Close[0] < long_sl1 & position > 0)
            {
                ExitLong("Buy1");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExBuy1Txt" + CurrentBar, false, "Ex-Buy1 \nStopLoss", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            if (Position.MarketPosition == MarketPosition.Short & Close[0] > short_sl1 & position < 0)
            {
                ExitShort("Sell1");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExSell1Txt" + CurrentBar, false, "Ex-Sell1 \nStopLoss", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }

        }
        protected void Strategy_2()
        {
            bool buy2 = volu1 & CrossAbove(Close, ret, 1) & Close[0] < ema[0] & Close[0] > ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            bool sell2 = vold1 & CrossBelow(Close, ret, 1) & Close[0] > ema[0] & Close[0] < ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            
            if (buy2 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("longalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterLong("Buy2");
                onStrategy = 2;
                position = 1;
                buy2_sl = High[1];
                longStopPrice_prev2 = buy2_sl;
                Draw.Text(this, "Buy2" + CurrentBar, false, "BUY-2", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
            }

            if (sell2 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("shortalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterShort("Sell2");
                onStrategy = 2;
                position = -1;
                sell2_sl = High[1];
                shortStopPrice_prev2 = sell2_sl;
                Draw.Text(this, "Sell2" + CurrentBar, false, "SELL-2", 0, High[0], +30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
            }

            double shortTrailPerc = longTrailPerc; // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
            double stopValue;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                stopValue = Close[0] * (1 - longTrailPerc / 100);
                longStopPrice2 = Math.Max(stopValue, Math.Max(longStopPrice_prev2, buy2_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, longStopPrice_prev2, 0, longStopPrice2, Brushes.LimeGreen, DashStyleHelper.Solid, 2);
                longStopPrice_prev2 = longStopPrice2;
            }

            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                stopValue = Close[0] * (1 + shortTrailPerc / 100);
                shortStopPrice2 = Math.Min(stopValue, Math.Min(shortStopPrice_prev2, sell2_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, shortStopPrice_prev2, 0, shortStopPrice2, Brushes.Red, DashStyleHelper.Solid, 2);
                shortStopPrice_prev2 = shortStopPrice2;
            }

            double long_sl2 = longStopPrice2;
            double short_sl2 = shortStopPrice2;

            if (Position.MarketPosition == MarketPosition.Long & Close[0] < long_sl2 & position > 0)
            {
                ExitLong("Buy2");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExBuy2Txt" + CurrentBar, false, "Ex-Buy2 \nStopLoss", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            if (Position.MarketPosition == MarketPosition.Short & Close[0] > short_sl2 & position < 0)
            {
                ExitShort("Sell2");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExSell2Txt" + CurrentBar, false, "Ex-Sell2 \nStopLoss", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
		}
        protected void Strategy_3()
        {
            bool buy3 = volu1 & CrossAbove(Close, ret, 1) & Close[0] < ema[0] & Close[0] < ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            bool sell3 = vold1 & CrossBelow(Close, ret, 1) & Close[0] > ema[0] & Close[0] > ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            
            if (buy3 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("longalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterLong("Buy3");
                onStrategy = 3;
                position = 1;
                buy3_sl = High[1];
                longStopPrice_prev3 = buy3_sl;
                Draw.Text(this, "Buy3" + CurrentBar, false, "BUY-3", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
            }

            if (sell3 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("shortalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterShort("Sell3");
                onStrategy = 3;
                position = -1;
                sell3_sl = High[1];
                shortStopPrice_prev3 = sell3_sl;
                Draw.Text(this, "Sell3" + CurrentBar, false, "SELL-3", 0, High[0], +30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
            }

            double shortTrailPerc = longTrailPerc; // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
            double stopValue;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                stopValue = Close[0] * (1 - longTrailPerc / 100);
                longStopPrice3 = Math.Max(stopValue, Math.Max(longStopPrice_prev3, buy3_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, longStopPrice_prev3, 0, longStopPrice3, Brushes.LimeGreen, DashStyleHelper.Solid, 2);
                longStopPrice_prev3 = longStopPrice3;
            }

            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                stopValue = Close[0] * (1 + shortTrailPerc / 100);
                shortStopPrice3 = Math.Min(stopValue, Math.Min(shortStopPrice_prev3, sell3_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, shortStopPrice_prev3, 0, shortStopPrice3, Brushes.Red, DashStyleHelper.Solid, 2);
                shortStopPrice_prev3 = shortStopPrice3;
            }

            double long_sl3 = longStopPrice3;
            double short_sl3 = shortStopPrice3;

            if (Position.MarketPosition == MarketPosition.Long & Close[0] < long_sl3 & position > 0)
            {
                ExitLong("Buy3");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExBuy3Txt" + CurrentBar, false, "Ex-Buy3 \nStopLoss", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            if (Position.MarketPosition == MarketPosition.Short & Close[0] > short_sl3 & position < 0)
            {
                ExitShort("Sell3");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExSell3Txt" + CurrentBar, false, "Ex-Sell3 \nStopLoss", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
        }
        protected void Strategy_4()
        {
            bool buy4 = volu1 & CrossAbove(Close, ema, 1) & Close[0] > ret[0] & Close[0] > ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            bool sell4 = vold1 & CrossBelow(Close, ema, 1) & Close[0] < ret[0] & Close[0] < ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            
            if (buy4 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("longalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterLong("Buy4");
                onStrategy = 4;
                position = 1;
                buy4_sl = Low[1];
                longStopPrice_prev4 = buy4_sl;
                Draw.Text(this, "Buy4" + CurrentBar, false, "BUY-4", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
            }

            if (sell4 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                Alert("shortalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterShort("Sell4");
                onStrategy = 4;
                position = -1;
                sell4_sl = High[1];
                shortStopPrice_prev4 = sell4_sl;
                Draw.Text(this, "Sell4" + CurrentBar, false, "SELL-4", 0, High[0], +30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
            }

            double shortTrailPerc = longTrailPerc; // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
            double stopValue;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                stopValue = Close[0] * (1 - longTrailPerc / 100);
                longStopPrice4 = Math.Max(stopValue, Math.Max(longStopPrice_prev4, buy4_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, longStopPrice_prev4, 0, longStopPrice4, Brushes.LimeGreen, DashStyleHelper.Solid, 2);
                longStopPrice_prev4 = longStopPrice4;
            }

            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                stopValue = Close[0] * (1 + shortTrailPerc / 100);
                shortStopPrice4 = Math.Min(stopValue, Math.Min(shortStopPrice_prev4, sell4_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, shortStopPrice_prev4, 0, shortStopPrice4, Brushes.Red, DashStyleHelper.Solid, 2);
                shortStopPrice_prev4 = shortStopPrice4;
            }

            double long_sl4 = longStopPrice4;
            double short_sl4 = shortStopPrice4;

            if (Position.MarketPosition == MarketPosition.Long & Close[0] < long_sl4 & position > 0)
            {
                ExitLong("Buy4");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExBuy4Txt" + CurrentBar, false, "Ex-Buy4 \nStopLoss", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
            if (Position.MarketPosition == MarketPosition.Short & Close[0] > short_sl4 & position < 0)
            {
                ExitShort("Sell4");
                onStrategy = 0;
                position = 0;
                Draw.Text(this, "ExSell4Txt" + CurrentBar, false, "Ex-Sell4 \nStopLoss", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
            }
        }
        protected void Strategy_5()
        {
            bool buy5 = volu1 & CrossAbove(Close, ema, 1) & Close[0] > ret[0] & Close[0] < ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            bool sell5 = vold1 & CrossBelow(Close, ema, 1) & Close[0] < ret[0] & Close[0] > ema2[0] & Position.MarketPosition == MarketPosition.Flat;
            
            if (buy5 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                //Alert("longalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterLong("Buy5");
                onStrategy = 5;
                position = 1;
                buy5_sl = Low[1];
                longStopPrice_prev5 = buy5_sl;
                Draw.Text(this, "Buy5" + CurrentBar, false, "BUY-5", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
            }

            if (sell5 & Position.MarketPosition == MarketPosition.Flat & position == 0)
            {
                //Alert("shortalert", Priority.High, "Color changed - Buy", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                EnterShort("Sell5");
                onStrategy = 5;
                position = -1;
                sell5_sl = High[1];
                shortStopPrice_prev5 = sell5_sl;
                Draw.Text(this, "Sell5" + CurrentBar, false, "SELL-5", 0, High[0], +30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
            }

            double shortTrailPerc = longTrailPerc; // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
            double stopValue;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                stopValue = Close[0] * (1 - longTrailPerc / 100);
                longStopPrice5 = Math.Max(stopValue, Math.Max(longStopPrice_prev5, buy5_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, longStopPrice_prev5, 0, longStopPrice5, Brushes.LimeGreen, DashStyleHelper.Solid, 2);
                longStopPrice_prev5 = longStopPrice5;
            }

            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                stopValue = Close[0] * (1 + shortTrailPerc / 100);
                shortStopPrice5 = Math.Min(stopValue, Math.Min(shortStopPrice_prev5, sell5_sl));
                Draw.Line(this, "Long" + CurrentBar, false, 1, shortStopPrice_prev5, 0, shortStopPrice5, Brushes.Red, DashStyleHelper.Solid, 2);
                shortStopPrice_prev5 = shortStopPrice5;
            }

            double long_sl5 = longStopPrice5;
            double short_sl5 = shortStopPrice5;

            if (Position.MarketPosition == MarketPosition.Long & position > 0)
            {
                if(Close[0] < long_sl5)
                {
                    ExitLong("Buy5");
                    onStrategy = 0;
                    position = 0;
                    Draw.Text(this, "ExBuy5Txt" + CurrentBar, false, "Ex-Buy5 \nStopLoss", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                    //Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                }
                if(Close[0] > ema2[0])
                {
                    ExitLong("Buy5");
                    onStrategy = 0;
                    position = 0;
                    Draw.Text(this, "ExBuy5Txt" + CurrentBar, false, "Ex-Buy5 \nProfitTarget", 0, High[0], 30, Brushes.White, myFont, TextAlignment.Center, Brushes.Green, null, 1);
                    //Alert("closelong", Priority.High, closelong, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                }
            }
            if (Position.MarketPosition == MarketPosition.Short & position < 0)
            {
                if (Close[0] > short_sl5)
                {
                    ExitShort("Sell5");
                    onStrategy = 0;
                    position = 0;
                    Draw.Text(this, "ExSell5Txt" + CurrentBar, false, "Ex-Sell5 \nStopLoss", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                    //Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                }
                if(Close[0] < ema2[0])
                {
                    ExitShort("Sell5");
                    onStrategy = 0;
                    position = 0;
                    Draw.Text(this, "ExSell5Txt" + CurrentBar, false, "Ex-Sell5 \nProfitTarget", 0, Low[0], -30, Brushes.White, myFont, TextAlignment.Center, Brushes.Red, null, 1);
                    //Alert("closeshort", Priority.High, closeshort, NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
                }
            }
            //plotshape(buy1 ? low : na, title = "Buy1", text = "Buy1", location = location.belowbar, style = shape.labelup, size = size.tiny, color = color.green, textcolor = color.white)
            //plotshape(sell1 ? high : na, title = "Sell1", text = "Sell1", location = location.abovebar, style = shape.labeldown, size = size.tiny, color = color.red, textcolor = color.white)
        }

        /// <FVG>

        private void BullFVGCheck()
        {
            if (High[2] < Low[0])
            {
                bullfvghigh.Add(High[2]);
                bullfvglow.Add(Low[0]);
                bullfvghight.Add(Time[2]);
                bullfvglowt.Add(Time[0]);
            }

            if (bullfvglow.Count > 0)
            {
                for (int x = bullfvglow.Count - 1; x >= 0; x--)
                {
                    if (mit == "no mitigation")
                    {
                        if ((Time[0] - bullfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bullfvghightemp.Add(bullfvghigh[x]);
                            bullfvghighttemp.Add(bullfvghight[x]);
                            bullfvglowtemp.Add(bullfvglow[x]);
                            bullfvglowttemp.Add(bullfvglowt[x]);
                        }
                    }
                }
            }
            bullfvghigh = bullfvghightemp.ToList();
            bullfvghight = bullfvghighttemp.ToList();
            bullfvglow = bullfvglowtemp.ToList();
            bullfvglowt = bullfvglowttemp.ToList();

            bullfvghightemp.Clear();
            bullfvghighttemp.Clear();
            bullfvglowtemp.Clear();
            bullfvglowttemp.Clear();
        }
        private void BearFVGCheck()
        {
            if (High[0] < Low[2])
            {
                bearfvghigh.Add(High[0]);
                bearfvglow.Add(Low[2]);
                bearfvghight.Add(Time[0]);
                bearfvglowt.Add(Time[2]);
            }

            if (bearfvglow.Count > 0)
            {
                for (int x = bearfvglow.Count - 1; x >= 0; x--)
                {
                    if (mit == "no mitigation")
                    {
                        if ((Time[0] - bearfvglowt[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bearfvghightemp.Add(bearfvghigh[x]);
                            bearfvghighttemp.Add(bearfvghight[x]);
                            bearfvglowtemp.Add(bearfvglow[x]);
                            bearfvglowttemp.Add(bearfvglowt[x]);
                        }
                    }
                }
            }
            bearfvghigh = bearfvghightemp.ToList();
            bearfvghight = bearfvghighttemp.ToList();
            bearfvglow = bearfvglowtemp.ToList();
            bearfvglowt = bearfvglowttemp.ToList();

            bearfvghightemp.Clear();
            bearfvghighttemp.Clear();
            bearfvglowtemp.Clear();
            bearfvglowttemp.Clear();
        }

        private void DrawBullFVG()
        {
            if (bullfvghigh.Count > 0)
            {
                n = 0;
                prevnearbull = High[0];
                for (int x = bullfvghigh.Count - 1; x >= 0; x--)
                {
                    if (Math.Abs(Close[0] - (bullfvglow[x] - (bullfvglow[x] - bullfvghigh[x]) / 2)) < prevnearbull)
                    {
                        prevnearbull = Math.Abs(Close[0] - (bullfvglow[x] - (bullfvglow[x] - bullfvghigh[x])) / 2);
                        n = x;
                    }
                }

                Draw.Line(this, "bullhighline" + CurrentBar, false, bullfvghight[n], bullfvghigh[n], Time[0], bullfvghigh[n], Bullfvg, DashStyleHelper.Solid, 1);
                Draw.Line(this, "bulllowline" + CurrentBar, false, bullfvglowt[n], bullfvglow[n], Time[0], bullfvglow[n], Bullfvg, DashStyleHelper.Solid, 1);

                bullhighlinearr.Add("bullhighline" + CurrentBar);
                bulllowlinearr.Add("bulllowline" + CurrentBar);
            }

            if (bullhighlinearr.Count > 1)
            {
                string delTag;
                delTag = bullhighlinearr[0];
                bullhighlinearr.RemoveAt(0);
                RemoveDrawObject(delTag);

                delTag = bulllowlinearr[0];
                bulllowlinearr.RemoveAt(0);
                RemoveDrawObject(delTag);
            }
        }

        private void DrawBearFVG()
        {
            if (bearfvghigh.Count > 0)
            {
                m = 0;
                prevnearbear = High[0];
                for (int x = bearfvghigh.Count - 1; x >= 0; x--)
                {
                    if (Math.Abs(Close[0] - (bearfvglow[x] - (bearfvglow[x] - bearfvglow[x]) / 2)) < prevnearbear)
                    {
                        prevnearbear = Math.Abs(Close[0] - (bearfvglow[x] - (bearfvglow[x] - bearfvglow[x])) / 2);
                        m = x;
                    }
                }

                Draw.Line(this, "bearhighline" + CurrentBar, false, bearfvghight[m], bearfvghigh[m], Time[0], bearfvghigh[m], Bearfvg, DashStyleHelper.Solid, 1);
                Draw.Line(this, "bearlowline" + CurrentBar, false, bearfvglowt[m], bearfvglow[m], Time[0], bearfvglow[m], Bearfvg, DashStyleHelper.Solid, 1);

                bearhighlinearr.Add("bearhighline" + CurrentBar);
                bearlowlinearr.Add("bearlowline" + CurrentBar);
            }

            if (bearhighlinearr.Count > 1)
            {
                string delTag;
                delTag = bearhighlinearr[0];
                bearhighlinearr.RemoveAt(0);
                RemoveDrawObject(delTag);

                delTag = bearlowlinearr[0];
                bearlowlinearr.RemoveAt(0);
                RemoveDrawObject(delTag);
            }
        }
        /// </FVG>

        #region Properties   

        // +------------------------------------------------------------+
        // | MT4/5  (order 1)
        // +------------------------------------------------------------+
        [NinjaScriptProperty]
        [Range(0, float.MaxValue)]
        [Display(Name = "Qty", Order = 1, GroupName = "MT4/5 Settings", Description = "Risk")]
        public float pc_risk
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "License ID", Order = 2, GroupName = "MT4/5 Settings", Description = "This is your license ID")]
        public string pc_id
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "MetaTrader Symbol", Order = 3, GroupName = "MT4/5 Settings", Description = "This is your broker's MetaTrader symbol name")]
        public string pc_prefix
        { get; set; }


        // +------------------------------------------------------------+
        // | EMA (order 2)
        // +------------------------------------------------------------+

        // EMA 200
        [NinjaScriptProperty]
        [Display(Name = "EMA1 Len", Order = 1, GroupName = "EMA", Description = "EMA 1 period")]
        public int ema_len
        { get; set; }

        // EMA 800
        [NinjaScriptProperty]
        [Display(Name = "EMA2 Len", Order = 2, GroupName = "EMA", Description = "EMA 2 period")]
        public int ema2_len
        { get; set; }

        // +------------------------------------------------------------+
        // | Trend Trader Strategy (grp3)
        // +------------------------------------------------------------+
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Length", Order = 1, GroupName = "Trend Trader Strategy", Description = "")]
        public int Length
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Multiplier", Order = 2, GroupName = "Trend Trader Strategy", Description = "")]
        public double Multiplier
        { get; set; }

        // +------------------------------------------------------------+
        // | Volume Based Coloured Bars (grp4)
        // +------------------------------------------------------------+
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "length", Order = 1, GroupName = "Volume Based Coloured Bars", Description = "")]
        public int smaLength
        { get; set; }

        // +------------------------------------------------------------+
        // | Parameters
        // +------------------------------------------------------------+
        [NinjaScriptProperty]
        [Display(Name = "Trail S.Loss (%)", Order = 0, GroupName = "Parameters", Description = "")]
        public double longTrailPerc
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity size:", Order = 1, GroupName = "Parameters", Description = "")]
        public int quantitySize
        { get; set; }

        // +------------------------------------------------------------+
        // | Strategies
        // +------------------------------------------------------------+
        //[NinjaScriptProperty]
        //[Display(Name = "Strategy-1", Order = 1, GroupName = "Strategies", Description = "")]
        //public bool strategy_1
        //{ get; set; }

        //[NinjaScriptProperty]
        //[Display(Name = "Strategy-2", Order = 2, GroupName = "Strategies", Description = "")]
        //public bool strategy_2
        //{ get; set; }

        //[NinjaScriptProperty]
        //[Display(Name = "Strategy-3", Order = 3, GroupName = "Strategies", Description = "")]
        //public bool strategy_3
        //{ get; set; }

        //[NinjaScriptProperty]
        //[Display(Name = "Strategy-4", Order = 4, GroupName = "Strategies", Description = "")]
        //public bool strategy_4
        //{ get; set; }

        //[NinjaScriptProperty]
        //[Display(Name = "Strategy-5", Order = 5, GroupName = "Strategies", Description = "")]
        //public bool strategy_5
        //{ get; set; }

        //////////////

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullfvg", Description = "Bull FVG", Order = 1, GroupName = "FVG")]
        public Brush Bullfvg
        { get; set; }

        [Browsable(false)]
        public string BullfvgSerializable
        {
            get { return Serialize.BrushToString(Bullfvg); }
            set { Bullfvg = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearfvg", Description = "Bear FVG", Order = 2, GroupName = "FVG")]
        public Brush Bearfvg
        { get; set; }

        [Browsable(false)]
        public string BearfvgSerializable
        {
            get { return Serialize.BrushToString(Bearfvg); }
            set { Bearfvg = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Lookback", Description = "Bars to look back", Order = 3, GroupName = "FVG")]
        public int Lookback
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bprs", Description = "Show when nearest FVGs create a BPR", Order = 4, GroupName = "FVG")]
        public bool Bprs
        { get; set; }
        ////////////////////////
        #endregion
    }
}