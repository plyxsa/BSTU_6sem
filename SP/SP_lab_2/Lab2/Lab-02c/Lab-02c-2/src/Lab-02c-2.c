#include <windows.h>
#include <stdio.h>

#define FILE_MAPPING_NAME L"Lab-02"
#define MUTEX_NAME L"Lab-02-Mutex"
#define VIEW_SIZE 65536   // 64  иЅ
#define ITERATIONS 10     // 10 итераций

void handle_error(const char* message) {
    fprintf(stderr, "%s (Error code: %lu)\n", message, GetLastError());
    exit(EXIT_FAILURE);
}

int main() {
    HANDLE hMap = NULL, hMutex = NULL;
    LPVOID pView = NULL;

    hMutex = OpenMutexW(SYNCHRONIZE, FALSE, MUTEX_NAME);
    if (hMutex == NULL) handle_error("Failed to open mutex");

    hMap = OpenFileMappingW(FILE_MAP_READ, FALSE, FILE_MAPPING_NAME);
    if (hMap == NULL) handle_error("Failed to open file mapping");

    pView = MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, VIEW_SIZE);
    if (pView == NULL) handle_error("Failed to map view");

    printf("Reader: Memory mapped successfully\n");

    for (int i = 0; i < ITERATIONS; i++) {
        WaitForSingleObject(hMutex, INFINITE);

        printf("Reader: Iteration %d - Data read: ", i + 1);
        for (size_t j = 0; j < VIEW_SIZE / sizeof(int); j++) {
            printf("%d ", ((int*)pView)[j]);
        }
        printf("\n");

        ReleaseMutex(hMutex);
        getchar();
    }

    UnmapViewOfFile(pView);
    CloseHandle(hMap);
    CloseHandle(hMutex);

    printf("Reader: Finished\n");
    printf("Press enter to continue... ");
    getchar();

    return 0;
}
