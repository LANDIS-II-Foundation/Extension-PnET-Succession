df <- read.table("C:/BRM/LANDIS_II/GitCode/aruzicka555/Extension-PnET-Succession/deploy/docs/Estimate_woodbio.txt", header=TRUE)

options(scipen = 100)
slopeFit <- lm(Slope ~ FracBelowG * FraCtWd * FracFol, data=df)
summary(slopeFit)
coef(slopeFit, digits=9)
plot(x=predict(slopeFit), y=df$Slope)

intFit <- lm(Int ~ FracBelowG * FraCtWd * FracFol, data=df)
summary(intFit, digits=11)
coef(intFit, digits=11)
plot(x=predict(intFit), y=df$Int)
