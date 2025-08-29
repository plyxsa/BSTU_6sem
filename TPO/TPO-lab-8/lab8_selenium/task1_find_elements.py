import time
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.common.by import By
from selenium.common.exceptions import NoSuchElementException, TimeoutException
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

BASE_URL = "https://www.kufar.by/"

# Безопасный поиск одного элемента с явным ожиданием.
def find_element_safe(driver, by, value, timeout=5):
    try:
        element = WebDriverWait(driver, timeout).until(
            EC.visibility_of_element_located((by, value))
        )
        return element
    except TimeoutException:
        print(f"   > Элемент НЕ НАЙДЕН (Timeout): {by} = {value}")
        return None
    except NoSuchElementException:
        print(f"   > Элемент НЕ НАЙДЕН (NoSuchElement): {by} = {value}")
        return None

# Безопасный поиск нескольких элементов с явным ожиданием хотя бы одного.
def find_elements_safe(driver, by, value, timeout=5):
    try:
        # Ждем присутствия *хотя бы одного* элемента, подходящего под локатор
        WebDriverWait(driver, timeout).until(
            EC.presence_of_all_elements_located((by, value))
        )
        # Затем получаем все элементы
        elements = driver.find_elements(by, value)
        return elements
    except TimeoutException:
        print(f"   > Элементы НЕ НАЙДЕНЫ (Timeout): {by} = {value}")
        return []
    except NoSuchElementException:
        print(f"   > Элементы НЕ НАЙДЕНЫ (NoSuchElement): {by} = {value}")
        return []

try:
    print("Запуск WebDriver...")
    service = ChromeService(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service)
    driver.implicitly_wait(5)
    driver.maximize_window()
    print("WebDriver успешно запущен.")
except Exception as e:
    print(f"Ошибка при запуске WebDriver: {e}")
    exit()

try:
    print(f"Открываем сайт: {BASE_URL}")
    driver.get(BASE_URL)

    print("\n--- Поиск элементов на Kufar.by ---")

    # 1. By.id / By.name
    print("\n1. Поиск по ID:")
    header_wrapper = find_element_safe(driver, By.ID, "header-wrapper")
    if header_wrapper:
        print(f"1.1. По ID ('header-wrapper'): Найден элемент <div> (главный контейнер хедера).")
        print(f"   > Атрибут 'class': {header_wrapper.get_attribute('class')}")
    else:
        print(f"1.1. Элемент с ID 'header-wrapper' не найден.")

    # 2. По CSS-селекторам
    print("\n2. Поиск по CSS-селекторам:")

    # 2.1 Найти кнопку "Войти"
    # Используем селектор для поиска кнопки внутри нужного div
    login_button_element_css = find_element_safe(driver, By.CSS_SELECTOR, "div[data-testid='login_button'] button")
    if login_button_element_css:
        button_text = login_button_element_css.text
        print(f"2.1. CSS ('div[data-testid='login_button'] button'): Найдена кнопка '{button_text}'.")
    else:
        print("2.1. Кнопка 'Войти' (через data-testid div) не найдена.")

    # 2.2 Найти ссылку на категорию "Электроника"
    electronics_category_css = find_element_safe(driver, By.CSS_SELECTOR,
                                                 "a[href='/l/elektronika?elementType=popular_categories']")
    if electronics_category_css:
        # Текст "Электроника" находится внутри span, но .text на <a> должен его извлечь
        category_text = electronics_category_css.text
        print(
            f"2.2. CSS ('a[href='/l/elektronika?elementType=popular_categories']'): Найдена ссылка на категорию '{category_text}'.")
        print(f"   > HREF: {electronics_category_css.get_attribute('href')}")
    else:
        print("2.2. Ссылка на категорию 'Электроника' (по href) не найдена.")

    # 3. По XPath
    print("\n3. Поиск по XPath:")
    try:
        # Идем от главного хедера -> правая часть -> контейнер кнопки -> сама кнопка
        submit_ad_button_xpath = find_element_safe(driver, By.XPATH,
                                                   "//div[contains(@class, 'styles_main_header__gXBLH')]//div[contains(@class, 'styles_right__7IfWj')]//div[@data-name='add-item-button']/button")
        if submit_ad_button_xpath:
            button_text = submit_ad_button_xpath.text
            print(f"3.1. XPath (структурный): Найдена кнопка '{button_text}'.")
        else:
            print("3.1. Кнопка 'Подать объявление' (через XPath) не найдена.")

        # Найти заголовок секции "Все объявления в Беларуси" по тексту
        recommended_header_xpath = find_element_safe(driver, By.XPATH,
                                                     "//h1[contains(text(), 'Все объявления в Беларуси')]")
        if recommended_header_xpath:
            print(f"3.2. XPath (по тексту): Найден заголовок секции 'Все объявления в Беларуси'.")
        else:
            print(f"3.2. Заголовок секции 'Все объявления в Беларуси' не найден.")

    except NoSuchElementException as e:
        print(f"3. Ошибка при поиске по XPath: {e}")
    except Exception as e:
        print(f"3. Неожиданная ошибка при поиске по XPath: {e}")

    # 4. По частичному тексту ссылки
    print("\n4. Поиск по частичному тексту ссылки:")
    help_link = find_element_safe(driver, By.PARTIAL_LINK_TEXT, "Помощь")
    if help_link:
        print(f"4.1. Partial Link Text ('Помощь'): Найдена ссылка '{help_link.text}'.")

        # 5. Найти несколько элементов (список/массив)
        print("\n5. Поиск нескольких элементов:")

        # 5.1 Найти все ссылки на категории по их data-testid
        category_links = find_elements_safe(driver, By.CSS_SELECTOR, "a[data-testid='kufar-main-page-categories-item']")
        if category_links:
            print(f"5.1. CSS (несколько): Найдено {len(category_links)} ссылок на категории (по data-testid):")
            for i, link_element in enumerate(category_links[:8]):
                try:
                    category_name = link_element.text
                    category_href = link_element.get_attribute('href')
                    print(
                        f"   - Категория {i}: '{category_name}' (href: ...{category_href[-30:]})")
                except Exception as e:
                    print(f"   - Категория {i}: (Ошибка извлечения данных: {e})")
        else:
            print("5.1. Ссылки на категории (по data-testid) не найдены.")


finally:
    print("\n--- Поиск завершен ---")
    time.sleep(3)
    if 'driver' in locals() and driver:
        driver.quit()
        print("WebDriver закрыт.")