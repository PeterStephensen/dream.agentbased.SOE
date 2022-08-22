rm(list=ls())
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
if(Sys.info()['nodename'] == "VDI00382")    # Fjernskrivebord for agentbased projekt
{
  o_dir = "C:/Users/B007566/Documents/Output"  
  o_dir_svg = "H:/AgentBased/SOE/Graphics"
}

d_prod = read.delim(paste0(o_dir,"/data_firms.txt"))



files = list.files(paste0(o_dir, "\\Scenarios"), full.names = T)

d0 = read.delim(files[1])
for(i in 2:length(files))
{
  #i=2
  cat(paste(files[i],":\t",i, "out of", length(files), "\n"))
  d0 = rbind(d0, read.delim(files[i]))
}


yr0 = 12*(2150-2014)-1
#yr0 = 12*(2100-2014)-1

d = d0 %>% filter(Time>yr0)
d$Time = d$Time - yr0 

d = d %>% arrange(Scenario)
ids=unique(d$Scenario)
n = length(ids)

ss=unique(d$Run)
n_ss = length(ss)

hpfilter      = function(x, mu = 100) {
  y = x
  n <- length(y)          # number of observations
  I <- diag(n)            # creates an identity matrix
  D <- diff(I,lag=1,d=2)  # second order differences
  d <- solve(I + mu * crossprod(D) , y) # solves focs
  d
}


#-------------------------
dd = d[is.na(d$marketWage),]
b_ids=unique(dd$Scenario)

d = d %>% filter(!(Scenario %in% b_ids)) 

d$Employment = as.numeric(d$Employment)

#-------------------------

# SPECIELT!!!!!!!!!!!!!!!!!!!!!!!!!!!
#-----------------------------------
#d$Employment = as.numeric(d$Employment)
#d = d[!is.na(d$Employment),]
#d = d %>% filter(Scenario!=101) 
#-----------------------------------


db = d %>% filter(Run=="Base") 

#----------------------------------------------------


#----------------------------------------------------

#bcol = rgb(0.5,0.8,0.5)
bcol = rgb(0.9,0.9,0.9)

lcol = rgb(0.3,0.5,0.99)
lwd = 1.0


lo = function(x)
{
  z = sort(x)
  n = length(z)
  i = floor(0.025*n)
  d = 0.025*n - i
  if(i==0)
  {
    return(d*z[1])
  }
  else
  {
    return((1-d) * z[i] + d*z[i+1])
  }
}

up = function(x)
{
  z = sort(x, decreasing = T)
  n = length(z)
  i = floor(0.025*n)
  d = 0.025*n - i
  if(i==0)
  {
    return(d*z[1])
  }
  else
  {
    return((1-d) * z[i] + d*z[i+1])
  }
}

pplot = function(zz, sMain, ylab="Relative change", 
                 s_ylim=c(1,1), abs_ylim=c(0,0), type="l", 
                 pch=1, cex=1, lwd=1, zero=F, col=0, horzLine=0)
{
  
  if(col==0)
    col = rgb(0.9,0.9,0.9)
  
  xcol = rgb(0.5,0.7,0.7)
  mx=max(zz$up)
  mn=min(zz$lo)
  if(zero)
    if(mn>0)
      mn=0
  if(zero)
    if(mx<0)
      mx=0
  
  if(abs_ylim==c(0,0))
  {
    ylim=c(s_ylim[1]*mn,s_ylim[2]*mx)
  }else
  {
    ylim=abs_ylim
  }
  
  plot(zz$Time/12, (zz$up), type="l", col=bcol, ylim=ylim, xlab="Year", cex.axis=0.8, 
       ylab=ylab, main=paste(sMain), cex.main=1)
  box(col=col)
  axis(1, col=col, col.ticks=xcol, cex.axis=0.8)
  axis(2, col=col, col.ticks=xcol, cex.axis=0.8)
  lines(zz$Time/12, zz$lo, col=col)
  polygon(c(zz$Time/12, rev(zz$Time/12)), c(zz$lo, rev(zz$up)), col=bcol, lty=0)
  abline(h=0, col=xcol)
  if(horzLine!=0)
    abline(h=horzLine, col=xcol, lty=2) 
  lines(zz$Time/12, zz$mean, lwd=lwd, col=lcol, type=type, pch=pch, cex=cex)
  
}

