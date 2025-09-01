cmake_minimum_required(VERSION 3.28)

include_directories(${CMAKE_SOURCE_DIR}/LibLPVd/include)
add_library(LibLPVd SHARED src/LibLPVd.c)
