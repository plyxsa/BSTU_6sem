#ifndef LIB_LPV_D_H
#define LIB_LPV_D_H

#define LIB_ARRAY_SIZE 1024

extern const int lib_array[LIB_ARRAY_SIZE];

int lpv_bsearch(int* a, int n, int x);
int lpv_bsearch_r(int* a, int x, int i, int j);

void __attribute__((constructor)) library_init(void);
void __attribute__((destructor)) library_cleanup(void);

#endif
