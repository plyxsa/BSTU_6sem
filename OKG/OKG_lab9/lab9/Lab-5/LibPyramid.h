#pragma once
#include <algorithm> 
#include <vector>


class CPyramid
{
private:
    CMatrix Vertices; // ���������� ������
    void GetRect(CMatrix& Vert, CRectD& RectView);

    // ��������������� ������� ��� ��������� 3D ������� (��� ���������� ����������)
    CMatrix GetVertex(int index) {
        return Vertices.GetCol(index, 0, 2);
    }
    // ��������������� ������� ��� ��������� ������ 4D ���������� �������
    CMatrix GetVertexHomogeneous(int index) {
        return Vertices.GetCol(index); // �����������, ��� 4-� ������ ��� w=1
    }


public:
    CPyramid();
    // ������� �������� ��������� ��� ���������� �������� PSourceLight
    void ColorDraw(CDC& dc, CMatrix& PView_sph, CMatrix& PSourceLight_sph, CRect& RW, COLORREF color);
};

CPyramid::CPyramid()
{
    Vertices.RedimMatrix(4, 6); // ABC    � ������ ���������
    // A'B'C' � ������� ���������
// �������: 0-A, 1-B, 2-C, 3-A', 4-B', 5-C'
/*       A                   B                    C                   A'                  B'                  C'      */
    Vertices(0, 0) = 0; Vertices(0, 1) = 0;  Vertices(0, 2) = 3; Vertices(0, 3) = 0; Vertices(0, 4) = 0; Vertices(0, 5) = 1;  // X
    Vertices(1, 0) = 3; Vertices(1, 1) = 0;  Vertices(1, 2) = 0; Vertices(1, 3) = 1; Vertices(1, 4) = 0; Vertices(1, 5) = 0;  // Y
    Vertices(2, 0) = 0; Vertices(2, 1) = 0;  Vertices(2, 2) = 0; Vertices(2, 3) = 3; Vertices(2, 4) = 3; Vertices(2, 5) = 3;  // Z
    Vertices(3, 0) = 1; Vertices(3, 1) = 1;  Vertices(3, 2) = 1; Vertices(3, 3) = 1; Vertices(3, 4) = 1; Vertices(3, 5) = 1;  // W
}

void CPyramid::GetRect(CMatrix& Vert_view, CRectD& RectView)
{
    // ������� Vert_view ������ ��������� ������� ��� � ������������ ���� (��������� MV * Vertices_world_homogeneous)
    // � ������ ����� ��� ������� 2 ������ ��� ��������� X, Y ������������ ����.
    if (Vert_view.rows() < 2) return; // ��� ���������� ������

    CMatrix Vx = Vert_view.GetRow(0); // x - ���������� � ������������ ����
    double xMin = Vx.MinElement();
    double xMax = Vx.MaxElement();
    CMatrix Vy = Vert_view.GetRow(1); // y - ���������� � ������������ ����
    double yMin = Vy.MinElement();
    double yMax = Vy.MaxElement();
    RectView.SetRectD(xMin, yMax, xMax, yMin); // �������: left, top, right, bottom
}


