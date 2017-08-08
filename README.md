# RoboND-UnitySim
Prototype project for Robotics NanoDegree

This project ..something, something.. quad drone controls and deep learning exercise

<br>The project features two main environments ('scenes') to experiment with:
1. quad_indoor: a giant box sized 300m³, with a 1m tiling grid covering all sides.
2. proto4: an outdoor city environment
The city scene starts with a menu to choose between controlling the quad for the Controls and Deep Learning projects, and setting up to train the neural network for the Deep Learning project. In the training mode, a human character will spawn in a random place with a random appearance, and a camera follows the person around to record.

Controlling the quad locally:
1. Fire up the scene or the executable with the indoor environment
2. To enable local control, press F12 or click the "Input off" button on-screen. Doing so again will disable control
3. The drone has a simplified local control script that allows the user to quickly and easily move around as if it was a game character.
4. Use WSAD or the arrow keys to move the quad forward, back, left, or right
5. Use Space and C to move the quad up and down
6. Q and E will yaw the quad

Capturing images for deep learning (proto4):
1. Fire up the scene or the executable with the city environment. Select DL Training from the menu
2. To begin recording, press R to bring up the dialog and choose where to save the recording. Select or create a convenient folder, such as in your Desktop or Documents, and confirm, and recording begins.
3. Images are captured from two cameras - one that sees the environment as you do, and one that sees in black&white as shown at the bottom right. The images are captured once every 3 seconds or so.
4. To stop recording, press R again, or simply close the executable (or stop the Editor if launching from Unity). If using Unity, the snapshot frequency as well as the camera's rotation and position speeds can be controlled as well from the Inspector, with the OrbitCamera object selected in the Hierarchy

Ros commands available locally:
1. F8: change camera distance (random distance between 2-20m selected at the moment)
2. Keys 1-5: change camera view - front, side, top, iso, free (uses iso at the moment)
3. R: reset quad's orientation
