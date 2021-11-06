; version 1.0.0

#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Event  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

#SingleInstance Force
#InstallKeybdHook

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

  if (UserInput = letter) {
    If (GetKeyState("LShift") = 1) {
      ; if LShift is pressed

      Send +#t
      ; Send Win + Shift + T. It switches to the left hand side.
    }

    If (GetKeyState("LShift") = 0) {
      ; if LShift is not pressed

      Send #t
      ; Send Win + T. It switches to the right hand side.
    }

    CheckKeyPress(letter)
    return
  }
  else {
    CharNull_local := Chr(0)
    CharLineFeed_local := Chr(10)
    If (UserInput = CharNull_local or UserInput = CharLineFeed_local) {
      Send {Up}
      Send {Enter}
      return
    }
    else {
      Send {Esc}
      Send {Esc}
      MsgBox, woops
      return
    }
  }

  return
}


Callback()
{
  Send {Up}
  Send {Enter}
  return
}


$!Tab::
$!+Tab::
  ; If we start from the Alt+Shift+Tab - the selection should be 
  ; not on the first app (the standard behaviour of Win+Shift+T)
  ; and the selection should be on the last app.

  If (GetKeyState("LShift") = 0) {
    ; if LShift is not pressed

    Send #t
    ; Send Win + T. It starts at the first app.

  } else {
    ; if LShift is pressed

    Send +#t
    ; Send Win + Shift + T the first time. 

    Send +#t
    ; Send Win + Shift + T the second time. It switches to the last app.
  }

  HotKey, !Tab, Off
  HotKey, !+Tab, Off
  ; Turn off this global handler, so it's not triggered multiple times.

  HotKey, LAlt Up, Callback
  HotKey, LAlt Up, On
  ; Lol, I'm not sure at this point why I did this. It works apparently.

  CharTab := Chr(9)
  ; The character code expected to be the right one to compare against the input to AHK,
  ; look to the next routine.

  CheckKeyPress(CharTab)
  ; Start the routine to handle the next Tab presses.

  HotKey, LAlt Up, Off
  ; I'm not sure why. Turns of the Callback, apparently. 

  HotKey, !Tab, On
  HotKey, !+Tab, On
  ; After the CheckKeyPress routine has exited, turn on the globar handler.

return