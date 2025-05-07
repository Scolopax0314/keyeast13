import cv2
import mediapipe as mp
import socket
import json
import time

# 야기서 mediapipe로 손 추적 -> UDP로 unity에 전송 -> ui업데이트(UnityMainThreadDispatcher.cs 필요)

# 소켓 설정 (UDP)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
unity_ip = '127.0.0.1'
unity_port = 5055

# MediaPipe 설정
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False,
                       max_num_hands=2,
                       min_detection_confidence=0.5,
                       min_tracking_confidence=0.5)

# 카메라 확인 및 선택
cap = None
for camera_index in [2, 0]:
    temp_cap = cv2.VideoCapture(camera_index)
    if temp_cap.isOpened():
        cap = temp_cap
        print(f"카메라 {camera_index}를 사용합니다.")
        break
else:
    print("사용할 수 있는 카메라가 없습니다.")
    exit()

last_send_time = 0
send_interval = 1 / 30  # 최대 30 FPS로 전송 제한

while cap.isOpened():
    success, frame = cap.read()
    if not success:
        print("프레임을 읽을 수 없습니다.")
        continue

    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(image)

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            data = [{'x': lm.x, 'y': lm.y, 'z': lm.z} for lm in hand_landmarks.landmark]

            current_time = time.time()
            if current_time - last_send_time >= send_interval:
                try:
                    json_data = json.dumps(data)
                    sock.sendto(json_data.encode(), (unity_ip, unity_port))
                    last_send_time = current_time
                except Exception as e:
                    print("전송 중 오류 발생:", e)
    else:
        # 손이 인식되지 않을 때 빈 리스트 전송
        try:
            sock.sendto(json.dumps([]).encode(), (unity_ip, unity_port))
        except Exception as e:
            print("빈 리스트 전송 중 오류 발생:", e)

cap.release()
sock.close()
