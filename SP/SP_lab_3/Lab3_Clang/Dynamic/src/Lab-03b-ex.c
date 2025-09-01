#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>

typedef int (*BSearchFunc)(int*, int, int);
typedef int (*BSearchRecursiveFunc)(int*, int, int, int);
typedef const int* LibArray;

void printUsage() {
    printf("Usage: .\\Lab-03b-Explicit.exe <Library> <Function|Number> [Number]\n");
    printf("Available libraries: LPVDLib.dll, LPVDLib++.dll\n");
    printf("Available functions:\n");
    printf("  1 or iterative_bsearch - iterative binary search\n");
    printf("  2 or recursive_bsearch - recursive binary search\n");
}

int main(int argc, char* argv[]) {
    if (argc < 3) {
        if (argc < 2) {
            printf("The downloadable library is not specified!\n");
        }
        else {
            printf("The function being called is not specified!\n");
        }
        printUsage();
        return 1;
    }

    char* libName = argv[1];
    char* funcName = argv[2];

    HMODULE hLib = LoadLibrary(libName);
    if (!hLib) {
        printf("The downloadable library was not found: %s (Error: %d)\n", libName, GetLastError());
        return 1;
    }

    BSearchFunc iterativeSearch = NULL;
    BSearchRecursiveFunc recursiveSearch = NULL;

    if (strcmp(libName, "LPVDLib.dll") == 0) {
        iterativeSearch = (BSearchFunc)GetProcAddress(hLib, "iterative_bsearch");  
        recursiveSearch = (BSearchRecursiveFunc)GetProcAddress(hLib, "lpv_bsearch_r"); 
    }
    else if (strcmp(libName, "LPVDLib++.dll") == 0) {
        iterativeSearch = (BSearchFunc)GetProcAddress(hLib, "iterative_bsearch"); 
        recursiveSearch = (BSearchRecursiveFunc)GetProcAddress(hLib, "?lpv_bsearch_r@@YAHPEAHHHH@Z"); 
    }

    if ((strcmp(funcName, "iterative_bsearch") == 0 || strcmp(funcName, "1") == 0) && !iterativeSearch) {
        printf("The iterative search function was not found in the library!\n");
        FreeLibrary(hLib);
        return 1;
    }
    
    if ((strcmp(funcName, "recursive_bsearch") == 0 || strcmp(funcName, "2") == 0) && !recursiveSearch) {
        printf("The recursive search function was not found in the library!\n");
        FreeLibrary(hLib);
        return 1;
    }

    LibArray lib_array = (LibArray)GetProcAddress(hLib, "lib_array");
    if (!lib_array) {
        printf("Error retrieving the array from the library! (Error: %d)\n", GetLastError());
        FreeLibrary(hLib);
        return 1;
    }

    int target;
    if (argc < 4) {
        printf("Enter a number to search for: ");
        if (scanf("%d", &target) != 1) {
            printf("Error entering the number!\n");
            FreeLibrary(hLib);
            return 1;
        }
    }
    else {
        target = atoi(argv[3]);
    }

    int position = -1;

    if (strcmp(funcName, "iterative_bsearch") == 0 || strcmp(funcName, "1") == 0) {
        position = iterativeSearch((int*)lib_array, 1024, target);
    }
    else if (strcmp(funcName, "recursive_bsearch") == 0 || strcmp(funcName, "2") == 0) {
        position = recursiveSearch((int*)lib_array, target, 0, 1023);
    }
    else {
        printf("The required %s function is not supported!\n", funcName);
        FreeLibrary(hLib);
        return 1;
    }

    if (position == -1) {
        printf("%s: The specified number was not found!\n", funcName);
    }
    else {
        printf("%s: The number %d was found at position %d!\n", funcName, target, position);
    }

    FreeLibrary(hLib);
    return 0;
}