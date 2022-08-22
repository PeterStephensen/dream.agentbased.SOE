rm(list=ls())
#install.packages("forecast")
library(dplyr)
#library(forecast)

if(Sys.info()['nodename'] == "C1709161")    # PSP's machine
{
  o_dir = "C:/test/Dream.AgentBased.MacroModel"  
}
if(Sys.info()['nodename'] == "VDI00316")    # Fjernskrivebord
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}

b = read.delim(paste0(o_dir,"/base.txt"))
c = read.delim(paste0(o_dir,"/count.txt"))

b = b %>% filter(Time>500)
c = c %>% filter(Time>500)

pdf(paste0(o_dir,"/shock.pdf"))

par(mfrow=c(1,1))


zb = b$nFirms
zc = c$nFirms
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Number of Firms", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

zb = b$marketPrice
zc = c$marketPrice
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Price", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

zb = b$marketWage
zc = c$marketWage
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Wage", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

zb = b$marketWage / b$marketPrice
zc = c$marketWage / c$marketPrice
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Real Wage", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

zb = b$expSharpeRatio
zc = c$expSharpeRatio
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(-0.2,0.2), main="Sharpe Ratio", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")
abline(h=0)

zb = b$Employment
zc = c$Employment
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Employment", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

zb = b$Sales
zc = c$Sales
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Sales", xlab="Year", ylab="")
lines(c$Time/12, zc, col="red")

#-------------------
zb = b$marketPrice
zc = c$marketPrice

n = min(length(zb),length(zc))

ib = zb[1:n]
ic = zc[1:n]

for(t in 2:length(ib))
{
  ib[t] = (zb[t]/zb[t-1])^12 - 1
  ic[t] = (zc[t]/zc[t-1])^12 - 1
}

ib[1] = ib[2] 
ic[1] = ic[2] 

zb = ib
zc = ic
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(40+(1:n)/12, zb, type="l", ylim=c(-0.1,0.05), main="Price inflation", xlab="Year", ylab="")
lines(40+(1:n)/12, zc, col="red")
abline(h=0)

#-------------------
zb = b$marketWage
zc = c$marketWage

n = min(length(zb),length(zc))

ib = zb[1:n]
ic = zc[1:n]

for(t in 2:length(ib))
{
  ib[t] = (zb[t]/zb[t-1])^12 - 1
  ic[t] = (zc[t]/zc[t-1])^12 - 1
}

ib[1] = ib[2] 
ic[1] = ic[2] 

zb = ib
zc = ic
mx = max(max(zb), max(zc))
mn = min(min(zb), min(zc))
plot(40+(1:n)/12, zb, type="l", ylim=c(-0.1,0.05), main="Wage inflation", xlab="Year", ylab="")
lines(40+(1:n)/12, zc, col="red")
abline(h=0)


dev.off()



