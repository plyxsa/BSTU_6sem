#include "stdafx.h"

CRectD::CRectD(double l, double t, double r, double b)
{
	left = l;
	top = t;
	right = r;
	bottom = b;
}
//------------------------------------------------------------------------------
void CRectD::SetRectD(double l, double t, double r, double b)
{
	left = l;
	top = t;
	right = r;
	bottom = b;
}

//------------------------------------------------------------------------------
CSizeD CRectD::SizeD()
{
	CSizeD cz;
	cz.cx = fabs(right - left);	// ������ ������������� �������
	cz.cy = fabs(top - bottom);	// ������ ������������� �������
	return cz;
}

//----------------------------------------------------------------------------

CMatrix CreateTranslate2D(double dx, double dy)
// ��������� ������� ��� �������������� ��������� ������� ��� ��� �������� 
// �� dx �� ��� X � �� dy �� ��� Y � ������������� ������� ���������
// ������� CreateTranslate2D(dx, dy) ��������� ������ �� (dx, dy).
// ���� ����� ��������� ��, ����������� �������� ������� CreateTranslate2D(-dx, -dy).
{
	CMatrix TM(3, 3);
	TM(0, 0) = 1; TM(0, 2) = dx;
	TM(1, 1) = 1;  TM(1, 2) = dy;
	TM(2, 2) = 1;
	return TM;
}

//------------------------------------------------------------------------------------
CMatrix CreateRotate2D(double fi)
// ��������� ������� ��� �������������� ��������� ������� ��� ��� ��������
// �� ���� fi (��� fi>0 ������ ������� �������)� ������������� ������� ���������
// ������� CreateRotate2D(fi) ������������ ������ �� ���� fi ������ ������� �������.
// ���� ����� ��������� ��, ����������� �������� ������� CreateRotate2D(-fi).
{
	double fg = fmod(fi, 360.0);
	double ff = (fg / 180.0) * pi; // ������� � �������
	CMatrix RM(3, 3);
	RM(0, 0) = cos(ff); RM(0, 1) = -sin(ff);
	RM(1, 0) = sin(ff);  RM(1, 1) = cos(ff);
	RM(2, 2) = 1;
	return RM;
}


//------------------------------------------------------------------------------

CMatrix SpaceToWindow(CRectD& RS, const CRect& RW) // ����� ������
{
	CMatrix M(3, 3);
	// CRect �� ����� ������ Size(), ���������� Width() � Height()
	int dwx = RW.Width(); // ������
	int dwy = RW.Height(); // ������
	CSizeD szd = RS.SizeD(); // ������ ������� � ������� �����������

	double dsx = szd.cx; // ������ � ������� �����������
	double dsy = szd.cy; // ������ � ������� �����������

	// ������ �� ������� �� ����, ���� ������� �������
	if (dsx == 0 || dsy == 0 || dwx == 0 || dwy == 0) {
			M(0, 0) = 1; M(1, 1) = 1; M(2, 2) = 1; // ��������, ���������
		return M;
	}

	double kx = (double)dwx / dsx; // ������� �� x
	double ky = (double)dwy / dsy; // ������� �� y

	// ��������� ����� ������� ������� ���������
	double rs_center_x = RS.left + dsx / 2.0;
	double rs_center_y = RS.bottom + dsy / 2.0; // � CRectD Y ������ ����� �����

	// ��������� ����� ������� ������� ��������� (� CRect Y ������ ������ ����)
	double rw_center_x = (double)RW.left + dwx / 2.0;
	double rw_center_y = (double)RW.top + dwy / 2.0; // ���������� RW.top

	// ��������� ����������� ��������
	// ������� ����� (rs_center_x, rs_center_y) ������ ������������ � (rw_center_x, rw_center_y)
	// ���������:
	// rw_center_x = kx * rs_center_x + dx  => dx = rw_center_x - kx * rs_center_x
	// rw_center_y = -ky * rs_center_y + dy => dy = rw_center_y + ky * rs_center_y
	double dx = rw_center_x - kx * rs_center_x;
	double dy = rw_center_y + ky * rs_center_y; // ��������� �������� Y (-ky)

	M(0, 0) = kx;	 M(0, 1) = 0;  M(0, 2) = dx;
	M(1, 0) = 0;  M(1, 1) = -ky;  M(1, 2) = dy; // �������� Y
	M(2, 0) = 0;  M(2, 1) = 0;	M(2, 2) = 1;
	return M;
}

//------------------------------------------------------------------------------

