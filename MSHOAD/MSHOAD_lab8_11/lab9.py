import pandas as pds
import mglearn
import matplotlib.pyplot as plt
from sklearn.datasets import load_iris

iris_dataset = load_iris()
#Содержит ключи и значения
print("Ключи iris_dataset: \n{}".format(iris_dataset.keys()))

#Значение ключа DESCR – это краткое описание набора данных
print(iris_dataset['DESCR'][:193] + "\n...")

#target_names Массив строк, содержащий сорта цветов
print("Названия ответов: {}".format(iris_dataset['target_names']))

#Значение feature_names – это список строк с описанием каждого признака
print("Названия признаков: \n{}".format(iris_dataset['feature_names']))

#Сами данные записаны в массивах target и data
print("Тип массива data: {}".format(type(iris_dataset['data'])))

#Строки в массиве data соответствуют цветам ириса, а столбцы представляют собой четыре признака измеренные для цветов
print("Форма массива data: {}".format(iris_dataset['data'].shape))
print("Первые пять строк массива data:\n{}".format(iris_dataset['data'][:5]))

#Массив target содержит сорта уже измеренных цветов, тоже записанные в виде массива NumPy
print("Тип массива target: {}".format(type(iris_dataset['target'])))
print("Форма массива target: {}".format(iris_dataset['target'].shape))
print("Ответы:\n{}".format(iris_dataset['target']))

from sklearn.model_selection import train_test_split

#Функция train_test_split, которая перемешивает набор данных и разбивает его на две части
X_train, X_test, y_train, y_test = train_test_split(
iris_dataset['data'], iris_dataset['target'], random_state=0)
print("форма массива X_train: {}".format(X_train.shape))
print("форма массива y_train: {}".format(y_train.shape))
print("форма массива X_test: {}".format(X_test.shape))
print("форма массива y_test: {}".format(y_test.shape))

#Создаем dataframe из данных в массиве X_train
#Маркируем столбцы, используя строки в iris_dataset.feature_names
iris_dataframe = pds.DataFrame(X_train, columns=iris_dataset.feature_names)
from pandas.plotting import scatter_matrix
#Создаем матрицу рассеяния из dataframe, цвет точек задаем с помощью y_train
grr = scatter_matrix(iris_dataframe, c=y_train, figsize=(15, 15), marker='o',
hist_kwds={'bins': 20}, s=60, alpha=.8, cmap=mglearn.cm3)
plt.show()