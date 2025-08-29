import pytest
import time
import os
from selenium import webdriver
from selenium.webdriver.chrome.service import Service as ChromeService
from webdriver_manager.chrome import ChromeDriverManager
from webdriver_manager.firefox import GeckoDriverManager
from selenium.webdriver.firefox.service import Service as FirefoxService
import pytest_html

# --- Константы ---
BASE_URL = "https://www.kufar.by/"
VALID_KUFAR_LOGIN = "+375293572548"
VALID_KUFAR_PASSWORD = "375293572548polina"
SEARCH_ITEM_TC1 = "велосипед"
ITEM_TO_VIEW_E2E = "Iphone"
FILTER_SEARCH_ITEM_CHECKBOX = "диван"

if not os.path.exists("screenshots"):
    os.makedirs("screenshots")
if not os.path.exists("reports"):
    os.makedirs("reports")

def pytest_addoption(parser):
    parser.addoption("--browser", action="store", default="chrome", help="Браузер для запуска тестов (chrome или firefox)")
    parser.addoption("--headless", action="store_true", help="Запуск браузера в фоновом режиме (headless)")

@pytest.fixture(scope="session")
def browser_choice(request):
    return request.config.getoption("--browser").lower()

@pytest.fixture(scope="session")
def headless_mode(request):
    return request.config.getoption("--headless")

@pytest.fixture(scope="module")
def driver(browser_choice, headless_mode):
    print(f"\nЗапуск WebDriver ({browser_choice}, headless={headless_mode}, scope=module)...")
    driver_instance = None
    try:
        if browser_choice == "chrome":
            options = webdriver.ChromeOptions()
            options.add_argument("--start-maximized") # Запуск в развернутом окне
            options.add_argument("--disable-gpu") # Часто рекомендуется
            options.add_argument("--no-sandbox") # Для CI окружений
            options.add_argument("--disable-dev-shm-usage") # Для CI окружений
            options.add_argument("--incognito") # Режим инкогнито
            options.add_argument("--lang=ru") # Установка языка браузера
            if headless_mode:
                options.add_argument("--headless")
                options.add_argument("--window-size=1920,1080")
            service = ChromeService(ChromeDriverManager().install())
            driver_instance = webdriver.Chrome(service=service, options=options)
        elif browser_choice == "firefox":
            options = webdriver.FirefoxOptions()
            if headless_mode:
                options.add_argument("--headless")
                options.add_argument("-width=1920")
                options.add_argument("-height=1080")
            service = FirefoxService(GeckoDriverManager().install())
            driver_instance = webdriver.Firefox(service=service, options=options)
            if not headless_mode:
                 driver_instance.maximize_window()
        else:
            pytest.fail(f"Неподдерживаемый браузер: {browser_choice}")

        driver_instance.implicitly_wait(5)
        yield driver_instance
    except Exception as e:
        pytest.fail(f"Не удалось инициализировать WebDriver ({browser_choice}): {e}")
    finally:
        if driver_instance:
            print(f"\nЗакрытие WebDriver ({browser_choice}, scope=module)...")
            driver_instance.quit()

# Хук для создания скриншота при падении теста
@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    outcome = yield
    report = outcome.get_result()
    if report.when == 'call' and report.failed:
        try:
            driver_fixture = item.funcargs['driver']
            timestamp = time.strftime("%Y%m%d-%H%M%S")

            test_name = report.nodeid.replace("::", "_").replace("/", "_").replace("[", "_").replace("]", "")
            screenshot_dir = "screenshots"

            if not os.path.exists(screenshot_dir):
                os.makedirs(screenshot_dir)
            file_name = f"{screenshot_dir}/{test_name}_failure_{timestamp}.png"
            driver_fixture.save_screenshot(file_name)
            print(f"Скриншот сохранен в {file_name} при ошибке.")

            extra = getattr(report, 'extra', [])
            if os.path.exists(file_name):
                relative_path_to_screenshot = os.path.join('..', file_name)
                html_img = f'<div><img src="{relative_path_to_screenshot}" alt="screenshot" style="width:300px;" onclick="window.open(this.src)" /></div>'
                extra.append(pytest_html.extras.html(html_img))
            report.extra = extra
        except Exception as e:
            print(f"Не удалось сделать скриншот или добавить его в отчет: {e}")