// trustDll.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//
#include "stdafx.h"
#include <windows.h>
#include "trustDll.h"

HHOOK hookCbt = NULL;
HHOOK hookShell = NULL;
HHOOK hookCallWndProc = NULL;
HHOOK hookGetMsg = NULL;

// DLLMainに設定されるインスタンス
HINSTANCE gAppInstance = NULL;

typedef void (CALLBACK *HookProc)(int code, WPARAM w, LPARAM l);

static LRESULT CALLBACK CbtHookCallback(int code, WPARAM wparam, LPARAM lparam);
static LRESULT CALLBACK ShellHookCallback(int code, WPARAM wparam, LPARAM lparam);
static LRESULT CALLBACK CallWndProcHookCallback(int code, WPARAM wparam, LPARAM lparam);
static LRESULT CALLBACK GetMsgHookCallback(int code, WPARAM wparam, LPARAM lparam);

#pragma region ■CbtHook定義■
bool InitializeCbtHook(int threadID, HWND destination)
{
	if (gAppInstance == NULL)
	{
		return false;
	}

	if (GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CBT") != NULL)
	{
		SendNotifyMessage((HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CBT"), 
			RegisterWindowMessage("I_CLICK_HOOK_CBT_REPLACED"), 0, 0);
	}

	SetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CBT", destination);

	hookCbt = SetWindowsHookEx(WH_CBT, (HOOKPROC)CbtHookCallback, gAppInstance, threadID);
	return hookCbt != NULL;
}

void UninitializeCbtHook()
{
	if (hookCbt != NULL)
	{
		UnhookWindowsHookEx(hookCbt);
	}
	hookCbt = NULL;
}

static LRESULT CALLBACK CbtHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code >= 0)
	{
		UINT msg = 0;

		if (code == HCBT_ACTIVATE)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_ACTIVATE");
		else if (code == HCBT_CREATEWND)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_CREATEWND");
		else if (code == HCBT_DESTROYWND)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_DESTROYWND");
		else if (code == HCBT_MINMAX)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_MINMAX");
		else if (code == HCBT_MOVESIZE)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_MOVESIZE");
		else if (code == HCBT_SETFOCUS)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_SETFOCUS");
		else if (code == HCBT_SYSCOMMAND)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HCBT_SYSCOMMAND");

		HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CBT");

		if (msg != 0)
			SendNotifyMessage(dstWnd, msg, wparam, lparam);
	}

	return CallNextHookEx(hookCbt, code, wparam, lparam);
}
#pragma endregion

#pragma region ■ShellHook定義■
bool InitializeShellHook(int threadID, HWND destination)
{
	if (gAppInstance == NULL)
	{
		return false;
	}

	if (GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_SHELL") != NULL)
	{
		SendNotifyMessage((HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_SHELL"), 
			RegisterWindowMessage("I_CLICK_HOOK_SHELL_REPLACED"), 0, 0);
	}

	SetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_SHELL", destination);

	hookShell = SetWindowsHookEx(WH_SHELL, (HOOKPROC)ShellHookCallback, gAppInstance, threadID);
	return hookShell != NULL;
}

void UninitializeShellHook()
{
	if (hookShell != NULL)
		UnhookWindowsHookEx(hookShell);
	hookShell = NULL;
}

static LRESULT CALLBACK ShellHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code >= 0)
	{
		UINT msg = 0;

		if (code == HSHELL_ACTIVATESHELLWINDOW)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_ACTIVATESHELLWINDOW");
		else if (code == HSHELL_GETMINRECT)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_GETMINRECT");
		else if (code == HSHELL_LANGUAGE)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_LANGUAGE");
		else if (code == HSHELL_REDRAW)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_REDRAW");
		else if (code == HSHELL_TASKMAN)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_TASKMAN");
		else if (code == HSHELL_WINDOWACTIVATED)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWACTIVATED");
		else if (code == HSHELL_WINDOWCREATED)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWCREATED");
		else if (code == HSHELL_WINDOWDESTROYED)
			msg = RegisterWindowMessage("I_CLICK_HOOK_HSHELL_WINDOWDESTROYED");

		HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_SHELL");

		if (msg != 0)
			SendNotifyMessage(dstWnd, msg, wparam, lparam);
	}

	return CallNextHookEx(hookShell, code, wparam, lparam);
}
#pragma endregion

