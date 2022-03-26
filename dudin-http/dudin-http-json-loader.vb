Dim hostJsonServer As String = "host.com"
Dim pathJsonServer As String = "/path/to/data"
Dim dataJsonVar As String = "dataJsonVar"
Dim statusJsonVar As String = "statusJsonVar"
Dim maxTryCount As Integer = 3

sub OnInit()
  GetJSON(hostJsonServer, pathJsonServer, dataJsonVar, statusJsonVar)
end sub

Sub GetJSON(host As String, path As String, dataVarName As String, statusVarName As String)
	Dim port = 80
	Dim query = "GET " & path & " HTTP/1.0\r\n"
	query &= "Host: " & host & ":" & port & "\r\n"
	query &= "User-Agent: Vizrt\r\n"
	query &= "Accept: application/json\r\n"
	query &= "Connection: close\r\n"
	query &= "Cache-Control: no-cache\r\n"
	query &= "\r\n"
	scene.map[dataVarName] = ""
	scene.map[statusVarName] = "NO STATUS"
	scene.map.RegisterChangedCallback(dataVarName)
	scene.map.RegisterChangedCallback(statusVarName)
	system.TcpSendAsyncStatus(dataVarName, host, port, query, 5000, statusVarName)
End Sub

Function ExtractResponseData(input As String) As String
	Dim httpHeaderSeparator = "\r\n\r\n"
	Dim headerSeparatorPos = input.Find(httpHeaderSeparator) + httpHeaderSeparator.length
	ExtractResponseData = input.GetSubstring(headerSeparatorPos, input.Length)
End Function

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey == dataJsonVar then
		println("HTTP data = " & map[mapKey])
		this.Geometry.Text = ExtractResponseData(map[mapKey])
	elseif mapKey == statusJsonVar then
		println("HTTP status = " & map[mapKey])
		if map[mapKey] == "ERROR" AND maxTryCount > 0 then
			maxTryCount -= 1
			map[mapKey] = "TRY AGAIN"
			GetJSON(hostJsonServer, pathJsonServer, dataJsonVar, statusJsonVar)
		end if
	end if
end sub
