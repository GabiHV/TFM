from launch import LaunchDescription
from launch_ros.actions import Node


def generate_launch_description():
    return LaunchDescription(
        [
            Node(
                package="unity_app_services",
                executable="default_server_endpoint",
                emulate_tty=True,
            )
        ]
    )
