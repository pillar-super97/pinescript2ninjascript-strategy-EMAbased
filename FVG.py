// // This source code is subject to the terms of the Mozilla Public License 2.0 at https://mozilla.org/MPL/2.0/
// // © Texmoonbeam

//@version=5
indicator("Auto Closest FVG with BPR", overlay = true, max_boxes_count = 100, max_lines_count = 100, max_labels_count = 500, max_bars_back = 5000)
bullfvg = input.color(defval=color.new(color.teal,25), title='Bull FVG')
bearfvg = input.color(defval=color.new(color.maroon,25), title='Bear FVG')
lookback = input.int(defval=50, title='Bars to look back')
mit = input.string(defval="full fill high/low", title = "Mitigation Type", options = ["full fill high/low","full fill open/close","half fill high/low","half fill open/close","no mitigation"], tooltip="Defines when an FVG will be mitigated and not used/displayed. This can be when full or half filled, by a high/low value or open/close value. When using no mitigation the nearest FVG will be shown always.")
bprs = input.bool(defval=true, title='Show when nearest FVGs create a BPR ▼', tooltip="BPR refers to balanced price range, and an overlapped bull/bear FVG pair")

//Convert a given string resolution into seconds
ResolutionToSec(res)=>
    mins = res == "1" ? 1 :
           res == "3" ? 3 :
           res == "5" ? 5 :
           res == "10" ? 10 :
           res == "15" ? 15 :
           res == "30" ? 30 :
           res == "45" ? 45 :
           res == "60" ? 60 :
           res == "120" ? 120 :
           res == "180" ? 180 :
           res == "240" ? 240 :
           res == "D" or res == "1D" ? 1440 :
           res == "W" or res == "1W" ? 10080 :
           res == "M" or res == "1M" ? 43200 :
           res == "" ? int(str.tonumber(timeframe.period)) : int(str.tonumber(res))
    ms = mins * 60 * 1000

bar = ResolutionToSec(timeframe.period) 

//addition = ResolutionToSec(timeframe == "" ? timeframe.period : timeframe)

var n = 0
var m = 0
var bullmit = false
var bearmit = false
var float bullhigh = na
var float bearlow = na
var float prevnearbull = high
var float prevnearbear = high
var line bullhighline = na
var line bearhighline = na
var line bulllowline = na
var line bearlowline = na
var label bearbpr = na
var label bullbpr = na

var bullfvghight = array.new_int()
var bullfvglowt= array.new_int()
var bearfvghight = array.new_int()
var bearfvglowt = array.new_int()

var bullfvghighttemp = array.new_int()
var bullfvglowttemp= array.new_int()
var bearfvghighttemp = array.new_int()
var bearfvglowttemp = array.new_int()
//array.copy
var bullfvghigh = array.new_float()
var bullfvglow = array.new_float()
var bearfvghigh = array.new_float()
var bearfvglow = array.new_float()

var bullfvghightemp = array.new_float()
var bullfvglowtemp = array.new_float()
var bearfvghightemp = array.new_float()
var bearfvglowtemp = array.new_float()
   
nearest(prev, curr, target) =>
    if (math.abs(prev-target) < math.abs(curr-target))
        prev
    else    
        curr

arrayadd(n, arr1, arr2) =>
    array.push(arr2, array.get(arr1, n))

if (high[2] < low)
    array.push(bullfvghigh, high[2])
    array.push(bullfvglow, low)
    array.push(bullfvghight, time[2])
    array.push(bullfvglowt, time)
    
