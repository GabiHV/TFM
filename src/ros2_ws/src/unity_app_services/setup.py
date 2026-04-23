from setuptools import find_packages, setup

package_name = 'unity_app_services'

setup(
    name=package_name,
    version='0.0.1',
    packages=find_packages(exclude=['test']),
    data_files=[
        ('share/ament_index/resource_index/packages',
            ['resource/' + package_name]),
        ('share/' + package_name, ['package.xml']),
    ],
    install_requires=['setuptools'],
    zip_safe=True,
    maintainer='Gabriel Hernandez Vallejo',
    maintainer_email='ghernande438@alumno.uned.es',
    description='Service related to Unity app',
    license='Apache-2.0',
    extras_require={
        'test': [
            'pytest',
        ],
    },
    entry_points={
        'console_scripts': [
            "default_server_endpoint = unity_app_services.default_server_endpoint:main",
        ],
    },
)
