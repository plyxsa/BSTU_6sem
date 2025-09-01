import mglearn
import matplotlib.pyplot as plt
import numpy as np

# --- Часть 1: Простая линейная регрессия на данных wave ---

# Визуализация линейной регрессии на наборе данных wave (Рис. 1)
print("--- Линейная регрессия на данных 'wave' ---")
mglearn.plots.plot_linear_regression_wave()
plt.title("Рис. 1: Прогнозы линейной модели для набора данных wave") # Добавим заголовок для ясности
plt.show()

# Загрузка данных и разделение на обучающую и тестовую выборки
X, y = mglearn.datasets.make_wave(n_samples=60)
from sklearn.model_selection import train_test_split
X_train, X_test, y_train, y_test = train_test_split(X, y, random_state=42)

# Обучение модели линейной регрессии
from sklearn.linear_model import LinearRegression
lr = LinearRegression().fit(X_train, y_train)

# Вывод коэффициентов (w) и свободного члена (b)
print(f"lr.coef_: {lr.coef_}")
print(f"lr.intercept_: {lr.intercept_}")

# Оценка качества модели (R^2)
print(f"Правильность на обучающем наборе: {lr.score(X_train, y_train):.2f}")
print(f"Правильность на тестовом наборе: {lr.score(X_test, y_test):.2f}")
print("-" * 30)

# --- Часть 2: Линейная регрессия, Ridge и Lasso на данных Boston Housing ---

print("\n--- Модели на данных 'Boston Housing' ---")
# Загрузка расширенного набора данных Boston Housing
X, y = mglearn.datasets.load_extended_boston()
X_train, X_test, y_train, y_test = train_test_split(X, y, random_state=0)

# Обучение обычной линейной регрессии (OLS)
lr = LinearRegression().fit(X_train, y_train)
print("Линейная регрессия (OLS):")
print(f"  Правильность на обучающем наборе: {lr.score(X_train, y_train):.2f}")
print(f"  Правильность на тестовом наборе: {lr.score(X_test, y_test):.2f}")

# --- Ridge регрессия ---
from sklearn.linear_model import Ridge

print("\nRidge регрессия:")
# Ridge с alpha=1.0 (по умолчанию)
ridge = Ridge().fit(X_train, y_train)
print("  alpha=1.0:")
print(f"    Правильность на обучающем наборе: {ridge.score(X_train, y_train):.2f}")
print(f"    Правильность на тестовом наборе: {ridge.score(X_test, y_test):.2f}")

# Ridge с alpha=10
ridge10 = Ridge(alpha=10).fit(X_train, y_train)
print("  alpha=10:")
print(f"    Правильность на обучающем наборе: {ridge10.score(X_train, y_train):.2f}")
print(f"    Правильность на тестовом наборе: {ridge10.score(X_test, y_test):.2f}")

# Ridge с alpha=0.1
ridge01 = Ridge(alpha=0.1).fit(X_train, y_train)
print("  alpha=0.1:")
print(f"    Правильность на обучающем наборе: {ridge01.score(X_train, y_train):.2f}")
print(f"    Правильность на тестовом наборе: {ridge01.score(X_test, y_test):.2f}")

# Визуализация коэффициентов для OLS и Ridge (Рис. 2)
plt.figure(figsize=(10, 6)) # Увеличим размер для читаемости
plt.plot(ridge.coef_, 's', label="Гребневая регрессия alpha=1")
plt.plot(ridge10.coef_, '^', label="Гребневая регрессия alpha=10")
plt.plot(ridge01.coef_, 'v', label="Гребневая регрессия alpha=0.1")
plt.plot(lr.coef_, 'o', label="Линейная регрессия")

plt.xlabel("Индекс коэффициента")
plt.ylabel("Оценка коэффициента")
plt.hlines(0, 0, len(lr.coef_))
plt.ylim(-25, 25)
plt.title("Рис. 2: Сравнение коэффициентов Ridge и OLS")
plt.legend()
plt.show()

# Визуализация кривых обучения (Рис. 3)
print("\nПостроение кривых обучения (Рис. 3)...")
mglearn.plots.plot_ridge_n_samples()
plt.title("Рис. 3: Кривые обучения Ridge и LinearRegression")
plt.show()

# --- Lasso регрессия ---
from sklearn.linear_model import Lasso

print("\nLasso регрессия:")
# Lasso с alpha=1.0 (по умолчанию)
lasso = Lasso().fit(X_train, y_train)
print("  alpha=1.0:")
print(f"    Правильность на обучающем наборе: {lasso.score(X_train, y_train):.2f}")
print(f"    Правильность на контрольном наборе: {lasso.score(X_test, y_test):.2f}")
print(f"    Количество использованных признаков: {np.sum(lasso.coef_ != 0)}")

# Lasso с alpha=0.01
# Увеличиваем max_iter, чтобы избежать предупреждения о сходимости
lasso001 = Lasso(alpha=0.01, max_iter=100000).fit(X_train, y_train)
print("  alpha=0.01:")
print(f"    Правильность на обучающем наборе: {lasso001.score(X_train, y_train):.2f}")
print(f"    Правильность на тестовом наборе: {lasso001.score(X_test, y_test):.2f}")
print(f"    Количество использованных признаков: {np.sum(lasso001.coef_ != 0)}")

# Lasso с alpha=0.0001
lasso00001 = Lasso(alpha=0.0001, max_iter=100000).fit(X_train, y_train)
print("  alpha=0.0001:")
print(f"    Правильность на обучающем наборе: {lasso00001.score(X_train, y_train):.2f}")
print(f"    Правильность на тестовом наборе: {lasso00001.score(X_test, y_test):.2f}")
print(f"    Количество использованных признаков: {np.sum(lasso00001.coef_ != 0)}")

# Визуализация коэффициентов для Lasso и Ridge (Рис. 4)
plt.figure(figsize=(10, 6))
plt.plot(lasso.coef_, 's', label="Лассо alpha=1")
plt.plot(lasso001.coef_, '^', label="Лассо alpha=0.01")
plt.plot(lasso00001.coef_, 'v', label="Лассо alpha=0.0001")
plt.plot(ridge01.coef_, 'o', label="Гребневая регрессия alpha=0.1") # Сравнение с лучшим Ridge

plt.legend(ncol=2, loc=(0, 1.05))
plt.ylim(-25, 25)
plt.xlabel("Индекс коэффициента")
plt.ylabel("Оценка коэффициента")
plt.title("Рис. 4: Сравнение коэффициентов Lasso и Ridge")
plt.hlines(0, 0, len(lr.coef_)) # Горизонтальная линия на нуле
plt.show()

print("\nКод выполнен.")