////////////////////////////////////////////////////////////////////////////////
//
//  FileName    : IEKG3DPointLight.h
//  Version     : 1.0
//  Creator     : Chen Tianhong
//  Create Date : 2008-3-5 16:54:44
//  Comment     : 
//
////////////////////////////////////////////////////////////////////////////////

#ifndef _INCLUDE_IEKG3DPOINTLIGHT_H_
#define _INCLUDE_IEKG3DPOINTLIGHT_H_
#include "IEKG3DCommonObject.h"
////////////////////////////////////////////////////////////////////////////////
struct IEKG3DPointLight
{
	STDMETHOD_(ULONG, GetType)() = 0;
};

#endif //_INCLUDE_IEKG3DPOINTLIGHT_H_
