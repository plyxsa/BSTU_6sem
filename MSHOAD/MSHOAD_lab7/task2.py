### 1. Списки (Lists) ###
print("\n=== Работа со списками ===")

# Создание списков
empty_list = []  # Пустой список
numbers = [1, 3, 2, 4, 5]  # Список чисел
fruits = ["apple", "banana", "cherry"]

# Добавление элементов
fruits.append("orange")  # Добавление в конец
print("После append:", fruits)

fruits.insert(1, "blueberry")  # Вставка по индексу
print("После insert:", fruits)

# Удаление элементов
fruits.remove("banana")  # Удаление по значению
print("После remove:", fruits)

popped = fruits.pop(1)  # Удаление по индексу (возвращает элемент)
print("После pop(1):", fruits, ", Удалённый элемент:", popped)

# Сортировка
numbers.sort()  # Сортировка по возрастанию
print("После sort:", numbers)

numbers.reverse()  # Разворот списка
print("После reverse:", numbers)

# Копирование
numbers_copy = numbers.copy()  # Поверхностная копия
print("Копия numbers:", numbers_copy)

# Поиск и подсчёт
print("Индекс 'cherry':", fruits.index("cherry"))
print("Количество 'apple':", fruits.count("apple"))

# Очистка списка
numbers.clear()
print("После clear:", numbers)

### 2. Кортежи (Tuples) ###
print("\n=== Работа с кортежами ===")

# Создание кортежей
empty_tuple = ()  # Пустой кортеж
point = (10, 20)  # Кортеж с числами
days = ("Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun")  # Кортеж строк

# Распаковка
x, y = point
print(f"Распаковка кортежа: x={x}, y={y}")

# Конкатенация
new_tuple = point + (30,)
print("После конкатенации:", new_tuple)

# Поиск и подсчёт
print("Индекс 'Wed':", days.index("Wed"))
print("Количество 'Mon':", days.count("Mon"))

### 3. Словари (Dictionaries) ###
print("\n=== Работа со словарями ===")

# Создание словарей
empty_dict = {}  # Пустой словарь
person = {"name": "Alice", "age": 25, "city": "New York"}

# Доступ и добавление элементов
print("Имя:", person.get("name"))
person["email"] = "alice@example.com"
print("После добавления email:", person)

# Обновление значения
person["age"] = 26
print("После обновления age:", person)

# Удаление элементов
del person["city"]
print("После del:", person)

popped_value = person.pop("age")
print(f"После pop('age'): {person}, Удалённое значение: {popped_value}")

# Получение ключей, значений и пар
print("Ключи:", person.keys())
print("Значения:", person.values())
print("Пары ключ-значение:", person.items())

# Объединение словарей
extra_info = {"job": "Engineer", "hobby": "Reading"}
person.update(extra_info)
print("После update:", person)

### 4. Множества (Sets) ###
print("\n=== Работа с множествами ===")

# Создание множеств
empty_set = set()  # Пустое множество
fruits_set = {"apple", "banana", "cherry"}

# Добавление и удаление элементов
fruits_set.add("orange")
print("После add:", fruits_set)

fruits_set.remove("banana")  # Удаление (ошибка, если нет)
print("После remove('banana'):", fruits_set)

fruits_set.discard("kiwi")  # Удаление (без ошибки, если нет)
print("После discard('kiwi'):", fruits_set)

# Операции с множествами
set1 = {1, 2, 3}
set2 = {3, 4, 5}

print("Объединение:", set1 | set2)
print("Пересечение:", set1 & set2)
print("Разность (set1 - set2):", set1 - set2)
print("Симметричная разность:", set1 ^ set2)
