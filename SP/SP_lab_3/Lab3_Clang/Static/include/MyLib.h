#ifndef MYLIB_H
#define MYLIB_H

#define LIB_ARRAY_SIZE 1024

#include <stdio.h>

extern const int lib_array[LIB_ARRAY_SIZE];

int lpv_bsearch(int* a, int n, int x);

int lpv_bsearch_r(int* a, int x, int i, int j);

#endif