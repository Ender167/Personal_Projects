import numpy as np

import DataProcessing as DP
import DataManager as DM
import RandomForestClassifier as RF

from sklearn.ensemble import RandomForestClassifier

def CustomRandomForest():

    np.random.seed(42)
    dataProcessor = DP.DataProcessor('StudentPerformanceFactors.csv')
    encoded_data, featureNames = dataProcessor.processData()
    dataManager = DM.DataManager(encoded_data)

    X, y = dataManager.split_features(15)  # target label
    X_train, Y_train, X_test, Y_test = dataManager.train_test_split(X, y, 0.8)

    model = RF.RandomForestClassifier(dataset=X_train, n_trees=5, max_depth=5, max_features=int(np.sqrt(X_train.shape[1])), min_samples_leaf=1, min_samples_split=2)
    #params = dataManager.grid_seach(model, X_train, Y_train, X_test, Y_test, params)

    model.fit(X_train, Y_train)
    statistical_results(dataManager , model, X_train, Y_train, X_test, Y_test)

def LibraryRandomForest():
    np.random.seed(42)
    dataProcessor = DP.DataProcessor('StudentPerformanceFactors.csv')
    encoded_data, featureNames = dataProcessor.processData()
    dataManager = DM.DataManager(encoded_data)

    X, y = dataManager.split_features(15)  # target label
    X_train, Y_train, X_test, Y_test = dataManager.train_test_split(X, y, 0.8)

    model = RandomForestClassifier(n_estimators=5, max_depth=5, min_samples_split=2, min_samples_leaf=1, criterion="entropy", random_state=42)
    model.fit(X_train, Y_train)

    statistical_results(dataManager, model, X_train, Y_train, X_test, Y_test)


def statistical_results(dataManager, model, X_train, Y_train, X_test, Y_test):
    metrics = dataManager.compute_metrics(model, X_train, Y_train, X_test, Y_test)

    print(f"Train Accuracy: {metrics['train_accuracy'] * 100:.2f}%")
    print(f"Test Accuracy: {metrics['test_accuracy'] * 100:.2f}%")
    print(f"Precision {metrics['precision']:.2f}")
    print(f"Recall {metrics['recall']:.2f}")
    print(f"F1-measure {metrics['f1']:.2f}")
    print(f"5-fold cross validation score {metrics['cross_validation_score']:.2f}")
    print(f"standard deviation {metrics['std']:.2f}")

    conf_lower, conf_high = metrics['confidence']
    print(f"confidence interval {conf_lower:.2f}, {conf_high:.2f}")
    print(f"AUC: {dataManager.auc_score(Y_test, model.predict_proba(X_test))}")
    print(f"AUPRC: {dataManager.auprc_score(Y_test, model.predict_proba(X_test)[:, 0])}")





def main():
    print("Custom Random Forest")
    CustomRandomForest()
    print('\n')
    print("Library Random Forest")
    LibraryRandomForest()


if __name__ == '__main__':
    main()
