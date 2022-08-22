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

if(F)
{
  dd10 = read.delim(paste0(o_dir,"/output.txt"))
  mean(dd10$DiscountedProfits)
  
  plot(dd10$DiscountedProfits)
  abline(h=0)
  abline(h=mean(dd10$DiscountedProfits), lty=2)
  
  dd2 = read.delim(paste0(o_dir,"/output_2.txt"))
  hist(dd2$n_firms, breaks = 70, xlim=c(0,400))
  hist(dd10$n_firms, breaks = 70, xlim=c(0,400))
  
  hist(dd2$Wage/dd2$Price, breaks = 50, xlim=c(0,0.2), ylim=c(0,85))
  hist(dd10$Wage/dd10$Price, breaks = 30, xlim=c(0,0.2), ylim=c(0,85))
  
}

hpfilter      = function(x, mu = 100) {
  y = x
  n <- length(y)          # number of observations
  I <- diag(n)            # creates an identity matrix
  D <- diff(I,lag=1,d=2)  # second order differences
  d <- solve(I + mu * crossprod(D) , y) # solves focs
  d
}

d = read.delim(paste0(o_dir,"/data_year.txt"))
d_house = read.delim(paste0(o_dir,"/data_households.txt"))
d_prod = read.delim(paste0(o_dir,"/data_firms.txt"))


y0 = 2014
burnIn = 2035

alpha = 0.5
fi = 2
k = 2.5

g = (1 + 0.02)^(1/12) - 1

l_bar = k * (fi/(1-alpha))^(1/alpha) /(k - (1/(1-alpha)))

pdf(paste0(o_dir,"/graph1.pdf"))

par(mfrow=c(3,3))

yr = last(d$Year)
if(yr < 2020)
{
  mx_yr = 2020
} else if(yr<2025)
{
  mx_yr = 2025
} else if(yr<2050)
{
  mx_yr = 2050
} else if(yr<2100)
{
  mx_yr = 2100
} else if(yr<2200)
{
  mx_yr = 2200
} else if(yr<2300)
{
  mx_yr = 2300
} else
{
  mx_yr = 2400
}


if(yr > 2100)
{
  y0 = 2075
  d = d %>% filter(Year>y0)
}  
x1=6+36/60
x2=5+54/60
x3=5+15/60

1-x3/x2


corr = (1 + 0.02)^(d$Year - y0)

cols=palette()

n_households = last(d$n_Households)
n_firms = n_households/l_bar

 
hist(d_prod$Productivity, breaks = 50, xlab="Firm Productivity", main=paste("Year:",yr), col=cols[3])

mx = max(max(d_prod$OptimalEmployment), max(d_prod$Employment))
plot(d_prod$OptimalEmployment, d_prod$Employment, xlab="Optimal employment", ylab="Employment", 
     log = "xy", col=cols[3], xlim=c(1,1.1*mx), ylim=c(1,1.1*mx))
abline(a=0,b=1, lty=2)

mx = max(max(d_prod$OptimalProduction), max(d_prod$Sales))
plot(d_prod$OptimalProduction, d_prod$Sales, xlab="Optimal production", ylab="Sales", 
     log = "xy", col=cols[3], xlim=c(0.001,1.1*mx), ylim=c(0.001,1.1*mx))
abline(a=0,b=1, lty=2)

mx = max(d$nUnemployed/d$LaborSupply)
if(last(d$nUnemployed/d$LaborSupply) < 0.2)
{
  mx = 0.2
}

plot(d$Year, d$nUnemployed/d$LaborSupply, main="Unemployment rate", xlab = "year", ylab="", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,mx), cex.main=0.8)
abline(h=0)
abline(v=burnIn, lty=2)



mx = max(d$nOptimalEmplotment)
plot(d$Year, d$nOptimalEmplotment, main="Total Optimal Employment", xlab = "year", ylab="Optimal Employment", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,1.1*mx), cex.main=0.8)
abline(h=n_households, lty=2)
abline(v=burnIn, lty=2)

z=d_prod$Profit<500
z=d_prod$Profit[z]>-100
hist(d_prod$Profit[z], breaks = 50, xlab="Profit", main="", col=cols[3])

