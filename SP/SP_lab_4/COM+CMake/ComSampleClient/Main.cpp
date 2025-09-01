#include <windows.h>
#include <combaseapi.h>
#include <strsafe.h>
#include <initguid.h>
#include "IComTest_h.h"
#include <ComSampleServerGuids.h>
#include <ComSampleServiceGuids.h>
#include <winerror.h>

HRESULT Execute(_In_ const IID &rclsid, _In_ DWORD dwCoInit, _In_ DWORD dwClsContext)
{
    HRESULT hr = CoInitializeEx(NULL, dwCoInit);
    if (SUCCEEDED(hr))
    {
        IComTest *pComTest;
        hr = CoCreateInstance(rclsid,
                              NULL,
                              dwClsContext,
                              IID_PPV_ARGS(&pComTest));
        if (SUCCEEDED(hr))
        {
            LPWSTR pwszWhoAmI = nullptr;
            HRESULT hrWhoAmI = pComTest->WhoAmI(&pwszWhoAmI);
            if (SUCCEEDED(hrWhoAmI))
            {
                wprintf(L"%s. Client calling from %s. COM Server running %s.\n",
                    pwszWhoAmI ? pwszWhoAmI : L"<NULL>",
                    (dwCoInit == COINIT_MULTITHREADED) ? L"MTA" : L"STA",
                    (dwClsContext == CLSCTX_INPROC_SERVER) ? L"in-process" : L"out-of-process");
                CoTaskMemFree(pwszWhoAmI);
            }
            else
            {
                wprintf(L"Error: WhoAmI failed with hr=0x%08X\n", hrWhoAmI);
            }

            ILPV* pLPV = nullptr;
            HRESULT hrQI = pComTest->QueryInterface(IID_ILPV, (void**)&pLPV);

            if (SUCCEEDED(hrQI))
            {
                double x = 2.0;
                double x2 = 0.0, x5 = 0.0, x17 = 0.0;

                HRESULT hrCalc = pLPV->CalculatePowers(x, &x2, &x5, &x17);

                if (SUCCEEDED(hrCalc))
                {
                    wprintf(L"  Calculations for x=%.2f: x^2=%.6f, x^5=%.6f, x^17=%.6f\n",
                        x, x2, x5, x17);
                }
                else
                {
                    wprintf(L"  Error: CalculatePowers failed with hr=0x%08X\n", hrCalc);
                }

                pLPV->Release();
                pLPV = nullptr;
            }
            else
            {
                wprintf(L"  Error: QueryInterface for ILPV failed with hr=0x%08X. Component might not implement it.\n", hrQI);
            }

            pComTest->Release();
            pComTest = nullptr;
        }

        CoUninitialize();
    }

    return hr;
}

int __cdecl main()
{
    Execute(CLSID_CComServerTest,  COINIT_MULTITHREADED,     CLSCTX_INPROC_SERVER);
    Execute(CLSID_CComServerTest,  COINIT_MULTITHREADED,     CLSCTX_LOCAL_SERVER);
    Execute(CLSID_CComServerTest,  COINIT_APARTMENTTHREADED, CLSCTX_INPROC_SERVER);
    Execute(CLSID_CComServerTest,  COINIT_APARTMENTTHREADED, CLSCTX_LOCAL_SERVER);
    Execute(CLSID_CComServiceTest, COINIT_MULTITHREADED,     CLSCTX_LOCAL_SERVER);
    Execute(CLSID_CComServiceTest, COINIT_APARTMENTTHREADED, CLSCTX_LOCAL_SERVER);

    return 0;
}
