### IMPORTANT: 
A src/micro_ros_setup/config/android/generic/create.sh MODIFICATION IS NEEDED IN ORDER TO PROTECT INCORRECT FILE CREATION WITH mcu_ws/src/rcl_logging/rcl_logging_log4cxx/COLCON_IGNORE and 
mcu_ws/ros2_tracing/test_tracetools/COLCON_IGNORE

REPLACE src/micro_ros_setup/config/android/generic/create.sh: 
```bash
touch mcu_ws/src/rcl_logging/rcl_logging_log4cxx/COLCON_IGNORE
touch mcu_ws/ros2_tracing/test_tracetools/COLCON_IGNORE
```
WITH:
```bash
if [ -d "mcu_ws/src/rcl_logging/rcl_logging_log4cxx" ]; then
    touch mcu_ws/src/rcl_logging/rcl_logging_log4cxx/COLCON_IGNORE
fi
if [ -d "mcu_ws/ros2_tracing/test_tracetools/COLCON_IGNORE" ]; then
    touch mcu_ws/ros2_tracing/test_tracetools/COLCON_IGNORE
fi
```


ADDITIONALY, micro_ros_demos_rclc PACKAGE WILL BE SKIPPED (CANNOT BE BUILT IN ANDROID) in src/micro_ros_setup/config/android/generic/build.sh
ADD in `colcon build`:
```bash
--packages-skip micro_ros_demos_rclc \
```
