from selenium.webdriver.common.by import By
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.support.wait import WebDriverWait
import time

from .base_page import BasePage

class SearchResultsPage(BasePage):
    RESULTS_HEADER_DYNAMIC = (By.XPATH, "//h1[contains(@class, 'styles_title')] | //h3[contains(@class, 'styles_title')]")
    ALL_AD_LINKS = (By.CSS_SELECTOR, "section > a[data-testid*='ad']")
    PHOTO_FILTER_LABEL = (By.XPATH, "//label[@for='oph']")
    SHOW_RESULTS_BUTTON = (By.CSS_SELECTOR, "button[data-name='filter-submit-button']")
    COUNT_ELEMENT = (By.CSS_SELECTOR, "button[data-cy='listing-tab-all-goods'] span.styles_counter__NSFh7")
    ITEM_LINK_BY_TITLE_TEMPLATE = "//section/a[contains(@data-testid, 'ad')]//h3[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ', 'abcdefghijklmnopqrstuvwxyzабвгдеёжзийклмнопрстуфхцчшщъыьэюя'),'{title_lower}')]/ancestor::a[contains(@data-testid, 'ad')]"


    def get_results_header_text(self, search_term):
        specific_header_locator = (
            By.XPATH,
            f"//h1[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ', 'abcdefghijklmnopqrstuvwxyzабвгдеёжзийклмнопрстуфхцчшщъыьэюя'),'{search_term.lower()}')] | "
            f"//h3[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ', 'abcdefghijklmnopqrstuvwxyzабвгдеёжзийклмнопрстуфхцчшщъыьэюя'),'{search_term.lower()}')]"
        )
        return self.get_text(specific_header_locator)

    def select_item_by_name_contains(self, item_name_part):
        item_name_lower = item_name_part.lower()
        item_locator_xpath = self.ITEM_LINK_BY_TITLE_TEMPLATE.format(title_lower=item_name_lower)
        item_locator = (By.XPATH, item_locator_xpath)

        print(f"   - Ищем ссылку на товар с XPath: {item_locator_xpath}")
        try:
            original_window = self.driver.current_window_handle
            all_windows_before_click = self.driver.window_handles

            item_link_element = self.find_element(item_locator)
            item_title = item_link_element.find_element(By.XPATH, ".//h3").text
            print(f"   - Найден товар '{item_title}', кликаем...")
            self.scroll_to_element(item_link_element)

            self.driver.execute_script("arguments[0].click();", item_link_element)
            print(f"   - Клик по товару '{item_title}' выполнен.")

            time.sleep(1)

            all_windows_after_click = self.driver.window_handles
            print(f"DEBUG: Окна до клика: {all_windows_before_click}, Окна после клика: {all_windows_after_click}")

            if len(all_windows_after_click) > len(all_windows_before_click):
                print("   - Обнаружено открытие новой вкладки/окна.")
                new_window = [window for window in all_windows_after_click if window not in all_windows_before_click][0]
                if new_window:
                    print(f"   - Переключаемся на новую вкладку: {new_window}")
                    self.driver.switch_to.window(new_window)

                    WebDriverWait(self.driver, self.timeout).until(
                        lambda d: d.execute_script('return document.readyState') == 'complete'
                    )
                    print(
                        f"   - Успешно переключились и дождались загрузки новой вкладки. Текущий URL: {self.driver.current_url}")
                else:
                    print("   - Новая вкладка была определена, но не удалось получить ее хэндл (странная ситуация).")
            elif self.driver.current_url == self.base_url or "search" in self.driver.current_url:
                print("   - Новая вкладка не открылась ИЛИ мы все еще на странице поиска. Это может быть проблемой.")

            return item_title
        except TimeoutException:
            print(
                f"   - Не удалось найти товар, содержащий '{item_name_part}' в заголовке на первой странице результатов (Таймаут).")
            self.take_screenshot(f"select_item_by_name_fail_{item_name_part.replace(' ', '_')}")
            return None
        except Exception as e:
            print(f"   - Произошла ошибка при выборе товара: {e}")
            self.take_screenshot(f"select_item_error_{item_name_part.replace(' ', '_')}")
            return None


    def apply_photo_filter_and_show(self):
        self.click(self.PHOTO_FILTER_LABEL)
        print("   - Фильтр 'Только с фото' выбран.")
        self.click(self.SHOW_RESULTS_BUTTON)
        print("   - Кнопка 'Показать объявления' нажата.")


    def get_results_count_text(self):
        try:
            return self.get_text(self.COUNT_ELEMENT)
        except TimeoutException:
            print("   - Не удалось найти элемент счетчика объявлений на странице результатов.")
            return "N/A"