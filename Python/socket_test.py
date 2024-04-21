import socket
import os
import time
import os.path
from collections import deque

sock_path = "/tmp/testomatic_sock.s"

if os.path.exists(sock_path):
    os.remove(sock_path)

server = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)

server.bind(sock_path)

while True:
    server.listen(1)
    conn, addr = server.accept()

    data = conn.recv(1024)

    if data:
        print(data.decode('ascii'))
        conn.send('lol lmao'.encode('ascii'))