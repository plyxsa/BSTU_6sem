using namespace std;


typedef vector<CMatrix> CVecMatrix;
typedef vector<CVecMatrix> CMasMatrix;

typedef vector<CPoint> CVecPoint;
typedef vector<CVecPoint> CMatrPoint;


typedef double(*pfunc2)(double, double);// ��������� �� �������


class CPlot3D
{
private:
	pfunc2 pFunc;          // ��������� �� ������� f(x,y), ����������� ����������� 
	CRectD SpaceRect;      // ������������� �������, �� ������� ��������� �����������
	CMasMatrix MatrF;      // ������� ��� �������� ��������� ����� (x,y,z,1) �����������
	CMasMatrix MatrView;   // ������� ��� �������� ��������� ����� (x,y,1) �������� XY ������� ��
	CRectD ViewRect;       // ������������� �������, ������������ �������� ����������� ������� ��
	CRect WinRect;         // ������������� ������� � ������� ������� ��������� ���
	CMatrix ViewPoint;     // ������ (3x1) � ���������� ����� ���������� � ������� ����������� �� (r,fi,q)
	CMatrPoint MatrWindow; // ������� ��� �������� ������� ���������

public:
	CPlot3D();
	~CPlot3D() { pFunc = NULL; }
	void SetFunction(pfunc2 pF, CRectD RS, double dx, double dy);
	void SetViewPoint(double r, double fi, double q);

	CMatrix GetViewPoint();                 // ���������� ������ ViewPoint
	void SetWinRect(CRect Rect);            // ������������� ������� � ���� ��� ���������
	void CreateMatrF(double dx, double dy); // ��������� ������� MatrF ������������
	void SetMatrF(CMasMatrix &Matr);        // ������ �������� ������� MatrF �����
	void CreateMatrView();                  // ��������� ������� MatrView ������������ ����� �������� XY ������� ��
	void CreateMatrWindow();                // ��������� ������� MatrWindow
	int GetNumberRegion();                  // ���������� ����� ������� ��� ���������
	void Draw(CDC& dc);
};


double Function1(double x, double y)
{
	return x*x + y*y;
}
double Function2(double x, double y)
{
	return x*x - y*y;
}
double Function3(double x, double y)
{
	//return sqrt(1 - x*x - y*y);
	double r = sqrt(x*x + y*y);
	double z;
	if (r < 0.001)
		z = 10;
	else
		z = 10 * sin(r) / r;
	return z;
}