#pragma region ■CallWndProcHook定義■
bool InitializeCallWndProcHook(int threadID, HWND destination)
{
	if (gAppInstance == NULL)
	{
		return false;
	}

	if (GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CALLWNDPROC") != NULL)
	{
		SendNotifyMessage((HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CALLWNDPROC"), 
			RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC_REPLACED"), 0, 0);
	}

	SetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CALLWNDPROC", destination);

	hookCallWndProc = SetWindowsHookEx(WH_CALLWNDPROC, (HOOKPROC)CallWndProcHookCallback, gAppInstance, threadID);
	return hookCallWndProc != NULL;
}

void UninitializeCallWndProcHook()
{
	if (hookCallWndProc != NULL)
		UnhookWindowsHookEx(hookCallWndProc);
	hookCallWndProc = NULL;
}

static LRESULT CALLBACK CallWndProcHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code >= 0)
	{
		UINT msg = 0;
		UINT msg2 = 0;

		msg = RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC");
		msg2 = RegisterWindowMessage("I_CLICK_HOOK_CALLWNDPROC_PARAMS");

		HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_CALLWNDPROC");

		CWPSTRUCT* pCwpStruct = (CWPSTRUCT*)lparam;

		if (msg != 0 && pCwpStruct->message != msg && pCwpStruct->message != msg2)
		{
			SendNotifyMessage(dstWnd, msg, (WPARAM)pCwpStruct->hwnd, pCwpStruct->message);
			SendNotifyMessage(dstWnd, msg2, pCwpStruct->wParam, pCwpStruct->lParam);
		}
	}

	return CallNextHookEx(hookCallWndProc, code, wparam, lparam);
}
#pragma endregion

#pragma region ■GetMsgHook定義■
bool InitializeGetMsgHook(int threadID, HWND destination)
{
	if (gAppInstance == NULL)
	{
		return false;
	}

	if (GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_GETMSG") != NULL)
	{
		SendNotifyMessage((HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_GETMSG"), 
			RegisterWindowMessage("I_CLICK_HOOK_GETMSG_REPLACED"), 0, 0);
	}

	SetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_GETMSG", destination);

	hookGetMsg = SetWindowsHookEx(WH_GETMESSAGE, (HOOKPROC)GetMsgHookCallback, gAppInstance, threadID);
	return hookGetMsg != NULL;
}

void UninitializeGetMsgHook()
{
	if (hookGetMsg != NULL)
		UnhookWindowsHookEx(hookGetMsg);
	hookGetMsg = NULL;
}

static LRESULT CALLBACK GetMsgHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code >= 0)
	{
		UINT msg = 0;
		UINT msg2 = 0;

		msg = RegisterWindowMessage("I_CLICK_HOOK_GETMSG");
		msg2 = RegisterWindowMessage("I_CLICK_HOOK_GETMSG_PARAMS");

		HWND dstWnd = (HWND)GetProp(GetDesktopWindow(), "I_CLICK_HOOK_HWND_GETMSG");

		MSG* pMsg = (MSG*)lparam;

		if (msg != 0 && pMsg->message != msg && pMsg->message != msg2)
		{
			SendNotifyMessage(dstWnd, msg, (WPARAM)pMsg->hwnd, pMsg->message);
			SendNotifyMessage(dstWnd, msg2, pMsg->wParam, pMsg->lParam);
		}
	}

	return CallNextHookEx(hookGetMsg, code, wparam, lparam);
}
#pragma endregion
