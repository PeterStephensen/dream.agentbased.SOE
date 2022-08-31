rm(list=ls())
#install.packages("forecast")
library(dplyr)
library(forecast)

if(Sys.info()['nodename'] == "C1709161")    # PSP's machine
{
  o_dir = "C:/test/Dream.AgentBased.MacroModel"  
}
if(Sys.info()['nodename'] == "VDI00316")    # Fjernskrivebord
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}
if(Sys.info()['nodename'] == "VDI00382")    # Fjernskrivebord
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}
if(Sys.info()['nodename'] == "C2210098")     # Peters nye maskine
{
  o_dir = "C:/Users/B007566/Documents/Output"  
}

d0 = read.delim(paste0(o_dir,"/sectors_year.txt"))


#-------------------------------

d = d0 %>% filter(Sector==0)
plot(d$Year, d$Price/d$PriceTotal, type="l", ylim=c(0,2))
for(i in 1:9)
{
  d = d0 %>% filter(Sector==i)
  lines(d$Year, d$Price/d$PriceTotal)
  
}

d = d0 %>% filter(Sector==0)
plot(d$Year, d$Wage/d$WageTotal, type="l", ylim=c(0,2))
for(i in 1:9)
{
  d = d0 %>% filter(Sector==i)
  lines(d$Year, d$Wage/d$WageTotal)
  
}

d = d0 %>% filter(Sector==0)
plot(d$Year, d$Employment, type="l")
for(i in 1:9)
{
  d = d0 %>% filter(Sector==i)
  lines(d$Year, d$Employment)
  
}

d = d0 %>% filter(Sector==0)
plot(d$Year, d$nFirm, type="l")
for(i in 1:9)
{
  d = d0 %>% filter(Sector==i)
  lines(d$Year, d$nFirm)
  
}


