using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Platform;

#if MACCATALYST
using UIKit;
#endif

namespace MauiDynamicMenus;

public class MenuService : IMenuService
{
	#region Properties
	
	public Page? MenuHostingPage { get; set; }
	
	#endregion

	#region Menu Flyouts

	public bool MenuFlyoutItemExists(string name) => 
		GetMenuFlyoutItem(name) != null;

	public void AddMenuFlyoutItem(string menu, string name, Action execute, int position = -1, KeyboardAcceleratorModifiers modifiers = KeyboardAcceleratorModifiers.None, string? shortCutKey = null)
	{
		if (this.MenuHostingPage == null)
		{
			throw new InvalidOperationException($"{nameof(this.MenuHostingPage)} must not be null");
		}

		var menuBarItem = this.MenuHostingPage.MenuBarItems.ToList().SingleOrDefault(menuBarItem => menuBarItem.Text == menu);

		if (menuBarItem == null)
		{
			throw new InvalidOperationException($"no MenuBarItem with text {menu} found in current application menu");
		}

		if (menuBarItem.Any(element => element.Text == name))
		{
			throw new InvalidOperationException($"MenuBarItem with text {menu} contains already an item with text '{name}'");
		}

		var itemToAdd = new MenuFlyoutItem()
		{
			Text = name,
			Command = new RelayCommand(execute)
		};

		if (modifiers != KeyboardAcceleratorModifiers.None && !string.IsNullOrWhiteSpace(shortCutKey))
		{
			itemToAdd.KeyboardAccelerators.Add(new KeyboardAccelerator()
			{
				Modifiers = modifiers,
				Key = shortCutKey
			});
		}

		if (position != -1)
		{
			menuBarItem.Insert(position, itemToAdd);
		}
		else
		{
			menuBarItem.Add(itemToAdd);
		}

		ForceMenuRebuild();
	}

	public IMenuFlyoutItem? GetMenuFlyoutItem(string name)
	{
		if (this.MenuHostingPage == null)
			throw new InvalidOperationException($"{nameof(this.MenuHostingPage)} must not be null");

		IMenuFlyoutItem? result = null;

		this.MenuHostingPage.MenuBarItems.ToList().ForEach(menuBarItem =>
		{
			IMenuElement? foundItem = menuBarItem.SingleOrDefault(menuElement => menuElement is MenuFlyoutItem menuItem && menuItem.Text == name);

			if (foundItem != null)
			{
				result = foundItem as MenuFlyoutItem;
			}
		});

		return result;
	}

	public void RemoveMenuFlyoutItem(string menu, string name)
	{
		if (this.MenuHostingPage == null)
		{
			throw new InvalidOperationException($"{nameof(this.MenuHostingPage)} must not be null");
		}

		var menuBarItem = this.MenuHostingPage.MenuBarItems.ToList().SingleOrDefault(menuBarItem => menuBarItem.Text == menu);

		if (menuBarItem == null)
		{
			throw new InvalidOperationException($"no MenuBarItem with text {menu} found in current application menu");
		}

		var itemToRemove = GetMenuFlyoutItem(name);

		if (itemToRemove == null)
		{
			throw new InvalidOperationException($"no MenuFlyoutItem with text {name} found in MenuBarItem with text {menu}");
		}

		menuBarItem.Remove(itemToRemove);

		ForceMenuRebuild();
	}

	#endregion

	#region Sub Menu on Flyout

	public bool SubMenuExists(string name) =>
		GetSubMenu(name) != null;

	public IMenuFlyoutSubItem? GetSubMenu(string name)
	{
		if (this.MenuHostingPage == null)
		{
			throw new InvalidOperationException($"{nameof(this.MenuHostingPage)} must not be null");
		}

		IMenuFlyoutSubItem? result = null;

		this.MenuHostingPage.MenuBarItems.ToList().ForEach(menuBarItem =>
		{
			var foundItem = menuBarItem.SingleOrDefault(menuElement => menuElement is MenuFlyoutSubItem subMenu && subMenu.Text == name);

			if (foundItem != null)
			{
				result = foundItem as MenuFlyoutSubItem;
			}
		});

		return result;
	}

	#endregion

	#region Flyout on Submenu

	public void AddMenuFlyoutItemToSubMenu(string parentSubMenu, string name, Action execute, int position = -1, KeyboardAcceleratorModifiers modifiers = KeyboardAcceleratorModifiers.None, string? shortCutKey = null)
	{

		var subMenu = GetSubMenu(parentSubMenu);

		if (subMenu == null)
		{
			throw new InvalidOperationException($"no MenuFlyoutSubItem with text {parentSubMenu} found in current application menu");
		}

		if (MenuFlyoutItemInSubMenuExists(parentSubMenu, name))
		{
			throw new InvalidOperationException($"MenuFlyoutSubItem with text {parentSubMenu} contains already an item with text '{name}'");
		}

		var itemToAdd = new MenuFlyoutItem()
		{
			Text = name,
			Command = new RelayCommand(execute)
		};

		if (modifiers != KeyboardAcceleratorModifiers.None && !string.IsNullOrWhiteSpace(shortCutKey))
		{
			itemToAdd.KeyboardAccelerators.Add(new KeyboardAccelerator()
			{
				Modifiers = modifiers,
				Key = shortCutKey
			});
		}

		if (position != -1)
		{
			subMenu.Insert(position, itemToAdd);
		}
		else
		{
			subMenu.Add(itemToAdd);
		}

		ForceMenuRebuild();
	}

	public IMenuFlyoutItem? GetMenuFlyoutItemInSubMenu(string parentSubMenu, string name)
	{
		IMenuFlyoutSubItem?	subMenu	= GetSubMenu(parentSubMenu);
		IMenuElement?		result	= subMenu?.SingleOrDefault(element => element.Text == name);
		return result as MenuFlyoutItem;
	}

	public void RemoveMenuFlyoutItemFromSubMenu(string parentSubMenu, string name)
	{
		var itemToRemove = GetMenuFlyoutItemInSubMenu(parentSubMenu, name);

		if (itemToRemove == null)
		{
			throw new InvalidOperationException($"no MenuFlyoutItem with text {name} and parent MenuFlyoutSubItem with text {parentSubMenu} found in current application menu");
		}

		var subMenu = GetSubMenu(parentSubMenu);

		if (subMenu == null)
		{
			throw new InvalidOperationException($"no MenuFlyoutSubItem with text {parentSubMenu} found in current application menu");
		}

		subMenu.Remove(itemToRemove);

		ForceMenuRebuild();
	}

	#endregion








	public bool MenuFlyoutItemInSubMenuExists(string parentSubMenu, string name) =>
		GetMenuFlyoutItemInSubMenu(parentSubMenu, name) != null;

	private void ForceMenuRebuild()
	{
		#if MACCATALYST
            UIMenuSystem.MainSystem.SetNeedsRebuild();
		#endif
	}
}