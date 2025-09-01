#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <string.h>

int my_bsearch(int *a, int n, int x);
int my_bsearch_r(int *a, int x, int i, int j);

int main(int argc, char *argv[]) {
    if (argc < 3) {
        fprintf(stderr, "No library or function specified!\n");
        return 1;
    }

    const char *lib_name = argv[1];
    const char *func_name = argv[2];

    void *handle = dlopen(lib_name, RTLD_LAZY);
    if (!handle) {
        fprintf(stderr, "Failed to load library: %s\n", dlerror());
        return 1;
    }

    int x;
    if (argc == 3) {
        printf("Enter the number to search for: ");
        if (scanf("%d", &x) != 1) {
            fprintf(stderr, "Error reading number!\n");
            dlclose(handle);
            return 1;
        }
    } else {
        x = atoi(argv[3]);
    }

    const int *lib_array = dlsym(handle, "lib_array");
    if (!lib_array) {
        fprintf(stderr, "Failed to find array in the library %s: %s\n", lib_name, dlerror());
        dlclose(handle);
        return 1;
    }

    int array_size = 1024;

    int position = -1;

    if (strcmp(func_name, "lpv_bsearch") == 0) {
        int (*search_function_i)(int *, int, int) = (int (*)(int *, int, int))dlsym(handle, func_name);
        if (!search_function_i) {
            fprintf(stderr, "Function %s not found in the library %s: %s\n", func_name, lib_name, dlerror());
            dlclose(handle);
            return 1;
        }
        position = search_function_i(lib_array, array_size, x);
    } 
    else if (strcmp(func_name, "lpv_bsearch_r") == 0) {
        int (*search_function_r)(int *, int, int, int) = (int (*)(int *, int, int, int))dlsym(handle, func_name);
        if (!search_function_r) {
            fprintf(stderr, "Function %s not found in the library %s: %s\n", func_name, lib_name, dlerror());
            dlclose(handle);
            return 1;
        }
        position = search_function_r(lib_array, x, 0, array_size - 1);
    } 
    else {
        fprintf(stderr, "Invalid function name: %s\n", func_name);
        dlclose(handle);
        return 1;
    }

    if (position == -1) {
        printf("%s: The number was not found!\n", func_name);
    } else {
        printf("%s: Number %d found at position %d!\n", func_name, x, position);
    }

    dlclose(handle);

    return 0;
}
