
// ChildView.cpp: ���������� ������ CChildView
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
	// ��������� ���� ������
	ON_COMMAND(ID_TESTS_TEST, &CChildView::OnTestsF1)
	ON_COMMAND(ID_TESTS_F2, &CChildView::OnTestsF2)
	ON_COMMAND(ID_TESTS_F12, &CChildView::OnTestsF3)

END_MESSAGE_MAP()



// ����������� ��������� CChildView

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
	CPaintDC dc(this); // �������� ���������� ��� ���������

	if (Index == 1)		// ����� ����������� MM_TEXT
	{
		Graph.Draw(dc, 1, 1);
	}

	if (Index == 2) // ����� ����������� MM_TEXT ��� ���������� � ���������������
	{
		GraphCircle.Draw(dc, 1, 1); // ������ ���������� � ������ � �����
		GraphOctagon.Draw(dc, 0, 0);
	}
}

double CChildView::MyF1(double x)
{
	if (x == 0.0) {
		return 1.0; // ������ sin(x)/x ��� x -> 0 ����� 1
	}
	double y = sin(x) / x;
	return y;
}

double CChildView::MyF2(double x)
{
	double y = sqrt(fabs(x)) * sin(x); // ���������� fabs ��� ����������� �������� (������)
	return y;
}


void CChildView::OnTestsF1()	// MM_TEXT
{
	double Xl = -3 * pi;		// ���������� � ������ ���� �������
	double Xh = 3 * pi;		// ���������� � ������� ���� �������
	double dX = 3.14159265358979323846 / 36;		// ��� ������� �������
	int N = (Xh - Xl) / dX;		// ���������� ����� �������
	X.RedimMatrix(N + 1);		// ������� ������ � N+1 �������� ��� �������� ��������� ����� �� �
	Y.RedimMatrix(N + 1);		// ������� ������ � N+1 �������� ��� �������� ��������� ����� �� Y
	for (int i = 0; i <= N; i++)
	{
		X(i) = Xl + i * dX;		// ��������� �������/������� ����������
		Y(i) = MyF1(X(i));
	}

	PenLine.Set(PS_SOLID, 1, RGB(255, 0, 0));	// ������������� ��������� ���� ��� ����� (�������� �����, ������� 1, ���� �������)
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 255));	// ������������� ��������� ���� ��� ���� (�������� �����, ������� 2, ���� �����)
	RW.SetRect(100, 100, 500, 500);				// ��������� ���������� ������������� ������� ��� ����������� ������� � ����
	Graph.SetParams(X, Y, RW);					// �������� ������� � ������������ ����� � ������� � ����
	Graph.SetPenLine(PenLine);					// ��������� ���������� ���� ��� ����� �������
	Graph.SetPenAxis(PenAxis);					// ��������� ���������� ���� ��� ����� ���� 
	Index = 1;									// �������� ��� ������ ����������� MM_TEXT
	this->Invalidate();
}

void CChildView::OnTestsF2()
{
	double Xl = -4 * pi;		// ���������� � ������ ���� �������
	double Xh = 4 * pi;		// ���������� � ������� ���� �������
	double dX = 3.14159265358979323846 / 36;		// ��� ������� �������
	int N = (Xh - Xl) / dX;		// ���������� ����� �������
	X.RedimMatrix(N + 1);		// ������� ������ � N+1 �������� ��� �������� ��������� ����� �� �
	Y.RedimMatrix(N + 1);		// ������� ������ � N+1 �������� ��� �������� ��������� ����� �� Y
	for (int i = 0; i <= N; i++)
	{
		X(i) = Xl + i * dX;		// ��������� �������/������� ����������
		Y(i) = MyF2(X(i));
	}
	PenLine.Set(PS_DASHDOT, 1, RGB(255, 0, 0));	// ������������� ��������� ���� ��� ����� (����� - ����������, ������� 3, ���� �������)
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 0));	// ������������� ��������� ���� ��� ���� (�������� �����, ������� 2, ���� ������)
	RW.SetRect(100, 100, 500, 500);				// ��������� ���������� ������������� ������� ��� ����������� ������� � ����
	Graph.SetParams(X, Y, RW);					// �������� ������� � ������������ ����� � ������� � ����
	Graph.SetPenLine(PenLine);					// ��������� ���������� ���� ��� ����� �������
	Graph.SetPenAxis(PenAxis);					// ��������� ���������� ���� ��� ����� ���� 
	Index = 1;									// �������� ��� ������ ����������� MM_TEXT
	this->Invalidate();
}

void CChildView::OnTestsF3()
{
	const double radius = 10.0; // ������ ����������
	const int numSides = 8;     // ���������� ������ ���������������
	const double angleIncrement = 2 * 3.14159265358979323846 / numSides; // ������� ���

	// ������� ������� ��� �������� ��������� ������ ���������������
	CMatrix octagonX(numSides + 1), octagonY(numSides + 1);

	// ��������� ���������� ������ ���������������
	for (int i = 0; i < numSides; i++)
	{
		double angle = i * angleIncrement;
		octagonX(i) = radius * cos(angle);
		octagonY(i) = radius * sin(angle);
	}
	octagonX(numSides) = octagonX(0); // �������� ������
	octagonY(numSides) = octagonY(0);

	const int numPoints = 100; // ���������� ����� ��� ������������� ����������
	CMatrix circleX(numPoints + 1), circleY(numPoints + 1);

	// ��������� ���������� ����������
	for (int i = 0; i < numPoints; i++)
	{
		double angle = 2 * 3.14159265358979323846 * i / numPoints;
		circleX(i) = radius * cos(angle);
		circleY(i) = radius * sin(angle);
	}
	circleX(numPoints) = circleX(0); // �������� ����������
	circleY(numPoints) = circleY(0);

	// ������������� ������������� ������� ��� ����������� �������
	RW.SetRect(100, 100, 500, 500);

	// ��������� ����������
	PenAxis.Set(PS_SOLID, 2, RGB(0, 0, 255)); // ����� ���� ��� ����������
	GraphCircle.SetParams(circleX, circleY, RW); // �������� ���������� ����������
	GraphCircle.SetPenLine(PenAxis); // ������������� ��������� ���� ��� ����� ������� (����������)

	// ��������� ���������������
	PenLine.Set(PS_SOLID, 3, RGB(255, 0, 0)); // ������� ���� ��� ���������������
	GraphOctagon.SetParams(octagonX, octagonY, RW); // �������� ���������� ���������������
	GraphOctagon.SetPenLine(PenLine); // ������������� ��������� ���� ��� ���������������

	Index = 2; // �������� ��� ������ ����������� MM_TEXT
	this->Invalidate(); // ��������� ���� ��� ����������� �������
}