if (array.size((bullfvglow)) > 0)
    
    for x = array.size((bullfvglow))-1 to 0
        
        if (mit == "full fill high/low")
            if (not(low <= array.get(bullfvghigh, x)) and array.get(bullfvghight, x) >= time - lookback*(bar+2))
                arrayadd(x, bullfvghigh, bullfvghightemp)
                arrayadd(x, bullfvghight, bullfvghighttemp)
                arrayadd(x, bullfvglow, bullfvglowtemp)
                arrayadd(x, bullfvglowt, bullfvglowttemp)
        if (mit == "half fill high/low")
            if (not(low <= array.get(bullfvglow, x)-(array.get(bullfvglow, x)-array.get(bullfvghigh, x))/2) and array.get(bullfvghight, x) >= time - lookback*(bar+2))
                arrayadd(x, bullfvghigh, bullfvghightemp)
                arrayadd(x, bullfvghight, bullfvghighttemp)
                arrayadd(x, bullfvglow, bullfvglowtemp)
                arrayadd(x, bullfvglowt, bullfvglowttemp)
        if (mit == "full fill open/close")
            if ((close<open ? close : open) >= array.get(bullfvghigh, x) and array.get(bullfvghight, x) >= time - lookback*(bar+2))
                arrayadd(x, bullfvghigh, bullfvghightemp)
                arrayadd(x, bullfvghight, bullfvghighttemp)
                arrayadd(x, bullfvglow, bullfvglowtemp)
                arrayadd(x, bullfvglowt, bullfvglowttemp)
        if (mit == "half fill open/close")
            if ((close<open ? close : open) >= array.get(bullfvglow, x)-(array.get(bullfvglow, x)-array.get(bullfvghigh, x))/2) and array.get(bullfvghight, x) >= time - lookback*(bar+2)
                arrayadd(x, bullfvghigh, bullfvghightemp)
                arrayadd(x, bullfvghight, bullfvghighttemp)
                arrayadd(x, bullfvglow, bullfvglowtemp)
                arrayadd(x, bullfvglowt, bullfvglowttemp)
        if (mit == "no mitigation")
            if (array.get(bullfvghight, x) >= time - lookback*(bar+2))
                arrayadd(x, bullfvghigh, bullfvghightemp)
                arrayadd(x, bullfvghight, bullfvghighttemp)
                arrayadd(x, bullfvglow, bullfvglowtemp)
                arrayadd(x, bullfvglowt, bullfvglowttemp)

bullfvghigh := array.copy(bullfvghightemp)
bullfvghight := array.copy(bullfvghighttemp)
bullfvglow := array.copy(bullfvglowtemp)
bullfvglowt := array.copy(bullfvglowttemp)
array.clear(bullfvghightemp)
array.clear(bullfvghighttemp)
array.clear(bullfvglowtemp)
array.clear(bullfvglowttemp)


if (low[2] > high)
    array.push(bearfvghigh, high)
    array.push(bearfvglow, low[2])
    array.push(bearfvghight, time)
    array.push(bearfvglowt, time[2])
    
if (array.size((bearfvglow)) > 0)
    
    for x = array.size((bearfvglow))-1 to 0
    
        if (mit == "full fill high/low")
            if (not(high >= array.get(bearfvglow, x)) and array.get(bearfvglowt, x) >= time - lookback*(bar+2))
                arrayadd(x, bearfvghigh, bearfvghightemp)
                arrayadd(x, bearfvghight, bearfvghighttemp)
                arrayadd(x, bearfvglow, bearfvglowtemp)
                arrayadd(x, bearfvglowt, bearfvglowttemp)
        if (mit == "half fill high/low")
            if (not(high >= array.get(bearfvglow, x)-(array.get(bearfvglow, x)-array.get(bearfvghigh, x))/2) and array.get(bearfvglowt, x) >= time - lookback*(bar+2))
                arrayadd(x, bearfvghigh, bearfvghightemp)
                arrayadd(x, bearfvghight, bearfvghighttemp)
                arrayadd(x, bearfvglow, bearfvglowtemp)
                arrayadd(x, bearfvglowt, bearfvglowttemp)
        if (mit == "full fill open/close")
            if ((close>open ? close : open) < array.get(bearfvglow, x) and array.get(bearfvglowt, x) >= time - lookback*(bar+2))
                arrayadd(x, bearfvghigh, bearfvghightemp)
                arrayadd(x, bearfvghight, bearfvghighttemp)
                arrayadd(x, bearfvglow, bearfvglowtemp)
                arrayadd(x, bearfvglowt, bearfvglowttemp)
        if (mit == "half fill open/close")
            if ((close>open ? close : open) < array.get(bearfvglow, x)-(array.get(bearfvglow, x)-array.get(bearfvghigh, x))/2 and array.get(bearfvglowt, x) >= time - lookback*(bar+2))
                arrayadd(x, bearfvghigh, bearfvghightemp)
                arrayadd(x, bearfvghight, bearfvghighttemp)
                arrayadd(x, bearfvglow, bearfvglowtemp)
                arrayadd(x, bearfvglowt, bearfvglowttemp)
        if (mit == "no mitigation")
            if (array.get(bearfvglowt, x) >= time - lookback*(bar+2))
                arrayadd(x, bearfvghigh, bearfvghightemp)
                arrayadd(x, bearfvghight, bearfvghighttemp)
                arrayadd(x, bearfvglow, bearfvglowtemp)
                arrayadd(x, bearfvglowt, bearfvglowttemp)

bearfvghigh := array.copy(bearfvghightemp)
bearfvghight := array.copy(bearfvghighttemp)
bearfvglow := array.copy(bearfvglowtemp)
bearfvglowt := array.copy(bearfvglowttemp)
array.clear(bearfvghightemp)
array.clear(bearfvghighttemp)
array.clear(bearfvglowtemp)
array.clear(bearfvglowttemp)



