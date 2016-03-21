using UnityEngine;
public class MenuManager : MonoBehaviour
{
	public Menu CurrentMenu;
	private string uName = "Default Username";
	private float gameVolume;

	public void Start()
	{
		ShowMenu(CurrentMenu);
	}

	public void ShowMenu(Menu menu)
	{
		if (CurrentMenu != null)
		{
			CurrentMenu.IsOpen = false;
		}
		CurrentMenu = menu;
		CurrentMenu.IsOpen = true;
	}

	public void quit()
	{
		Application.Quit();
	}

	public void StartGame ()
	{
		
		Application.LoadLevel ("localGame");
	}

	public void setUsername(string name)
	{
		uName = name;
	}

	public void setGameVolume(float vol)
	{
		gameVolume = vol;
		PlayerPrefs.SetFloat("volume", vol);

	}


}