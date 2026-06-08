import rclpy
import datetime as dt
from rclpy.node import Node
from .topic_list_service import TopicListService
from .node_list_service import NodeListService
from .robot_list_service import RobotListService
from .goal_path_adapter import GoalPathAdapter

class UnityAppService(Node):
    def __init__(self):
        super().__init__('UnityAppService')
        self.setup_node()

    def setup_node(self):
        TopicListService(self)
        NodeListService(self)
        RobotListService(self)
        GoalPathAdapter(self)

def main():
    rclpy.init()
    node = UnityAppService()
    rclpy.spin(node)
    rclpy.shutdown()

if __name__ == '__main__':
    main()
