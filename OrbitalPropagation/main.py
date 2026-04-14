import plotly.graph_objects as go
import glob
import pandas as pd
import os
import re

def numerical_sort(value):
    match = re.search(r'frame_(\d+)\.csv', value)
    return int(match.group(1)) if match else -1

def main():
    folder = "./frames"
    files = glob.glob(os.path.join(folder, "frame_*.csv"))
    files = sorted(files, key=numerical_sort)

    df0 = pd.read_csv(files[0], header=None, names=['x', 'y', 'z'])
    trace = go.Scatter3d(
        x=df0['x'],
        y=df0['y'],
        z=df0['z'],
        mode='markers',
        marker=dict(size=2, color='blue')
    )

    frames = []
    for file in files:
        df = pd.read_csv(file, header=None, names=['x', 'y', 'z'])
        frames.append(go.Frame(
            data=[dict(type='scatter3d', x=df['x'], y=df['y'], z=df['z'], mode='markers',
                       marker=dict(size=2, color='blue'))]
        ))

    fig = go.Figure(
        data=[trace],
        frames=frames
    )

    fig.update_layout(
        updatemenus=[dict(
            type="buttons",
            showactive=False,
            buttons=[dict(label="Play",
                          method="animate",
                          args=[None, {"frame": {"duration": 50, "redraw": True},
                                       "fromcurrent": True}])])]
    )
    fig.update_layout(
        scene=dict(
            xaxis=dict(range=[-7000, 7000], title='X (km)'),
            yaxis=dict(range=[-7000, 7000], title='Y (km)'),
            zaxis=dict(range=[-2, 2], title='Z (km)')
        )
    )

    fig.show()

main()