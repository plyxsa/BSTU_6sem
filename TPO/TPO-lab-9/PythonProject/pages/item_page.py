from selenium.webdriver.common.by import By
from .base_page import BasePage

class ItemPage(BasePage):
    ITEM_PAGE_HEADER = (By.TAG_NAME, "h1")
    PRICE_ELEMENT = (By.CSS_SELECTOR, "span.styles_main__eFbJH")
    WRITE_BUTTON = (By.CSS_SELECTOR, "button[data-cy='listings-sidebar-btn-write']")
    MESSAGE_POPUP_CLOSE_BUTTON = (By.XPATH, "//button[normalize-space()='Закрыть']")
    MESSAGE_POPUP_CONTAINER = (By.CSS_SELECTOR, "div.styles_chat_popup_modal__header__Zg7_R")

    def get_item_title(self):
        return self.get_text(self.ITEM_PAGE_HEADER)

    def get_item_price(self):
        return self.get_text(self.PRICE_ELEMENT)

    def click_write_button_and_check_popup(self):
        write_btn = self.find_element(self.WRITE_BUTTON)
        self.scroll_to_element(write_btn)
        self.click(self.WRITE_BUTTON)
        return self.is_element_visible(self.MESSAGE_POPUP_CLOSE_BUTTON)

    def close_message_popup(self):
        self.click(self.MESSAGE_POPUP_CLOSE_BUTTON)
        return self.is_element_invisible(self.MESSAGE_POPUP_CLOSE_BUTTON)