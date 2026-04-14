
import csv
import MyLabelEncoder as MLE

class DataProcessor:

    def __init__(self, filepath):
        self._filepath = filepath

    def readData(self):

        with open(self._filepath, 'r') as file:
            reader = csv.reader(file)
            data = list(reader)
        return data

    def encodeData(self, data):

        labelEncoder = MLE.MyLabelEncoder()

        categoricalIdx = []
        for j in range(len(data[0])):
            if isinstance(data[0][j], str):
                categoricalIdx.append(j)

        for idx in categoricalIdx:
            column_data = [row[idx] for row in data]
            encoded_column = labelEncoder.fit_transform(column_data)
            for i, row in enumerate(data):
                row[idx] = encoded_column[i]

        return data


    def cleanData(self, data):
        feature_names = data[0]
        data = data[1:]

        data = [row for row in data if all(cell != '' for cell in row)]

        for i in range(len(data)):
            for j in range(len(data[i])):
                if data[i][j].isdigit():
                    data[i][j] = int(data[i][j])

        for i in range(len(data)):
            if data[i][19] > 100:
                data[i][19] = 100

        return data, feature_names


    def processData(self):
        data = self.readData()
        cleanedData, feature_names = self.cleanData(data)
        encodedData = self.encodeData(cleanedData)
        return encodedData, feature_names


