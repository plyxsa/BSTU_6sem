# pip install sympy numpy matplotlib pandas scipy ipython scikit-learn mglearn
# pip install -r requirements.txt
# sympy библиотека для символьных вычислений
import sympy as sp

x = sp.symbols('x')
f = x**2 + 1
derivative_f = sp.diff(f, x)
print("Производная функции f(x):", derivative_f)

integral_f = sp.integrate(f, (x, 0, 1))
print("Интеграл функции f(x) на отрезке [0, 1]:", integral_f)

f = 1/(x**2 + 1)
f_limit = sp.limit(f, x, sp.oo)
print("Предел функции f(x) при x → ∞:", f_limit)

# numpy — основная библиотека для работы с числовыми данными и многомерными массивами.
import numpy as np

array_1d = np.random.randint(0, 80, size=10)
print("Одномерный массив:", array_1d)

array_2d = array_1d.reshape(2, 5)
print("Двумерный массив:\n", array_2d)

array1, array2 = np.split(array_2d, [3], axis=1)
print("\nПервый массив (3 столбца):\n", array1)
print("\nВторой массив (2 столбца):\n", array2)

matches = np.where(array1 == 5)
if matches[0].size == 0:
    print("Число 5 не найдено в массиве")
else:
    print("Индексы элементов, равных 5:", matches)
    count_matches = len(matches[0])
    print("Количество элементов, равных 5:", count_matches)

min_value = np.min(array2)
max_value = np.max(array2)
mean_value = np.mean(array2)

print("Минимальное значение:", min_value)
print("Максимальное значение:", max_value)
print("Среднее значение:", mean_value)

# pandas библиотека для работы с данными в табличном виде
import pandas as pd

series_from_array = pd.Series(np.random.randint(0, 10, 5))
print("Series из массива NumPy:\n", series_from_array)

series_from_dict = pd.Series({'a': 1, 'b': 2, 'c': 3})
print("Series из словаря:\n", series_from_dict)

result = series_from_array + 10
print("Series после добавления 10:\n", result)

df_from_array = pd.DataFrame(np.random.rand(4, 3), columns=['A', 'B', 'C'])
print("DataFrame из массива NumPy:\n", df_from_array)

df_from_dict = pd.DataFrame({'name': ['Alice', 'Bob'], 'age': [25, 30]})
print("DataFrame из словаря:\n", df_from_dict)

df_from_series = pd.DataFrame({'values': series_from_array})
print("DataFrame из Series:\n", df_from_series)

# matplotlib библиотека для создания статичных, анимированных и интерактивных графиков и визуализаций данных
import matplotlib.pyplot as plt

x_values = np.linspace(-10, 10, 100)
y_values = x_values**3 + 2
plt.plot(x_values, y_values)
plt.title("График функции f(x) = x^3 + 2")
plt.xlabel("x")
plt.ylabel("f(x)")
plt.grid(True)
plt.show()

x = np.linspace(-5, 5, 100)
y = np.linspace(-5, 5, 100)
# создаёт две 2D-матрицы, которые представляют сетку координат для плоскости
x, y = np.meshgrid(x, y)
z = x**2 + 2*y**2 + 1
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.plot_surface(x, y, z, cmap='viridis')
ax.set_title("График функции f(x, y) = x^2 + 2y^2 + 1")
plt.show()

# Столбчатая диаграмма
plt.bar(['A', 'B', 'C'], [30, 15, 20])
plt.title('Столбчатая диаграмма')
plt.show()

# Круговая диаграмма
labels = ['A', 'B', 'C']
sizes = [25, 35, 40]
plt.pie(sizes, labels=labels, autopct='%1.1f%%')
plt.title('Круговая диаграмма')
plt.show()
