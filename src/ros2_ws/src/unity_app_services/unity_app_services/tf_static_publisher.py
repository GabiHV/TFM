import rclpy
from rclpy.node import Node
from tf2_msgs.msg import TFMessage
from unity_server_interfaces.msg import TFStatic, TFStaticArray

class TFStaticCollector:
    def __init__(self, node: Node):
        self._node = node

        self.transforms = {}
        self.dynamic_topics = set()
        self.subscribers = {}
        self.subscribed_topics = set()

        self._node.create_timer(0.5, self.discover_tf_static)
        self.publisher = self._node.create_publisher(TFStaticArray, '/tf_static_unity', 10)
        self._node.create_timer(5, self.publish_tf)

    def discover_tf_static(self):
        topics = self._node.get_topic_names_and_types()
        for topic, types in topics:

            if 'tf2_msgs/msg/TFMessage' not in types:
                continue

            publishers_info = self._node.get_publishers_info_by_topic(topic)
            if len(publishers_info) == 0: # No publishers
                continue

            is_not_latching = publishers_info[0].qos_profile.durability == rclpy.qos.QoSDurabilityPolicy.TRANSIENT_LOCAL
            if not is_not_latching: # Non-static tf
                continue
            
            if topic in self.subscribed_topics:
                continue

            self._node.get_logger().info(f"[{TFStaticCollector.__name__}] Subscribing to TF topic: {topic}")
            
            sub = self._node.create_subscription(
                TFMessage,
                topic,
                lambda msg, topic=topic: self.callback(msg, topic),
                rclpy.qos.QoSProfile(depth=10, durability=rclpy.qos.QoSDurabilityPolicy.TRANSIENT_LOCAL, reliability=rclpy.qos.QoSReliabilityPolicy.RELIABLE))

            self.subscribers[topic] = sub
            self.subscribed_topics.add(topic)

    def unsubscribe_to_tf_dynamic(self, topic):
        self._node.get_logger().info(f"[{TFStaticCollector.__name__}] Unubscribing to TF topic: {topic}")
        self._node.destroy_subscription(self.subscribers[topic])

    def callback(self, msg, topic):
        if topic in self.dynamic_topics:
            return
        self._node.get_logger().info(f"[{TFStaticCollector.__name__}] Evaluating to TF topic: {topic}")
        
        publishers_info = self._node.get_publishers_info_by_topic(topic)
        is_latching = publishers_info[0].qos_profile.durability == rclpy.qos.QoSDurabilityPolicy.TRANSIENT_LOCAL

        if not is_latching:
            self.dynamic_topics.add(topic)
            self.unsubscribe_to_tf_dynamic(topic)
            return

        self.transforms[topic] = msg.transforms

    def publish_tf(self):
        static_transforms = TFStaticArray()

        for topic, transforms in self.transforms.items():
            tf_info = TFStatic()
            tf_info.topic = topic
            tf_info.transforms = transforms
            static_transforms.tf.append(tf_info)

        self.publisher.publish(static_transforms)

if __name__ == '__main__':
    TFStaticCollector()
    rospy.spin()
