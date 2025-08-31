
// ChildView.cpp: реализация класса CChildView
//

#include "pch.h"
#include "framework.h"
#include "MFCTest.h"
#include "ChildView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CChildView

CChildView::CChildView()
{
	Index = 0;
}

CChildView::~CChildView()
{
}


BEGIN_MESSAGE_MAP(CChildView, CWnd)
	ON_WM_PAINT()
	ON_COMMAND(ID_MYMENU_MENU1, &CChildView::OnMymenuMenu1)
	ON_COMMAND(ID_MYMENU_MENU3, &CChildView::OnMymenuMenu2)
	ON_COMMAND(ID_GRAFI32775, &CChildView::OnGrafi32775)
	ON_COMMAND(ID_GRAFI32776, &CChildView::OnGrafi32776)
	ON_WM_LBUTTONDBLCLK()
	ON_COMMAND(ID_MYMENU_MATRIX, &CChildView::OnMymenuMatrix)
END_MESSAGE_MAP()



// Обработчики сообщений CChildView

BOOL CChildView::PreCreateWindow(CREATESTRUCT& cs) 
{
	if (!CWnd::PreCreateWindow(cs))
		return FALSE;

	cs.dwExStyle |= WS_EX_CLIENTEDGE;
	cs.style &= ~WS_BORDER;
	cs.lpszClass = AfxRegisterWndClass(CS_HREDRAW|CS_VREDRAW|CS_DBLCLKS, 
		::LoadCursor(nullptr, IDC_ARROW), reinterpret_cast<HBRUSH>(COLOR_WINDOW+1), nullptr);

	return TRUE;
}

void CChildView::OnPaint() 
{
	CPaintDC dc(this); // контекст устройства для рисования

	if (Index == 1)
	{
		dc.MoveTo(0, 0);
		dc.LineTo(200, 300);
		dc.LineTo(400, 100);
	}

	if (Index == 2)
	{
		dc.Rectangle(200, 200, 400, 400);
	}


	if (Index == 3)
	{
		dc.TextOutA(15, 15, ss1);
		dc.TextOutA(15, 60, ss2);
	}

	
	// TODO: Добавьте код обработки сообщений
	
	// Не вызывайте CWnd::OnPaint() для сообщений рисования
}



void CChildView::OnMymenuMenu1()
{

	AfxMessageBox("Это Menu1");

	// TODO: добавьте свой код обработчика команд
}


void CChildView::OnMymenuMenu2()
{

AfxMessageBox("Это Menu2");
	// TODO: добавьте свой код обработчика команд
}


void CChildView::OnGrafi32775()
{
	Index = 1;
	Invalidate();
	
	// TODO: добавьте свой код обработчика команд
}


void CChildView::OnGrafi32776()
{
	Index = 2;
	Invalidate();
	// TODO: добавьте свой код обработчика команд
}


void CChildView::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	// TODO: добавьте свой код обработчика сообщений или вызов стандартного
	Index = 0;
	Invalidate();

	CWnd::OnLButtonDblClk(nFlags, point);
}


void CChildView::OnMymenuMatrix()
{
	
	CMatrix V1(2), V2(2);
	V1(0) = 1;   V1(1) = 2;
	V2(0) = 3;   V2(1) = 4;
	
	CMatrix S = V1 + V2;  //Сумма V1+V2

	ss1.Format("S(0)=%5.1f      S(1)=%5.1f ", S(0), S(1));  // Сторка для вывода S

	CMatrix W=V1.Transp()*V2; // Скалярное произведение векторов V1*V2
	double p = W(0, 0);
	ss2.Format("p=%5.1f",p);  // Строка для вывода p
	
	Index = 3;

	this->Invalidate();
	

	// TODO: добавьте свой код обработчика команд
}
