import mglearn
import matplotlib.pyplot as plt
import numpy as np
from sklearn.datasets import make_blobs
from sklearn.svm import LinearSVC
from sklearn.svm import SVC
from sklearn.model_selection import train_test_split
from sklearn.datasets import load_breast_cancer
from mpl_toolkits.mplot3d import Axes3D, axes3d

# Часть 1
print("--- Линейные модели и нелинейные признаки ---")

# Генерация и визуализация исходных 2D данных (Рис. 1)
X, y = make_blobs(centers=4, random_state=8)
y = y % 2

plt.figure()
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.title("Рис. 1: Исходные 2D данные (линейно неразделимые)")
plt.show()

# Попытка разделения с помощью линейного SVM (Рис. 2) ---
linear_svm = LinearSVC(random_state=0, C=1, max_iter=10000)
linear_svm.fit(X, y)

plt.figure()
mglearn.plots.plot_2d_separator(linear_svm, X)
mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.title("Рис. 2: Граница линейного SVM на исходных данных")
plt.show()

X_new = np.hstack([X, X[:, 1:] ** 2])

# Визуализация данных в новом 3D пространстве (Рис. 3)
figure = plt.figure()
ax = Axes3D(figure, elev=-152, azim=-26, auto_add_to_figure=False)
figure.add_axes(ax)

mask = y == 0
ax.scatter(X_new[mask, 0], X_new[mask, 1], X_new[mask, 2], c='b',
           cmap=mglearn.cm2, s=60)

ax.scatter(X_new[~mask, 0], X_new[~mask, 1], X_new[~mask, 2], c='r', marker='^',
           cmap=mglearn.cm2, s=60)

ax.set_xlabel("признак0")
ax.set_ylabel("признак1")
ax.set_zlabel("признак1 ** 2")
ax.set_title("Рис. 3: Данные в расширенном 3D пространстве")
plt.show()

# Обучение линейного SVM на 3D данных и визуализация плоскости (Рис. 4)
linear_svm_3d = LinearSVC(random_state=0, C=1, max_iter=10000)
linear_svm_3d.fit(X_new, y)
coef, intercept = linear_svm_3d.coef_.ravel(), linear_svm_3d.intercept_

figure = plt.figure()
ax = Axes3D(figure, elev=-152, azim=-26, auto_add_to_figure=False)
figure.add_axes(ax)

xx = np.linspace(X_new[:, 0].min() - 2, X_new[:, 0].max() + 2, 50)
yy = np.linspace(X_new[:, 1].min() - 2, X_new[:, 1].max() + 2, 50)
XX, YY = np.meshgrid(xx, yy)

if abs(coef[2]) > 1e-6:
    ZZ = (coef[0] * XX + coef[1] * YY + intercept) / -coef[2]
else:
    ZZ = np.full_like(XX, X_new[:, 2].mean())
    print("Предупреждение: Коэффициент для 3-го признака близок к нулю, плоскость может быть неверно отображена.")

ax.plot_surface(XX, YY, ZZ, rstride=8, cstride=8, alpha=0.3)

ax.scatter(X_new[mask, 0], X_new[mask, 1], X_new[mask, 2], c='b',
           cmap=mglearn.cm2, s=60)
ax.scatter(X_new[~mask, 0], X_new[~mask, 1], X_new[~mask, 2], c='r', marker='^',
           cmap=mglearn.cm2, s=60)

ax.set_xlabel("признак0")
ax.set_ylabel("признак1")
ax.set_zlabel("признак1 ** 2")
ax.set_title("Рис. 4: Разделяющая плоскость линейного SVM в 3D")
plt.show()

# Проекция разделяющей границы обратно на 2D (Рис. 5)
ZZ_dec = YY**2
dec = linear_svm_3d.decision_function(np.c_[XX.ravel(), YY.ravel(), ZZ_dec.ravel()])
plt.figure()

plt.contourf(XX, YY, dec.reshape(XX.shape), levels=[dec.min(), 0, dec.max()],
             cmap=mglearn.cm2, alpha=0.5)

mglearn.discrete_scatter(X[:, 0], X[:, 1], y)
plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.title("Рис. 5: Граница решения (проекция из 3D) на 2D")
plt.show()

