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

  CharTab_local := Chr(9)

  if (UserInput = CharTab_local or UserInput = "c") {
    if (UserInput = CharTab_local) {
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
    }
    else {
      ; Send {Esc}
      Send +#t
      ; Send {Down}
    }
    CheckKeyPress(letter)
    return
  }
  else {
    CharNull_local := Chr(0)
    CharLineFeed_local := Chr(10)
    If (UserInput = CharNull_local or UserInput = CharLineFeed_local) {
      Send {Enter}
      return
    }
    else {
      Send {Esc}
      Send {Esc}
      MsgBox, how the fuck
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



; $!#+Tab::
$!Tab::
$!+Tab::
  ; I wanted to reduce the number of flashy animations.
  ; Send #d
  ; Sleep 200

  Send #t
  ; Send {Down}

  HotKey, !Tab, Off
  HotKey, !+Tab, Off

  HotKey, LAlt Up, Callback
  HotKey, LAlt Up, On

  CharTab := Chr(9)
  CheckKeyPress(CharTab)

  HotKey, LAlt Up, Off

  HotKey, !Tab, On
  HotKey, !+Tab, On

return