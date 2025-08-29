import os

# Читает содержимое файла по указанному пути.
def read_file(filepath: str) -> str:
    if not os.path.exists(filepath):
        raise FileNotFoundError(f"Файл не найден: {filepath}")
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        return content
    except IOError as e:
        raise IOError(f"Ошибка чтения файла {filepath}: {e}")
    except Exception as e:
        raise Exception(f"Неожиданная ошибка при чтении файла {filepath}: {e}")