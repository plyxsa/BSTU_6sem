import mglearn
import sklearn
import matplotlib.pyplot as plt
import numpy as np

# --- Часть 1: Создание и визуализация данных (Рис. 1) ---
from sklearn.datasets import make_blobs

X, y = make_blobs(random_state=42)
print("--- Визуализация исходных данных (Рис. 1) ---")
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.legend(["Класс 0", "Класс 1", "Класс 2"])
plt.show()

# --- Часть 2: Обучение LinearSVC и анализ коэффициентов ---
from sklearn.svm import LinearSVC

linear_svm = LinearSVC(max_iter=5000).fit(X, y) # Увеличим max_iter для сходимости
print("\n--- Обучение LinearSVC ---")
print("Форма коэффициента: ", linear_svm.coef_.shape)
print("Форма константы: ", linear_svm.intercept_.shape)

# --- Часть 3: Визуализация границ бинарных классификаторов (Рис. 2) ---
print("\n--- Визуализация границ бинарных классификаторов (Рис. 2) ---")
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
line = np.linspace(-15, 15)
for coef, intercept, color in zip(linear_svm.coef_, linear_svm.intercept_,
                                 mglearn.cm3.colors):
    plt.plot(line, -(line * coef[0] + intercept) / coef[1], c=color)
plt.ylim(-10, 15)
plt.xlim(-10, 8)
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
# Уточненная легенда для Рис. 2
plt.legend(['Класс 0', 'Класс 1', 'Класс 2', 'Линия класса 0', 'Линия класса 1', 'Линия класса 2'],
           loc=(1.01, 0.3))
plt.title("Рис. 2: Границы бинарных классификаторов (OvR)")
plt.show()

# --- Часть 4: Визуализация мультиклассовых границ (Рис. 3) ---
print("\n--- Визуализация мультиклассовых границ (Рис. 3) ---")
mglearn.plots.plot_2d_classification(linear_svm, X, fill=True, alpha=.7)
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
line = np.linspace(-15, 15)
for coef, intercept, color in zip(linear_svm.coef_, linear_svm.intercept_,
                                  mglearn.cm3.colors):
    plt.plot(line, -(line * coef[0] + intercept) / coef[1], c=color)
# Уточненная легенда для Рис. 3
plt.legend(['Класс 0', 'Класс 1', 'Класс 2', 'Линия класса 0', 'Линия класса 1', 'Линия класса 2'],
           loc=(1.01, 0.3))
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.title("Рис. 3: Мультиклассовые границы (OvR)")
plt.show()

print("\nКод выполнен.")
