import numpy as np

from sklearn.naive_bayes import BernoulliNB # Для бинарных данных (0/1)
from sklearn.naive_bayes import MultinomialNB # Для счетных данных
from sklearn.naive_bayes import GaussianNB # Для непрерывных (вещественных) данных

print("--- BernoulliNB ---")
X = np.array([[0, 1, 0, 1], [1, 0, 1, 1], [0, 0, 0, 1], [1, 0, 1, 0]])
# Создание массива меток y для каждого объекта в X (два класса: 0 и 1)
y = np.array([0, 1, 0, 1])

# подсчет ненулевых признаков для каждого класса
counts = {}
for label in np.unique(y): # Проходим по уникальным меткам классов (0 и 1)
    counts[label] = X[y == label].sum(axis=0)
print("Частоты признаков (подсчитаны вручную):\n{}".format(counts))

clf = BernoulliNB()
# Обучение классификатора на данных X и метках y
clf.fit(X, y)
prediction = clf.predict(X[2:3])
print("Предсказание clf.predict для X[2]:\n{}".format(prediction)) # Ожидаем класс 0


print("\n--- MultinomialNB ---")
rng = np.random.RandomState(1)
# Создание набора данных X: 6 объектов, 100 признаков
X = rng.randint(5, size=(6, 100))
#  6 разных классов
y = np.array([1, 2, 3, 4, 5, 6])

clf = MultinomialNB()
clf.fit(X, y)
prediction = clf.predict(X[2:3])
print("Предсказание clf.predict для X[2] (MultinomialNB):\n{}".format(prediction))


print("\n--- GaussianNB ---")
# Создание набора данных X с 6 объектами и 2 непрерывными признаками (вещественные числа)
X = np.array([[-1, -1], [-2, -1], [-3, -2], [1, 1], [2, 1], [3, 2]])
# два класса: 1 и 2
Y = np.array([1, 1, 1, 2, 2, 2])

# Обучение с помощью fit
clf = GaussianNB()
clf.fit(X, Y)
prediction = clf.predict([[-0.8, -1]])
print("Предсказание clf.predict для [-0.8, -1] (GaussianNB):\n{}".format(prediction))

# Обучение с помощью partial_fit
clf_pf = GaussianNB()
clf_pf.partial_fit(X, Y, np.unique(Y))
prediction_pf = clf_pf.predict([[-0.8, -1]])
print("Предсказание clf_pf.predict для [-0.8, -1] (GaussianNB, partial_fit):\n{}".format(prediction_pf))