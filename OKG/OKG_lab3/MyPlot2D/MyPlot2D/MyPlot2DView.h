
// MyPlot2DView.h: интерфейс класса CMyPlot2DView
//

#pragma once


class CMyPlot2DView : public CView
{
protected: // создать только из сериализации
	CMyPlot2DView() noexcept;
	DECLARE_DYNCREATE(CMyPlot2DView)

// Атрибуты
public:
	CMyPlot2DDoc* GetDocument() const;

// Операции
public:

// Переопределение
public:
	virtual void OnDraw(CDC* pDC);  // переопределено для отрисовки этого представления
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
protected:
	virtual BOOL OnPreparePrinting(CPrintInfo* pInfo);
	virtual void OnBeginPrinting(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnEndPrinting(CDC* pDC, CPrintInfo* pInfo);

// Реализация
public:
	virtual ~CMyPlot2DView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Созданные функции схемы сообщений
protected:
	DECLARE_MESSAGE_MAP()
};

#ifndef _DEBUG  // версия отладки в MyPlot2DView.cpp
inline CMyPlot2DDoc* CMyPlot2DView::GetDocument() const
   { return reinterpret_cast<CMyPlot2DDoc*>(m_pDocument); }
#endif

