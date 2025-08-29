import pytest
import time
from pages.home_page import HomePage
from pages.search_results_page import SearchResultsPage
from pages.item_page import ItemPage
from pages.favorites_page import FavoritesPage
from .conftest import (
    BASE_URL, VALID_KUFAR_LOGIN, VALID_KUFAR_PASSWORD,
    SEARCH_ITEM_TC1, ITEM_TO_VIEW_E2E, FILTER_SEARCH_ITEM_CHECKBOX
)

@pytest.mark.login
@pytest.mark.smoke
@pytest.mark.run(order=1)
def test_login_success_attempt(driver):
    print("\nТест: Попытка успешного входа (POM)")
    home_page = HomePage(driver)
    home_page.open_home_page()

    login_modal = home_page.click_login_button_header()
    assert login_modal.is_modal_displayed(), "Модальное окно входа не появилось."
    print(" - Модальное окно входа появилось.")

    login_successful = login_modal.login(VALID_KUFAR_LOGIN, VALID_KUFAR_PASSWORD)

    if not login_successful:
        error = login_modal.get_error_message()
        if error:
            home_page.take_screenshot("login_failed_with_error")
            pytest.fail(f"Попытка входа не удалась, ошибка в форме: {error}")
        else:
            home_page.take_screenshot("login_failed_modal_stuck")
            pytest.fail("Попытка входа не удалась: модальное окно не закрылось, конкретная ошибка не найдена.")

    assert home_page.is_user_logged_in(), "Аватар профиля не найден после предполагаемого входа."
    print(" - Аватар профиля найден. Вход УСПЕШЕН (до этапа SMS, если он есть).")
    home_page.take_screenshot("login_success")

@pytest.mark.search
@pytest.mark.parametrize("search_term, expected_header_part", [
    (SEARCH_ITEM_TC1, SEARCH_ITEM_TC1),
    ("телефон", "телефон"),
    # Параметризация тестов: ожидаемо падающий тест (xfail)
    pytest.param("несуществующийтовар123абв", "несуществующийтовар123абв",
                 marks=pytest.mark.xfail(reason="Ожидается, что поиск несуществующего товара не даст точного совпадения в заголовке или ничего не найдет"))
])
@pytest.mark.run(order=2)
def test_search_item(driver, search_term, expected_header_part):
    print(f"\nТест: Поиск товара '{search_term}' (POM)")
    home_page = HomePage(driver)
    home_page.open_home_page()
    home_page.search_for_item(search_term)

    results_page = SearchResultsPage(driver)
    header_text = results_page.get_results_header_text(search_term)
    print(f" - Страница результатов для '{search_term}' загружена. Заголовок: '{header_text}'")
    assert expected_header_part.lower() in header_text.lower(), \
        f"Заголовок '{header_text}' не содержит ожидаемую часть '{expected_header_part}'"
    results_page.take_screenshot(f"search_results_{search_term.replace(' ','_')}")

@pytest.mark.favorites
@pytest.mark.dependency(depends=["test_login_success_attempt"]) # Зависит от успешного выполнения теста входа
@pytest.mark.run(order=3)
def test_add_to_favorites(driver):
    print("\nТест: Добавление в избранное (POM)")
    home_page = HomePage(driver)

    if not home_page.is_user_logged_in():
        print("   - Пользователь не авторизован. Попытка входа...")
        login_modal = home_page.click_login_button_header()
        assert login_modal.is_modal_displayed(), "Модальное окно входа не появилось."
        print("   - Модальное окно входа появилось.")
        login_successful = login_modal.login(VALID_KUFAR_LOGIN, VALID_KUFAR_PASSWORD)

        if not login_successful:
            error_msg = login_modal.get_error_message()
            home_page.take_screenshot("e2e_login_failed_modal")
            pytest.fail(
                f"Не удалось войти в систему для E2E теста. Ошибка: {error_msg if error_msg else 'Модальное окно не закрылось'}")

        assert home_page.is_user_logged_in(), "Не удалось подтвердить вход (аватар не найден) для E2E теста."
        print("   - Вход выполнен успешно.")
    else:
        print("   - Пользователь уже авторизован.")

    item_id, item_title = home_page.get_first_ad_details_and_add_to_favorites()
    assert item_id != "N/A", "Не удалось получить ID товара для добавления в избранное."
    print(f" - Товар '{item_title}' (ID: {item_id}) добавлен в избранное со страницы Home.")

    home_page.go_to_favorites()
    favorites_page = FavoritesPage(driver)
    assert favorites_page.is_item_in_favorites(item_id, item_title), \
        f"Товар ID {item_id} ('{item_title}') НЕ найден на странице 'Избранное'"
    favorites_page.take_screenshot("favorites_page_with_item")

