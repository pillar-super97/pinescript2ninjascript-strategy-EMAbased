
//@version=5
strategy("FXMATHEUS strategy_v1", overlay=true , process_orders_on_close = true )


pc_risk = input.float(title="Qty:", defval=2, step=0.1,minval = 0, group="MT4/5 Settings", tooltip="Risk")
pc_id = input.string(title="License ID", defval="", group="MT4/5 Settings", tooltip="This is your license ID")
pc_prefix = input.string(title="MetaTrader Symbol", defval="", group="MT4/5 Settings", tooltip="This is your broker's MetaTrader symbol name")

usef = pc_risk

var symbol = pc_prefix

longalert =  pc_id + ",buy,"+ symbol+ ",risk=" + str.tostring(usef,"#.##") + ""
shortalert =    pc_id + ",sell,"+symbol+",risk=" + str.tostring(usef,"#.##") + ""
closelong =    pc_id + ",closelong," +symbol+""
closeshort =     pc_id + ",closeshort," +symbol+""




sps =strategy.position_size 

ema_src =  input.source(title="EMA1 Src", defval=close,inline="1",group="EMA")
ema_len = input.int(title="Len", defval=200,inline="1",group="EMA")
ema = ta.ema(ema_src,ema_len)
plot( ema , title='EMA', color=color.purple, linewidth=3, style=plot.style_line )


ema2_src =  input.source(title="EMA2 Src", defval=close,inline="2",group="EMA")
ema2_len = input.int(title="Len", defval=800,inline="2",group="EMA")
ema2 = ta.ema(ema2_src,ema2_len)
plot( ema2 , title='EMA2', color=color.orange, linewidth=3, style=plot.style_line )

// -- 'Trend Trader Strategy'
grp3 = 'Trend Trader Strategy'
Length = input.int(21, minval=1,group=grp3)
Multiplier = input.float(3, minval=0.000001,group=grp3)
avgTR = ta.wma(ta.atr(1), Length)
highestC = ta.highest(Length)
lowestC = ta.lowest(Length)
hiLimit = highestC[1] - avgTR[1] * Multiplier
loLimit = lowestC[1] + avgTR[1] * Multiplier
ret = 0.0
pos = 0.0
ret:= close > hiLimit and close > loLimit ? hiLimit :
         close < loLimit and close < hiLimit ? loLimit : nz(ret[1], close)
pos:=  close > ret ? 1 :close < ret ? -1 : nz(pos[1], 0)
if pos != pos[1] and pos == 1
    alert("Color changed - Buy", alert.freq_once_per_bar_close)
if pos != pos[1] and pos == -1
    alert("Color changed - Sell", alert.freq_once_per_bar_close)
// barcolor(pos == -1 ? color.red : pos == 1 ? color.green : color.blue)
plot(ret, color=color.new(color.blue, 0), title='Trend Trader Strategy')

//-- 'Volume Based Coloured Bars' 
grp4 = 'Volume Based Coloured Bars' 
length = input.int(21, 'length', minval=1,group=grp4)
avrg = ta.sma(volume, length)

vold1 = volume > avrg * 1.5 and close < open
vold2 = volume >= avrg * 0.5 and volume <= avrg * 1.5 and close < open
vold3 = volume < avrg * 0.5 and close < open

volu1 = volume > avrg * 1.5 and close > open
volu2 = volume >= avrg * 0.5 and volume <= avrg * 1.5 and close > open
volu3 = volume < avrg * 0.5 and close > open


cold1 = #800000 // sell
cold2 = #FF0000
cold3 = color.orange

 
colu1 = #006400  // buy
colu2 = color.lime
colu3 = #7FFFD4


color_1 = vold1 ? cold1 : vold2 ? cold2 : vold3 ? cold3 : volu1 ? colu1 : volu2 ? colu2 : volu3 ? colu3 : na

barcolor(color_1)


// -- strategy 

buy1  = volu1 and ta.crossover(close,ret) and close>ema and close>ema2 and sps==0
sell1 = vold1 and ta.crossunder(close,ret) and close<ema and close<ema2 and sps==0

var float buy1_sl = 0
var float sell1_sl = 0
if buy1 and sps==0
    strategy.entry("Buy1",strategy.long,alert_message = longalert )
    buy1_sl := low[1]

