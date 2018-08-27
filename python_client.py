import socket

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect(('127.0.0.1', 6789))
print("Connecting to Server...")
while True:
    msg = client_socket.recv(1024)
    client_socket.sendall('hey\n')
    print(msg)
