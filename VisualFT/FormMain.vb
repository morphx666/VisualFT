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
        'TextBoxFormula.Text = "2 * Cos(x)"
        'fcnVis.SamplePosition = 0
        'fcnVis.CyclesPerSecond = 1

        CreatePlots()
    End Sub

    Private Sub SetupEventsHandlers()
        AddHandler Me.KeyDown, Sub(s1 As Object, e1 As KeyEventArgs)
                                   ' Normal         = 0.01
                                   ' SHIFT          = 0.1
                                   ' SHIFT + CTRL   = 0.2
                                   Dim k As Double = If(e1.Shift, If(e1.Control, 0.2, 0.1), 0.01)

                                   Select Case e1.KeyCode
                                       Case Keys.Up
                                           fcnVis.CyclesPerSecond += k
                                           CreatePlots()
                                       Case Keys.Down
                                           fcnVis.CyclesPerSecond -= k
                                           CreatePlots()

                                       Case Keys.Right
                                           fcnVis.SamplePosition += k
                                           CreatePlots()
                                       Case Keys.Left
                                           fcnVis.SamplePosition -= k
                                           CreatePlots()

                                       Case Keys.Enter
                                           animationMode = Not animationMode
                                           If animationMode Then
                                               fcnVis.DoFourierTransform()
                                           Else
                                               fcnVis.StopFourierTransform()
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
                                           TextBoxFormula.Width = LabelFormula.Width
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
