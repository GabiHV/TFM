import rclpy
from .service_node import UnityAppService

def main(args=None):
    rclpy.init()
    
    node = UnityAppService()
    rclpy.spin(node)
    rclpy.shutdown()

if __name__ == "__main__":
    main()