vert_lin = function(t)
{
  for(v in seq(from=0, to=max(t), by=10))
    abline(v=v, col="gray")
}

pplot2 = function(t,x, main="", s_miny=0, s_maxy=1.2)
{
  plot(t, x, type="l", main=main, ylim=max(x)*c(s_miny,s_maxy), ylab="", xlab="Year")
  abline(h=0)
  vert_lin(t)
  lines(t, x)
  
}

if(F)
{
  jj=1
  jj=jj+1
  ddd = db %>% filter(Scenario==ids[jj])
  
  
  ddd$u = ddd$nUnemployed / ddd$nLaborSupply
  ddd$w_infl = (ddd$marketWage / lag(ddd$marketWage) - 1) 
  
  plot(ddd$u, ddd$w_infl)
  (lm1 = lm(u ~ w_infl, data=ddd))
  

  #-----
  ddd = db %>% filter(Scenario==ids[1])
  ddd$u = ddd$nUnemployed / ddd$nLaborSupply
  ddd$w_infl = (ddd$marketWage / lag(ddd$marketWage) - 1) 
  plot(ddd$u, ddd$w_infl, col=rgb(0,0,0,0.025), ylim=c(-0.007, -0.001), xlim=c(0.053, 0.08),
       main="Philips curve", xlab="Rate of unemplyment", ylab="Rate of wage inflation")
  
  
  for(jj in c(2:length(ids)))
  {
    ddd = db %>% filter(Scenario==ids[jj])
    
    ddd$u = ddd$nUnemployed / ddd$nLaborSupply
    ddd$w_infl = (ddd$marketWage / lag(ddd$marketWage) - 1) 
    
    lines(ddd$u, ddd$w_infl, type="p", col=rgb(0,0,0,0.025))
    
  }
  
}





#output="pdf"
output="svg"

#----------------------------------------------------
lwd=0.5
i=1
if(output=="pdf")
  pdf(paste0(o_dir, "/base.pdf"))

if(output=="svg")
  svg(paste0(o_dir_svg, "/base1.svg"))

par(mfrow=c(2,2))

shk = 1
#r_col = rgb(1,0.3,0.3)
r_col = rgb(0.9,0.9,0.2)

#------------------------
db$u = db$nUnemployed/db$nLaborSupply
d4 = db %>% filter(Scenario==ids[i])
#i=i+1
#plot(d4$Time/12, d4$expSharpeRatio, type="l", main="Sharpe Ratio", xlab="Year", ylab="", ylim=c(-0.1, 0.1))
#abline(h=0)
#vert_lin(d4$Time/12)

plot.frequency.spectrum <- function(X.k, xlimits=c(0,length(X.k))) {
  plot.data  <- cbind(0:(length(X.k)-1), Mod(X.k))
  
  # TODO: why this scaling is necessary?
  plot.data[2:length(X.k),2] <- 2*plot.data[2:length(X.k),2] 
  
  #plot(plot.data, t="l", lwd=2, main="", 
  #     xlab="Frequency (Hz)", ylab="Strength", 
  #     xlim=xlimits)

  plot(plot.data, t="l", lwd=2, main="", 
       xlab="Frequency (Hz)", ylab="Strength", 
       xlim=xlimits, ylim=c(0,max(Mod(plot.data[,2]))))
}
plot.frequency.spectrum2 <- function(X.k, xlimits=c(0,length(X.k))) {
  plot.data  <- cbind(0:(length(X.k)-1), Mod(X.k))
  
  # TODO: why this scaling is necessary?
  plot.data[2:length(X.k),2] <- 2*plot.data[2:length(X.k),2] 
  plot.data[,2] <- 1/plot.data[,2] 
  plot.data[,1] <- plot.data[,1]/12 
  
  #plot(plot.data, t="l", lwd=2, main="", 
  #     xlab="Frequency (Hz)", ylab="Strength", 
  #     xlim=xlimits)
  
  plot(plot.data, t="h", lwd=2, main="")
}

#z = fft(d4$SharpeRatio)
#plot.frequency.spectrum2(z, xlimits = c(0,100*12))
#abline(h=0)

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(expSharpeRatio), up=up(expSharpeRatio), lo=lo(expSharpeRatio))