mx = max(d$nVacancies/d$LaborSupply)
if(last(d$nVacancies/d$LaborSupply) < 0.2)
{
  mx = 0.2
}
plot(d$Year, d$nVacancies/d$LaborSupply, main="Vacancies rate", xlab = "year", ylab="", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,mx), cex.main=0.8)
abline(h=0)
abline(v=burnIn, lty=2)


mx = max(d$Wage / d$Price / corr)
plot(d$Year, d$Wage / d$Price / corr, main="", xlab = "year", ylab="Wage / Price", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,1.1*mx))
abline(h=0)
abline(v=burnIn, lty=2)

mx = max(d$Sales / corr)
plot(d$Year, d$Sales / corr, main="Sales", xlab = "year", ylab="sales", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,1.1*mx), cex.main=0.8)
abline(h=0)
abline(v=burnIn, lty=2)


#-------
# Page 2
#-------


mx = max(d$Wage)
plot(d$Year, d$Wage, xlab = "year", ylab="Wage", ylim=c(0,1.05*mx), 
     type="l", xlim=c(y0,mx_yr), main=paste("Year:",yr))
abline(h=0)
abline(v=burnIn, lty=2)

mx = max(d$Price)
plot(d$Year, d$Price, main="", xlab = "year", ylab="Price", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,1.05*mx))
abline(h=0)
abline(v=burnIn, lty=2)

mx = max(d$n_Households)
plot(d$Year, d$n_Households, main="", xlab = "year", ylab="#Households", 
     type="l", xlim=c(y0,mx_yr), ylim=c(0,1.05*mx))
abline(h=0)
lines(d$Year, d$LaborSupply, lty=2)

hist(d_house$Age/12, breaks = 100, xlab="Houshold age", main="", xlim=c(18,100), col=cols[3])



#plot(d$Year, d$ProfitPerHousehold/d$Price, main="", xlab = "year", ylab="ProfitHoush / Price", 
#     type="l", xlim=c(y0,mx_yr))
#abline(h=0)
#abline(v=burnIn, lty=2)

#plot(d$Year, d$MeanValue, main="", type="l", xlim=c(y0,mx_yr), ylim=c(0, 1.1*max(d$MeanValue)), 
#     ylab="MeanValue/Price", xlab="year")
#abline(h=0)
#abline(h=40, lty=2)
#abline(v=burnIn, lty=2)


plot(d$Year, d$MeanAge / 12, main="", type="l", xlim=c(y0,mx_yr), ylim=c(0, 1.1*max(d$MeanAge/12)), ylab="Mean firm age", xlab="year")
abline(h=0)
abline(v=burnIn, lty=2)

if(yr>burnIn)
{
  dd = d %>% filter(Year>burnIn)
  x_mx = max(dd$nUnemployed/dd$n_Households) 
  y_mx = max(dd$nVacancies/dd$n_Households)
  plot(dd$nUnemployed/dd$n_Households, dd$nVacancies/dd$n_Households, main="Beveridge Curve", 
       xlab="U-rate", ylab="V-rate", type="p", xlim=c(0,1.1*x_mx), ylim=c(0,1.1*y_mx), pch=19, cex=0.3)
  abline(h=0,v=0)
  
}else
{
  plot(0)
}

if(yr>burnIn)
{
  dd = d %>% filter(Year>burnIn)
  x_mx = max(dd$nUnemployed/dd$n_Households) 
  y_mx = max(d$Wage/lag(d$Wage)-1, na.rm = T)
  y_mn = min(d$Wage/lag(d$Wage)-1, na.rm = T)
  plot(dd$nUnemployed/dd$n_Households, dd$Wage/lag(dd$Wage)-1, main="Philips Curve", 
       xlab="U-rate", ylab="W-growth", type="p", xlim=c(0,1.1*x_mx), ylim=c(1.1*y_mn,1.1*y_mx), pch=19, cex=0.3)
  abline(h=0,v=0)
  
}else
{
  plot(0)
}


z = d$Sales / d$nEmployment / corr
plot(d$Year, z, main="", type="l", xlim=c(y0,mx_yr), ylim=c(0, 1.1*max(z)), ylab="Productivity", xlab="year")
abline(h=0)
abline(v=burnIn, lty=2)

