#pragma once
#include "UiWndWindow.h"

class KUiWndWindowCommon : public KUiWndWindow
{
public:
	static KUiWndWindow*	Create();
	virtual int             Init(HWND hWnd, RECT* pRc, const char *pszName);
	virtual void			Release();
	virtual UI_WND_TYPE		Type() { return UI_WND_WINDOW; }
	virtual void			UpdateData(const char* pszName, KUiWndWindowData& data, IIniFile* pIni);
	virtual void			Show(int bShow, int nSubWnd = -1);
    virtual void            FillPageInfomation(KUIWNDPAGEINFOMATION *pPageInfo);

protected:
	KUiWndWindowCommon();
	~KUiWndWindowCommon();

	static	LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

private:
	void OnButtonClick(int nBtnId);
	void OnUpdateEditText(int nId);
    void OnUpdataComboBox(int nId);
	
	char* EquirotalStrStr(const char *pString, const char *pStrSearch);
	int OnChangeArea();
};