﻿' https://www.youtube.com/watch?v=spUNpyF58BY

Imports System.Threading

Public Class FunctionVisualizer
    Public Structure GraphSettings
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property Scale As Integer

        Public Sub New(width As Integer, height As Integer, scale As Integer)
            Me.Width = width
            Me.Height = height
            Me.Scale = scale
        End Sub
    End Structure

    Public Delegate Function FunctionProvider(x As Double) As Double

    Private mFunction As FunctionProvider
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

    Private Const ToRad As Single = Math.PI / 180
    Private Tau As Double = 2 * Math.PI
    Private TwoTau As Double = 2 * Tau
    Private FourTau As Double = 2 * TwoTau

    Private animCancelTask As CancellationTokenSource
    Private centersOfMass As New List(Of Tuple(Of Double, Double))

    Private linearPlotPen As New Pen(Color.Yellow, 2)
    Private circularPlotPen As New Pen(Color.Yellow, 2)
    Private fftPlotPen As New Pen(Color.Violet, 2)

    Public SyncObject As New Object()

    Public Event NewFrameAvailable(isLastFrame As Boolean)

    Public Sub New()
        mFunction = New FunctionProvider(Function(x As Double) As Double
                                             Return 1 + Math.Cos(3 * x)
                                         End Function)

        mCyclesPerSecond = 3
        mSamplePosition = 14.7
        mResolution = 50
        centersOfMass.Clear()
    End Sub

    Public Sub CreateBitmaps(linearPlotSettings As GraphSettings, circularPlotSettings As GraphSettings, fftPlotSettings As GraphSettings)
        mLinearPlotSettings = linearPlotSettings
        mLinearPlot = New Bitmap(mLinearPlotSettings.Width + 1, mLinearPlotSettings.Height + 1)

        mCircularPlotSettings = circularPlotSettings
        mCircularPlot = New Bitmap(mCircularPlotSettings.Width + 1, mCircularPlotSettings.Height + 1)

        mFFTPlotSettings = fftPlotSettings
        mFFTPlot = New Bitmap(mFFTPlotSettings.Width + 1, mFFTPlotSettings.Height + 1)
    End Sub

    Public Sub New([function] As FunctionProvider, Optional cyclesPerSecond As Double = 3, Optional resolution As Double = 50)
        Me.New()
        mFunction = [function]
        mCyclesPerSecond = cyclesPerSecond
        mResolution = resolution
    End Sub

    Public Property [Function] As FunctionProvider
        Get
            Return mFunction
        End Get
        Set(value As FunctionProvider)
            mFunction = value
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

    Public Sub DoFFT(frequency As Double)
        animCancelTask = New CancellationTokenSource()
        Dim ct As CancellationToken = animCancelTask.Token

        centersOfMass.Clear()

        Task.Run(Sub()
                     For cps As Double = 0.0001 To frequency * 2 Step 0.01
                         mCyclesPerSecond = cps

                         CreatePlots()

                         RaiseEvent NewFrameAvailable(False)

                         Thread.Sleep(5)

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
            CreateLinearPlot()
            CreateCircularPlot()
            CreateFFTPlot()
        End SyncLock
    End Sub

    Private Sub CreateLinearPlot()
        Dim width As Integer = mLinearPlotSettings.Width
        Dim height As Integer = mLinearPlotSettings.Height
        Dim scale As Integer = mLinearPlotSettings.Scale

        Dim w2 As Integer = width / 2
        Dim h2 As Integer = height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim t As Double
        Dim secPerCycle As Double = 1 / mCyclesPerSecond

        Using g As Graphics = Graphics.FromImage(mLinearPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(width / 2, -height / 2)

            For x As Integer = -w2 To w2 Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, h2, x, -h2)
                For y As Integer = -h2 To h2 Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), -w2, y, w2, y)
                Next
            Next

            p1 = New PointF(-w2, scale * mFunction((mSamplePosition + w2) / width * TwoTau))
            For x As Double = -w2 To w2
                t = (x + w2) / width * TwoTau
                p2 = New PointF(x, scale * mFunction(t))

                g.DrawLine(linearPlotPen, p1, p2)
                p1 = p2
            Next

            ' Draw sample line
            g.DrawLine(Pens.Red, CInt(mSamplePosition), -height, CInt(mSamplePosition), height)
        End Using
    End Sub

    Private Sub CreateCircularPlot()
        Dim width As Integer = mCircularPlotSettings.Width
        Dim height As Integer = mCircularPlotSettings.Height
        Dim scale As Integer = mCircularPlotSettings.Scale

        Dim w2 As Integer = width / 2
        Dim h2 As Integer = height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim t As Double
        Dim a As Double
        Dim secPerCycle As Double = 1 / mCyclesPerSecond

        Dim sumX As Double = 0
        Dim sumY As Double = 0

        Using g As Graphics = Graphics.FromImage(mCircularPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(width / 2, -height / 2)

            For x As Integer = -w2 To w2 Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, h2, x, -h2)
                For y As Integer = -h2 To h2 Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), -w2, y, w2, y)
                Next
            Next

            'p1 = New PointF(scale * mFunction(0) * Math.Cos(0), ' Math.Cos(0) = 1
            '                scale * mFunction(0) * Math.Sin(0)) ' Math.Sin(0) = 0
            p1 = New PointF(scale * mFunction(0), 0)

            For a = 0 To FourTau Step 1 / mResolution
                t = a * secPerCycle

                p2 = New PointF(scale * mFunction(t) * Math.Cos(a),
                                scale * mFunction(t) * Math.Sin(a))

                g.DrawLine(circularPlotPen, p1, p2)
                p1 = p2

                sumX += p2.X / scale
                sumY += p2.Y / scale
            Next

            mCenterOfMass = Math.Sqrt(sumX ^ 2 + sumY ^ 2) / mResolution / 4

            Dim p As New Tuple(Of Double, Double)(mCyclesPerSecond, mCenterOfMass)
            If Not centersOfMass.Contains(p) Then centersOfMass.Add(p)

            ' Draw sample line
            t = mSamplePosition / width * FourTau
            a = (t / secPerCycle)
            p1 = New PointF(0, 0)
            p2 = New PointF(scale * mFunction(t) * Math.Cos(a),
                            scale * mFunction(t) * Math.Sin(a))
            g.DrawLine(Pens.Red, p1, p2)

            ' Draw center of mass
            Using sb As New SolidBrush(fftPlotPen.Color)
                g.FillEllipse(sb, CSng(sumX / 4),
                                  CSng(sumY / 4), 8, 8)
            End Using
        End Using
    End Sub

    Private Sub CreateFFTPlot()
        Dim width As Integer = mLinearPlotSettings.Width
        Dim height As Integer = mLinearPlotSettings.Height
        Dim scale As Integer = mLinearPlotSettings.Scale

        Dim w2 As Integer = width / 2
        Dim h2 As Integer = height / 2

        Dim p1 As PointF
        Dim p2 As PointF

        Dim secPerCycle As Double = 1 / mCyclesPerSecond

        Using g As Graphics = Graphics.FromImage(mFFTPlot)
            g.SmoothingMode = Drawing2D.SmoothingMode.None
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.None

            g.Clear(Color.Black)

            g.ScaleTransform(1.0, -1.0)
            g.TranslateTransform(0, -height)

            For x As Integer = 0 To width Step mResolution
                g.DrawLine(If(x = 0, Pens.LightGray, Pens.DimGray), x, 0, x, height)
                For y As Integer = 0 To height Step mResolution
                    g.DrawLine(If(y = 0, Pens.LightGray, Pens.DimGray), 0, y, width, y)
                Next
            Next
            If centersOfMass.Count = 0 Then Exit Sub

            Dim ordered As IOrderedEnumerable(Of Tuple(Of Double, Double)) = centersOfMass.OrderBy(Function(com) com.Item1)

            p1 = New PointF(ordered(0).Item1 * mResolution, ordered(0).Item2 * scale)
            For Each com In ordered
                p2 = New PointF(com.Item1 * mResolution, com.Item2 * scale)

                g.DrawLine(fftPlotPen, p1, p2)

                p1 = p2
            Next

            ' Draw mCyclesPerSecond line
            g.DrawLine(Pens.Red, CInt(mCyclesPerSecond * mResolution), 0, CInt(mCyclesPerSecond * mResolution), height)
        End Using
    End Sub
End Class
