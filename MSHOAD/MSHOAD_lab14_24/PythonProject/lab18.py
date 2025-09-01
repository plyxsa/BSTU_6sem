import mglearn
import matplotlib.pyplot as plt
import numpy as np
from sklearn.neural_network import MLPClassifier
from sklearn.datasets import make_moons
from sklearn.datasets import load_breast_cancer
from sklearn.model_selection import train_test_split
try:
    from IPython.display import display
    ipython_display_available = True
except ImportError:
    ipython_display_available = False
    display = print

# График функций активации (Рис. 3)
plt.figure()
line = np.linspace(-3, 3, 100)
plt.plot(line, np.tanh(line), label="tanh")
plt.plot(line, np.maximum(line, 0), label="relu") # ReLU
plt.legend(loc="best")
plt.xlabel("x")
plt.ylabel("relu(x), tanh(x)")
plt.title("Рис. 3: Функции активации tanh и ReLU")
plt.show()

# График MLP с двумя скрытыми слоями (Рис. 4)
try:
    mglearn.plots.plot_two_hidden_layer_graph()
except Exception as e: print(f"Ошибка визуализации: {e}")
print("-" * 20)

print("\n--- Настройка MLP на make_moons ---")
X_m, y_m = make_moons(n_samples=100, noise=0.25, random_state=3)
X_train_m, X_test_m, y_train_m, y_test_m = train_test_split(X_m, y_m, stratify=y_m, random_state=42)

mlp_default = MLPClassifier(solver='lbfgs', random_state=0).fit(X_train_m, y_train_m)
plt.figure()
mglearn.plots.plot_2d_separator(mlp_default, X_train_m, fill=True, alpha=.3)
mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m)
plt.xlabel("Признак 0"); plt.ylabel("Признак 1")
plt.title("Рис. 5: MLP (100 скрытых нейронов, relu)")
plt.show()

# Уменьшение числа нейронов до 10
mlp_10 = MLPClassifier(solver='lbfgs', random_state=0, hidden_layer_sizes=[10])
mlp_10.fit(X_train_m, y_train_m)
plt.figure()
mglearn.plots.plot_2d_separator(mlp_10, X_train_m, fill=True, alpha=.3)
mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m)
plt.xlabel("Признак 0"); plt.ylabel("Признак 1")
plt.title("Рис. 6: MLP (10 скрытых нейронов, relu)")
plt.show()

# Использование двух скрытых слоев по 10 нейронов (relu по умолчанию)
mlp_10_10_relu = MLPClassifier(solver='lbfgs', random_state=0, hidden_layer_sizes=[10, 10])
mlp_10_10_relu.fit(X_train_m, y_train_m)
plt.figure()
mglearn.plots.plot_2d_separator(mlp_10_10_relu, X_train_m, fill=True, alpha=.3)
mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m)
plt.xlabel("Признак 0"); plt.ylabel("Признак 1")
plt.title("Рис. 7*: MLP (2x10 скрытых нейронов, relu)")
plt.show()

# Использование двух скрытых слоев по 10 нейронов и активации tanh
mlp_10_10_tanh = MLPClassifier(solver='lbfgs', activation='tanh',
                               random_state=0, hidden_layer_sizes=[10, 10])
mlp_10_10_tanh.fit(X_train_m, y_train_m)
plt.figure()
mglearn.plots.plot_2d_separator(mlp_10_10_tanh, X_train_m, fill=True, alpha=.3)
mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m)
plt.xlabel("Признак 0"); plt.ylabel("Признак 1")
plt.title("Рис. 8: MLP (2x10 скрытых нейронов, tanh)")
plt.show()

# Исследование влияния alpha (регуляризация) (Рис. 9)
fig, axes = plt.subplots(2, 4, figsize=(20, 8))
for axx, n_hidden_nodes in zip(axes, [10, 100]):
    for ax, alpha in zip(axx, [0.0001, 0.01, 0.1, 1]):
        mlp_alpha = MLPClassifier(solver='lbfgs', random_state=0,
                                  hidden_layer_sizes=[n_hidden_nodes, n_hidden_nodes],
                                  alpha=alpha)
        mlp_alpha.fit(X_train_m, y_train_m)
        mglearn.plots.plot_2d_separator(mlp_alpha, X_train_m, fill=True, alpha=.3, ax=ax)
        mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m, ax=ax)
        ax.set_title("n_hidden=[{}, {}]\nalpha={:.4f}".format(
                      n_hidden_nodes, n_hidden_nodes, alpha))
