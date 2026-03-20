using Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TowerDefense.UI.HUD
{
	public class TowerDefenseMainMenu : MainMenu
	{
		public OptionsMenu optionsMenu;
		public SimpleMainMenuPage titleMenu;
		public LevelSelectScreen levelSelectMenu;

		public void ShowOptionsMenu()
		{
			ResolvePages();
			ShowPage(optionsMenu);
		}

		public void ShowLevelSelectMenu()
		{
			ResolvePages();
			ShowPage(levelSelectMenu);
		}

		public void ShowTitleScreen()
		{
			ResolvePages();
			ShowPage(titleMenu, useBackPath: true);
		}

		protected virtual void Awake()
		{
			ResolvePages();
			BindFallbackButtons();
			ShowTitleScreen();
		}

		protected virtual void Update()
		{
			Keyboard keyboard = Keyboard.current;
			if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
			{
				return;
			}

			if (ReferenceEquals(m_CurrentPage, titleMenu))
			{
				Application.Quit();
			}
			else
			{
				Back();
			}
		}

		void ResolvePages()
		{
			optionsMenu ??= FindPage<OptionsMenu>("Options");
			titleMenu ??= FindPage<SimpleMainMenuPage>("Main");
			levelSelectMenu ??= FindPage<LevelSelectScreen>("LevelSelect");
		}

		void BindFallbackButtons()
		{
			BindButton("Options", ShowOptionsMenu);
			BindButton("Level Select", ShowLevelSelectMenu);

			Button[] buttons = GetComponentsInChildren<Button>(true);
			foreach (Button button in buttons)
			{
				if (button != null && button.name == "Back")
				{
					button.onClick.AddListener(ShowTitleScreen);
				}
			}
		}

		void BindButton(string buttonName, UnityEngine.Events.UnityAction handler)
		{
			Button[] buttons = GetComponentsInChildren<Button>(true);
			foreach (Button button in buttons)
			{
				if (button != null && button.name == buttonName)
				{
					button.onClick.AddListener(handler);
				}
			}
		}

		void ShowPage(IMainMenuPage page, bool useBackPath = false)
		{
			if (page == null || ReferenceEquals(m_CurrentPage, page))
			{
				return;
			}

			if (useBackPath)
			{
				Back(page);
			}
			else
			{
				ChangePage(page);
			}
		}

		T FindPage<T>(string objectName) where T : Component, IMainMenuPage
		{
			T[] pages = GetComponentsInChildren<T>(true);
			foreach (T page in pages)
			{
				if (page != null && page.gameObject.name == objectName)
				{
					return page;
				}
			}

			return null;
		}
	}
}
