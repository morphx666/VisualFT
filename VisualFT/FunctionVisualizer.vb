' https://www.youtube.com/watch?v=spUNpyF58BY

Imports System.Threading

Public Class FunctionVisualizer
    Public Structure GraphSettings
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property Scale As Single
        Public ReadOnly Property Color As Pen

        Public Sub New(width As Integer, height As Integer, scale As Single, color As Pen)
            Me.Width = width
            Me.Height = height
            Me.Scale = scale
            Me.Color = color
        End Sub
    End Structure

    Private mFormula As String
    Private mCyclesPerSecond As Double
    Private mSamplePosition As Double
    Private mCenterOfMass As Double
    Private mResolution As Double

    Private mLinearPlot As Bitmap
    Private mLinearPlotSettings As GraphSettings
    Private mCircularPlot As Bitmap
    Private mCircularPlotSettings As GraphSettings
    Private mFFTPlot As Bitmap
    Private mFFTPlotSettings As GraphSettings

    Private Tau As Double = 2 * Math.PI

    Private evaluator As Evaluator
    Private animCancelTask As CancellationTokenSource
    Private centersOfMass As New List(Of Tuple(Of Double, Double))

    ' FIXME: Need to define what "factor" is...
    Private Const factor As Double = 8

    Public SyncObject As New Object()

    Public Event NewFrameAvailable(isLastFrame As Boolean)

    Public Sub New()
        evaluator = New Evaluator()
        evaluator.CustomParameters.Add("x", 0)

        Formula = "1 + Cos(3 * x)"
        mCyclesPerSecond = 3
        mSamplePosition = 2.9
        mResolution = 50
        centersOfMass.Clear()
    End Sub

    Public Sub New(formula As String, Optional cyclesPerSecond As Double = 3, Optional resolution As Double = 50)
        Me.New()
        Me.Formula = formula
        mCyclesPerSecond = cyclesPerSecond
        mResolution = resolution
    End Sub

    Public Sub Setup(linearPlotSettings As GraphSettings, circularPlotSettings As GraphSettings, fftPlotSettings As GraphSettings)
        mLinearPlotSettings = linearPlotSettings
        mLinearPlot = New Bitmap(mLinearPlotSettings.Width + 1, mLinearPlotSettings.Height + 1)

        mCircularPlotSettings = circularPlotSettings
        mCircularPlot = New Bitmap(mCircularPlotSettings.Width + 1, mCircularPlotSettings.Height + 1)

        mFFTPlotSettings = fftPlotSettings
        mFFTPlot = New Bitmap(mFFTPlotSettings.Width + 1, mFFTPlotSettings.Height + 1)
    End Sub

    Public Property Formula As String
        Get
            Return mFormula
        End Get
        Set(value As String)
            mFormula = value
            evaluator.Formula = mFormula
            centersOfMass.Clear()
        End Set
    End Property

    Public Property CyclesPerSecond As Double
        Get
            Return mCyclesPerSecond
        End Get
        Set(value As Double)
            mCyclesPerSecond = value
        End Set
    End Property

    Public Property SamplePosition As Double
        Get
            Return mSamplePosition
        End Get
        Set(value As Double)
            mSamplePosition = value
        End Set
    End Property

    Public ReadOnly Property CenterOfMass As Double
        Get
            Return mCenterOfMass
        End Get
    End Property

    Public ReadOnly Property LinearPlot As Bitmap
        Get
            Return mLinearPlot
        End Get
    End Property

    Public ReadOnly Property LinearPlotSettings As GraphSettings
        Get
            Return mLinearPlotSettings
        End Get
    End Property

    Public ReadOnly Property CircularPlot As Bitmap
        Get
            Return mCircularPlot
        End Get
    End Property

    Public ReadOnly Property CircularPlotSettings As GraphSettings
        Get
            Return mCircularPlotSettings
        End Get
    End Property

    Public ReadOnly Property FFTPlot As Bitmap
        Get
            Return mFFTPlot
        End Get
    End Property

    Public ReadOnly Property FFTPlotSettings As GraphSettings
        Get
            Return mFFTPlotSettings
        End Get
    End Property

    Public Property Resolution As Double
        Get
            Return mResolution
        End Get
        Set(value As Double)
            mResolution = value
        End Set
    End Property

    Public Sub DoFFT()
        animCancelTask = New CancellationTokenSource()
        Dim ct As CancellationToken = animCancelTask.Token

        centersOfMass.Clear()

        Task.Run(Sub()
                     For cps As Double = 0 To mLinearPlotSettings.Width / mResolution Step 0.02
                         mCyclesPerSecond = cps

                         CreatePlots()

                         RaiseEvent NewFrameAvailable(False)

                         Thread.Sleep(1)

                         If ct.IsCancellationRequested Then
                             animCancelTask.Dispose()
                             animCancelTask = Nothing
                             Exit For
                         End If
                     Next

                     RaiseEvent NewFrameAvailable(True)
                 End Sub, ct)
    End Sub

    Public Sub StopFFT()
        animCancelTask?.Cancel()
    End Sub

    Public Sub CreatePlots()
        SyncLock SyncObject
            Try
                CreateLinearPlot()
            Catch ex As Exception
            End Try

            Try
                CreateCircularPlot()
            Catch ex As Exception
            End Try

            Try
                CreateFFTPlot()
            Catch ex As Exception
            End Try
        End SyncLock
    End Sub

    Private Sub CreateLinearPlot()
        Dim w2 As Single = mLinearPlotSettings.Width / 2
        Dim h2 As Single = mLinearPlotSettings.Height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim t As Double
        Dim secPerCycle As Double = 1 / mCyclesPerSecond

        Dim graphLength As Double = 2 * Tau

        Using g As Graphics = Graphics.FromImage(mLinearPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(mLinearPlotSettings.Width / 2, -mLinearPlotSettings.Height / 2)

            For x As Integer = -w2 To w2 Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, h2, x, -h2)
                For y As Integer = -h2 To h2 Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), -w2, y, w2, y)
                Next
            Next

            p1 = New PointF(-w2, mLinearPlotSettings.Scale * evaluator.Evaluate(-w2 / mLinearPlotSettings.Width * graphLength))
            For x As Double = -w2 To w2
                t = x / mLinearPlotSettings.Width * graphLength
                p2 = New PointF(x, mLinearPlotSettings.Scale * evaluator.Evaluate(t))

                g.DrawLine(LinearPlotSettings.Color, p1, p2)
                p1 = p2
            Next

            ' Draw sample line
            g.DrawLine(Pens.Red, CInt(mSamplePosition * factor), -mLinearPlotSettings.Height,
                                 CInt(mSamplePosition * factor), mLinearPlotSettings.Height)
        End Using
    End Sub

    Private Sub CreateCircularPlot()
        Dim w2 As Single = mCircularPlotSettings.Width / 2
        Dim h2 As Single = mCircularPlotSettings.Height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim t As Double
        Dim a As Double
        Dim secPerCycle As Double

        Dim sumX As Double = 0
        Dim sumY As Double = 0

        Dim sampleLength As Double = factor * Tau * Math.Sqrt(mCyclesPerSecond)  ' Increase the sampling length as the frequency increases

        If mCyclesPerSecond > 0 Then
            secPerCycle = 1 / mCyclesPerSecond
        Else
            ' Simulate infinity. We don't use Double.MaxValue as this would quickly
            ' produce an actual Double.IsInfinity result breaking everything
            secPerCycle = Evaluator.Infinity
        End If

        Using g As Graphics = Graphics.FromImage(mCircularPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(mCircularPlotSettings.Width / 2, -mCircularPlotSettings.Height / 2)

            For x As Integer = -w2 To w2 Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, h2, x, -h2)
                For y As Integer = -h2 To h2 Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), -w2, y, w2, y)
                Next
            Next

            p1 = New PointF(mCircularPlotSettings.Scale * evaluator.Evaluate(0), 0)

            For a = 0 To sampleLength Step 1 / (mResolution - mCyclesPerSecond * 2) ' This "Step" formula increases performance, while progressively decreasing the quality of the plot
                t = a * secPerCycle

                p2 = New PointF(mCircularPlotSettings.Scale * evaluator.Evaluate(t) * Math.Cos(a),
                                mCircularPlotSettings.Scale * evaluator.Evaluate(t) * Math.Sin(a))

                g.DrawLine(CircularPlotSettings.Color, p1, p2)
                p1 = p2

                sumX += p2.X / mCircularPlotSettings.Scale
                sumY += p2.Y / mCircularPlotSettings.Scale
            Next

            mCenterOfMass = Math.Sqrt(sumX ^ 2 + sumY ^ 2) / mResolution / factor

            Dim p As New Tuple(Of Double, Double)(mCyclesPerSecond, mCenterOfMass)
            If Not centersOfMass.Contains(p) Then centersOfMass.Add(p)

            ' Draw sample line
            t = mSamplePosition / mCircularPlotSettings.Width * sampleLength
            a = (t / secPerCycle)
            p1 = New PointF(0, 0)
            p2 = New PointF(mCircularPlotSettings.Scale * evaluator.Evaluate(t) * Math.Cos(a),
                            mCircularPlotSettings.Scale * evaluator.Evaluate(t) * Math.Sin(a))
            g.DrawLine(Pens.Red, p1, p2)

            ' Draw center of mass
            Using sb As New SolidBrush(FFTPlotSettings.Color.Color)
                g.FillEllipse(sb, CSng(sumX / factor),
                                  CSng(sumY / factor), 8, 8)
            End Using
        End Using
    End Sub

    Private Sub CreateFFTPlot()
        Dim w2 As Single = mFFTPlotSettings.Width / 2
        Dim h2 As Single = mFFTPlotSettings.Height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim secPerCycle As Double = 1 / mCyclesPerSecond

        Using g As Graphics = Graphics.FromImage(mFFTPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(0, -mFFTPlotSettings.Height)

            For x As Integer = 0 To mFFTPlotSettings.Width Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, 0, x, mFFTPlotSettings.Height)
                For y As Integer = 0 To mFFTPlotSettings.Height Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), 0, y, mFFTPlotSettings.Width, y)
                Next
            Next
            If centersOfMass.Count = 0 Then Exit Sub

            Dim ordered As IOrderedEnumerable(Of Tuple(Of Double, Double)) = centersOfMass.OrderBy(Function(com) com.Item1)

            p1 = New PointF(ordered(0).Item1 * mResolution, ordered(0).Item2 * mFFTPlotSettings.Scale)
            For Each com In ordered
                p2 = New PointF(com.Item1 * mResolution, com.Item2 * mFFTPlotSettings.Scale)

                g.DrawLine(FFTPlotSettings.Color, p1, p2)

                p1 = p2
            Next

            ' Draw mCyclesPerSecond line
            g.DrawLine(Pens.CadetBlue, CSng(mCyclesPerSecond * mResolution), 0, CSng(mCyclesPerSecond * mResolution), mFFTPlotSettings.Height)
        End Using
    End Sub
End Class
