import rclpy
import datetime as dt
from rclpy.node import Node
from unity_server_interfaces.msg import TopicInfoArray, TopicInfo

class TopicInfoPublisher():
    def __init__(self, node: Node):
        self._node = node

        self.publisher = self._node.create_publisher(TopicInfoArray, '/get_topics_unity', 10)
        self._node.create_timer(5, self.callback)

    def callback(self):
        topic_array = TopicInfoArray()

        topics = self._node.get_topic_names_and_types()
        for name, message_type in topics:
            topic_info = TopicInfo()
            topic_info.name = name
            topic_info.types = message_type

            topic_array.topics.append(topic_info)
        
        self.publisher.publish(topic_array)