# Functions to generate lookup for polynomial regression coefficients
# (a,b,c) values to convert PnET-derived cohort establishment 
# probabilities (pest) into individual-seedling level establishment 
# probabilities that will yield a similar cohort-level probability
# of establishment.  The result depends on the ratio of the number 
# of seeds that arrive at the plot to the number of trees that must 
# establish to be considered a cohort (nseed/ntree).
#
# Nate Lichti (nlichti@purdue.edu)  04 September 2020
#
# Function definitions
find_abc <- function(nseed, ntree, wfun = NULL, ..., plot = FALSE){
  pest <- seq(1e-5, 1-1e-5, len=301)
  if(is.null(wfun)) wfun <- function(pest){1}
  fit <- optim(
    par = c(a = 1e-4, b = -1e-4, c = 1e-2),
    fn = objective,
    ntree = ntree,
    nseed = nseed,
    pest = pest,
    weights = wfun(pest, ...)
  )
  if(plot){
    pindiv <- calc_pindiv(pest, fit$par)
    pcohort <- calc_pcohort(pindiv, nseed, ntree)
    layout(matrix(1:2,nrow=2))
    plot(pest, pcohort, col = 'grey', main = 'cohort establishment',
         xlim=0:1, ylim=0:1)
    abline(coef = 0:1, col=2)
    plot(pest, pindiv, type = 'l', main = 'individual establishment')
  }
  cbind(
    data.frame(
      seeds_arriving = nseed,
      cohort_threshold = ntree
      ), 
    t(fit$par)
  )
}
wf1 <- function(pest, v, z) plogis(-v*(pest-z))
wf2 <- function(pest, v)exp(-v*pest)
calc_pindiv <- function(pest, pars){
  lnpred <- t(outer(pest, pars))^(1:length(pars))
  pmin(pmax(0, colSums(lnpred)), 1)
}
calc_pcohort <- function(p, n, t){
  pbinom(0, ceiling(n/t), p, lower = FALSE)
}
objective <- function(pars, ntree, nseed, pest, weights){
  pindiv <- calc_pindiv(pest, pars)
  pcohort <- calc_pcohort(pindiv, nseed, ntree)
  sum(weights*(pcohort - pest)^2)
}

# Demonstrate the basic function
# in plot, grey is the calculated P(establish a cohort) and
# red is a 1:1 reference line.  Lower plot shows the individual
# seedling establishment probability as a function of PnET's Pest.
find_abc(nseed = 41970, ntree = 1, wfun=wf1, 
         v=10, z=0.5, plot = TRUE)

# Make a lookup table
lookup_at <- expand.grid(
  nseed = 10^seq(from=0, to=7,by=0.25),
  ntree = 10^(0:4)
)
lookup_abc <- with(
  lookup_at,{
    table <- mapply(find_abc, nseed=nseed, ntree=ntree, MoreArgs = list(
      wfun = wf1, v = 10, z = 0.5
    ))
    t(table)    
  }
)
head(lookup_abc)

write.csv(lookup_abc, "C:/BRM/LANDIS_II/GitCode/Extension-PnET-Succession/deploy/docs/abc_table.csv",row.names=FALSE)
