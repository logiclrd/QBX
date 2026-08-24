# pip install numpy pyvista

import numpy as np
import pyvista as pv
from matplotlib.colors import ListedColormap

# 1. Grid Configuration
GRID_SIZE = 120
x = np.linspace(-10, 10, GRID_SIZE)
y = np.linspace(-10, 10, GRID_SIZE)
z = np.linspace(-10, 10, GRID_SIZE)
X, Y, Z = np.meshgrid(x, y, z, indexing="ij")

# 2. Define the Voxel Mesh Structure (Uniform Grid)
grid = pv.ImageData()
grid.dimensions = (GRID_SIZE, GRID_SIZE, GRID_SIZE)
grid.spacing = (1.0, 1.0, 1.0)  # Voxel spacing sizes
grid.origin = (0, 0, 0)


# 3. Alcubierre warp field calculation
def calculate_field():
    BUBBLERADIUS = 5.5
    BUBBLESIGMA = 2.5

    R = np.sqrt(X**2 + Y**2 + Z**2)
    SHAPE = -BUBBLESIGMA * np.exp(BUBBLESIGMA * (R - BUBBLERADIUS)) / ((np.exp(BUBBLESIGMA * (R - BUBBLERADIUS)) + 1) ** 2)

    field = (
        (Z / R) * SHAPE
    )

    field = field / field.max()

    return field.flatten()  # PyVista expects a flat 1D array mapped to cells/points


# 4. Initialize the Interactive Plotter Window
plotter = pv.Plotter(title="Alcubierre Warp Field")
plotter.add_axes()
plotter.set_background("black")  # High contrast for translucent voxels

# Generate warp field
field = calculate_field()

# We map scalars to points for smooth volumetric rendering (Volume/Voxel raycasting)
grid.point_data["Field_Values"] = field

# 5. Define Custom Color Mapping & Opacity Mapping
RED = [1, 0, 0]
WHITE = [1, 1, 1]
BLUE = [0, 0, 1]

NEGATIVE = np.linspace(RED, WHITE, 30)
POSITIVE = np.linspace(WHITE, BLUE, 30)

custom_cmap = ListedColormap(np.concatenate([NEGATIVE, POSITIVE[1:]]))

opacity_domain = np.concatenate([[0], np.linspace(1, 0, 127), np.linspace(0, 1, 127)[1:], [0]])
opacity_transfer = (opacity_domain ** 6).flatten()

# Add volume rendering model to the scene
volume_actor = plotter.add_volume(
    grid,
    scalars="Field_Values",
    cmap=custom_cmap,
    opacity=opacity_transfer,
    opacity_unit_distance=1,
    clim=[-1.0, 1.0],  # Fix limits so color/opacity meaning stays consistent
    show_scalar_bar=True,
)

# Render the window and show it asynchronously to allow the loop to run
plotter.show(interactive_update=True, auto_close=False)

# Wire up window close
interactor = plotter.render_window.GetInteractor()

if not interactor:
    print("Window interactor not found, you will have to ^C")
else:
    def close_window(obj, event):
        print("Exit event received")
        plotter.render_window.Finalize()
        obj.TerminateApp()
        plotter.close()

    interactor.AddObserver("ExitEvent", close_window);

# 6. Real-Time Calculation & Animation Loop
print("Running voxel grid animation. Close the plotter window to stop.")
time_step = 0.0
try:
    while plotter.render_window is not None:
        plotter.update()

except KeyboardInterrupt:
    print("Animation stopped by user.")
