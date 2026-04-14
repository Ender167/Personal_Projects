import numpy as np

def calculate_entropy(Y):
    _, labels_counts = np.unique(Y, return_counts=True)
    total_instances = len(Y)
    entropy = sum([label_count / total_instances * np.log2(1 / (label_count / total_instances)) for label_count in labels_counts])
    return entropy

def get_y(data):
    y = data[:, -1]
    return y

def get_pred_class(Y):
    labels, labels_counts = np.unique(Y, return_counts=True)
    index = np.argmax(labels_counts)
    return labels[index]
