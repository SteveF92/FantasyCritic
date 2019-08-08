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
  select(EligiblePercentCounterPick, TotalBidAmount, DateAdjustedHypeFactor, CriticScore)

# Replacing all zeroes to NA:
ds[ds == 0] <- NA

# summary(ds)

# all games in 2020 have a preset score of 70
# table(ds$CriticScore==70)

# take out score 70 from response variable
ds <- ds %>% filter(!CriticScore==70)

# replace NAs with column Means
ds <- replace_na(ds,as.list(colMeans(ds,na.rm=T)))

# simple regression model
simple.lm = lm(CriticScore ~ ., data = ds)

# summary(simple.lm)


# output the formula with updated coefficients
#as.formula(
  paste0("CriticScoreFormula = ", round(coefficients(simple.lm)[1],6), " + ", 
         paste(sprintf("%.6f * %s", 
                       coefficients(simple.lm)[-1],  
                       names(coefficients(simple.lm)[-1])), 
               collapse=" + ")
  )
#)
