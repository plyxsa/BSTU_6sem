cmake_minimum_required(VERSION 3.28)

include_directories(${CMAKE_SOURCE_DIR}/LPVDLib/include)
add_executable(Lab-03b-Implicit WIN32 src/Lab-03b-im.c)
target_link_libraries(Lab-03b-Implicit LPVDLib)