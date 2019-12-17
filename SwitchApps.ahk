#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Event  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

#SingleInstance Force
#InstallKeybdHook

/*
CharTab := Chr(9)

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

	; Number := Ord(UserInput)
	; MsgBox, %Number%

	; MsgBox, %UserInput%

	CharTab2 := Chr(9)
	if (UserInput = CharTab2 or UserInput = "c") {
		; MsgBox, It's CharTab1
		if (UserInput = CharTab2) {
			; MsgBox, It's CharTab2
			Send #t
			; Send {Down}
		}
		else {
			Send +#t
			; Send {Down}
		}
		CheckKeyPress2(letter)
		return
	}
	else {
		Send {Esc}
		; Send {Esc}
		; MsgBox, Send Esc
		return
	}
	return
}

Callback10()
{
	Send {Enter}{Enter}
	return
}

$!Tab::
; Send #t
SetKeyDelay, 200
Send abcdef
; Send #t
; Send {Right}
SetKeyDelay, 200
Send {Down}
SetKeyDelay, 10

; Number2 := Ord(CharTab)
; MsgBox, %Number2%

HotKey, !Tab, Off
; HotKey, !c, Off
HotKey, LAlt Up, Callback10
HotKey, LAlt Up, On
CheckKeyPress2(CharTab)
HotKey, !Tab, On
; HotKey, !c, On
HotKey, LAlt Up, Off
; KeyHistory
return
*/

/*
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
*/

/*
!::
return
*/

/*
o::
MsgBox, Alt + Tab activated
return
*/


CheckKeyPress(letter)
{
	if (GetKeyState("LAlt") = 0) {
		Send {Enter}
		return
	}
	
	Input, UserInput, T1 L1 M,,
	
	if (ErrorLevel = "Timeout") {
		if (GetKeyState("LAlt") = 0) {
			return
		}
	 	else {
	 		CheckKeyPress(letter)
			return
	 	}
	}

	CharTab_local := Chr(9)
	if (UserInput = CharTab_local or UserInput = "c") {
		if (UserInput = CharTab_local) {
			Send {Up}
			Send #t
			Send {Down}
		}
		else {
			Send {Up}
			Send +#t
			Send {Down}
		}
		CheckKeyPress(letter)
		return
	}
	else {

		; Number3 := Ord(UserInput)
		; MsgBox, %Number3%
		
		CharNull_local := Chr(0)
		CharLineFeed_local := Chr(10)
		If (UserInput = CharNull_local or UserInput = CharLineFeed_local) {
			Send {Enter}
			return
		}
		else {
			MsgBox, how the fuck
			Send {Esc}
			Send {Esc}
			return
		}
	}
	return
}

Callback()
{
	; Sleep, 500
	Send {Enter}
	return
}




$!Tab::
Send #t
Send {Down}

HotKey, !Tab, Off

HotKey, LAlt Up, Callback
HotKey, LAlt Up, On

CharTab := Chr(9)
CheckKeyPress(CharTab)

HotKey, LAlt Up, Off

HotKey, !Tab, On

return



/*
$!Tab::
Input, UserInput, T1 L1 M,,
Number := Ord(UserInput)
MsgBox, %Number%
return
*/








/*
Send {LWin down}
Sleep, 200
Send t
Send {LWin up}

Send {Down}
*/

/*
Sleep, 1000
Send {Down}

Sleep, 1000

Send #t
Sleep, 1000
Send {Down}
*/

/*
Input, UserInput, L1 M,,

	CharTab2 := Chr(9)
	if (UserInput = CharTab2 or UserInput = "c") {
		; MsgBox, It's CharTab1
		if (UserInput = CharTab2) {
			Send #t
			; Send {Down}
		}
		else {
			Send +#t
			; Send {Down}
		}
		CheckKeyPress2(letter)
		return
	}
	else {
		Send {Esc}
		; Send {Esc}
		; MsgBox, Send Esc
		return
	}
	return
}
*/
; Send abcdef
; return