pplot(zz, "A. Sharpe Ratio", ylab = "", abs_ylim = c(-0.05, 0.05))
lines(d4$Time/12, d4$expSharpeRatio, lwd=lwd)

#lines(dd2$Time/12, dd2$expSharpeRatio, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$expSharpeRatio, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

#pplot2(d4$Time/12, d4$nFirms, main="Number of firms", s_miny = 0.8, s_maxy = 1.05)

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(nFirms), up=up(nFirms), lo=lo(nFirms))

pplot(zz, "B. Number of firms", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$nFirms, lwd=lwd)
#lines(dd2$Time/12, dd2$nFirms, lwd=0.5, col=rgb(0.7,0.7,0.7))


#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(Employment), up=up(Employment), lo=lo(Employment))

pplot(zz, "C. Employment", ylab = "", s_ylim = c(0.97, 1.03))
lines(d4$Time/12, d4$Employment, lwd=lwd)
#lines(dd2$Time/12, dd2$Employment, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(u), up=up(u), lo=lo(u))

pplot(zz, "D. Rate of unemployment", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$u, lwd=lwd)
#lines(dd2$Time/12, dd2$Employment, lwd=0.5, col=rgb(0.7,0.7,0.7))

if(output=="svg")
{
  dev.off()
  svg(paste0(o_dir_svg, "/base2.svg"))
  par(mfrow=c(2,2))
}  

###########################################


#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(Sales), up=up(Sales), lo=lo(Sales))

pplot(zz, "A. Sales", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$Sales, lwd=lwd)
#lines(dd2$Time/12, dd2$Sales, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(marketWage/marketPrice), up=up(marketWage/marketPrice), lo=lo(marketWage/marketPrice))

pplot(zz, "B. Real wage", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$marketWage / d4$marketPrice, lwd=lwd)
#lines(dd2$Time/12, dd2$marketWage / dd2$marketPrice, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------
g = (1 + 0.02)^(1/12) - 1
corr = (1+g)^(d4$Time-1)

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(Sales), up=up(Sales), lo=lo(Sales))
zz[,2:4] = zz[,2:4] / corr

pplot(zz, "C. Sales (detrended 2% p.a.)", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$Sales/corr, lwd=lwd)
#lines(d4$Time/12, as.numeric(d4$Production)/corr, lwd=lwd, col=rgb(1,0,0))

#plot(as.numeric(d4$Production) / d4$Sales-1, type="l")
#lines(d4$nUnemployed/d4$nLaborSupply, col="red")
#lines(d4$SharpeRatio+0.06, col="blue")

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(marketWage/marketPrice), up=up(marketWage/marketPrice), lo=lo(marketWage/marketPrice))
zz[,2:4] = zz[,2:4] / corr

pplot(zz, "D. Real wage (detrended 2% p.a.)", ylab = "", s_ylim = c(0.9, 1.1))
lines(d4$Time/12, d4$marketWage / d4$marketPrice / corr, lwd=lwd)

dev.off()

###########################################


#------------------------

if(F)
{
  zz = db %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(marketPrice), up=up(marketPrice), lo=lo(marketPrice))

  pplot(zz, "Price", ylab = "", s_ylim = c(0.9, 1.1))
  lines(d4$Time/12, d4$marketPrice)
  #lines(dd2$Time/12, dd2$marketPrice, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

  zz = db %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(marketWage), up=up(marketWage), lo=lo(marketWage))

  pplot(zz, "Wage", ylab = "", s_ylim = c(0.9, 1.1))
  lines(d4$Time/12, d4$marketWage)
  #lines(dd2$Time/12, dd2$marketWage, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

  #dev.off()

}



