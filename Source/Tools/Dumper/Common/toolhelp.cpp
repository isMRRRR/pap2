#include "stdafx.h"
#include "toolhelp.h"
#include "nb30.h"
#include "KSG_MD5_String.h"
#include <atlstr.h>

#pragma comment(lib,"netapi32.lib")

typedef struct _ASTAT_ 
{ 
	ADAPTER_STATUS adapt; 
	NAME_BUFFER NameBuff [30]; 
}ASTAT, * PASTAT; 


/**
* @brief		    删除字符串首尾空格
*
* @details
* 
* @param[in]        源字符串
* @return[TCHAR*]   输出字符串
* @return[0]        转换失败
* @remarks	
*/
TCHAR* trim(TCHAR* str)
{
	if( ' ' == *str )
	{
		while( ' '== *str )
		{
			str++;
			if( 0 == *str )
				return 0;
		}
		trim( str );        
		return str;
	}
	else
	{
		while ( ' ' != *str )
		{
			str++;
			if( 0 == *str )
				return 0;
		}
		if( 0 == trim( str ) )
		{
			*str=0;
			return str;
		}
		return str;
	}
}

/**
* @brief		    获取指定网卡Mac地址
*
* @details
* 
* @param[in]		指定网卡索引
* @param[out]		返回Mac地址
* @return[true]		成功
* @return[false]	失败
* @remarks			pszMac长度不能小于18
*/

bool getmac_mac(int lana_num, CString& strMac) 
{
	bool	bRet = false;
	ASTAT	Adapter = {0}; 
	NCB		ncb; 
	UCHAR	uRetCode; 

	memset( &ncb, 0, sizeof(ncb) ); 
	ncb.ncb_command = NCBRESET; 
	ncb.ncb_lana_num = lana_num; 
	// 指定网卡号 

	// 首先对选定的网卡发送一个NCBRESET命令，以便进行初始化 
	uRetCode = Netbios( &ncb ); 
	if (uRetCode != NRC_GOODRET || ncb.ncb_retcode != NRC_GOODRET)
		return false;

	memset( &ncb, 0, sizeof(ncb) ); 
	ncb.ncb_command = NCBASTAT; 
	ncb.ncb_lana_num = lana_num; // 指定网卡号 

	strcpy( (char *)ncb.ncb_callname, "*     " );
	ncb.ncb_buffer = (unsigned char *) &Adapter; 

	// 指定返回的信息存放的变量 
	ncb.ncb_length = sizeof(Adapter); 

	// 接着，可以发送NCBASTAT命令以获取网卡的信息 
	uRetCode = Netbios( &ncb ); 
	if ( uRetCode == 0 && ncb.ncb_retcode == 0) 
	{ 
		// 把网卡MAC地址格式化成常用的16进制形式，如0010-A4E4-5802 
		if (
			Adapter.adapt.adapter_address[0] == 0 &&
			Adapter.adapt.adapter_address[1] == 0 &&
			Adapter.adapt.adapter_address[2] == 0 &&
			Adapter.adapt.adapter_address[3] == 0 &&
			Adapter.adapt.adapter_address[4] == 0 &&
			Adapter.adapt.adapter_address[5] == 0
			)
		{
			return false; 
		}	

		strMac.Format("%02X-%02X-%02X-%02X-%02X-%02X\n",
			Adapter.adapt.adapter_address[0], 
			Adapter.adapt.adapter_address[1], 
			Adapter.adapt.adapter_address[2], 
			Adapter.adapt.adapter_address[3], 
			Adapter.adapt.adapter_address[4], 
			Adapter.adapt.adapter_address[5]
		);

		return true;
	} 

	return false;
}

/**
* @brief		    获取所有网卡MAC地址
*
* @details
* 
* @param[out]       Mac字符串
* @return[int]		网卡数
* @remarks			网卡数小于等于0则表示失败
*/
int getnet_mac(CString& strMac)
{
	int			nNet = 0;
	NCB			ncb; 
	UCHAR		uRetCode; 
	LANA_ENUM	lana_enum; 

	memset( &ncb, 0, sizeof(ncb) ); 
	ncb.ncb_command = NCBENUM; 

	ncb.ncb_buffer = (unsigned char *) &lana_enum; 
	ncb.ncb_length = sizeof(lana_enum); 

	// 向网卡发送NCBENUM命令，以获取当前机器的网卡信息，如有多少个网卡、每张网卡的编号等 
	uRetCode = Netbios( &ncb ); 
	if ( uRetCode == 0 && ncb.ncb_retcode == 0) 
	{ 
		// 对每一张网卡，以其网卡编号为输入编号，获取其MAC地址 
		for ( int i = 0; i< lana_enum.length; ++i) 
		{
			CString strMacOne;
			if (getmac_mac(lana_enum.lana[i], strMacOne))
			{
				if (strMac.GetLength() > 0)
				{
					strMac.Append("&");
				}
				strMac.Append(strMacOne);

				nNet++;
			}
		}
	}

	return nNet;
}


int GetUUID(char szUUID[64])
{
	CString strHardId;
	int nNet = 0;

	szUUID[0] = '\0';
	 
	nNet = getnet_mac(strHardId);
	if (nNet > 0 && strHardId.GetLength() > 0)
	{
		KG_EDStringToMD5String(szUUID, strHardId.GetString());
		return true;
	}

	return false;
}

int GetGUID(char szGUID[64])
{
    int nResult = false;
    GUID guid = {0};
    HRESULT hr = E_FAIL;
    char szBuf[64] = {0};
    szGUID[0] = '\0';

    CoInitialize(NULL);

    hr = CoCreateGuid(&guid);
    if (hr == E_FAIL)
        goto Exit0;

    _snprintf(
        szBuf, sizeof(szBuf),
        "%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",
        guid.Data1, guid.Data2, guid.Data3,
        guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3],
        guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7]
    );

    KG_EDStringToMD5String(szGUID, szBuf);

    nResult = true;
Exit0:
    CoUninitialize();
    return nResult;
}