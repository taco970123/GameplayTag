using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Taco.Editor
{
    public class DropdownMenuHandler
    {
        Action<DropdownMenu> m_MenuBuilder;

        public DropdownMenuHandler(Action<DropdownMenu> menuBuilder)
        {
            m_MenuBuilder = menuBuilder;
        }

        public void ShowMenu(VisualElement target)
        {
            DropdownMenu dropdownMenu = new DropdownMenu();
            m_MenuBuilder?.Invoke(dropdownMenu);
            if (!dropdownMenu.MenuItems().Any())
            {
                return;
            }
            if (target != null)
            {
                Rect worldBound = target.worldBound;
                if (worldBound.x <= 0f)
                {
                    worldBound.x = 1f;
                }

                DoDisplayEditorMenu(dropdownMenu, worldBound);
            }
        }
        private GenericMenu PrepareMenu(DropdownMenu menu, EventBase triggerEvent)
        {
            menu.PrepareForDisplay(triggerEvent);
            GenericMenu genericMenu = new GenericMenu();
            foreach (DropdownMenuItem item in menu.MenuItems())
            {
                DropdownMenuAction action = item as DropdownMenuAction;
                if (action != null)
                {
                    if ((action.status & DropdownMenuAction.Status.Hidden) == DropdownMenuAction.Status.Hidden || action.status == DropdownMenuAction.Status.None)
                    {
                        continue;
                    }

                    bool on = (action.status & DropdownMenuAction.Status.Checked) == DropdownMenuAction.Status.Checked;
                    if ((action.status & DropdownMenuAction.Status.Disabled) == DropdownMenuAction.Status.Disabled)
                    {
                        genericMenu.AddDisabledItem(new GUIContent(action.name), on);
                        continue;
                    }

                    genericMenu.AddItem(new GUIContent(action.name), on, delegate
                    {
                        action.Execute();
                    });
                }
                else
                {
                    DropdownMenuSeparator dropdownMenuSeparator = item as DropdownMenuSeparator;
                    if (dropdownMenuSeparator != null)
                    {
                        genericMenu.AddSeparator(dropdownMenuSeparator.subMenuPath);
                    }
                }
            }

            return genericMenu;
        }
        private void DoDisplayEditorMenu(DropdownMenu menu, Rect rect)
        {
            PrepareMenu(menu, null).DropDown(rect);
        }
        private void DoDisplayEditorMenu(DropdownMenu menu, EventBase triggerEvent)
        {
            GenericMenu genericMenu = PrepareMenu(menu, triggerEvent);
            Vector2 position = Vector2.zero;
            if (triggerEvent is IMouseEvent)
            {
                position = ((IMouseEvent)triggerEvent).mousePosition;
            }
            else if (triggerEvent is IPointerEvent)
            {
                position = ((IPointerEvent)triggerEvent).position;
            }
            else if (triggerEvent.target is VisualElement)
            {
                position = ((VisualElement)triggerEvent.target).layout.center;
            }

            genericMenu.DropDown(new Rect(position, Vector2.zero));
        }
    }
}