var bullhighlinearr = array.new_line()
var bulllowlinearr = array.new_line()
var bearhighlinearr = array.new_line()
var bearlowlinearr = array.new_line()



if (bar_index == last_bar_index)

    if (array.size(bullfvghigh) > 0)
        n:=0
        prevnearbull := high
        for x = array.size((bullfvghigh))-1 to 0
            if (math.abs(close-(array.get(bullfvglow, x)-(array.get(bullfvglow, x)-array.get(bullfvghigh, x))/2)) < prevnearbull)
                prevnearbull := math.abs(close-(array.get(bullfvglow, x)-(array.get(bullfvglow, x)-array.get(bullfvghigh, x))/2))
                n := x
                    
        bullhighline := line.new(x1=array.get(bullfvghight, n), y1=array.get(bullfvghigh, n), x2=time+(bar*2), y2 = array.get(bullfvghigh, n),  xloc=xloc.bar_time, color = bullfvg)
        bulllowline := line.new(x1=array.get(bullfvglowt, n), y1=array.get(bullfvglow, n), x2=time+(bar*2), y2 = array.get(bullfvglow, n),  xloc=xloc.bar_time, color = bullfvg)
        
        line.set_x2(bullhighline, time+(bar*2))
        line.set_x2(bulllowline, time+(bar*2))
        array.push(bullhighlinearr, bullhighline)
        array.push(bulllowlinearr, bulllowline)

    if (array.size(bullhighlinearr) > 1)
        delline1 = array.remove(bullhighlinearr, 0) 
        line.delete(delline1)
        delline2 = array.remove(bulllowlinearr, 0) 
        line.delete(delline2)

     
    if (array.size(bearfvghigh) > 0)
        m:=0
        prevnearbear := high
        for x = array.size((bearfvghigh))-1 to 0
            if (math.abs(close-(array.get(bearfvglow, x)-(array.get(bearfvglow, x)-array.get(bearfvghigh, x))/2)) < prevnearbear)
                prevnearbear := math.abs(close-(array.get(bearfvglow, x)-(array.get(bearfvglow, x)-array.get(bearfvghigh, x))/2))
                m := x
                
        bearhighline := line.new(x1=array.get(bearfvghight, m), y1=array.get(bearfvghigh, m), x2=time+(bar*2), y2 = array.get(bearfvghigh, m),  xloc=xloc.bar_time, color = bearfvg)
        bearlowline := line.new(x1=array.get(bearfvglowt, m), y1=array.get(bearfvglow, m), x2=time+(bar*2), y2 = array.get(bearfvglow, m),  xloc=xloc.bar_time, color = bearfvg)
        
        line.set_x2(bearhighline, time+(bar*2))
        line.set_x2(bearlowline, time+(bar*2))
        array.push(bearhighlinearr, bearhighline)
        array.push(bearlowlinearr, bearlowline)

    if (array.size(bearhighlinearr) > 1)
        delline3 = array.remove(bearhighlinearr, 0) 
        line.delete(delline3)
        delline4 = array.remove(bearlowlinearr, 0) 
        line.delete(delline4)

    label.delete(bearbpr)
    label.delete(bullbpr)
    
    if (array.size(bulllowlinearr) > 0 and array.size(bearlowlinearr) > 0)
        if (line.get_y1(array.get(bulllowlinearr, 0)) < line.get_y1(array.get(bearlowlinearr, 0)) and line.get_y1(array.get(bullhighlinearr, 0)) > line.get_y1(array.get(bearhighlinearr, 0)) and bprs)
            bearbpr := label.new(x=time, y=high, text=str.tostring("▼"), xloc=xloc.bar_time, color=color.white, style=label.style_none, textcolor=bearfvg, size=size.normal, textalign=text.align_right)
    
        if (line.get_y1(array.get(bearlowlinearr, 0)) < line.get_y1(array.get(bulllowlinearr, 0)) and line.get_y1(array.get(bearhighlinearr, 0)) > line.get_y1(array.get(bullhighlinearr, 0)) and bprs)
            bearbpr := label.new(x=time, y=high, text=str.tostring("▼"), xloc=xloc.bar_time, color=color.white, style=label.style_none, textcolor=bullfvg, size=size.normal, textalign=text.align_right)

// debug label.new(x=time, y=high, text=str.tostring(prevnearbear), xloc=xloc.bar_time, color=color.white, style=label.style_none, textcolor=color.green, size=size.normal, textalign=text.align_right)