This project enables mouse control using real-time hand gestures. It combines a Python-based hand tracking module with a C# WinForms application to simulate mouse movement, clicks, and scrolling.

Features
Mouse movement via index finger tracking

Single click using pinch gesture

Double click using peace (two-finger) gesture

Scroll using vertical thumb-index distance

Settings stored locally in SQLite database

Right and left click via flick gestures (planned)

Technologies Used
Python (OpenCV, MediaPipe, socket)

C# WinForms (.NET), System.Data.SQLite, Newtonsoft.Json

How It Works
Python script captures webcam input and detects hand landmarks

Sends gesture data via UDP to C# application

C# app interprets gestures and simulates mouse actions

User settings (scroll speed, sensitivity) saved in SQLite

Setup Instructions
Run the Python script (hand_tracking_udp.py)

Start the C# application and press "Start Gesture Mouse"

Use hand gestures to control the mouse

Install Python dependencies:

pip install opencv-python mediapipe numpy

In Visual Studio, install required packages via NuGet:

System.Data.SQLite

Newtonsoft.Json

Project Structure
Python folder: Gesture tracking and UDP socket

C# folder: WinForms app, gesture handling, settings form

SQLite database: Stores user preferences locally

Future Work
Add support for right/left click gestures

Improve gesture calibration

Implement drag-and-drop functionality

