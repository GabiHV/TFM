cmake_minimum_required(VERSION 3.8)
project(unity_server_interfaces)

find_package(ament_cmake REQUIRED)
find_package(rosidl_default_generators REQUIRED)
find_package(geometry_msgs REQUIRED)

rosidl_generate_interfaces(${PROJECT_NAME}
  "msg/Robot.msg"
  "msg/RobotArray.msg"
  "msg/NodeInfo.msg"
  "msg/NodeInfoArray.msg"
  "msg/TopicInfo.msg"
  "msg/TopicInfoArray.msg"
  "msg/TFStaticArray.msg"
  "msg/TFStatic.msg"
  DEPENDENCIES geometry_msgs
)

ament_export_dependencies(rosidl_default_runtime)

ament_package()