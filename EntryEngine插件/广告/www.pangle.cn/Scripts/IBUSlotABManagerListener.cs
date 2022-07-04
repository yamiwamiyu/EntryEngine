using System;

namespace ByteDance.Union
{
    public interface IBUSlotABManagerListener
    {
        void onComplete(String slotId, AdSlotType type, int errorCode, String errorMsg);
    }
}