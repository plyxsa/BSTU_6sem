import unittest
from unittest.mock import patch, mock_open

# import sys
# import os
# sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from file_handler import read_file

class TestFileHandler(unittest.TestCase):

    # Тест успешного чтения файла
    @patch("file_handler.os.path.exists")
    @patch("builtins.open", new_callable=mock_open, read_data="Тестовое содержимое\nФайла")
    def test_read_file_success(self, mock_file_open, mock_exists):
        mock_exists.return_value = True # Говорим, что файл существует
        filepath = "dummy/path/to/file.txt"
        content = read_file(filepath)

        mock_file_open.assert_called_once_with(filepath, 'r', encoding='utf-8')
        self.assertEqual(content, "Тестовое содержимое\nФайла")
        mock_exists.assert_called_once_with(filepath)


    # Тест на ошибку FileNotFoundError
    @patch("file_handler.os.path.exists")
    def test_read_file_not_found(self, mock_exists):
        mock_exists.return_value = False # Говорим, что файл НЕ существует
        filepath = "non_existent_file.txt"

        with self.assertRaises(FileNotFoundError) as cm:
            read_file(filepath)

        self.assertEqual(
            str(cm.exception),
            f"Файл не найден: {filepath}"
        )
        # Убедимся, что open() не вызывался, т.к. проверка не пройдена
        mock_exists.assert_called_once_with(filepath)

    # Тест на ошибку IOError при чтении
    @patch("file_handler.os.path.exists")
    @patch("builtins.open", new_callable=mock_open)
    def test_read_file_io_error(self, mock_file_open, mock_exists):
        mock_exists.return_value = True # Файл существует
        filepath = "dummy/path/to/error_file.txt"

        mock_file_handle = mock_file_open.return_value.__enter__.return_value
        mock_file_handle.read.side_effect = IOError("Ошибка чтения диска")

        with self.assertRaises(IOError) as cm:
            read_file(filepath)

        self.assertIn(f"Ошибка чтения файла {filepath}", str(cm.exception))
        self.assertIn("Ошибка чтения диска", str(cm.exception))

        mock_exists.assert_called_once_with(filepath)
        mock_file_open.assert_called_once_with(filepath, 'r', encoding='utf-8')

if __name__ == '__main__':
    unittest.main()