void SetMyMode(CDC& dc, CRectD& RS, CRect& RW)
// ������������� ����� ����������� MM_ANISOTROPIC � ��� ���������
// dc - ������ �� ����� CDC MFC
// RS -  ������� � ������� ����������� - int
// RW -	 ������� � ������� ����������� - int  
{
	double dsx = RS.right - RS.left;
	double dsy = RS.top - RS.bottom;
	double xsL = RS.left;
	double ysL = RS.bottom;

	int dwx = RW.right - RW.left;
	int dwy = RW.bottom - RW.top;
	int xwL = RW.left;
	int ywH = RW.bottom;

	dc.SetMapMode(MM_ANISOTROPIC);
	dc.SetWindowExt((int)dsx, (int)dsy);
	dc.SetViewportExt(dwx, -dwy);
	dc.SetWindowOrg((int)xsL, (int)ysL);
	dc.SetViewportOrg(xwL, ywH);
}



CBlade::CBlade()
{
	double rS = 30;
	double RoE = 10 * rS;
	RS.SetRectD(-RoE, RoE, RoE, -RoE);		    // ������� ������� � ������� �����������
	// �������� RW �� ������ ������� RS
	RW.SetRect(0, 0, static_cast<int>(RS.right - RS.left), static_cast<int>(RS.top - RS.bottom));
	MainPoint.SetRect(-rS, rS, rS, -rS);
	FirstTop.SetRect(-5, 5, 5, -5);
	SecondTop.SetRect(-5, 5, 5, -5);
	FirstBootom.SetRect(-5, 5, 5, -5);
	SecondBootom.SetRect(-5, 5, 5, -5);
	EarthOrbit.SetRect(-RoE, RoE, RoE, -RoE);
	fiSB = 215;
	fiFB = 205;
	fiST = 25;
	fiFT = 35;
	wPoint = -8;
	dt = 1;
	FTCoords.RedimMatrix(3);
	STCoords.RedimMatrix(3);
	FBCoords.RedimMatrix(3);
	SBCoords.RedimMatrix(3);

	// ��� ������ �������
	FTCoords2.RedimMatrix(3);
	STCoords2.RedimMatrix(3);
	FBCoords2.RedimMatrix(3);
	SBCoords2.RedimMatrix(3);
}

void CBlade::SetNewCoords()
{
	double RoV = (EarthOrbit.right - EarthOrbit.left) / 2;
	double ff = (fiFT / 90.0) * pi;
	double x = RoV * cos(ff);
	double y = RoV * sin(ff);
	FTCoords(0) = x;
	FTCoords(1) = y;
	FTCoords(2) = 1;

	fiFT += wPoint * dt;
	CMatrix P = CreateRotate2D(fiFT);
	FTCoords = P * FTCoords;

	RoV = (EarthOrbit.right - EarthOrbit.left) / 2;
	ff = (fiST / 90.0) * pi;
	x = RoV * cos(ff);
	y = RoV * sin(ff);
	STCoords(0) = x;
	STCoords(1) = y;
	STCoords(2) = 1;

	fiST += wPoint * dt;
	P = CreateRotate2D(fiST);
	STCoords = P * STCoords;

	RoV = (EarthOrbit.right - EarthOrbit.left) / 2;
	ff = (fiFB / 90.0) * pi;
	x = RoV * cos(ff);
	y = RoV * sin(ff);
	FBCoords(0) = x;
	FBCoords(1) = y;
	FBCoords(2) = 1;

	fiFB += wPoint * dt;
	P = CreateRotate2D(fiFB);
	FBCoords = P * FBCoords;

	RoV = (EarthOrbit.right - EarthOrbit.left) / 2;
	ff = (fiSB / 90.0) * pi;
	x = RoV * cos(ff);
	y = RoV * sin(ff);
	SBCoords(0) = x;
	SBCoords(1) = y;
	SBCoords(2) = 1;

	// ��� ������ �������
	fiSB += wPoint * dt;
	P = CreateRotate2D(fiSB);
	SBCoords = P * SBCoords;

	fiFT2 += wPoint * dt;
	CMatrix P2 = CreateRotate2D(fiFT2);
	FTCoords2 = P2 * FTCoords;

	fiST2 += wPoint * dt;
	P2 = CreateRotate2D(fiST2);
	STCoords2 = P2 * STCoords;

	fiFB2 += wPoint * dt;
	P2 = CreateRotate2D(fiFB2);
	FBCoords2 = P2 * FBCoords;

	fiSB2 += wPoint * dt;
	P2 = CreateRotate2D(fiSB2);
	SBCoords2 = P2 * SBCoords;

}


