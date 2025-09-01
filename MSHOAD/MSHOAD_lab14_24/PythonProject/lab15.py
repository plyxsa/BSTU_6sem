import mglearn
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
import graphviz
import pydotplus
from sklearn.tree import DecisionTreeClassifier # Классификатор
from sklearn.tree import DecisionTreeRegressor # Регрессор
from sklearn.tree import export_graphviz # Функция для экспорта дерева
from sklearn.datasets import load_breast_cancer # Пример набора данных
from sklearn.model_selection import train_test_split # Разделение данных
from sklearn.linear_model import LinearRegression # Для сравнения в регрессии
from IPython.display import display, Image

# Код для демонстрации работы BernoulliNB
print("Блок из ЛР23 (BernoulliNB)")
X_nb = np.array([[0, 1, 0, 1], [1, 0, 1, 1], [0, 0, 0, 1], [1, 0, 1, 0]])
y_nb = np.array ([0, 1, 0, 1])
counts = {}
for label in np.unique(y_nb):
    counts[label] = X_nb[y_nb == label].sum(axis=0)
print("Частоты признаков (из ЛР23):\n{}".format(counts))
print("-" * 20)

# Пример 1: Визуализация простого дерева (классификация животных)
print("--- Пример: Дерево классификации животных ---")
try:
    mglearn.plots.plot_animal_tree()
    plt.title("Дерево классификации животных (mglearn)")
    plt.show()
except Exception as e:
    print(f"Ошибка при вызове mglearn.plots.plot_animal_tree(): {e}")
    print("Убедитесь, что Graphviz установлен и прописан в PATH.")

# Пример 2: Визуализация пошагового построения дерева (стр. 16)
print("\n--- Пример: Пошаговое построение дерева ---")
try:
    mglearn.plots.plot_tree_progressive()
except Exception as e:
    print(f"Ошибка при вызове mglearn.plots.plot_tree_progressive(): {e}")
    print("Убедитесь, что Graphviz установлен и прописан в PATH.")

# Пример 3: Классификация рака груди (без обрезки)
print("\n--- Пример: Классификация рака груди (без обрезки) ---")
cancer = load_breast_cancer()
X_train, X_test, y_train, y_test = train_test_split(
    cancer.data, cancer.target, stratify=cancer.target, random_state=42)

# Обучаем необрезанное дерево
tree = DecisionTreeClassifier(random_state=0)
tree.fit(X_train, y_train)

print("Правильность на обучающем наборе: {:.3f}".format(tree.score(X_train, y_train)))
print("Правильность на тестовом наборе: {:.3f}".format(tree.score(X_test, y_test)))

# Пример 4: Классификация рака груди (с предварительной обрезкой)
print("\n--- Пример: Классификация рака груди (с обрезкой max_depth=4) ---")
# Обучаем дерево с ограничением глубины
tree = DecisionTreeClassifier(max_depth=4, random_state=0)
tree.fit(X_train, y_train)

print("Правильность на обучающем наборе: {:.3f}".format(tree.score(X_train, y_train)))
print("Правильность на тестовом наборе: {:.3f}".format(tree.score(X_test, y_test)))

# Пример 5: Визуализация дерева решений (обрезанного) с помощью graphviz
print("\n--- Пример: Визуализация дерева (graphviz) ---")
if graphviz:
    try:
        # Экспорт обрезанного дерева (переменная tree теперь содержит его)
        export_graphviz(tree, out_file="tree.dot",
                        class_names=["malignant", "benign"],
                        feature_names=cancer.feature_names,
                        impurity=False,
                        filled=True)

        # Чтение файла .dot и отображение с помощью graphviz
        with open("tree.dot") as f:
            dot_graph = f.read()
        graphviz_source = graphviz.Source(dot_graph)
        print("Визуализация graphviz создана. Попытка отображения (требуется Jupyter/IPython):")
        display(graphviz_source)

    except Exception as e:
        print(f"Ошибка при визуализации через graphviz: {e}")
        print("Убедитесь, что Graphviz установлен и прописан в PATH.")
else:
    print("Пропуск визуализации через graphviz (библиотека не импортирована).")


# Альтернативная визуализация с помощью pydotplus (сохранение в PDF/PNG)
print("\n--- Пример: Экспорт как PDF (pydotplus) ---")
if pydotplus and graphviz:
    try:
        dot_data = export_graphviz(tree, out_file=None,
                                   feature_names=cancer.feature_names,
                                   class_names=cancer.target_names,
                                   filled=True, rounded=True,
                                   special_characters=True)
        graph = pydotplus.graph_from_dot_data(dot_data)
        graph.write_pdf("cancer.pdf")
        print("Дерево сохранено в cancer.pdf")
    except Exception as e:
        print(f"Ошибка при экспорте в PDF через pydotplus: {e}")
