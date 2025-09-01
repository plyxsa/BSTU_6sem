# -*- coding: utf-8 -*-
{
    'name': "Тестовый Модуль IBAN", # Дайте описательное имя

    'summary': """
        Добавляет поле IBAN сотрудникам и функцию экспорта зарплат.""",

    'description': """
        - Добавляет поле IBAN в форму Сотрудника.
        - Предоставляет контроллер для скачивания простого списка зарплат для Беларусбанка.
    """,

    'author': "Ваше Имя", # Измените на ваше имя/компанию
    'website': "http://www.yourcompany.com", # Необязательно

    # Категории используются для фильтрации модулей в списке
    # См. https://github.com/odoo/odoo/blob/13.0/odoo/addons/base/data/ir_module_category_data.xml
    # для полного списка
    'category': 'Human Resources', # Или Uncategorized
    'version': '13.0.1.0.1', # Версия Odoo.Major.Minor.Patch.Build

    # Модули, необходимые для работы этого модуля
    'depends': ['base', 'hr'], # Добавьте 'hr' сюда

    # Всегда загружаемые данные
    'data': [
        # 'security/ir.model.access.csv', # Оставьте закомментированным, если не требуется
        'views/views.xml',
        'views/templates.xml', # Оставьте, даже если пустой
    ],
    # Данные, загружаемые только в демонстрационном режиме
    'demo': [
        'demo/demo.xml', # Оставьте, даже если пустой
    ],
    'installable': True,
    'application': False,
    'auto_install': False,
}