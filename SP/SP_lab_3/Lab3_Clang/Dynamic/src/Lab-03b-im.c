#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>

__declspec(dllimport) int iterative_bsearch(int*, int, int);
__declspec(dllimport) int lpv_bsearch_r(int*, int, int, int);
__declspec(dllimport) const int lib_array[1024];

void print_usage() {
    printf("Usage: Lab-03b-im <Function Name> [Target Number]\n");
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        printf("Error: No function specified!\n");
        print_usage();
        return 1;
    }

    char* function_name = argv[1];

    int target = 0;
    if (argc < 3) {
        printf("Enter the number to search for: ");
        if (scanf_s("%d", &target) != 1) {
            printf("Error: Invalid input for number! Please enter a valid integer.\n");
            return 1;
        }
    }
    else {
        char* endptr;
        target = strtol(argv[2], &endptr, 10);
        if (*endptr != '\0') { 
            printf("Error: Invalid target number format! Please provide a valid integer.\n");
            return 1;
        }
    }

    int position = -1;

    if (strcmp(function_name, "iterative_bsearch") == 0) {
        position = iterative_bsearch((int*)lib_array, 1024, target);
    }
    else if (strcmp(function_name, "bsearch_r") == 0) {
        position = lpv_bsearch_r((int*)lib_array, target, 0, 1023);
    }
    else {
        printf("Error: Function '%s' not found in the library!\n", function_name);
        print_usage();
        return 1;
    }

    if (position == -1) {
        printf("%s: The number %d was not found in the array!\n", function_name, target);
    }
    else {
        printf("%s: The number %d was found at position %d!\n", function_name, target, position);
    }

    return 0;
}
