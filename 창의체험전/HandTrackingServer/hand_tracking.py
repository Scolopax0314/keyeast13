import cv2
import mediapipe as mp
import socket
import json

# 소켓 설정 (UDP)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
unity_ip = '127.0.0.1'  # Unity가 실행되는 컴퓨터의 IP
unity_port = 5055

mp_hands = mp.solutions.hands
hands = mp_hands.Hands()

# 카메라 확인 및 선택
cap = None
for camera_index in [2, 0]:  # 2번 카메라가 있으면 2번을 사용, 없으면 0번 카메라 사용
    cap = cv2.VideoCapture(camera_index)
    if cap.isOpened():
        print(f"카메라 {camera_index}를 사용합니다.")
        break
else:
    print("사용할 수 있는 카메라가 없습니다.")
    exit()

while cap.isOpened():
    success, frame = cap.read()
    if not success:
        break

    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(image)

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            data = []
            for lm in hand_landmarks.landmark:
                data.append({'x': lm.x, 'y': lm.y, 'z': lm.z})
            json_data = json.dumps(data)
            sock.sendto(json_data.encode(), (unity_ip, unity_port))

cap.release()
