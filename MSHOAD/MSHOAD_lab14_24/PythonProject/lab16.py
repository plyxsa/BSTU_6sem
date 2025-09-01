import mglearn
import matplotlib.pyplot as plt
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.ensemble import GradientBoostingClassifier
from sklearn.datasets import make_moons
from sklearn.datasets import load_breast_cancer
from sklearn.model_selection import train_test_split

# Часть 1
print("--- Случайный лес (Random Forest) ---")

# Создание и разделение датасета make_moons
X_moons, y_moons = make_moons(n_samples=100, noise=0.25, random_state=3)
# Разделяем данные на обучающую и тестовую выборки
X_train_m, X_test_m, y_train_m, y_test_m = train_test_split(
    X_moons, y_moons, stratify=y_moons, random_state=42
    )
X_train, y_train = X_train_m, y_train_m

# Обучение модели случайного леса
forest = RandomForestClassifier(n_estimators=5, random_state=2)
forest.fit(X_train, y_train)

# Визуализация границ решений для отдельных деревьев и ансамбля
fig, axes = plt.subplots(2, 3, figsize=(20, 10))

for i, (ax, tree) in enumerate(zip(axes.ravel(), forest.estimators_)):
    ax.set_title("Дерево {}".format(i))
    mglearn.plots.plot_tree_partition(X_train, y_train, tree, ax=ax)

mglearn.plots.plot_2d_separator(forest, X_train, fill=True, ax=axes[-1, -1], alpha=.4)
axes[-1, -1].set_title("Случайный лес")

mglearn.discrete_scatter(X_train[:, 0], X_train[:, 1], y_train, ax=axes[-1, -1]) # Добавляем точки на последний график

plt.suptitle("Границы решений 5 деревьев и случайного леса (make_moons)")
plt.show()

# Применение случайного леса к датасету Breast Cancer
print("\n--- Случайный лес (Breast Cancer) ---")
cancer = load_breast_cancer()

X_train_c, X_test_c, y_train_c, y_test_c = train_test_split(
    cancer.data, cancer.target, random_state=0, stratify=cancer.target
    )

forest_cancer = RandomForestClassifier(n_estimators=100, random_state=0)
forest_cancer.fit(X_train_c, y_train_c)

print("Правильность на обучающем наборе (RF): {:.3f}".format(forest_cancer.score(X_train_c, y_train_c)))
print("Правильность на тестовом наборе (RF): {:.3f}".format(forest_cancer.score(X_test_c, y_test_c)))

def plot_feature_importances_cancer(model):
    n_features = cancer.data.shape[1]
    plt.figure(figsize=(10, 8))
    plt.barh(range(n_features), model.feature_importances_, align='center')
    plt.yticks(np.arange(n_features), cancer.feature_names)
    plt.xlabel("Важность признака")
    plt.ylabel("Признак")
    plt.ylim(-1, n_features)
    plt.tight_layout()

plot_feature_importances_cancer(forest_cancer)
plt.title("Важность признаков (Случайный лес, Breast Cancer)")
plt.show()

# Часть 2: Градиентный бустинг (Gradient Boosting)
print("\n--- Градиентный бустинг (Gradient Boosting) ---")

# Применение градиентного бустинга к датасету Breast Cancer
print("\n--- GB: Параметры по умолчанию ---")
gbrt = GradientBoostingClassifier(random_state=0)
gbrt.fit(X_train_c, y_train_c)

print("Правильность на обучающем наборе (GB по умолч.): {:.3f}".format(gbrt.score(X_train_c, y_train_c)))
print("Правильность на тестовом наборе (GB по умолч.): {:.3f}".format(gbrt.score(X_test_c, y_test_c)))

# Уменьшение переобучения: Ограничение глубины
print("\n--- GB: Ограничение max_depth=1 ---")
gbrt_depth1 = GradientBoostingClassifier(random_state=0, max_depth=1)
gbrt_depth1.fit(X_train_c, y_train_c)

print("Правильность на обучающем наборе (GB max_depth=1): {:.3f}".format(gbrt_depth1.score(X_train_c, y_train_c)))
print("Правильность на тестовом наборе (GB max_depth=1): {:.3f}".format(gbrt_depth1.score(X_test_c, y_test_c)))

# Уменьшение переобучения: Снижение скорости обучения
print("\n--- GB: Снижение learning_rate=0.01 ---")
gbrt_lr = GradientBoostingClassifier(random_state=0, learning_rate=0.01)
gbrt_lr.fit(X_train_c, y_train_c)

print("Правильность на обучающем наборе (GB lr=0.01): {:.3f}".format(gbrt_lr.score(X_train_c, y_train_c)))
print("Правильность на тестовом наборе (GB lr=0.01): {:.3f}".format(gbrt_lr.score(X_test_c, y_test_c)))

print("\n--- GB: Важность признаков (max_depth=1) ---")
gbrt_final_importance = GradientBoostingClassifier(random_state=0, max_depth=1)
gbrt_final_importance.fit(X_train_c, y_train_c)

plot_feature_importances_cancer(gbrt_final_importance)
plt.title("Важность признаков (Градиентный бустинг max_depth=1, Breast Cancer)")
plt.show()

print("\n--- Конец кода Лабораторной работы №25 ---")