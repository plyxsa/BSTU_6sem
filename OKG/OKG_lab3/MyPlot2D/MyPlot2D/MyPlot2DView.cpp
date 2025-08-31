
// MyPlot2DView.cpp: реализация класса CMyPlot2DView
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS можно определить в обработчиках фильтров просмотра реализации проекта ATL, эскизов
// и поиска; позволяет совместно использовать код документа в данным проекте.
#ifndef SHARED_HANDLERS
#include "MyPlot2D.h"
#endif

#include "MyPlot2DDoc.h"
#include "MyPlot2DView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CMyPlot2DView

IMPLEMENT_DYNCREATE(CMyPlot2DView, CView)

BEGIN_MESSAGE_MAP(CMyPlot2DView, CView)
	// Стандартные команды печати
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CView::OnFilePrintPreview)
END_MESSAGE_MAP()

// Создание или уничтожение CMyPlot2DView

CMyPlot2DView::CMyPlot2DView() noexcept
{
	// TODO: добавьте код создания

}

CMyPlot2DView::~CMyPlot2DView()
{
}

BOOL CMyPlot2DView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: изменить класс Window или стили посредством изменения
	//  CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

// Рисование CMyPlot2DView

void CMyPlot2DView::OnDraw(CDC* /*pDC*/)
{
	CMyPlot2DDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: добавьте здесь код отрисовки для собственных данных
}


// Печать CMyPlot2DView

BOOL CMyPlot2DView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// подготовка по умолчанию
	return DoPreparePrinting(pInfo);
}

void CMyPlot2DView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: добавьте дополнительную инициализацию перед печатью
}

void CMyPlot2DView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: добавьте очистку после печати
}


// Диагностика CMyPlot2DView

#ifdef _DEBUG
void CMyPlot2DView::AssertValid() const
{
	CView::AssertValid();
}

void CMyPlot2DView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CMyPlot2DDoc* CMyPlot2DView::GetDocument() const // встроена неотлаженная версия
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CMyPlot2DDoc)));
	return (CMyPlot2DDoc*)m_pDocument;
}
#endif //_DEBUG


// Обработчики сообщений CMyPlot2DView
