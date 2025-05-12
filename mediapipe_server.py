import socket
import cv2
import mediapipe as mp
import json

mp_hands = mp.solutions.hands
hands = mp_hands.Hands()
HOST = '127.0.0.1'
PORT = 9999

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind((HOST, PORT))
sock.listen(1)
conn, _ = sock.accept()
for idx in [2,1,0]:
    cap = cv2.VideoCapture(idx)
    if cap.isOpened():
        break
while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = hands.process(image)

    if result.multi_hand_landmarks:
        lm = result.multi_hand_landmarks[0]
        landmarks = [{'x': pt.x, 'y': pt.y, 'z': pt.z} for pt in lm.landmark]
        data = json.dumps(landmarks)
        conn.sendall(data.encode('utf-8') + b'\n')
    
cap.release()
conn.close()
sock.close()