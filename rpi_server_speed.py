import RPi.GPIO as GPIO
import time
import threading
from flask import Flask, jsonify

app = Flask(__name__)

# 리드 스위치 핀 설정 및 속도 계산 변수
reed_switch_pin = 2  # 리드 스위치가 연결된 핀
pulse_count = 0
tire_circumference = 3.141592 * 0.116  # 타이어 둘레 (단위: 미터)
current_speed = 0.0  # 현재 속도

# 핀 모드 설정
GPIO.setmode(GPIO.BCM)
GPIO.setup(reed_switch_pin, GPIO.IN, pull_up_down=GPIO.PUD_UP)

def calculate_speed(pulses):
    distance = pulses * (tire_circumference / 4.0)  # 자석 4개 기준
    speed_mps = distance / 0.2  # 속도 (m/s)
    speed_kmh = speed_mps * 3.6  # 속도 (km/h)
    return speed_kmh

def read_speed():
    global pulse_count, current_speed

    previous_state = GPIO.input(reed_switch_pin)
    last_time = time.time()

    while True:
        current_state = GPIO.input(reed_switch_pin)

        if previous_state == GPIO.HIGH and current_state == GPIO.LOW:
            pulse_count += 1

        previous_state = current_state
        current_time = time.time()

        if current_time - last_time >= 0.2:
            current_speed = calculate_speed(pulse_count)
            print(f"Speed: {current_speed:.2f} km/h")
            pulse_count = 0
            last_time = current_time

        time.sleep(0.01)

@app.route('/speed', methods=['GET'])
def get_speed():
    global current_speed
    return jsonify(speed=current_speed)

if __name__ == '__main__':
    try:
        speed_thread = threading.Thread(target=read_speed)
        speed_thread.daemon = True
        speed_thread.start()
        app.run(host='0.0.0.0', port=5000)  # 포트 5000으로 설정
    except KeyboardInterrupt:
        print("프로그램 종료")
    finally:
        GPIO.cleanup()
