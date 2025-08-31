
// ChildView.h: ��������� ������ CChildView
//


#pragma once


// ���� CChildView

class CChildView : public CWnd
{
	// ��������
public:
	CChildView();

	// ��������
public:
	int Index;
	CMatrix X, Y;			// ������� ���������
	CRect RW;				// ������������� � ����
	CRectD RS;				// ������������� ������� � ���
	CPlot2D Graph;			// ������ �������
	CPlot2D GraphCircle;    // ������ ��� ����������
	CPlot2D GraphOctagon;   // ������ ��� ���������������
	CMyPen PenLine,			// ���� ��� �����
		PenAxis;			// ���� ��� ����
	// ��������
public:
	double MyF1(double x);	// ������ �������
	double MyF2(double x);
	double MyF3(double x);
	// ���������������
protected:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);

	// ����������
public:
	virtual ~CChildView();

	// ��������� ������� ����� ���������
protected:
	afx_msg void OnPaint();
	DECLARE_MESSAGE_MAP()
public:
	// �������� ��� ������ ������ ����
	afx_msg void OnTestsF1();
	afx_msg void OnTestsF2();
	afx_msg void OnTestsF3();
};