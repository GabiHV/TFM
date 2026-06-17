import rclpy
import datetime as dt
from rclpy.node import Node
from unity_server_interfaces.msg import NodeInfo, NodeInfoArray

class NodeInfoPublisher():
    def __init__(self, node: Node):
        self._node = node

        self.publisher = self._node.create_publisher(NodeInfoArray, '/get_nodes_unity', 10)
        self._node.create_timer(2, self.callback)

    def callback(self):
        node_array = NodeInfoArray()
        nodes = self._node.get_node_names()
        for node_info in nodes:
            if (node_info.startswith("_")):
                continue
            node_obj = NodeInfo()
            node_obj.name = node_info
            node_array.nodes.append(node_obj)

        self.publisher.publish(node_array)