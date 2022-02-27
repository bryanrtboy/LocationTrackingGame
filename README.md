# LocationTrackingGame
A testbed to figure out geo locating a player for a location based mobile game. The concept is to use a players GPS location to sense when they are close to objects placed randomly or by GPS coordinates.

To do this in a game, it's helpful to have the players starting postion be at 0,0,0 in Unity. As they move towards things, we want to know how many meters away they are from the thing based on the longitude and latitude of the device, and of the object placed. 

The challenge is to convert all of this into meters based on the longitude of the location where the player is. The distance between one degree of longitude varies based on how far away the location is from the equator. Near the poles, the distance in longitude decreases down to 0 at the poles. Longitudes at the equator is at the equator are about 111 km per degree. Latitude does not vary as much.

Using a [Great Circle](http://edwilliams.org/gccalc.htm)  calculator, we can feed in a one degree change to calculate our distances at any location. This is then used to move the player in the Unity scene a corresponding distance in meters. We also use the equation to place objects based on supplied GPS coordinates. Then, it's a simple matter to calculate how close the player is to any object place into the game.

#Directions to use

There are two ways to test this, using preset coordinates or your current location.

To use your current location, turn off 'Use Preset Locations" in the manager gameobject.

To use preset coordinates, go to Google Maps, right-click on your location to get the GPS coordinates of your starting position and any other places where you want markers to appear. Copy those coordinates into the 'Preset Locations' array on the manager gameobject. The first location is your starting location.
