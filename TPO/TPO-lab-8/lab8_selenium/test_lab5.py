import pytest
import time
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, NoSuchElementException, ElementClickInterceptedException

# --- Константы ---
BASE_URL = "https://www.kufar.by/"
VALID_KUFAR_LOGIN = ""
VALID_KUFAR_PASSWORD = ""
SEARCH_ITEM_TC1 = "велосипед"
ITEM_TO_VIEW_E2E = "Iphone"
FILTER_SEARCH_ITEM_CHECKBOX = "диван"

@pytest.fixture(scope="module")
def driver():
    print("\nЗапуск WebDriver (scope=module)...")
    try:
        service = ChromeService(ChromeDriverManager().install())
        driver_instance = webdriver.Chrome(service=service)
        driver_instance.implicitly_wait(5)
        driver_instance.maximize_window()
        yield driver_instance
    except Exception as e:
        pytest.fail(f"Не удалось инициализировать WebDriver: {e}")

    print("\nЗакрытие WebDriver (scope=module)...")
    driver_instance.quit()

def handle_cookie_consent(driver, timeout=10):
    try:
        cookie_accept_button_locator = (By.XPATH, "//button[normalize-space()='Принять']")
        accept_button = WebDriverWait(driver, timeout).until(
            EC.element_to_be_clickable(cookie_accept_button_locator)
        )
        print("   - Найдена кнопка 'Принять' куки. Нажимаем...")
        accept_button.click()
        time.sleep(0.5)
        print("   - Кнопка 'Принять' куки нажата.")
        return True
    except TimeoutException:
        print("   - Поп-ап куки не найден или кнопка не кликабельна (Timeout).")
        return False
    except Exception as e:
        print(f"   - Ошибка при клике на кнопку куки: {e}")
        return False

# Тест 1: Авторизация
def test_login_success_attempt(driver):
    print("\nТест: Попытка успешного входа")
    try:
        driver.get(BASE_URL)
        handle_cookie_consent(driver)

        login_button_header = WebDriverWait(driver, 10).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, "div[data-testid='login_button'] button"))
        )
        login_button_header.click()
        print(" - Кнопка 'Войти' в шапке нажата.")

        print(" - Ожидание появления формы входа...")
        modal_container_locator = (By.CSS_SELECTOR, "div.styles_modal__container__DO24y")
        modal_container = WebDriverWait(driver, 10).until(
             EC.visibility_of_element_located(modal_container_locator)
        )
        print(" - Форма входа (модальное окно) появилась.")

        # Находим поля и кнопку внутри видимого модального окна
        login_input = WebDriverWait(driver, 10).until(
            EC.visibility_of_element_located((By.NAME, "login"))
        )
        password_input = WebDriverWait(driver, 10).until(
            EC.visibility_of_element_located((By.NAME, "password"))
        )
        submit_button_locator = (By.XPATH, "//form[@data-name='form_login']//button[normalize-space()='Войти']")

        WebDriverWait(driver, 10).until(
            EC.visibility_of_element_located(submit_button_locator)
        )
        print(" - Поля и кнопка 'Войти' найдены.")

        login_input.send_keys(VALID_KUFAR_LOGIN)
        password_input.send_keys(VALID_KUFAR_PASSWORD)
        print(" - Валидные данные введены.")

        print(" - Ожидание активации кнопки 'Войти'...")
        submit_button_form = WebDriverWait(driver, 10).until(
             EC.element_to_be_clickable(submit_button_locator)
        )
        print(" - Кнопка 'Войти' стала активной.")

        submit_button_form.click()
        print(" - Кнопка 'Войти' в форме нажата.")

        print(" - Ожидание результата входа (исчезновения модального окна или появления аватара)...")
        try:
            WebDriverWait(driver, 15).until(
                EC.invisibility_of_element_located(modal_container_locator)
            )
            print(" - Модальное окно входа закрылось.")

            profile_avatar = WebDriverWait(driver, 10).until(
                EC.visibility_of_element_located((By.CSS_SELECTOR, "div[data-testid='user_profile_pic'] img"))
            )
            assert profile_avatar.is_displayed(), "Аватар профиля не найден после попытки входа."
            print(" - Аватар профиля найден. Вход УСПЕШЕН (до этапа SMS).")
            print("Тест успешного входа пройден (до SMS).")

        except TimeoutException:
            print(" - Не удалось подтвердить успешный вход (окно не закрылось или аватар не найден). Возможно, требуется SMS или данные неверны.")
            try:

                if modal_container.is_displayed():
                     error_message = modal_container.find_element(By.XPATH, ".//*[contains(@class, 'styles_text__error')]") # Ищем ошибку внутри модального
                     print(f"   - Обнаружено сообщение об ошибке в форме: {error_message.text}")
                     pytest.fail("Попытка входа не удалась, обнаружено сообщение об ошибке в форме.")
                else:
                    pytest.fail("Попытка входа не удалась: модальное окно закрылось, но аватар не найден.")

            except Exception as inner_e:
                 print(f"   - Ошибка при поиске сообщения об ошибке: {inner_e}")
                 pytest.fail("Попытка входа не удалась: окно не закрылось или аватар не найден (ошибка при доп. проверке).")

    except TimeoutException as e:
        timestamp = time.strftime("%Y%m%d-%H%M%S")
        screenshot_name = f"screenshot_login_fail_{timestamp}.png"
        try:
            driver.save_screenshot(screenshot_name)
            print(f"   !!! Скриншот сохранен: {screenshot_name}")
        except Exception as screen_e:
             print(f"   !!! Не удалось сохранить скриншот: {screen_e}")
        pytest.fail(f"Элемент не найден/не появился вовремя (TimeoutException): {e.msg}")
    except NoSuchElementException as e:
         pytest.fail(f"Элемент не найден во время теста: {e}")
    except Exception as e:
         pytest.fail(f"Неожиданная ошибка в тесте test_login_success_attempt: {e}")


