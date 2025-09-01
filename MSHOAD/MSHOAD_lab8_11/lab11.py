import numpy as np
import matplotlib.pyplot as plt
import sklearn
import mglearn
from sklearn.model_selection import train_test_split
from sklearn.neighbors import KNeighborsClassifier, KNeighborsRegressor
from sklearn.datasets import load_breast_cancer, fetch_california_housing

# синтетический набор 1
X, y = mglearn.datasets.make_forge()
# диаграмма рассеяния
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
plt.legend(["Класс 0", "Класс 1"], loc=4)
plt.xlabel("Первый признак")
plt.ylabel("Второй признак")
print("Форма массива X: {}".format(X.shape))
plt.show()

# синтетический набор 2
X, y = mglearn.datasets.make_wave(n_samples=40)
plt.plot(X, y, 'o')
plt.ylim(-3, 3)
plt.xlabel("Признак")
plt.ylabel("Целевая переменная")
plt.show()

# набор данных по раку молочной железы Университета Висконсин
cancer = load_breast_cancer()
print("Ключи cancer: \n{}".format(cancer.keys()))
print("Форма массива data для набора cancer: {}".format(cancer.data.shape))
# набор данных включает 569 точек данных и 30 признаков.
# описание каждого признака
print("Имена признаков:\n{}".format(cancer.feature_names))
print("Количество примеров для каждого класса:\n{}".format(
    {n: v for n, v in zip(cancer.target_names, np.bincount(cancer.target))}))
# benign - доброкачественная
# malignant - злокачественная

# реальный набор данных
# Набор данных Boston Housing (load_boston) был удалён из scikit-learn
# заменяем на California Housing Dataset

california = fetch_california_housing()
X, y = california.data, california.target

print("Форма массива data для набора данных california : {}".format(california.data.shape))
print("Названия признаков:", california.feature_names)

# алгоритм k ближайших соседей
mglearn.plots.plot_knn_classification(n_neighbors=1)
plt.show()

mglearn.plots.plot_knn_classification(n_neighbors=3)
plt.show()

# разделяем набор данных на тестовый и обучающий
X, y = mglearn.datasets.make_forge()
X_train, X_test, y_train, y_test = train_test_split(X, y, random_state=0)

#  создаем объект-экземпляр класса
clf = KNeighborsClassifier(n_neighbors=3)
clf.fit(X_train, y_train)
#  Для кажд точки тестового набора  вычисляет ее ближайших соседей в обучающем наборе
# и находит среди них наиболее часто встречающийся класс
print("Прогнозы на тестовом наборе: {}".format(clf.predict(X_test)))
#  оценка обобщающей способности модели
print("Правильность на тестовом наборе: {:.2f}".format(clf.score(X_test, y_test)))

# строим сетку из 1 строки и 3 столбцов для отображения графиков
fig, axes = plt.subplots(1, 3, figsize=(10, 3))
# Перебираем значения количества соседей и соответствующие оси для рисования
for n_neighbors, ax in zip([1, 3, 9], axes):
    # Обучаем классификатор k-ближайших соседей с заданным количеством соседей
    clf = KNeighborsClassifier(n_neighbors=n_neighbors).fit(X, y)
    # Рисуем границу принятия решений на плоскости признаков
    mglearn.plots.plot_2d_separator(clf, X, fill=True, eps=0.5, ax=ax, alpha=.4)
    # Отображаем точки обучающего набора
    mglearn.discrete_scatter(X[:, 0], X[:, 1], y, ax=ax)
    ax.set_title("количество соседей: {}".format(n_neighbors))
    ax.set_xlabel("Признак 0")
    ax.set_ylabel("Признак 1")
axes[0].legend(loc=3)
plt.show()


X_train, X_test, y_train, y_test = train_test_split(
    cancer.data, cancer.target, stratify=cancer.target, random_state=66)

training_accuracy = []
test_accuracy = []
# пробуем n_neighbors от 1 до 10
neighbors_settings = range(1, 11)

for n_neighbors in neighbors_settings:
    # строим модель
    clf = KNeighborsClassifier(n_neighbors=n_neighbors)
    clf.fit(X_train, y_train)
    # записываем правильность на обучающем наборе
    training_accuracy.append(clf.score(X_train, y_train))
    # записываем правильность на тестовом наборе
    test_accuracy.append(clf.score(X_test, y_test))

plt.plot(neighbors_settings, training_accuracy, label="Обучающая точность")
plt.plot(neighbors_settings, test_accuracy, label="Тестовая точность")
plt.ylabel("Правильность")
plt.xlabel("Количество соседей")
plt.legend()
plt.show()


# Регрессия k ближайших соседий
#  для н = 1, прогноз- целевое значение ближайшего соседа
mglearn.plots.plot_knn_regression(n_neighbors=1)
plt.show()

mglearn.plots.plot_knn_regression(n_neighbors=3)
plt.show()


X, y = mglearn.datasets.make_wave(n_samples=40)
# разбиваем набор данных wave на обучающую и тестовую выборки
X_train, X_test, y_train, y_test = train_test_split(X, y, random_state=0)
# создаем экземпляр модели
reg = KNeighborsRegressor(n_neighbors=3)
# подгоняем модель с использованием обучающих данных и обучающих ответов
reg.fit(X_train, y_train)
# получим прогнозы для тестового набора

print("Прогнозы для тестового набора:\n{}".format(reg.predict(X_test)))
# показатель качества регрессионной модели. Значение 1 - идеально прогнозир способность
print("R^2 на тестовом наборе: {:.2f}".format(reg.score(X_test, y_test)))


fig, axes = plt.subplots(1, 3, figsize=(15, 4))
# Генерируем равномерно распределённые значения признака от -3 до 3
line = np.linspace(-3, 3, 1000).reshape(-1, 1)

# Перебираем различные значения числа соседей и соответствующие оси
for n_neighbors, ax in zip([1, 3, 9], axes):
    reg = KNeighborsRegressor(n_neighbors=n_neighbors)
    reg.fit(X_train, y_train)
    # Строим линию предсказаний модели по всей области line
    ax.plot(line, reg.predict(line), label="Прогнозы модели")
    # Отображаем обучающие данные как треугольники вверх
    ax.plot(X_train, y_train, '^', c=mglearn.cm2(0), markersize=8, label="Обучающие данные")
    # Отображаем тестовые данные как треугольники вниз
    ax.plot(X_test, y_test, 'v', c=mglearn.cm2(1), markersize=8, label="Тестовые данные")
    # Заголовок с числом соседей и оценками R² для обучающей и тестовой выборок
    ax.set_title(f"{n_neighbors} сосед(ей)\n"
                 f"Обуч. score: {reg.score(X_train, y_train):.2f}, "
                 f"Тест. score: {reg.score(X_test, y_test):.2f}")
    ax.set_xlabel("Признак")
    ax.set_ylabel("Целевая переменная")
axes[0].legend(loc="best")
plt.show()
