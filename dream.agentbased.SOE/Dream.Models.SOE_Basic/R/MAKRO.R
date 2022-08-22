pct    = function(x) {diff(x)/x[1:(length(x)-1)]}
xLast  = function(x, n=1) {x[1:(length(x)-n)]}
xFirst = function(x, n=1) {x[(1+n):length(x)]}

lag = stats::lag

p.value = function(m,var) (1-pnorm(abs(m)/sqrt(var)))*2
p.value.fit = function(fit) p.value(fit$coef, diag(fit$var.coef))


hpfilter      = function(y, mu = 100) {
  n <- length(y)          # number of observations
  I <- diag(n)            # creates an identity matrix
  D <- diff(I,lag=1,d=2)  # second order differences
  solve(I + mu * crossprod(D) , y) # solves focs
}


loadData    = function(fileName){
  gSmec_data <<- as.matrix(read.table(fileName))
}

getVariable = function(varName){
  x = gSmec_data[match(toupper(varName), gSmec_data[,1]),]
  ret = as.numeric(x[2:length(x)])
}

getString = function(varName){
  x = gSmec_data[match(toupper(varName), gSmec_data[,1]),]
  ret = as.character(x[2:length(x)])
}


Loess  = function(x, span=0.75){predict(loess(x ~ I(1:length(x)), span = span))}
