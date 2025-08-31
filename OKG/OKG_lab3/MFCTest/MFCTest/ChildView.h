
// ChildView.h: интерфейс класса CChildView
//


#pragma once


// Окно CChildView

class CChildView : public CWnd
{
// Создание
public:
	int Index;
	CString ss1,ss2;
	CChildView();

// Атрибуты
public:

// Операции
public:

// Переопределение
	protected:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);

// Реализация
public:
	virtual ~CChildView();

	// Созданные функции схемы сообщений
protected:
	afx_msg void OnPaint();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnMymenuMenu1();
	afx_msg void OnMymenuMenu2();
	afx_msg void OnGrafi32775();
	afx_msg void OnGrafi32776();
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnMymenuMatrix();
};

