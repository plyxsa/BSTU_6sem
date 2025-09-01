#include "CMatrix.h"
#include "CPyramid.h"
#include <afxwin.h>
#include <windows.h>
#include <fstream>
#include <string>
#include <vector>

#define ID_Dr1 2002
#define ID_Dr 2003
#define ID_AngF 2004
#define ID_AngD 2005


class CMainWin : public CFrameWnd
{
public:
	// ������������� ���������� ��� �������� ������� ���������� ������
	int ang1 = 10;
	int ang2 = 315;
	int ang3 = 45;

	CMainWin()
	{
		Create(NULL, L"Lab07");
		Index = 0;
		PView.RedimMatrix(3);
		// ������������� ��������� �������� PView ��� �������� ����

		PView(0) = ang1;
		PView(1) = ang2;
		PView(2) = ang3;

	}
	CMenu *menu;
	CRect WinRect;
	CPyramid PIR;
	CMatrix PView;
	int Index;
	afx_msg int OnCreate(LPCREATESTRUCT);
	afx_msg void OnPaint();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	void Dr1();
	void Dr();
	void AngF();
	void AngD();
	DECLARE_MESSAGE_MAP();
};
class CMyApp : public CWinApp
{
public:
	CMyApp() {};
	virtual BOOL InitInstance()
	{
		m_pMainWnd = new CMainWin();
		int windowWidth = 1100; 
		int windowHeight = 960;

		int screenWidth = GetSystemMetrics(SM_CXSCREEN);
		int screenHeight = GetSystemMetrics(SM_CYSCREEN);
		int xPos = (screenWidth - windowWidth) / 2;
		int yPos = (screenHeight - windowHeight) / 2;

		m_pMainWnd->SetWindowPos(NULL, xPos, yPos, windowWidth, windowHeight, SWP_SHOWWINDOW);

		return TRUE;
	}
};
BEGIN_MESSAGE_MAP(CMainWin, CFrameWnd)
	ON_WM_CREATE()
	ON_WM_PAINT()
	ON_COMMAND(ID_Dr1, Dr1)
	ON_COMMAND(ID_Dr, Dr)
	ON_COMMAND(ID_AngF, AngF)
	ON_COMMAND(ID_AngD, AngD)
	ON_WM_SIZE()
END_MESSAGE_MAP()

// �������� ���������� ��� ���������
// ����������, ����� ���� ��������� � �����������
afx_msg void CMainWin::OnPaint()
{
	CPaintDC dc(this);
	if (Index == 1) PIR.Draw(dc, PView, WinRect);
	if (Index == 2) PIR.Draw1(dc, PView, WinRect); // ������ �������

}

afx_msg int CMainWin::OnCreate(LPCREATESTRUCT)
{
	menu = new CMenu();
	if (!menu->CreateMenu()) // �������� �������� ��������� ����
	{
		// Handle error
		delete menu;
		menu = NULL;
		return -1; // Indicate creation failure
	}

	// ������� ���������� ���� (popup menu)
	CMenu popupMenu;
	if (!popupMenu.CreatePopupMenu()) // �������� �������� ����������� ����
	{
		// Handle error
		delete menu;
		menu = NULL;
		return -1; // Indicate creation failure
	}

	// ��������� ������ ������ � ���������� ����
	popupMenu.AppendMenu(MF_STRING, ID_Dr1, _T("��� �����"));
	popupMenu.AppendMenu(MF_STRING, ID_Dr, _T("������ ������� �����"));

	// ��������� ����������� (�����������)
	popupMenu.AppendMenu(MF_SEPARATOR);

	popupMenu.AppendMenu(MF_STRING, ID_AngF, _T("������� ��������� ������"));
	popupMenu.AppendMenu(MF_STRING, ID_AngD, _T("��������� ������ �� ���������"));

	menu->AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popupMenu.Detach(), _T("��������")); 

	SetMenu(menu);

	return 0; // Indicate creation success
}

void CMainWin::Dr1() // ��� �����
{
	PView(0) = ang1;
	PView(1) = ang2;
	PView(2) = ang3;
	Index = 1;
	Invalidate();
}

void CMainWin::Dr() // ������ ������� �����
{
	PView(0) = ang1;
	PView(1) = ang2;
	PView(2) = ang3;
	Index = 2;
	Invalidate();
}

void CMainWin::AngF() // ������� ��������� ������ �� �����
{
	std::ifstream file("angles.txt");
	std::string line;
	std::vector<std::string> lines;

	if (file.is_open()) {
		while (std::getline(file, line)) {

			lines.push_back(line);
		}
		file.close();
	}
	
	ang2 = stoi(lines[0]);
	ang3 = stoi(lines[1]);
	PView(1) = ang2;
	PView(2) = ang3;

	Invalidate();
}

void CMainWin::AngD()
{
	ang1 = 10;   
	ang2 = 315; 
	ang3 = 45; 

	PView(0) = ang1;
	PView(1) = ang2;
	PView(2) = ang3;
	Invalidate();
}
afx_msg void CMainWin::OnSize(UINT nType, int cx, int cy)
{
	CFrameWnd::OnSize(nType, cx, cy); // �������� ������� �����

	// ���������� ������������� ��� ��������� (RW)
	int margin = 10;
	if (cx > 2 * margin && cy > 2 * margin)
	{
		WinRect.SetRect(margin, margin, cx - margin, cy - margin);
	}
	else // ���� ���� ����� ���������, �������� ��� �������
	{
		WinRect.SetRect(0, 0, cx, cy);
	}

}


CMyApp theApp;