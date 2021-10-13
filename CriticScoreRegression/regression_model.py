import sys
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
import statsmodels.api as sm
from sklearn import linear_model

filePath = sys.argv[1]
data = pd.read_csv(filePath)

df = pd.DataFrame(data, columns=['Year', 'MasterGameID', 'GameName', 'EligiblePercentStandardGame', 'AdjustedPercentCounterPick', 'DateAdjustedHypeFactor','AverageDraftPosition','TotalBidAmount','BidPercentile','CriticScore'])

df=df.replace(0,np.nan)

df=df.fillna(df.mean())

X=df[['EligiblePercentStandardGame', 'AdjustedPercentCounterPick', 'DateAdjustedHypeFactor']]
Y=df['CriticScore']

regr = linear_model.LinearRegression()
regr.fit(X, Y)

print("CriticScoreFormula = {:.4} + {:.4} * EligiblePercentStandardGame + {:.4} * AdjustedPercentCounterPick + {:.4} * DateAdjustedHypeFactor".format(regr.intercept_, regr.coef_[0], regr.coef_[1], regr.coef_[2]))