CPlot3D::CPlot3D()
{
	pFunc = NULL;
	ViewPoint.RedimMatrix(3);
	WinRect.SetRect(0, 0, 200, 200);
	ViewPoint(0) = 10, ViewPoint(1) = 30, ViewPoint(2) = 45;
}
void CPlot3D::SetFunction(pfunc2 pF, CRectD RS, double dx, double dy)
// ������������� ��������� �� ������ f(x,y)
// pFunc � ��������� �� ������� f(x,y) � �����������
// RS � �������  ��� (xmin, ymax, xmax, ymin) ��� �������
// dx, dy � ���� ��� ������� �������� f(x) �� x � y
// VP � ���������� ����� ���������� (r,fi,q) � ����������� ��
// RW � ������� � ���� ��� ���������
{
	pFunc = pF;
	SpaceRect.SetRectD(RS.left, RS.top, RS.right, RS.bottom);
	MatrF.clear();
	MatrView.clear();
	MatrWindow.clear();

	CreateMatrF(dx, dy);
	CreateMatrView();
	CreateMatrWindow();

}
void CPlot3D::SetViewPoint(double r, double fi, double q)
// ������������� ��������� ����� ���������� � ������� ��
// ���������� ������� ViewPoint
// r � ������ ������-�������
// fi � ������ (�� ��� OX ������ ������� �������)
// q � ���� (�� ��� Z �� ������� �������, 0-180)
{
	ViewPoint(0) = r, ViewPoint(1) = fi, ViewPoint(2) = q;
	MatrView.clear();
	CreateMatrView();
	MatrWindow.clear();
	CreateMatrWindow();
}
CMatrix CPlot3D::GetViewPoint()
{
	CMatrix P = ViewPoint;
	return P;
}
void CPlot3D::SetWinRect(CRect Rect)
{
	WinRect = Rect;
	MatrWindow.clear();
	CreateMatrWindow();
}
void CPlot3D::CreateMatrF(double dx, double dy)
// ��������� ������� MatrF ������������ ����� �����������
// dx � ��� ��� ������� �������� ������� f(x,y) ���
// dy � ��� ��� ������� �������� ������� f(x,y) ���
{
	double xL = SpaceRect.left;
	double xH = SpaceRect.right;
	double yL = SpaceRect.bottom;
	double yH = SpaceRect.top;
	CVecMatrix VecMatrix;
	CMatrix V(4);
	V(3) = 1;

	for (double x = xL; x <= xH; x += dx)
	{
		VecMatrix.clear();
		for (double y = yL; y <= yH; y += dy)
		{
			V(0) = x; V(1) = y; V(2) = pFunc(x, y);
			VecMatrix.push_back(V);
		}
		MatrF.push_back(VecMatrix);
	}
}
void CPlot3D::SetMatrF(CMasMatrix &Matr)
// ������ �������� ������� MatrF �����
// Matr � ������ �������� ��� MatrF
{
	CVecMatrix VecMatrix;
	CMatrix V(4);
	double xmin, xmax, ymin, ymax;
	pFunc = NULL;
	MatrF.clear();
	MatrView.clear();
	MatrWindow.clear();
	V = Matr[0][0];
	xmin = xmax = V(0);
	ymin = ymax = V(1);
	for (int i = 0; i < Matr.size(); i++)
	{
		VecMatrix.clear();
		for (int j = 0; j < Matr[i].size(); j++)
		{
			V = Matr[i][j];
			VecMatrix.push_back(V);
			double x = V(0);
			double y = V(1);
			if (x < xmin) xmin = x;
			if (x < xmax) xmax = x;
			if (y < ymin) ymin = y;
			if (y < ymax) ymax = y;
		}
		MatrF.push_back(VecMatrix);
	}
	SpaceRect.SetRectD(xmin, ymax, xmax, ymin);
	CreateMatrView();
	CreateMatrWindow();
}
int CPlot3D::GetNumberRegion()
// ���������� ����� ������� ��� ���������
{
	CMatrix CartPoint = SphereToCart(ViewPoint);
	double xView = CartPoint(0);
	double yView = CartPoint(1);
	double zView = CartPoint(2);

	double xL = SpaceRect.left;
	double xH = SpaceRect.right;
	double yL = SpaceRect.bottom;
	double yH = SpaceRect.top;

	// ���������� ��� �������� ����� ���������� ������������
	// �������� ��������� ��������� y1=y1(x) [����� (x)]
	double y1 = yL + (yH - yL)*(xView - xL) / (xH - xL);
	// �������� ��������� ��������� y2=y2(x) [����� (x)]
	double y2 = yH + (yH - yL)*(xView - xL) / (xH - xL);
	if ((yView <= y1) && (yView <= y2)) 
		return 1;
	if ((yView > y2) && (yView < y1)) 
		return 2;
	if ((yView >= y1) && (yView >= y2)) 
		return 3;
	if ((yView > y1) && (yView < y2)) 
		return 4;
}
void CPlot3D::CreateMatrView()
// ��������� ������� MatrView ������������ ����� �������� ����������� f(x,y)
// �� ��������� XY ������� ��
// �
// ���������� ������������� ������� ViewRect, ������������ ��������
// ����������� �� ��������� XY ������� ��
{
	CMatrix MV = CreateViewCoord(ViewPoint(0), ViewPoint(1), ViewPoint(2));
	CVecMatrix VecMatrix;
	CMatrix VX(4), V(3);
	V(2) = 1;
	double xmin = DBL_MAX;
	double xmax = DBL_MIN;
	double ymin = DBL_MAX;
	double ymax = DBL_MIN;

	for (int i = 0; i < MatrF.size(); i++)
	{
		VecMatrix.clear();
		for (int j = 0; j < MatrF[i].size(); j++)
		{
			VX = MatrF[i][j];
			VX = MV*VX;
			V(0) = VX(0); // x � ���������� ��������
			V(1) = VX(1); // y � ���������� ��������
			VecMatrix.push_back(V);

			// ��� ����������� ������� ViewRect
			double x = V(0);
			double y = V(1);
			if (x < xmin) xmin = x;
			if (x > xmax) xmax = x;
			if (y < ymin) ymin = y;
			if (y < ymax) xmin = y;
		}
		MatrView.push_back(VecMatrix);
	}
	ViewRect.SetRectD(xmin, ymax, xmax, ymin);
}
void CPlot3D::CreateMatrWindow()
// ��������� ������� MatrWindow �������� ������������ ����� �����������
{
	CMatrix MW = SpaceToWindow(ViewRect, WinRect);

	CVecPoint VecPoint;
	CMatrix V(3);
	for (int i = 0; i < MatrView.size(); i++)
	{
		VecPoint.clear();
		for (int j = 0; j < MatrView[i].size(); j++)
		{
			V = MatrView[i][j];
			V = MW*V;
			CPoint P((int)V(0), (int)V(1));
			VecPoint.push_back(P);
		}
		MatrWindow.push_back(VecPoint);
	}
}
void CPlot3D::Draw(CDC& dc)
{
	if (MatrWindow.empty())
	{
		TCHAR* error = TEXT("������ ������ ��� ��������� ����");
		MessageBox(NULL, error, TEXT("������"), MB_ICONSTOP);
		return;
	}
	CPoint pt[4];
	int kRegion = GetNumberRegion();
	int nRows = MatrWindow.size();
	int nCols = MatrWindow[0].size();

	switch (kRegion)
	{
	case 1:
	{
		for (int j = nCols - 1; j > 0; j--)
			for (int i = 0; i < nRows - 1; i++)
			{
				pt[0] = MatrWindow[i][j];
				pt[1] = MatrWindow[i][j - 1];
				pt[2] = MatrWindow[i + 1][j - 1];
				pt[3] = MatrWindow[i + 1][j];
				dc.Polygon(pt, 4);
			}
	} break;
	case 2:
	{
		for (int i = 0; i < nRows - 1; i++)
			for (int j = 0; i < nCols - 1; j++)
			{
				pt[0] = MatrWindow[i][j];
				pt[1] = MatrWindow[i][j + 1];
				pt[2] = MatrWindow[i + 1][j + 1];
				pt[3] = MatrWindow[i + 1][j];
				dc.Polygon(pt, 4);
			}
	} break;
	case 3:
	{
		for (int j = 0; j < nCols; j++)
			for (int i = 0; i < nRows - 1; i++)
			{
				pt[0] = MatrWindow[i][j];
				pt[1] = MatrWindow[i][j + 1];
				pt[2] = MatrWindow[i + 1][j + 1];
				pt[3] = MatrWindow[i + 1][j];
				dc.Polygon(pt, 4);
			}
	} break;
	case 4:
	{
		for (int i = nRows - 1; i > 0; i--)
			for (int j = 0; i < nCols - 1; j++)
			{
				pt[0] = MatrWindow[i][j];
				pt[1] = MatrWindow[i][j + 1];
				pt[2] = MatrWindow[i - 1][j + 1];
				pt[3] = MatrWindow[i - 1][j];
				dc.Polygon(pt, 4);
			}
	} break;
	}
}


