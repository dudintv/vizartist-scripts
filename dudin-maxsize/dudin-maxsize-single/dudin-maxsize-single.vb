RegisterPluginVersion(1, 0, 0)

' --- Plugin documentation and metadata ---
Dim info = "Single Container MaxSize
Developer: Dmitry Dudin, dudin.tv
Scales down the current container if its width exceeds a target limit.
Supports Numeric Width, Reference Container, and Distance Between Containers modes with optional padding."

' --- Radio button option definitions ---
Dim buttonNamesTargetMode As Array[String]
buttonNamesTargetMode.Push("Numeric Width")
buttonNamesTargetMode.Push("Reference Container")
buttonNamesTargetMode.Push("Distance Between Containers")
Dim MODE_NUMERIC As Integer = 0
Dim MODE_CONTAINER As Integer = 1
Dim MODE_DISTANCE As Integer = 2

' --- Interactive action constants ---
Dim BUTTON_INIT_SCALE As Integer = 1

' --- Scene references ---
Dim cSelf As Container

'------------------------------------------------
Sub OnInitParameters()
    RegisterPluginVersion(1, 0, 0)
    RegisterInfoText(info)
    
    ' Default scale settings
    RegisterParameterDouble("def_x", "Default x-scale", 1.0, 0.0, 100.0)
    RegisterParameterDouble("def_y", "Default y-scale", 1.0, 0.0, 100.0)
    
    ' Target configuration
    RegisterRadioButton("target_mode", "Max size mode", MODE_NUMERIC, buttonNamesTargetMode)
    RegisterParameterDouble("target_num", "└ Max width", 1000.0, 0.0, 9999.0)
    RegisterParameterContainer("target_cont", "└ Target Container:")
    RegisterParameterContainer("target_dist_a", "└ Distance Container A:")
    RegisterParameterContainer("target_dist_b", "└ Distance Container B:")
    
    ' Padding settings (only for Mode 2 and Mode 3)
    RegisterParameterBool("sync_pad", "└ Sync Left/Right Padding", true)
    RegisterParameterDouble("pad_left", "└ Padding (Left / Both)", 0.0, -9999.0, 9999.0)
    RegisterParameterDouble("pad_right", "   └ Padding Right", 0.0, -9999.0, 9999.0)
    
    ' Axis Mode
    RegisterParameterBool("prop_scale", "Scale y (Proportional 2D)", false)
    
    ' Action Button
    RegisterPushButton("btn_init", "Initialize text scale", BUTTON_INIT_SCALE)
End Sub

Sub OnInit()
    cSelf = this
    UpdateGuiVisibility()
End Sub

Sub OnParameterChanged(parameterName As String)
    cSelf = this
    
    ' Keep right padding value synced with left when synchronization is enabled
    If GetParameterBool("sync_pad") Then
        If parameterName == "pad_left" Or parameterName == "sync_pad" Then
            this.ScriptPluginInstance.SetParameterDouble("pad_right", GetParameterDouble("pad_left"))
        End If
    End If
    
    UpdateGuiVisibility()
End Sub

Sub UpdateGuiVisibility()
    Dim mode As Integer = GetParameterInt("target_mode")
    Dim isSync As Boolean = GetParameterBool("sync_pad")
    
    Select Case mode
        Case MODE_NUMERIC
            SendGuiParameterShow("target_num", SHOW)
            SendGuiParameterShow("target_cont", HIDE)
            SendGuiParameterShow("target_dist_a", HIDE)
            SendGuiParameterShow("target_dist_b", HIDE)
            SendGuiParameterShow("sync_pad", HIDE)
            SendGuiParameterShow("pad_left", HIDE)
            SendGuiParameterShow("pad_right", HIDE)
        Case MODE_CONTAINER
            SendGuiParameterShow("target_num", HIDE)
            SendGuiParameterShow("target_cont", SHOW)
            SendGuiParameterShow("target_dist_a", HIDE)
            SendGuiParameterShow("target_dist_b", HIDE)
            SendGuiParameterShow("sync_pad", SHOW)
            SendGuiParameterShow("pad_left", SHOW)
            SendGuiParameterShow("pad_right", CInt(Not isSync))
        Case MODE_DISTANCE
            SendGuiParameterShow("target_num", HIDE)
            SendGuiParameterShow("target_cont", HIDE)
            SendGuiParameterShow("target_dist_a", SHOW)
            SendGuiParameterShow("target_dist_b", SHOW)
            SendGuiParameterShow("sync_pad", SHOW)
            SendGuiParameterShow("pad_left", SHOW)
            SendGuiParameterShow("pad_right", CInt(Not isSync))
    End Select
End Sub

Sub OnExecAction(buttonId As Integer)
    If buttonId == BUTTON_INIT_SCALE Then
        If CheckValidity(cSelf) Then
            cSelf.Scaling.x = GetParameterDouble("def_x")
            cSelf.Scaling.y = GetParameterDouble("def_y")
        End If
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

' Finds the local coordinate space where the current container resides
Function GetReferenceSpaceContainer() As Container
    If CheckValidity(cSelf) Then
        Dim p As Container = cSelf.ParentContainer
        If p <> Null Then
            GetReferenceSpaceContainer = p
            Exit Function
        End If
    End If
    GetReferenceSpaceContainer = this
