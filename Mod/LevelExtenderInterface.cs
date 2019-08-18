using System;

namespace EasyUI
{
    public interface LevelExtenderInterface
    {
        int[] currentXP();
        int[] requiredXP();
    }

    public interface LEEvents
    {
        event EventHandler OnXPChanged;
        void raiseEvent();
    }
}
