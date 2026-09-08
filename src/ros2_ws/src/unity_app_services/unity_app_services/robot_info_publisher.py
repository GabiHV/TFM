import rclpy
from rclpy.node import Node
from unity_server_interfaces.msg import Robot, RobotArray

ROLE_MAP = {
    "cmd_vel": ["cmd_vel", "cmdvel", "velocity"],
    "odom": ["odom", "odometry"],
    "scan": ["scan", "laser"],
    "tf": ["tf"]
}

class RobotInfoPublisher():
    def __init__(self, node: Node):
        self._node = node
        
        self.publisher = self._node.create_publisher(RobotArray, '/get_robots_unity', 10)
        self._node.create_timer(2, self.callback)

    def callback(self):
        array_robot = RobotArray()

        topics = self._node.get_topic_names_and_types()
        self.clusters = {}

        for topic, _ in topics:
            key = self.extract_group_key(topic)

            if key not in self.clusters:
                self.clusters[key] = {
                    "topics": [],
                    "roles": set()
                }

            self.clusters[key]["topics"].append(topic)

            role = self.detect_role(topic)
            if role:
                self.clusters[key]["roles"].add(role)

        for key, data in self.clusters.items():
            if not self.is_robot(data):
                continue

            robot = Robot()
            robot.id = key
            robot.topics = data["topics"]
            robot.confidence = self.score_cluster(data)
            array_robot.robots.append(robot)

        self.publisher.publish(array_robot)
    
    def detect_role(self, topic):
        for role, patterns in ROLE_MAP.items():
            if any(p in topic.lower() for p in patterns):
                return role
        return None
    
    def extract_group_key(self, topic):
        t = topic.lower().strip("/")

        # if exists namespace
        if "/" in t:
            return t.split("/")[0]

        # if fixed prefix such as robot1_cmd_vel
        for suffix in ["cmd_vel", "odom", "scan"]:
            if t.endswith(suffix):
                prefix = t[: -len(suffix)].rstrip("_")
                return prefix if prefix else "default"

        # if there is no namespaces and fixed prefix (i.e. common middlewares or one logic entity -a.k.a robot-)
        return "default"
    
    def is_robot(self, cluster):
        roles = cluster["roles"]
        return (
            "cmd_vel" in roles and
            ("odom" in roles or "tf" in roles)
        )
    
    def score_cluster(self, cluster):
        score = 0
        roles = cluster["roles"]

        if "cmd_vel" in roles:
            score += 3
        if "odom" in roles:
            score += 3
        if "tf" in roles:
            score += 2
        if len(cluster["topics"]) > 3:
            score += 1

        return score

    
def main():
    rclpy.init()
    clusterer = RobotListService()

    clusters = clusterer.compute_clusters()
    for cluster in clusters:
        clusterer.get_logger().info(str(cluster))

    rclpy.shutdown()

if __name__ == "__main__":
    main()