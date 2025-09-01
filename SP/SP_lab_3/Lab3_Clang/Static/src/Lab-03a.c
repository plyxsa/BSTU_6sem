#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "MyLib.h"

int main(int argc, char* argv[]) {
   
    if (argc < 2) {
        printf("The function being called is not specified\n");
        return 1;
    }

    char* function_name = argv[1];
    int searchVal = -1;

    if (argc > 2) {
        searchVal = atoi(argv[2]);
    }
    else {
        printf("Enter the desired number: ");
        if (scanf("%d", &searchVal) != 1) {
            printf("Invalid input. Expected a number.\n");
            return 1;
        }
    }

    int index = -1;

    if (strcmp(function_name, "bsearch") == 0) {
        index = lpv_bsearch(lib_array, LIB_ARRAY_SIZE - 1, searchVal);
    }
    else if (strcmp(function_name, "bsearch_r") == 0) {
        index = lpv_bsearch_r(lib_array, searchVal, 0, LIB_ARRAY_SIZE - 1);
    }
    else {
        printf("Unknown function: %s\n", function_name);
        return 1;
    }

    if (index == -1) {
        printf("%s: The specified number was not found\n", function_name);
    }
    else {
        printf("%s: The number %d found in the position %d\n", function_name, searchVal, index);
    }

    return 0;
}
