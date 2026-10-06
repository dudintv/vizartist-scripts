RegisterPluginVersion(1, 0, 0)

' --- Plugin documentation and metadata ---
Dim info = "Dynamic MaxSize
Scales down multiple containers equally if their total width exceeds a target.
To change the number of available containers, modify the QUANTITY_OF_CONTAINERS constant.
Based on the builtin MaxSize logic with default scale values."

' --- Scalable capacity limit ---
Dim QUANTITY_OF_CONTAINERS As Integer = 2

' --- Radio button option definitions ---
Dim targetModes As Array[String]
targetModes.Push("Numeric Width")
targetModes.Push("Reference Container")
Dim MODE_NUMERIC As Integer = 0
Dim MODE_CONTAINER As Integer = 1

' --- Interactive action constants ---
Dim BUTTON_INIT_SCALE As Integer = 1

' --- Runtime state arrays ---
Dim cTargets As Array[Container]

'------------------------------------------------
Sub OnInitParameters()
    RegisterPluginVersion(1, 0, 0)
    RegisterInfoText(info)
    
    ' Default scale settings
    RegisterParameterDouble("def_x", "Default x-scale", 1.0, 0.0, 100.0)
    RegisterParameterDouble("def_y", "Default y-scale", 1.0, 0.0, 100.0)
    
    ' Target configuration
    RegisterRadioButton("target_mode", "Max size", MODE_NUMERIC, targetModes)
    RegisterParameterDouble("target_num", "└ Max width", 1000.0, 0.0, 9999.0)
    RegisterParameterContainer("target_cont", "└ Target Container:")
    
    ' Axis Mode
    RegisterParameterBool("prop_scale", "Proportional Scale y (2D)", false)
    
    ' Dynamic Dropzones
    Dim i As Integer
    For i = 1 To QUANTITY_OF_CONTAINERS
        RegisterParameterContainer("c" & i, "Container " & i & ":")
    Next
    
    ' Action Button
    RegisterPushButton("btn_init", "Initialize text scale", BUTTON_INIT_SCALE)
End Sub

Sub OnInit()
    cTargets.Clear()
    Dim i As Integer
    For i = 1 To QUANTITY_OF_CONTAINERS
        Dim c As Container = GetParameterContainer("c" & i)
        If c <> Null Then cTargets.Push(c)
    Next
End Sub

Sub OnParameterChanged(parameterName As String)
    OnInit()
    
    ' Switch Case for dynamic UI visibility based on the chosen mode
    Select Case GetParameterInt("target_mode")
        Case MODE_NUMERIC
            SendGuiParameterShow("target_num", SHOW)
            SendGuiParameterShow("target_cont", HIDE)
        Case MODE_CONTAINER
            SendGuiParameterShow("target_num", HIDE)
            SendGuiParameterShow("target_cont", SHOW)
    End Select
End Sub

Sub OnExecAction(buttonId As Integer)
    If buttonId == BUTTON_INIT_SCALE Then
        ' Reset scale to defaults instantly
        Dim i As Integer
        For i = 0 To cTargets.Size - 1
            Dim c As Container = cTargets[i]
            If CheckValidity(c) Then
                c.Scaling.x = GetParameterDouble("def_x")
                c.Scaling.y = GetParameterDouble("def_y")
            End If
        Next
    End If
End Sub

'------------------------------------------------
' Guard clause pattern for safe container validation
Function CheckValidity(_c As Container) As Boolean
    If _c == Null Then
        CheckValidity = false
        Exit Function
    End If
    If _c.Active == false Then
        CheckValidity = false
        Exit Function
    End If
    CheckValidity = true
End Function

' Guard clause pattern for safe container width reading
Function GetLocalWidth(_c As Container) As Double
    If CheckValidity(_c) == false Then
        GetLocalWidth = 0.0
        Exit Function
    End If
    
    Dim v1, v2 As Vertex
    _c.GetBoundingBox(v1, v2)
    GetLocalWidth = v2.x - v1.x
End Function

Function GetTargetWidth() As Double
    If GetParameterInt("target_mode") == MODE_NUMERIC Then
        GetTargetWidth = GetParameterDouble("target_num")
        Exit Function
    End If
    
    Dim tc As Container = GetParameterContainer("target_cont")
    If CheckValidity(tc) == false Then
        GetTargetWidth = 0.0
        Exit Function
    End If
    
    ' We use transformed bounding box for the target to respect its own world scaling
    tc.RecomputeMatrix()
    Dim v1, v2 As Vertex
    tc.GetTransformedBoundingBox(v1, v2)
    GetTargetWidth = v2.x - v1.x
End Function

'------------------------------------------------
Sub OnExecPerField()
    Dim targetW As Double = GetTargetWidth()
    If targetW <= 0.0 Then Exit Sub
    
    Dim totalW As Double = 0.0
    Dim i As Integer
    
    ' Sum up unscaled widths of all active targets
    For i = 0 To cTargets.Size - 1
        totalW += GetLocalWidth(cTargets[i])
    Next
    
    If totalW <= 0.0 Then Exit Sub
    
    ' Read defaults
    Dim defX As Double = GetParameterDouble("def_x")
    Dim defY As Double = GetParameterDouble("def_y")
    
    Dim targetScaleX As Double = defX
    Dim targetScaleY As Double = defY
    
    ' Check if total width AT default scale exceeds target
    If (totalW * defX) > targetW Then
        targetScaleX = targetW / totalW
        
        ' If proportional scale is enabled, scale Y relative to the compression ratio of X
        If GetParameterBool("prop_scale") Then
            Dim ratio As Double = targetScaleX / defX
            targetScaleY = defY * ratio
        End If
    End If
    
    ' Apply calculated scale
    For i = 0 To cTargets.Size - 1
        Dim c As Container = cTargets[i]
        If CheckValidity(c) Then
            c.Scaling.x = targetScaleX
            c.Scaling.y = targetScaleY
        End If
    Next
End Sub