# Тест 2: Поиск товара
def test_search_item_tc1(driver):
    """Тестирует поиск товара по ключевому слову (ЛР5 - SEARCH_001)."""
    print(f"\nТест: Поиск товара '{SEARCH_ITEM_TC1}' (TC1)")
    try:
        driver.get(BASE_URL)
        handle_cookie_consent(driver)

        # Находим поле ввода и кнопку
        search_input_locator = (By.CSS_SELECTOR, "input[data-testid='searchbar-input']")
        search_button_locator = (By.CSS_SELECTOR, "button[data-testid='searchbar-search-button']")

        search_input = WebDriverWait(driver, 2).until(
            EC.visibility_of_element_located(search_input_locator)
        )
        WebDriverWait(driver, 2).until(
            EC.visibility_of_element_located(search_button_locator)
        )
        print(" - Поле поиска и кнопка найдены.")

        # Вводим текст
        search_input.send_keys(SEARCH_ITEM_TC1)
        print(f" - Введено '{SEARCH_ITEM_TC1}' в поиск.")

        # Ждем активации кнопки
        print(" - Ожидание активации кнопки поиска...")
        search_button = WebDriverWait(driver, 2).until(
            EC.element_to_be_clickable(search_button_locator)
        )
        print(" - Кнопка поиска стала активной.")

        # Кликаем кнопку
        search_button.click()
        print(f" - Кнопка поиска нажата.")

        results_header_locator = (By.XPATH, f"//h3[contains(text(), '{SEARCH_ITEM_TC1}')]")
        print(f" - Ожидание заголовка результатов: {results_header_locator[1]}")
        results_header = WebDriverWait(driver, 15).until(
            EC.visibility_of_element_located(results_header_locator)
        )
        print(f" - Страница результатов для '{SEARCH_ITEM_TC1}' загружена. Заголовок найден: '{results_header.text}'")

        print("Тест поиска товара (TC1) пройден.")

    except TimeoutException as e:
         timestamp = time.strftime("%Y%m%d-%H%M%S")
         screenshot_name = f"screenshot_search_fail_{timestamp}.png"
         try:
             driver.save_screenshot(screenshot_name)
             print(f"   !!! Скриншот сохранен: {screenshot_name}")
         except Exception as screen_e:
             print(f"   !!! Не удалось сохранить скриншот: {screen_e}")
         pytest.fail(f"Элемент не найден/не появился вовремя при поиске (TimeoutException): {e.msg}")
    except NoSuchElementException as e:
         pytest.fail(f"Элемент не найден во время теста поиска: {e}")
    except Exception as e:
         pytest.fail(f"Неожиданная ошибка в тесте test_search_item_tc1: {e}")

