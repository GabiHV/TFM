import rclpy
from rclpy.node import Node
from unity_server_interfaces.srv import RobotListSrv

class RobotListService():
    def __init__(self, node: Node):
        self._node = node
        self._node.create_service(RobotListSrv, '/get_robots', self.robot_list_callback)
        
    def robot_list_callback(self, request, response):
        node_names = self._node.get_node_names_and_namespaces()
        namespaces = set()
        for ns, _ in node_names:
            if not ns.startswith("_"):
                namespaces.add(ns)
        response.robot_names = list(namespaces)

        return response
    
def main():
    rclpy.init()
    clusterer = RobotListService()

    clusters = clusterer.compute_clusters()
    for cluster in clusters:
        clusterer.get_logger().info(str(cluster))

    rclpy.shutdown()

if __name__ == "__main__":
    main()