// TenzoDevice.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "Device.h"

CDevice g_hDevice;

extern "C"
{
	__declspec( dllexport ) BOOL USBInit( void ) {
		return g_hDevice.InitUsb();
	}
	__declspec( dllexport ) BOOL USBReadData( BYTE *pDest, int arraySize ) {
		return g_hDevice.ReadUsb( pDest, arraySize );
	}
	__declspec( dllexport ) BOOL USBWriteData( BYTE *pSrc, int arraySize ) {
		return g_hDevice.WriteUsb( pSrc, arraySize );
	}
	__declspec( dllexport ) BOOL USBIsConnect( void ) {
		return g_hDevice.IsConnect();
	}
	__declspec( dllexport ) BOOL DLLIsConnect( void ) {
		return true;
	}
}
