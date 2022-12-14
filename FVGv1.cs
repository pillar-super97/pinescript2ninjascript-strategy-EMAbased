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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class FVGv1 : Indicator
    {
        private int bar;

        int n = 0;
        int m = 0;
        bool bullmit = false;
        bool bearmit = false;
        private double bullhigh;
        private double bearlow;
        private double prevnearbull;
        private double prevnearbear;

        //private Line bullhighline;
        //private Line bearhighline;
        //private Line bulllowline;
        //private Line bearlowline;

        private List<string> bullhighlinearr = new List<string>();
        private List<string> bulllowlinearr = new List<string>();
        private List<string> bearhighlinearr = new List<string>();
        private List<string> bearlowlinearr = new List<string>();
        // private label bearbpr = na;
        // private label bullbpr = na;

        private List<DateTime> bullfvghight = new List<DateTime>();
        private List<DateTime> bullfvglowt = new List<DateTime>();
        private List<DateTime> bearfvghight = new List<DateTime>();
        private List<DateTime> bearfvglowt = new List<DateTime>();

        private List<DateTime> bullfvghighttemp = new List<DateTime>();
        private List<DateTime> bullfvglowttemp = new List<DateTime>();
        private List<DateTime> bearfvghighttemp = new List<DateTime>();
        private List<DateTime> bearfvglowttemp = new List<DateTime>();
        //array.copy;
        private List<double> bullfvghigh = new List<double>();
        private List<double> bullfvglow = new List<double>();
        private List<double> bearfvghigh = new List<double>();
        private List<double> bearfvglow = new List<double>();

        private List<double> bullfvghightemp = new List<double>();
        private List<double> bullfvglowtemp = new List<double>();
        private List<double> bearfvghightemp = new List<double>();
        private List<double> bearfvglowtemp = new List<double>();

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
        protected int nearest(int prev, int curr, int target)
        {
            if (Math.Abs(prev - target) < Math.Abs(curr - target))
                return prev;
            else
                return curr;
        }
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "FVGv1";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                Bullfvg = Brushes.Teal;
                Bearfvg = Brushes.Maroon;
                Lookback = 50;
                Mit = "full fill high/low";
                Bprs = true;

            }
            else if (State == State.Configure)
            {
            }
        }

        protected override void OnBarUpdate()
        {
            bar = ResolutionToSec();
            if (CurrentBar < 2) return;

            BullFVGCheck();
            BearFVGCheck();
            DrawBullFVG();
            DrawBearFVG();
        }

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
                    if (Mit == "full fill high/low")
                    {
                        if (!(Low[0] <= bullfvghigh[x] && (Time[0] - bullfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2)))
                        {
                            bullfvghightemp.Add(bullfvghigh[x]);
                            bullfvghighttemp.Add(bullfvghight[x]);
                            bullfvglowtemp.Add(bullfvglow[x]);
                            bullfvglowttemp.Add(bullfvglowt[x]);
                        }
                    }
                    if (Mit == "half fill high/low")
                    {
                        if (!(Low[0] <= bullfvglow[x] - (bullfvglow[x] - bullfvghigh[x]) / 2 && (Time[0] - bullfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2)))
                        {
                            bullfvghightemp.Add(bullfvghigh[x]);
                            bullfvghighttemp.Add(bullfvghight[x]);
                            bullfvglowtemp.Add(bullfvglow[x]);
                            bullfvglowttemp.Add(bullfvglowt[x]);
                        }
                    }
                    if (Mit == "full fill open/close")
                    {
                        if ((Close[0] < Open[0] ? Close[0] : Open[0]) >= bullfvghigh[x] && (Time[0] - bullfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bullfvghightemp.Add(bullfvghigh[x]);
                            bullfvghighttemp.Add(bullfvghight[x]);
                            bullfvglowtemp.Add(bullfvglow[x]);
                            bullfvglowttemp.Add(bullfvglowt[x]);
                        }
                    }
                    if (Mit == "half fill open/close")
                    {
                        if ((Close[0] < Open[0] ? Close[0] : Open[0]) >= bullfvglow[x] - (bullfvglow[x] - bullfvghigh[x]) / 2 && (Time[0] - bullfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bullfvghightemp.Add(bullfvghigh[x]);
                            bullfvghighttemp.Add(bullfvghight[x]);
                            bullfvglowtemp.Add(bullfvglow[x]);
                            bullfvglowttemp.Add(bullfvglowt[x]);
                        }
                    }
                    if (Mit == "no mitigation")
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

                    if (Mit == "no mitigation")
                    {
                        if ((Time[0] - bearfvglowt[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bearfvghightemp.Add(bearfvghigh[x]);
                            bearfvghighttemp.Add(bearfvghight[x]);
                            bearfvglowtemp.Add(bearfvglow[x]);
                            bearfvglowttemp.Add(bearfvglowt[x]);
                        }
                    }
                    if (Mit == "full fill high/low")
                    {
                        if (!(Low[0] <= bearfvghigh[x] && (Time[0] - bearfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2)))
                        {
                            bearfvghightemp.Add(bearfvghigh[x]);
                            bearfvghighttemp.Add(bearfvghight[x]);
                            bearfvglowtemp.Add(bearfvglow[x]);
                            bearfvglowttemp.Add(bearfvglowt[x]);
                        }
                    }
                    if (Mit == "half fill high/low")
                    {
                        if (!(Low[0] <= bearfvglow[x] - (bearfvglow[x] - bearfvghigh[x]) / 2 && (Time[0] - bearfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2)))
                        {
                            bearfvghightemp.Add(bearfvghigh[x]);
                            bearfvghighttemp.Add(bearfvghight[x]);
                            bearfvglowtemp.Add(bearfvglow[x]);
                            bearfvglowttemp.Add(bearfvglowt[x]);
                        }
                    }
                    if (Mit == "full fill open/close")
                    {
                        if ((Close[0] < Open[0] ? Close[0] : Open[0]) >= bearfvghigh[x] && (Time[0] - bearfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2))
                        {
                            bearfvghightemp.Add(bearfvghigh[x]);
                            bearfvghighttemp.Add(bearfvghight[x]);
                            bearfvglowtemp.Add(bearfvglow[x]);
                            bearfvglowttemp.Add(bearfvglowt[x]);
                        }
                    }
                    if (Mit == "half fill open/close")
                    {
                        if ((Close[0] < Open[0] ? Close[0] : Open[0]) >= bearfvglow[x] - (bearfvglow[x] - bearfvghigh[x]) / 2 && (Time[0] - bearfvghight[x]).TotalMilliseconds <= Lookback * (bar + 2))
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
        #region Properties
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullfvg", Description = "Bull FVG", Order = 1, GroupName = "Parameters")]
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
        [Display(Name = "Bearfvg", Description = "Bear FVG", Order = 2, GroupName = "Parameters")]
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
        [Display(Name = "Lookback", Description = "Bars to look back", Order = 3, GroupName = "Parameters")]
        public int Lookback
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Mit", Description = "Mitigation Type", Order = 4, GroupName = "Parameters")]
        public string Mit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bprs", Description = "Show when nearest FVGs create a BPR", Order = 5, GroupName = "Parameters")]
        public bool Bprs
        { get; set; }

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
    {
        private FVGv1[] cacheFVGv1;
        public FVGv1 FVGv1(Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            return FVGv1(Input, bullfvg, bearfvg, lookback, bprs);
        }

        public FVGv1 FVGv1(ISeries<double> input, Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            if (cacheFVGv1 != null)
                for (int idx = 0; idx < cacheFVGv1.Length; idx++)
                    if (cacheFVGv1[idx] != null && cacheFVGv1[idx].Bullfvg == bullfvg && cacheFVGv1[idx].Bearfvg == bearfvg && cacheFVGv1[idx].Lookback == lookback && cacheFVGv1[idx].Bprs == bprs && cacheFVGv1[idx].EqualsInput(input))
                        return cacheFVGv1[idx];
            return CacheIndicator<FVGv1>(new FVGv1() { Bullfvg = bullfvg, Bearfvg = bearfvg, Lookback = lookback, Bprs = bprs }, input, ref cacheFVGv1);
        }
    }
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
    public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
    {
        public Indicators.FVGv1 FVGv1(Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            return indicator.FVGv1(Input, bullfvg, bearfvg, lookback, bprs);
        }

        public Indicators.FVGv1 FVGv1(ISeries<double> input, Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            return indicator.FVGv1(input, bullfvg, bearfvg, lookback, bprs);
        }
    }
}

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.FVGv1 FVGv1(Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            return indicator.FVGv1(Input, bullfvg, bearfvg, lookback, bprs);
        }

        public Indicators.FVGv1 FVGv1(ISeries<double> input, Brush bullfvg, Brush bearfvg, int lookback, bool bprs)
        {
            return indicator.FVGv1(input, bullfvg, bearfvg, lookback, bprs);
        }
    }
}

#endregion
