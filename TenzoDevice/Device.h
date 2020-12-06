#include "WinUsbDevice.h"

#define IN_PIPE_ID          0x81
#define OUT_PIPE_ID         0x01

class CDevice{
private:
	CWinUsbDevice mDevice;

public:
	CDevice();
	~CDevice();
	CWinUsbDevice* getDevice();

public:
	BOOL InitUsb();
	BOOL ReadUsb(BYTE *pDest, int arraySize);
	BOOL WriteUsb(BYTE *pSrc, int arraySize);
	BOOL IsConnect();
};