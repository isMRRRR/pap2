// stdafx.h : ��׼ϵͳ�����ļ��İ����ļ���
// ���Ǿ���ʹ�õ��������ĵ�
// �ض�����Ŀ�İ����ļ�
//

#pragma once

#ifndef _WIN32_WINNT		// ����ʹ���ض��� Windows XP ����߰汾�Ĺ��ܡ�
#define _WIN32_WINNT 0x0501	// ����ֵ����Ϊ��Ӧ��ֵ���������� Windows �������汾��
#endif						

#define	_USE_32BIT_TIME_T

#include <stdio.h>
#include <winsock2.h>
#include <windows.h>
#include <Windows.h>

#include "KGPublic.h"
#include "SO3GlobalDef.h"
#include "Engine.h"

#include "../include/KMathStressTestDef.h"
#include "Test.h"

// TODO: Ҫ���Եİ汾

#ifdef TEST_NEW_VERSION

#include "KMath.1.20.h"
#define VERSION NEW_VERSION
#pragma comment(lib, "SO3WorldServerD.1.53.lib")

#else

#include "KMath.1.16.h"
#define VERSION OLD_VERSION 
#pragma comment(lib, "SO3WorldServerD.1.46.lib")

#endif