else:
    print("Пропуск экспорта в PDF (pydotplus или graphviz недоступны).")

print("\n--- Пример: Визуализация через pydotplus/IPython ---")
if pydotplus and graphviz:
    try:
        dot_data = export_graphviz(tree, out_file=None,
                                   feature_names=cancer.feature_names,
                                   class_names=cancer.target_names,
                                   filled=True, rounded=True,
                                   special_characters=True)
        graph = pydotplus.graph_from_dot_data(dot_data)
        print("Попытка отобразить дерево через pydotplus/Image:")
        display(Image(graph.create_png()))
    except Exception as e:
         print(f"Ошибка при визуализации через pydotplus/Image: {e}")
else:
    print("Пропуск визуализации через pydotplus (библиотеки недоступны).")


# Пример 6: Важность признаков (стр. 17)
print("\n--- Пример: Важность признаков ---")
tree_importance = DecisionTreeClassifier(random_state=0).fit(X_train, y_train)
print("Важности признаков:\n{}".format(tree_importance.feature_importances_))

# Функция для построения графика важности признаков
def plot_feature_cancer(model):
    n_features = cancer.data.shape[1]
    plt.figure(figsize=(10, 8))
    plt.barh(range(n_features), model.feature_importances_, align='center')
    plt.yticks(np.arange(n_features), cancer.feature_names)
    plt.xlabel("Важность признака") # Названия осей как в лабе
    plt.ylabel("Признак")
    plt.ylim(-1, n_features)
    plt.tight_layout()

plot_feature_cancer(tree_importance)
plt.title("Важность признаков (необрезанное дерево)")
plt.show()

# Пример 7: Немонотонная взаимосвязь (стр. 17)
print("\n--- Пример: Немонотонная взаимосвязь ---")
try:
    # Генерация и отображение данных + дерева
    tree_non_mono_viz = mglearn.plots.plot_tree_not_monotone()
    plt.title("Визуализация немонотонной зависимости (mglearn)")
    plt.show()
    print("Структура дерева для визуализации (mglearn):")
    display(tree_non_mono_viz)
    print("Примечание: Важности признаков не могут быть получены напрямую из mglearn.plots.plot_tree_not_monotone().")
    print("Эта функция возвращает объект визуализации, а не обученную модель.")

except Exception as e:
    print(f"Ошибка при вызове mglearn.plots.plot_tree_not_monotone(): {e}")
    print("Убедитесь, что Graphviz установлен и прописан в PATH.")

# Пример 8: Регрессия с помощью дерева (цены на RAM)
print("\n--- Пример: Регрессия дерева (цены RAM) ---")
ram_file_path = "ram_price.csv"
try:
    ram_prices = pd.read_csv(ram_file_path)

    plt.figure(figsize=(10, 6))
    plt.semilogy(ram_prices.date, ram_prices.price) # Логарифмическая шкала по Y
    plt.xlabel("Год")
    plt.ylabel("Цена $/Мбайт")
    plt.title("Исторические цены на RAM")
    plt.grid(True)
    plt.show()

    data_train = ram_prices[ram_prices.date < 2000]
    data_test = ram_prices[ram_prices.date >= 2000]
    y_train = np.log(data_train.price)
    X_train = data_train.date.values[:, np.newaxis]

    tree_reg = DecisionTreeRegressor().fit(X_train, y_train) # Не задаем max_depth, как в лабе
    linear_reg = LinearRegression().fit(X_train, y_train)

    X_all = ram_prices.date.values[:, np.newaxis]
    pred_tree = tree_reg.predict(X_all)
    pred_lr = linear_reg.predict(X_all)

    price_tree = np.exp(pred_tree)
    price_lr = np.exp(pred_lr)

    plt.figure(figsize=(10, 6))
    plt.semilogy(data_train.date, data_train.price, label="Обучающие данные") # Метки как в лабе
    plt.semilogy(data_test.date, data_test.price, label="Тестовые данные")
    plt.semilogy(ram_prices.date, price_tree, label="Прогнозы дерева")
    plt.semilogy(ram_prices.date, price_lr, label="Прогнозы линейной регрессии")
    plt.legend()
    plt.xlabel("Год") # Добавим оси для полноты
    plt.ylabel("Цена $/Мбайт (лог. шкала)")
    plt.title("Сравнение прогнозов")
    plt.grid(True)
    plt.show()

except FileNotFoundError:
    print(f"Ошибка: Файл '{ram_file_path}' не найден. Пропустили пример с регрессией цен RAM.")
except Exception as e:
    print(f"Произошла ошибка в примере с регрессией цен RAM: {e}")

print("\n--- Конец кода Лабораторной работы №24 ---")