# Тест 3: Добавление товара в избранное
def test_add_to_favorites_tc2(driver):
    print("\nТест: Добавление в избранное (TC2)")

    item_id_to_add = None
    added_item_title = "N/A"

    try:
        print(" - Шаг 1: Переход на главную и поиск первого товара...")
        driver.get(BASE_URL)
        handle_cookie_consent(driver)

        first_ad_link_locator = (By.CSS_SELECTOR, "section > a[data-testid*='ad']")
        first_ad_link = WebDriverWait(driver, 15).until(
            EC.element_to_be_clickable(first_ad_link_locator)
        )

        item_href = first_ad_link.get_attribute('href')
        try:
            item_id_to_add = item_href.split('/item/')[1].split('?')[0]
            added_item_title = first_ad_link.find_element(By.CSS_SELECTOR, "h3").text
            print(f"   - Найден товар: '{added_item_title}' (ID: {item_id_to_add}, href: {item_href})")
        except (IndexError, NoSuchElementException) as e:
            print(f"   - Не удалось извлечь ID или заголовок из href: {item_href}. Ошибка: {e}")
            pytest.fail("Не удалось получить ID/заголовок первого товара.")

        print(" - Шаг 2: Добавление товара в избранное...")
        favorite_button_locator = (By.XPATH, ".//div[@data-testid='favorite']")
        driver.execute_script("arguments[0].scrollIntoViewIfNeeded(true);", first_ad_link)
        time.sleep(0.5)
        favorite_button = WebDriverWait(first_ad_link, 10).until(
            EC.element_to_be_clickable(favorite_button_locator)
        )
        driver.execute_script("arguments[0].click();", favorite_button)
        print("   - Кнопка 'Избранное' нажата (через JS).")

        filled_heart_locator = (By.XPATH, ".//div[@data-testid='favorite']//*[local-name()='svg']//*[local-name()='path' and contains(@fill, 'FF3B4C')]")
        WebDriverWait(first_ad_link, 10).until(
            EC.visibility_of_element_located(filled_heart_locator)
        )
        print("   - Кнопка 'Избранное' изменила состояние (сердце закрашено).")

        print(" - Шаг 3: Переход на страницу 'Избранное'...")
        profile_avatar_locator = (By.CSS_SELECTOR, "div[data-testid='user_profile_pic'] img")
        profile_avatar = WebDriverWait(driver, 10).until(EC.visibility_of_element_located(profile_avatar_locator))
        profile_avatar.click()
        print("   - Клик на аватар.")

        favorites_link_locator = (By.CSS_SELECTOR, "a[data-testid='profile_menu_saved_link']")
        favorites_link = WebDriverWait(driver, 10).until(
            EC.element_to_be_clickable(favorites_link_locator)
        )
        favorites_link.click()
        print("   - Перешли на страницу 'Избранное'.")

        print(f" - Шаг 4: Проверка наличия товара ID {item_id_to_add} в избранном...")

        favorite_items_locator = (By.CSS_SELECTOR, "a.styles_wrapper__xi81o")
        print(f"   - Ожидание первой карточки в избранном (локатор: {favorite_items_locator[1]})...")
        try:
            WebDriverWait(driver, 15).until(
                EC.visibility_of_element_located(favorite_items_locator)
            )
            print("   - Первая карточка в избранном видима.")
            favorite_items = driver.find_elements(*favorite_items_locator)
            print(f"   - Найдено {len(favorite_items)} элементов в избранном.")
        except TimeoutException:
            favorite_items = []
            print("   - Не найдено ни одной карточки в избранном (Timeout).")

        item_found_in_favorites = False
        if not favorite_items:
            print("   - Список избранного пуст.")
        else:
            for item in favorite_items:
                item_link = item.get_attribute('href')
                # Проверяем, содержит ли ссылка ID товара
                if item_id_to_add and item_id_to_add in item_link:
                    item_found_in_favorites = True
                    try:
                        found_title = item.find_element(By.CSS_SELECTOR, "h3.styles_title__jqhlH").text
                        print(f"   - Товар '{found_title}' (ID: {item_id_to_add}) найден в избранном!")
                    except NoSuchElementException:
                        print(
                            f"   - Товар с ID {item_id_to_add} найден в избранном, но не удалось найти его заголовок (h3).")
                    break
                else:
                    print(f"     - Проверен элемент с href: {item_link} (не соответствует ID {item_id_to_add})")

        assert item_found_in_favorites, f"Товар ID {item_id_to_add} ('{added_item_title}') НЕ найден на странице 'Избранное'"
        print("Тест добавления в избранное (TC2) пройден.")

    except TimeoutException as e:
        pytest.fail(f"Элемент не найден/не появился вовремя (TimeoutException): {e.msg}")
    except NoSuchElementException as e:
         pytest.fail(f"Элемент не найден во время теста: {e}")
    except Exception as e:
         pytest.fail(f"Неожиданная ошибка в тесте test_add_to_favorites_tc2: {e}")