#----------------------------------------------------
if(F)
{
pdf(paste0(o_dir, "/base_rel.pdf"))

shk = 1
#r_col = rgb(1,0.3,0.3)
r_col = rgb(0.9,0.9,0.2)


#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(expSharpeRatio), up=up(expSharpeRatio)-mean, lo=lo(expSharpeRatio)-mean, mean=0)

pplot(zz, "Sharpe Ratio", ylab = "", s_ylim = c(1.6, 1.6))
#lines(dd2$Time/12, dd2$expSharpeRatio, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$expSharpeRatio, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(nFirms), up=up(nFirms)/mean, lo=lo(nFirms)/mean, mean=1)

pplot(zz, "Number of firms", ylab = "", s_ylim = c(0.9, 1.1))
#lines(dd2$Time/12, dd2$nFirms, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$nFirms, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(Sales), up=up(Sales)/mean, lo=lo(Sales)/mean, mean=1)

pplot(zz, "Sales", ylab = "", s_ylim = c(0.9, 1.1))
#lines(dd2$Time/12, dd2$Sales, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$Sales, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(Employment), up=up(Employment)/mean, lo=lo(Employment)/mean, mean=1)

pplot(zz, "Employment", ylab = "", s_ylim = c(0.95, 1.05))
#lines(dd2$Time/12, dd2$Employment, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$Employment, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(marketWage/marketPrice), up=up(marketWage/marketPrice)/mean, lo=lo(marketWage/marketPrice)/mean, mean=1)

pplot(zz, "Real wage", ylab = "", s_ylim = c(0.9, 1.1))
#lines(dd2$Time/12, dd2$marketWage / dd2$marketPrice, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$marketWage / dd2$marketPrice, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(marketPrice), up=up(marketPrice)/mean, lo=lo(marketPrice)/mean, mean=1)

pplot(zz, "Price", ylab = "", s_ylim = c(0.9, 1.1))
#lines(dd2$Time/12, dd2$marketPrice, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$marketPrice, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

zz = db %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(marketWage), up=up(marketWage)/mean, lo=lo(marketWage)/mean, mean=1)

pplot(zz, "Wage", ylab = "", s_ylim = c(0.9, 1.1))
#lines(dd2$Time/12, dd2$marketWage, lwd=2, col=r_col)
#lines(dd2$Time/12, dd2$marketWage, lwd=0.5, col=rgb(0.7,0.7,0.7))

#------------------------

dev.off()
}

#----------------------------------------------------
#output="svg"
if(output=="pdf")
{
  pdf(paste0(o_dir, "/shocks.pdf"))
  par(mfrow=c(3,3))
}


nSvg = c("", "/shock1.svg", "/shock2.svg")
shkYr = c(0, 10, 10)



