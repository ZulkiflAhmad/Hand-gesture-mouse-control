import cv2
import mediapipe as mp
import socket
import json

# Initialize MediaPipe Hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)
mp_draw = mp.solutions.drawing_utils

# Setup UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
dest = ("127.0.0.1", 9999)

# Open webcam
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

while True:
    success, frame = cap.read()
    if not success:
        break

    frame = cv2.flip(frame, 1)
    h, w, _ = frame.shape
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = hands.process(rgb_frame)

    if result.multi_hand_landmarks:
        for hand_landmarks in result.multi_hand_landmarks:
            mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

            index_finger = hand_landmarks.landmark[8]
            middle_finger = hand_landmarks.landmark[12]
            thumb_finger = hand_landmarks.landmark[4]

            data = {
                "index_x": index_finger.x,
                "index_y": index_finger.y,
                "middle_x": middle_finger.x,
                "middle_y": middle_finger.y,
                "thumb_y": thumb_finger.y
            }

            message = json.dumps(data).encode()
            sock.sendto(message, dest)

    cv2.imshow("Gesture Mouse - Python", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
