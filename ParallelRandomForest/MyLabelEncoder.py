class MyLabelEncoder:
    def __init__(self):
        self.label_to_int = {}
        self.int_to_label = {}

    def fit(self, labels):
        unique_labels = sorted(set(labels))
        self.label_to_int = {label: idx for idx, label in enumerate(unique_labels)}
        self.int_to_label = {idx: label for idx, label in enumerate(unique_labels)}

    def transform(self, labels):
        if not self.label_to_int:
            raise ValueError("The encoder has not been fitted yet.")
        return [self.label_to_int[label] for label in labels]

    def fit_transform(self, labels):
        self.fit(labels)
        return self.transform(labels)

    def inverse_transform(self, integers):
        if not self.int_to_label:
            raise ValueError("The encoder has not been fitted yet.")
        return [self.int_to_label[idx] for idx in integers]