for(shk in 2:n_ss)
{
  if(output=="svg")
  {
    svg(paste0(o_dir_svg, nSvg[shk]))
    par(mfrow=c(3,3))
  }
  
  #shk=2
  
  dc = d %>% filter(Run==ss[shk]) 
  
  dd = merge(dc, db, by=c("Scenario", "Time"))

  dd = dd %>% filter(Time<shkYr[shk]*12)
  
  dd$dnFirms = dd$nFirms.x / dd$nFirms.y - 1 
  dd$dSales = dd$Sales.x  / dd$Sales.y - 1 
  dd$dProduction = as.numeric(dd$Production.x)  / as.numeric(dd$Production.y) - 1 
  dd$dEmployment = as.numeric(dd$Employment.x)  / as.numeric(dd$Employment.y) - 1 
  dd$dRealWage = (dd$marketWage.x  / dd$marketPrice.x ) / (dd$marketWage.y / dd$marketPrice.y) - 1 
  dd$dexpSharpeRatio = dd$expSharpeRatio.x  - dd$expSharpeRatio.y 
  dd$dmarketWage = dd$marketWage.x / dd$marketWage.y - 1 
  dd$dmarketPrice = dd$marketPrice.x / dd$marketPrice.y - 1 
  dd$dnFirmClosed = dd$nFirmClosed.x  / dd$nFirmClosed.y - 1  
  dd$dnFirmNew = dd$nFirmNew.x  / dd$nFirmNew.y   - 1
  dd$dVacancies = dd$Vacancies.x / dd$Vacancies.y - 1 
  
  dd = dd[dd$dnFirms!=0,] # !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  

  if(F)
  {
    zids=unique(dd$Scenario)
    zn = length(ids)
    
    d2 = dd %>% filter(Scenario==zids[1]) %>% arrange(Time)
    plot(d2$Time/12, d2$dnFirms, type="l")
    for(i in 2:zn)
    {
      d2 = dd %>% filter(Scenario==zids[i]) %>% arrange(Time)
      lines(d2$Time/12, d2$dnFirms)
    }
  }

  
  #-----------------
  
  if(F)
  {
    zz = dd %>% group_by(Time) %>%
      dplyr::summarize(mean=mean(dnFirmNew), up=up(dnFirmNew), lo=lo(dnFirmNew))
    
    zz2 = dd %>% group_by(Time) %>%
      dplyr::summarize(mean=mean(dnFirmClosed), up=up(dnFirmClosed), lo=lo(dnFirmClosed))
    
    plot(zz$Time/12,  zz$mean, type="l", ylim=c(-0.15, 0.1), xlab="Years", ylab="Relative change")
    lines(zz2$Time/12, zz2$mean, col="red", type= "p")  
    abline(h=0, v=0)
    
    #pplot(zz2, "New firms", type="p", pch=20, cex=0.5)
    
  }

  #-----------------
  
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dnFirms), up=up(dnFirms), lo=lo(dnFirms))

  pplot(zz, "A1. Number of firms", type="p", pch=20, cex=0.5)
  
  #-----------------
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dexpSharpeRatio, na.rm = T), up=up(dexpSharpeRatio), lo=lo(dexpSharpeRatio))
  
  pplot(zz, "A2. Expected Sharpe Ratio", type="p", pch=20, cex=0.5, ylab = "Absolute change")
  
  #-----------------
  
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dEmployment, na.rm = T), up=up(dEmployment), lo=lo(dEmployment))
  
  pplot(zz, "A3. Employment", type="p", pch=20, cex=0.5)
  
  
  #-----------------
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dSales, na.rm = T), up=up(dSales), lo=lo(dSales))
  
  
  
  pplot(zz, "B1.Sales", type="p", pch=20, cex=0.5)
  
  #-----------------
  
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dProduction, na.rm = T), up=up(dProduction), lo=lo(dProduction))

  zz2 = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dSales, na.rm = T), up=up(dSales), lo=lo(dSales))
  
    
  pplot(zz, "B2. Supply and Sales (Red)", type="p", pch=20, cex=0.5, zero = T)
  #lines(zz2$Time/12, zz2$mean, type="l", lwd=0.8, lty=3)
  lines(zz2$Time/12, zz2$mean, type="p", cex=0.25, col=rgb(1,0,0), lwd=0.5)
  
  #-----------------
  
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dVacancies, na.rm = T), up=up(dVacancies), lo=lo(dVacancies))
  
  pplot(zz, "B3. Vacancies", type="p", pch=20, cex=0.5)
  


  #-----------------
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dRealWage, na.rm = T), up=up(dRealWage), lo=lo(dRealWage))
  
  pplot(zz, "C1. Real wage", type="p", pch=20, cex=0.5, zero = T)
  
  #-----------------
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dmarketWage, na.rm = T), up=up(dmarketWage), lo=lo(dmarketWage))
  
  pplot(zz, "C2. Wage", type="p", pch=20, cex=0.5, zero = T)
  
  #-----------------
  zz = dd %>% group_by(Time) %>%
    dplyr::summarize(mean=mean(dmarketPrice, na.rm = T), up=up(dmarketPrice), lo=lo(dmarketPrice))
  
  pplot(zz, "C3. Price", type="p", pch=20, cex=0.5, zero = T)
  
  
    #-----------------
  #zz = dd %>% group_by(Time) %>%
  #  dplyr::summarize(mean=mean(dnFirmClosed, na.rm = T), up=up(nFirmClosed), lo=lo(nFirmClosed))
  
  #pplot(zz, "Firm Closed", type="p", pch=20, cex=0.5, zero = T)
  
  #-----------------

  if(output=="svg")
    dev.off()
  
}

if(output=="pdf")
  dev.off()


#--------------------------

svg(paste0(o_dir_svg, "/NewClosed.svg"), width = 10, height = 7)
par(mfrow=c(1,2))

shk = 2
dc = d %>% filter(Run==ss[shk]) 

dd = merge(dc, db, by=c("Scenario", "Time"))

dd = dd %>% filter(Time<10*12)