End Function

' Calculates exact visual width of a container projected into the reference coordinate space
Function GetWidthInRefSpace(_c As Container, _ref As Container) As Double
    Dim v1, v2 As Vertex
    _c.GetTransformedBoundingBox(v1, v2)
    Dim localV1 As Vertex = _ref.WorldPosToLocalPos(v1)
    Dim localV2 As Vertex = _ref.WorldPosToLocalPos(v2)
    GetWidthInRefSpace = Abs(localV2.x - localV1.x)
End Function

Function GetRawTargetWidth(_refSpace As Container) As Double
    Dim mode As Integer = GetParameterInt("target_mode")
    
    If mode == MODE_NUMERIC Then
        GetRawTargetWidth = GetParameterDouble("target_num")
        Exit Function
    End If
    
    If mode == MODE_CONTAINER Then
        Dim tc As Container = GetParameterContainer("target_cont")
        If CheckValidity(tc) == false Then
            GetRawTargetWidth = 0.0
            Exit Function
        End If
        
        tc.RecomputeMatrix()
        GetRawTargetWidth = GetWidthInRefSpace(tc, _refSpace)
        Exit Function
    End If
    
    If mode == MODE_DISTANCE Then
        Dim cA As Container = GetParameterContainer("target_dist_a")
        Dim cB As Container = GetParameterContainer("target_dist_b")
        If CheckValidity(cA) == false Or CheckValidity(cB) == false Then
            GetRawTargetWidth = 0.0
            Exit Function
        End If
        
        cA.RecomputeMatrix()
        cB.RecomputeMatrix()
        
        Dim vA1, vA2 As Vertex
        cA.GetTransformedBoundingBox(vA1, vA2)
        
        Dim vB1, vB2 As Vertex
        cB.GetTransformedBoundingBox(vB1, vB2)
        
        ' Convert global bounding box vertices to local parent space
        Dim localA1 As Vertex = _refSpace.WorldPosToLocalPos(vA1)
        Dim localA2 As Vertex = _refSpace.WorldPosToLocalPos(vA2)
        Dim localB1 As Vertex = _refSpace.WorldPosToLocalPos(vB1)
        Dim localB2 As Vertex = _refSpace.WorldPosToLocalPos(vB2)
        
        ' Robustly find the left (min) and right (max) edges of both containers
        Dim aMinX As Double = Min(localA1.x, localA2.x)
        Dim aMaxX As Double = Max(localA1.x, localA2.x)
        Dim bMinX As Double = Min(localB1.x, localB2.x)
        Dim bMaxX As Double = Max(localB1.x, localB2.x)
        
        ' Calculate distance between the inner edges
        If aMaxX < bMinX Then
            GetRawTargetWidth = bMinX - aMaxX
        ElseIf bMaxX < aMinX Then
            GetRawTargetWidth = aMinX - bMaxX
        Else
            GetRawTargetWidth = 0.0
        End If
        
        Exit Function
    End If
    
    GetRawTargetWidth = 0.0
End Function

'------------------------------------------------
Sub OnExecPerField()
    If CheckValidity(cSelf) == false Then Exit Sub
    
    Dim refSpace As Container = GetReferenceSpaceContainer()
    refSpace.RecomputeMatrix()
    
    Dim rawTargetW As Double = GetRawTargetWidth(refSpace)
    If rawTargetW <= 0.0 Then Exit Sub
    
    ' Calculate total padding for Reference Container and Distance modes
    Dim mode As Integer = GetParameterInt("target_mode")
    Dim totalPadding As Double = 0.0
    
    If mode == MODE_CONTAINER Or mode == MODE_DISTANCE Then
        Dim padL As Double = GetParameterDouble("pad_left")
        Dim padR As Double = padL
        If GetParameterBool("sync_pad") == false Then
            padR = GetParameterDouble("pad_right")
        End If
        totalPadding = padL + padR
    End If
    
    Dim effectiveTargetW As Double = rawTargetW - totalPadding
    
    Dim defX As Double = GetParameterDouble("def_x")
    Dim defY As Double = GetParameterDouble("def_y")
    
    ' PRE-PASS: Temporarily set container to default scale to read its true projected width
    cSelf.Scaling.x = defX
    cSelf.Scaling.y = defY
    cSelf.RecomputeMatrix()
    
    Dim currentW As Double = GetWidthInRefSpace(cSelf, refSpace)
    If currentW <= 0.0 Then Exit Sub
    
    Dim targetScaleX As Double = defX
    Dim targetScaleY As Double = defY
    
    ' Compress container if its default width exceeds the effective available space
    If currentW > effectiveTargetW Then
        ' Guard clause to prevent negative scale if padding consumes all target space
        If effectiveTargetW > 0.0 Then
            Dim ratio As Double = effectiveTargetW / currentW
            targetScaleX = defX * ratio
            
            If GetParameterBool("prop_scale") Then
                targetScaleY = defY * ratio
            End If
        Else
            targetScaleX = 0.001
            If GetParameterBool("prop_scale") Then targetScaleY = 0.001
        End If
    End If
    
    ' Apply calculated final scale
    cSelf.Scaling.x = targetScaleX
    cSelf.Scaling.y = targetScaleY
End Sub
