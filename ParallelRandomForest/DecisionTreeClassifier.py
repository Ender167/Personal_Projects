import numpy as np
import Utility


class Node:
    def __init__(self, data=None, children=None, split_on = None, pred_class=None, is_leaf=False):
        self.data = data
        self.children = children
        self.split_on = split_on
        self.pred_class = pred_class
        self.is_leaf = is_leaf


class DecisionTreeClassifier:
    def __init__(self, dataset, max_features=None, max_depth=None, min_samples_split=2, min_samples_leaf=1):
        self.root = Node()
        self._dataset = dataset

        self.max_depth = max_depth
        self.min_samples_split = min_samples_split
        self.min_samples_leaf = min_samples_leaf

        self.max_features = max_features
        self.feature_importances_ = {i: 0 for i in range(self._dataset.shape[1] - 1)}
        np.random.seed(42)

    def split_on_feature(self, data, feat_index):
        feature_values = data[:, feat_index]
        unique_values = np.unique(feature_values)

        split_nodes = {}
        weighted_entropy = 0
        total_instances = len(data)

        for unique_value in unique_values:
            partition = data[data[:, feat_index] == unique_value, :]
            node = Node(data=partition)
            split_nodes[unique_value] = node
            partition_y = Utility.get_y(partition)
            node_entropy = Utility.calculate_entropy(partition_y)
            weighted_entropy += (len(partition) / total_instances) * node_entropy

        return split_nodes, weighted_entropy

    def meet_criteria(self, node, current_depth):
        y = Utility.get_y(node.data)

        if len(node.data) < self.min_samples_split:
            return True

        elif self.max_depth is not None and current_depth >= self.max_depth:
            return True

        elif Utility.calculate_entropy(y) == 0:
            return True

        return False

    def best_split(self, node, current_depth):

        if self.meet_criteria(node, current_depth):
            node.is_leaf = True
            y = Utility.get_y(node.data)
            node.pred_class = Utility.get_pred_class(y)
            return

        index_feature_split = -1
        min_entropy = 1
        child_nodes = {}

        num_features = self._dataset.shape[1] - 1
        features_to_consider = range(num_features)

        if self.max_features is not None:
            features_to_consider = np.random.choice(features_to_consider, self.max_features, replace=False)
        for i in features_to_consider:
            split_nodes, weighted_entropy = self.split_on_feature(node.data, i)

            valid_split = all(len(child.data) >= self.min_samples_leaf for child in split_nodes.values())
            if not valid_split:
                continue

            current_entropy = Utility.calculate_entropy(Utility.get_y(node.data))
            entropy_reduction = current_entropy - weighted_entropy

            if entropy_reduction > 0:
                self.feature_importances_[i] += entropy_reduction

            if weighted_entropy < min_entropy:
                child_nodes, min_entropy = split_nodes, weighted_entropy
                index_feature_split = i

        node.children = child_nodes
        node.split_on = index_feature_split

        for child_node in child_nodes.values():
            self.best_split(child_node, current_depth+1)

    def fit(self, X, Y):
        data = np.column_stack([X, Y])
        self.root.data = data
        self.best_split(self.root, current_depth=0)

    def fit_with_weights(self, X, Y, sample_weights):
        data = np.column_stack([X, Y])
        self.root.data = data
        self.root.weights = sample_weights
        self.best_split(self.root, current_depth=0)

    def predict(self, X):
        pred_list = [self.traverse_tree(x, self.root) for x in X]
        predictions = np.array(pred_list)
        return predictions

    def traverse_tree(self, x, node):
        if node.is_leaf:
            return node.pred_class
        feat_value = x[node.split_on]
        if feat_value not in node.children:
            y = Utility.get_y(node.data)
            return Utility.get_pred_class(y)

        predicted_class = self.traverse_tree(x, node.children[feat_value])
        return predicted_class

    def get_feature_importance(self):
        total_importance = sum(self.feature_importances_.values())
        return {feature: importance / total_importance for feature, importance in self.feature_importances_.items()}
