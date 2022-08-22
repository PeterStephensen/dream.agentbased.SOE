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

b = b %>% filter(Time>12*70)


pdf(paste0(o_dir,"/base.pdf"))

par(mfrow=c(1,1))


zb = b$nFirms
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Number of Firms", xlab="Year", ylab="")
abline(h=0)

zb = b$marketPrice
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Price", xlab="Year", ylab="")
abline(h=0)


zb = b$marketWage
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Wage", xlab="Year", ylab="")
abline(h=0)

zb = b$marketWage / b$marketPrice
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Real Wage", xlab="Year", ylab="")
abline(h=0)

zb = b$expSharpeRatio
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(-0.2,0.2), main="Sharpe Ratio", xlab="Year", ylab="")
abline(h=0)

zb = b$Employment
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Employment", xlab="Year", ylab="")
abline(h=0)

zb = b$Sales
mx = max(max(zb))
mn = min(min(zb))
plot(b$Time/12, zb, type="l", ylim=c(0,mx), main="Sales", xlab="Year", ylab="")
abline(h=0)

#-------------------
zb = b$marketPrice

n = min(length(zb))

ib = zb[1:n]

for(t in 2:length(ib))
{
  ib[t] = (zb[t]/zb[t-1])^12 - 1
}

ib[1] = ib[2] 

zb = ib
mx = max(max(zb))
mn = min(min(zb))
plot(40+(1:n)/12, zb, type="l", ylim=c(-0.1,0.05), main="Price inflation", xlab="Year", ylab="")
abline(h=0)

#-------------------
zb = b$marketWage

n = min(length(zb))

ib = zb[1:n]

for(t in 2:length(ib))
{
  ib[t] = (zb[t]/zb[t-1])^12 - 1
}

ib[1] = ib[2] 

zb = ib
mx = max(max(zb))
mn = min(min(zb))
plot(40+(1:n)/12, zb, type="l", ylim=c(-0.1,0.05), main="Wage inflation", xlab="Year", ylab="")
abline(h=0)


dev.off()



