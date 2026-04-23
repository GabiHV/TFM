cmake_minimum_required(VERSION 3.10)
project(apriltag_unity_wrapper C)

add_library(apriltag_wrapper SHARED
    apriltag_unity_wrapper.c
)

# Ruta hacia la librería estática de AprilTag
target_link_libraries(apriltag_wrapper
    ${CMAKE_CURRENT_LIST_DIR}/lib/libapriltag.a
)

# Asegurar visibilidad
target_include_directories(apriltag_wrapper PUBLIC
    ${CMAKE_CURRENT_LIST_DIR}/apriltag
)