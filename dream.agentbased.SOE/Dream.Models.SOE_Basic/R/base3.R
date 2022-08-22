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

files = list.files(paste0(o_dir, "\\Scenarios"), full.names = T)

d = read.delim(files[1])
for(i in 2:length(files))
{
  d = rbind(d, read.delim(files[i]))
}

d = d[!is.na(d$marketWage),]
d$Employment = as.numeric(d$Employment)

yr0 = 12*(2100-2014)-1
d = d %>% filter(Time>yr0)
d$Time = d$Time - yr0 
d$Year = floor(d$Time/12)


d = d %>% arrange(Scenario)
ids=unique(d$Scenario)
n = length(ids)

ss=unique(d$Run)
n_ss = length(ss)

db = d %>% filter(Run=="Base") 


mx = max(db$Employment)
mn = min(db$Employment)
d = db %>% filter(Scenario==ids[1]) %>% arrange(Time)
plot(d$Time/12, d$Employment, type="l", col=rgb(0,0,0,0.5), ylim=c(mn,mx))
lines(d$Time/12, hpfilter(d$Employment, 900000))

d = db %>% filter(Scenario==ids[3]) %>% arrange(Time)
lines(d$Time/12, d$Employment, col=rgb(0,0,0,0.5))


for(i in 2:n)
{
  d = db %>% filter(Scenario==ids[i]) %>% arrange(Time)
  lines(d$Time/12, d$Employment, col=rgb(0,0,0,0.05))
}




