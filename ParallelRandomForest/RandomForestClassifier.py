import os

from DecisionTreeClassifier import DecisionTreeClassifier
from concurrent.futures import ProcessPoolExecutor
import numpy as np

class RandomForestClassifier:
    def __init__(self, dataset, n_trees=100, max_depth=None, min_samples_split=2, min_samples_leaf=1, max_features=None, params=None):

        self.trees = []
        self.dataset = dataset
        self.max_features = max_features
        if params is None:
            self.min_samples_split = min_samples_split
            self.min_samples_leaf = min_samples_leaf
            self.max_depth = max_depth
            self.n_trees = n_trees
        else:
            self.min_samples_split = params["min_samples_split"]
            self.min_samples_leaf = params["min_samples_leaf"]
            self.max_depth = params["max_depth"]
            self.n_trees = params["n_trees"]

    def bootstrap_sample(self, X, Y):
        n_samples = len(X)
        indices = np.random.choice(range(n_samples), size=n_samples, replace=True)
        return X[indices], Y[indices]

    def fit_single_tree(self, X_sample, Y_sample):
        tree = DecisionTreeClassifier(self.dataset, max_features=self.max_features, max_depth=self.max_depth, min_samples_split=self.min_samples_split, min_samples_leaf=self.min_samples_leaf)
        tree.fit(X_sample, Y_sample)
        return tree

    def train_batch(self, tasks):
        batch_trees = []
        for task in tasks:
            X_sample, Y_sample = task
            tree = self.fit_single_tree(X_sample, Y_sample)
            batch_trees.append(tree)
        return batch_trees

    def fit_parallel_batches(self, X, Y):
        executor = ProcessPoolExecutor(max_workers=os.cpu_count())
        futures = []
        trees_per_batch = 10
        batches = self.n_trees // trees_per_batch if self.n_trees % trees_per_batch == 0 else (self.n_trees // trees_per_batch) + 1

        for i in range(batches):
            tasks = []
            for j in range(trees_per_batch):
                X_sample, Y_sample = self.bootstrap_sample(X, Y)
                tasks.append((X_sample, Y_sample))
            futures.append(executor.submit(self.train_batch, tasks))

        for future in futures:
            self.trees.extend(future.result())

        executor.shutdown()

    def fit_sequential(self, X, Y):
        for i in range(self.n_trees):
            X_sample, Y_sample = self.bootstrap_sample(X, Y)

            tree = DecisionTreeClassifier(self.dataset, max_features=self.max_features, max_depth=self.max_depth, min_samples_split=self.min_samples_split, min_samples_leaf=self.min_samples_leaf)
            tree.fit(X_sample, Y_sample)
            self.trees.append(tree)

    def fit(self, X, Y):
        if self.n_trees < 20:
            self.fit_sequential(X, Y)
        else:
            self.fit_parallel_batches(X, Y)

    def predict(self, X):
        tree_predictions = np.array([tree.predict(X) for tree in self.trees])

        final_predictions = []

        for i in range(len(X)):
            predictions = tree_predictions[:, i]
            valid_predictions = predictions
            final_predictions.append(np.bincount(valid_predictions.astype(int)).argmax())

        return np.array(final_predictions)

    def predict_proba(self, X):

        n_samples = len(X)
        n_classes = 2
        proba = np.zeros((n_samples, n_classes))

        tree_predictions = np.array([tree.predict(X) for tree in self.trees])
        for i in range(n_samples):
            sample_predictions = tree_predictions[:, i]

            proba[i, 0] = np.mean(sample_predictions == 0)
            proba[i, 1] = np.mean(sample_predictions == 1)
        return proba

    def get_feature_importances(self):
        total_importance = {i: 0 for i in range(self.dataset.shape[1] - 1)}

        for tree in self.trees:
            tree_importance = tree.get_feature_importances()
            for feature, importance in tree_importance.items():
                total_importance[feature] += importance

        total_importance_sum = sum(total_importance.values())
        normalized_importance = {feature: importance / total_importance_sum for feature, importance in total_importance.items()}

        return normalized_importance
