import rclpy
import datetime as dt
from rclpy.node import Node
from .topic_linfo_publisher import TopicInfoPublisher
from .node_info_publisher import NodeInfoPublisher
from .robot_info_publisher import RobotInfoPublisher
from .tf_static_publisher import TFStaticCollector

class UnityAppService(Node):
    def __init__(self):
        super().__init__('UnityAppService')
        self.setup_node()

    def setup_node(self):
        TFStaticCollector(self)

        TopicInfoPublisher(self)
        NodeInfoPublisher(self)
        RobotInfoPublisher(self)

def main():
    rclpy.init()
    node = UnityAppService()
    rclpy.spin(node)
    rclpy.shutdown()

if __name__ == '__main__':
    main()
