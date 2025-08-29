import time
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, ElementClickInterceptedException

class BasePage:
    def __init__(self, driver, base_url="https://www.kufar.by/"):
        self.driver = driver
        self.base_url = base_url
        self.timeout = 10

    def open(self, url=""):
        self.driver.get(self.base_url + url)
        self.handle_cookie_consent()

    def find_element(self, locator, timeout=None):
        if timeout is None:
            timeout = self.timeout
        return WebDriverWait(self.driver, timeout).until(
            EC.visibility_of_element_located(locator)
        )

    def find_elements(self, locator, timeout=None):
        if timeout is None:
            timeout = self.timeout
        return WebDriverWait(self.driver, timeout).until(
            EC.visibility_of_all_elements_located(locator)
        )

    def click(self, locator, timeout=None):
        if timeout is None:
            timeout = self.timeout
        try:
            element = WebDriverWait(self.driver, timeout).until(
                EC.element_to_be_clickable(locator)
            )
            element.click()
        except ElementClickInterceptedException:
            print(f"   - ElementClickInterceptedException для {locator}, пробую JS клик.")
            element = WebDriverWait(self.driver, timeout).until(
                EC.presence_of_element_located(locator)
            )
            self.driver.execute_script("arguments[0].click();", element)
        except TimeoutException:
            raise TimeoutException(f"Элемент {locator} не стал кликабельным за {timeout} секунд.")


    def type_text(self, locator, text, timeout=None):
        element = self.find_element(locator, timeout)
        element.clear()
        element.send_keys(text)

    def get_text(self, locator, timeout=None):
        return self.find_element(locator, timeout).text

    def is_element_visible(self, locator, timeout=1):
        try:
            WebDriverWait(self.driver, timeout).until(EC.visibility_of_element_located(locator))
            return True
        except TimeoutException:
            return False

    def is_element_invisible(self, locator, timeout=10):
        try:
            WebDriverWait(self.driver, timeout).until(EC.invisibility_of_element_located(locator))
            return True
        except TimeoutException:
            return False

    def wait_for_element_to_be_clickable(self, locator, timeout=None):
        if timeout is None:
            timeout = self.timeout
        return WebDriverWait(self.driver, timeout).until(
            EC.element_to_be_clickable(locator)
        )

    def scroll_to_element(self, locator_or_element):
        if isinstance(locator_or_element, tuple): # если это локатор
            element = self.find_element(locator_or_element)
        else: # если это уже элемент
            element = locator_or_element
        self.driver.execute_script("arguments[0].scrollIntoViewIfNeeded(true);", element)
        time.sleep(0.5)

    def handle_cookie_consent(self, timeout=5):
        from selenium.webdriver.common.by import By
        cookie_accept_button_locator = (By.XPATH, "//button[normalize-space()='Принять']")
        try:
            accept_button = WebDriverWait(self.driver, timeout).until(
                EC.element_to_be_clickable(cookie_accept_button_locator)
            )
            print("   - Найдена кнопка 'Принять' куки. Нажимаем...")
            accept_button.click()
            time.sleep(0.5)
            print("   - Кнопка 'Принять' куки нажата.")
            return True
        except TimeoutException:
            print("   - Всплывающее окно куки не найдено или кнопка не кликабельна (Таймаут).")
            return False
        except Exception as e:
            print(f"   - Ошибка при клике на кнопку куки: {e}")
            return False

    def take_screenshot(self, name_prefix="screenshot"):
        timestamp = time.strftime("%Y%m%d-%H%M%S")
        import os
        if not os.path.exists("screenshots"):
            os.makedirs("screenshots")
        screenshot_name = f"screenshots/{name_prefix}_{timestamp}.png"
        self.driver.save_screenshot(screenshot_name)
        print(f"   !!! Скриншот сохранен: {screenshot_name}")
        return screenshot_name