using System;
using UnityEngine.UIElements;

namespace Taco.Editor
{
    public class DropdownMenuManipulator : Clickable
    {
        DropdownMenuHandler m_DropdownMenuHandler;

        public DropdownMenuManipulator(Action<DropdownMenu> menuBuilder, MouseButton mouseButton, Action onClick = null) : base(onClick)
        {
            m_DropdownMenuHandler = new DropdownMenuHandler(menuBuilder);
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter
            {
                button = mouseButton
            });
            clicked += () =>
            {
                m_DropdownMenuHandler.ShowMenu(target);
            };
        }
    }
}
