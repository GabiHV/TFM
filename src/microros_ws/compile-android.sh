#!/bin/bash

### IMPORTANT: A SRC/MICRO_ROS_SETUP/CONFIG/ANDROID/GENERIC/CREATE.SH MODIFICATION IS NEEDED IN ORDER TO 
### PROTECT INCORRECT FILE CREATION WITH mcu_ws/src/rcl_logging/rcl_logging_log4cxx/COLCON_IGNORE and 
### mcu_ws/ros2_tracing/test_tracetools/COLCON_IGNORE
### ADDITIONALY, micro_ros_demos_rclc PACKAGE WILL BE SKIPPED (CANNOT BE BUILT IN ANDROID) in src/micro_ros_setup/config/android/generic/build.sh


export ANDROID_ABI=arm64-v8a
export ANDROID_PLATFORM=android-21
export ANDROID_NDK=~/Android/Sdk/ndk/30.0.14904198

vcs src/micro-ros.repos

# Build micro-ROS and sourcing
colcon build --package-select micro_ros_setup
source install/setup.bash

# Create firmware
rm -rf firmware # Delete previous firmware
ros2 run micro_ros_setup create_firmware_ws.sh android

# Configure firmware (optional in jazzy)
ros2 run micro_ros_setup configure_firmware.sh android \
  --android-ndk $ANDROID_NDK \
  --android-abi $ANDROID_ABI \
  --android-platform $ANDROID_PLATFORM

# Build firmware
rm -rf firmware/mcu_ws/build \
       firmware/mcu_ws/install \
       firmware/mcu_ws/log \
       firmware/build
ros2 run micro_ros_setup build_firmware.sh android 