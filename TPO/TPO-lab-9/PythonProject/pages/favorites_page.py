from selenium.common import TimeoutException
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait # Явный импорт для лямбды
from .base_page import BasePage

class FavoritesPage(BasePage):
    FAVORITE_ITEM_LINK_TEMPLATE = (By.XPATH, "//a[contains(@class, 'styles_wrapper__') and contains(@href, '/item/{item_id}')]")
    FAVORITE_ITEMS_LIST = (By.CSS_SELECTOR, "a.styles_wrapper__xi81o")
    EMPTY_FAVORITES_MESSAGE = (By.XPATH, "//*[contains(text(),'пока ничего нет') or contains(text(),'здесь пока ничего нет')]") # Сообщение о пустом избранном

    def is_item_in_favorites(self, item_id, item_title=""):
        if not item_id or item_id == "N/A":
            print(f"   - Предоставлен неверный item_id ('{item_id}') для проверки избранного.")
            return False

        try:
            WebDriverWait(self.driver, 10).until(
                lambda d: d.find_elements(*self.FAVORITE_ITEMS_LIST) or \
                          d.find_element(*self.EMPTY_FAVORITES_MESSAGE)
            )
        except TimeoutException:
            print("   - Страница избранного не загрузила товары или сообщение о пустом списке за отведенное время.")
            self.take_screenshot("favorites_page_load_fail")
            return False


        item_locator = (self.FAVORITE_ITEM_LINK_TEMPLATE[0], self.FAVORITE_ITEM_LINK_TEMPLATE[1].format(item_id=item_id))
        print(f"   - Проверяем наличие товара ID {item_id} (Заголовок: '{item_title}') в избранном, используя локатор: {item_locator}")
        found = self.is_element_visible(item_locator, timeout=5)
        if found:
            print(f"   - Товар ID {item_id} ('{item_title}') НАЙДЕН в избранном.")
        else:
            print(f"   - Товар ID {item_id} ('{item_title}') НЕ НАЙДЕН в избранном.")
            self.take_screenshot(f"item_not_in_favorites_{item_id}")
        return found