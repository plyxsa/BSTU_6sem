# main.py
import sys
import argparse # Используем argparse для удобной работы с аргументами
from file_handler import read_file
from text_analyzer import analyze_text

def main():
    # Настройка парсера аргументов командной строки
    parser = argparse.ArgumentParser(description="Анализатор текстовых файлов: подсчет строк, слов и символов.")
    parser.add_argument("filepath", help="Путь к текстовому файлу для анализа.")

    # Если аргументы не переданы, показать справку и выйти
    if len(sys.argv) == 1:
        parser.print_help(sys.stderr)
        sys.exit(1)

    args = parser.parse_args()
    filepath = args.filepath

    try:
        # 1. Прочитать содержимое файла с помощью file_handler
        content = read_file(filepath)

        # 2. Проанализировать текст с помощью text_analyzer
        analysis_results = analyze_text(content)

        # 3. Вывести результаты
        print(f"Анализ файла: {filepath}")
        print("-" * 30)
        print(f"Количество строк:    {analysis_results['lines']}")
        print(f"Количество слов:     {analysis_results['words']}")
        print(f"Количество символов: {analysis_results['chars']}")
        print("-" * 30)

    except FileNotFoundError as e:
        print(f"Ошибка: {e}", file=sys.stderr)
        sys.exit(1)
    except IOError as e:
        print(f"Ошибка ввода-вывода: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Неожиданная ошибка: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()