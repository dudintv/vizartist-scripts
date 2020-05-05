RegisterPluginVersion(1,0,0)
Dim count_time_periods As Integer = 3

sub OnInitParameters()
	for i=1 to count_time_periods
		RegisterParameterString("period_begin_" & i, "Period begin #" & i, "00:00", 7, 5, "0123456789:")
	next
end sub

Dim timeperiodbegins As Array[Integer]

sub OnInit()
	Dim s As String
	Dim arr_s As Array[String]
	timeperiodbegins.clear()
	for i=1 to count_time_periods
		s = GetParameterString("period_begin_" & i)
		s.Trim()
		s.Split(":", arr_s)
		
		' get total count minutes from begin of the day, to make comparison much easier
		timeperiodbegins.Push(CInt(arr_s[0])*60 + CInt(arr_s[1]))
	next
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

Dim dt As DateTime
Dim current_count_minutes_from_begin_of_today As Integer
Dim current_period As Integer

sub OnExecPerField()
	dt = GetCurrentTime()
	current_count_minutes_from_begin_of_today = dt.Hour*60 + dt.Minute
	
	' lets find currect time period
	current_period = timeperiodbegins.ubound
	for i=timeperiodbegins.ubound to 0 step -1
		if current_count_minutes_from_begin_of_today >= timeperiodbegins[i] then
			current_period = i
			exit for
		end if
	next
	
	this.ShowOneChildContainer(current_period)
end sub