plt.suptitle("Рис. 9: Влияние alpha и числа нейронов")
plt.tight_layout(rect=[0, 0.03, 1, 0.95])
plt.show()

# Исследование влияния random_state (инициализация весов) (Рис. 10)
fig, axes = plt.subplots(2, 4, figsize=(20, 8))
for i, ax in enumerate(axes.ravel()):
    mlp_rs = MLPClassifier(solver='lbfgs', random_state=i,
                           hidden_layer_sizes=[100, 100])
    mlp_rs.fit(X_train_m, y_train_m)
    mglearn.plots.plot_2d_separator(mlp_rs, X_train_m, fill=True, alpha=.3, ax=ax)
    mglearn.discrete_scatter(X_train_m[:, 0], X_train_m[:, 1], y_train_m, ax=ax)
    ax.set_title(f"random_state={i}")
plt.suptitle("Рис. 10: Влияние случайной инициализации (random_state)")
plt.tight_layout(rect=[0, 0.03, 1, 0.95])
plt.show()

# Применение MLP к датасету Breast Cancer
print("\n--- MLP на Breast Cancer ---")
cancer = load_breast_cancer()
print("Максимальные значения характеристик:\n{}".format(cancer.data.max(axis=0)))
X_train_c, X_test_c, y_train_c, y_test_c = train_test_split(
    cancer.data, cancer.target, random_state=0)

# Обучение на не масштабированных данных
mlp_cancer_unscaled = MLPClassifier(random_state=42)
mlp_cancer_unscaled.fit(X_train_c, y_train_c)
print("\n--- Результаты на НЕ масштабированных данных ---")
print("Правильность на обучающем наборе: {:.2f}".format(mlp_cancer_unscaled.score(X_train_c, y_train_c)))
print("Правильность на тестовом наборе: {:.2f}".format(mlp_cancer_unscaled.score(X_test_c, y_test_c)))

# Масштабирование данных (StandardScaler вручную)
mean_on_train = X_train_c.mean(axis=0)
std_on_train = X_train_c.std(axis=0)
X_train_scaled = (X_train_c - mean_on_train) / std_on_train
X_test_scaled = (X_test_c - mean_on_train) / std_on_train

# Обучение на масштабированных данных
print("\n--- Результаты на МАСШТАБИРОВАННЫХ данных ---")
mlp_cancer_scaled = MLPClassifier(random_state=0)
mlp_cancer_scaled.fit(X_train_scaled, y_train_c)
print("Правильность на обучающем наборе: {:.3f}".format(mlp_cancer_scaled.score(X_train_scaled, y_train_c)))
print("Правильность на тестовом наборе: {:.3f}".format(mlp_cancer_scaled.score(X_test_scaled, y_test_c)))

# Увеличение числа итераций
print("\n--- Результаты на МАСШТАБИРОВАННЫХ данных (max_iter=1000) ---")
mlp_cancer_iter = MLPClassifier(max_iter=1000, random_state=0)
mlp_cancer_iter.fit(X_train_scaled, y_train_c)
print("Правильность на обучающем наборе: {:.3f}".format(mlp_cancer_iter.score(X_train_scaled, y_train_c)))
print("Правильность на тестовом наборе: {:.3f}".format(mlp_cancer_iter.score(X_test_scaled, y_test_c)))

# Увеличение alpha (регуляризация)
print("\n--- Результаты на МАСШТАБИРОВАННЫХ данных (max_iter=1000, alpha=1) ---")
mlp_cancer_alpha = MLPClassifier(max_iter=1000, alpha=1, random_state=0)
mlp_cancer_alpha.fit(X_train_scaled, y_train_c)
print("Правильность на обучающем наборе: {:.3f}".format(mlp_cancer_alpha.score(X_train_scaled, y_train_c)))
print("Правильность на тестовом наборе: {:.3f}".format(mlp_cancer_alpha.score(X_test_scaled, y_test_c)))

# --- Визуализация весов первого слоя (Рис. 11) ---
print("\n--- Визуализация весов первого слоя ---")
plt.figure(figsize=(20, 5))
plt.imshow(mlp_cancer_alpha.coefs_[0], interpolation='none', cmap='viridis')
plt.yticks(range(cancer.data.shape[1]), cancer.feature_names)
plt.xlabel("Столбцы матрицы весов (скрытые нейроны)")
plt.ylabel("Входная характеристика")
plt.title("Рис. 11: Теплокарта весов первого слоя MLP (alpha=1)")
plt.colorbar()
plt.show()