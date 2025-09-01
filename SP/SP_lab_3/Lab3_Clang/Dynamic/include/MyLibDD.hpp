#ifndef MYLIBDD_PP_H
#define MYLIBDD_PP_H

#define LIB_ARRAY_SIZE 1024

#ifdef _WIN32
    #define DLL_EXPORT __declspec(dllexport)
#else
    #define DLL_EXPORT
#endif

#ifdef __cplusplus
extern "C" {
#endif

DLL_EXPORT extern const int lib_array[LIB_ARRAY_SIZE];
DLL_EXPORT int lpv_bsearch(int* a, int n, int x);

#ifdef __cplusplus
}

DLL_EXPORT int lpv_bsearch_r(int* a, int x, int i, int j);

#endif
#endif