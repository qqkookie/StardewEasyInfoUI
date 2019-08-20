using System;

namespace EasyUI
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
