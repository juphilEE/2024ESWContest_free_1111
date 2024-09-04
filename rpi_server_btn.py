import RPi.GPIO as GPIO
import time
import threading
from flask import Flask, jsonify

app = Flask(__name__)

# 스위치 핀 설정
switch_pins = [17, 27, 22]  # 택트 스위치가 연결된 핀 번호 (예: GPIO 17, 27, 22)
switch_states = [0, 0, 0]   # 스위치 상태 저장

# 핀 모드 설정
GPIO.setmode(GPIO.BCM)

# 스위치 핀 모드 설정
for pin in switch_pins:
    GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)  # 스위치 핀을 풀업 저항과 함께 입력 모드로 설정

def read_switch_states():
    global switch_states

    while True:
        for i, pin in enumerate(switch_pins):
            switch_states[i] = 1 if GPIO.input(pin) == GPIO.HIGH else 0
        time.sleep(0.1)  # 스위치 상태를 너무 자주 체크하지 않도록 약간의 딜레이 추가

@app.route('/btn', methods=['GET'])
def get_switch_status():
    global switch_states
    return jsonify(switch_states=switch_states)

if __name__ == '__main__':
    try:
        switch_thread = threading.Thread(target=read_switch_states)
        switch_thread.daemon = True
        switch_thread.start()
        app.run(host='0.0.0.0', port=5001)  # 포트 5001로 설정
    except KeyboardInterrupt:
        print("프로그램 종료")
    finally:
        GPIO.cleanup()
