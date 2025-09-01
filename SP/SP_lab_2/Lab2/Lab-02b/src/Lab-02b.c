#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#define FILE_SIZE ((size_t)536870912)   // 512 MiB
#define VIEW_OFFSET ((size_t)0)         // Смещение
#define VIEW_SIZE ((size_t)134217728)   // 128 MiB

void handle_error(const char* message) {
    fprintf(stderr, "%s (Error code: %lu)\n", message, GetLastError());
    exit(EXIT_FAILURE);
}

void generate_random_text(char* buffer, size_t size) {
    const char charset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (size_t i = 0; i < size; i++) {
        buffer[i] = charset[rand() % (sizeof(charset) - 1)];
    }
}

int main() {
    HANDLE hFile = INVALID_HANDLE_VALUE;
    HANDLE hMap = NULL;
    LPVOID pView = NULL;
    char* buffer = (char*)malloc(FILE_SIZE);
    if (!buffer) handle_error("Memory allocation failed");

    srand((unsigned int)time(NULL));
    generate_random_text(buffer, FILE_SIZE);

    hFile = CreateFileW(L"fileb.txt", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) {
        free(buffer);
        handle_error("Failed to create file");
    }

    DWORD bytesWritten;
    if (!WriteFile(hFile, buffer, FILE_SIZE, &bytesWritten, NULL)) {
        CloseHandle(hFile);
        free(buffer);
        handle_error("Failed to write random data to file");
    }
    free(buffer);

    printf("\nFile created. Press enter to continue... ");
    getchar();

    hMap = CreateFileMappingW(hFile, NULL, PAGE_READWRITE, 0, FILE_SIZE, L"Lab2bFileMapping");
    if (hMap == NULL) {
        CloseHandle(hFile);
        handle_error("Failed to create file mapping");
    }

    pView = MapViewOfFile(hMap, FILE_MAP_READ, 0, VIEW_OFFSET, VIEW_SIZE);
    if (pView == NULL) {
        CloseHandle(hMap);
        CloseHandle(hFile);
        handle_error("Failed to map view for reading");
    }

    printf("File is mapped for reading\n");
    printf("Contents at offset %zu (size %zu):\n", VIEW_OFFSET, VIEW_SIZE);

    fwrite(pView, 1, VIEW_SIZE, stdout);

    printf("\nPress enter to continue... ");
    getchar();

    UnmapViewOfFile(pView);
    CloseHandle(hMap);
    CloseHandle(hFile);

    printf("File mapping is ended\n");
    printf("Press enter to exit... ");
    getchar();

    return 0;
}