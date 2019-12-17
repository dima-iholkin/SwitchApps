#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Event  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

#SingleInstance Force
#InstallKeybdHook

; LAlt::return

CheckKeyPress2(letter)
{
	if (GetKeyState("LAlt") = 0) {
		; Send {Enter}
		return
	}
	
	Input, UserInput, T1 L1 M,,
	
	if (ErrorLevel = "Timeout") {
		if (GetKeyState("LAlt") = 0) {
			return
		}
	 	else {
	 		CheckKeyPress2(letter)
			return
	 	}
	}
	
	if (UserInput = "v" or UserInput = "c") {
		if (UserInput = "v") {
			Send #t
		}
		else {
			Send +#t
		}
		CheckKeyPress2(letter)
		return
	}
	else {
		Send {Esc}
		return
	}
	return
}

Callback10()
{
	Send {Enter}{Enter}
	return
}

$!v::
Send #t
Send {Right}
HotKey, !v, Off
HotKey, !c, Off
HotKey, LAlt Up, Callback10
HotKey, LAlt Up, On
CheckKeyPress2("v")
HotKey, !v, On
HotKey, !c, On
HotKey, LAlt Up, Off
return

$!c::
Send +#t
Send {Left}
HotKey, !v, Off
HotKey, !c, Off
HotKey, LAlt Up, Callback10
HotKey, LAlt Up, On
CheckKeyPress2("c")
HotKey, !v, On
HotKey, !c, On
HotKey, LAlt Up, Off
return















; Callback11()
; {
; 	; MsgBox, something
; 	if (GetKeyState("LAlt") = 0) {
; 		Send {Enter}
; 		KeyHistory
; 	}
; }


; Number := Asc(%UserInput%)
; MsgBox, %Number%
; KeyHistory

; SetTimer, F24, -1
; HotKey, #x, Off
; KeyWait LWin
; MsgBox, Win released

/*
F24::
MsgBox, in Callback
Input, UserInput, L1, ,
MsgBox, %UserInput%
KeyHistory
HotKey, #x, On
return
*/

/*
CheckKeyPress()
{
	; SetKeyDelay 10
	if (GetKeyState("LAlt") = 0) {
		Send {Enter}
	}
	else {
		; MsgBox, Waiting for input
		Input, UserInput, L1 M, ,
		; MsgBox, %UserInput%
		if (UserInput = "v") {
			Send #t							; or Send t
			CheckKeyPress()
		}
		else {
			Send {Esc}
		}
	}
}
*/

; MatchList = x,{LWin up}

/*
Callback()
{
	if (GetKeyState("LAlt") = 0) {
		
		Send {Enter}
	}
}
*/

; Callback2:
; if (GetKeyState("LAlt") = 0) {
; 	; SetTimer, "Callback2", Delete
; 	Send {Enter}
; 	KeyHistory
; }
; return


	; if (ErrorLevel = "Timeout") {
	; 	if (GetKeyState("LAlt") = 0) {
	; 		return
	; 	}
	; 	else {
	; 		CheckKeyPress2()
	; 	}
	; }
	
	
/*
CheckKeyPress2(letter)
{
	; MsgBox, CheckKeyPress2 runs
	if (GetKeyState("LAlt") = 0) {
		; Send {Enter}
		; MsgBox, return from CheckKeyPress2
		return
	}
	
	Input, UserInput, T1 L1 M,
	
	if (ErrorLevel = "Timeout") {
		; MsgBox, Error in there
		if (GetKeyState("LAlt") = 0) {
			; MsgBox, return from Error
			return
		}
	 	else {
			MsgBox, execute 1
	 		CheckKeyPress2(letter)
			MsgBox, execute 2
			return
	 	}
	}
	
	MsgBox, we have some input %UserInput%
	if (UserInput = %letter%) {
		; MsgBox, %letter%
		if (UserInput = "v") {
			Send #t
		}
		else {
			Send +#t
		}
		; MsgBox, Send Win + T
		CheckKeyPress2(letter)
		return
	}
	else {
		; Send {Esc}
		; MsgBox, send Esc
		return
	}
	return
}
*/