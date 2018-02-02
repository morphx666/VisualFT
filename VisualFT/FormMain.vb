Public Class FormMain
    Private fcnVis As FunctionVisualizer

    Private animationMode As Boolean
    Private formulaFont As New Font("Consolas", 16, FontStyle.Bold, GraphicsUnit.Pixel)

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        Me.BackColor = Color.Black

        fcnVis = New FunctionVisualizer()

        fcnVis.Setup(New FunctionVisualizer.GraphSettings(800, 100, 20, New Pen(Color.Yellow, 2)),
                     New FunctionVisualizer.GraphSettings(800, 500, 120, New Pen(Color.Yellow, 2)),
                     New FunctionVisualizer.GraphSettings(800, 100, 20, New Pen(Color.Violet, 2)))

        SetupEventsHandlers()

        TextBoxFormula.Text = "1 + Cos(3 * x)"

        CreatePlots()
    End Sub

    Private Sub SetupEventsHandlers()
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
                                               fcnVis.DoFFT()
                                           Else
                                               fcnVis.StopFFT()
                                           End If
                                   End Select
                               End Sub

        AddHandler fcnVis.NewFrameAvailable, Sub(isLastFrame As Boolean)
                                                 Me.Invalidate()
                                                 If isLastFrame Then animationMode = False
                                             End Sub

        AddHandler TextBoxFormula.TextChanged, Sub()
                                                   fcnVis.Formula = TextBoxFormula.Text
                                                   LabelFormula.Text = fcnVis.Formula
                                                   CreatePlots()
                                               End Sub
        AddHandler TextBoxFormula.KeyDown, Sub(s1 As Object, s2 As KeyEventArgs)
                                               If s2.KeyCode = Keys.Enter Then
                                                   TextBoxFormula.Visible = False
                                                   Me.Focus()
                                               End If
                                           End Sub

        AddHandler LabelFormula.Click, Sub()
                                           TextBoxFormula.Location = LabelFormula.Location
                                           TextBoxFormula.Visible = True
                                           TextBoxFormula.Focus()
                                       End Sub
    End Sub

    Private Sub CreatePlots()
        If animationMode Then Exit Sub

        Task.Run(Sub()
                     fcnVis.CreatePlots()
                     Me.Invoke(New MethodInvoker(Sub() Me.Invalidate()))
                 End Sub)
    End Sub

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics

        SyncLock fcnVis.SyncObject
            g.DrawImageUnscaled(fcnVis.LinearPlot, 10, 60)
            g.DrawImageUnscaled(fcnVis.FFTPlot, 10, 60 + fcnVis.LinearPlotSettings.Height + 10)
            g.DrawImageUnscaled(fcnVis.CircularPlot, 10, 60 + fcnVis.LinearPlotSettings.Height + fcnVis.FFTPlotSettings.Height + 10 * 2)
        End SyncLock

        g.DrawString($"Cycles/s: {fcnVis.CyclesPerSecond:N2}", Me.Font, Brushes.CadetBlue, 5, 5)
        g.DrawString($"Position: {fcnVis.SamplePosition:N2}", Me.Font, Brushes.Red, 5, 20)

        Using sb As New SolidBrush(fcnVis.FFTPlotSettings.Color.Color)
            g.DrawString($"Center Of Mass: {fcnVis.CenterOfMass:N2}", Me.Font, sb, 5, 35)
        End Using
    End Sub
End Class
