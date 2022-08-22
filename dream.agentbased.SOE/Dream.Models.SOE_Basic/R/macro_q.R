rm(list=ls())
library(dplyr)
#library(forecast)

hpfilter      = function(x, mu = 100) {
  y = x
  n <- length(y)          # number of observations
  I <- diag(n)            # creates an identity matrix
  D <- diff(I,lag=1,d=2)  # second order differences
  d <- solve(I + mu * crossprod(D) , y) # solves focs
  d
}


if(Sys.info()['nodename'] == "C1709161")    # PSP's machine
{
  o_dir = "C:/test/Dream.AgentBased.MacroModel"  
}
if(Sys.info()['nodename'] == "VDI00316")    # Fjernskrivebord
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}
if(Sys.info()['nodename'] == "VDI00382")    # Fjernskrivebord for agentbased projekt
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}


d = read.delim(paste0(o_dir, "\\macro.txt"))


if(T)
{
  yr0 = 12*(2100-2014)-1
  d = d %>% filter(Time>yr0)
  d$Time = d$Time - yr0 
  d$Quater = floor(d$Time/3)
  
  dd = d %>% group_by(Quater) %>%
    summarise(Sales=sum(Sales), Employment=mean(Employment), Price=mean(marketPrice), 
              Wage=mean(marketWage), SharpeRatio=mean(SharpeRatio), nFirmClosed=sum(nFirmClosed),
              nFirmNew=sum(nFirmNew), nFirms=mean(nFirms), nUnemployed=mean(nUnemployed),
              nLaborSupply=mean(nLaborSupply), nHouseholds=mean(nHouseholds))

  dd$Year = dd$Quater/4
  
  maxyr = max(dd$Year)
  dd = dd[dd$Year<maxyr & dd$Year>0,]
  
}

vert_lin = function(t)
{
  for(v in seq(from=0, to=max(t), by=10))
    abline(v=v, col="gray")
}

pplot = function(t,x, main="", s_miny=0, s_maxy=1.2)
{
  plot(t, x, type="l", main=main, ylim=max(x)*c(s_miny,s_maxy), ylab="", xlab="Year")
  abline(h=0)
  vert_lin(t)
  lines(t, x)
  
}

#pplot(d$Time, d$nFirms)

#s = spectrum(d$Employment, log="no")
#x = s$freq
#y = s$spec
#plot(x,y, type="l")
#abline(h=0)


#plot(d$Time/12, d$Employment, type="l")
#lines(d$Time/12, hpfilter(d$Employment, mu = 900000), lty=2)

#hist(log(d$Employment)-hpfilter(log(d$Employment), mu = 900000), breaks = 20, xlim=c(-0.04, 0.04))

#library(fitdistrplus)
#descdist(log(d$Employment)-hpfilter(log(d$Employment)), discrete = FALSE)


pdf(paste0(o_dir, "\\macro_q.pdf"))

plot(dd$Quater/4, dd$SharpeRatio, type="l", main="Sharpe Ratio", xlab="Year", ylab="", ylim=c(-0.2, 0.2))
abline(h=0)
vert_lin(d$Quater/4)

pplot(dd$Quater/4, dd$Wage*dd$Employment/(dd$Price*dd$Sales), main="Wage share")

#pplot(d$Time/12, d$nFirmClosed, main="New (red) and Closed firms", s_miny = 0.1, s_maxy = 1.1)
pplot(dd$Quater/4, dd$nFirmClosed, main="New (red) and Closed firms")
lines(dd$Quater/4, dd$nFirmNew, col="red", lwd=2)

#pplot(d$Time/12, d$nFirms, main="Number of firms", s_miny = 0.2, s_maxy = 1.1)
pplot(dd$Quater/4, dd$nFirms, main="Number of firms")

#pplot(d$Time/12, d$SigmaRisk, main="Risk (black) and Profit (red)")
#lines(d$Time/12, 7+3*d$SigmaRisk*d$SharpeRatio, col="red")

#pplot(d$Time/12, d$Production/(d$nLaborSupply - d$nUnemployed), main="Productivity per head")

#pplot(d$Time/12, d$Production/d$Employment, main="Productivity per productivity unit")

#pplot(d$Time/12, d$Production/d$nFirms, main="Productivity per firm")
#lines(d$Time/12, 6+10*d$expSharpeRatio, lty=2)

pplot(dd$Quater/4, dd$Sales, main="Sales", s_miny = 0.1, s_maxy = 1.1)
#pplot(d_yr$Year, d_yr$Sales, main="Sales (Yearly)")

pplot(dd$Quater/4, dd$Employment, main="Employment", s_miny = 0.5, s_maxy = 1.05)
#pplot(d_yr$Year, d_yr$Employment, main="Employment (Yearly)")

pplot(dd$Quater/4, dd$nUnemployed/dd$nLaborSupply, main="Unemployment rate", s_maxy = 1.7)
#lines(d$Time/12, 0.03-0.2*d$expSharpeRatio, lty=2)
#abline(h=0.03, lty=2)

pplot(dd$Quater/4, dd$nHouseholds, main="Poupulation and labor supply (dashed)", s_maxy = 1.7)
lines(dd$Quater/4, dd$nLaborSupply, lty=2)


pplot(dd$Quater/4, dd$Wage/dd$Price, main="Real wage")
#pplot(d_yr$Year, d_yr$Wage/d_yr$Price, main="Real wage (Yearly)")

pplot(dd$Quater/4, dd$Price, main="Price")
#pplot(d_yr$Year, d_yr$Price, main="Price (Yearly)")

pplot(dd$Quater/4, dd$Wage, main="Wage")
#pplot(d_yr$Year, d_yr$Wage, main="Wage (Yearly)")

#pplot(d$Time/12, d$nHouseholds, main="Number of households (Red: In labor force)", s_miny = 0.5)
#lines(d$Time/12, d$nLaborSupply, col="red")

dev.off()







