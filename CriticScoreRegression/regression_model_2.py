# # Importing Libraries
import sys
import pandas as pd
from sklearn.linear_model import LinearRegression
from sklearn.model_selection import train_test_split

# # Reading Dataset
filePath = sys.argv[1]
dat = pd.read_csv(filePath)
dat.head(5)

# # Selecting the required Columns
ds = dat[["EligiblePercentStandardGame","AdjustedPercentCounterPick","DateAdjustedHypeFactor","CriticScore"]]

# # Checking that how many null values in each column
ds.isnull().sum(axis = 0)

# # Filling the Null values by the mean of the column
ds = ds.fillna(ds.mean())

# # Selecing input and outputs for the Linear Regression Model
X = ds[["EligiblePercentStandardGame","AdjustedPercentCounterPick","DateAdjustedHypeFactor"]]
Y = ds[["CriticScore"]]

# # Applying the Linear Regression Model
LR = LinearRegression()
model = LR.fit(X, Y)
x_train, x_test, y_train, y_test = train_test_split(X,Y, test_size=0.25, random_state=0)

# # Printing the coefficients of Linear Regression Model
print('intercept:', model.intercept_)
print('slope:', model.coef_)

# # Printing the equation of Linear Regression Model
print("CriticScoreFormula = ",model.intercept_[0],"+",model.coef_[0][0],"* EligiblePercentStandardGame +",model.coef_[0][1],"* AdjustedPercentCounterPick +",model.coef_[0][2],"* DateAdjustedHypeFactor")