// flock_example.c
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/file.h> // ��� flock
#include <string.h>

const char* FILENAME = "shared_flock_file.txt";

void lock_and_write_flock(int process_id) {
    FILE* fp; // ���������� ��������� ����-����� ��� �������� � flock
    char buffer[100];

    fp = fopen(FILENAME, "a+"); // ��������� ��� ���������� � ������/������, ������� ���� ���
    if (fp == NULL) {
        perror("fopen");
        exit(EXIT_FAILURE);
    }
    int fd = fileno(fp); // �������� �������� ���������� �� FILE*

    printf("Process %d: Trying to get exclusive flock...\n", process_id);

    // �������� ������������� ���� (����������� �����)
    if (flock(fd, LOCK_EX) == -1) { // LOCK_EX ��� ������������ ����������
        perror("flock LOCK_EX");
        fclose(fp);
        exit(EXIT_FAILURE);
    }

    printf("Process %d: Got the flock. Writing...\n", process_id);
    sprintf(buffer, "Data from process %d using flock!\n", process_id);
    fprintf(fp, "%s", buffer);
    fflush(fp); // ��������, ��� ������ ��������
    sleep(3);

    // ����������� ����������
    if (flock(fd, LOCK_UN) == -1) {
        perror("flock LOCK_UN");
    }
    printf("Process %d: Released the flock.\n", process_id);

    fclose(fp);
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        fprintf(stderr, "Usage: %s <process_id_number>\n", argv[0]);
        return 1;
    }
    int id = atoi(argv[1]);
    lock_and_write_flock(id);
    return 0;
}
// ����������: gcc flock_example.c -o flock_example
// ������ �� ���� ������ ����������:
// �������� 1: ./flock_example 1
// �������� 2: ./flock_example 2