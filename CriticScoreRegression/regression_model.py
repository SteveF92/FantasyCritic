import sys
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
import statsmodels.api as sm
from sklearn import linear_model

filePath = sys.argv[1]
data = pd.read_csv(filePath)

df = pd.DataFrame(data, columns=['Year', 'MasterGameID', 'GameName', 'EligiblePercentStandardGame', 'EligiblePercentCounterPick', 'DateAdjustedHypeFactor','AverageDraftPosition','TotalBidAmount','BidPercentile','CriticScore'])

df=df.replace(0,np.nan)

df=df.fillna(df.mean())

X=df[['EligiblePercentStandardGame', 'EligiblePercentCounterPick', 'DateAdjustedHypeFactor','AverageDraftPosition','TotalBidAmount','BidPercentile']]
Y=df['CriticScore']

regr = linear_model.LinearRegression()
regr.fit(X, Y)

print("CriticScoreFormula = {:.4} + {:.4} * EligiblePercentStandardGame + {:.4} * EligiblePercentCounterPick + {:.4} * DateAdjustedHypeFactor + {:.4} * AverageDraftPosition + {:.4} * TotalBidAmount + {:.4} * BidPercentile".format(regr.intercept_, regr.coef_[0], regr.coef_[1], regr.coef_[2],regr.coef_[3],regr.coef_[4],regr.coef_[5]))