void DrawLightSphere(CDC& dc, double Radius, CMatrix& PView_Observer, CMatrix& PSourceLight, CRect RW, COLORREF Color, int Index)
// PView_Observer - ����������� (����������� r, phi, theta)
// PSourceLight - �������� ����� (����������� r, phi, theta)
{
	BYTE R_component = GetRValue(Color);
	BYTE G_component = GetGValue(Color);
	BYTE B_component = GetBValue(Color);

	// ������� ��� ������ (��������� ����� �������� ������ �����, ���� ��� ��� �����)
	CMatrix point_on_sphere_sph(3);    // (r, phi, theta) ��� ������� ����� �� �����
	CMatrix P_world_cart(3);           // ������� ����� �� ����� � ���������� ������� �����������
	CMatrix N_world_norm(3);           // ������� � ����� � ����� P_world_cart (���������������)
	CMatrix L_world_norm(3);           // ������ �� ����� P_world_cart � ��������� ����� (���������������)
	CMatrix V_world_norm(3);           // ������ �� ����� P_world_cart � ����������� (���������������)
	CMatrix P_view_homogeneous(4);     // ����� � ������� ���������� �����������
	CMatrix P_screen_coords(3);        // �������� ���������� (x, y, 1)

	COLORREF finalPixelColor;
	double df_step = 0.7; // ��� �� ������� (phi). ������� ��������.
	double dq_step = 0.7; // ��� �� ���� ����� (theta). ������� ��������.
	double kLightIntensity; // �������� ����������� ������������ [0,1]

	// 1. ������������� ������ ��� ����� �� �����
	point_on_sphere_sph(0) = 0.6;

	// 2. ����������� ��������� ����������� � ��������� ����� � ��������� ������� ����������
	CMatrix V_Observer_world_cart = SphereToCart(PView_Observer);
	CMatrix V_SourceLight_world_cart = SphereToCart(PSourceLight);

	// 3. ������� ������� ���� (�� ������� �� � ������� ��)
	CMatrix MV_world_to_view = CreateViewCoord(PView_Observer(0), PView_Observer(1), PView_Observer(2));

	// 4. ���������� ������� �������� � ������� ������� ��������� (XY ���������)
	CRectD RV_view_projection_plane(-Radius, Radius, Radius, -Radius); // left, top, right, bottom

	// 5. ������� ������� �������������� �� ������� ������������ ��������� � �������� ����������
	CMatrix MW_view_to_screen = SpaceToWindow(RV_view_projection_plane, RW);

	// ������������ ��������� ��� ���������
	double k_d = 0.7; // ��������� ����������� ��������� ���������
	double k_s = 1; // ���������� ����������� ��������� ���������
	double shininess = 3.0; // ���������� ������

	// �������� �� ������ �����
	for (double fi_deg = 0; fi_deg < 360.0; fi_deg += df_step)
	{
		point_on_sphere_sph(1) = fi_deg; // phi
		for (double q_deg = 0; q_deg <= 180.0; q_deg += dq_step)
		{
			point_on_sphere_sph(2) = q_deg; // theta

			// ��������� ���������� ������� ����� �� ����� � ������� ��
			P_world_cart = SphereToCart(point_on_sphere_sph);

			// --- ������ ������� N (���������������) ---
			N_world_norm = P_world_cart; // ��� ����� �� (0,0,0) ������ � ����� = �������
			double mod_N = ModVec(N_world_norm);
			if (mod_N > 1e-9) {
				N_world_norm(0) /= mod_N; N_world_norm(1) /= mod_N; N_world_norm(2) /= mod_N;
			}
			else { // ������������ ��� Radius > 0
				N_world_norm(0) = 0; N_world_norm(1) = 0; N_world_norm(2) = 1; // �������� �������
			}

			// --- ������ � ����������� V (���������������) ---
			CMatrix V_to_observer_temp = V_Observer_world_cart - P_world_cart;
			V_world_norm = V_to_observer_temp;
			double mod_V = ModVec(V_world_norm);
			if (mod_V > 1e-9) {
				V_world_norm(0) /= mod_V; V_world_norm(1) /= mod_V; V_world_norm(2) /= mod_V;
			} // else V_world_norm ��������� (0,0,0)

			// ��������� ��������� ������ (Back-face culling)
			// ����� �����, ���� ��������� ������������ (N . V) > 0
			if (ScalarMult(N_world_norm, V_world_norm) <= 1e-6) // ����� ����� ��� ��������
			{
				continue; // ����� �� ����� �����������, ���������� ��
			}

			// --- �������������� ��������� ����� ��� ��������� �� ������ ---
			P_view_homogeneous(0) = P_world_cart(0);
			P_view_homogeneous(1) = P_world_cart(1);
			P_view_homogeneous(2) = P_world_cart(2);
			P_view_homogeneous(3) = 1.0;

			P_view_homogeneous = MV_world_to_view * P_view_homogeneous; // � ������� ��

			// ��� �����������������/��������������� �������� ����� Xv, Yv
			P_screen_coords(0) = P_view_homogeneous(0); // X_view
			P_screen_coords(1) = P_view_homogeneous(1); // Y_view
			P_screen_coords(2) = 1.0;                   // ��� 2D ��������������

			P_screen_coords = MW_view_to_screen * P_screen_coords; // � �������� ����������
			int screen_x = static_cast<int>(round(P_screen_coords(0)));
			int screen_y = static_cast<int>(round(P_screen_coords(1)));


			// --- ������ ������������ ---
			kLightIntensity = 0.0; // ������������� �������� �������������

			// --- ������ � ��������� ����� L (���������������) ---
			CMatrix L_to_source_temp = V_SourceLight_world_cart - P_world_cart;
			L_world_norm = L_to_source_temp;
			double mod_L = ModVec(L_world_norm);
			if (mod_L > 1e-9) {
				L_world_norm(0) /= mod_L; L_world_norm(1) /= mod_L; L_world_norm(2) /= mod_L;
			} // else L_world_norm ��������� (0,0,0)

			// ��������� ������������ ������� � ������� � �����
			double N_dot_L = ScalarMult(N_world_norm, L_world_norm);

			if (Index == 0) {
				if (N_dot_L > 0) // ������ ���� ���� ������ �� ������� ������� �����������
				{
					// ��������� ���������
					double diffuse_intensity = k_d * N_dot_L;
					kLightIntensity += diffuse_intensity;
				}
			}
			// ���������� ��������� (������ ���� ������� ��������������� ������)
			if (Index == 1)
			{
				// ������ ��������� R = 2 * (N.L) * N - L
				CMatrix R_reflected_world(3);
				R_reflected_world(0) = 2.0 * N_dot_L * N_world_norm(0) - L_world_norm(0);
				R_reflected_world(1) = 2.0 * N_dot_L * N_world_norm(1) - L_world_norm(1);
				R_reflected_world(2) = 2.0 * N_dot_L * N_world_norm(2) - L_world_norm(2);
				// R_reflected_world �� ����� ������������� ���������������, ���� N � L ���� �������������

				double R_dot_V = ScalarMult(R_reflected_world, V_world_norm);
				if (R_dot_V > 0)
				{
					double specular_intensity = k_s * pow(R_dot_V, shininess);
					kLightIntensity += specular_intensity;
				}
			}
			// ����� �������� Ambient ��������� ����:
			// double ambient_intensity = 0.1; // ������
			// kLightIntensity += ambient_intensity;

			// ������������ �������� ������������� ���������� [0, 1]
			kLightIntensity = max(0.0, min(1.0, kLightIntensity));

			finalPixelColor = RGB(
				static_cast<BYTE>(kLightIntensity * R_component),
				static_cast<BYTE>(kLightIntensity * G_component),
				static_cast<BYTE>(kLightIntensity * B_component)
			);

			// ������ �������, ���� �� � �������� ������� ���������
			// ��� �������� ����� ���� ��������, ���� SpaceToWindow ������ ���������� ���������� ������ RW.
			// if (RW.PtInRect(CPoint(screen_x, screen_y))) 
			// {
			dc.SetPixelV(screen_x, screen_y, finalPixelColor);
			// }
			// ���� ��� �� ����� ��� ����� ��������, �������, ��� RW �� ������� ���������
			// � ��� P_screen_coords ����������� ���������.
		}
	}
}