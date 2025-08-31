
// ChildView.cpp: реализация класса CChildView
//

#include "pch.h"
#include "framework.h"
#include "MyPlot2D.h"
#include "ChildView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
#define pi 3.14159265358979323846;

// CChildView

CChildView::CChildView()
{
}

CChildView::~CChildView()
{
}


BEGIN_MESSAGE_MAP(CChildView, CWnd)
	ON_WM_PAINT()
	// сообщения меню выбора
	ON_COMMAND(ID_TESTS_TEST, &CChildView::OnTestsF1)
	ON_COMMAND(ID_TESTS_F2, &CChildView::OnTestsF2)
	ON_COMMAND(ID_TESTS_F12, &CChildView::OnTestsF3)

END_MESSAGE_MAP()



// Обработчики сообщений CChildView

BOOL CChildView::PreCreateWindow(CREATESTRUCT& cs)
{
	if (!CWnd::PreCreateWindow(cs))
		return FALSE;

	cs.dwExStyle |= WS_EX_CLIENTEDGE;
	cs.style &= ~WS_BORDER;
	cs.lpszClass = AfxRegisterWndClass(CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS,
		::LoadCursor(nullptr, IDC_ARROW), reinterpret_cast<HBRUSH>(COLOR_WINDOW + 1), nullptr);

	return TRUE;
}

void CChildView::OnPaint()
{
	CPaintDC dc(this); // контекст устройства для рисования

	if (Index == 1)		// режим отображения MM_TEXT
	{
		Graph.Draw(dc, 1, 1);
	}

	if (Index == 2) // режим отображения MM_TEXT для окружности и восьмиугольника
	{
		GraphCircle.Draw(dc, 1, 1); // Рисуем окружность с рамкой и осями
		GraphOctagon.Draw(dc, 0, 0);
	}
}

double CChildView::MyF1(double x)
{
	if (x == 0.0) {
		return 1.0; // Предел sin(x)/x при x -> 0 равен 1
	}
	double y = sin(x) / x;
	return y;
}

double CChildView::MyF2(double x)
{
	double y = sqrt(fabs(x)) * sin(x); // Используем fabs для абсолютного значения (модуль)
	return y;
}


void CChildView::OnTestsF1()	// MM_TEXT
{
	double Xl = -3 * pi;		// Координата Х левого угла области
	double Xh = 3 * pi;		// Координата Х правого угла области
	double dX = 3.14159265358979323846 / 36;		// Шаг графика функции
	int N = (Xh - Xl) / dX;		// Количество точек графика
	X.RedimMatrix(N + 1);		// Создаем вектор с N+1 строками для хранения координат точек по Х
	Y.RedimMatrix(N + 1);		// Создаем вектор с N+1 строками для хранения координат точек по Y
	for (int i = 0; i <= N; i++)
	{
		X(i) = Xl + i * dX;		// Заполняем массивы/векторы значениями
		Y(i) = MyF1(X(i));
	}

	PenLine.Set(PS_SOLID, 1, RGB(255, 0, 0));	// Устанавливаем параметры пера для линий (сплошная линия, толщина 1, цвет красный)
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 255));	// Устанавливаем параметры пера для осей (сплошная линия, толщина 2, цвет синий)
	RW.SetRect(100, 100, 500, 500);				// Установка параметров прямоугольной области для отображения графика в окне
	Graph.SetParams(X, Y, RW);					// Передаем векторы с координатами точек и область в окне
	Graph.SetPenLine(PenLine);					// Установка параметров пера для линии графика
	Graph.SetPenAxis(PenAxis);					// Установка параметров пера для линий осей 
	Index = 1;									// Помечаем для режима отображения MM_TEXT
	this->Invalidate();
}

void CChildView::OnTestsF2()
{
	double Xl = -4 * pi;		// Координата Х левого угла области
	double Xh = 4 * pi;		// Координата Х правого угла области
	double dX = 3.14159265358979323846 / 36;		// Шаг графика функции
	int N = (Xh - Xl) / dX;		// Количество точек графика
	X.RedimMatrix(N + 1);		// Создаем вектор с N+1 строками для хранения координат точек по Х
	Y.RedimMatrix(N + 1);		// Создаем вектор с N+1 строками для хранения координат точек по Y
	for (int i = 0; i <= N; i++)
	{
		X(i) = Xl + i * dX;		// Заполняем массивы/векторы значениями
		Y(i) = MyF2(X(i));
	}
	PenLine.Set(PS_DASHDOT, 1, RGB(255, 0, 0));	// Устанавливаем параметры пера для линий (штрих - пунктирная, толщина 3, цвет красный)
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 0));	// Устанавливаем параметры пера для осей (сплошная линия, толщина 2, цвет черный)
	RW.SetRect(100, 100, 500, 500);				// Установка параметров прямоугольной области для отображения графика в окне
	Graph.SetParams(X, Y, RW);					// Передаем векторы с координатами точек и область в окне
	Graph.SetPenLine(PenLine);					// Установка параметров пера для линии графика
	Graph.SetPenAxis(PenAxis);					// Установка параметров пера для линий осей 
	Index = 1;									// Помечаем для режима отображения MM_TEXT
	this->Invalidate();
}

void CChildView::OnTestsF3()
{
	const double radius = 10.0; // Радиус окружности
	const int numSides = 8;     // Количество сторон восьмиугольника
	const double angleIncrement = 2 * 3.14159265358979323846 / numSides; // Угловой шаг

	// Создаем массивы для хранения координат вершин восьмиугольника
	CMatrix octagonX(numSides + 1), octagonY(numSides + 1);

	// Вычисляем координаты вершин восьмиугольника
	for (int i = 0; i < numSides; i++)
	{
		double angle = i * angleIncrement;
		octagonX(i) = radius * cos(angle);
		octagonY(i) = radius * sin(angle);
	}
	octagonX(numSides) = octagonX(0); // Замыкаем фигуру
	octagonY(numSides) = octagonY(0);

	const int numPoints = 100; // Количество точек для аппроксимации окружности
	CMatrix circleX(numPoints + 1), circleY(numPoints + 1);

	// Вычисляем координаты окружности
	for (int i = 0; i < numPoints; i++)
	{
		double angle = 2 * 3.14159265358979323846 * i / numPoints;
		circleX(i) = radius * cos(angle);
		circleY(i) = radius * sin(angle);
	}
	circleX(numPoints) = circleX(0); // Замыкаем окружность
	circleY(numPoints) = circleY(0);

	// Устанавливаем прямоугольную область для отображения графика
	RW.SetRect(100, 100, 500, 500);

	// Отрисовка окружности
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 255)); // Синий цвет для окружности
	GraphCircle.SetParams(circleX, circleY, RW); // Передаем координаты окружности
	GraphCircle.SetPenLine(PenAxis); // Устанавливаем параметры пера для линии графика (окружности)

	// Отрисовка восьмиугольника
	PenLine.Set(PS_SOLID, 3, RGB(255, 0, 0)); // Красный цвет для восьмиугольника
	GraphOctagon.SetParams(octagonX, octagonY, RW); // Передаем координаты восьмиугольника
	GraphOctagon.SetPenLine(PenLine); // Устанавливаем параметры пера для восьмиугольника

	Index = 2; // Помечаем для режима отображения MM_TEXT
	this->Invalidate(); // Обновляем окно для отображения графика
}