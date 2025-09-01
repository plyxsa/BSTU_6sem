#ifndef LIBPLANETS
#define LIBPLANETS 1
const double pi = 3.14159;


struct CSizeD
{
	double cx;
	double cy;
};
//-------------------------------------------------------------------------------
struct CRectD
{
	double left;
	double top;
	double right;
	double bottom;
	CRectD() { left = top = right = bottom = 0; };
	CRectD(double l, double t, double r, double b);
	void SetRectD(double l, double t, double r, double b);
	CSizeD SizeD();		// ���������� �������(������, ������) �������������� 
};
//-------------------------------------------------------------------------------

CMatrix CreateTranslate2D(double dx, double dy);
CMatrix CreateRotate2D(double fi);
CMatrix SpaceToWindow(CRectD& RS, const CRect& RW);
void SetMyMode(CDC& dc, CRectD& RS, CRect& RW);




class CBlade
{
	CRect MainPoint;
	CRect FirstTop;
	CRect SecondTop;
	CRect FirstBootom;
	CRect SecondBootom;
	CRect EarthOrbit;
	CMatrix FTCoords;
	CMatrix STCoords;
	CMatrix FBCoords;
	CMatrix SBCoords;
	CRect RW;		   // ������������� � ����
	CRectD RS;		   // ������������� ������� � ���
	double wPoint;		//������� ��������
	double fiSB;
	double fiFB;
	double fiST;
	double fiFT;
	// ��� ������ �������
	CMatrix FTCoords2, STCoords2, FBCoords2, SBCoords2;
	double fiSB2, fiFB2, fiST2, fiFT2;



	double dt;		   // �������� �������������, ���.
public:
	CBlade();
	void DrawTriangle(CMatrix FCoords, CMatrix SCoords, CDC& dc, COLORREF color);
	void DrawTransformedTriangle(CMatrix FCoords, CMatrix SCoords, CMatrix& SW, CDC& dc, COLORREF color);
	void SetDT(double dtx) { dt = dtx; };	// ��������� ��������� �������������
	void SetNewCoords();					// ��������� ����� ���������� ������
	void GetRS(CRectD& RSX);				// ���������� ������� ������� � ������� ��
	CRect GetRW() { return RW; };			// ���������� ������� ������� � ����	
	void Draw(CDC& dc, const CRect& actualWindowRect);					// ��������� ��� ���������������� ��������� ���������
};


#endif