dd$dnFirms = dd$nFirms.x / dd$nFirms.y - 1 
dd$dnFirmClosed = dd$nFirmClosed.x  / dd$nFirmClosed.y - 1  
dd$dnFirmNew = dd$nFirmNew.x  / dd$nFirmNew.y   - 1

dd = dd[dd$dnFirms!=0,] # !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

zz = dd %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(dnFirmNew), up=up(dnFirmNew), lo=lo(dnFirmNew))

zz2 = dd %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(dnFirmClosed), up=up(dnFirmClosed), lo=lo(dnFirmClosed))

plot(zz$Time/12,  zz$mean, type="l", ylim=c(-0.15, 0.1), xlab="Years"
     , ylab="Relative change", main="Productivity shock", col=rgb(0,0.5,0.9), lwd=3)
lines(zz2$Time/12, zz2$mean, col=rgb(0.9,0.25,0), type= "p", pch=20)  
abline(h=0, v=0)
legend("topright", legend=c("New firms", "Closed firms"),
       col=c(rgb(0,0.5,0.9), rgb(0.9,0.25,0)), lwd=3, lty=1:1, cex=1)


#----


shk = 3
dc = d %>% filter(Run==ss[shk]) 

dd = merge(dc, db, by=c("Scenario", "Time"))

dd = dd %>% filter(Time<10*12)

dd$dnFirms = dd$nFirms.x / dd$nFirms.y - 1 
dd$dnFirmClosed = dd$nFirmClosed.x  / dd$nFirmClosed.y - 1  
dd$dnFirmNew = dd$nFirmNew.x  / dd$nFirmNew.y   - 1

dd = dd[dd$dnFirms!=0,] # !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

zz = dd %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(dnFirmNew), up=up(dnFirmNew), lo=lo(dnFirmNew))

zz2 = dd %>% group_by(Time) %>%
  dplyr::summarize(mean=mean(dnFirmClosed), up=up(dnFirmClosed), lo=lo(dnFirmClosed))

plot(zz$Time/12,  zz$mean, type="l", ylim=c(-0.15, 0.1), xlab="Years"
     , ylab="Relative change", main="Firm destruction", col=rgb(0,0.5,0.9), lwd=3)
lines(zz2$Time/12, zz2$mean, col=rgb(0.9,0.25,0), type= "p", pch=20)  
abline(h=0, v=0)
legend("topright", legend=c("New firms", "Closed firms"),
       col=c(rgb(0,0.5,0.9), rgb(0.9,0.25,0)), lwd=3, lty=1:1, cex=1)

dev.off()








#------------------------------------------------

d4 = db %>% filter(Scenario==ids[1])

mard = function(x)
{
  m = mean(x)
  mean(abs(x/m-1))
}

mad = function(x)
{
  m = mean(x)
  mean(abs(x-m))
}


g = (1 + 0.02)^(1/12) - 1
corr = (1+g)^(d4$Time-1)

mad(d4$expSharpeRatio)
mard(d4$nFirms)
mard(d4$Employment)
mard(d4$Sales/corr)
mard((d4$marketWage/d$marketPrice)/corr)


library(forecast)

a = auto.arima(d4$SharpeRatio)
summary(a)

a = auto.arima(d4$expSharpeRatio)
summary(a)

a = auto.arima(log(d4$nFirms))
summary(a)

a = auto.arima(log(d4$Employment))
summary(a)

a = auto.arima(log(d4$Sales))
summary(a)

a = auto.arima(log(d4$marketWage/d4$marketPrice/corr))
summary(a)

plot(d4$expSharpeRatio-mean(d4$expSharpeRatio), type="l", ylim=c(-0.05,0.05))
lines(log(d4$nFirms)-mean(log(d4$nFirms)), type="l")
lines(log(d4$Employment)-mean(log(d4$Employment)), type="l")

z = log(d4$marketWage/d4$marketPrice/corr)

z = z - mean(z)
plot(z, type="l")
lines(hpfilter(z, mu = 14400), lwd=2 )
abline(h=0)

plot(hpfilter(z, mu = 14400), type="l")

i=0
i=i+1
d4 = db %>% filter(Scenario==ids[i])
yy = d4$Employment
lambda = BoxCox.lambda(yy)
z = BoxCox(yy, lambda = lambda)
a = auto.arima(z)
lambda
summary(a)
















