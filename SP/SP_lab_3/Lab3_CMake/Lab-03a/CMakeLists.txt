cmake_minimum_required(VERSION 3.28)

include_directories(${PROJECT_SOURCE_DIR}/LPVLib/include)
add_executable(Lab-03a src/Lab-03a.c)
target_link_libraries(Lab-03a LPVLib)