import rclpy
import datetime as dt
from rclpy.node import Node
from std_srvs.srv import SetBool

class NodeListService():
    def __init__(self, node: Node):
        self._node = node
        node.create_service(SetBool, '/get_nodes', self.callback)

    def callback(self, request, response):
        nodes = self._node.get_node_names()
        text = ""
        for nodeInfo in nodes:
            if (nodeInfo.startswith("_")):
                continue
            text += f"{nodeInfo}\n"

        response.success = True
        response.message = text

        print(f"{dt.datetime.now()} >> {NodeListService.__name__}:\n\t{request}\n\t{response}")
        return response