void CPyramid::ColorDraw(CDC& dc, CMatrix& PView_sph, CMatrix& PSourceLight_sph, CRect& RW, COLORREF baseColor)
{
    BYTE R_base = GetRValue(baseColor);
    BYTE G_base = GetGValue(baseColor);
    BYTE B_base = GetBValue(baseColor);

    // 1. �������������� ���������
    CMatrix PView_cart = SphereToCart(PView_sph);     // ������� ������ � ���������� ������� �����������
    CMatrix PLight_cart = SphereToCart(PSourceLight_sph); // ������� ��������� ����� � ���������� ������� �����������

    CMatrix MV = CreateViewCoord(PView_sph(0), PView_sph(1), PView_sph(2)); // ������� ��� -> ���
    CMatrix Vertices_view_hom = MV * Vertices; // ��� ������� �������� � ������������ ���� (����������)

    CRectD Rect_view_bounds;
    GetRect(Vertices_view_hom, Rect_view_bounds); // �������� �������������� ������������� � XY ������������ ����
    CMatrix M_ViewToScreen = SpaceToWindow(Rect_view_bounds, RW); // ������� ��� -> �����

    // ���������� ����� ��������� ��������
   
    struct FaceInfo {
        int num_verts;
        int v_idx[4];
        CMatrix normal_world;
        CMatrix center_world;
        CPoint screen_points[4];
        COLORREF final_color;
        bool visible;
        double avg_z_view;

        // ����������� ��� FaceInfo
        FaceInfo()
            : normal_world(3, 1),
            center_world(3, 1),
            num_verts(0),
            visible(false),
            avg_z_view(0.0),
            final_color(RGB(0, 0, 0))
        {
            for (int i = 0; i < 4; ++i) {
                v_idx[i] = 0;
                screen_points[i].x = 0;
                screen_points[i].y = 0;
            }
        }
    };

    FaceInfo faces[5];

    // ������� ������� (3D �����)
    CMatrix vA = GetVertex(0), vB = GetVertex(1), vC = GetVertex(2);
    CMatrix vA1 = GetVertex(3), vB1 = GetVertex(4), vC1 = GetVertex(5);

    // ����� 0: ������ ��������� (A, C, B) ��� ������� ������ (���� A=(0,3,0), B=(0,0,0), C=(3,0,0))
    // (C-A) x (B-A) = (3,-3,0) x (0,-3,0) = (0,0,-9) -> ���������� �� -Z. ��������� ��� ������� ���������, ���� Z - ������.
    faces[0].num_verts = 3; faces[0].v_idx[0] = 0; faces[0].v_idx[1] = 2; faces[0].v_idx[2] = 1; // A, C, B
    faces[0].normal_world = Normalize(VectorMult(vC - vA, vB - vA));
    faces[0].center_world = (vA + vB + vC) * (1.0 / 3.0);

    // ����� 1: ������� ��������� (A', B', C') ��� ������� ������
    // (B1-A1) x (C1-A1) = (0,-1,0) x (1,-1,0) = (0,0,1) -> ���������� �� +Z. ��������� ��� �������� ���������.
    faces[1].num_verts = 3; faces[1].v_idx[0] = 3; faces[1].v_idx[1] = 4; faces[1].v_idx[2] = 5; // A', B', C'
    faces[1].normal_world = Normalize(VectorMult(vB1 - vA1, vC1 - vA1));
    faces[1].center_world = (vA1 + vB1 + vC1) * (1.0 / 3.0);

    // ������� ����� (����������������)
    // ����� 2: A, B, B', A'  (0, 1, 4, 3)
    // �������: (B-A) x (A1-A) = (0,-3,0) x (0,-2,3) = (-9,0,0) -> ���������� �� -X
    faces[2].num_verts = 4; faces[2].v_idx[0] = 0; faces[2].v_idx[1] = 1; faces[2].v_idx[2] = 4; faces[2].v_idx[3] = 3;
    faces[2].normal_world = Normalize(VectorMult(vB - vA, vA1 - vA));
    faces[2].center_world = (vA + vB + vB1 + vA1) * 0.25;

    // ����� 3: B, C, C', B'  (1, 2, 5, 4)
    // �������: (C-B) x (B1-B) = (3,0,0) x (0,0,3) = (0,-9,0) -> ���������� �� -Y
    faces[3].num_verts = 4; faces[3].v_idx[0] = 1; faces[3].v_idx[1] = 2; faces[3].v_idx[2] = 5; faces[3].v_idx[3] = 4;
    faces[3].normal_world = Normalize(VectorMult(vC - vB, vB1 - vB));
    faces[3].center_world = (vB + vC + vC1 + vB1) * 0.25;

    // ����� 4: C, A, A', C'  (2, 0, 3, 5)
    // �������: (A-C) x (C1-C) = (-3,3,0) x (-2,0,3) = (9,9,6) -> ������������� ���
    faces[4].num_verts = 4; faces[4].v_idx[0] = 2; faces[4].v_idx[1] = 0; faces[4].v_idx[2] = 3; faces[4].v_idx[3] = 5;
    faces[4].normal_world = Normalize(VectorMult(vA - vC, vC1 - vC));
    faces[4].center_world = (vC + vA + vA1 + vC1) * 0.25;


    // ������������ ������ �����: ���������, ���������, ��������
    for (int i = 0; i < 5; ++i) {
        // ��������� ��������� ������
        CMatrix ViewDirection_world = PView_cart - faces[i].center_world; // ������ �� ������ ����� � ������
        if (ScalarMult(faces[i].normal_world, ViewDirection_world) <= 1e-6) { // ������� ���������� �� ������ ��� ����� ������
            faces[i].visible = false;
            continue;
        }
        faces[i].visible = true;

        // ��������� ���������
        CMatrix LightDirection_world = PLight_cart - faces[i].center_world; // ������ �� ������ ����� � ��������� �����
        LightDirection_world = Normalize(LightDirection_world); // L ������ ���� ������������

        // N ��� ������������ ��� ����������� �����
        double diffuse_intensity = ScalarMult(faces[i].normal_world, LightDirection_world);
        diffuse_intensity = max(0.0, diffuse_intensity); // ������������ [0, ...)

        // ������� ��������� �������� ���������, ����� ������ ������� �� ���� ��������� �������
        double ambient_coeff = 0.2;
        double final_intensity = ambient_coeff + (1.0 - ambient_coeff) * diffuse_intensity;
        final_intensity = min(1.0, final_intensity); // ������������ [0,1]

        BYTE R_final = static_cast<BYTE>(R_base * final_intensity);
        BYTE G_final = static_cast<BYTE>(G_base * final_intensity);
        BYTE B_final = static_cast<BYTE>(B_base * final_intensity);
        faces[i].final_color = RGB(R_final, G_final, B_final);

        // ���������� ������� �� ����� � ��������� ������� Z � ������������ ���� ��� ��������� ���������
        faces[i].avg_z_view = 0;
        CMatrix P_view_temp(3); P_view_temp(2) = 1.0; // ��� M_ViewToScreen
        for (int j = 0; j < faces[i].num_verts; ++j) {
            int vdx = faces[i].v_idx[j];
            // Vertices_view_hom �������� X, Y, Z, W ���������� ����
            P_view_temp(0) = Vertices_view_hom(0, vdx);
            P_view_temp(1) = Vertices_view_hom(1, vdx);

            CMatrix P_screen = M_ViewToScreen * P_view_temp;
            faces[i].screen_points[j].x = static_cast<int>(P_screen(0));
            faces[i].screen_points[j].y = static_cast<int>(P_screen(1));

            // ��������� Z-���������� �� ������������ ����
            // Vertices_view_hom(2,vdx) ��� Z � ������������ ����.
            // ������ � CreateViewCoord ������� ����� ����� -Z ���. ������� Z �������� ������.
            faces[i].avg_z_view += Vertices_view_hom(2, vdx);
        }
        if (faces[i].num_verts > 0) {
            faces[i].avg_z_view /= faces[i].num_verts;
        }
    }

    // ������ �����
    for (int i = 0; i < 5; ++i) {
        if (faces[i].visible) {
            CPen pen(PS_SOLID, 1, RGB(30, 30, 30)); // �����-����� �������
            CBrush brush(faces[i].final_color);

            CPen* pOldPen = dc.SelectObject(&pen);
            CBrush* pOldBrush = dc.SelectObject(&brush);

            dc.Polygon(faces[i].screen_points, faces[i].num_verts);

            dc.SelectObject(pOldPen);
            dc.SelectObject(pOldBrush);
        }
    }
}