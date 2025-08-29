from selenium.webdriver.common.by import By
from .base_page import BasePage
from .login_modal import LoginModalPage

class HomePage(BasePage):
    LOGIN_BUTTON_HEADER = (By.CSS_SELECTOR, "div[data-testid='login_button'] button")
    SEARCH_INPUT = (By.CSS_SELECTOR, "input[data-testid='searchbar-input']")
    SEARCH_BUTTON = (By.CSS_SELECTOR, "button[data-testid='searchbar-search-button']")
    PROFILE_AVATAR = (By.CSS_SELECTOR, "div[data-testid='user_profile_pic'] img")
    FAVORITES_LINK_IN_PROFILE_MENU = (By.CSS_SELECTOR, "a[data-testid='profile_menu_saved_link']")
    FIRST_AD_LINK = (By.CSS_SELECTOR, "section > a[data-testid*='ad']:first-of-type")
    FIRST_AD_FAVORITE_BUTTON = (By.XPATH, "(//section/a[contains(@data-testid, 'ad')]//div[@data-testid='favorite'])[1]")
    FIRST_AD_FILLED_HEART_ICON = (By.XPATH, "(//section/a[contains(@data-testid, 'ad')]//div[@data-testid='favorite']//*[local-name()='svg']//*[local-name()='path' and contains(@fill, 'FF3B4C')])[1]")


    def __init__(self, driver):
        super().__init__(driver)
        self.login_modal = LoginModalPage(self.driver) # Создаем экземпляр LoginModalPage

    def open_home_page(self):
        self.open()

    def click_login_button_header(self):
        self.click(self.LOGIN_BUTTON_HEADER)
        return self.login_modal

    def is_user_logged_in(self):
        return self.is_element_visible(self.PROFILE_AVATAR, timeout=5)

    def search_for_item(self, item_name):
        self.type_text(self.SEARCH_INPUT, item_name)
        self.click(self.SEARCH_BUTTON)

    def get_first_ad_details_and_add_to_favorites(self):
        first_ad_element = self.find_element(self.FIRST_AD_LINK)
        item_href = first_ad_element.get_attribute('href')
        item_id = "N/A"
        try:
            item_id = item_href.split('/item/')[1].split('?')[0]
        except IndexError:
            print(f"Предупреждение: Не удалось извлечь item_id из href: {item_href}")

        item_title = "N/A"
        try:
            item_title = first_ad_element.find_element(By.CSS_SELECTOR, "h3").text
        except:
            print(f"Предупреждение: Не удалось найти заголовок для первого объявления.")

        favorite_button_on_card = first_ad_element.find_element(*self.FIRST_AD_FAVORITE_BUTTON)
        self.scroll_to_element(favorite_button_on_card)
        self.driver.execute_script("arguments[0].click();", favorite_button_on_card)

        self.find_element(self.FIRST_AD_FILLED_HEART_ICON)
        print(f"   - Товар '{item_title}' (ID: {item_id}) добавлен в избранное (иконка стала красной).")
        return item_id, item_title

    def go_to_favorites(self):
        self.click(self.PROFILE_AVATAR)
        self.click(self.FAVORITES_LINK_IN_PROFILE_MENU)