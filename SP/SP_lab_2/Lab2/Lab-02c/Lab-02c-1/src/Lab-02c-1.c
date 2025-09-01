#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

#define FILE_MAPPING_NAME L"Lab-02"
#define MUTEX_NAME L"Lab-02-Mutex"
#define VIEW_SIZE 65536   // 64  иЅ
#define ITERATIONS 10     // 10 итераций

void handle_error(const char* message) {
    fprintf(stderr, "%s (Error code: %lu)\n", message, GetLastError());
    exit(EXIT_FAILURE);
}

void start_reader_process() {
    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi;

    if (!CreateProcessW(L"Lab-02c-2.exe", NULL, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, NULL, &si, &pi)) {
        handle_error("Failed to start Lab-02c-2.exe");
    }

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    printf("Writer: Reader process started successfully\n");
}

int main() {
    HANDLE hMap = NULL, hMutex = NULL;
    LPVOID pView = NULL;

    hMutex = CreateMutexW(NULL, FALSE, MUTEX_NAME);
    if (hMutex == NULL) handle_error("Failed to create mutex");

    hMap = CreateFileMappingW(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, VIEW_SIZE, FILE_MAPPING_NAME);
    if (hMap == NULL) handle_error("Failed to create file mapping");

    pView = MapViewOfFile(hMap, FILE_MAP_WRITE, 0, 0, VIEW_SIZE);
    if (pView == NULL) handle_error("Failed to map view");

    printf("Writer: Memory mapped successfully\n");

    //start_reader_process();

    for (int i = 0; i < ITERATIONS; i++) {
        WaitForSingleObject(hMutex, INFINITE);

        for (size_t j = 0; j < VIEW_SIZE / sizeof(int); j++) {
            ((int*)pView)[j] = rand() % 1000;
        }

        printf("Writer: Iteration %d - Data written\n", i + 1);
        ReleaseMutex(hMutex);
        getchar();
    }

    UnmapViewOfFile(pView);
    CloseHandle(hMap);
    CloseHandle(hMutex);

    printf("Writer: Finished\n");
    printf("Press enter to continue... ");
    getchar();

    return 0;
}
