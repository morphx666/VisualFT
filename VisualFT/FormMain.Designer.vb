<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.LabelFormula = New System.Windows.Forms.Label()
        Me.TextBoxFormula = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'LabelFormula
        '
        Me.LabelFormula.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LabelFormula.AutoEllipsis = True
        Me.LabelFormula.BackColor = System.Drawing.Color.FromArgb(CType(CType(33, Byte), Integer), CType(CType(33, Byte), Integer), CType(CType(33, Byte), Integer))
        Me.LabelFormula.Cursor = System.Windows.Forms.Cursors.Hand
        Me.LabelFormula.Font = New System.Drawing.Font("Consolas", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelFormula.ForeColor = System.Drawing.Color.CadetBlue
        Me.LabelFormula.Location = New System.Drawing.Point(505, 9)
        Me.LabelFormula.Name = "LabelFormula"
        Me.LabelFormula.Size = New System.Drawing.Size(312, 22)
        Me.LabelFormula.TabIndex = 0
        Me.LabelFormula.Text = "Label1"
        Me.LabelFormula.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TextBoxFormula
        '
        Me.TextBoxFormula.AcceptsReturn = True
        Me.TextBoxFormula.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxFormula.BackColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(31, Byte), Integer), CType(CType(31, Byte), Integer))
        Me.TextBoxFormula.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TextBoxFormula.Font = New System.Drawing.Font("Consolas", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBoxFormula.ForeColor = System.Drawing.Color.CadetBlue
        Me.TextBoxFormula.Location = New System.Drawing.Point(505, 88)
        Me.TextBoxFormula.Name = "TextBoxFormula"
        Me.TextBoxFormula.Size = New System.Drawing.Size(312, 19)
        Me.TextBoxFormula.TabIndex = 1
        Me.TextBoxFormula.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.TextBoxFormula.Visible = False
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 18.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(829, 806)
        Me.Controls.Add(Me.TextBoxFormula)
        Me.Controls.Add(Me.LabelFormula)
        Me.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "FormMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Visual Fourier Transform"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LabelFormula As Label
    Friend WithEvents TextBoxFormula As TextBox
End Class