z = d_house$Productivity
z = z[z>0]
hist(z, breaks = 100, xlab="Household productivity", main="", xlim=c(0,3), col=cols[3])
abline(v=0, h=0)


#plot(d$Year, d$ProfitPerFirm, main="", xlab = "year", ylab="ProfitFirm", 
#     type="l", xlim=c(y0,mx_yr))
#abline(h=0)
#abline(v=burnIn, lty=2)

#plot(d$Year, d$ProfitPerFirm/d$Price, main="", xlab = "year", ylab="ProfitFirm / Price", 
#     type="l", xlim=c(y0,mx_yr))
#abline(h=0)
#abline(v=burnIn, lty=2)

#-------
# Page 3a
#-------
par(mfrow=c(3,1))

plot(d$Year, d$nFirms, main="", type="l", xlim=c(y0,mx_yr), ylim=c(0, 1.1*max(d$nFirms)), ylab="Stock of firms", xlab="year")
abline(h=0)
abline(v=burnIn, lty=2)
abline(h=n_firms, lty=2)

z = d$nFirmCloseNegativeProfit + d$nFirmCloseNatural + d$nFirmCloseTooBig  
mx = max(max(z), max(d$nFirmNew))
plot(d$Year, z, xlab = "year", ylab="Flow of firms", ylim=c(0, 1.1*mx), 
     type="l", xlim=c(y0,mx_yr), main="", cex.main=0.7)
lines(d$Year, d$nFirmCloseNatural, col=cols[2], type="l")
lines(d$Year, d$nFirmCloseTooBig, col=cols[3], type="s")
abline(h=120, lty=2)
lines(d$Year, d$nFirmNew, col=cols[4], type="l")
abline(h=0)
abline(v=burnIn, lty=2)
abline(v=2050, lty=2)
ContourFunctions::multicolor.title(c("Closed:Total  ","Closed:Natural  ", "Closed:TooBig  ", "New"), 1:4, cex.main = 0.7)


plot(d$Year, d$SharpeRatio, main="", xlab = "year", ylab="SharpeRatio", 
     type="l", xlim=c(y0,mx_yr), col=cols[3])
lines(d$Year, d$ExpSharpRatio, col=cols[4])
abline(h=0)
abline(v=burnIn, lty=2)
ContourFunctions::multicolor.title(c("SharpeRatio  ","Expected SharpeRatio"), 3:4, cex.main = 0.7)


#plot(d$Year, d$DiscountedProfits / d$nFirms, main="", xlab = "year", ylab="Discounted Profits per firm", 
#     type="l", xlim=c(y0,mx_yr))
#lines(d$Year, d$ExpDiscountedProfits / d$nFirms, lty=2)
#abline(h=0)
#abline(v=burnIn, lty=2)


#-------
# Page 4
#-------
par(mfrow=c(3,3))

hist(d_prod$Age / 12, breaks = 50, xlab="Firm Age (years)", main=paste("Year:",yr))


plot(d_prod$Productivity, d_prod$Profit, xlab="Productivity",ylab="Profit", pch=19, cex=0.3)
abline(h=0)

barplot(log(table(d_house$UnemplDuration)), xlab="UnemplDuration (months)", ylab="log(Antal)")

plot(d_prod$Productivity, d_prod$Age/12, xlab="Productivity",ylab="Age (year)",
     pch=19, cex=0.2)
abline(h=0)
abline(v=0.5, lty=2)


plot(d_prod$Age/12, d_prod$Profit, xlab="Age (year)",ylab="Profit",
     pch=19, cex=0.2, xlim=c(0,50))
abline(h=0)

plot(d_prod$Age/12, d_prod$DiscountedProfits, xlab="Age (year)",ylab="Discounted Profit",
     pch=19, cex=0.2, xlim=c(0,50))
abline(h=0)
abline(h=mean(d_prod$DiscountedProfits), lty=2, col="red")
abline(h=median(d_prod$DiscountedProfits), lty=2, col="blue")

plot(d_prod$Age/12, d_prod$DiscountedProfits, xlab="Age (year)",ylab="Discounted Profit",
     pch=19, cex=0.2, xlim=c(0,5))
