import rclpy
from rclpy.node import Node
from geometry_msgs.msg import PoseStamped
from nav2_msgs.action import NavigateToPose
from rclpy.action import ActionClient
import subprocess

class GoalPathAdapter():
    def __init__(self, node: Node):
        self._node = node
        self.clients = {}
        self.subscribers = {}

        self.detect_robots()

        self._node.create_timer(3.0, self.detect_robots)

    def detect_robots(self):

        actions = self.get_actions_and_types()

        for action_name, action_type in actions:

            if not action_type:
                continue

            if not self.action_has_pose_stamped(action_type):
                continue

            robot_ns = self.extract_namespace(action_name)
            if robot_ns in self.clients:
                continue

            self._node.get_logger().info(f"🤖 Robot detected: {robot_ns or '[root]'}")
            self._node.get_logger().info(f"   ↳ Action: {action_name}")
            self._node.get_logger().info(f"   ↳ Type: {action_type}")

            client = ActionClient(self._node, NavigateToPose, action_name)
            self.clients[robot_ns] = client

            topic_name = f"{robot_ns}/goal_pose".replace("//", "/")

            sub = self._node.create_subscription(
                PoseStamped,
                topic_name,
                lambda msg, ns=robot_ns: self.goal_callback(msg, ns),
                10
            )

            self.subscribers[robot_ns] = sub

            self._node.get_logger().info(f"📡 Listening in topic: {topic_name}")
        
        if not self.clients:
            self._node.get_logger().warn("No robots were found with NavigateToPose")
        else:
            self._node.get_logger().info(f"Detected robots: {list(self.clients.keys())}")
    
    def get_actions_and_types(self):
        result = subprocess.run(
            ['ros2', 'action', 'list', '-t'],
            capture_output=True,
            text=True
        )
        action_list = list(tuple())
        for line in result.stdout.splitlines():
            if(line.find(" ") == -1): 
                continue
            action = line.split(" ")[0]
            msg_type = line.split(" ")[1].replace("[", "").replace("]", "") 
            action_list.append((action, msg_type))

        return action_list

    def action_has_pose_stamped(self, action_type):
        return "nav2_msgs/action/NavigateToPose" == action_type

    def extract_namespace(self, action_name):
        # /robot1/navigate_to_pose -> /robot1
        parts = action_name.split('/')
        if len(parts) > 2:
            return f"/{parts[1]}"
        return ""
    
    def goal_callback(self, msg, robot_ns):

        client = self.clients[robot_ns]

        if not client.wait_for_server(timeout_sec=2.0):
            self._node.get_logger().error(f"Nav2 not available in: {robot_ns}")
            return

        goal_msg = NavigateToPose.Goal()
        goal_msg.pose = msg

        send_goal_future = client.send_goal_async(
            goal_msg,
            feedback_callback=lambda fb: self.feedback_callback(fb, robot_ns)
        )

        send_goal_future.add_done_callback(
            lambda future: self.goal_response_callback(future, robot_ns)
        )

    def goal_response_callback(self, future, robot_ns):
        goal_handle = future.result()

        if not goal_handle.accepted:
            self._node.get_logger().error(f"Goal rejected in: {robot_ns}")
            return

        result_future = goal_handle.get_result_async()

        result_future.add_done_callback(
            lambda future: self.result_callback(future, robot_ns)
        )
    
    def feedback_callback(self, feedback_msg, robot_ns):
        feedback = feedback_msg.feedback
        self._node.get_logger().info(
            f"Distance remaining: {feedback.distance_remaining:.2f}"
        )

    def result_callback(self, future, robot_ns):
        self._node.get_logger().info(f"Goal completed: {robot_ns}")
