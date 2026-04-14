import numpy as np
import copy

from lime.lime_tabular import LimeTabularExplainer

import RandomForestClassifier as RF

class DataManager:
    def __init__(self, data=None):
        if data is None:
            data = []
        self._data = np.array(data)

    def get_row(self, i):
        return self._data[i]

    def get_count(self):
        return len(self._data)

    def get_data(self):
        copied_data = self._data.copy()
        return copied_data

    def split_features(self, i):
        X = np.delete(self._data, i, axis=1)
        Y = self._data[:, i]
        return X, Y

    def train_test_split(self, X, Y, split):

        indices = np.random.permutation(len(X))
        X_shuffled = X[indices]
        Y_shuffled = Y[indices]

        train_size = int(split * len(X))

        X_train = X_shuffled[:train_size]
        Y_train = Y_shuffled[:train_size]

        X_test = X_shuffled[train_size:]
        Y_test = Y_shuffled[train_size:]

        return X_train, Y_train, X_test, Y_test

    def compute_confidence_interval(self, mean, std, n):
        Z = 1.96
        std_error = std / np.sqrt(n)
        lower_bound = mean - Z * std_error
        upper_bound = mean + Z * std_error
        return lower_bound, upper_bound

    def K_fold_cross_validation(self, model, X, Y, k=5):

        fold_size = len(X) // k
        indices = np.arange(len(X))
        accuracies = []
        for i in range(k):
            test_indices = indices[i * fold_size: (i + 1) * fold_size]
            train_indices = np.concatenate([indices[:i * fold_size], indices[(i + 1) * fold_size:]])

            X_train, X_test = X[train_indices], X[test_indices]
            Y_train, Y_test = Y[train_indices], Y[test_indices]

            model_copy = copy.deepcopy(model)

            model_copy.fit(X_train, Y_train)

            accuracy = self.compute_accuracy(model_copy, X_test, Y_test)
            accuracies.append(accuracy)

        acc_mean = np.mean(accuracies)
        acc_std = np.std(accuracies)
        confidence_interval = self.compute_confidence_interval(acc_mean, acc_std, len(accuracies))
        results = [acc_mean, acc_std, confidence_interval]
        return results

    def compute_metrics(self, model, X_train, Y_train, X_test, Y_test):
        metrics = {}

        metrics["train_accuracy"] = self.compute_accuracy(model, X_train, Y_train)
        metrics["test_accuracy"] = self.compute_accuracy(model, X_test, Y_test)

        predictions = model.predict(X_test)
        metrics["precision"] = self.compute_precision(predictions, Y_test)
        metrics["recall"] = self.compute_recall(predictions, Y_test)
        metrics["f1"] = self.compute_f1_measure(metrics["precision"], metrics["recall"])

        mean, std, conf = self.K_fold_cross_validation(model, X_train, Y_train, 5)
        metrics["cross_validation_score"] = mean
        metrics["std"] = std
        metrics["confidence"] = conf

        return metrics

    def compute_accuracy(self, model, X_test, Y_test):
        predictions = model.predict(X_test)

        correct_predictions = np.sum(predictions == Y_test)
        accuracy = correct_predictions / len(Y_test)

        return accuracy

    def compute_precision(self, predictions, Y_test):

        true_positive = np.sum((predictions == 0) & (Y_test == 0))
        false_positive = np.sum((predictions == 0) & (Y_test == 1))
        precision = true_positive / (true_positive + false_positive) if (true_positive + false_positive) > 0 else 0

        return precision

    def compute_recall(self, predictions, Y_test):

        true_positive = np.sum((predictions == 0) & (Y_test == 0))
        false_negative = np.sum((predictions == 1) & (Y_test == 0))
        recall = true_positive / (true_positive + false_negative) if (true_positive + false_negative) > 0 else 0

        return recall

    def compute_f1_measure(self, precision, recall):
        f1 = 2 * ((precision * recall)/(precision + recall))
        return f1

    def auc_score(self, y_true, probs):
        sorted_indices = np.argsort(probs[:, 0])[::-1]
        sorted_y_true = np.array(y_true)[sorted_indices]

        tp = 0
        fp = 0
        fn = np.sum(np.array(y_true) == 1)
        tn = np.sum(np.array(y_true) == 0)

        tpr = [0]
        fpr = [0]

        for i in range(len(y_true)):
            if sorted_y_true[i] == 1:
                tp += 1
                fn -= 1
            else:
                fp += 1
                tn -= 1

            tpr.append(tp / (tp + fn))
            fpr.append(fp / (fp + tn))

        tpr.append(1)
        fpr.append(1)

        auc = 0.0
        for i in range(1, len(fpr)):
            auc += 0.5 * (fpr[i] - fpr[i - 1]) * (tpr[i] + tpr[i - 1])

        return auc

    def auprc_score(self, y_true, y_scores):

        sorted_indices = np.argsort(y_scores)[::-1]
        y_true_sorted = np.array(y_true)[sorted_indices]

        tp = 0
        fp = 0
        fn = np.sum(y_true == 1)

        precisions = []
        recalls = []

        for i in range(len(y_true)):
            if y_true_sorted[i] == 1:
                tp += 1
                fn -= 1
            else:
                fp += 1

            precision = tp / (tp + fp) if tp + fp > 0 else 0
            recall = tp / (tp + fn) if tp + fn > 0 else 0

            precisions.append(precision)
            recalls.append(recall)

        auprc = 0
        for i in range(1, len(precisions)):
            auprc += (recalls[i] - recalls[i-1]) * (precisions[i] + precisions[i-1]) / 2

        return auprc
    def lime_test(self, model, X_train, Y_train, X_test, featureNames):
        features = featureNames[:15] + featureNames[16:]
        class_names = np.unique(Y_train).tolist()

        explainer = LimeTabularExplainer(
            training_data=X_train,
            training_labels=Y_train,
            mode="classification",
            feature_names=features,
            class_names=class_names,
            discretize_continuous=True
        )

        instance_to_explain = X_test[0]
        explanation = explainer.explain_instance(instance_to_explain, model.predict_proba, num_features=len(features))
        print(explanation.as_list())

    def grid_search(self,  X_train, Y_train, X_test, Y_test):

        best_score = 0
        best_params = {}
        n_trees = []
        min_samples_split = []
        min_samples_leaf = []
        max_depth = []

        params = {"min_samples_split": [1, 2],
              "min_samples_leaf": [1, 2],
              "max_depth": [5, 10],
              "n_trees": [5, 10, 20]}

        for param_key in params:
            if param_key == "n_trees":
                n_trees = params[param_key]
            elif param_key == "min_samples_split":
                min_samples_split = params[param_key]
            elif param_key == "min_samples_leaf":
                min_samples_leaf = params[param_key]
            elif param_key == "max_depth":
                max_depth = params[param_key]

        param_combinations = np.array(np.meshgrid(n_trees, min_samples_split, min_samples_leaf, max_depth)).T.reshape(-1, 4)

        for combination in param_combinations:
            current_params = {
                "n_trees": combination[0],
                "min_samples_split": combination[1],
                "min_samples_leaf": combination[2],
                "max_depth": combination[3],
            }
            model = RF.RandomForestClassifier(dataset=X_train, max_features=int(np.sqrt(X_train.shape[1])), params=current_params)
            model.fit_parallel_batches(X_train, Y_train)
            metrics = self.compute_metrics(model, X_train, Y_train, X_test, Y_test)

            if metrics['test_accuracy'] > best_score:
                best_score = metrics['test_accuracy']
                best_params = current_params

        return best_params
