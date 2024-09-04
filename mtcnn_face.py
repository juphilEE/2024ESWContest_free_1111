import cv2
from facenet_pytorch import MTCNN
import json
import time
import socket
import requests  # 추가된 라이브러리

def send_data(rotationX, rotationY, speed, buttons):
    try:
        client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client_socket.connect(('127.0.0.1', 8080))
        message = json.dumps({
            "rotationX": rotationX,
            "rotationY": rotationY,
            "speed": speed,
            "buttons": buttons
        }).encode('ascii')
        client_socket.sendall(message)
        client_socket.close()
        print(f"Data sent successfully - Rotation X: {rotationX}, Y: {rotationY}, Speed: {speed}, Buttons: {buttons}")
    except Exception as e:
        print(f"Failed to send data: {e}")

def get_speed_from_server():
    try:
        response = requests.get("http://172.20.10.3:5000/speed")
        if response.status_code == 200:
            speed_data = response.json()
            return speed_data.get("speed", 0)
        else:
            print(f"Failed to get speed from server, status code: {response.status_code}")
            return 0
    except Exception as e:
        print(f"Error retrieving speed from server: {e}")
        return 0

# 추가된 함수: 버튼 데이터 가져오기
def get_buttons_from_server():
    try:
        response = requests.get("http://172.20.10.3:5001/btn")
        if response.status_code == 200:
            button_data = response.json()
            return button_data.get("switch_states", [0, 0, 0])  # 버튼 데이터가 없을 경우 기본값 [0, 0, 0] 설정
        else:
            print(f"Failed to get buttons from server, status code: {response.status_code}")
            return [0, 0, 0]
    except Exception as e:
        print(f"Error retrieving buttons from server: {e}")
        return [0, 0, 0]

if __name__ == "__main__":
    try:
        mtcnn = MTCNN(keep_all=True)
        print("MTCNN model initialized.")
    except Exception as e:
        print(f"Error initializing MTCNN: {e}")
        exit()

    cap = cv2.VideoCapture(0)

    if not cap.isOpened():
        print("Error: Could not open video stream.")
        exit()

    print("Video stream opened successfully.")

    lastRX = 0
    lastRY = 0

    while True:
        try:
            ret, frame = cap.read()
            if not ret:
                print("Error: Could not read frame. Retrying in 1 second...")
                time.sleep(1)
                continue

            try:
                rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            except Exception as e:
                print(f"Error in converting frame to RGB: {e}")
                continue

            H = rgb_frame.shape[0]
            W = rgb_frame.shape[1]

            try:
                boxes, probs = mtcnn.detect(rgb_frame)
            except Exception as e:
                print(f"얼굴 감지 중 오류 발생: {e}")
                boxes = None

            if boxes is not None:
                for index, box in enumerate(boxes):
                    if index > 0:
                        break

                    x, y, x2, y2 = map(int, box)
                    cv2.rectangle(frame, (x, y), (x2, y2), (0, 255, 0), 2)

                    padding = 20
                    fx, fy = (x + x2) / 2, (y + y2) / 2

                    if padding <= fx <= 640 - padding and padding <= fy <= 480 - padding:
                        rotationX = lastRX
                        rotationY = lastRY
                    rotationX = (fx - 320) * 60 / 320  # 각도 값
                    rotationY = -((fy - 240) * 40 / 240)  # 각도 값
                    lastRX = rotationX
                    lastRY = rotationY

                    # 서버에서 스피드 데이터를 가져옴
                    speed = get_speed_from_server()

                    # 서버에서 버튼 데이터를 가져옴
                    buttons = get_buttons_from_server()

                    print(f"RotationX: {rotationX}, RotationY: {rotationY}, Speed: {speed}, Buttons: {buttons}")

                    # 유니티로 회전, 속도, 버튼 데이터 전송
                    send_data(rotationY, rotationX, speed, buttons)

            else:
                print("No faces detected.")

        except Exception as e:
            print(f"Error in main loop: {e}")

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()
