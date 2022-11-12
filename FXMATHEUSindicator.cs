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
	public class FXMATHEUSindicator : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FXMATHEUSindicator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
                Length = 21;
                Multiplier = 3;

                AddPlot(Brushes.DodgerBlue, "Trend Trader Strategy");
			}
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < 1)
                return;

            WMA avgTR = WMA(ATR(Close, 1), Length);
            MAX highestC = MAX(High, Length);
            MIN lowestC = MIN(Low, Length);
            double HiLimit = highestC[1] - avgTR[1] * Multiplier;
            double LoLimit = lowestC[1] + avgTR[1] * Multiplier;
            double ret;

			if(Close[0] > HiLimit & Close[0] > LoLimit)
				ret = LoLimit;
			else if(Close[0] < HiLimit & Close[0] < LoLimit)
				ret = LoLimit;
			else if(CurrentBar < 1)
				ret = Close[0];
			else
				ret = Value[1];			
			Value[0] = ret;
		}		
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name= "Length", Order=0, GroupName="Parameters")]
		public int Length
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Multiplier", Order = 1, GroupName = "Parameters")]
        public double Multiplier
        { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FXMATHEUSindicator[] cacheFXMATHEUSindicator;
		public FXMATHEUSindicator FXMATHEUSindicator(int length, double multiplier)
		{
			return FXMATHEUSindicator(Input, length, multiplier);
		}

		public FXMATHEUSindicator FXMATHEUSindicator(ISeries<double> input, int length, double multiplier)
		{
			if (cacheFXMATHEUSindicator != null)
				for (int idx = 0; idx < cacheFXMATHEUSindicator.Length; idx++)
					if (cacheFXMATHEUSindicator[idx] != null && cacheFXMATHEUSindicator[idx].Length == length && cacheFXMATHEUSindicator[idx].Multiplier == multiplier && cacheFXMATHEUSindicator[idx].EqualsInput(input))
						return cacheFXMATHEUSindicator[idx];
			return CacheIndicator<FXMATHEUSindicator>(new FXMATHEUSindicator(){ Length = length, Multiplier = multiplier }, input, ref cacheFXMATHEUSindicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FXMATHEUSindicator FXMATHEUSindicator(int length, double multiplier)
		{
			return indicator.FXMATHEUSindicator(Input, length, multiplier);
		}

		public Indicators.FXMATHEUSindicator FXMATHEUSindicator(ISeries<double> input , int length, double multiplier)
		{
			return indicator.FXMATHEUSindicator(input, length, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FXMATHEUSindicator FXMATHEUSindicator(int length, double multiplier)
		{
			return indicator.FXMATHEUSindicator(Input, length, multiplier);
		}

		public Indicators.FXMATHEUSindicator FXMATHEUSindicator(ISeries<double> input , int length, double multiplier)
		{
			return indicator.FXMATHEUSindicator(input, length, multiplier);
		}
	}
}

#endregion
