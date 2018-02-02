Imports NCalc

Public Class Evaluator
    Public Const Infinity As Double = 10 ^ 6
    Public Const ToRad As Double = Math.PI / 180

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
                                                         args.Result = args.Parameters(0).Evaluate() * ToRad
                                                     Case "Abs"
                                                         args.Result = Math.Abs(args.Parameters(0).Evaluate())
                                                     Case "Rnd"
                                                         args.Result = (New Random()).NextDouble()
                                                 End Select
                                             End Sub

            AddHandler exp.EvaluateParameter, Sub(name As String, args As ParameterArgs)
                                                  Select Case name
                                                      Case "Pi"
                                                          args.Result = Math.PI
                                                      Case "e"
                                                          args.Result = Math.E
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
            Try
                Dim result As Double = exp.Evaluate()
                If Double.IsInfinity(result) Then
                    Return Infinity
                Else
                    Return result
                End If
            Catch ex As OverflowException
                Return Infinity
            End Try
        End If
    End Function
End Class