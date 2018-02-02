Imports NCalc

Public Class Evaluator
    Public Delegate Function CustomFunctionDel(name As String, args As FunctionArgs)

    Private mFormula As String
    Private mCustomParameters As New Dictionary(Of String, Double)
    Private mCustomFunction As CustomFunctionDel

    Private exp As Expression

    Public Property CustomFunctionHandler As CustomFunctionDel
        Get
            Return mCustomFunction
        End Get
        Set(value As CustomFunctionDel)
            mCustomFunction = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the formula to be evaluated
    ''' </summary>
    ''' <returns><see cref="String"/></returns>
    Public Property Formula As String
        Get
            Return mFormula
        End Get
        Set(value As String)
            mFormula = value
            If mFormula = "" Then mFormula = "0"
            exp = New Expression(mFormula)

            AddHandler exp.EvaluateFunction, Sub(name As String, args As FunctionArgs)
                                                 Select Case name
                                                     Case "IIf"
                                                         If args.Parameters(0).Evaluate() Then
                                                             args.Result = args.Parameters(1).Evaluate()
                                                         Else
                                                             args.Result = args.Parameters(2).Evaluate()
                                                         End If
                                                     Case "ToRad"
                                                         args.Result = args.Parameters(0).Evaluate() * Math.PI / 180
                                                     Case "Abs"
                                                         args.Result = Math.Abs(args.Parameters(0).Evaluate())
                                                     Case "Osc"
                                                         Dim f As Double = args.Parameters(0).Evaluate()
                                                         Dim f2 As Double = f / 2
                                                         Dim f4 As Double = f / 4
                                                         Dim t As Double = Now.Ticks Mod f
                                                         args.Result = If(t < f2, t - f4, f2 - t + f4) / f4
                                                     Case "Rnd"
                                                         args.Result = (New Random()).NextDouble()
                                                     Case Else
                                                         ' This is a cheap trick to handle arrays
                                                         ' By default, this parser will interpret an array (such as A(3)) as a function call,
                                                         ' so before giving up, we check if there's any parameter named A3.
                                                         ' If such parameter exists, it means that the "user" has defined "A" as an array.
                                                         Dim arrayName As String = $"{name}{args.Parameters(0).Evaluate()}"
                                                         If mCustomParameters.ContainsKey(arrayName) Then
                                                             args.Result = mCustomParameters(arrayName)
                                                         Else
                                                             If mCustomFunction IsNot Nothing Then
                                                                 mCustomFunction(name, args)
                                                             End If
                                                         End If
                                                 End Select
                                             End Sub

            AddHandler exp.EvaluateParameter, Sub(name As String, args As ParameterArgs)
                                                  Select Case name
                                                      Case "Pi"
                                                          args.Result = Math.PI
                                                      Case "e"
                                                          args.Result = Math.E
                                                      Case "t"
                                                          Dim t = Now.Ticks Mod 1000
                                                          args.Result = If(t < 500, t - 250, 500 - t + 250)
                                                      Case Else
                                                          If mCustomParameters.ContainsKey(name) Then
                                                              args.Result = mCustomParameters(name)
                                                          End If
                                                  End Select
                                              End Sub
        End Set
    End Property

    Public ReadOnly Property Variables As Dictionary(Of String, Object)
        Get
            If exp Is Nothing Then
                Return Nothing
            Else
                Return exp.Parameters
            End If
        End Get
    End Property

    Public ReadOnly Property CustomParameters As Dictionary(Of String, Double)
        Get
            Return mCustomParameters
        End Get
    End Property

    Public Function Evaluate() As Double
        If exp Is Nothing Then
            Return 0
        Else
            Return exp.Evaluate()
        End If
    End Function

    Public Function Evaluate(xValue As Double) As Double
        If exp Is Nothing Then
            Return 0
        Else
            mCustomParameters("x") = xValue
            Return exp.Evaluate()
        End If
    End Function
End Class