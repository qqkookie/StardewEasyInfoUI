using System;

namespace EasyInfoUI
{
    public interface ILevelExtenderInterface
    {
        int[] currentXP();
        int[] requiredXP();
    }

    public interface ILevelExtenderEvents
    {
        event EventHandler OnXPChanged;
        void raiseEvent();
    }
}
