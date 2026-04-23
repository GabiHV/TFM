import rclpy
import datetime as dt
from rclpy.node import Node
from std_srvs.srv import SetBool

class TopicListService():
    def __init__(self, node: Node):
        self._node = node
        node.create_service(SetBool, '/get_topics', self.callback)

    def callback(self, request, response):

        topics = self._node.get_topic_names_and_types()
        text = ""
        for name, message_type in topics:
            text += f"{name}:{message_type}\n"
        
        response.success = True
        response.message = text
        print(f"{dt.datetime.now()} >> {TopicListService.__name__}:\n\t{request}\n\t{response}")

        return response