import json
import sys
import boto3
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
import statsmodels.api as sm
from sklearn import linear_model
from io import BytesIO

def handler(event, context):
    s3 = boto3.resource('s3')
    obj = s3.Object('fantasy-critic-production', 'HypeFactor/LiveData.csv')
    body = obj.get()['Body'].read()

    data = pd.read_csv(BytesIO(body))
    df = pd.DataFrame(data, columns=['EligiblePercentStandardGame', 'AdjustedPercentCounterPick', 'DateAdjustedHypeFactor','AverageDraftPosition','TotalBidAmount','BidPercentile','CriticScore'])

    df=df.replace(0,np.nan)

    df=df.fillna(df.mean())

    X=df[['EligiblePercentStandardGame', 'AdjustedPercentCounterPick', 'DateAdjustedHypeFactor']]
    Y=df['CriticScore']

    regr = linear_model.LinearRegression()
    regr.fit(X, Y)

    print("BaseScore:" + str(regr.intercept_))
    print("EligiblePercentStandardGame:" + str(regr.coef_[0]))
    print("AdjustedPercentCounterPick:" + str(regr.coef_[1]))
    print("DateAdjustedHypeFactor:" + str(regr.coef_[2]))

    returnData = {}
    returnData['BaseScore'] = regr.intercept_
    returnData['EligiblePercentStandardGame'] = regr.coef_[0]
    returnData['AdjustedPercentCounterPick'] = regr.coef_[1]
    returnData['DateAdjustedHypeFactor'] = regr.coef_[2]

    json_data = json.dumps(returnData)
    return json_data