abline(h=0)
abline(h=mean(d_prod$DiscountedProfits), lty=2, col="red")
abline(h=median(d_prod$DiscountedProfits), lty=2, col="blue")


hist(d_prod$DiscountedProfits, breaks = 50, xlab="Discounted profits", main="")

dd = d_house[sample(1:nrow(d_house),1000), ]
plot(dd$Age/12, dd$Productivity,pch=19, cex=0.1, xlab="Age (years)", ylab="Productivity", ylim=c(0,5))

#par(mfrow=c(1,1))

#-----------------------------------------------
par(mfrow=c(2,2))

#z = (d$nFirmCloseNegativeProfit + d$nFirmCloseNatural + d$nFirmCloseTooBig) / (12*d$nFirms)
  
#mx = max(max(z), max(d$nFirmNew / (12*d$nFirms)))
#plot(d$Year, z, xlab = "year", ylab="Flow of firms", ylim=c(0, 1.1*mx), 
#     type="l", xlim=c(y0,mx_yr), main="", cex.main=0.7)
#lines(d$Year, d$nFirmCloseNatural / (12*d$nFirms), col=cols[2], type="l")
#lines(d$Year, d$nFirmCloseTooBig / (12*d$nFirms), col=cols[3], type="s")
#abline(h=120, lty=2)
#lines(d$Year, d$nFirmNew / (12*d$nFirms), col=cols[4], type="l")
#abline(h=0)
#abline(v=burnIn, lty=2)
#abline(v=2050, lty=2)
#ContourFunctions::multicolor.title(c("Closed:Total  ","Closed:Natural  ", "Closed:TooBig  ", "New"), 1:4, cex.main = 0.7)

if(F)
{
if(d$Year[1]>2050 & length(d$Year)>50)
{
  z = (length(d$Year)-50):length(d$Year)

  fit = as.character(auto.arima(d$YearConsumption))
  hp = hpfilter(d$YearConsumption)
  plot(d$Year[z], d$YearConsumption[z], type="b", ylab="Consumption per year", xlab="Year", main=fit, cex=0.5, pch=20) 
  lines(d$Year[z], hp[z], lty=2)

  fit = as.character(auto.arima(d$YearConsumption[z]/hp[z]))
  plot(d$Year[z], d$YearConsumption[z]/hp[z], type="b", ylab="Consumption per year", xlab="Year", main=fit, cex=0.5, pch=20)
  abline(h=1, lty=2)
  
  fit = as.character(auto.arima(d$YearEmployment))
  hp = hpfilter(d$YearEmployment)
  plot(d$Year[z], d$YearEmployment[z], type="b", ylab="Employment per year", xlab="Year", main=fit, cex=0.5, pch=20)
  lines(d$Year[z], hp[z], lty=2)

  fit = as.character(auto.arima(d$YearEmployment[z]/hp[z]))
  plot(d$Year[z], d$YearEmployment[z]/hp[z], type="b", ylab="Employment per year", xlab="Year", main=fit, cex=0.5, pch=20) 
  abline(h=1, lty=2)
  
    
}else
{
  
  plot(d$Year, d$YearConsumption, type="b", ylab="Consumption per year", xlab="Year", cex=0.5, pch=20)
  plot(d$Year, d$YearEmployment, type="b", ylab="Employment per year", xlab="Year", cex=0.5, pch=20)
    
}
}

#plot(d_house$Good, d_house$ShopGood, col=rgb(0,0,0,0.1), pch=20)

#d_house$diff1 = abs(d_house$Good-d_house$ShopGood)
#d_house$diff2 = abs(1-d_house$Good-d_house$ShopGood)
#d_house$diff = ifelse(d_house$diff1<d_house$diff2, d_house$diff1, d_house$diff2)

#hist(d_house$diff, breaks = 20)

p = d$Price[nrow(d)]
hist(d_house$Price, breaks=50, xlim=c(0.9*p, 1.1*p))
abline(v=p, col="red", lwd=2, lty=2)

w = d$Wage[nrow(d)]
d_h2 = d_house %>% filter(Wage>0)
hist(d_h2$Wage, breaks=50, xlim=c(0.9*w, 1.1*w))
abline(v=w, col="red", lwd=2, lty=2)



dev.off()


#-----------------------------------------------




