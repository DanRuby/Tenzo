#include "stdafx.h"
#include "Device.h"

CDevice::CDevice() {}
CDevice::~CDevice() {}
CWinUsbDevice* CDevice::getDevice() {
	return &mDevice;
}

BOOL CDevice::InitUsb() {
	if( mDevice.IsOpened() == TRUE )
		return TRUE;
	DWORD deviceNumber = mDevice.GetNumDevices();
	if( deviceNumber != 1 ){ return FALSE; }

	BOOL successOpen = mDevice.OpenByIndex(0);
	if( !successOpen ){ return FALSE; }

	BOOL isOpen = mDevice.IsOpened();
	return isOpen;
}
BOOL CDevice::ReadUsb(BYTE *pDest, int arraySize) {
	if( IsConnect() == FALSE )
		return FALSE;
	memset(pDest, 0, arraySize);
	DWORD bytesRead = 0;
	BOOL successRead = mDevice.ReadPipe(IN_PIPE_ID, pDest, arraySize, &bytesRead);
	return successRead;
}
BOOL CDevice::WriteUsb(BYTE *pSrc, int arraySize) {
	DWORD bytesWritten = 0;
	BOOL successWrite = mDevice.WritePipe(OUT_PIPE_ID, pSrc, arraySize, &bytesWritten);
	return successWrite;
}
BOOL CDevice::IsConnect() {
	DWORD deviceNumber = mDevice.GetNumDevices();
	if( deviceNumber != 1 ){
		mDevice.Close();
		return FALSE;
	}
	BOOL isOpen = mDevice.IsOpened();
	if( isOpen == FALSE )
		return InitUsb();
	return isOpen;
}