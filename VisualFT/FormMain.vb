Public Class FormMain
    Private fcnVis As FunctionVisualizer

    Private animationMode As Boolean

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        Me.BackColor = Color.Black

        AddHandler Me.KeyDown, Sub(s1 As Object, e1 As KeyEventArgs)
                                   Select Case e1.KeyCode
                                       Case Keys.Up
                                           fcnVis.CyclesPerSecond += 0.01
                                           CreatePlots()
                                       Case Keys.Down
                                           fcnVis.CyclesPerSecond -= 0.01
                                           CreatePlots()
                                       Case Keys.PageUp
                                           fcnVis.CyclesPerSecond += 0.1
                                           CreatePlots()
                                       Case Keys.PageDown
                                           fcnVis.CyclesPerSecond -= 0.1
                                           CreatePlots()

                                       Case Keys.Right
                                           fcnVis.SamplePosition += 0.5
                                           CreatePlots()
                                       Case Keys.Left
                                           fcnVis.SamplePosition -= 0.5
                                           CreatePlots()

                                       Case Keys.Enter
                                           animationMode = Not animationMode
                                           If animationMode Then
                                               fcnVis.DoFFT(5)
                                           Else
                                               fcnVis.StopFFT()
                                           End If
                                   End Select
                               End Sub

        fcnVis = New FunctionVisualizer()
        fcnVis.CreateBitmaps(New FunctionVisualizer.GraphSettings(800, 100, 20),
                          New FunctionVisualizer.GraphSettings(800, 500, 120),
                          New FunctionVisualizer.GraphSettings(800, 100, 20))


        AddHandler fcnVis.NewFrameAvailable, Sub(isLastFrame As Boolean)
                                                 Me.Invalidate()
                                                 If isLastFrame Then animationMode = False
                                             End Sub

        fcnVis.Function = New FunctionVisualizer.FunctionProvider(Function(x As Double) As Double
                                                                      Return 1 + Math.Cos(3 * x)
                                                                  End Function)

        CreatePlots()
    End Sub

    Private Sub CreatePlots()
        fcnVis.CreatePlots()
        Me.Invalidate()
    End Sub

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics

        SyncLock fcnVis.SyncObject
            g.DrawImageUnscaled(fcnVis.LinearPlot, 10, 60)
            g.DrawImageUnscaled(fcnVis.FFTPlot, 10, 60 + fcnVis.LinearPlotSettings.Height + 10)
            g.DrawImageUnscaled(fcnVis.CircularPlot, 10, 60 + fcnVis.LinearPlotSettings.Height + fcnVis.FFTPlotSettings.Height + 10 * 2)
        End SyncLock

        g.DrawString($"Cycles/s: {fcnVis.CyclesPerSecond:N2}", Me.Font, Brushes.LightBlue, 5, 5)
        g.DrawString($"Position: {fcnVis.SamplePosition:N2}", Me.Font, Brushes.IndianRed, 5, 20)
        g.DrawString($"Center Of Mass: {fcnVis.CenterOfMass:N2}", Me.Font, Brushes.IndianRed, 5, 35)
    End Sub
End Class