@pytest.mark.e2e # Метка для сквозных тестов
@pytest.mark.run(order=4)
def test_search_view_write_e2e(driver):
    print(f"\nТест: E2E - Поиск '{ITEM_TO_VIEW_E2E}', просмотр, 'Написать' (POM)")
    home_page = HomePage(driver)
    home_page.open_home_page()

    if not home_page.is_user_logged_in():
        print("   - Пользователь не авторизован. Попытка входа...")
        login_modal = home_page.click_login_button_header()
        assert login_modal.is_modal_displayed(), "Модальное окно входа не появилось."
        print("   - Модальное окно входа появилось.")
        login_successful = login_modal.login(VALID_KUFAR_LOGIN, VALID_KUFAR_PASSWORD)

        if not login_successful:
            error_msg = login_modal.get_error_message()
            home_page.take_screenshot("e2e_login_failed_modal")
            pytest.fail(
                f"Не удалось войти в систему для E2E теста. Ошибка: {error_msg if error_msg else 'Модальное окно не закрылось'}")

        assert home_page.is_user_logged_in(), "Не удалось подтвердить вход (аватар не найден) для E2E теста."
        print("   - Вход выполнен успешно.")
    else:
        print("   - Пользователь уже авторизован.")

    home_page.search_for_item(ITEM_TO_VIEW_E2E)

    results_page = SearchResultsPage(driver)
    selected_item_title_in_results = results_page.select_item_by_name_contains(ITEM_TO_VIEW_E2E.split(" ")[0])
    assert selected_item_title_in_results is not None, f"Не удалось найти и выбрать товар, содержащий '{ITEM_TO_VIEW_E2E}'"
    print(f" - Перешли на страницу товара: '{selected_item_title_in_results}'")

    item_page = ItemPage(driver)
    item_title_on_page = item_page.get_item_title()
    item_price = item_page.get_item_price()
    print(f" - Страница товара загружена. Заголовок: '{item_title_on_page}', Цена: '{item_price}'")
    assert item_title_on_page, "Заголовок на странице товара пустой"

    assert ITEM_TO_VIEW_E2E.lower().split(" ")[0] in item_title_on_page.lower(), \
        f"Заголовок на странице '{item_title_on_page}' не соответствует искомому '{ITEM_TO_VIEW_E2E}'"
    assert item_price, "Цена на странице товара не найдена"

    assert item_page.click_write_button_and_check_popup(), "Окно сообщения не открылось после нажатия 'Написать'."
    print("   - Окно сообщения открылось.")
    item_page.take_screenshot("message_popup_opened")

    assert item_page.close_message_popup(), "Окно сообщения не закрылось."
    print("   - Окно сообщения закрыто.")

@pytest.mark.filters
@pytest.mark.run(order=5)
def test_filter_search_results_checkbox(driver):
    print(f"\nТест: Фильтр 'Только с фото' для '{FILTER_SEARCH_ITEM_CHECKBOX}' (POM)")
    home_page = HomePage(driver)
    home_page.open_home_page()
    home_page.search_for_item(FILTER_SEARCH_ITEM_CHECKBOX)

    results_page = SearchResultsPage(driver)
    initial_count_text = results_page.get_results_count_text()
    print(f"   - Начальное количество результатов (текст): {initial_count_text}")
    assert initial_count_text != "N/A", "Не удалось получить начальное количество результатов."

    results_page.apply_photo_filter_and_show()

    print("   - Ожидаем обновления счетчика после применения фильтра...")
    time.sleep(3)

    new_count_text = results_page.get_results_count_text()
    print(f"   - Новое количество результатов (текст): {new_count_text}")
    assert new_count_text != "N/A", "Не удалось получить количество результатов после фильтра."

    if initial_count_text != "0" and initial_count_text != "N/A" and \
       new_count_text != "0" and new_count_text != "N/A":
         assert new_count_text != initial_count_text, \
             f"Счетчик результатов не изменился (или изменился на то же значение) после фильтра (был '{initial_count_text}', стал '{new_count_text}')"
         print(f"   - Счетчик изменился с '{initial_count_text}' на '{new_count_text}'.")
    else:
        print(f"   - Пропуск ассерта на изменение счетчика, т.к. начальный или новый счетчик был '0' или 'N/A'. (Был '{initial_count_text}', стал '{new_count_text}')")
    results_page.take_screenshot("filter_results_photo")


@pytest.mark.cookies
@pytest.mark.run(order=6)
def test_cookie_operations(driver):
    print("\nТест: Работа с куками (POM)")
    home_page = HomePage(driver)
    home_page.open_home_page()

    print(" - Получение всех кук:")
    all_cookies = driver.get_cookies()
    assert all_cookies, "Не удалось получить куки."
    if all_cookies:
        print(f"   - Всего кук: {len(all_cookies)}. Первые несколько:")
        for cookie in all_cookies[:3]:
            print(f"     - Имя: {cookie.get('name')}, Значение (часть): {str(cookie.get('value'))[:30]}..., Домен: {cookie.get('domain')}")
    home_page.take_screenshot("after_getting_cookies")

    print("\n - Добавление тестовой куки:")
    test_cookie_name = 'my_test_cookie_pom'
    test_cookie_value = 'hello_kufar_pom_test'
    test_cookie_details = {'name': test_cookie_name, 'value': test_cookie_value, 'domain': '.kufar.by', 'path': '/'}
    driver.add_cookie(test_cookie_details)
    print(f"   - Добавлена кука: Имя='{test_cookie_name}', Значение='{test_cookie_value}'")

    print("\n - Проверка наличия добавленной куки:")
    retrieved_cookie = driver.get_cookie(test_cookie_name)
    assert retrieved_cookie is not None, f"Не удалось получить добавленную куку '{test_cookie_name}'."
    assert retrieved_cookie['value'] == test_cookie_value, f"Значение добавленной куки '{test_cookie_name}' не совпадает."
    print(f"   - Кука '{retrieved_cookie['name']}' найдена со значением '{retrieved_cookie['value']}'.")

    print("\n - Удаление тестовой куки:")
    driver.delete_cookie(test_cookie_name)
    assert driver.get_cookie(test_cookie_name) is None, f"Тестовая кука '{test_cookie_name}' не была удалена."
    print(f"   - Тестовая кука '{test_cookie_name}' успешно удалена.")
    home_page.take_screenshot("after_cookie_manipulation")

@pytest.mark.skip(reason="Этот тест демонстрационно пропущен по требованию.")
@pytest.mark.run(order=99)
def test_skipped_example(driver):
    print("Этот тест не должен выполниться, так как он пропущен.")
    assert False, "Этот ассерт не должен сработать, так как тест пропущен."