cmake_minimum_required(VERSION 3.28)

include_directories(${CMAKE_SOURCE_DIR}/LPVDLib/include)
add_library(LPVDLib SHARED src/MyLibD.c)

target_link_options(LPVDLib PRIVATE "/DEF:${CMAKE_SOURCE_DIR}/LPVDLib/src/MyLibD.def")
