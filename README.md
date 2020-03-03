# Visualising-the-DFN-Dataset
The Desert Fireball Network (DFN) has a sizeable dataset on the orbits of small bodies in our solar system. As a summer scholarship program with the Curtin HIVE in 2019-2020, a Unity project was created that visualised this dataset in an interactive dynamic environment. The scripts stored in this repository all play a part in operating the visualisaiton.

## MasterScript.cs
The mother of all scripts. This is the only script executed by the Unity engine in `Start()` and `Update()`. In turn, MasterScript.cs calls the `start()` and `update()` functions of other scripts. This is done to ensure they are executed in the correct order.

## BodyPlotter.cs
The 'backbone' of the operation. It takes care of several important things:
- Reading data from CSV files.
- Calculating and assigning values to planets and meteoroids.
- Instantiating planets and meteoroids at beginning of game.
- Recalculating the positions of all bodies every frame.
- Updates the scene when the time is changed.
- Resizing halos and orbit lines according to camera distance every frame.
- Activates / Reactivates bodies and orbit lines when a button is pressed on the GUI system.

## ScreenMaster.cs
Governs everything to do with the GUI system on the screen, such as displaying or hiding GUI elements when the game is paused or a button is pressed.

## CameraController.cs
Controls the camera's position and orientation based on mouse input. Will also change the center-point when a new selection from the dropdown menu is made.

## PlanetInfo.cs and MeteoroidInfo.cs
Scripts that must be assigned to each planet and meteoroid. They store values about the planet/meteoroid and calculates the body's current position based on the time and the 6 orbital elements.

## All the rest
All the other scripts are simple and must be assigned to specific GameObjects or GUI elements. More information about them can be found in the documentation (TO DO).
