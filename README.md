# VisualFT
Simple Fourier Transform visualizer

![VisualFT](https://xfx.net/stackoverflow/VisualFT/VisualFT.png)

Inspired by the visualizations from ![3Blue1Brown](https://www.youtube.com/channel/UCYO_jab_esuFRV4b17AJtAw) for the !["But what is the Fourier Transform? A visual introduction."](https://www.youtube.com/watch?v=spUNpyF58BY) video.

* At this moment, the only way to change the function is to edit the following code:
  ```
  fcnVis.Function = New FunctionVisualizer.FunctionProvider(Function(x As Double) As Double
      Return 1 + Math.Cos(3 * x)
  End Function)
  ```
* Use the arrow keys and page up and page down keys to change various parameters

* Use the ENTER key to run a simple animation