if sell1 and sps==0
    strategy.entry("Sell1",strategy.short,alert_message = shortalert )
    sell1_sl := high[1]

longTrailPerc = input.float(title="Trail S.Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
shortTrailPerc = longTrailPerc // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

// Determine trail stop loss prices
longStopPrice = 0.0, shortStopPrice = 0.0

longStopPrice := if (strategy.position_size > 0)
    stopValue = close * (1 - longTrailPerc)
    math.max(stopValue, longStopPrice[1],buy1_sl)
else
    0

shortStopPrice := if (strategy.position_size < 0)
    stopValue = close * (1 + shortTrailPerc)
    math.min(stopValue, shortStopPrice[1],sell1_sl)
else
    999999



long_sl1  = longStopPrice
short_sl1 = shortStopPrice
strategy.exit("Ex-Buy1","Buy1"  ,stop = long_sl1,alert_message = closelong )
strategy.exit("Ex-Sell1","Sell1",stop = short_sl1,alert_message = closeshort )


plotshape(buy1  ? low : na, title="Buy1", text="Buy1", location=location.belowbar, style=shape.labelup, size=size.tiny, color=color.green, textcolor=color.white )
plotshape(sell1 ? high : na, title="Sell1", text="Sell1", location=location.abovebar, style=shape.labeldown, size=size.tiny, color=color.red, textcolor=color.white )



// -- buy2 sell2


buy2  = volu1 and ta.crossover(close,ret) and close<ema and close>ema2 and sps==0
sell2 = vold1 and ta.crossunder(close,ret) and close>ema and close<ema2 and sps==0

var int ema200_break_up = 0
var int ema200_break_dn = 0
var float buy2_sl = 0
var float sell2_sl = 0
if buy2 and sps==0
    strategy.entry("Buy2",strategy.long,alert_message = longalert )
    buy2_sl := low[1]
    ema200_break_up := 0

if close>ema and ema200_break_up==0
    ema200_break_up := 1
if close<ema and ema200_break_dn==0
    ema200_break_dn := 1

if sell2 and sps==0
    strategy.entry("Sell2",strategy.short,alert_message = shortalert)
    sell2_sl := high[1]
    ema200_break_dn := 0

longTrailPerc2  = longTrailPerc  //input.float(title="Trail S.Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
shortTrailPerc2 = longTrailPerc // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

// Determine trail stop loss prices
longStopPrice2 = 0.0, shortStopPrice2 = 0.0

longStopPrice2 := if (strategy.position_size > 0 and ema200_break_up==1 )
    stopValue2 = close * (1 - longTrailPerc2)
    math.max(stopValue2, longStopPrice2[1],0 )
else
    0

shortStopPrice2 := if (strategy.position_size < 0 and ema200_break_dn==1 )
    stopValue2 = close * (1 + shortTrailPerc2)
    math.min(stopValue2, shortStopPrice2[1],99999999999)
else
    999999



long_sl12  = math.max(longStopPrice2,buy2_sl) 
short_sl12 = math.min(shortStopPrice2,sell2_sl)

ex_qty1 = input.float(50,"Exit1 % :")

strategy.exit("Ex1-Buy2","Buy2"  ,qty_percent  =ex_qty1 ,stop = long_sl12,limit=ema ,alert_message = closelong)
strategy.exit("Ex2-Buy2","Buy2"  ,qty_percent  =100 ,stop = long_sl12 ,alert_message = closelong)

strategy.exit("Ex1-Sell2","Sell2"  ,qty_percent  =ex_qty1 ,stop = short_sl12,limit=ema,alert_message = closeshort  )
strategy.exit("Ex-Sell2","Sell2",qty_percent =100 ,stop = short_sl12 ,alert_message = closeshort )

plotshape(buy2  ? low : na, title="Buy2", text="Buy2", location=location.belowbar, style=shape.labelup, size=size.tiny, color=color.green, textcolor=color.white )
plotshape(sell2 ? high : na, title="Sell2", text="Sell2", location=location.abovebar, style=shape.labeldown, size=size.tiny, color=color.red, textcolor=color.white )

// --  


buy3 = volu1 and ta.crossover(close,ret) and close<ema and close<ema2 and sps==0
sell3 = vold1 and ta.crossunder(close,ret) and close>ema and close>ema2 and sps==0

var int ema200_break_up3 = 0
var int ema200_break_dn3 = 0
var float buy3_sl = 0
var float sell3_sl = 0
if buy3 and sps==0
    strategy.entry("Buy3",strategy.long,alert_message = longalert )
    buy3_sl := low[1]
    ema200_break_up3 := 0

if close>ema and ema200_break_up3==0
    ema200_break_up3 := 1
if close<ema and ema200_break_dn3==0
    ema200_break_dn3 := 1

if sell3 and sps==0
    strategy.entry("Sell3",strategy.short,alert_message = shortalert)
    sell3_sl := high[1]
    ema200_break_dn3 := 0

longTrailPerc3  = longTrailPerc  //input.float(title="Trail S.Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
shortTrailPerc3 = longTrailPerc // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

// Determine trail stop loss prices
longStopPrice3 = 0.0, shortStopPrice3 = 0.0

longStopPrice3 := if (strategy.position_size > 0 and ema200_break_up3==1 )
    stopValue3 = close * (1 - longTrailPerc3)
    math.max(stopValue3, longStopPrice3[1],0 )
else
    0

shortStopPrice3 := if (strategy.position_size < 0 and ema200_break_dn3==1 )
    stopValue3 = close * (1 + shortTrailPerc3)
    math.min(stopValue3, shortStopPrice3[1],99999999999)
else
    999999



long_sl13  = math.max(longStopPrice3,buy3_sl) 
short_sl13 = math.min(shortStopPrice3,sell3_sl)

strategy.exit("Ex1-Buy3","Buy3"  ,qty_percent  =ex_qty1 ,stop = long_sl13,limit=ema,alert_message = closelong )
strategy.exit("Ex2-Buy3","Buy3"  ,qty_percent  =100 ,stop = long_sl13,limit=ema2 ,alert_message = closelong)

strategy.exit("Ex1-Sell3","Sell3"  ,qty_percent  =ex_qty1 ,stop =short_sl13,limit=ema ,alert_message = closeshort )
strategy.exit("Ex2-Sell3","Sell3"  ,qty_percent  =100 ,stop = short_sl13,limit=ema2 ,alert_message = closeshort )



plotshape(buy3  ? low : na, title="Buy3", text="Buy3", location=location.belowbar, style=shape.labelup, size=size.tiny, color=color.green, textcolor=color.white )
plotshape(sell3 ? high : na, title="Sell3", text="Sell3", location=location.abovebar, style=shape.labeldown, size=size.tiny, color=color.red, textcolor=color.white )




// -- 4


buy4 = volu1 and ta.crossover(close,ema) and close>ret and close>ema2 and sps==0
sell4 = vold1 and ta.crossunder(close,ema) and close<ret and close<ema2 and sps==0

var float buy4_sl = 0
var float sell4_sl = 0
if buy4 and sps==0
    strategy.entry("Buy4",strategy.long,alert_message = longalert )
    buy4_sl := low[1]

if sell4 and sps==0
    strategy.entry("Sell4",strategy.short,alert_message = shortalert)
    sell4_sl := high[1]

longTrailPerc4 = longTrailPerc //input.float(title="Trail S.Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
shortTrailPerc4 = longTrailPerc // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

// Determine trail stop loss prices
longStopPrice4 = 0.0, shortStopPrice4 = 0.0

longStopPrice4 := if (strategy.position_size > 0)
    stopValue4 = close * (1 - longTrailPerc4)
    math.max(stopValue4, longStopPrice4[1],buy4_sl)
else
    0

shortStopPrice4 := if (strategy.position_size < 0)
    stopValue4 = close * (1 + shortTrailPerc4)
    math.min(stopValue4, shortStopPrice4[1],sell4_sl)
else
    999999



long_sl14  = longStopPrice4
short_sl14 = shortStopPrice4
strategy.exit("Ex-Buy4","Buy4"  ,stop = long_sl14,alert_message = closelong )
strategy.exit("Ex-Sell4","Sell4",stop = short_sl14  ,alert_message = closeshort )


plotshape(buy4  ? low : na, title="Buy4", text="Buy4", location=location.belowbar, style=shape.labelup, size=size.tiny, color=color.green, textcolor=color.white )
plotshape(sell4 ? high : na, title="Sell4", text="Sell4", location=location.abovebar, style=shape.labeldown, size=size.tiny, color=color.red, textcolor=color.white )


// -- 5

buy5 = volu1 and ta.crossover(close,ema) and close>ret and close<ema2 and sps==0
sell5 = vold1 and ta.crossunder(close,ema) and close<ret and close>ema2 and sps==0


var float buy5_sl = 0
var float sell5_sl = 0
if buy5 and sps==0
    strategy.entry("Buy5",strategy.long,alert_message = longalert )
    buy5_sl := low[1]

if sell5 and sps==0
    strategy.entry("Sell5",strategy.short,alert_message = shortalert)
    sell5_sl := high[1]

longTrailPerc5 = longTrailPerc //input.float(title="Trail S.Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01
shortTrailPerc5 = longTrailPerc // input.float(title="Trail Short Loss (%)" , minval=0.0, step=0.1, defval=1) * 0.01

// Determine trail stop loss prices
longStopPrice5 = 0.0, shortStopPrice5 = 0.0

longStopPrice5 := if (strategy.position_size > 0)
    stopValue5 = close * (1 - longTrailPerc5)
    math.max(stopValue5, longStopPrice5[1],buy5_sl)
else
    0

shortStopPrice5 := if (strategy.position_size < 0)
    stopValue5 = close * (1 + shortTrailPerc5)
    math.min(stopValue5, shortStopPrice5[1],sell5_sl)
else
    999999



long_sl15  = longStopPrice5
short_sl15 = shortStopPrice5
strategy.exit("Ex-Buy5","Buy5"  ,stop = long_sl15 ,limit = ema2,alert_message = closelong)
strategy.exit("Ex-Sell5","Sell5",stop = short_sl15 ,limit = ema2,alert_message = closeshort )


plotshape(buy5  ? low : na, title="Buy5", text="Buy5", location=location.belowbar, style=shape.labelup, size=size.tiny, color=color.green, textcolor=color.white )
plotshape(sell5 ? high : na, title="Sell5", text="Sell5", location=location.abovebar, style=shape.labeldown, size=size.tiny, color=color.red, textcolor=color.white )


// -- InTrade -- //

var int intrade_l = 0
if buy1 
    intrade_l := 1
if sell2 
    intrade_l := 2
if buy3
    intrade_l := 3
if sell4 
    intrade_l := 4
if buy5
    intrade_l := 5

var int intrade_s = 0
if sell1 
    intrade_s := 1
if buy2 
    intrade_s := 2
if sell3
    intrade_s := 3
if buy4
    intrade_s := 4
if sell5
    intrade_s := 5



// Plot stop loss values for confirmation
plot(series=(sps > 0 and intrade_l==1 ) ? long_sl1 : na,color=color.green, style=plot.style_linebr,linewidth=2, title="Long Trail Stop")
plot(series=(sps < 0 and intrade_s==1) ? short_sl1 : na,color=color.red, style=plot.style_linebr,linewidth=2, title="Short Trail Stop")
// Plot stop loss values for confirmation
plot(series=(sps > 0 and intrade_l==2) ? long_sl12 : na,color=color.green, style=plot.style_linebr,linewidth=2, title="Buy2 Trail Stop")
plot(series=(sps < 0 and intrade_s==2) ? short_sl12 : na,color=color.red, style=plot.style_linebr,linewidth=2, title="Sell2 Trail Stop")

plot(series=(sps > 0 and intrade_l==3) ? long_sl13 : na,color=color.green, style=plot.style_linebr,linewidth=2, title="Buy2 Trail Stop")
plot(series=(sps < 0 and intrade_s==3) ? short_sl13 : na,color=color.red, style=plot.style_linebr,linewidth=2, title="Sell2 Trail Stop")

plot(series=(sps > 0 and intrade_l==4) ? long_sl14 : na,color=color.green, style=plot.style_linebr,linewidth=2, title="Buy2 Trail Stop")
plot(series=(sps < 0 and intrade_s==4) ? short_sl14 : na,color=color.red, style=plot.style_linebr,linewidth=2, title="Sell2 Trail Stop")

plot(series=(sps > 0 and intrade_l==5) ? long_sl15 : na,color=color.green, style=plot.style_linebr,linewidth=2, title="Buy2 Trail Stop")
plot(series=(sps < 0 and intrade_s==5) ? short_sl15 : na,color=color.red, style=plot.style_linebr,linewidth=2, title="Sell2 Trail Stop")