void CBlade::Draw(CDC& dc, const CRect& actualWindowRect)
{
	CBrush SBrush(RGB(0, 255, 0));
	CBrush EBrush(RGB(255, 0, 0));
	CBrush MBrush(RGB(0, 0, 255));

	// ���������� ���������� actualWindowRect ������ ����� RW
	CMatrix SW = SpaceToWindow(RS, actualWindowRect);

	dc.SelectStockObject(NULL_BRUSH);

	// ����������� � ������ ������
	POINT orbitPoints[2];
	orbitPoints[0].x = (int)(SW(0, 0) * EarthOrbit.left + SW(0, 2));
	orbitPoints[0].y = (int)(SW(1, 1) * EarthOrbit.top + SW(1, 2)); // ���������� top ��� Y
	orbitPoints[1].x = (int)(SW(0, 0) * EarthOrbit.right + SW(0, 2));
	orbitPoints[1].y = (int)(SW(1, 1) * EarthOrbit.bottom + SW(1, 2)); // ���������� bottom ��� Y

	// ����������� ���������� ������� (left, top, right, bottom)
	CRect transformedEarthOrbit;
	transformedEarthOrbit.left = min(orbitPoints[0].x, orbitPoints[1].x);
	transformedEarthOrbit.top = min(orbitPoints[0].y, orbitPoints[1].y); // min Y -> top
	transformedEarthOrbit.right = max(orbitPoints[0].x, orbitPoints[1].x);
	transformedEarthOrbit.bottom = max(orbitPoints[0].y, orbitPoints[1].y); // max Y -> bottom

	dc.Ellipse(transformedEarthOrbit);

	// ������ ������� (�������)
	DrawTransformedTriangle(FTCoords, STCoords, SW, dc, RGB(255, 0, 0));
	DrawTransformedTriangle(FBCoords, SBCoords, SW, dc, RGB(255, 0, 0));

	// ������ ������� (�����)
	DrawTransformedTriangle(FTCoords2, STCoords2, SW, dc, RGB(0, 0, 255));
	DrawTransformedTriangle(FBCoords2, SBCoords2, SW, dc, RGB(0, 0, 255));

	// ����������� ����� (������)
	CBrush* pOldBrush = dc.SelectObject(&SBrush);
	POINT centerPoint[2];
	centerPoint[0].x = (int)(SW(0, 0) * MainPoint.left + SW(0, 2));
	centerPoint[0].y = (int)(SW(1, 1) * MainPoint.top + SW(1, 2));
	centerPoint[1].x = (int)(SW(0, 0) * MainPoint.right + SW(0, 2));
	centerPoint[1].y = (int)(SW(1, 1) * MainPoint.bottom + SW(1, 2));

	// ����������� ���������� ������� (left, top, right, bottom)
	CRect transformedMainPoint;
	transformedMainPoint.left = min(centerPoint[0].x, centerPoint[1].x);
	transformedMainPoint.top = min(centerPoint[0].y, centerPoint[1].y);
	transformedMainPoint.right = max(centerPoint[0].x, centerPoint[1].x);
	transformedMainPoint.bottom = max(centerPoint[0].y, centerPoint[1].y);

	dc.Ellipse(transformedMainPoint);
	dc.SelectObject(pOldBrush);
}

void CBlade::DrawTransformedTriangle(CMatrix FCoords, CMatrix SCoords, CMatrix& SW, CDC& dc, COLORREF color) {
	CBrush TBrush(color);
	CBrush* pOldBrush = dc.SelectObject(&TBrush);

	POINT points[3];
	points[0].x = (int)(SW(0, 2)); // ������ ��������� ����� ��������������
	points[0].y = (int)(SW(1, 2));
	points[1].x = (int)(SW(0, 0) * FCoords(0) + SW(0, 1) * FCoords(1) + SW(0, 2));
	points[1].y = (int)(SW(1, 0) * FCoords(0) + SW(1, 1) * FCoords(1) + SW(1, 2));
	points[2].x = (int)(SW(0, 0) * SCoords(0) + SW(0, 1) * SCoords(1) + SW(0, 2));
	points[2].y = (int)(SW(1, 0) * SCoords(0) + SW(1, 1) * SCoords(1) + SW(1, 2));

	dc.Polygon(points, 3);

	dc.SelectObject(pOldBrush); // ��������������� �����
}


void CBlade::GetRS(CRectD& RSX)
// RS - ���������, ���� ������������ ��������� ������� �������
{
	RSX.left = RS.left;
	RSX.top = RS.top;
	RSX.right = RS.right;
	RSX.bottom = RS.bottom;
}