# Часть 2: Ядерный SVM (Kernel SVM)
print("\n--- Ядерный SVM (RBF ядро) ---")

# Пример с RBF ядром на 'handcrafted' датасете (Рис. 6)
X_hc, y_hc = mglearn.tools.make_handcrafted_dataset()
svm = SVC(kernel='rbf', C=10, gamma=0.1).fit(X_hc, y_hc)

plt.figure()

mglearn.plots.plot_2d_separator(svm, X_hc, eps=.5)
mglearn.discrete_scatter(X_hc[:, 0], X_hc[:, 1], y_hc)
sv = svm.support_vectors_
sv_labels = svm.dual_coef_.ravel() > 0
mglearn.discrete_scatter(sv[:, 0], sv[:, 1], sv_labels, s=15, markeredgewidth=3)

plt.xlabel("Признак 0")
plt.ylabel("Признак 1")
plt.title("Рис. 6: Граница RBF SVM и опорные векторы")
plt.show()

# Исследование параметров C и gamma (Рис. 7)
fig, axes = plt.subplots(3, 3, figsize=(15, 10))

for ax_row, C in zip(axes, [-1, 0, 3]):
    for ax, gamma in zip(ax_row, range(-1, 2)):
        mglearn.plots.plot_svm(log_C=C, log_gamma=gamma, ax=ax)

axes[0, 0].legend(["class 0", "class 1", "sv class 0", "sv class 1"],
                  ncol=4, loc=(.9, 1.2))
plt.suptitle("Рис. 7: Влияние параметров C и gamma на RBF SVM")
plt.show()

# Часть 3: Предобработка данных для SVM (на примере Breast Cancer)
print("\n--- Предобработка данных для SVM (Breast Cancer) ---")

cancer = load_breast_cancer()
X_train, X_test, y_train, y_test = train_test_split(
    cancer.data, cancer.target, random_state=0)

svc = SVC()
svc.fit(X_train, y_train)

print("--- Результаты на НЕ масштабированных данных ---")
print("Правильность на обучающем наборе: {:.2f}".format(svc.score(X_train, y_train)))
print("Правильность на тестовом наборе: {:.2f}".format(svc.score(X_test, y_test)))

# --- Визуализация диапазонов признаков (Рис. 8)
plt.figure(figsize=(10, 6))
plt.plot(X_train.min(axis=0), 'o', label="min")
plt.plot(X_train.max(axis=0), '^', label="max")

plt.legend(loc=4)
plt.xlabel("Индекс признака")
plt.ylabel("Величина признака")
plt.yscale("log")
plt.title("Рис. 8: Диапазоны признаков Breast Cancer (лог. шкала)")
plt.show()

min_on_training = X_train.min(axis=0)
range_on_training = (X_train - min_on_training).max(axis=0)

range_on_training[range_on_training == 0] = 1

X_train_scaled = (X_train - min_on_training) / range_on_training

print("\n--- Масштабирование обучающего набора (MinMaxScaler вручную) ---")
print("Минимальное значение для каждого признака:\n{}".format(
    X_train_scaled.min(axis=0)))
print("Максимальное значение для каждого признака:\n {}".format(
    X_train_scaled.max(axis=0)))

X_test_scaled = (X_test - min_on_training) / range_on_training

svc_scaled = SVC()
svc_scaled.fit(X_train_scaled, y_train)

print("\n--- Результаты на МАСШТАБИРОВАННЫХ данных (параметры по умолч.) ---")
print("Правильность на обучающем наборе: {:.3f}".format(
    svc_scaled.score(X_train_scaled, y_train)))
print("Правильность на тестовом наборе: {:.3f}".format(
    svc_scaled.score(X_test_scaled, y_test)))

print("\n--- Результаты на МАСШТАБИРОВАННЫХ данных (C=1000) ---")
svc_scaled_C1000 = SVC(C=1000)
svc_scaled_C1000.fit(X_train_scaled, y_train)

print("Правильность на обучающем наборе: {:.3f}".format(
    svc_scaled_C1000.score(X_train_scaled, y_train)))
print("Правильность на тестовом наборе: {:.3f}".format(
    svc_scaled_C1000.score(X_test_scaled, y_test)))

print("\n--- Конец кода Лабораторной работы №26 ---")