# Тест 4: Сквозной сценарий - Поиск, просмотр, попытка написать сообщение
def test_search_view_write_e2e(driver):

    print(f"\nТест: Поиск, просмотр и 'Написать' для '{ITEM_TO_VIEW_E2E}' (E2E)")
    item_title_in_results = "N/A"
    target_ad_link_element = None
    target_ad_href = None

    try:
        driver.get(BASE_URL)
        handle_cookie_consent(driver)

        # --- Шаг 1: Поиск товара ---
        search_input_locator = (By.CSS_SELECTOR, "input[data-testid='searchbar-input']")
        search_button_locator = (By.CSS_SELECTOR, "button[data-testid='searchbar-search-button']")
        search_input = WebDriverWait(driver, 10).until(EC.visibility_of_element_located(search_input_locator))
        WebDriverWait(driver, 10).until(EC.visibility_of_element_located(search_button_locator))
        search_input.send_keys(ITEM_TO_VIEW_E2E)
        search_button = WebDriverWait(driver, 10).until(EC.element_to_be_clickable(search_button_locator))
        search_button.click()
        print(f" - Выполнен поиск '{ITEM_TO_VIEW_E2E}'.")
        results_header_locator = (By.XPATH, f"//h1[contains(text(), '{ITEM_TO_VIEW_E2E}')]")
        WebDriverWait(driver, 15).until(EC.visibility_of_element_located(results_header_locator))
        print(f" - Заголовок страницы результатов '{ITEM_TO_VIEW_E2E}' загружен.")

        # --- Шаг 2: Ожидание и ВЫБОР ПРАВИЛЬНОГО товара ---
        print(f" - Шаг 2: Поиск товара, содержащего '{ITEM_TO_VIEW_E2E}' в заголовке...")
        all_ad_links_locator = (By.CSS_SELECTOR, "section > a[data-testid*='ad']")
        WebDriverWait(driver, 15).until(EC.presence_of_element_located(all_ad_links_locator))
        all_ad_links = driver.find_elements(*all_ad_links_locator)
        print(f"   - Найдено {len(all_ad_links)} карточек в результатах.")

        for i, ad_link in enumerate(all_ad_links):
             item_title_in_results = "N/A"
             print(f"     --- Проверка карточки #{i} ---")
             current_href = ad_link.get_attribute('href')
             print(f"     - Href: {current_href}")
             try:
                 title_element = WebDriverWait(ad_link, 4).until(
                     EC.visibility_of_element_located((By.XPATH, ".//h3"))
                 )
                 item_title_in_results = title_element.text
                 print(f"     - Найден заголовок: '{item_title_in_results}'")
                 is_match = ITEM_TO_VIEW_E2E.lower() in item_title_in_results.lower()
                 print(f"     - Содержит '{ITEM_TO_VIEW_E2E}'? -> {is_match}")
                 if is_match:
                     target_ad_href = current_href
                     print(f"   - Найден ПОДХОДЯЩИЙ товар: '{item_title_in_results}'. URL: {target_ad_href}")
                     break
             except (NoSuchElementException, TimeoutException):
                 print(f"     - Пропуск карточки #{i} (не удалось найти заголовок h3 за 4 сек).")
                 continue

        if target_ad_href is None:
            pytest.fail(f"Не найден ни один товар с '{ITEM_TO_VIEW_E2E}' в заголовке на первой странице результатов.")

        print(f"   - Переход на страницу товара по URL: {target_ad_href}")
        driver.get(target_ad_href)

        # --- Шаг 3: Проверка страницы товара ---
        print(" - Шаг 3: Проверка страницы товара...")
        print("   - Ожидание заголовка H1 на странице товара...")
        item_page_header = WebDriverWait(driver, 15).until(
            EC.visibility_of_element_located((By.TAG_NAME, "h1"))
        )
        item_title_on_page = item_page_header.text
        print(f"   - Страница товара загружена. Заголовок: '{item_title_on_page}'.")

        assert item_title_on_page, "Заголовок на странице товара пустой"
        if item_title_in_results != "N/A":
             assert item_title_in_results.lower() in item_title_on_page.lower() or \
                    item_title_on_page.lower() in item_title_in_results.lower(), \
                    f"Заголовок на странице '{item_title_on_page}' не соответствует заголовку в результатах '{item_title_in_results}'"
        price_element_locator = (By.CSS_SELECTOR, "span.styles_main__eFbJH")
        print(f"   - Ожидание элемента цены (локатор: {price_element_locator[1]})...")
        price_element = WebDriverWait(driver, 15).until(
            EC.visibility_of_element_located(price_element_locator)
        )
        assert price_element.is_displayed(), "Цена на странице товара не найдена"
        print(f"   - Цена найдена: {price_element.text}")
        # --- Конец Шага 3 ---

        # --- Шаг 4: Нажатие кнопки "Написать" ---
        print(" - Шаг 4: Нажатие кнопки 'Написать'...")
        write_button_locator = (By.CSS_SELECTOR, "button[data-cy='listings-sidebar-btn-write']")
        try:
            print("   - Ожидание кликабельности кнопки 'Написать'...")
            write_button = WebDriverWait(driver, 15).until(
                EC.element_to_be_clickable(write_button_locator)
            )
            print("   - Скролл к кнопке 'Написать'...")
            driver.execute_script("arguments[0].scrollIntoViewIfNeeded(true);", write_button)
            time.sleep(0.5)
            print("   - Клик по кнопке 'Написать' (через JS)...")
            driver.execute_script("arguments[0].click();", write_button)
            print("   - Кнопка 'Написать' нажата.")
        except TimeoutException:
             pytest.fail("Кнопка 'Написать' не найдена или не стала кликабельной за 15 сек.")


        # --- Шаг 5: Ожидание и закрытие окна сообщения ---
        print(" - Шаг 5: Ожидание и закрытие окна сообщения...")
        close_button_locator = (By.XPATH, "//button[normalize-space()='Закрыть']")
        close_button = WebDriverWait(driver, 15).until(
            EC.element_to_be_clickable(close_button_locator)
        )
        print("   - Окно сообщения открылось (кнопка 'Закрыть' найдена).")
        close_button.click()
        print("   - Кнопка 'Закрыть' нажата.")
        WebDriverWait(driver, 10).until(
            EC.invisibility_of_element_located(close_button_locator)
        )
        print("   - Окно сообщения закрыто.")

        print(f"Тест поиска, просмотра и 'Написать' (E2E) пройден.")

    except TimeoutException as e:
        pytest.fail(f"Элемент не найден/не появился вовремя в E2E тесте (TimeoutException): {e.msg}")
    except NoSuchElementException as e:
         pytest.fail(f"Элемент не найден во время E2E теста: {e}")
    except Exception as e:
         pytest.fail(f"Неожиданная ошибка в тесте test_search_view_write_e2e: {e}")




