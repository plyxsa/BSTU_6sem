from selenium.webdriver.common.by import By
from .base_page import BasePage

class LoginModalPage(BasePage):
    MODAL_CONTAINER = (By.CSS_SELECTOR, "div.styles_modal__container__DO24y")
    LOGIN_INPUT = (By.NAME, "login")
    PASSWORD_INPUT = (By.NAME, "password")
    SUBMIT_BUTTON_FORM = (By.XPATH, "//form[@data-name='form_login']//button[normalize-space()='Войти']")
    ERROR_MESSAGE_IN_MODAL = (By.XPATH, ".//*[contains(@class, 'styles_text__error')]")

    def is_modal_displayed(self):
        return self.is_element_visible(self.MODAL_CONTAINER, timeout=5)

    def login(self, username, password):
        if not self.is_modal_displayed():
            print("Модальное окно входа не видимо, невозможно продолжить вход.")
            return False

        self.type_text(self.LOGIN_INPUT, username)
        self.type_text(self.PASSWORD_INPUT, password)
        self.wait_for_element_to_be_clickable(self.SUBMIT_BUTTON_FORM)
        self.click(self.SUBMIT_BUTTON_FORM)
        modal_disappeared = self.is_element_invisible(self.MODAL_CONTAINER, timeout=15)
        return modal_disappeared

    def get_error_message(self):
        if self.is_modal_displayed():
            try:
                modal_element = self.find_element(self.MODAL_CONTAINER)
                return modal_element.find_element(*self.ERROR_MESSAGE_IN_MODAL).text
            except:
                return None
        return None