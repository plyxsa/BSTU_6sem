import mglearn
import matplotlib.pyplot as plt
import numpy as np

# --- Часть 1: Визуализация LogisticRegression и LinearSVC на данных forge ---
from sklearn.linear_model import LogisticRegression
from sklearn.svm import LinearSVC

X, y = mglearn.datasets.make_forge()
fig, axes = plt.subplots(1, 2, figsize=(10, 3))

print("--- Визуализация границ решений (Рис. 1) ---")
for model, ax in zip([LinearSVC(max_iter=5000), LogisticRegression()], axes): # Увеличил max_iter для LinearSVC
    clf = model.fit(X, y)
    mglearn.plots.plot_2d_separator(clf, X, fill=False, eps=0.5,
                                   ax=ax, alpha=.7)
    mglearn.discrete_scatter(X[:, 0], X[:, 1], y, ax=ax)
    ax.set_title(f"{clf.__class__.__name__}") # Исправлено получение имени класса
    ax.set_xlabel("Признак 0")
    ax.set_ylabel("Признак 1")
axes[0].legend()
plt.show()

# --- Часть 2: Визуализация влияния параметра C для LinearSVC (Рис. 2) ---
print("\n--- Визуализация влияния параметра C (Рис. 2) ---")
# Эта функция из mglearn напрямую генерирует Рис. 2 из лабораторной
# Она внутри себя создает модели LinearSVC с C=0.01, 1, 100
mglearn.plots.plot_linear_svc_regularization()
plt.show()

# --- Часть 3: LogisticRegression на данных Breast Cancer ---
from sklearn.datasets import load_breast_cancer
from sklearn.model_selection import train_test_split

cancer = load_breast_cancer()
X_train, X_test, y_train, y_test = train_test_split(
    cancer.data, cancer.target, stratify=cancer.target, random_state=42)

print("\n--- LogisticRegression на данных Breast Cancer (L2 регуляризация) ---")
# Модель с C=1 (по умолчанию)
logreg = LogisticRegression(max_iter=10000).fit(X_train, y_train) # Увеличил max_iter
print("C=1 (по умолчанию):")
print(f"  Правильность на обучающем наборе: {logreg.score(X_train, y_train):.3f}")
print(f"  Правильность на тестовом наборе: {logreg.score(X_test, y_test):.3f}")

# Модель с C=100 (меньше регуляризации)
logreg100 = LogisticRegression(C=100, max_iter=10000).fit(X_train, y_train) # Увеличил max_iter
print("C=100:")
print(f"  Правильность на обучающем наборе: {logreg100.score(X_train, y_train):.3f}")
print(f"  Правильность на тестовом наборе: {logreg100.score(X_test, y_test):.3f}")

# Модель с C=0.01 (больше регуляризации)
logreg001 = LogisticRegression(C=0.01, max_iter=10000).fit(X_train, y_train) # Увеличил max_iter
print("C=0.01:")
print(f"  Правильность на обучающем наборе: {logreg001.score(X_train, y_train):.3f}")
print(f"  Правильность на тестовом наборе: {logreg001.score(X_test, y_test):.3f}")

# --- Часть 4: Визуализация коэффициентов LogisticRegression (L2, Рис. 3) ---
print("\n--- Визуализация коэффициентов LogisticRegression (L2, Рис. 3) ---")
plt.figure(figsize=(12, 6)) # Увеличим размер для читаемости названий признаков
plt.plot(logreg.coef_.T, 'o', label="C=1")
plt.plot(logreg100.coef_.T, '^', label="C=100")
plt.plot(logreg001.coef_.T, 'v', label="C=0.01") # В лабе опечатка С=0.001 на рис.3, но в коде C=0.01
plt.xticks(range(cancer.data.shape[1]), cancer.feature_names, rotation=90)
plt.hlines(0, 0, cancer.data.shape[1])
plt.ylim(-5, 5)
plt.xlabel("Индекс коэффициента")
plt.ylabel("Оценка коэффициента")
plt.title("Рис. 3: Коэффициенты LogisticRegression (L2) для разных C")
plt.legend()
plt.show()

# --- Часть 5: LogisticRegression с L1 регуляризацией на данных Breast Cancer ---
print("\n--- LogisticRegression на данных Breast Cancer (L1 регуляризация) ---")
plt.figure(figsize=(12, 6)) # Создаем новую фигуру для графика Рис. 4

for C, marker in zip([0.01, 1, 100], ['o', '^', 'v']): # Исправлены значения C для L1 как на рис.4
    # Добавляем solver='liblinear', т.к. он хорошо работает с L1
    lr_l1 = LogisticRegression(C=C, penalty="l1", solver='liblinear', max_iter=10000).fit(X_train, y_train)
    print(f"Правильность на обучении для логрегрессии l1 с C={C:.3f}: {lr_l1.score(X_train, y_train):.2f}")
    print(f"Правильность на тесте для логрегрессии l1 с C={C:.3f}: {lr_l1.score(X_test, y_test):.2f}")
    print(f"  Количество ненулевых признаков: {np.sum(lr_l1.coef_ != 0)}") # Добавим подсчет признаков
    plt.plot(lr_l1.coef_.T, marker, label="C={:.3f}".format(C)) # Исправлено С=0.001 на C=0.01

# --- Часть 6: Визуализация коэффициентов LogisticRegression (L1, Рис. 4) ---
print("\n--- Визуализация коэффициентов LogisticRegression (L1, Рис. 4) ---")
plt.xticks(range(cancer.data.shape[1]), cancer.feature_names, rotation=90)
plt.hlines(0, 0, cancer.data.shape[1])
plt.xlabel("Индекс коэффициента")
plt.ylabel("Оценка коэффициента")
plt.ylim(-5, 5)
# В лабе loc=3, но это перекрывает оси, loc='best' или без loc обычно лучше
plt.legend(loc='best')
plt.title("Рис. 4: Коэффициенты LogisticRegression (L1) для разных C")
plt.show()

print("\nКод выполнен.")