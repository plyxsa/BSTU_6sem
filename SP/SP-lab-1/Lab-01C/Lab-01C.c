#include <stdio.h>

int main() {
    double x;
    printf("Type the number x: ");
    scanf("%lf", &x);

    double x2 = x * x;
    double x4 = x2 * x2;
    double x8 = x4 * x4;
    double x16 = x8 * x8;
    double x17 = x16 * x;
    double x5 = x4 * x;

    printf("x^2  = %.6f\n", x2);
    printf("x^5  = %.6f\n", x5);
    printf("x^17 = %.6f\n", x17);

    return 0;
}
