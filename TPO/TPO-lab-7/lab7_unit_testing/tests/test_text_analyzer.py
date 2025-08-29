import unittest

from text_analyzer import count_lines, count_words, count_chars, analyze_text

class TestTextAnalyzer(unittest.TestCase):

    def test_count_lines_empty(self):
        self.assertEqual(count_lines(""), 0)

    def test_count_lines_single(self):
        self.assertEqual(count_lines("Одна строка"), 1)

    def test_count_lines_multiple(self):
        self.assertEqual(count_lines("Строка 1\nСтрока 2\nСтрока 3"), 3)

    def test_count_lines_with_trailing_newline(self):
        # splitlines() не создает пустую строку в конце, если есть \n
        self.assertEqual(count_lines("Строка 1\nСтрока 2\n"), 2)

    def test_count_lines_only_newlines(self):
        self.assertEqual(count_lines("\n\n\n"), 3)

    def test_count_words_empty(self):
        self.assertEqual(count_words(""), 0)
        self.assertEqual(count_words("   \t\n "), 0)

    def test_count_words_single(self):
        self.assertEqual(count_words("слово"), 1)

    def test_count_words_multiple(self):
        self.assertEqual(count_words("Несколько простых слов"), 3)

    def test_count_words_with_punctuation(self):
        # Стандартный split() считает пунктуацию частью слова
        self.assertEqual(count_words("Слова, разделенные знаками."), 3)

    def test_count_words_extra_spaces(self):
        self.assertEqual(count_words("  Слова   с  лишними   пробелами "), 4)

    def test_count_chars_empty(self):
        self.assertEqual(count_chars(""), 0)

    def test_count_chars_simple(self):
        self.assertEqual(count_chars("abc"), 3)

    def test_count_chars_with_spaces_and_newlines(self):
        self.assertEqual(count_chars("Строка\nс пробелами"), 18) # Включая \n

    # Тест для комплексной функции analyze_text
    def test_analyze_text_simple(self):
        text = "Первая строка.\nВторая строка."
        expected = {'lines': 2, 'words': 4, 'chars': 29}
        self.assertDictEqual(analyze_text(text), expected)

    def test_analyze_text_empty(self):
        text = ""
        expected = {'lines': 0, 'words': 0, 'chars': 0}
        self.assertDictEqual(analyze_text(text), expected)

    def test_analyze_text_complex(self):
        text = "  Строка 1\n\nСтрока 3 со словами.\n"
        expected = {'lines': 4, 'words': 6, 'chars': 33}
        self.assertDictEqual(analyze_text(text), expected)

if __name__ == '__main__':
    unittest.main()