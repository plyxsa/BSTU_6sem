#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

#define VIEW_OFFSET_1 ((size_t)65536)      // Смещение 64 KB
#define VIEW_SIZE_1 ((size_t)4096)         // 4 KB для чтения
#define VIEW_OFFSET_2 ((size_t)131072)     // Смещение 128 KB
#define VIEW_SIZE_2 ((size_t)4096)         // 4 KB для записи
#define FILE_SIZE ((size_t)262144)         // 256 KB для файла


void handle_error(const char* message) {
    fprintf(stderr, "%s (Error code: %lu)\n", message, GetLastError());
    exit(EXIT_FAILURE);
}

void generate_random_text(char* buffer, size_t size) {
    const char charset[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (size_t i = 0; i < size - 1; i++) {
        buffer[i] = charset[rand() % (sizeof(charset) - 1)];
    }
    buffer[size - 1] = '\0';
}

int main() {
    HANDLE hFile = INVALID_HANDLE_VALUE;
    HANDLE hMap = NULL;
    LPVOID pView = NULL;
    char randomText[FILE_SIZE];

    srand((unsigned int)time(NULL));

    hFile = CreateFileW(L"file.txt", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) handle_error("Failed to create file");

    generate_random_text(randomText, FILE_SIZE);
    DWORD bytesWritten;
    if (!WriteFile(hFile, randomText, FILE_SIZE, &bytesWritten, NULL)) {
        CloseHandle(hFile);
        handle_error("Failed to write random data to file");
    }

    hMap = CreateFileMappingW(hFile, NULL, PAGE_READWRITE, 0, FILE_SIZE, L"MyFileMapping");
    if (hMap == NULL) {
        CloseHandle(hFile);
        handle_error("Failed to create file mapping");
    }
    printf("File mapping created successfully\n");

    pView = MapViewOfFile(hMap, FILE_MAP_READ, 0, VIEW_OFFSET_1, VIEW_SIZE_1);
    if (pView == NULL) {
        CloseHandle(hMap);
        CloseHandle(hFile);
        handle_error("Failed to map view for reading");
    }

    printf("File is mapped for reading\n");
    printf("Contents at offset %zu (size %zu):\n", VIEW_OFFSET_1, VIEW_SIZE_1);

    for (size_t i = 0; i < VIEW_SIZE_1; i++) {
        putchar(((char*)pView)[i]);
    }
    printf("\n");

    printf("Press enter to continue... ");
    getchar();

    UnmapViewOfFile(pView);

    pView = MapViewOfFile(hMap, FILE_MAP_WRITE, 0, VIEW_OFFSET_2, VIEW_SIZE_2);
    if (pView == NULL) {
        CloseHandle(hMap);
        CloseHandle(hFile);
        handle_error("Failed to map view for writing");
    }

    printf("File is mapped for writing\n");

    memset(pView, '\0', VIEW_SIZE_2);

    printf("Press enter to continue... ");
    getchar();

    FlushViewOfFile(pView, VIEW_SIZE_2);
    UnmapViewOfFile(pView);

    CloseHandle(hMap);
    CloseHandle(hFile);

    printf("File mapping is ended\n");
    printf("Press enter to continue... ");
    getchar();

    return 0;
}