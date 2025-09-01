cmake_minimum_required(VERSION 3.28)
include_directories(${CMAKE_SOURCE_DIR}/LPVLib/include/)

add_library(LPVLib STATIC src/MyLib.c)