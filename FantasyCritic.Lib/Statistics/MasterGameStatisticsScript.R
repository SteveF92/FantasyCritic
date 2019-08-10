library(dplyr)
#library(tidytext)
library(tidyr)
#library(ggplot2)

# read CSV file
args <- commandArgs()
inputFilePath <- args[2]
dat <- read.csv(inputFilePath)

# select meaningful + response vars
ds <- dat %>%
  select(EligiblePercentStandardGame,EligiblePercentCounterPick,DateAdjustedHypeFactor,AverageDraftPosition,TotalBidAmount,BidPercentile,CriticScore)

# Replacing all zeroes to NA:
ds[ds == 0] <- NA

# summary(ds)

# replace NAs with column Means
ds <- replace_na(ds,as.list(colMeans(ds,na.rm=T)))

# simple regression model
simple.lm = lm(CriticScore ~ ., data = ds)

# output the formula with updated coefficients
#as.formula(
  paste0("CriticScoreFormula = ", round(coefficients(simple.lm)[1],6), " + ", 
         paste(sprintf("%.6f * %s", 
                       coefficients(simple.lm)[-1],  
                       names(coefficients(simple.lm)[-1])), 
               collapse=" + ")
  )
#)
