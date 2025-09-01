cmake_minimum_required(VERSION 3.30)
project(Lab-01)

if (WIN32)
    message("Hello, I am Windows!")
elseif (APPLE)
    message("Hello, I am macOS!")
elseif (UNIX)
    message("Hello, I am Linux!")
endif()

add_subdirectory(Lab-01C)

if (WIN32)
    add_subdirectory(Lab-01CS)
endif()