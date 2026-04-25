#!/bin/bash
set -e  # Exit on error

# Source ROS2 distribution setup (adjust distro as needed)
distro=$ROS_DISTRO
source /opt/ros/$distro/setup.bash  # Or your ROS2 distro

# Source workspace setup
current_dir=$(dirname "$0")
pwd=$(realpath "$current_dir")
source $pwd/install/setup.bash

# Run nodes with proper error handling and logging
{
    ros2 run ros_tcp_endpoint default_server_endpoint
} &
SERVER_PID=$!

{
    ros2 run unity_app_services default_server_endpoint
} &
CLIENT_PID=$!

# Trap for cleanup on script exit
trap "kill $SERVER_PID $CLIENT_PID 2>/dev/null" EXIT

# Keep script alive
wait