# Тест 5: Использование фильтра (чекбокса) в поиске
def test_filter_search_results_checkbox(driver):
    print(f"\nТест: Применение фильтра 'Только с фото' к поиску '{FILTER_SEARCH_ITEM_CHECKBOX}'")
    initial_result_count_text = None
    try:
        driver.get(BASE_URL)
        handle_cookie_consent(driver)

        # --- Шаг 1: Поиск товара ---
        search_input_locator = (By.CSS_SELECTOR, "input[data-testid='searchbar-input']")
        search_button_locator = (By.CSS_SELECTOR, "button[data-testid='searchbar-search-button']")
        search_input = WebDriverWait(driver, 10).until(EC.visibility_of_element_located(search_input_locator))
        WebDriverWait(driver, 10).until(EC.visibility_of_element_located(search_button_locator))
        search_input.send_keys(FILTER_SEARCH_ITEM_CHECKBOX)
        search_button = WebDriverWait(driver, 10).until(EC.element_to_be_clickable(search_button_locator))
        search_button.click()
        print(f" - Выполнен поиск '{FILTER_SEARCH_ITEM_CHECKBOX}'.")

        # --- Шаг 2: Ожидание результатов и получение начального количества ---
        print(" - Шаг 2: Ожидание результатов поиска и получение счетчика...")
        results_cards_container_locator = (By.CSS_SELECTOR, "div.styles_cards__bBppJ")
        WebDriverWait(driver, 20).until(EC.visibility_of_element_located(results_cards_container_locator))
        print("   - Страница результатов поиска загружена.")
        count_element_locator = (By.CSS_SELECTOR, "button[data-cy='listing-tab-all-goods'] span.styles_counter__NSFh7")
        try:
            count_element = WebDriverWait(driver, 10).until(
                EC.visibility_of_element_located(count_element_locator)
            )
            initial_result_count_text = count_element.text # Сохраняем текст
            print(f"   - Начальное количество результатов (текст): {initial_result_count_text}")
        except TimeoutException:
            print("   - Не удалось найти элемент счетчика объявлений.")

        # --- Шаг 3: Применение фильтра "Только с фото" ---
        print(" - Шаг 3: Применение фильтра 'Только с фото'...")
        photo_filter_label_locator = (By.XPATH, "//label[@for='oph']")
        try:
            photo_filter_label = WebDriverWait(driver, 15).until(EC.element_to_be_clickable(photo_filter_label_locator))
            driver.execute_script("arguments[0].scrollIntoViewIfNeeded(true);", photo_filter_label)
            time.sleep(0.5)
            driver.execute_script("arguments[0].click();", photo_filter_label)
            print("   - Фильтр 'Только с фото' выбран.")
        except TimeoutException:
             pytest.fail("Фильтр 'Только с фото' не найден или не стал кликабельным.")

        # --- Шаг 3.5: Нажатие кнопки "Показать объявления"
        print(" - Шаг 3.5: Нажатие кнопки 'Показать объявления'...")
        show_results_button_locator = (By.CSS_SELECTOR, "button[data-name='filter-submit-button']")
        try:
            show_results_button = WebDriverWait(driver, 10).until(EC.element_to_be_clickable(show_results_button_locator))
            show_results_button.click()
            print("   - Кнопка 'Показать объявления' нажата.")
        except ElementClickInterceptedException:
             print("   - Обычный клик по 'Показать объявления' перехвачен, пробуем JS-клик...")
             try:
                 show_results_button_js = WebDriverWait(driver, 5).until(EC.presence_of_element_located(show_results_button_locator))
                 driver.execute_script("arguments[0].click();", show_results_button_js)
                 print("   - Кнопка 'Показать объявления' нажата (через JS).")
             except Exception as e_js:
                 pytest.fail(f"Не удалось нажать кнопку 'Показать объявления' даже через JS: {e_js}")
        except TimeoutException:
             pytest.fail("Кнопка 'Показать объявления' не найдена или не стала кликабельной.")

        # --- Шаг 4: Ожидание ИЗМЕНЕНИЯ счетчика ---
        print(f" - Шаг 4: Ожидание изменения счетчика (был: '{initial_result_count_text}')...")
        try:
            WebDriverWait(driver, 15).until(
                EC.any_of(
                    EC.staleness_of(count_element),
                    EC.none_of(
                         EC.text_to_be_present_in_element(count_element_locator, initial_result_count_text)
                         )
                    )
            )
            print("   - Счетчик результатов изменился или элемент обновился.")
            new_count_element = WebDriverWait(driver, 5).until(
                EC.visibility_of_element_located(count_element_locator)
            )
            new_result_count_text = new_count_element.text
            print(f"   - Новое количество результатов (текст): {new_result_count_text}")

            if initial_result_count_text is not None:
                assert new_result_count_text != initial_result_count_text, \
                    f"Счетчик результатов не изменился после применения фильтра (был '{initial_result_count_text}', стал '{new_result_count_text}')"
                print("   - АССЕРТ пройден: Текст счетчика изменился.")
            else:
                 print("   - Пропуск ассерта на изменение счетчика (начальное значение не было получено).")

        except TimeoutException:
            print("   - Счетчик результатов не изменился или элемент не обновился (Timeout).")
            try:
                 current_text = driver.find_element(*count_element_locator).text
                 print(f"     - Текущий текст счетчика: {current_text}")
                 if initial_result_count_text is not None and current_text == initial_result_count_text:
                     pytest.fail("Счетчик результатов не изменился после применения фильтра.")
                 else:
                      print("     - Странная ситуация: Timeout при ожидании изменения, но текст либо другой, либо не был получен.")
            except:
                 pytest.fail("Счетчик результатов не найден после ожидания изменения.")


        print("Тест применения фильтра (чекбокса) пройден.")


    except TimeoutException as e:
        pytest.fail(f"Элемент не найден/не появился вовремя в тесте фильтра (TimeoutException): {e.msg}")
    except NoSuchElementException as e:
         pytest.fail(f"Элемент не найден во время теста фильтра: {e}")
    except Exception as e:
         pytest.fail(f"Неожиданная ошибка в тесте test_filter_search_results_checkbox: {e}")
