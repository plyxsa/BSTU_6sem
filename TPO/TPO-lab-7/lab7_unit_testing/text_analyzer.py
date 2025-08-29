# Подсчитывает количество строк в тексте
def count_lines(text: str) -> int:
    if not text:
        return 0
    lines = text.splitlines()
    return len(lines)

# Подсчитывает количество слов в тексте
def count_words(text: str) -> int:
    if not text.strip():
        return 0
    words = text.split()
    return len(words)

# Подсчитывает общее количество символов в тексте.
def count_chars(text: str) -> int:
    return len(text)

# Выполняет полный анализ текста (строки, слова, символы)
def analyze_text(text: str) -> dict:
    return {
        'lines': count_lines(text),
        'words': count_words(text),
        'chars': count_chars(text)
    }